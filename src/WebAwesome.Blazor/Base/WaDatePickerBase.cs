using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class of the wrappers of wa-date-picker (<see cref="WaDatePicker"/> for a single date,
/// <see cref="WaDateRangePicker"/> for a date range): declares the
/// element's parameters, content, events and methods once and renders the element, including the calendar options
/// (<see cref="IWaCalendarOptions"/>) through the renderer wa-date-input shares. Each wrapper declares the value
/// conversion of its value type and the selection-mode attributes that go with it.
/// </summary>
/// <remarks>
/// wa-date-picker is not form-associated (no name, no validity, no wa-invalid), so, unlike the date input, this
/// base is a plain <see cref="ComponentBase"/> with a <c>Value</c>/<c>ValueChanged</c> pair, not an
/// <see cref="Microsoft.AspNetCore.Components.Forms.InputBase{TValue}"/>. What it shares with the date input are the
/// calendar options, declared through <see cref="IWaCalendarOptions"/> and rendered by one static renderer, so the
/// two element wrappers need no common base class.
/// </remarks>
/// <typeparam name="TValue">The type of value bound to the date picker</typeparam>
public abstract class WaDatePickerBase<TValue> : ComponentBase, IWaCalendarOptions
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The selected value; bind it with <c>@bind-Value</c>.
    /// </summary>
    [Parameter] public TValue? Value { get; set; }

    /// <summary>
    /// Invoked when the committed value changes, enabling <c>@bind-Value</c>.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// The current view.
    /// </summary>
    [Parameter] public WaDatePickerView? View { get; set; }

    /// <summary>
    /// The earliest selectable date; null leaves the calendar unbounded.
    /// </summary>
    [Parameter] public DateOnly? Min { get; set; }

    /// <summary>
    /// The latest selectable date; null leaves the calendar unbounded.
    /// </summary>
    [Parameter] public DateOnly? Max { get; set; }

    /// <summary>
    /// Disable all dates strictly before <see cref="Today"/>.
    /// </summary>
    [Parameter] public bool DisablePast { get; set; }

    /// <summary>
    /// Disable all dates strictly after <see cref="Today"/>.
    /// </summary>
    [Parameter] public bool DisableFuture { get; set; }

    /// <summary>
    /// Disables the entire picker.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Displays the current value without allowing changes. Cells remain focusable.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Dates that cannot be selected; null or an empty set disables none. Rendered as a space-separated list of ISO
    /// dates in ascending order.
    /// </summary>
    [Parameter] public IReadOnlySet<DateOnly>? DisabledDates { get; set; }

    /// <summary>
    /// Days of the week that cannot be selected (e.g. Saturday and Sunday); null or an empty set disables none.
    /// </summary>
    [Parameter] public IReadOnlySet<DayOfWeek>? DisabledDaysOfWeek { get; set; }

    /// <summary>
    /// The first day of the week.
    /// </summary>
    [Parameter] public WaFirstDayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The date the calendar focuses initially; drives the roving tabindex and the visible month. The element moves
    /// its focus as the user navigates without updating this parameter (see <see cref="OnFocusDay"/>).
    /// </summary>
    [Parameter] public DateOnly? FocusedDate { get; set; }

    /// <summary>
    /// BCP-47 locale override. When empty, the inherited <c>lang</c> attribute is used.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// Number of months rendered side-by-side. Either 1 or 2.
    /// </summary>
    [Parameter] public int? Months { get; set; }

    /// <summary>
    /// Whether prev/next advances by the visible range or one month at a time.
    /// </summary>
    [Parameter] public WaDatePageBy? PageBy { get; set; }

    /// <summary>
    /// Visual size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// Overrides the date considered "today"; null uses the browser's current date.
    /// </summary>
    [Parameter] public DateOnly? Today { get; set; }

    /// <summary>
    /// The weekday header format.
    /// </summary>
    [Parameter] public WaWeekdayFormat? WeekdayFormat { get; set; }

    /// <summary>
    /// Shows leading and trailing days from adjacent months.
    /// </summary>
    [Parameter] public bool WithOutsideDays { get; set; }

    /// <summary>
    /// Shows an ISO week-number column.
    /// </summary>
    [Parameter] public bool WithWeekNumbers { get; set; }

    /// <summary>
    /// Optional content rendered below the calendar grid.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Replaces the entire header row including title and navigation buttons. Advanced use only.
    /// </summary>
    [Parameter] public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Icon shown inside the previous-page button. Defaults to a left chevron.
    /// </summary>
    [Parameter] public RenderFragment? PreviousIconContent { get; set; }

    /// <summary>
    /// Icon shown inside the next-page button. Defaults to a right chevron.
    /// </summary>
    [Parameter] public RenderFragment? NextIconContent { get; set; }

    /// <summary>
    /// Invoked when the value changes during interaction. In range mode, this fires after the first click of a new
    /// range. The event's value is the element's wire string (ISO <c>YYYY-MM-DD</c>, or <c>from/to</c> for a range).
    /// </summary>
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }

    /// <summary>
    /// Invoked when the focused day changes via keyboard navigation, paging, or pointer hover.
    /// </summary>
    [Parameter] public EventCallback<WaDatePickerFocusDayEventArgs> OnFocusDay { get; set; }

    /// <summary>
    /// Invoked when the date picker switches between day, month, and year views.
    /// </summary>
    [Parameter] public EventCallback<WaDatePickerViewChangeEventArgs> OnViewChange { get; set; }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ClearAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot clear the date picker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "clear");
    }

    /// <summary>
    /// Focuses the calendar at the currently focused day.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the date picker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Scrolls the view to show the given date and sets the focused day.
    /// </summary>
    /// <param name="date">The target date</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task GoToDateAsync(DateOnly date)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate the date picker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "goToDate", WaWireFormat.FormatDate(date));
    }

    /// <summary>
    /// Navigates the view to today.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task GoToTodayAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate the date picker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "goToToday");
    }

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-date-picker");

        // add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", Class);
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // add date-picker-specific attributes: the selection mode of the wrapper, then the calendar options
        AddSelectionModeAttributes(builder, 10);
        builder.AddAttributeIfNotNull(13, "view", View?.ToHtmlValue());
        FormControlRendering.AddCalendarAttributes(builder, 14, this);
        builder.AddAttribute(27, "disabled", Disabled);
        builder.AddAttribute(28, "readonly", Readonly);
        builder.AddDateAttribute(29, "focused-date", FocusedDate);
        builder.AddAttributeIfNotNullOrEmpty(30, "locale", Locale);
        builder.AddAttributeIfNotNull(31, "size", Size?.ToHtmlValue());

        // add value binding (the native change event drives ValueChanged); the value attribute is the live value
        var wireValue = FormatValue(Value);
        builder.AddAttributeIfNotNullOrEmpty(35, "value", wireValue);
        builder.AddAttribute(36, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => SetValueAsync(ParseValue(__value)), wireValue));
        builder.SetUpdatesAttributeName("value");

        // add event handlers
        builder.AddAttributeIfHasDelegate(40, "oninput", OnInput);
        builder.AddAttributeIfHasDelegate(41, "onwa-focus-day", OnFocusDay);
        builder.AddAttributeIfHasDelegate(42, "onwa-view-change", OnViewChange);

        // add element reference capture
        builder.AddElementReferenceCapture(50, __datePickerReference => Element = __datePickerReference);

        // add slot content; 80 onwards stays free for the day slots
        builder.AddSlotContent(60, "header", HeaderContent);
        builder.AddSlotContent(65, "footer", FooterContent);
        builder.AddSlotContent(70, "previous-icon", PreviousIconContent);
        builder.AddSlotContent(75, "next-icon", NextIconContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Internals ------


    /// <summary>
    /// Formats a value as the element's wire string.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The wire string, or null or empty for no value</returns>
    private protected abstract string? FormatValue(TValue? value);

    /// <summary>
    /// Parses the element's wire string; an empty or invalid string gives the empty value.
    /// </summary>
    /// <param name="value">The wire string</param>
    /// <returns>The value</returns>
    private protected abstract TValue ParseValue(string? value);

    /// <summary>
    /// Adds the attributes of the wrapper's selection mode ("mode", "min-range", "max-range") at sequence + 0..2;
    /// a single-date wrapper adds none.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    private protected virtual void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
    }

    // updates the bound value and notifies the parent through ValueChanged
    private async Task SetValueAsync(TValue value)
    {
        if (EqualityComparer<TValue?>.Default.Equals(Value, value)) return;

        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    #endregion
}
