using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the start/end slots added to WaBadge in Web Awesome 3.3.0, including the
/// icon-slot convenience parameters.
/// </summary>
public class WaBadgeEnhancementTests : BunitContext
{
    [Fact]
    public void DefaultRender_HasNoStartOrEndSlots()
    {
        var cut = Render<WaBadge>();

        Assert.Empty(cut.FindAll("[slot='start']"));
        Assert.Empty(cut.FindAll("[slot='end']"));
    }

    [Fact]
    public void StartAndEndContent_RenderIntoNamedSlots()
    {
        var cut = Render<WaBadge>(parameters => parameters
            .Add(p => p.StartContent, builder => builder.AddContent(0, "start-frag"))
            .Add(p => p.EndContent, builder => builder.AddContent(0, "end-frag"))
            .AddChildContent("99"));

        Assert.Equal("start-frag", cut.Find("span[slot='start']").TextContent);
        Assert.Equal("end-frag", cut.Find("span[slot='end']").TextContent);
        Assert.Contains("99", cut.Markup);
    }

    [Fact]
    public void StartIconName_And_EndIconName_RenderWaIcons_WhenNoFragment()
    {
        var cut = Render<WaBadge>(parameters => parameters
            .Add(p => p.StartIconName, "star")
            .Add(p => p.EndIconName, "check"));

        Assert.Equal("star", cut.Find("wa-icon[slot='start']").GetAttribute("name"));
        Assert.Equal("check", cut.Find("wa-icon[slot='end']").GetAttribute("name"));
    }

    [Fact]
    public void Fragment_WinsOverIconName()
    {
        var cut = Render<WaBadge>(parameters => parameters
            .Add(p => p.StartContent, builder => builder.AddContent(0, "frag"))
            .Add(p => p.StartIconName, "star"));

        // the fragment renders and the convenience icon is suppressed
        Assert.Equal("frag", cut.Find("span[slot='start']").TextContent);
        Assert.Empty(cut.FindAll("wa-icon[slot='start']"));
    }
}
