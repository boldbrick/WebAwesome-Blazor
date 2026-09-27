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
/// An experimental form control that collects a list of short strings as removable tags.
/// Corresponds to the wa-tag-input Web Awesome component.
/// </summary>
/// <remarks>
/// <para>
/// The live value is the element's "value" JS property, a string array; the "value" attribute only sets
/// defaultValue, as a delimiter-separated string, so it is never rendered (see the bound "Value" parameter).
/// </para>
/// <para>
/// <see cref="WaInputBase{TValue}.OnInput"/> is bound to the "input" event like on every other form control, but
/// its <see cref="Microsoft.AspNetCore.Components.ChangeEventArgs.Value"/> carries the element's current tag array
/// (the same shape the bound "Value" parameter takes), not the text being typed.
/// </para>
/// </remarks>
public class WaTagInput : WaLabeledInputBase<IReadOnlyList<string>?>, IWaClearableControl, IWaAffixedControl
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Makes the tag input read-only. Tags stay visible and are still submitted, but can't be added or removed.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the tag input as required for form validation: at least one tag must be added.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// The Web Awesome default of <see cref="Placeholder"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPlaceholder = "";

    /// <summary>
    /// Placeholder text to show in the text box. Hidden once the maximum number of tags is reached.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

    /// <summary>
    /// The tag input's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Draws a pill-style tag input, and pill-style tags, with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Adds a clear button that removes all tags.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Allows the same tag to be added more than once. By default, duplicates are ignored.
    /// </summary>
    [Parameter] public bool AllowDuplicates { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Delimiter"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultDelimiter = ",";

    /// <summary>
    /// The characters that turn typed text into a tag. Each character is a separate delimiter, so ",;" accepts both
    /// commas and semicolons. Pasted text is split on the same characters. An explicit empty string (as opposed to
    /// null, which leaves the element's own default) is rendered as the attribute "delimiter=\"\"", which the element
    /// reads as "only Enter adds a tag". Also used to parse and format the bound "Value" parameter as a
    /// delimiter-separated string, falling back to a comma when empty.
    /// </summary>
    [Parameter] public string? Delimiter { get; set; }

    /// <summary>
    /// The maximum number of tags that can be added. Once reached, no more tags can be added until one is removed.
    /// </summary>
    [Parameter] public int? MaxTags { get; set; }

    /// <summary>
    /// The minimum number of tags required for the control to be valid. Has no effect when there are no tags.
    /// </summary>
    [Parameter] public int? MinTags { get; set; }

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
    /// The Web Awesome default of <see cref="Spellcheck"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const bool DefaultSpellcheck = true;

    /// <summary>
    /// Enables spell checking on the text box.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

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
    /// Invoked before typed text becomes a tag. The event detail carries the text that would become the tag.
    /// </summary>
    /// <remarks>
    /// The underlying Web Awesome event is cancelable via <c>event.preventDefault()</c> (it would reject the tag),
    /// but Blazor dispatches custom event callbacks asynchronously after the DOM event has already run its course,
    /// so a .NET handler cannot call back into the DOM synchronously to reject it. This event is delivered as an
    /// informational notification only.
    /// </remarks>
    [Parameter] public EventCallback<WaCreateEventArgs> OnCreate { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the tag input.
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the tag input.
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default clear icon (see <see cref="WithClear"/>), rendered into the element's "clear-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

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
        var attributes = builder.OpenWaElement(this, 0, "wa-tag-input");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add tag-input-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 20, "placeholder", Placeholder, DefaultPlaceholder);
        builder.AddAttributeIfNotNull(attributes, 21, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 23, this);
        builder.AddAttribute(24, "allow-duplicates", AllowDuplicates);

        // an explicit empty string must render delimiter="" ("only Enter adds a tag"), so this uses the sticky
        // overload whose null check (not empty check) decides whether the parameter is unset
        builder.AddAttributeIfNotNull(attributes, 25, "delimiter", Delimiter, DefaultDelimiter);

        builder.AddAttributeIfNotNull(26, "max-tags", MaxTags);
        builder.AddAttributeIfNotNull(27, "min-tags", MinTags);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttributeIfNotNull(33, "autocapitalize", AutoCapitalize?.ToHtmlValue());
        builder.AddOnOffAttribute(34, "autocorrect", AutoCorrect);
        builder.AddAttributeIfNotNull(36, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(37, "inputmode", InputMode?.ToHtmlValue());
        builder.AddTrueFalseAttribute(attributes, 38, "spellcheck", Spellcheck, DefaultSpellcheck);

        // No "value" attribute is rendered: it only sets defaultValue as a delimiter-separated string, and the
        // element ignores it once the user has interacted. The live value is pushed into the "value" JS property
        // instead (see LiveValuePropertyName/GetLiveValue).
        builder.AddAttribute(32, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleChangeAsync));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add tag-input-specific event handlers
        FormControlRendering.AddClearEventHandler(builder, 50, this);
        builder.AddAttributeIfHasDelegate(51, "onwa-create", OnCreate);
        builder.AddAttributeIfHasDelegate(52, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(59, __tagInputReference => Element = __tagInputReference);

        // Add start and end slot content (the fragment wins over the icon-name shortcut)
        FormControlRendering.AddAffixSlots(builder, 60, this, StartIconName, EndIconName);

        // Add clear-icon slot content
        FormControlRendering.AddClearIconSlot(builder, 110, this);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 80);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out IReadOnlyList<string>? result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = ParseDelimited(value, Delimiter);
        validationErrorMessage = null;
        return true;
    }

    /// <inheritdoc />
    protected override string? FormatValueAsString(IReadOnlyList<string>? value)
    {
        if (value is not { Count: > 0 }) return null;

        var delimiterChar = string.IsNullOrEmpty(Delimiter) ? DefaultDelimiterChar : Delimiter[0];
        return string.Join(delimiterChar, value);
    }

    /// <summary>
    /// The value lives in the element's "value" property as a string array; the "value" attribute only sets
    /// defaultValue.
    /// </summary>
    protected override string? LiveValuePropertyName => LiveValueProperty;

    /// <summary>
    /// The tags currently held, as the array the element's "value" property expects.
    /// </summary>
    /// <returns>The tags</returns>
    protected override object? GetLiveValue() => liveTags;

    /// <summary>
    /// Keeps the array pushed to the element stable while the bound "Value" parameter keeps its content, so a
    /// re-render with an equal list does not reassign the element's tags.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var values = Value ?? [];
        if (!values.SequenceEqual(liveTags)) liveTags = values.ToArray();
    }

    /// <summary>
    /// Pushes the initial tags too, since no "value" attribute is rendered.
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && liveTags.Length > 0 && Element is not null)
            await JSInterop.SyncPropertyAsync(Element.Value, LiveValueProperty, liveTags);
    }

    #endregion

    #region ------ Internals ------

    private const string LiveValueProperty = "value";
    private const char DefaultDelimiterChar = ',';

    // the tags last pushed to or received from the element
    private string[] liveTags = [];

    // parses a delimiter-separated string the way the element's own parseDelimited does: split on any delimiter
    // character (a comma when the delimiter is empty), trim, drop empty items
    private static IReadOnlyList<string>? ParseDelimited(string? value, string? delimiter)
    {
        if (string.IsNullOrEmpty(value)) return null;

        var separators = string.IsNullOrEmpty(delimiter) ? [DefaultDelimiterChar] : delimiter.ToCharArray();
        var tags = value.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => tag.Length > 0)
            .ToArray();

        return tags.Length > 0 ? tags : null;
    }

    // the change event reports the element's current tag array; recorded as the live value before the model
    // changes, so the next render does not push it back
    private Task HandleChangeAsync(ChangeEventArgs args)
    {
        var values = args.GetStringArrayValue();
        liveTags = values;
        MarkLiveValueSynced(values);

        CurrentValue = values;

        return Task.CompletedTask;
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the text box.
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
    /// Removes focus from the text box.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    #endregion
}
