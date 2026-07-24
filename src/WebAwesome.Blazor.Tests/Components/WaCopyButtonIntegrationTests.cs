using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaCopyButton component focusing on validation and the WA 3.7.0 tooltip attribute.
/// </summary>
public class WaCopyButtonIntegrationTests : BunitContext
{
    public WaCopyButtonIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task CopyAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCopyButton();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.CopyAsync());

        Assert.Contains("Cannot copy: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public void Tooltip_DefaultsToFull()
    {
        // WA 3.7.0 added the tooltip attribute (default 'full')
        var cut = Render<WaCopyButton>();

        Assert.Equal("full", cut.Find("wa-copy-button").GetAttribute("tooltip"));
    }

    [Theory]
    [InlineData(WaCopyButtonTooltip.Full, "full")]
    [InlineData(WaCopyButtonTooltip.Copy, "copy")]
    [InlineData(WaCopyButtonTooltip.None, "none")]
    public void Tooltip_WhenSet_RendersExpectedAttribute(WaCopyButtonTooltip tooltip, string expected)
    {
        var cut = Render<WaCopyButton>(parameters => parameters
            .Add(p => p.Tooltip, tooltip));

        Assert.Equal(expected, cut.Find("wa-copy-button").GetAttribute("tooltip"));
    }
}
