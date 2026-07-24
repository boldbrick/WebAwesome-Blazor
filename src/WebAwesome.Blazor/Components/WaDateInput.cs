using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental date input with segmented text entry and a popup calendar.
/// Corresponds to the wa-date-input Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaDateInput : WaInputBase<string?>
{
    #region ------ Visual &amp; Behavior Properties ------

    /// <summary>
    /// The date input's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Selection mode.
    /// </summary>
    [Parameter] public WaDateSelectionMode Mode { get; set; } = WaDateSelectionMode.Single;

    /// <summary>
    /// Earliest selectable date as <c>YYYY-MM-DD</c>. A committed value before <see cref="Min"/> fails
    /// constraint validation with <c>rangeUnderflow</c>.
    /// </summary>
    [Parameter] public string? Min { get; set; }

    /// <summary>
    /// Latest selectable date as <c>YYYY-MM-DD</c>. A committed value after <see cref="Max"/> fails
    /// constraint validation with <c>rangeOverflow</c>.
    /// </summary>
    [Parameter] public string? Max { get; set; }

    /// <summary>
    /// Minimum range length in days (range mode only). <c>0</c> disables.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// Maximum range length in days (range mode only). <c>0</c> disables.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

    /// <summary>
    /// Disable all dates strictly before today.
    /// </summary>
    [Parameter] public bool DisablePast { get; set; }

    /// <summary>
    /// Disable all dates strictly after today.
    /// </summary>
    [Parameter] public bool DisableFuture { get; set; }

    /// <summary>
    /// Dates that cannot be selected. Accepts a whitespace-separated list of ISO dates.
    /// </summary>
    [Parameter] public string? DisabledDates { get; set; }

    /// <summary>
    /// Days of the week that cannot be selected. Accepts a space-separated list of three-letter weekday names.
    /// </summary>
    [Parameter] public string? DisabledDaysOfWeek { get; set; }

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
    /// Override "today" as <c>YYYY-MM-DD</c> (defaults to the runtime date).
    /// </summary>
    [Parameter] public string? Today { get; set; }

    /// <summary>
    /// The preferred popup placement.
    /// </summary>
    [Parameter] public WaPlacement? Placement { get; set; }

    /// <summary>
    /// Distance in pixels between the popup and the input.
    /// </summary>
    [Parameter] public int? Distance { get; set; }

    /// <summary>
    /// Whether the popup calendar is open.
    /// </summary>
    [Parameter] public bool Open { get; set; }

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
    /// Only required for SSR. Set to true if you're slotting in a hint element.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true if you're slotting in a label element.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the clear button is activated.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked when the popup is about to open. Cancelable on the underlying element.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the popup is about to close. Cancelable on the underlying element.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the popup opens and animations complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the popup closes and animations complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Slots ------

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

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-date-input");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add date-input-specific attributes
        builder.AddAttributeIfNotNull(20, "appearance", Appearance?.ToHtmlValue());
        if (Mode != WaDateSelectionMode.Single)
            builder.AddAttribute(21, "mode", Mode.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(22, "min", Min);
        builder.AddAttributeIfNotNullOrEmpty(23, "max", Max);
        builder.AddAttributeIfNotNull(24, "min-range", MinRange);
        builder.AddAttributeIfNotNull(25, "max-range", MaxRange);
        builder.AddAttribute(26, "disable-past", DisablePast);
        builder.AddAttribute(27, "disable-future", DisableFuture);
        builder.AddAttributeIfNotNullOrEmpty(28, "disabled-dates", DisabledDates);
        builder.AddAttributeIfNotNullOrEmpty(29, "disabled-days-of-week", DisabledDaysOfWeek);
        builder.AddAttributeIfNotNull(30, "first-day-of-week", FirstDayOfWeek?.ToHtmlValue());
        builder.AddAttributeIfNotNull(31, "months", Months);
        builder.AddAttributeIfNotNull(32, "page-by", PageBy?.ToHtmlValue());
        builder.AddAttributeIfNotNull(33, "weekday-format", WeekdayFormat?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(34, "today", Today);
        builder.AddAttributeIfNotNull(35, "placement", Placement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(36, "distance", Distance);
        builder.AddAttribute(37, "open", Open);
        builder.AddAttribute(38, "pill", Pill);
        builder.AddAttribute(39, "with-clear", WithClear);
        builder.AddAttribute(40, "with-outside-days", WithOutsideDays);
        builder.AddAttribute(41, "with-week-numbers", WithWeekNumbers);
        builder.AddAttribute(42, "with-hint", WithHint);
        builder.AddAttribute(43, "with-label", WithLabel);

        // Add value binding
        builder.AddAttribute(45, "value", CurrentValueAsString);
        builder.AddAttribute(46, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add date-input-specific event handlers
        builder.AddAttributeIfHasDelegate(60, "onwa-clear", OnClear);
        builder.AddAttributeIfHasDelegate(61, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(62, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(63, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(64, "onwa-after-hide", OnAfterHide);
        builder.AddAttributeIfHasDelegate(65, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(66, __dateInputReference => Element = __dateInputReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(70, "span");
            builder.AddAttribute(71, "slot", "start");
            builder.AddContent(72, StartContent);
            builder.CloseElement();
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(75, "span");
            builder.AddAttribute(76, "slot", "end");
            builder.AddContent(77, EndContent);
            builder.CloseElement();
        }

        // Add clear-icon slot content
        if (ClearIconContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "clear-icon");
            builder.AddContent(82, ClearIconContent);
            builder.CloseElement();
        }

        // Add expand-icon slot content
        if (ExpandIconContent is not null)
        {
            builder.OpenElement(85, "span");
            builder.AddAttribute(86, "slot", "expand-icon");
            builder.AddContent(87, ExpandIconContent);
            builder.CloseElement();
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(90, "span");
            builder.AddAttribute(91, "slot", "footer");
            builder.AddContent(92, FooterContent);
            builder.CloseElement();
        }

        // Add previous-icon slot content
        if (PreviousIconContent is not null)
        {
            builder.OpenElement(95, "span");
            builder.AddAttribute(96, "slot", "previous-icon");
            builder.AddContent(97, PreviousIconContent);
            builder.CloseElement();
        }

        // Add next-icon slot content
        if (NextIconContent is not null)
        {
            builder.OpenElement(100, "span");
            builder.AddAttribute(101, "slot", "next-icon");
            builder.AddContent(102, NextIconContent);
            builder.CloseElement();
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 110);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #endregion

    #region ------ Public Methods ------

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

    /// <summary>
    /// Opens the popup calendar.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show the popup before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Closes the popup calendar.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide the popup before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    #endregion
}
