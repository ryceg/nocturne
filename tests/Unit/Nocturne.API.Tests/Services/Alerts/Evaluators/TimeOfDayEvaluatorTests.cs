using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Nocturne.API.Services.Alerts.Evaluators;
using Nocturne.Core.Models;
using Nocturne.Core.Models.Alerts;
using Xunit;

namespace Nocturne.API.Tests.Services.Alerts.Evaluators;

[Trait("Category", "Unit")]
public class TimeOfDayEvaluatorTests
{
    [Fact]
    public void ConditionType_ShouldBeTimeOfDay()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc));
        sut.ConditionType.Should().Be(AlertConditionType.TimeOfDay);
    }

    [Fact]
    public async Task WithinWindow_Utc_ReturnsTrue()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 10, 30, 0, DateTimeKind.Utc));
        var json = """{"from": "09:00", "to": "12:00", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task BeforeWindow_ReturnsFalse()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 8, 30, 0, DateTimeKind.Utc));
        var json = """{"from": "09:00", "to": "12:00", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task AfterWindow_ReturnsFalse()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "09:00", "to": "12:00", "timezone": null}""";

        // Half-open [09:00, 12:00) — exactly 12:00 is OUT.
        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task OvernightWindow_BeforeMidnightInside_ReturnsTrue()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 23, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "22:00", "to": "06:00", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task OvernightWindow_AfterMidnightInside_ReturnsTrue()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 2, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "22:00", "to": "06:00", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task OvernightWindow_DaytimeOutside_ReturnsFalse()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "22:00", "to": "06:00", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task UnknownTimezone_ReturnsFalse()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 10, 30, 0, DateTimeKind.Utc));
        var json = """{"from": "09:00", "to": "12:00", "timezone": "Made/Up"}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task UnparseableTime_ReturnsFalse()
    {
        var sut = MakeSut(new DateTime(2026, 3, 22, 10, 30, 0, DateTimeKind.Utc));
        var json = """{"from": "9am", "to": "noon", "timezone": null}""";

        (await sut.EvaluateAsync(json, MakeContext(), CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task NullConditionTimezone_FallsBackToTenantTimeZoneId()
    {
        // Sydney is UTC+11 in March (AEDT). 11:00 UTC = 22:00 Sydney local — must NOT match
        // a 10:00–14:00 local-time window. Before this fix, a rule built in the UI saved
        // condition.Timezone=null and the evaluator interpreted "10:00–14:00" as UTC, firing
        // at the wrong wall-clock hour for any non-UTC tenant.
        var sut = MakeSut(new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "10:00", "to": "14:00", "timezone": null}""";
        var ctx = MakeContext(timezone: TryResolve("Australia/Sydney", "AUS Eastern Standard Time"));

        (await sut.EvaluateAsync(json, ctx, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task NullConditionTimezone_TenantTimeZoneMatchesWindow_ReturnsTrue()
    {
        // 23:00 UTC = 10:00 Sydney local — inside the 10:00–14:00 local-time window.
        var sut = MakeSut(new DateTime(2026, 3, 21, 23, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "10:00", "to": "14:00", "timezone": null}""";
        var ctx = MakeContext(timezone: TryResolve("Australia/Sydney", "AUS Eastern Standard Time"));

        (await sut.EvaluateAsync(json, ctx, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task ConditionTimezone_OverridesTenantTimeZoneId()
    {
        // Rule explicitly says UTC; tenant tz must not override an explicit per-rule tz.
        var sut = MakeSut(new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "10:00", "to": "14:00", "timezone": "UTC"}""";
        var ctx = MakeContext(timezone: TryResolve("Australia/Sydney", "AUS Eastern Standard Time"));

        (await sut.EvaluateAsync(json, ctx, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task BothTimezonesNull_FallsBackToUtc()
    {
        // Legacy behaviour: when neither the rule nor the tenant supplies a timezone, evaluate
        // in UTC rather than refusing to fire. Preserves the original semantics from before
        // the tenant-tz fallback landed.
        var sut = MakeSut(new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc));
        var json = """{"from": "10:00", "to": "14:00", "timezone": null}""";
        var ctx = MakeContext(timezone: null);

        (await sut.EvaluateAsync(json, ctx, CancellationToken.None)).Should().BeTrue();
    }

    private static TimeOfDayEvaluator MakeSut(DateTime utcNow) =>
        new(new FakeTimeProvider(new DateTimeOffset(utcNow)));

    private static SensorContext MakeContext(string? timezone = null) => new()
    {
        LatestValue = 100m,
        LatestTimestamp = new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc),
        TrendRate = 0m,
        LastReadingAt = new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc),
        TenantTimeZoneId = timezone,
    };

    /// <summary>
    /// Some test hosts expose IANA names, others Windows names. Resolve to whichever the host
    /// recognises so the test passes on both Windows CI and Linux CI.
    /// </summary>
    private static string TryResolve(string ianaId, string windowsId)
    {
        try { TimeZoneInfo.FindSystemTimeZoneById(ianaId); return ianaId; }
        catch { return windowsId; }
    }
}
