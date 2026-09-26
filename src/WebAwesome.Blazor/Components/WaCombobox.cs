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
public class WaCombobox : WaPopupInputBase<string?>, IWaClearableControl, IWaAffixedControl
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    #endregion

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
    /// The preferred placement of the combobox's listbox, above or below the field. The actual placement may vary as needed to
    /// keep the listbox inside the viewport. When null, the attribute is omitted and Web Awesome's default (bottom) applies.
    /// </summary>
    [Parameter] public WaListboxPlacement? Placement { get; set; }

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

        // Add the form control attributes the element declares
        builder.AddAttribute(8, "required", Required);
        AddLabelAndHintAttributes(builder, 12);

        // Add combobox-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(21, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 23, this);
        builder.AddAttribute(24, "multiple", Multiple);
        builder.AddAttribute(25, "allow-custom-value", AllowCustomValue);
        builder.AddAttributeIfNotNull(26, "max-options-visible", MaxOptionsVisible);
        builder.AddAttributeIfNotNull(27, "placement", Placement?.ToHtmlValue());
        builder.AddAttribute(28, "open", Open);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttribute(33, "allow-create", AllowCreate);
        builder.AddAttributeIfNotNullOrEmpty(34, "autocapitalize", AutoCapitalize);
        builder.AddAttributeIfNotNullOrEmpty(35, "autocorrect", AutoCorrect);
        builder.AddAttributeIfNotNullOrEmpty(36, "enterkeyhint", EnterKeyHint);
        builder.AddAttributeIfNotNullOrEmpty(37, "inputmode", InputMode);
        builder.AddTrueFalseAttribute(38, "spellcheck", Spellcheck);

        // Add value binding - handle both single and multiple selection
        if (Multiple)
        {
            // multiple selection: no value attribute, which the element would read as one option value; the
            // selection is pushed into the live value property as an array instead (see LiveValuePropertyName)
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
        FormControlRendering.AddClearEventHandler(builder, 50, this);
        builder.AddAttributeIfHasDelegate(51, "onwa-create", OnCreate);
        AddPopupEventHandlers(builder, 52);
        builder.AddAttributeIfHasDelegate(56, "onwa-invalid", OnInvalid);

        // the keydown is relayed, because the element stops its propagation in the shadow root
        AddRelayedKeyDownHandler(builder, 57);

        // Add element reference capture
        builder.AddElementReferenceCapture(59, __comboboxReference => Element = __comboboxReference);

        // Add start and end slot content (the fragment wins over the icon-name shortcut)
        FormControlRendering.AddAffixSlots(builder, 60, this, StartIconName, EndIconName);

        // Add clear-icon slot content
        FormControlRendering.AddClearIconSlot(builder, 110, this);

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

    /// <summary>
    /// In multiple selection mode, the selection lives in the element's value property as a string array.
    /// </summary>
    protected override string? LiveValuePropertyName => Multiple ? MultipleValueProperty : null;

    /// <inheritdoc />
    internal override bool RelaysKeyDown => true;

    /// <summary>
    /// The selected values as the array the element's value property holds in multiple selection mode.
    /// </summary>
    /// <returns>The selected values</returns>
    protected override object? GetLiveValue() => Multiple ? liveSelectedValues : base.GetLiveValue();

    /// <summary>
    /// Keeps the array pushed to the element stable while <see cref="SelectedValues"/> keeps its content, so a
    /// re-render with an equal list does not reassign the element's selection.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var values = SelectedValues ?? [];
        if (!values.SequenceEqual(liveSelectedValues)) liveSelectedValues = values.ToArray();
    }

    /// <summary>
    /// Pushes the initial multiple selection too, since multiple selection mode renders no value attribute.
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && Multiple && liveSelectedValues.Length > 0 && Element is not null)
            await JSInterop.SyncPropertyAsync(Element.Value, MultipleValueProperty, liveSelectedValues);
    }

    #endregion

    #region ------ Internals ------

    private const string MultipleValueProperty = "value";

    // the selection last pushed to or received from the element in multiple selection mode
    private string[] liveSelectedValues = [];

    /// <summary>
    /// Handles change events for multiple selection mode.
    /// </summary>
    private async Task HandleMultipleSelectionChange(ChangeEventArgs args)
    {
        // the element reports its selection as a string array; recorded as the live value before the model changes,
        // so the next render does not push it back
        var values = args.GetStringArrayValue();
        liveSelectedValues = values;
        MarkLiveValueSynced(values);

        SelectedValues = values;
        await SelectedValuesChanged.InvokeAsync(values);

        // Also update the single value for consistency (use first selected or null)
        var singleValue = values.FirstOrDefault();
        if (CurrentValueAsString != singleValue)
        {
            CurrentValueAsString = singleValue;
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

    #endregion
}
