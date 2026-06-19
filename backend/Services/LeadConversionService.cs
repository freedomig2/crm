using System.Text.Json;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class LeadConversionService : ILeadConversionService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOpportunityConversionAdapter _opportunityConversionAdapter;
    private readonly INumberSequenceService _numberSequenceService;

    public LeadConversionService(
        AppDbContext dbContext,
        ICurrentUserContext currentUserContext,
        IOpportunityConversionAdapter opportunityConversionAdapter,
        INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _opportunityConversionAdapter = opportunityConversionAdapter;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<LeadConversionResultDto?> ConvertAsync(Guid leadId, LeadConversionRequestDto request, CancellationToken cancellationToken = default)
    {
        var lead = await _dbContext.Leads
            .Include(x => x.LeadStatus)
            .Include(x => x.QualificationStatus)
            .FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);

        if (lead is null)
        {
            return null;
        }

        EnsureConvertible(lead);
        EnsureExactlyOneAccountOption(request);
        EnsureExactlyOneContactOption(request);

        var account = request.CreateAccount
            ? await CreateAccountFromLeadAsync(lead)
            : await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == request.ExistingAccountId!.Value, cancellationToken)
                ?? throw new InvalidOperationException("Existing account could not be found.");

        if (request.CreateAccount)
        {
            _dbContext.Accounts.Add(account);
        }

        var contact = request.CreateContact
            ? await CreateContactFromLeadAsync(lead, account.Id)
            : await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == request.ExistingContactId!.Value, cancellationToken)
                ?? throw new InvalidOperationException("Existing contact could not be found.");

        if (!request.CreateContact && contact.AccountId != account.Id)
        {
            throw new InvalidOperationException("Existing contact must belong to the selected account.");
        }

        if (request.CreateContact)
        {
            _dbContext.Contacts.Add(contact);
        }

        var opportunityId = request.CreateOpportunity
            ? await _opportunityConversionAdapter.TryCreateOpportunityAsync(lead, request, account.Id, contact.Id, cancellationToken)
            : null;

        var convertedStatusId = await GetLookupValueIdAsync("LEAD_STATUS", "CONVERTED", cancellationToken)
            ?? throw new InvalidOperationException("Converted lead status is not configured.");

        lead.LeadStatusId = convertedStatusId;
        lead.ConvertedAccountId = account.Id;
        lead.ConvertedContactId = contact.Id;
        lead.ConvertedOpportunityId = opportunityId;
        lead.ConvertedAt = DateTime.UtcNow;
        lead.ConvertedById = _currentUserContext.UserId;

        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(Lead),
            EntityId = lead.Id.ToString(),
            Action = "Convert",
            NewValues = JsonSerializer.Serialize(new
            {
                accountId = account.Id,
                contactId = contact.Id,
                opportunityId,
                request.CreateOpportunity
            }),
            UserId = _currentUserContext.UserId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _currentUserContext.UserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeadConversionResultDto
        {
            LeadId = lead.Id,
            ConvertedAccountId = account.Id,
            ConvertedAccountName = account.Name,
            ConvertedContactId = contact.Id,
            ConvertedContactName = contact.FullName,
            ConvertedOpportunityId = opportunityId,
            OpportunityMessage = request.CreateOpportunity && opportunityId is null
                ? "Opportunity creation is reserved for the Opportunity module."
                : null
        };
    }

    private static void EnsureConvertible(Lead lead)
    {
        var statusCode = lead.LeadStatus.Code.ToUpperInvariant();
        var qualificationCode = lead.QualificationStatus?.Code.ToUpperInvariant();

        if (lead.ConvertedAt.HasValue || statusCode == "CONVERTED")
        {
            throw new InvalidOperationException("Lead is already converted.");
        }

        if (statusCode == "DISQUALIFIED" || qualificationCode == "DISQUALIFIED")
        {
            throw new InvalidOperationException("Disqualified leads cannot be converted.");
        }

        if (statusCode != "QUALIFIED" && qualificationCode != "QUALIFIED")
        {
            throw new InvalidOperationException("Lead must be qualified before conversion.");
        }
    }

    private static void EnsureExactlyOneAccountOption(LeadConversionRequestDto request)
    {
        if (request.CreateAccount == request.ExistingAccountId.HasValue)
        {
            throw new InvalidOperationException("Choose either a new account or an existing account.");
        }
    }

    private static void EnsureExactlyOneContactOption(LeadConversionRequestDto request)
    {
        if (request.CreateContact == request.ExistingContactId.HasValue)
        {
            throw new InvalidOperationException("Choose either a new contact or an existing contact.");
        }
    }

    private async Task<Account> CreateAccountFromLeadAsync(Lead lead)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = await _numberSequenceService.GenerateNextAsync("ACCOUNT"),
            Name = FirstNonBlank(lead.CompanyName, lead.FullName, lead.Topic) ?? "Converted Lead",
            Website = TrimToNull(lead.Website),
            MainPhone = TrimToNull(lead.WorkPhone) ?? TrimToNull(lead.MobilePhone),
            Email = TrimToNull(lead.Email),
            Description = TrimToNull(lead.Description),
            IndustryId = lead.IndustryId,
            OwnerUserId = lead.OwnerUserId,
            OwnerTeamId = lead.OwnerTeamId,
            IsActive = true
        };
    }

    private async Task<Contact> CreateContactFromLeadAsync(Lead lead, Guid accountId)
    {
        var firstName = TrimToNull(lead.FirstName) ?? "Converted";
        var lastName = TrimToNull(lead.LastName) ?? "Lead";
        var fullName = string.Join(' ', new[] { firstName, TrimToNull(lead.MiddleName), lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return new Contact
        {
            Id = Guid.NewGuid(),
            ContactNumber = await _numberSequenceService.GenerateNextAsync("CONTACT"),
            AccountId = accountId,
            FirstName = firstName,
            MiddleName = TrimToNull(lead.MiddleName),
            LastName = lastName,
            FullName = fullName,
            JobTitle = TrimToNull(lead.JobTitle),
            Email = TrimToNull(lead.Email),
            MobilePhone = TrimToNull(lead.MobilePhone),
            WorkPhone = TrimToNull(lead.WorkPhone),
            OwnerUserId = lead.OwnerUserId,
            OwnerTeamId = lead.OwnerTeamId,
            IsActive = true
        };
    }

    private async Task<Guid?> GetLookupValueIdAsync(string categoryCode, string valueCode, CancellationToken cancellationToken)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == valueCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.Select(TrimToNull).FirstOrDefault(value => value is not null);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
