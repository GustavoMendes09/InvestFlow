using System.Globalization;

namespace InvestFlow.Api.Features.Common;

public static class MonthPeriodParser
{
    private const string Format = "yyyy-MM-dd";

    public static MonthPeriod ParseOrCurrent(string? month, TimeProvider timeProvider)
    {
        if (TryParse(month, out var period))
        {
            return period;
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var start = new DateOnly(today.Year, today.Month, 1);

        return new MonthPeriod(start, start.AddMonths(1));
    }

    private static bool TryParse(string? month, out MonthPeriod period)
    {
        var parsed = DateOnly.TryParseExact(
            $"{month}-01",
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var start);

        period = parsed
            ? new MonthPeriod(start, start.AddMonths(1))
            : default;

        return parsed;
    }
}
