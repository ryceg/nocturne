using FluentAssertions;
using Nocturne.Connectors.Dexcom.Services;
using Xunit;

namespace Nocturne.Connectors.Dexcom.Tests.Services;

public class DexcomStartTimeTests
{
    private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CalculateStartTime_WithNoSince_DefaultsToTwoDaysAgo()
    {
        var result = DexcomConnectorService.CalculateStartTime(since: null, now: Now);

        result.Should().Be(Now.AddDays(-2));
    }

    [Fact]
    public void CalculateStartTime_WithRecentSince_UsesProvidedValue()
    {
        var since = Now.AddHours(-6);

        var result = DexcomConnectorService.CalculateStartTime(since, Now);

        result.Should().Be(since);
    }

    [Fact]
    public void CalculateStartTime_WithOldSince_UsesProvidedValueNotClamped()
    {
        var since = Now.AddMonths(-6);

        var result = DexcomConnectorService.CalculateStartTime(since, Now);

        result.Should().Be(since);
    }
}
