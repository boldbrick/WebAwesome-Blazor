using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class of the wrappers of wa-date-input (<see cref="WaDateInput"/> for a single date, <see cref="WaDateRangeInput"/>
/// for a date range): declares the element's
/// parameters, content and methods once and renders the element, including the calendar options it forwards to its
/// popup calendar (<see cref="IWaCalendarOptions"/>, rendered by the renderer the date picker shares). Each wrapper
/// declares the value conversion of its value type and the selection-mode attributes that go with it.
/// </summary>
/// <remarks>
/// Custom day content goes in <see cref="ChildContent"/> as <see cref="WaDayContent"/> children, which the element
/// forwards to its popup calendar. Web Awesome's JS-only <c>dayContent</c> and <c>isDateDisabled</c> callbacks are
/// not supported: the calendar calls them for every rendered day cell, which would need a JS round-trip per cell. Use
/// <see cref="WaDayContent"/> for day content and <see cref="DisabledDates"/> (with <see cref="DisabledDaysOfWeek"/>,
/// <see cref="DisablePast"/>, <see cref="DisableFuture"/>, <see cref="Min"/> and <see cref="Max"/>) for disabled days.
/// </remarks>
/// <typeparam name="TValue">The type of value bound to the date input</typeparam>
public abstract class WaDateInputBase<TValue> : WaPopupInputBase<TValue>, IWaClearableControl, IWaAffixedControl, IWaCalendarOptions,
    IWaDayContentHost
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
    /// The Web Awesome default of <see cref="Autocomplete"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultAutocomplete = "";

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

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
    /// The Web Awesome default of <see cref="Placement"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaPickerPlacement DefaultPlacement = WaPickerPlacement.BottomStart;

    /// <summary>
    /// The preferred placement of the date picker popup, above or below the field. When null, the attribute is omitted and
    /// Web Awesome's default (bottom-start) applies.
    /// </summary>
    [Parameter] public WaPickerPlacement? Placement { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Distance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultDistance = 0;

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
    /// Custom day content for the popup calendar: <see cref="WaDayContent"/> children, each shown in the day cell of its
    /// date (the element's <c>day-YYYY-MM-DD</c> slots, which it forwards to the calendar). wa-date-input has no default
    /// slot, so any other content is not shown.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

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
        var attributes = builder.OpenWaElement(this, 0, "wa-date-input");

        // add common attributes
        AddCommonAttributes(builder, 1);

        // add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "autocomplete", Autocomplete, DefaultAutocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // add date-input-specific attributes: the selection mode of the wrapper, then the calendar options
        builder.AddAttributeIfNotNull(attributes, 20, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        AddSelectionModeAttributes(builder, 21);
        FormControlRendering.AddCalendarAttributes(builder, 24, this, IWaCalendarOptions.DefaultDisabledDates);
        builder.AddAttributeIfNotNull(attributes, 40, "placement", Placement?.ToHtmlValue(), DefaultPlacement.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 41, "distance", Distance, DefaultDistance);
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

        // add label and hint slots, then the day content with this input cascaded as its host
        AddLabelAndHintSlots(builder, 110);
        FormControlRendering.AddDayContent(builder, 120, this, ChildContent);

        builder.CloseElement();

        // a day content changing its slot from here on is forwarded after this render
        afterRenderPending = true;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    /// <summary>
    /// Syncs the live value (see the base class), then has the element forward its day slots to the popup calendar
    /// again when a <see cref="WaDayContent"/> was added, removed or moved to another date.
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        afterRenderPending = false;
        await base.OnAfterRenderAsync(firstRender);

        if (!daySlotsChanged || Element is null) return;

        daySlotsChanged = false;
        await JSInterop.SignalDefaultSlotChangeAsync(Element.Value);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        disposed = true;
        base.Dispose(disposing);
    }

    #endregion

    #region ------ Implementation of IWaDayContentHost ------

    /// <summary>
    /// Records that the day slots changed, to be forwarded after the current render (see
    /// <see cref="OnAfterRenderAsync"/>), or after a render started here when the change did not come from one of this
    /// input's renders (e.g. a day content inside a child component that re-rendered on its own).
    /// </summary>
    void IWaDayContentHost.DayContentChanged()
    {
        if (disposed) return;

        daySlotsChanged = true;
        if (!afterRenderPending) StateHasChanged();
    }

    #endregion

    #region ------ Internals ------

    // wa-date-input forwards its day-YYYY-MM-DD children to the popup calendar only on its first update and on the
    // slotchange of its default slot (updateForwardedDaySlots, 3.12.0). A day-slotted child is assigned to no default
    // slot, so adding or removing one later forwards nothing: an added day's content never shows, and a removed day
    // shows an empty cell (its forwarded slot stays, without content or fallback). The interop call fires that
    // slotchange, so the element runs its own forwarding again
    private bool daySlotsChanged;

    // set by BuildRenderTree, cleared by OnAfterRenderAsync: a day slot change in between is forwarded by that
    // after-render without a render of its own
    private bool afterRenderPending;

    private bool disposed;

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
