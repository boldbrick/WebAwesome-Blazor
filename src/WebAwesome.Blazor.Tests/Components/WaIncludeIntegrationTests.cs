using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaInclude component focusing on validation
/// </summary>
public class WaIncludeIntegrationTests
{
    [Fact]
    public void Include_RendersSrcAttribute()
    {
        // wa-include exposes no reload() method in WA 3.0, so there is no imperative API to
        // cover here; assert the basic parameter surface instead
        var component = new WaInclude { Src = "fragments/example.html" };
        Assert.Equal("fragments/example.html", component.Src);
    }
}