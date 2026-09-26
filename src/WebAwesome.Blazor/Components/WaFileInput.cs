using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A file input component that allows the user to choose one or more files from their device.
/// Corresponds to the wa-file-input Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// Selected files are runtime <c>File</c> objects and are not exposed as a bindable scalar value;
/// use the underlying element's change/input events together with JavaScript interop or a custom
/// upload handler to read the selected files.
/// </remarks>
public class WaFileInput : ComponentBase, IFormValidation, IWaLabeledControl
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // File input properties
    /// <summary>
    /// The file types the input accepts, each an extension (<c>.pdf</c>), a MIME type (<c>image/png</c>) or a wildcard
    /// MIME type (<c>image/*</c>), rendered separated by a comma like the native accept; null or empty accepts any file.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? Accept { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute; <see cref="MarkupHint"/> takes precedence when set.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute; <see cref="MarkupLabel"/> takes precedence when set.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Allows the user to select more than one file.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Disables the file input, preventing user interaction.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Marks the file input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// The file input's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// On mobile devices, which camera or microphone to use when capturing media. Only applies when
    /// <see cref="Accept"/> includes an image, video, or audio type and may be ignored on devices lacking the
    /// corresponding hardware.
    /// </summary>
    [Parameter] public WaCaptureMode? Capture { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the hint slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the label slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the selected files change and the control loses focus.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnChange { get; set; }

    /// <summary>
    /// Invoked when the selected files change.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInput { get; set; }

    /// <summary>
    /// Invoked when the focus moves into the control. Bound to the bubbling, composed focusin event (so
    /// <see cref="FocusEventArgs.Type"/> is "focusin"), because the focus lands in the element's shadow root,
    /// where Blazor never sees the non-bubbling focus event.
    /// </summary>
    /// <remarks>
    /// A focus move inside the control's shadow root raises nothing; a move between the control and focusable
    /// content slotted into it (e.g. a button in <see cref="DropzoneContent"/>) raises <see cref="OnBlur"/>
    /// followed by <see cref="OnFocus"/>.
    /// </remarks>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the focus leaves the control. Bound to the bubbling, composed focusout event (so
    /// <see cref="FocusEventArgs.Type"/> is "focusout").
    /// </summary>
    /// <remarks>
    /// Also raised, followed by <see cref="OnFocus"/>, when the focus moves between the control and focusable
    /// content slotted into it; see <see cref="OnFocus"/>.
    /// </remarks>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Rich markup rendered into the "dropzone" slot, replacing the default dropzone content.
    /// </summary>
    [Parameter] public RenderFragment? DropzoneContent { get; set; }

    /// <summary>
    /// Rich markup label rendered into the element's "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <summary>
    /// Rich markup hint rendered into the element's "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-file-input");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add file-input-specific attributes
        builder.AddTokenListAttribute(attributes, 10, "accept", Accept, WaWireFormat.CommaSeparator, WaWireFormat.CommaSeparators);

        // hint before label, unlike the shared label cluster renderer, so the attribute order stays as it was
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "hint", Hint);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "label", Label);
        builder.AddAttribute(13, "multiple", Multiple);
        builder.AddAttribute(14, "required", Required);
        builder.AddAttribute(18, "disabled", Disabled);
        builder.AddAttributeIfNotNull(attributes, 15, "size", Size?.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 19, "capture", Capture?.ToHtmlValue());
        FormControlRendering.AddWithHintAndLabelAttributes(builder, 16, this);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(30, "onchange", OnChange);
        builder.AddAttributeIfHasDelegate(31, "oninput", OnInput);
        builder.AddAttributeIfHasDelegate(32, "onfocusin", OnFocus);
        builder.AddAttributeIfHasDelegate(33, "onfocusout", OnBlur);
        builder.AddAttributeIfHasDelegate(34, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(40, __fileInputReference => Element = __fileInputReference);

        // Add dropzone slot content
        if (DropzoneContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "dropzone");
            builder.AddContent(52, DropzoneContent);
            builder.CloseElement();
        }

        // Add label and hint slot content
        FormControlRendering.AddLabelAndHintSlots(builder, 60, this);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the file input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the file input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the file input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the file input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion

    #region ------ Implementation of IFormValidation ------

    /// <inheritdoc />
    public async Task SetCustomValidityAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set custom validity before the component is rendered. Element reference is null.");

        await JSInterop.SetCustomValidityAsync(Element.Value, message);
    }

    /// <inheritdoc />
    public async Task ResetValidityAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot reset validity before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "resetValidity");
    }

    #endregion
}
