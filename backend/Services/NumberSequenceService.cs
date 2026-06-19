using System.Data;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NumberSequenceService : INumberSequenceService
{
    private readonly AppDbContext _dbContext;

    public NumberSequenceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateNextAsync(string sequenceCode)
    {
        var normalizedCode = sequenceCode.Trim().ToUpperInvariant();
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var sequence = await _dbContext.NumberSequences
            .Include(x => x.ResetFrequency)
            .FirstOrDefaultAsync(x => x.SequenceCode == normalizedCode && x.IsActive);

        if (sequence is null)
        {
            throw new InvalidOperationException($"Number sequence '{normalizedCode}' is not configured.");
        }

        var now = DateTime.UtcNow;
        if (NumberSequenceFormatter.ShouldReset(sequence.ResetFrequency?.Code, sequence.LastResetDate, now))
        {
            sequence.CurrentNumber = 0;
            sequence.NextNumber = 1;
            sequence.LastResetDate = now;
        }
        else if (NumberSequenceFormatter.UsesResetPeriod(sequence.ResetFrequency?.Code) && sequence.LastResetDate is null)
        {
            sequence.LastResetDate = now;
        }

        var next = Math.Max(sequence.NextNumber, sequence.CurrentNumber + 1);
        var generated = NumberSequenceFormatter.Format(sequence, next, now);

        sequence.CurrentNumber = next;
        sequence.NextNumber = next + 1;
        NumberSequenceFormatter.RefreshFormatPreview(sequence, now);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return generated;
    }

    public async Task<string> PreviewAsync(Guid numberSequenceId)
    {
        var sequence = await _dbContext.NumberSequences
            .Include(x => x.ResetFrequency)
            .FirstOrDefaultAsync(x => x.Id == numberSequenceId);

        if (sequence is null)
        {
            throw new InvalidOperationException("Number sequence could not be found.");
        }

        var now = DateTime.UtcNow;
        return NumberSequenceFormatter.Format(sequence, NumberSequenceFormatter.PreviewNumber(sequence, now), now);
    }

    public async Task ResetAsync(Guid numberSequenceId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var sequence = await _dbContext.NumberSequences
            .Include(x => x.ResetFrequency)
            .FirstOrDefaultAsync(x => x.Id == numberSequenceId);

        if (sequence is null)
        {
            throw new InvalidOperationException("Number sequence could not be found.");
        }

        sequence.CurrentNumber = 0;
        sequence.NextNumber = 1;
        sequence.LastResetDate = DateTime.UtcNow;
        NumberSequenceFormatter.RefreshFormatPreview(sequence, DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
