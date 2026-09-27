using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for WaDayContent (3.12.0), the per-day content of the date controls: it renders a span into the host
/// element's day-YYYY-MM-DD slot, named in Web Awesome's exact ISO form whatever the culture, as a direct child of the
/// element (slot assignment needs one) for each of the four hosts, which cascade themselves through their ChildContent;
/// outside that ChildContent it throws. Several contents for one date all render, as Web Awesome shows every element
/// assigned to one slot name. The C# half of the wa-date-input forwarding nudge is covered here (the interop call after
/// a change of the day slots, and only then); the browser proof that the element forwards them is day-content.spec.js.
/// </summary>
public class WaDayContentTests : BunitContext
{
    public WaDayContentTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// The four day content hosts with the tag each renders.
    /// </summary>
    public static TheoryData<Type, string> Hosts => new()
    {
        { typeof(WaDatePicker), DatePickerTag },
        { typeof(WaDateRangePicker), DatePickerTag },
        { typeof(WaDateInput), DateInputTag },
        { typeof(WaDateRangeInput), DateInputTag },
    };

    [Theory]
    [MemberData(nameof(Hosts))]
    public void DayContent_RendersItsSlotAsADirectChildOfTheHostElement(Type hostType, string tag)
    {
        var cut = Render(Host(hostType, DayContent(Christmas, ChristmasText)));

        var element = cut.Find(tag);
        var slotted = Assert.Single(element.Children, c => c.HasAttribute(SlotAttribute));
        Assert.Equal(ChristmasSlot, slotted.GetAttribute(SlotAttribute));
        Assert.Equal(ChristmasText, slotted.TextContent);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public void ChildContent_RendersDirectlyInsideTheHostElement(Type hostType, string tag)
    {
        RenderFragment content = builder =>
        {
            builder.OpenElement(0, MarkerTag);
            builder.CloseElement();
        };

        var cut = Render(Host(hostType, content));

        Assert.Equal(tag, cut.Find(MarkerTag).ParentElement!.LocalName);
    }

    [Theory]
    [InlineData(HostileCultureName)]
    [InlineData(BuddhistCalendarCultureName)]
    [InlineData(HijriCalendarCultureName)]
    public void SlotName_IsTheIsoDate_InAnyCulture(string cultureName)
    {
        var culture = cultureName == HostileCultureName ? RenderedAttributeParityTests.HostileCulture : CultureInfo.GetCultureInfo(cultureName);
        using var scope = new CultureScope(culture);

        var cut = Render(Host(typeof(WaDatePicker), DayContent(Christmas, ChristmasText)));

        Assert.Equal(ChristmasSlot, cut.Find(SlottedSelector).GetAttribute(SlotAttribute));
    }

    [Fact]
    public void SlotName_PadsTheYearToFourDigits_AsWebAwesomeDoes()
    {
        var cut = Render(Host(typeof(WaDatePicker), DayContent(new DateOnly(999, 1, 5), ChristmasText)));

        Assert.Equal("day-0999-01-05", cut.Find(SlottedSelector).GetAttribute(SlotAttribute));
    }

    [Fact]
    public void Content_IsRenderedInsideTheSlotElement()
    {
        RenderFragment markup = builder =>
        {
            builder.OpenElement(0, "strong");
            builder.AddContent(1, ChristmasText);
            builder.CloseElement();
        };

        var cut = Render(Host(typeof(WaDatePicker), DayContent(Christmas, markup)));

        var slotted = cut.Find(SlottedSelector);
        Assert.Equal("strong", Assert.Single(slotted.Children).LocalName);
        Assert.Equal(ChristmasText, slotted.TextContent);
    }

    [Fact]
    public void WithoutChildContent_RendersNothing_SoTheDayKeepsItsNumber()
    {
        var cut = Render(Host(typeof(WaDatePicker), DayContent(Christmas, (RenderFragment?)null)));

        Assert.Empty(cut.Find(DatePickerTag).Children);
    }

    [Fact]
    public void SeveralContentsForOneDate_AllRender_InDocumentOrder()
    {
        RenderFragment both = builder =>
        {
            builder.AddContent(0, DayContent(Christmas, ChristmasText));
            builder.AddContent(1, DayContent(Christmas, SecondText));
        };

        var cut = Render(Host(typeof(WaDatePicker), both));

        var slotted = cut.Find(DatePickerTag).Children;
        Assert.Equal(new[] { ChristmasSlot, ChristmasSlot }, slotted.Select(c => c.GetAttribute(SlotAttribute)));
        Assert.Equal(new[] { ChristmasText, SecondText }, slotted.Select(c => c.TextContent));
    }

    [Fact]
    public void OutsideAHost_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Render<WaDayContent>(parameters => parameters
            .Add(p => p.Date, Christmas)
            .AddChildContent(ChristmasText)));

        Assert.Contains(nameof(WaDatePicker), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(WaDateInput), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InAnotherSlotOfAHost_Throws()
    {
        // only the ChildContent cascades the host: a day slot inside another slot's wrapper would not be a direct child
        Assert.Throws<InvalidOperationException>(() => Render<WaDatePicker>(parameters => parameters
            .Add(p => p.FooterContent, DayContent(Christmas, ChristmasText))));
    }

    [Fact]
    public void DateInput_SignalsTheSlotChange_WhenADayContentIsAdded()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render<DayContentList>(parameters => parameters.Add(p => p.HostType, typeof(WaDateInput)));
        var before = SignalCount(module);

        cut.Render(parameters => parameters.Add(p => p.Dates, new[] { Christmas }));

        Assert.Equal(before + 1, SignalCount(module));
    }

