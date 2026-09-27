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
/// This is a Pro component. The element's JS-only <c>dataSource</c> callback property (an alternative asynchronous
/// data source) is not wrapped, since Blazor cannot supply a JS function property; use <see cref="Server"/>,
/// <see cref="OnOptionsRequest"/> and swapped <c>WaOption</c> children instead.
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
    /// The Web Awesome default of <see cref="Placeholder"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPlaceholder = "";

    /// <summary>
    /// Placeholder text to show as a hint when the combobox is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

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
    /// The Web Awesome default of <see cref="MaxOptionsVisible"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultMaxOptionsVisible = 3;

    /// <summary>
    /// The maximum number of selected options to show when <see cref="Multiple"/> is true. Beyond this count, a "+n" indicator is shown. Set to 0 to remove the limit.
    /// </summary>
    [Parameter] public int? MaxOptionsVisible { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Placement"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaListboxPlacement DefaultPlacement = WaListboxPlacement.Bottom;

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
    [Parameter] public WaAutoCapitalize? AutoCapitalize { get; set; }

    /// <summary>
    /// Turns the browser's autocorrect feature on (true, rendered "on") or off (false, rendered "off"); null leaves
    /// the element's default, which is off.
    /// </summary>
    [Parameter] public bool? AutoCorrect { get; set; }

    /// <summary>
    /// Used to customize the label or icon of the Enter key on virtual keyboards.
    /// </summary>
    [Parameter] public WaEnterKeyHint? EnterKeyHint { get; set; }

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public WaInputMode? InputMode { get; set; }

    /// <summary>
    /// Enables spell checking on the combobox.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    /// <summary>
    /// Enables server (event) mode: instead of filtering the slotted options locally, the combobox fires
    /// <see cref="OnOptionsRequest"/> as the user types and expects the consumer to swap in matching
    /// <c>WaOption</c> children. Not a CEM-invented state: see <see cref="Loading"/> and
    /// <see cref="OnOptionsRequest"/> for how the loading state is cleared.
    /// </summary>
    [Parameter] public bool Server { get; set; }

    /// <summary>
    /// Shows the combobox's loading indicator (and the "loading" status slot). The element itself sets this to
    /// true when it schedules a request in server mode; the wrapper clears it back to false once the
    /// <see cref="OnOptionsRequest"/> handler for the latest request has completed and the resulting render has
    /// reached the DOM, unless this parameter is true. A handler that fires and forgets its fetch (rather than
    /// awaiting it) should set this parameter itself instead of relying on the automatic clear.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="FilterDebounce"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultFilterDebounce = 250;

    /// <summary>
    /// How long to wait, in milliseconds, after the user stops typing before filtering locally or firing
    /// <see cref="OnOptionsRequest"/> in <see cref="Server"/> mode.
    /// </summary>
    [Parameter] public int? FilterDebounce { get; set; }

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

    /// <summary>
    /// Invoked when the user selects the "create" option (requires <see cref="AllowCreate"/>). The event detail
    /// carries the typed input value.
    /// </summary>
    [Parameter] public EventCallback<WaCreateEventArgs> OnCreate { get; set; }

    /// <summary>
    /// Invoked in <see cref="Server"/> mode when the combobox needs matching options for the typed query. Handle
    /// it by swapping in the matching <c>WaOption</c> children. The wrapper clears the element's own loading
    /// state once the handler for the latest request has completed and re-rendered, unless <see cref="Loading"/>
    /// is true (see its remarks); the event detail's AbortSignal is not transferable and is dropped. The
    /// alternative, JS-only <c>dataSource</c> callback property is not wrapped; use this event instead.
    /// </summary>
    [Parameter] public EventCallback<WaOptionsRequestEventArgs> OnOptionsRequest { get; set; }

    /// <summary>
    /// Invoked when a request of the element's <c>dataSource</c> callback rejects, with the error message and the
    /// query of the failed request. The <c>dataSource</c> callback is a JavaScript function property that no
    /// parameter sets, so this fires only when the consumer's own JavaScript assigns one to the element; in the
    /// Blazor server mode (<see cref="Server"/> with <see cref="OnOptionsRequest"/>) there is no request to reject.
    /// </summary>
    [Parameter] public EventCallback<WaOptionsErrorEventArgs> OnOptionsError { get; set; }

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

    /// <summary>
    /// Content shown in the listbox when there are no options at all, replacing the element's localized default
    /// text.
    /// </summary>
    [Parameter] public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Content shown in the listbox after a failed <c>dataSource</c> request. Since the wrapper does not expose
    /// <c>dataSource</c> (a JS-only callback property), this slot only ever shows when a consumer sets it up
    /// through direct JS interop; use <see cref="Server"/> and <see cref="OnOptionsRequest"/> instead.
    /// </summary>
    [Parameter] public RenderFragment? ErrorContent { get; set; }

    /// <summary>
    /// Content shown in the listbox while <see cref="Loading"/> is true, replacing the element's localized
    /// default text.
    /// </summary>
    [Parameter] public RenderFragment? LoadingContent { get; set; }

    /// <summary>
    /// Content shown in the listbox when filtering yields no matching options, replacing the element's localized
    /// default text.
    /// </summary>
    [Parameter] public RenderFragment? NoResultsContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-combobox");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(8, "required", Required);
        AddLabelAndHintAttributes(builder, 12);

        // Add combobox-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 20, "placeholder", Placeholder, DefaultPlaceholder);
        builder.AddAttributeIfNotNull(attributes, 21, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 23, this);
        builder.AddAttribute(24, "multiple", Multiple);
        builder.AddAttribute(25, "allow-custom-value", AllowCustomValue);
        builder.AddAttributeIfNotNull(attributes, 26, "max-options-visible", MaxOptionsVisible, DefaultMaxOptionsVisible);
        builder.AddAttributeIfNotNull(attributes, 27, "placement", Placement?.ToHtmlValue(), DefaultPlacement.ToHtmlValue());
        builder.AddAttribute(28, "open", Open);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttribute(33, "allow-create", AllowCreate);
        builder.AddAttributeIfNotNull(34, "autocapitalize", AutoCapitalize?.ToHtmlValue());
        builder.AddOnOffAttribute(35, "autocorrect", AutoCorrect);
        builder.AddAttributeIfNotNull(36, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(37, "inputmode", InputMode?.ToHtmlValue());
        builder.AddTrueFalseAttribute(38, "spellcheck", Spellcheck);
        builder.AddAttribute(120, "server", Server);
        builder.AddAttribute(121, "loading", Loading);
        builder.AddAttributeIfNotNull(attributes, 122, "filter-debounce", FilterDebounce, DefaultFilterDebounce);

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

        // the wrapper handles wa-options-request itself (to drive the loading-clear logic in
        // OnAfterRenderAsync) and only relays it to the consumer's OnOptionsRequest from there
        if (OnOptionsRequest.HasDelegate)
            builder.AddAttribute(125, "onwa-options-request", EventCallback.Factory.Create<WaOptionsRequestEventArgs>(this, HandleOptionsRequestAsync));

        builder.AddOwnEventStopPropagation(125, "onwa-options-request");
        builder.AddAttributeIfHasDelegate(126, "onwa-options-error", OnOptionsError);

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

        // Add status slots (each replaces the element's localized default text)
        builder.AddSlotContent(130, "empty", EmptyContent);
        builder.AddSlotContent(133, "error", ErrorContent);
        builder.AddSlotContent(136, "loading", LoadingContent);
        builder.AddSlotContent(139, "no-results", NoResultsContent);

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
    /// Pushes the initial multiple selection too, since multiple selection mode renders no value attribute; also
    /// clears the element's own "loading" property once the <see cref="OnOptionsRequest"/> handler for the
    /// latest request has completed and this render has reached the DOM (see <see cref="HandleOptionsRequestAsync"/>).
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && Multiple && liveSelectedValues.Length > 0 && Element is not null)
            await JSInterop.SyncPropertyAsync(Element.Value, MultipleValueProperty, liveSelectedValues);

        if (pendingLoadingClear && Element is not null)
        {
            pendingLoadingClear = false;
            await JSInterop.SetPropertyAsync(Element.Value, "loading", false);
        }
    }

    #endregion

    #region ------ Internals ------

    private const string MultipleValueProperty = "value";

    // the selection last pushed to or received from the element in multiple selection mode
    private string[] liveSelectedValues = [];

    // identifies the latest wa-options-request, so a request superseded before its handler completed does not
    // clear the loading state the newer request needs
    private int latestOptionsRequestId;

    // set once the OnOptionsRequest handler for the latest request has completed and Loading was false at that
    // point; consumed by OnAfterRenderAsync once the resulting render has reached the DOM
    private bool pendingLoadingClear;

    /// <summary>
    /// Handles the element's own wa-options-request (server mode): forwards it to <see cref="OnOptionsRequest"/>
    /// and, once that handler completes, arranges for the element's "loading" property to be cleared in the next
    /// <see cref="OnAfterRenderAsync"/> unless a newer request arrived meanwhile or <see cref="Loading"/> is true.
    /// </summary>
    private async Task HandleOptionsRequestAsync(WaOptionsRequestEventArgs args)
    {
        var requestId = ++latestOptionsRequestId;

        await OnOptionsRequest.InvokeAsync(args);

        if (requestId == latestOptionsRequestId && !Loading)
            pendingLoadingClear = true;
    }

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

    /// <summary>
    /// Re-runs the current filter, either locally or (in <see cref="Server"/> mode) by firing
    /// <see cref="OnOptionsRequest"/> again for the current query.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ReloadAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reload: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reload");
    }

    #endregion
}
