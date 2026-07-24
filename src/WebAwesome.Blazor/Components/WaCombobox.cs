using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental combobox component that combines a text input with a list of selectable options.
/// Corresponds to the wa-combobox Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaCombobox : WaInputBase<string?>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Placeholder text to show as a hint when the combobox is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The combobox's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Draws a pill-style combobox with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Adds a clear button when the combobox is not empty.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Allows more than one option to be selected.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Allows the user to enter a custom value that is not present among the options.
    /// </summary>
    [Parameter] public bool AllowCustomValue { get; set; }

    /// <summary>
    /// The maximum number of selected options to show when <see cref="Multiple"/> is true. Beyond this count, a "+n" indicator is shown. Set to 0 to remove the limit.
    /// </summary>
    [Parameter] public int? MaxOptionsVisible { get; set; }

    /// <summary>
    /// The preferred placement of the combobox's listbox. The actual placement may vary as needed to keep the listbox inside the viewport.
    /// </summary>
    [Parameter] public WaPlacement? Placement { get; set; }

    /// <summary>
    /// Indicates whether the combobox's listbox is open.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Reserves space for the hint even when it is not populated.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Reserves space for the label even when it is not populated.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    /// <summary>
    /// When true, if the user types text that does not match any existing option, a "Create [value]" option
    /// appears in the listbox. Selecting it creates a new option in the DOM and selects it; a cancelable
    /// <see cref="OnCreate"/> event fires before creation.
    /// </summary>
    [Parameter] public bool AllowCreate { get; set; }

    /// <summary>
    /// Controls whether and how text input is automatically capitalized as it is entered by the user.
    /// </summary>
    [Parameter] public string? AutoCapitalize { get; set; }

    /// <summary>
    /// Indicates whether the browser's autocorrect feature is on or off. As an attribute, use "off" or "on".
    /// </summary>
    [Parameter] public string? AutoCorrect { get; set; }

    /// <summary>
    /// Used to customize the label or icon of the Enter key on virtual keyboards.
    /// </summary>
    [Parameter] public string? EnterKeyHint { get; set; }

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public string? InputMode { get; set; }

    /// <summary>
    /// Enables spell checking on the combobox.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    #endregion

    #region ------ Multiple Selection Support ------

    /// <summary>
    /// The selected values when Multiple is true. Use this for two-way binding in multiple selection mode.
    /// </summary>
    [Parameter] public string[]? SelectedValues { get; set; }

    /// <summary>
    /// Callback for when SelectedValues changes in multiple selection mode.
    /// </summary>
    [Parameter] public EventCallback<string[]?> SelectedValuesChanged { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the control's value is cleared.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked when the combobox's listbox opens.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the combobox's listbox closes.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the combobox's listbox opens and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the combobox's listbox closes and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    /// <summary>
    /// Invoked when the user selects the "create" option (requires <see cref="AllowCreate"/>). The event detail
    /// carries the typed input value.
    /// </summary>
    [Parameter] public EventCallback<WaCreateEventArgs> OnCreate { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the combobox.
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the combobox.
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Custom content for the clear button icon.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

    /// <summary>
    /// Custom content for the expand/collapse icon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// The options to display in the combobox.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="StartContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? StartIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="EndContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? EndIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-combobox");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add combobox-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(21, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        builder.AddAttribute(23, "with-clear", WithClear);
        builder.AddAttribute(24, "multiple", Multiple);
        builder.AddAttribute(25, "allow-custom-value", AllowCustomValue);
        builder.AddAttributeIfNotNull(26, "max-options-visible", MaxOptionsVisible);
        builder.AddAttributeIfNotNull(27, "placement", Placement?.ToHtmlValue());
        builder.AddAttribute(28, "open", Open);
        builder.AddAttribute(29, "with-hint", WithHint);
        builder.AddAttribute(30, "with-label", WithLabel);
        builder.AddAttribute(33, "allow-create", AllowCreate);
        builder.AddAttributeIfNotNullOrEmpty(34, "autocapitalize", AutoCapitalize);
        builder.AddAttributeIfNotNullOrEmpty(35, "autocorrect", AutoCorrect);
        builder.AddAttributeIfNotNullOrEmpty(36, "enterkeyhint", EnterKeyHint);
        builder.AddAttributeIfNotNullOrEmpty(37, "inputmode", InputMode);
        builder.AddAttributeIfNotNull(38, "spellcheck", Spellcheck);

        // Add value binding - handle both single and multiple selection
        if (Multiple)
        {
            // For multiple selection, we need special handling
            var selectedValuesString = SelectedValues != null ? string.Join(",", SelectedValues) : string.Empty;
            builder.AddAttribute(31, "value", selectedValuesString);
            builder.AddAttribute(32, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleMultipleSelectionChange));
        }
        else
        {
            // For single selection, use standard binding
            builder.AddAttribute(31, "value", CurrentValueAsString);
            builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        }

        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add combobox-specific event handlers
        builder.AddAttributeIfHasDelegate(50, "onwa-clear", OnClear);
        builder.AddAttributeIfHasDelegate(51, "onwa-create", OnCreate);
        builder.AddAttributeIfHasDelegate(52, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(53, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(54, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(55, "onwa-after-hide", OnAfterHide);
        builder.AddAttributeIfHasDelegate(56, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(57, __comboboxReference => Element = __comboboxReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "start");
            builder.AddContent(62, StartContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(100, "start", StartIconName);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "end");
            builder.AddContent(67, EndContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(105, "end", EndIconName);
        }

        // Add clear-icon slot content
        if (ClearIconContent is not null)
        {
            builder.OpenElement(110, "span");
            builder.AddAttribute(111, "slot", "clear-icon");
            builder.AddContent(112, ClearIconContent);
            builder.CloseElement();
        }

        // Add expand-icon slot content
        if (ExpandIconContent is not null)
        {
            builder.OpenElement(115, "span");
            builder.AddAttribute(116, "slot", "expand-icon");
            builder.AddContent(117, ExpandIconContent);
            builder.CloseElement();
        }

        // Add child content (options)
        if (ChildContent is not null)
        {
            builder.AddContent(70, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 80);

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

    #region ------ Internals ------

    /// <summary>
    /// Handles change events for multiple selection mode.
    /// </summary>
    private async Task HandleMultipleSelectionChange(ChangeEventArgs args)
    {
        if (args.Value is string stringValue)
        {
            // Parse the comma-separated values
            var values = string.IsNullOrEmpty(stringValue)
                ? Array.Empty<string>()
                : stringValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            SelectedValues = values;
            await SelectedValuesChanged.InvokeAsync(values);

            // Also update the single value for consistency (use first selected or null)
            var singleValue = values.FirstOrDefault();
            if (CurrentValueAsString != singleValue)
            {
                CurrentValueAsString = singleValue;
            }
        }
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Removes focus from the combobox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Sets focus on the combobox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Hides the combobox's listbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Shows the combobox's listbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    #endregion
}
