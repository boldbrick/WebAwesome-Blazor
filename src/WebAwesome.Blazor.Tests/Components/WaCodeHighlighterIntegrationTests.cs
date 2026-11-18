using Bunit;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaCodeHighlighter component (Web Awesome 3.15.0): the code comes either from the
/// Code parameter (the element's code attribute) or from the default slot, and the two are independent paths of the
/// wrapper. Its attributes and defaults are covered by RenderedAttributeParityTests, its load and error events by
/// EventCallbackBindingParityTests.
/// </summary>
public class WaCodeHighlighterIntegrationTests : BunitContext
{
    [Fact]
    public void ChildContent_RendersIntoTheDefaultSlot_WithoutCodeAttribute()
    {
        // Arrange & Act
        var cut = Render<WaCodeHighlighter>(parameters => parameters.Add(p => p.ChildContent, SlotProbe.Fragment));

        // Assert
        var element = cut.Find("wa-code-highlighter");
        Assert.Equal(string.Empty, SlotProbe.SlotOf(element));
        Assert.False(element.HasAttribute("code"));
    }

    [Fact]
    public void Code_RendersTheCodeAttribute_WithoutSlotContent()
    {
        // Arrange & Act
        var cut = Render<WaCodeHighlighter>(parameters => parameters.Add(p => p.Code, "let a = 1;"));

        // Assert
        var element = cut.Find("wa-code-highlighter");
        Assert.Equal("let a = 1;", element.GetAttribute("code"));
        Assert.Empty(element.Children);
        Assert.Equal(string.Empty, element.TextContent);
    }

    [Fact]
    public void CodeAndChildContent_BothRender()
    {
        // Arrange & Act - the wrapper forwards both, the element decides which one it reads
        var cut = Render<WaCodeHighlighter>(parameters => parameters
            .Add(p => p.Code, "let a = 1;")
            .Add(p => p.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "let b = 2;"))));

        // Assert
        var element = cut.Find("wa-code-highlighter");
        Assert.Equal("let a = 1;", element.GetAttribute("code"));
        Assert.Equal("let b = 2;", element.TextContent);
    }
}
