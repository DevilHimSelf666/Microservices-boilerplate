using Xunit;

namespace DAR.Api.Tests;

public class HealthCheckTests
{
    [Fact]
    public void Always_Passes()
    {
        Assert.True(true);
    }
}
