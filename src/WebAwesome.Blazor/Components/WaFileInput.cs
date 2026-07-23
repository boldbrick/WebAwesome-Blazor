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
/// Selected files are runtime <c>File</c> objects and are not exposed as a bindable scalar value;
/// use the underlying element's change/input events together with JavaScript interop or a custom
/// upload handler to read the selected files.
/// </remarks>
public class WaFileInput : ComponentBase, IFormValidation
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
    /// One or more comma-separated file types the input should accept.
    /// </summary>
    [Parameter] public string? Accept { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute. Ignored when <see cref="HintContent"/> is set.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute. Ignored when <see cref="LabelContent"/> is set.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Allows the user to select more than one file.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Marks the file input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// The file input's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

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
    /// Invoked when the control gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the control loses focus.
    /// </summary>
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
    /// Rich markup rendered into the "file-icon" slot, replacing the default file icon.
    /// </summary>
    [Parameter] public RenderFragment? FileIconContent { get; set; }

    /// <summary>
    /// Rich markup rendered into the "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Rich markup rendered into the "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? HintContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-file-input");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add file-input-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "accept", Accept);
        builder.AddAttributeIfNotNullOrEmpty(11, "hint", Hint);
        builder.AddAttributeIfNotNullOrEmpty(12, "label", Label);
        builder.AddAttribute(13, "multiple", Multiple);
        builder.AddAttribute(14, "required", Required);
        builder.AddAttributeIfNotNull(15, "size", Size?.ToHtmlValue());
        builder.AddAttribute(16, "with-hint", WithHint);
        builder.AddAttribute(17, "with-label", WithLabel);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(30, "onchange", OnChange);
        builder.AddAttributeIfHasDelegate(31, "oninput", OnInput);
        builder.AddAttributeIfHasDelegate(32, "onfocus", OnFocus);
        builder.AddAttributeIfHasDelegate(33, "onblur", OnBlur);
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

        // Add file-icon slot content
        if (FileIconContent is not null)
        {
            builder.OpenElement(55, "span");
            builder.AddAttribute(56, "slot", "file-icon");
            builder.AddContent(57, FileIconContent);
            builder.CloseElement();
        }

        // Add label slot content
        if (LabelContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "label");
            builder.AddContent(62, LabelContent);
            builder.CloseElement();
        }

        // Add hint slot content
        if (HintContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "hint");
            builder.AddContent(67, HintContent);
            builder.CloseElement();
        }

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

    #endregion
}
