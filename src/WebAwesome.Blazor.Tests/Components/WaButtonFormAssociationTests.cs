using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render-level tests for the WaButton form-association attribute. The form attribute left the
/// Web Awesome 3.1.0 CEM because it moved to native platform form association (ElementInternals),
/// but it remains fully functional — the wrapper deliberately keeps the Form parameter so a submit
/// button outside an EditForm can still drive it.
/// </summary>
public class WaButtonFormAssociationTests : BunitContext
{
    public WaButtonFormAssociationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Form_WhenSet_RendersFormAttribute()
    {
        // Arrange & Act
        var cut = Render<WaButton>(parameters => parameters
            .Add(p => p.Form, "external-form")
            .Add(p => p.Type, WaButtonType.Submit));

        // Assert
        var element = cut.Find("wa-button");
        Assert.Equal("external-form", element.GetAttribute("form"));
        Assert.Equal("submit", element.GetAttribute("type"));
    }

    [Fact]
    public void Form_WhenUnset_DoesNotRenderFormAttribute()
    {
        // Arrange & Act
        var cut = Render<WaButton>();

        // Assert
        Assert.False(cut.Find("wa-button").HasAttribute("form"));
    }
}
