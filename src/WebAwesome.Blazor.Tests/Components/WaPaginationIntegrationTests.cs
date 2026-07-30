using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaPagination wrapper, new in Web Awesome 3.11.0. Covers default attribute
/// omission, kebab-case attribute rendering when parameters are set, the appearance/format enum
/// mappings, the before/after page-change events, and the four icon slots (RenderFragment and
/// icon-name forms).
/// </summary>
public class WaPaginationIntegrationTests : BunitContext
{
    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaPagination>();

        var element = cut.Find("wa-pagination");
        Assert.False(element.HasAttribute("appearance"));
        Assert.False(element.HasAttribute("format"));
        Assert.False(element.HasAttribute("boundary-count"));
        Assert.False(element.HasAttribute("sibling-count"));
        Assert.False(element.HasAttribute("href-template"));
        Assert.False(element.HasAttribute("label"));
        Assert.False(element.HasAttribute("page"));
        Assert.False(element.HasAttribute("page-size"));
        Assert.False(element.HasAttribute("total"));
        // boolean attributes are omitted when false
        Assert.False(element.HasAttribute("disabled"));
        Assert.False(element.HasAttribute("hide-single-page"));
        Assert.False(element.HasAttribute("with-edges"));
        Assert.False(element.HasAttribute("with-summary"));
        Assert.False(element.HasAttribute("without-nav"));
    }

    [Fact]
    public void Attributes_WhenSet_RenderExpectedKebabCaseNames()
    {
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.Appearance, WaPaginationAppearance.Filled)
            .Add(p => p.Format, WaPaginationFormat.Compact)
            .Add(p => p.Disabled, true)
            .Add(p => p.BoundaryCount, 2)
            .Add(p => p.SiblingCount, 1)
            .Add(p => p.HideSinglePage, true)
            .Add(p => p.HrefTemplate, "/products?page={page}")
            .Add(p => p.Label, "Product pages")
            .Add(p => p.Page, 3)
            .Add(p => p.PageSize, 10)
            .Add(p => p.Total, 237)
            .Add(p => p.WithEdges, true)
            .Add(p => p.WithSummary, true)
            .Add(p => p.WithoutNav, true));

        var element = cut.Find("wa-pagination");
        Assert.Equal("filled", element.GetAttribute("appearance"));
        Assert.Equal("compact", element.GetAttribute("format"));
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("2", element.GetAttribute("boundary-count"));
        Assert.Equal("1", element.GetAttribute("sibling-count"));
        Assert.True(element.HasAttribute("hide-single-page"));
        Assert.Equal("/products?page={page}", element.GetAttribute("href-template"));
        Assert.Equal("Product pages", element.GetAttribute("label"));
        Assert.Equal("3", element.GetAttribute("page"));
        Assert.Equal("10", element.GetAttribute("page-size"));
        Assert.Equal("237", element.GetAttribute("total"));
        Assert.True(element.HasAttribute("with-edges"));
        Assert.True(element.HasAttribute("with-summary"));
        Assert.True(element.HasAttribute("without-nav"));
    }

    [Fact]
    public void Class_And_Style_AreApplied()
    {
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Style, "margin-top: 1rem;"));

        var element = cut.Find("wa-pagination");
        Assert.Equal("custom-class", element.GetAttribute("class"));
        Assert.Equal("margin-top: 1rem;", element.GetAttribute("style"));
    }

    [Fact]
    public void Appearance_MapsToHtmlValue()
    {
        Assert.Equal("outlined", WaPaginationAppearance.Outlined.ToHtmlValue());
        Assert.Equal("filled", WaPaginationAppearance.Filled.ToHtmlValue());
        Assert.Equal("plain", WaPaginationAppearance.Plain.ToHtmlValue());
    }

    [Fact]
    public void Format_MapsToHtmlValue()
    {
        Assert.Equal("standard", WaPaginationFormat.Standard.ToHtmlValue());
        Assert.Equal("compact", WaPaginationFormat.Compact.ToHtmlValue());
    }

    [Fact]
    public void OnBeforePageChange_WhenWired_ReceivesDomEventWithPage()
    {
        WaPaginationPageChangeEventArgs? received = null;
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.OnBeforePageChange, args => received = args));

        cut.Find("wa-pagination").TriggerEvent("onwa-before-page-change",
            new WaPaginationPageChangeEventArgs { Page = 4, PageSize = 10 });

        Assert.NotNull(received);
        Assert.Equal(4, received!.Page);
        Assert.Equal(10, received.PageSize);
    }

    [Fact]
    public void OnPageChange_WhenWired_ReceivesDomEventWithPage()
    {
        WaPaginationPageChangeEventArgs? received = null;
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.OnPageChange, args => received = args));

        cut.Find("wa-pagination").TriggerEvent("onwa-page-change",
            new WaPaginationPageChangeEventArgs { Page = 5, PageSize = 10 });

        Assert.NotNull(received);
        Assert.Equal(5, received!.Page);
        Assert.Equal(10, received.PageSize);
    }

    [Fact]
    public void IconContentSlots_WhenProvided_RenderIntoNamedSlots()
    {
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.FirstIconContent, b => b.AddContent(0, "first"))
            .Add(p => p.LastIconContent, b => b.AddContent(0, "last"))
            .Add(p => p.NextIconContent, b => b.AddContent(0, "next"))
            .Add(p => p.PreviousIconContent, b => b.AddContent(0, "previous")));

        Assert.Equal("first", cut.Find("span[slot='first-icon']").TextContent);
        Assert.Equal("last", cut.Find("span[slot='last-icon']").TextContent);
        Assert.Equal("next", cut.Find("span[slot='next-icon']").TextContent);
        Assert.Equal("previous", cut.Find("span[slot='previous-icon']").TextContent);
    }

    [Fact]
    public void IconNames_WhenProvidedWithoutContent_RenderWaIconIntoSlots()
    {
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.FirstIconName, "chevrons-left")
            .Add(p => p.LastIconName, "chevrons-right")
            .Add(p => p.NextIconName, "chevron-right")
            .Add(p => p.PreviousIconName, "chevron-left"));

        Assert.Equal("chevrons-left", cut.Find("wa-icon[slot='first-icon']").GetAttribute("name"));
        Assert.Equal("chevrons-right", cut.Find("wa-icon[slot='last-icon']").GetAttribute("name"));
        Assert.Equal("chevron-right", cut.Find("wa-icon[slot='next-icon']").GetAttribute("name"));
        Assert.Equal("chevron-left", cut.Find("wa-icon[slot='previous-icon']").GetAttribute("name"));
    }

    [Fact]
    public void IconContent_TakesPrecedenceOverIconName()
    {
        var cut = Render<WaPagination>(parameters => parameters
            .Add(p => p.FirstIconContent, b => b.AddContent(0, "first"))
            .Add(p => p.FirstIconName, "chevrons-left"));

        Assert.Equal("first", cut.Find("span[slot='first-icon']").TextContent);
        Assert.Empty(cut.FindAll("wa-icon[slot='first-icon']"));
    }
}
