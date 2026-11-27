using Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Canchero.Presentation.UnitTest;

public sealed class ControllerRouteTests
{
    [Fact]
    public void AuthController_UsesVersionedApiRoute()
    {
        var route = typeof(AuthController).GetCustomAttributes(typeof(RouteAttribute), false)
            .Cast<RouteAttribute>().Single();
        Assert.Equal("api/v1/auth", route.Template);
    }
}
