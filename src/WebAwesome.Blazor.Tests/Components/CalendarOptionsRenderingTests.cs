using System;
using System.Collections.Generic;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The typed calendar options (IWaCalendarOptions) of every date wrapper render in Web Awesome's culture-free wire
/// formats, through the one renderer the date input and the date picker share: ISO yyyy-MM-dd bounds, the disabled
/// dates as ascending ISO dates separated by a space, the disabled weekdays as sun..sat tokens in DayOfWeek order,
/// and nothing for a null or empty set (owner rule: an empty collection omits the attribute).
/// </summary>
public class CalendarOptionsRenderingTests : BunitContext
{
    public CalendarOptionsRenderingTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// The date wrappers and the tag each renders.
    /// </summary>
    public static TheoryData<Type, string> CalendarWrappers => new()
    {
        { typeof(WaDateInput), "wa-date-input" },
        { typeof(WaDatePicker), "wa-date-picker" },
    };

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void DateBounds_RenderAsIso_InAnyCulture(Type wrapper, string tag)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var element = RenderWrapper(wrapper, tag, ("Min", new DateOnly(2026, 1, 2)), ("Max", new DateOnly(2026, 12, 31)),
            ("Today", new DateOnly(987, 3, 4)));

        Assert.Equal("2026-01-02", element.GetAttribute("min"));
        Assert.Equal("2026-12-31", element.GetAttribute("max"));
        Assert.Equal("0987-03-04", element.GetAttribute("today"));
    }

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void DisabledDates_RenderAscendingIso_SpaceSeparated(Type wrapper, string tag)
    {
        var dates = new HashSet<DateOnly> { new(2026, 12, 25), new(2026, 1, 1), new(2026, 12, 24) };

        var element = RenderWrapper(wrapper, tag, ("DisabledDates", dates));

        Assert.Equal("2026-01-01 2026-12-24 2026-12-25", element.GetAttribute("disabled-dates"));
    }

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void DisabledDaysOfWeek_RenderWeekdayTokens_SundayFirst(Type wrapper, string tag)
    {
        var days = new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Wednesday, DayOfWeek.Sunday };

        var element = RenderWrapper(wrapper, tag, ("DisabledDaysOfWeek", days));

        Assert.Equal("sun wed sat", element.GetAttribute("disabled-days-of-week"));
    }

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void EmptySets_RenderNoAttribute(Type wrapper, string tag)
    {
        var element = RenderWrapper(wrapper, tag, ("DisabledDates", new HashSet<DateOnly>()), ("DisabledDaysOfWeek", new HashSet<DayOfWeek>()));

        Assert.False(element.HasAttribute("disabled-dates"));
        Assert.False(element.HasAttribute("disabled-days-of-week"));
    }

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void UnsetOptions_RenderNoAttribute(Type wrapper, string tag)
    {
        var element = RenderWrapper(wrapper, tag);

        foreach (var attribute in new[] { "min", "max", "today", "disabled-dates", "disabled-days-of-week" })
            Assert.False(element.HasAttribute(attribute), attribute);
    }

    [Theory]
    [MemberData(nameof(CalendarWrappers))]
    public void CalendarOptions_AreDeclaredThroughTheSharedInterface(Type wrapper, string tag)
    {
        Assert.True(typeof(IWaCalendarOptions).IsAssignableFrom(wrapper), $"{wrapper.Name} ({tag}) implements IWaCalendarOptions");
    }

    #region ------ Internals ------

    // renders the wrapper with the given parameters (and a ValueExpression for an InputBase) and returns its root
    private AngleSharp.Dom.IElement RenderWrapper(Type wrapper, string tag, params (string Name, object Value)[] parameters)
    {
        DateOnly? value = null;
        var cut = Render(builder =>
        {
            builder.OpenComponent(0, wrapper);
            if (typeof(WaDateInputBase<DateOnly?>).IsAssignableFrom(wrapper))
                builder.AddComponentParameter(1, "ValueExpression", (System.Linq.Expressions.Expression<Func<DateOnly?>>)(() => value));

            var sequence = 2;
            foreach (var (name, parameterValue) in parameters)
                builder.AddComponentParameter(sequence++, name, parameterValue);

            builder.CloseComponent();
        });

        return cut.Find(tag);
    }

    #endregion
}
