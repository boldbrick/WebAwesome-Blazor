using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Layout;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// The placement metadata of the wrappers for the WA Inspector extension (WebAwesomeOptions.EmitInspectorMetadata):
/// off by default; on, every wrapper writes its type, its component chain and two placement hashes. The switch is
/// process-wide, so these tests run in a collection of their own that never runs in parallel with other tests.
/// </summary>
[Collection(InspectorMetadataCollection.Name)]
public sealed class WaInspectorMetadataTests : BunitContext
{
    [Fact]
    public void Off_ByDefault_WritesNoMetadata()
    {
        Services.AddWebAwesome();

        var cut = Render<WaBadge>(parameters => parameters.AddChildContent("1"));

        var badge = cut.Find("wa-badge");
        Assert.DoesNotContain(badge.Attributes, attribute => attribute.Name.StartsWith(MetadataPrefix, StringComparison.Ordinal));
    }

    [Fact]
    public void On_WritesComponentChainPlacementAndSite()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            builder.OpenComponent<Panel>(0);
            builder.CloseComponent();
        });

        var badge = cut.Find("wa-badge");
        Assert.Equal(nameof(WaBadge), badge.GetAttribute(ComponentAttribute));
        Assert.EndsWith($"{nameof(Panel)}>{nameof(WaBadge)}", badge.GetAttribute(ChainAttribute));
        Assert.Matches(HashPattern, badge.GetAttribute(PlacementAttribute));
        Assert.Matches(HashPattern, badge.GetAttribute(SiteAttribute));
    }

    [Fact]
    public void Chain_LeavesOutFrameworkComponents()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingValue<int>>(0);
            builder.AddComponentParameter(1, nameof(CascadingValue<int>.Value), 1);
            builder.AddComponentParameter(2, nameof(CascadingValue<int>.ChildContent), (RenderFragment)(inner =>
            {
                inner.OpenComponent<Panel>(0);
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var chain = cut.Find("wa-badge").GetAttribute(ChainAttribute)!;
        Assert.DoesNotContain("CascadingValue", chain);
        Assert.EndsWith($"{nameof(Panel)}>{nameof(WaBadge)}", chain);
    }

    [Fact]
    public void LoopInstances_ShareOnePlacement()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            // a Razor @foreach: the loop body reuses its sequence numbers
            for (var index = 0; index < 3; index++)
            {
                builder.OpenComponent<WaBadge>(1);
                builder.AddComponentParameter(2, nameof(WaBadge.ChildContent), (RenderFragment)(content => content.AddContent(0, index)));
                builder.CloseComponent();
            }
        });

        var badges = cut.FindAll("wa-badge");
        Assert.Equal(3, badges.Count);
        Assert.Single(badges.Select(badge => badge.GetAttribute(PlacementAttribute)).Distinct());
        Assert.Single(badges.Select(badge => badge.GetAttribute(SiteAttribute)).Distinct());
    }

    [Fact]
    public void TwoSourcePositions_GetDifferentPlacementsAndSites()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            builder.OpenComponent<WaBadge>(1);
            builder.CloseComponent();
            builder.OpenElement(2, "div");
            builder.OpenComponent<WaBadge>(3);
            builder.CloseComponent();
            builder.CloseElement();
        });

        var badges = cut.FindAll("wa-badge");
        Assert.NotEqual(badges[0].GetAttribute(PlacementAttribute), badges[1].GetAttribute(PlacementAttribute));
        Assert.NotEqual(badges[0].GetAttribute(SiteAttribute), badges[1].GetAttribute(SiteAttribute));
    }

    [Fact]
    public void SharedComponent_AtTwoPositions_SameSiteDifferentPlacement()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            builder.OpenComponent<Panel>(1);
            builder.CloseComponent();
            builder.OpenComponent<Panel>(2);
            builder.CloseComponent();
        });

        var badges = cut.FindAll("wa-badge");
        Assert.Equal(badges[0].GetAttribute(SiteAttribute), badges[1].GetAttribute(SiteAttribute));
        Assert.NotEqual(badges[0].GetAttribute(PlacementAttribute), badges[1].GetAttribute(PlacementAttribute));
    }

    [Fact]
    public void ChildContentOfAWrapper_IsPlacedThroughTheWrapper()
    {
        EnableMetadata();

        var cut = Render(builder =>
        {
            builder.OpenComponent<WaCard>(0);
            builder.AddComponentParameter(1, nameof(WaCard.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<WaBadge>(0);
                content.CloseComponent();
                content.OpenComponent<WaBadge>(1);
                content.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var badges = cut.FindAll("wa-badge");
        Assert.All(badges, badge => Assert.EndsWith($"{nameof(WaCard)}>{nameof(WaBadge)}", badge.GetAttribute(ChainAttribute)));
        Assert.NotEqual(badges[0].GetAttribute(PlacementAttribute), badges[1].GetAttribute(PlacementAttribute));
    }

    [Fact]
    public void Placement_IsStableAcrossRenders()
    {
        EnableMetadata();

        var cut = Render<Panel>();
        var before = Attributes(cut.Find("wa-badge"));

        cut.Render();

        Assert.Equal(before, Attributes(cut.Find("wa-badge")));
    }

    [Fact]
    public void LayoutWrapper_WritesMetadata()
    {
        EnableMetadata();

        var cut = Render<WaStack>(parameters => parameters.AddChildContent("content"));

        Assert.Equal(nameof(WaStack), cut.Find("div.wa-stack").GetAttribute(ComponentAttribute));
    }

    [Fact]
    public void Configuration_BindsEmitInspectorMetadata()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["WebAwesome:EmitInspectorMetadata"] = "true" })
            .Build();
        Services.AddWebAwesome(configuration);

        var options = Services.BuildServiceProvider().GetRequiredService<WebAwesomeOptions>();
        Assert.True(options.EmitInspectorMetadata);
    }

    #region ------ Internals ------

    private void EnableMetadata() => Services.AddWebAwesome(options => options.EmitInspectorMetadata = true);

    private static string Attributes(IElement element) =>
        string.Join(';', element.Attributes.Where(attribute => attribute.Name.StartsWith(MetadataPrefix, StringComparison.Ordinal))
            .Select(attribute => $"{attribute.Name}={attribute.Value}"));

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // the switch is process-wide: leave it off for the other tests
        new ServiceCollection().AddWebAwesome();
        base.Dispose(disposing);
    }

    // a component of the application that renders one wrapper, used at several positions
    private sealed class Panel : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "section");
            builder.OpenComponent<WaBadge>(1);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    private const string MetadataPrefix = "data-wablazor-";
    private const string ComponentAttribute = "data-wablazor-component";
    private const string ChainAttribute = "data-wablazor-chain";
    private const string PlacementAttribute = "data-wablazor-placement";
    private const string SiteAttribute = "data-wablazor-site";
    private const string HashPattern = "^[0-9a-f]{12}$";

    #endregion
}

/// <summary>
/// The test collection of <see cref="WaInspectorMetadataTests"/>: it runs alone, because the metadata switch is
/// process-wide and other tests compare the rendered attributes.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class InspectorMetadataCollection
{
    /// <summary>
    /// The collection name.
    /// </summary>
    public const string Name = "Inspector metadata";
}
