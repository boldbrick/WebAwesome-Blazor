using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental standalone calendar for selecting a date or date range. Unlike <see cref="WaDateInput"/>
/// it is not a form-associated control; bind its value with <c>@bind-Value</c>.
/// Corresponds to the wa-date-picker Web Awesome component.
/// </summary>
public class WaDatePicker : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

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

    // Date picker properties
    /// <summary>
    /// The selected date(s). For single mode, an ISO date string (<c>YYYY-MM-DD</c>) or empty. For range mode,
    /// two ISO dates separated by <c>/</c>.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Invoked when the committed value changes, enabling <c>@bind-Value</c>.
    /// </summary>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// The selection mode.
    /// </summary>
    [Parameter] public WaDateSelectionMode? Mode { get; set; }

    /// <summary>
    /// The current view.
    /// </summary>
    [Parameter] public WaDatePickerView? View { get; set; }

    /// <summary>
    /// The earliest selectable date as <c>YYYY-MM-DD</c>.
    /// </summary>
    [Parameter] public string? Min { get; set; }

    /// <summary>
    /// The latest selectable date as <c>YYYY-MM-DD</c>.
    /// </summary>
    [Parameter] public string? Max { get; set; }

    /// <summary>
    /// Minimum range length in days (range mode only). <c>0</c> disables the check.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// Maximum range length in days (range mode only). <c>0</c> disables the check.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

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
    /// A whitespace-separated list of ISO dates that should be disabled.
    /// </summary>
    [Parameter] public string? DisabledDates { get; set; }

    /// <summary>
    /// Weekdays to disable. Accepts a space-separated list of three-letter weekday names.
    /// </summary>
    [Parameter] public string? DisabledDaysOfWeek { get; set; }

    /// <summary>
    /// The first day of the week.
    /// </summary>
    [Parameter] public WaFirstDayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The currently focused date as <c>YYYY-MM-DD</c>. Drives roving tabindex and the visible month.
    /// </summary>
    [Parameter] public string? FocusedDate { get; set; }

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
    /// Overrides the date considered "today" as <c>YYYY-MM-DD</c>.
    /// </summary>
    [Parameter] public string? Today { get; set; }

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

    #endregion

    #region ------ Content ------

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

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the value changes during interaction. In range mode, this fires after the first click of a new range.
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

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-date-picker");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add date-picker-specific attributes
        builder.AddAttributeIfNotNull(10, "mode", Mode?.ToHtmlValue());
        builder.AddAttributeIfNotNull(11, "view", View?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(12, "min", Min);
        builder.AddAttributeIfNotNullOrEmpty(13, "max", Max);
        builder.AddAttributeIfNotNull(14, "min-range", MinRange);
        builder.AddAttributeIfNotNull(15, "max-range", MaxRange);
        builder.AddAttribute(16, "disable-past", DisablePast);
        builder.AddAttribute(17, "disable-future", DisableFuture);
        builder.AddAttribute(18, "disabled", Disabled);
        builder.AddAttribute(19, "readonly", Readonly);
        builder.AddAttributeIfNotNullOrEmpty(20, "disabled-dates", DisabledDates);
        builder.AddAttributeIfNotNullOrEmpty(21, "disabled-days-of-week", DisabledDaysOfWeek);
        builder.AddAttributeIfNotNull(22, "first-day-of-week", FirstDayOfWeek?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(23, "focused-date", FocusedDate);
        builder.AddAttributeIfNotNullOrEmpty(24, "locale", Locale);
        builder.AddAttributeIfNotNull(25, "months", Months);
        builder.AddAttributeIfNotNull(26, "page-by", PageBy?.ToHtmlValue());
        builder.AddAttributeIfNotNull(27, "size", Size?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(28, "today", Today);
        builder.AddAttributeIfNotNull(29, "weekday-format", WeekdayFormat?.ToHtmlValue());
        builder.AddAttribute(30, "with-outside-days", WithOutsideDays);
        builder.AddAttribute(31, "with-week-numbers", WithWeekNumbers);

        // Add value binding (the native change event drives ValueChanged)
        builder.AddAttributeIfNotNullOrEmpty(35, "value", Value);
        builder.AddAttribute(36, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => SetValueAsync(__value), Value));
        builder.SetUpdatesAttributeName("value");

        // Add event handlers
        builder.AddAttributeIfHasDelegate(40, "oninput", OnInput);
        builder.AddAttributeIfHasDelegate(41, "onwa-focus-day", OnFocusDay);
        builder.AddAttributeIfHasDelegate(42, "onwa-view-change", OnViewChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(50, __datePickerReference => Element = __datePickerReference);

        // Add header slot content
        if (HeaderContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "header");
            builder.AddContent(62, HeaderContent);
            builder.CloseElement();
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "footer");
            builder.AddContent(67, FooterContent);
            builder.CloseElement();
        }

        // Add previous-icon slot content
        if (PreviousIconContent is not null)
        {
            builder.OpenElement(70, "span");
            builder.AddAttribute(71, "slot", "previous-icon");
            builder.AddContent(72, PreviousIconContent);
            builder.CloseElement();
        }

        // Add next-icon slot content
        if (NextIconContent is not null)
        {
            builder.OpenElement(75, "span");
            builder.AddAttribute(76, "slot", "next-icon");
            builder.AddContent(77, NextIconContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

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
    /// <param name="date">The target date as an ISO date string (<c>YYYY-MM-DD</c>)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task GoToDateAsync(string date)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate the date picker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "goToDate", date);
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

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Updates the bound value and notifies the parent via <see cref="ValueChanged"/>.
    /// </summary>
    private async Task SetValueAsync(string? value)
    {
        if (Value == value) return;

        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
