using System;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The [Flags] enums rendered as space-separated token lists: WaIframeSandbox (the iframe sandbox tokens, None being
/// the fully restricted empty sandbox) and WaPageSections (wa-page's CSS-only disable-sticky tokens, which no CEM
/// union covers, so they are pinned here against page.md and the page's :host([disable-sticky~='…']) rules).
/// </summary>
public class FlagTokenTests
{
    [Fact]
    public void Sandbox_JoinsTheSetFlags_InDeclarationOrder()
    {
        Assert.Equal("allow-forms allow-scripts", (WaIframeSandbox.AllowScripts | WaIframeSandbox.AllowForms).ToHtmlValue());
        Assert.Equal(string.Empty, WaIframeSandbox.None.ToHtmlValue());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((WaIframeSandbox)(1 << 20)).ToHtmlValue());
    }

    [Fact]
    public void PageSections_JoinTheSetFlags_InThePagesOrder()
    {
        Assert.Equal("banner header subheader aside menu",
            (WaPageSections.Menu | WaPageSections.Aside | WaPageSections.Subheader | WaPageSections.Header | WaPageSections.Banner).ToHtmlValue());
        Assert.Equal("header", WaPageSections.Header.ToHtmlValue());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((WaPageSections)0).ToHtmlValue());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((WaPageSections)64).ToHtmlValue());
    }
}
