using Nexsure.Api.Services;
using Nexsure.Api.Types;
using Xunit;

namespace Nexsure.Api.Tests;

public class ServiceArgsTests
{
    [Fact]
    public void From_Null_IsEmpty()
    {
        Assert.False(ServiceArgs.From(null).Has("anything"));
    }

    [Fact]
    public void From_AnonymousObject_ReadsProperties()
    {
        var args = ServiceArgs.From(new { client_id = 42, name = "x" });
        Assert.Equal(42, args.Get<int>("client_id"));
        Assert.Equal("x", args.Get<string>("name"));
    }

    [Fact]
    public void Get_MissingKey_Throws()
    {
        Assert.Throws<ArgumentException>(() => ServiceArgs.Empty.Get<int>("nope"));
    }

    [Fact]
    public void GetOptional_MissingKey_ReturnsDefault()
    {
        Assert.Equal("fallback", ServiceArgs.Empty.GetOptional("nope", "fallback"));
    }

    [Fact]
    public void GetOptional_Enum_RoundTrips()
    {
        var args = ServiceArgs.From(new { search_type = SearchType.StartsWith });
        Assert.Equal(SearchType.StartsWith, args.GetOptional("search_type", SearchType.Contains));
    }

    [Fact]
    public void Get_CoercesEnumFromString()
    {
        var args = ServiceArgs.From(new Dictionary<string, object?> { ["t"] = "Mailing" });
        Assert.Equal(AddressType.Mailing, args.Get<AddressType>("t"));
    }
}
