using InvestFlow.Api.Features.Common;

namespace InvestFlow.Api.Tests;

public sealed class MonthPeriodParserTests
{
    [Fact]
    public void ParseOrCurrent_ReturnsTheRequestedCalendarMonth()
    {
        var period = MonthPeriodParser.ParseOrCurrent("2026-08", TimeProvider.System);

        Assert.Equal(new DateOnly(2026, 8, 1), period.Start);
        Assert.Equal(new DateOnly(2026, 9, 1), period.End);
    }
}
