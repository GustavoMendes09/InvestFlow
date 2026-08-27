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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-month")]
    [InlineData("2026-13")]
    public void ParseOrCurrent_InvalidValueFallsBackToCurrentUtcMonth(string? value)
    {
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 12, 15, 23, 30, 0, TimeSpan.Zero));

        var period = MonthPeriodParser.ParseOrCurrent(value, timeProvider);

        Assert.Equal(new DateOnly(2026, 12, 1), period.Start);
        Assert.Equal(new DateOnly(2027, 1, 1), period.End);
    }

    [Fact]
    public void ParseOrCurrent_DecemberEndsAtFirstDayOfNextYear()
    {
        var period = MonthPeriodParser.ParseOrCurrent("2026-12", TimeProvider.System);

        Assert.Equal(new DateOnly(2026, 12, 1), period.Start);
        Assert.Equal(new DateOnly(2027, 1, 1), period.End);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
