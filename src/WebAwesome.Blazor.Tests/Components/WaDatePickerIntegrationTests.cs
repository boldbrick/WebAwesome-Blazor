using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaDatePicker wrapper (new in WA 3.8.0, typed DateOnly? since 3.12.0). Unlike the date/time inputs it
/// is not a form-associated control, so it implements Value/ValueChanged two-way binding itself, from the native
/// change event; that C# handler logic and its culture-free ISO conversion are tested here, together with the
/// imperative methods and the typed event args. Its attributes and defaults are covered by
/// RenderedAttributeParityTests and its events by EventCallbackBindingParityTests, both against the CEM; the
/// browser delivery of change by the e2e suite.
/// </summary>
public class WaDatePickerIntegrationTests : BunitContext
{
    public WaDatePickerIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Value_RendersAsIsoValueAttribute_InAnyCulture()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var cut = Render<WaDatePicker>(parameters => parameters.Add(p => p.Value, SampleDate));

        Assert.Equal(SampleIso, cut.Find("wa-date-picker").GetAttribute("value"));
    }

    [Fact]
    public void NullValue_RendersNoValueAttribute()
    {
        var cut = Render<WaDatePicker>(parameters => parameters.Add(p => p.Value, (DateOnly?)null));

        Assert.False(cut.Find("wa-date-picker").HasAttribute("value"));
    }

    [Fact]
    public void ChangeEvent_ParsesIsoValue_AndRaisesValueChanged()
    {
        // C# handler logic only: the change binder parses the element's ISO value and passes it to ValueChanged
        DateOnly? received = null;
        var cut = Render<WaDatePicker>(parameters => parameters
            .Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change(SampleIso);

        Assert.Equal(SampleDate, received);
        Assert.Equal(SampleDate, cut.Instance.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("24.07.2026")]
    [InlineData("2026-7-24")]
    [InlineData("2026-02-30")]
    public void ChangeEvent_WithEmptyOrInvalidValue_BindsNull(string wireValue)
    {
        DateOnly? received = SampleDate;
        var cut = Render<WaDatePicker>(parameters => parameters
            .Add(p => p.Value, SampleDate)
            .Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change(wireValue);

        Assert.Null(received);
    }

    [Fact]
    public void FocusedDate_RendersAsIsoAttribute()
    {
        var cut = Render<WaDatePicker>(parameters => parameters.Add(p => p.FocusedDate, SampleDate));

        Assert.Equal(SampleIso, cut.Find("wa-date-picker").GetAttribute("focused-date"));
    }

    [Fact]
    public async Task GoToDateAsync_PassesTheIsoDate()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaDatePicker>();

        await cut.InvokeAsync(() => cut.Instance.GoToDateAsync(SampleDate));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("goToDate", invocation.Arguments[1]);
        Assert.Equal(new object[] { SampleIso }, (object[])invocation.Arguments[2]!);
    }

    [Theory]
    [InlineData(typeof(WaDatePickerFocusDayEventArgs))]
    [InlineData(typeof(WaDatePickerViewChangeEventArgs))]
    public void EventArgsDate_DeserializesFromTheProjectedIsoString(Type argsType)
    {
        // the JS initializer projects the detail's Date to an ISO string (or null); Blazor deserializes custom event
        // args with the web defaults (camelCase, case-insensitive)
        var args = JsonSerializer.Deserialize($"{{\"date\":\"{SampleIso}\"}}", argsType, JsonSerializerOptions.Web);
        var empty = JsonSerializer.Deserialize("{\"date\":null}", argsType, JsonSerializerOptions.Web);

        Assert.Equal(SampleDate, argsType.GetProperty(nameof(WaDatePickerFocusDayEventArgs.Date))!.GetValue(args));
        Assert.Null(argsType.GetProperty(nameof(WaDatePickerFocusDayEventArgs.Date))!.GetValue(empty));
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

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string SampleIso = "2026-07-24";

    private static readonly DateOnly SampleDate = new(2026, 7, 24);

    #endregion
}
