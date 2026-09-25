using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaDatePicker wrapper (new in WA 3.8.0). Unlike the date/time inputs it is not a form-associated
/// control, so it implements Value/ValueChanged two-way binding itself, from the native change event; that C#
/// handler logic is tested here, together with the imperative method guard clauses. Its attributes and defaults
/// are covered by RenderedAttributeParityTests and its events by EventCallbackBindingParityTests, both against the
/// CEM; the browser delivery of change by the e2e suite.
/// </summary>
public class WaDatePickerIntegrationTests : BunitContext
{
    public WaDatePickerIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Value_RendersAsTheValueAttribute()
    {
        var cut = Render<WaDatePicker>(parameters => parameters.Add(p => p.Value, SampleDate));

        Assert.Equal(SampleDate, cut.Find("wa-date-picker").GetAttribute("value"));
    }

    [Fact]
    public void ChangeEvent_UpdatesValueAndRaisesValueChanged()
    {
        // C# handler logic only: the change binder must pass the element's value to ValueChanged and keep it
        string? received = null;
        var cut = Render<WaDatePicker>(parameters => parameters
            .Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change(SampleDate);

        Assert.Equal(SampleDate, received);
        Assert.Equal(SampleDate, cut.Instance.Value);
    }

    [Fact]
    public async Task ClearAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ClearAsync());
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task GoToDateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GoToDateAsync(SampleDate));
    }

    [Fact]
    public async Task GoToTodayAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GoToTodayAsync());
    }

    #region ------ Internals ------

    private const string SampleDate = "2026-07-24";

    #endregion
}
