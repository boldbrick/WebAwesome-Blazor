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
/// A select component that allows choosing items from a menu of predefined options.
/// Corresponds to the wa-select Web Awesome component.
/// </summary>
public class WaSelect : WaPopupInputBase<string?>, IWaClearableControl, IWaAffixedControl
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Placeholder text to show as a hint when the select is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The select's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Draws a pill-style select with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Adds a clear button when the select is not empty.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Allows more than one option to be selected.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// The maximum number of selected options to show when <see cref="Multiple"/> is true. Beyond this count, a "+n" indicator is shown. Set to 0 to remove the limit.
    /// </summary>
    [Parameter] public int? MaxOptionsVisible { get; set; }

    /// <summary>
    /// The preferred placement of the select's menu, above or below the field. The actual placement may vary as needed to keep
    /// the listbox inside the viewport. When null, the attribute is omitted and Web Awesome's default (bottom) applies.
    /// </summary>
    [Parameter] public WaListboxPlacement? Placement { get; set; }

    #endregion

    #region ------ Multiple Selection Support ------

    /// <summary>
    /// The selected values when Multiple is true. Use this for two-way binding in multiple selection mode.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? SelectedValues { get; set; }

    /// <summary>
    /// Callback for when SelectedValues changes in multiple selection mode.
    /// </summary>
    [Parameter] public EventCallback<IReadOnlyList<string>?> SelectedValuesChanged { get; set; }

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

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the select
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the select
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// The options to display in the select
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

    /// <summary>
    /// An icon to use in lieu of the default clear icon (see <see cref="WithClear"/>), rendered into the element's "clear-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

    /// <summary>
    /// The icon to show when the control is expanded and collapsed, rendered into the element's "expand-icon" slot; it rotates on open and close.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    #endregion

    #region ------ JavaScript Interop Properties ------

    /// <summary>
    /// Custom tag generation function for multiple selection mode.
    /// Note: This requires JavaScript interop to implement.
    /// </summary>
    /// <remarks>
    /// In the actual implementation, this would be a JavaScript function that gets called
    /// for each selected option to generate custom tag markup.
    /// </remarks>
    public Func<WaOption, int, string>? GetTag { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-select");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(8, "required", Required);
        AddLabelAndHintAttributes(builder, 12);

        // Add select-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(21, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 23, this);
        builder.AddAttribute(24, "multiple", Multiple);
        builder.AddAttributeIfNotNull(25, "max-options-visible", MaxOptionsVisible);
        builder.AddAttributeIfNotNull(26, "placement", Placement?.ToHtmlValue());
        builder.AddAttribute(27, "open", Open);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding - handle both single and multiple selection
        if (Multiple)
        {
            // multiple selection: no value attribute, which the element would read as one option value; the
            // selection is pushed into the live value property as an array instead (see LiveValuePropertyName)
            builder.AddAttribute(31, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleMultipleSelectionChange));
        }
        else
        {
            // For single selection, use standard binding
            builder.AddAttribute(30, "value", CurrentValueAsString);
            builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        }

        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add select-specific event handlers
        FormControlRendering.AddClearEventHandler(builder, 50, this);
        AddPopupEventHandlers(builder, 52);
        builder.AddAttributeIfHasDelegate(56, "onwa-invalid", OnInvalid);

        // the keydown is relayed, because the element stops its propagation in the shadow root
        AddRelayedKeyDownHandler(builder, 57);

        // Add element reference capture
        builder.AddElementReferenceCapture(59, __selectReference => Element = __selectReference);

        // Add start and end slot content (the fragment wins over the icon-name shortcut)
        FormControlRendering.AddAffixSlots(builder, 60, this, StartIconName, EndIconName);

        // Add child content (options)
        if (ChildContent is not null)
        {
            builder.AddContent(70, ChildContent);
        }

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

    #region ------ Private Methods ------

    /// <summary>
    /// Handles change events for multiple selection mode
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

    private const string MultipleValueProperty = "value";

    // the selection last pushed to or received from the element in multiple selection mode
    private string[] liveSelectedValues = [];

    #endregion

    #region ------ Public Methods ------


    /// <summary>
    /// Sets the custom tag generation function for multiple selection mode.
    /// </summary>
    /// <param name="jsFunction">JavaScript function string that generates custom HTML for each selected option</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when jsFunction is null or empty</exception>
    public async Task SetGetTagFunctionAsync(string jsFunction)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set get tag function: component has not been rendered yet.");

        if (string.IsNullOrEmpty(jsFunction))
            throw new ArgumentNullException(nameof(jsFunction));

        await JSInterop.SetPropertyAsync(Element.Value, "getTag", jsFunction);
    }

    /// <summary>
    /// Removes focus from the select.
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
    /// Sets focus on the select.
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
