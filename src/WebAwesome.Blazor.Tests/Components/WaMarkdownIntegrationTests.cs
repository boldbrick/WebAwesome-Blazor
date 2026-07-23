using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaMarkdown component introduced in Web Awesome 3.5.0.
/// </summary>
public class WaMarkdownIntegrationTests : BunitContext
{
    public WaMarkdownIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsTabSize_AndEmitsMarkdownScriptChild()
    {
        var cut = Render<WaMarkdown>();

        var element = cut.Find("wa-markdown");
        // unset TabSize emits no attribute so the Web Awesome default (4) applies
        Assert.False(element.HasAttribute("tab-size"));

        // the required source channel is always present
        var script = cut.Find("wa-markdown > script");
        Assert.Equal("text/markdown", script.GetAttribute("type"));
    }

    [Fact]
    public void TabSize_WhenSet_RendersAttribute()
    {
        var cut = Render<WaMarkdown>(parameters => parameters
            .Add(p => p.TabSize, 2));

        Assert.Equal("2", cut.Find("wa-markdown").GetAttribute("tab-size"));
    }

    [Fact]
    public void Content_RendersIntoMarkdownScript()
    {
        var cut = Render<WaMarkdown>(parameters => parameters
            .Add(p => p.Content, "## Hello"));

        var script = cut.Find("wa-markdown > script[type='text/markdown']");
        Assert.Contains("## Hello", script.TextContent);
    }

    [Fact]
    public void ChildContent_TakesPrecedenceOverContent()
    {
        var cut = Render<WaMarkdown>(parameters => parameters
            .Add(p => p.Content, "from-content")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "from-fragment")));

        var script = cut.Find("wa-markdown > script[type='text/markdown']");
        Assert.Contains("from-fragment", script.TextContent);
        Assert.DoesNotContain("from-content", script.TextContent);
    }

    [Fact]
    public async Task RenderMarkdownAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaMarkdown();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.RenderMarkdownAsync());
        Assert.Contains("Cannot render markdown: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task RenderMarkdownAsync_WithRenderedElement_InvokesRenderMarkdownOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaMarkdown>();

        await cut.InvokeAsync(() => cut.Instance.RenderMarkdownAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("renderMarkdown", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
