using Nexsure.Api;
using Nexsure.Api.Services.Auth;
using Nexsure.Api.Services.Clients;
using Nexsure.Api.Services.Lookup;
using Xunit;

namespace Nexsure.Api.Tests;

public class ServiceFactoryTests
{
    [Fact]
    public void DiscoversAllNineteenServices()
    {
        var factory = new ServiceFactory();
        Assert.Equal(19, factory.Services.Count);
    }

    [Fact]
    public void DiscoversServicesByName()
    {
        var factory = new ServiceFactory();
        Assert.Same(typeof(GetToken), factory.Services["GetToken"]);
        Assert.Same(typeof(GetClientList), factory.Services["GetClientList"]);
        Assert.Same(typeof(ListLookupManagementValues), factory.Services["ListLookupManagementValues"]);
    }

    [Fact]
    public void DoesNotDiscoverAbstractBase()
    {
        var factory = new ServiceFactory();
        Assert.DoesNotContain("AbstractService`1", factory.Services.Keys);
    }
}
