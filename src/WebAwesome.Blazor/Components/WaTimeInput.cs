using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental time picker with segmented text entry and a column-based popup.
/// Corresponds to the wa-time-input Web Awesome component.
/// </summary>
public class WaTimeInput : WaInputBase<string?>
{
    #region ------ Visual &amp; Behavior Properties ------

    /// <summary>
    /// The time picker's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Whether the UI uses a 12-hour or 24-hour clock. <see cref="WaTimeHourFormat.Auto"/> follows the resolved locale.
    /// </summary>
    [Parameter] public WaTimeHourFormat? HourFormat { get; set; }

    /// <summary>
    /// The earliest selectable time in wire format. May be later than <see cref="Max"/> to represent an overnight range.
    /// </summary>
    [Parameter] public string? Min { get; set; }

    /// <summary>
    /// The latest selectable time in wire format.
    /// </summary>
    [Parameter] public string? Max { get; set; }

    /// <summary>
    /// The granularity, in seconds, matching HTML <c>&lt;input type="time"&gt;</c>. The default <c>60</c> hides the
    /// seconds segment; values below 60 reveal it; <c>"any"</c> disables step-mismatch enforcement.
    /// </summary>
    [Parameter] public string? Step { get; set; }

    /// <summary>
    /// The preferred popup placement.
    /// </summary>
    [Parameter] public WaPlacement? Placement { get; set; }

    /// <summary>
    /// Distance in pixels between the popup and the input.
    /// </summary>
    [Parameter] public int? Distance { get; set; }

    /// <summary>
    /// Whether the popup is open.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Draws a pill-style time picker with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Shows a clear button when the time picker has a value.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Renders a "Now" button in the popup footer.
    /// </summary>
    [Parameter] public bool WithNow { get; set; }

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
    /// The icon to show on the popup toggle button. Defaults to a clock icon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// Content shown below the column picker in the popup. Replaces the default Now button when present.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-time-input");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add time-input-specific attributes
        builder.AddAttributeIfNotNull(20, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(21, "hour-format", HourFormat?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(22, "min", Min);
        builder.AddAttributeIfNotNullOrEmpty(23, "max", Max);
        builder.AddAttributeIfNotNullOrEmpty(24, "step", Step);
        builder.AddAttributeIfNotNull(25, "placement", Placement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(26, "distance", Distance);
        builder.AddAttribute(27, "open", Open);
        builder.AddAttribute(28, "pill", Pill);
        builder.AddAttribute(29, "with-clear", WithClear);
        builder.AddAttribute(30, "with-now", WithNow);
        builder.AddAttribute(31, "with-hint", WithHint);
        builder.AddAttribute(32, "with-label", WithLabel);

        // Add value binding
        builder.AddAttribute(35, "value", CurrentValueAsString);
        builder.AddAttribute(36, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add time-input-specific event handlers
        builder.AddAttributeIfHasDelegate(50, "onwa-clear", OnClear);
        builder.AddAttributeIfHasDelegate(51, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(52, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(53, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(54, "onwa-after-hide", OnAfterHide);
        builder.AddAttributeIfHasDelegate(55, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(56, __timeInputReference => Element = __timeInputReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "start");
            builder.AddContent(62, StartContent);
            builder.CloseElement();
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "end");
            builder.AddContent(67, EndContent);
            builder.CloseElement();
        }

        // Add clear-icon slot content
        if (ClearIconContent is not null)
        {
            builder.OpenElement(70, "span");
            builder.AddAttribute(71, "slot", "clear-icon");
            builder.AddContent(72, ClearIconContent);
            builder.CloseElement();
        }

        // Add expand-icon slot content
        if (ExpandIconContent is not null)
        {
            builder.OpenElement(75, "span");
            builder.AddAttribute(76, "slot", "expand-icon");
            builder.AddContent(77, ExpandIconContent);
            builder.CloseElement();
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "footer");
            builder.AddContent(82, FooterContent);
            builder.CloseElement();
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 90);

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
            throw new InvalidOperationException("Cannot focus the time picker before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the time picker.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the time picker before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Opens the popup.
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
    /// Closes the popup.
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