    [Fact]
    public void DateInput_SignalsTheSlotChange_WhenADayContentIsRemoved_OrMovedToAnotherDate()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render<DayContentList>(parameters => parameters
            .Add(p => p.HostType, typeof(WaDateInput))
            .Add(p => p.Dates, new[] { Christmas, NewYear }));
        var before = SignalCount(module);

        // the first content moves to the next date, the second is disposed
        cut.Render(parameters => parameters.Add(p => p.Dates, new[] { NewYear }));

        Assert.Equal(before + 1, SignalCount(module));
    }

    [Fact]
    public void DateInput_DoesNotSignal_WhenTheDaySlotsStayTheSame()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render<DayContentList>(parameters => parameters
            .Add(p => p.HostType, typeof(WaDateInput))
            .Add(p => p.Dates, new[] { Christmas }));
        var before = SignalCount(module);

        cut.Render(parameters => parameters.Add(p => p.Dates, new[] { Christmas }).Add(p => p.Text, SecondText));

        Assert.Equal(before, SignalCount(module));
    }

    [Fact]
    public async Task DateInput_SignalsTheSlotChange_OfADayContentRenderedWithoutTheInput()
    {
        // the day content sits in a component of its own, whose render does not render the input
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render(Host(typeof(WaDateInput), builder =>
        {
            builder.OpenComponent<IndependentDayContent>(0);
            builder.CloseComponent();
        }));
        var before = SignalCount(module);

        await cut.FindComponent<IndependentDayContent>().Instance.ShowAsync();

        Assert.Equal(ChristmasSlot, cut.Find(SlottedSelector).GetAttribute(SlotAttribute));
        Assert.Equal(before + 1, SignalCount(module));
    }

    [Fact]
    public void DatePicker_NeverSignals()
    {
        // wa-date-picker renders a day slot in every cell, so the browser assigns day content on its own
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render<DayContentList>(parameters => parameters.Add(p => p.HostType, typeof(WaDatePicker)));

        cut.Render(parameters => parameters.Add(p => p.Dates, new[] { Christmas, NewYear }));
        cut.Render(parameters => parameters.Add(p => p.Dates, Array.Empty<DateOnly>()));

        Assert.Equal(0, SignalCount(module));
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string SignalIdentifier = "signalDefaultSlotChange";
    private const string DatePickerTag = "wa-date-picker";
    private const string DateInputTag = "wa-date-input";
    private const string SlotAttribute = "slot";
    private const string SlottedSelector = "[slot]";
    private const string MarkerTag = "day-content-marker";
    private const string ChristmasSlot = "day-2026-12-25";
    private const string ChristmasText = "🎄";
    private const string SecondText = "🎁";
    private const string HostileCultureName = "hostile";
    private const string BuddhistCalendarCultureName = "th-TH";
    private const string HijriCalendarCultureName = "ar-SA";
    private const string ValueExpressionParameter = "ValueExpression";
    private const string ChildContentParameter = "ChildContent";

    private static readonly DateOnly Christmas = new(2026, 12, 25);
    private static readonly DateOnly NewYear = new(2027, 1, 1);

    private static int SignalCount(BunitJSModuleInterop module) => module.Invocations.Count(i => i.Identifier == SignalIdentifier);

    // renders a host with the given ChildContent; an input host is bound to a field of a fresh holder, as it requires
    // a ValueExpression
    private static RenderFragment Host(Type hostType, RenderFragment content) => builder =>
    {
        builder.OpenComponent(0, hostType);
        if (hostType == typeof(WaDateInput))
        {
            var bound = new BoundValue<DateOnly?>();
            builder.AddComponentParameter(1, ValueExpressionParameter, (Expression<Func<DateOnly?>>)(() => bound.Value));
        }
        else if (hostType == typeof(WaDateRangeInput))
        {
            var bound = new BoundValue<WaDateRange?>();
            builder.AddComponentParameter(1, ValueExpressionParameter, (Expression<Func<WaDateRange?>>)(() => bound.Value));
        }
        builder.AddComponentParameter(2, ChildContentParameter, content);
        builder.CloseComponent();
    };

    /// <summary>
    /// The target of an input host's ValueExpression.
    /// </summary>
    /// <typeparam name="T">The bound value type</typeparam>
    private sealed class BoundValue<T>
    {
        public T? Value { get; set; }
    }

    private static RenderFragment DayContent(DateOnly date, string text) => DayContent(date, builder => builder.AddContent(0, text));

    private static RenderFragment DayContent(DateOnly date, RenderFragment? content) => builder =>
    {
        builder.OpenComponent<WaDayContent>(0);
        builder.AddComponentParameter(1, nameof(WaDayContent.Date), date);
        builder.AddComponentParameter(2, nameof(WaDayContent.ChildContent), content);
        builder.CloseComponent();
    };

    /// <summary>
    /// A host whose ChildContent holds one day content per date, rendered in a loop without keys (so removing the
    /// first date moves its day content to the next one).
    /// </summary>
    private sealed class DayContentList : ComponentBase
    {
        [Parameter] public Type HostType { get; set; } = typeof(WaDatePicker);

        [Parameter] public DateOnly[] Dates { get; set; } = [];

        [Parameter] public string Text { get; set; } = ChristmasText;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Host(HostType, content =>
            {
                foreach (var date in Dates)
                    content.AddContent(0, DayContent(date, Text));
            }));
        }
    }

    /// <summary>
    /// A day content inside a component of its own, shown by <see cref="ShowAsync"/>, which renders only this
    /// component.
    /// </summary>
    private sealed class IndependentDayContent : ComponentBase
    {
        public Task ShowAsync() => InvokeAsync(() =>
        {
            shown = true;
            StateHasChanged();
        });

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (shown) builder.AddContent(0, DayContent(Christmas, ChristmasText));
        }

        private bool shown;
    }

    #endregion
}
