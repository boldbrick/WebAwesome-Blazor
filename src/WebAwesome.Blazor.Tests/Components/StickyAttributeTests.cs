using System;
using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The sticky attribute rule on the owner's examples: a parameter left at its default renders nothing, but once its
/// attribute was rendered, returning to the default (or to null) renders the element default instead of removing the
/// attribute, which would make Lit set the element property to null (a slider's max 0, an input's type null). Exempt:
/// boolean attributes (a removed Lit boolean reads false, the default) and attributes without a literal default. The
/// browser half is sticky-attributes.spec.js; RenderedAttributeParityTests (i) checks every wrapper parameter. The date of
/// wa-relative-time and wa-format-date defaults to new Date(), so a return to null renders the clock's current instant.
/// </summary>
public class StickyAttributeTests : BunitContext
{
    public StickyAttributeTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SliderMax_BackToTheDefault_RendersTheDefault()
    {
        var cut = Render<WaSlider>(p => p.Add(x => x.ValueExpression, () => sliderValue));
        Assert.False(cut.Find("wa-slider").HasAttribute("max"));

        cut.Render(p => p.Add(x => x.Max, 50m));
        Assert.Equal("50", cut.Find("wa-slider").GetAttribute("max"));

        cut.Render(p => p.Add(x => x.Max, WaSlider.DefaultMax));
        Assert.Equal("100", cut.Find("wa-slider").GetAttribute("max"));
    }

    [Fact]
    public void InputType_BackToText_RendersText()
    {
        var cut = Render<WaInput>(p => p.Add(x => x.ValueExpression, () => textValue));
        Assert.False(cut.Find("wa-input").HasAttribute("type"));

        cut.Render(p => p.Add(x => x.Type, WaInputType.Password));
        Assert.Equal("password", cut.Find("wa-input").GetAttribute("type"));

        cut.Render(p => p.Add(x => x.Type, WaInput.DefaultType));
        Assert.Equal("text", cut.Find("wa-input").GetAttribute("type"));
    }

    [Fact]
    public void NullableDistance_BackToNull_RendersTheElementDefault()
    {
        var cut = Render<WaTooltip>(p => p.Add(x => x.For, "anchor"));
        Assert.False(cut.Find("wa-tooltip").HasAttribute("distance"));

        cut.Render(p => p.Add(x => x.Distance, 20));
        Assert.Equal("20", cut.Find("wa-tooltip").GetAttribute("distance"));

        cut.Render(p => p.Add(x => x.Distance, (int?)null));
        Assert.Equal("8", cut.Find("wa-tooltip").GetAttribute("distance"));
    }

    [Fact]
    public void ExplicitDefault_OnTheFirstRender_RendersNothing()
    {
        var cut = Render<WaSlider>(p => p
            .Add(x => x.ValueExpression, () => sliderValue)
            .Add(x => x.Max, WaSlider.DefaultMax));

        Assert.False(cut.Find("wa-slider").HasAttribute("max"));
    }

    [Fact]
    public void BooleanAndDefaultlessAttributes_AreRemoved()
    {
        var cut = Render<WaInput>(p => p
            .Add(x => x.ValueExpression, () => textValue)
            .Add(x => x.Pill, true)
            .Add(x => x.Pattern, "[0-9]+"));

        cut.Render(p => p
            .Add(x => x.Pill, false)
            .Add(x => x.Pattern, (string?)null));

        var element = cut.Find("wa-input");
        Assert.False(element.HasAttribute("pill"));
        Assert.False(element.HasAttribute("pattern"));
    }

    [Fact]
    public void RelativeTimeDate_BackToNull_RendersTheClocksCurrentInstant()
    {
        Services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedNow));
        var cut = Render<WaRelativeTime>();
        Assert.False(cut.Find("wa-relative-time").HasAttribute("date"));

        cut.Render(p => p.Add(x => x.Date, PastDate));
        Assert.Equal(PastDateText, cut.Find("wa-relative-time").GetAttribute("date"));

        // the element's own default is new Date(); a removed attribute would read as the 1970 epoch
        cut.Render(p => p.Add(x => x.Date, (DateTimeOffset?)null));
        Assert.Equal(FixedNowText, cut.Find("wa-relative-time").GetAttribute("date"));
    }

    [Fact]
    public void FormatDateDate_BackToNull_RendersTheClocksCurrentInstant()
    {
        Services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedNow));
        var cut = Render<WaFormatDate>();
        Assert.False(cut.Find("wa-format-date").HasAttribute("date"));

        cut.Render(p => p.Add(x => x.Date, PastDate));
        Assert.Equal(PastDateText, cut.Find("wa-format-date").GetAttribute("date"));

        cut.Render(p => p.Add(x => x.Date, (DateTimeOffset?)null));
        Assert.Equal(FixedNowText, cut.Find("wa-format-date").GetAttribute("date"));
    }

    [Fact]
    public void DateBackToNull_WithoutARegisteredClock_RendersTheSystemClocksCurrentInstant()
    {
        var cut = Render<WaRelativeTime>(p => p.Add(x => x.Date, PastDate));
        cut.Render(p => p.Add(x => x.Date, (DateTimeOffset?)null));

        var rendered = DateTimeOffset.Parse(cut.Find("wa-relative-time").GetAttribute("date")!, CultureInfo.InvariantCulture);
        Assert.InRange(DateTimeOffset.UtcNow - rendered, TimeSpan.Zero, SystemClockTolerance);
    }

    #region ------ Internals ------

    // the bound fields the ValueExpressions point at
    private readonly decimal? sliderValue = null;
    private readonly string? textValue = null;

    private static readonly DateTimeOffset FixedNow = new(2026, 9, 26, 10, 30, 0, TimeSpan.Zero);
    private const string FixedNowText = "2026-09-26T10:30:00.000+00:00";
    private static readonly DateTimeOffset PastDate = new(2020, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private const string PastDateText = "2020-01-01T12:00:00.000+00:00";
    private static readonly TimeSpan SystemClockTolerance = TimeSpan.FromMinutes(1);

    // a clock standing still at the given instant
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    #endregion
}
