using System.Globalization;
using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class LeadScoringService : ILeadScoringService
{
    private readonly AppDbContext _dbContext;

    public LeadScoringService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ApplyScoreAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        var rules = await _dbContext.LeadScoreRules
            .Include(x => x.RuleType)
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var score = 0;
        foreach (var rule in rules)
        {
            if (await RuleMatchesAsync(rule, lead, cancellationToken))
            {
                score += rule.ScoreValue;
            }
        }

        lead.Score = Math.Max(0, score);
        lead.ScoreGrade = lead.Score >= 80 ? "Hot" : lead.Score >= 40 ? "Warm" : "Cold";
    }

    public async Task<int> RecalculateLeadScoreAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            return 0;
        }

        await ApplyScoreAsync(lead, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return 1;
    }

    public async Task<int> RecalculateAllLeadScoresAsync(CancellationToken cancellationToken = default)
    {
        var leads = await _dbContext.Leads.OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        foreach (var lead in leads)
        {
            await ApplyScoreAsync(lead, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return leads.Count;
    }

    private async Task<bool> RuleMatchesAsync(LeadScoreRule rule, Lead lead, CancellationToken cancellationToken)
    {
        var ruleTypeCode = rule.RuleType.Code.ToUpperInvariant();
        if (ruleTypeCode is "ACTIVITY" or "ENGAGEMENT")
        {
            var completedActivityCount = lead.Id == Guid.Empty
                ? 0
                : await _dbContext.LeadActivities.CountAsync(x => x.LeadId == lead.Id && x.CompletedDate != null, cancellationToken);

            if (string.IsNullOrWhiteSpace(rule.Operator) && string.IsNullOrWhiteSpace(rule.CompareValue))
            {
                return completedActivityCount > 0;
            }

            return CompareValues(completedActivityCount, rule.Operator, rule.CompareValue);
        }

        var fieldValue = GetLeadFieldValue(lead, rule.FieldName);
        if (ruleTypeCode == "FIELD_COMPLETENESS")
        {
            return CompareValues(fieldValue, string.IsNullOrWhiteSpace(rule.Operator) ? "exists" : rule.Operator, rule.CompareValue);
        }

        if (ruleTypeCode == "SOURCE")
        {
            return await LookupMatchesAsync(lead.LeadSourceId, rule.Operator, rule.CompareValue, cancellationToken);
        }

        if (fieldValue is Guid guidValue)
        {
            return await LookupMatchesAsync(guidValue, rule.Operator, rule.CompareValue, cancellationToken);
        }

        return CompareValues(fieldValue, rule.Operator, rule.CompareValue);
    }

    private async Task<bool> LookupMatchesAsync(Guid? lookupValueId, string? ruleOperator, string? compareValue, CancellationToken cancellationToken)
    {
        if (!lookupValueId.HasValue)
        {
            return CompareValues(null, ruleOperator, compareValue);
        }

        var lookupValue = await _dbContext.LookupValues
            .Where(x => x.Id == lookupValueId.Value)
            .Select(x => new { x.Id, x.Name, x.Code })
            .FirstOrDefaultAsync(cancellationToken);

        if (lookupValue is null)
        {
            return false;
        }

        var op = NormalizeOperator(ruleOperator);
        if (op is "exists")
        {
            return true;
        }

        if (op is "notexists")
        {
            return false;
        }

        var expected = compareValue?.Trim();
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        return string.Equals(lookupValue.Id.ToString(), expected, StringComparison.OrdinalIgnoreCase)
            || string.Equals(lookupValue.Name, expected, StringComparison.OrdinalIgnoreCase)
            || string.Equals(lookupValue.Code, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static object? GetLeadFieldValue(Lead lead, string? fieldName)
    {
        return fieldName?.Trim().ToLowerInvariant() switch
        {
            "leadnumber" => lead.LeadNumber,
            "topic" => lead.Topic,
            "firstname" => lead.FirstName,
            "middlename" => lead.MiddleName,
            "lastname" => lead.LastName,
            "fullname" => lead.FullName,
            "companyname" => lead.CompanyName,
            "jobtitle" => lead.JobTitle,
            "email" => lead.Email,
            "mobilephone" => lead.MobilePhone,
            "workphone" => lead.WorkPhone,
            "website" => lead.Website,
            "leadsourceid" or "leadsource" or "source" => lead.LeadSourceId,
            "leadstatusid" or "leadstatus" or "status" => lead.LeadStatusId,
            "qualificationstatusid" or "qualificationstatus" => lead.QualificationStatusId,
            "ratingid" or "rating" => lead.RatingId,
            "industryid" or "industry" => lead.IndustryId,
            "estimatedvalue" => lead.EstimatedValue,
            "estimatedclosedate" => lead.EstimatedCloseDate,
            "score" => lead.Score,
            "description" => lead.Description,
            "notes" => lead.Notes,
            _ => null
        };
    }

    private static bool CompareValues(object? rawValue, string? ruleOperator, string? compareValue)
    {
        var op = NormalizeOperator(ruleOperator);
        var hasValue = rawValue switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            decimal decimalValue => decimalValue != default,
            int intValue => intValue != default,
            DateTime dateValue => dateValue != default,
            _ => true
        };

        if (op == "exists")
        {
            return hasValue;
        }

        if (op == "notexists")
        {
            return !hasValue;
        }

        if (!hasValue || string.IsNullOrWhiteSpace(compareValue))
        {
            return false;
        }

        if (rawValue is decimal actualDecimal && decimal.TryParse(compareValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalCompare))
        {
            return CompareNumbers(actualDecimal, decimalCompare, op);
        }

        if (rawValue is int actualInt && int.TryParse(compareValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var intCompare))
        {
            return CompareNumbers(actualInt, intCompare, op);
        }

        if (rawValue is DateTime actualDate && DateTime.TryParse(compareValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateCompare))
        {
            return op switch
            {
                "greaterthan" or "after" => actualDate > dateCompare,
                "greaterthanorequal" or "onorafter" => actualDate >= dateCompare,
                "lessthan" or "before" => actualDate < dateCompare,
                "lessthanorequal" or "onorbefore" => actualDate <= dateCompare,
                "notequals" => actualDate != dateCompare,
                _ => actualDate == dateCompare
            };
        }

        var actual = Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
        var expected = compareValue.Trim();
        return op switch
        {
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            "startswith" => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
            "endswith" => actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase),
            "notequals" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            _ => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool CompareNumbers(decimal actual, decimal expected, string op)
    {
        return op switch
        {
            "greaterthan" => actual > expected,
            "greaterthanorequal" => actual >= expected,
            "lessthan" => actual < expected,
            "lessthanorequal" => actual <= expected,
            "notequals" => actual != expected,
            _ => actual == expected
        };
    }

    private static string NormalizeOperator(string? ruleOperator)
    {
        if (string.IsNullOrWhiteSpace(ruleOperator))
        {
            return "equals";
        }

        return ruleOperator
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
    }
}
