using Infrastructure.Services;

namespace Canchero.Infrastructure.UnitTest;

public sealed class DateTimeProviderTests
{
    [Fact]
    public void UtcNow_IsUtcAndCloseToCurrentTime()
    {
        var before = DateTimeOffset.UtcNow;
        var actual = new DateTimeProvider().UtcNow;
        var after = DateTimeOffset.UtcNow;
        Assert.Equal(TimeSpan.Zero, actual.Offset);
        Assert.InRange(actual, before, after);
    }
}
