using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class of the wrappers of wa-date-input (<see cref="WaDateInput"/> for a single date): declares the element's
/// parameters, content and methods once and renders the element, including the calendar options it forwards to its
/// popup calendar (<see cref="IWaCalendarOptions"/>, rendered by the renderer the date picker shares). Each wrapper
/// declares the value conversion of its value type and the selection-mode attributes that go with it.
/// </summary>
/// <typeparam name="TValue">The type of value bound to the date input</typeparam>
public abstract class WaDateInputBase<TValue> : WaPopupInputBase<TValue>, IWaClearableControl, IWaAffixedControl, IWaCalendarOptions
{
    /// <summary>
    /// Makes the input read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    /// <summary>
    /// The date input's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Earliest selectable date; null leaves the calendar unbounded. A committed value before <see cref="Min"/> fails
    /// constraint validation with <c>rangeUnderflow</c>.
    /// </summary>
    [Parameter] public DateOnly? Min { get; set; }

    /// <summary>
    /// Latest selectable date; null leaves the calendar unbounded. A committed value after <see cref="Max"/> fails
    /// constraint validation with <c>rangeOverflow</c>.
    /// </summary>
    [Parameter] public DateOnly? Max { get; set; }

    /// <summary>
    /// Disable all dates strictly before today.
    /// </summary>
    [Parameter] public bool DisablePast { get; set; }

    /// <summary>
    /// Disable all dates strictly after today.
    /// </summary>
    [Parameter] public bool DisableFuture { get; set; }

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
    /// The first day of the week in the popup calendar.
    /// </summary>
    [Parameter] public WaFirstDayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Number of months rendered in the popup calendar. Either 1 or 2.
    /// </summary>
    [Parameter] public int? Months { get; set; }

    /// <summary>
    /// Whether prev/next pages by the visible range or one month at a time.
    /// </summary>
    [Parameter] public WaDatePageBy? PageBy { get; set; }

    /// <summary>
    /// Weekday header format in the popup calendar.
    /// </summary>
    [Parameter] public WaWeekdayFormat? WeekdayFormat { get; set; }

    /// <summary>
    /// Overrides the date considered "today"; null uses the browser's current date.
    /// </summary>
    [Parameter] public DateOnly? Today { get; set; }

    /// <summary>
    /// The preferred placement of the date picker popup, above or below the field. When null, the attribute is omitted and
    /// Web Awesome's default (bottom-start) applies.
    /// </summary>
    [Parameter] public WaPickerPlacement? Placement { get; set; }

    /// <summary>
    /// Distance in pixels between the popup and the input.
    /// </summary>
    [Parameter] public int? Distance { get; set; }

    /// <summary>
    /// Draws a pill-style date input with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Shows a clear button when the date input has a value.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Show leading/trailing days from adjacent months in the popup calendar.
    /// </summary>
    [Parameter] public bool WithOutsideDays { get; set; }

    /// <summary>
    /// Show ISO 8601 week numbers in the popup calendar.
    /// </summary>
    [Parameter] public bool WithWeekNumbers { get; set; }

    /// <summary>
    /// Invoked when the clear button is activated.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    /// <summary>
    /// Content placed at the start of the input.
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content placed at the end of the input.
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default clear icon.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

    /// <summary>
    /// The icon to show on the date picker toggle button. Defaults to a calendar icon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// Content shown below the date picker inside the popup.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Icon for the date picker's previous-page button.
    /// </summary>
    [Parameter] public RenderFragment? PreviousIconContent { get; set; }

    /// <summary>
    /// Icon for the date picker's next-page button.
    /// </summary>
    [Parameter] public RenderFragment? NextIconContent { get; set; }

    /// <summary>
    /// Sets focus on the first empty (else first) segment.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the date input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the date input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the date input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Clears the current value. No-op when already empty or when disabled/readonly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ClearAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot clear the date input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "clear");
    }

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-date-input");

        // add common attributes
        AddCommonAttributes(builder, 1);

        // add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // add date-input-specific attributes: the selection mode of the wrapper, then the calendar options
        builder.AddAttributeIfNotNull(20, "appearance", Appearance?.ToHtmlValue());
        AddSelectionModeAttributes(builder, 21);
        FormControlRendering.AddCalendarAttributes(builder, 24, this);
        builder.AddAttributeIfNotNull(40, "placement", Placement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(41, "distance", Distance);
        builder.AddAttribute(42, "open", Open);
        builder.AddAttribute(43, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 44, this);
        AddWithHintAndLabelAttributes(builder, 14);

        // add value binding
        builder.AddAttribute(45, "value", CurrentValueAsString);
        builder.AddAttribute(46, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // add common event handlers
        AddCommonEventHandlers(builder, 50);

        // add date-input-specific event handlers
        FormControlRendering.AddClearEventHandler(builder, 60, this);
        AddPopupEventHandlers(builder, 61);
        builder.AddAttributeIfHasDelegate(65, "onwa-invalid", OnInvalid);

        // add element reference capture
        builder.AddElementReferenceCapture(66, __dateInputReference => Element = __dateInputReference);

        // add start and end slot content, then the clear-icon slot content
        FormControlRendering.AddAffixSlots(builder, 70, this);
        FormControlRendering.AddClearIconSlot(builder, 80, this);

        // add the popup's slot content
        builder.AddSlotContent(85, "expand-icon", ExpandIconContent);
        builder.AddSlotContent(90, "footer", FooterContent);
        builder.AddSlotContent(95, "previous-icon", PreviousIconContent);
        builder.AddSlotContent(100, "next-icon", NextIconContent);

        // add label and hint slots; 120 onwards stays free for the day slots
        AddLabelAndHintSlots(builder, 110);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Adds the attributes of the wrapper's selection mode ("mode", "min-range", "max-range") at sequence + 0..2;
    /// a single-date wrapper adds none.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    private protected virtual void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
    }

    #endregion
}
