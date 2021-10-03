using Xunit;

namespace JoyMoe.Common.Json.Tests;

public class SnakeCaseNamingPolicyTests
{
    [Fact]
    public void ConvertNameTest()
    {
        var policy = new SnakeCaseNamingPolicy();

        Assert.Equal("foo", policy.ConvertName("Foo"));
        Assert.Equal("foo_bar", policy.ConvertName("FooBar"));
    }
}
