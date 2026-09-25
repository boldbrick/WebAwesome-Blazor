using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class for Web Awesome input components that provides common functionality
/// for labels, hints, validation, and styling
/// </summary>
/// <typeparam name="TValue">The type of value bound to the input</typeparam>
public abstract class WaInputBase<TValue> : InputBase<TValue>, IFormValidation
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used by derived input components to call methods on the underlying Web Awesome element.
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

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names applied to the rendered element alongside validation state classes.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Visual & behavior properties
    /// <summary>
    /// Size variant of the input, mapped to the underlying Web Awesome element's "size" attribute.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// Disables the input, preventing user interaction and value submission.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Makes the input read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    // Labels & hints (string or RenderFragment)
    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Rich markup label rendered into the "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// Rich markup hint rendered into the "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    // Validation
    /// <summary>
    /// Minimum number of characters required for a valid value.
    /// </summary>
    [Parameter] public int? MinLength { get; set; }

    /// <summary>
    /// Maximum number of characters allowed for the value.
    /// </summary>
    [Parameter] public int? MaxLength { get; set; }

    // Browser behavior
    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    // Common events
    /// <summary>
    /// Invoked when the focus moves into the control, by keyboard or pointer. Bound to the bubbling, composed
    /// focusin event (so <see cref="FocusEventArgs.Type"/> is "focusin"), because the element that takes the
    /// focus usually lies in the control's shadow root, where Blazor never sees the non-bubbling focus event.
    /// Moving the focus between the parts of the control's own shadow root raises nothing; moving it between
    /// the control and content slotted into it (e.g. from one radio of a radio group to the next, or from a
    /// select's input into its option list) raises <see cref="OnBlur"/> followed by <see cref="OnFocus"/>.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the focus leaves the control. Bound to the bubbling, composed focusout event (so
    /// <see cref="FocusEventArgs.Type"/> is "focusout"); see <see cref="OnFocus"/>.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Invoked when a key is pressed down while the input is focused.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Invoked when a key is released while the input is focused.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Invoked when a character-producing key is pressed while the input is focused.
    /// </summary>
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }

    /// <summary>
    /// Invoked when the input's value changes as the user types.
    /// </summary>
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }

    #endregion

    #region ------ Protected Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes with validation state
    /// </summary>
    protected string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(' ', classes);
    }

    /// <summary>
    /// Adds common attributes to the render tree builder
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The starting sequence number</param>
    /// <returns>The next available sequence number</returns>
    protected int AddCommonAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddMultipleAttributes(sequence + 0, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 1, "name", NameAttributeValue);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(sequence + 3, "style", Style);
        builder.AddAttributeIfNotNull(sequence + 4, "size", Size?.ToHtmlValue());
        builder.AddAttribute(sequence + 5, "disabled", Disabled);
        builder.AddAttribute(sequence + 6, "readonly", Readonly);
        builder.AddAttribute(sequence + 7, "required", Required);
        builder.AddAttributeIfNotNull(sequence + 8, "minlength", MinLength);
        builder.AddAttributeIfNotNull(sequence + 9, "maxlength", MaxLength);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 10, "autocomplete", Autocomplete);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 11, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 12, "hint", Hint);

        return sequence + 13;
    }

    /// <summary>
    /// Adds common event handlers to the render tree builder
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number; uses sequence + 0..5</param>
    /// <returns>The next available sequence number</returns>
    protected int AddCommonEventHandlers(RenderTreeBuilder builder, int sequence)
        => AddCommonEventHandlers(builder, sequence, includeInputHandler: true);

    /// <summary>
    /// Adds common event handlers to the render tree builder, optionally leaving out the <see cref="OnInput"/>
    /// handler for a derived class that emits its own "oninput" handler (e.g. merged with the value binder, see
    /// <see cref="CreateImmediateInputHandler"/>) or binds <see cref="OnInput"/> to a different event
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number; uses sequence + 0..5</param>
    /// <param name="includeInputHandler">Whether to emit <see cref="OnInput"/> as the "oninput" handler</param>
    /// <returns>The next available sequence number</returns>
    protected int AddCommonEventHandlers(RenderTreeBuilder builder, int sequence, bool includeInputHandler)
    {
        builder.AddAttributeIfHasDelegate(sequence + 0, "onfocusin", OnFocus);
        builder.AddAttributeIfHasDelegate(sequence + 1, "onfocusout", OnBlur);

        // a wrapper whose element stops the keydown's propagation binds the relayed keydown instead
        if (!RelaysKeyDown)
            builder.AddAttributeIfHasDelegate(sequence + 2, "onkeydown", OnKeyDown);

        builder.AddAttributeIfHasDelegate(sequence + 3, "onkeyup", OnKeyUp);
        builder.AddAttributeIfHasDelegate(sequence + 4, "onkeypress", OnKeyPress);

        if (includeInputHandler)
            builder.AddAttributeIfHasDelegate(sequence + 5, "oninput", OnInput);

        return sequence + 6;
    }

    /// <summary>
    /// Records the value the element's live property currently holds (in the JS type returned by
    /// <see cref="GetLiveValue"/>), so the next render does not push it back to the element. Call it with the
    /// element's own value whenever a value arrives from the element (change/input binders, read-backs), before
    /// the model is assigned; a later C#-side change then differs from the recorded value and is pushed.
    /// </summary>
    /// <param name="value">The element's live value</param>
    protected void MarkLiveValueSynced(object? value)
    {
        lastSyncedLiveValue = value;
        hasSyncedLiveValue = true;
    }

    /// <summary>
    /// Assigns a string value received from the element to <see cref="InputBase{TValue}.CurrentValueAsString"/>
    /// (which parses it and notifies the edit context), recording it as the element's live value first; the
    /// setter for value binders created with <c>EventCallback.Factory.CreateBinder&lt;string?&gt;</c>
    /// </summary>
    /// <param name="value">The element's value</param>
    protected void SetCurrentValueAsStringFromElement(string? value)
    {
        MarkLiveValueSynced(value);
        CurrentValueAsString = value;
    }

    /// <summary>
    /// Creates the single "oninput" handler of an opt-in immediate binding: it updates the value from the input
    /// event (like the "onchange" binder does on commit) and then invokes <see cref="OnInput"/>. Emit it instead
    /// of the common "oninput" handler (see <see cref="AddCommonEventHandlers(RenderTreeBuilder, int, bool)"/>),
    /// because two attributes with the same name would clash.
    /// </summary>
    /// <returns>The merged input handler</returns>
    protected EventCallback<ChangeEventArgs> CreateImmediateInputHandler()
        => EventCallback.Factory.Create<ChangeEventArgs>(this, HandleImmediateInputAsync);

    /// <summary>
    /// Adds label and hint slots to the render tree if MarkupLabel or MarkupHint are provided
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The starting sequence number</param>
    /// <returns>The next available sequence number</returns>
    protected int AddLabelAndHintSlots(RenderTreeBuilder builder, int sequence)
    {
        var currentSequence = sequence;

        if (MarkupLabel is not null)
        {
            builder.OpenElement(currentSequence++, "span");
            builder.AddAttribute(currentSequence++, "slot", "label");
            builder.AddContent(currentSequence++, MarkupLabel);
            builder.CloseElement();
        }

        if (MarkupHint is not null)
        {
            builder.OpenElement(currentSequence++, "span");
            builder.AddAttribute(currentSequence++, "slot", "hint");
            builder.AddContent(currentSequence++, MarkupHint);
            builder.CloseElement();
        }

        return currentSequence;
    }

    #endregion

    #region ------ Implementation of IFormValidation ------

    /// <inheritdoc />
    public virtual async Task SetCustomValidityAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set custom validity before the component is rendered. Element reference is null.");

        await JSInterop.SetCustomValidityAsync(Element.Value, message);
    }

    /// <inheritdoc />
    public virtual async Task ResetValidityAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot reset validity before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "resetValidity");
    }

    #endregion

    #region ------ Overrides ------

    /// <summary>
    /// Pushes a C#-side value change into the element's live property (see <see cref="LiveValuePropertyName"/>).
    /// Derived classes overriding this method must call the base implementation.
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await SyncLiveValueAsync(firstRender);
    }

    #endregion

    #region ------ Internals ------

    // pushes the model into the live property when it changed since the value last known to the element; the
    // first render only records it, because the attribute already delivered the initial value
    private async Task SyncLiveValueAsync(bool firstRender)
    {
        var propertyName = LiveValuePropertyName;
        if (propertyName is null)
        {
            // re-baseline once syncing applies again (e.g. a slider switched out of range mode)
            hasSyncedLiveValue = false;
            return;
        }

        var liveValue = GetLiveValue();
        if (firstRender || !hasSyncedLiveValue)
        {
            MarkLiveValueSynced(liveValue);
            return;
        }

        if (Element is null || Equals(liveValue, lastSyncedLiveValue)) return;

        // recorded before the call, so a render completing meanwhile does not push the same value again
        MarkLiveValueSynced(liveValue);
        await JSInterop.SyncPropertyAsync(Element.Value, propertyName, liveValue);
    }

    // the merged immediate-binding input handler, see CreateImmediateInputHandler
    private async Task HandleImmediateInputAsync(ChangeEventArgs args)
    {
        SetCurrentValueAsStringFromElement(args.Value as string);

        // while typing, the wrapper's own formatting of an in-progress value (e.g. "1." parsed and formatted as
        // "1") must not be pushed back into the element; the change binder records the raw value on commit, so
        // such a normalization is pushed then, and a model change made by the parent differs and is pushed anyway
        MarkLiveValueSynced(GetLiveValue());

        await OnInput.InvokeAsync(args);
    }

    /// <summary>
    /// Binds <see cref="OnInput"/> to the numericinput alias of the input event, for number-valued elements whose
    /// input event Blazor's built-in reader cannot carry (see <see cref="Constants.NumericInputEventAttribute"/>);
    /// use together with <see cref="AddCommonEventHandlers(RenderTreeBuilder, int, bool)"/> leaving out the input
    /// handler. <see cref="OnInput"/> receives the value as a string, like with the built-in input event.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The sequence number for the handler attribute</param>
    internal void AddNumericInputHandler(RenderTreeBuilder builder, int sequence)
    {
        if (OnInput.HasDelegate)
            builder.AddAttribute(sequence, Constants.NumericInputEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleNumericInputAsync));
    }

    // hands OnInput the same string-valued ChangeEventArgs the built-in input event would
    private Task HandleNumericInputAsync(ChangeEventArgs args)
        => OnInput.InvokeAsync(new ChangeEventArgs { Value = args.GetStringValue() });

    /// <summary>
    /// Whether the element stops the propagation of the keydown in its shadow root, so that
    /// <see cref="AddCommonEventHandlers(RenderTreeBuilder, int, bool)"/> leaves out the "onkeydown" handler and the
    /// wrapper binds <see cref="OnKeyDown"/> with <see cref="AddRelayedKeyDownHandler"/> instead.
    /// </summary>
    internal virtual bool RelaysKeyDown => false;

    /// <summary>
    /// Binds <see cref="OnKeyDown"/> to the relayed keydown (see <see cref="Constants.RelayedKeyDownEventAttribute"/>),
    /// for a wrapper overriding <see cref="RelaysKeyDown"/>; uses sequence + 0..1.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    internal void AddRelayedKeyDownHandler(RenderTreeBuilder builder, int sequence)
        => builder.AddRelayedEventIfHasDelegate(sequence, Constants.RelayedKeyDownEventAttribute, OnKeyDown);

    // the live value last pushed to or received from the element; only meaningful when hasSyncedLiveValue is set
    private object? lastSyncedLiveValue;
    private bool hasSyncedLiveValue;

    #endregion

    #region ------ Interface for descendants ------

    /// <summary>
    /// Name of the element's live property that holds the current value, for elements whose value attribute only
    /// sets the default (Web Awesome maps value/checked to defaultValue/defaultChecked, and the element ignores the
    /// attribute once the user has interacted). When set, a C#-side value change is pushed into this property
    /// after rendering. Null (the default) means the attribute reaches the live property and no sync is needed.
    /// </summary>
    protected virtual string? LiveValuePropertyName => null;

    /// <summary>
    /// The current value typed as the element's live property expects it (string, number or boolean). Values
    /// passed to <see cref="MarkLiveValueSynced"/> must use the same type. Defaults to
    /// <see cref="InputBase{TValue}.CurrentValueAsString"/>.
    /// </summary>
    /// <returns>The value to assign to the live property</returns>
    protected virtual object? GetLiveValue() => CurrentValueAsString;

    #endregion
}
