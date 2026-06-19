using backend.Entities;

namespace backend.Services;

public static class NumberSequenceFormatter
{
    public static string Format(NumberSequence sequence, long number, DateTime timestamp)
    {
        var separator = string.IsNullOrWhiteSpace(sequence.Separator) ? "-" : sequence.Separator;
        var parts = new List<string> { sequence.Prefix.Trim() };

        if (sequence.IncludeYear)
        {
            parts.Add(timestamp.Year.ToString("0000"));
        }

        if (sequence.IncludeMonth)
        {
            parts.Add(timestamp.Month.ToString("00"));
        }

        if (sequence.IncludeDay)
        {
            parts.Add(timestamp.Day.ToString("00"));
        }

        parts.Add(Math.Max(0, number).ToString().PadLeft(Math.Clamp(sequence.MinimumDigits, 1, 20), '0'));

        var formatted = string.Join(separator, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(sequence.Suffix) ? formatted : $"{formatted}{sequence.Suffix.Trim()}";
    }

    public static long PreviewNumber(NumberSequence sequence, DateTime timestamp)
    {
        return ShouldReset(sequence.ResetFrequency?.Code, sequence.LastResetDate, timestamp)
            ? 1
            : Math.Max(sequence.NextNumber, sequence.CurrentNumber + 1);
    }

    public static void RefreshFormatPreview(NumberSequence sequence, DateTime timestamp)
    {
        sequence.FormatPreview = Format(sequence, PreviewNumber(sequence, timestamp), timestamp);
    }

    public static bool ShouldReset(string? resetFrequencyCode, DateTime? lastResetDate, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(resetFrequencyCode) ||
            resetFrequencyCode.Equals("NEVER", StringComparison.OrdinalIgnoreCase) ||
            lastResetDate is null)
        {
            return false;
        }

        var last = lastResetDate.Value;
        return resetFrequencyCode.ToUpperInvariant() switch
        {
            "DAILY" => last.Date < timestamp.Date,
            "MONTHLY" => last.Year != timestamp.Year || last.Month != timestamp.Month,
            "YEARLY" => last.Year != timestamp.Year,
            _ => false
        };
    }

    public static bool UsesResetPeriod(string? resetFrequencyCode)
    {
        return !string.IsNullOrWhiteSpace(resetFrequencyCode) &&
            !resetFrequencyCode.Equals("NEVER", StringComparison.OrdinalIgnoreCase);
    }
}
