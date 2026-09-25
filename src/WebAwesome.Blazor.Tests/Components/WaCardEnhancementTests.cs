using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for the WaCard content slots (header-actions and footer-actions were added in Web Awesome
/// 3.0.0-beta.6): each fragment parameter must render into its named slot of wa-card, ChildContent into the
/// default slot. The card's attributes, including the orientation added in the same release, are covered by
/// the CEM-driven RenderedAttributeParityTests.
/// </summary>
public class WaCardEnhancementTests : BunitContext
{
    public WaCardEnhancementTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaCard.ChildContent), "")]
    [InlineData(nameof(WaCard.MediaContent), "media")]
    [InlineData(nameof(WaCard.HeaderContent), "header")]
    [InlineData(nameof(WaCard.HeaderActionsContent), "header-actions")]
    [InlineData(nameof(WaCard.FooterContent), "footer")]
    [InlineData(nameof(WaCard.FooterActionsContent), "footer-actions")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaCard>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find(CardTag)));
    }

    [Fact]
    public void DefaultRender_RendersNoSlotWrappers()
    {
        var cut = Render<WaCard>();

        Assert.Empty(cut.FindAll("[slot]"));
    }

    #region ------ Internals ------

    private const string CardTag = "wa-card";

    #endregion
}
