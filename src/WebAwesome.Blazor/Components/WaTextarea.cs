using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A multiline input component for editing <see cref="string"/> values.
/// </summary>
public class WaTextArea : InputBase<string?>, IFormValidation
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    // ----- Common parameters -----
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // --- Visual & behavior props ---
    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The number of rows to display by default.
    /// </summary>
    [Parameter] public int Rows { get; set; }

    /// <summary>
    /// The textarea's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// The textarea's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Controls how the textarea can be resized.
    /// </summary>
    [Parameter] public WaResize? Resize { get; set; }

    /// <summary>
    /// Disables the textarea.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Makes the textarea readonly.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Makes the textarea a required field.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    // Validation
    /// <summary>
    /// The minimum length of input that will be considered valid.
    /// </summary>
    [Parameter] public int? MinLength { get; set; }

    /// <summary>
    /// The maximum length of input that will be considered valid.
    /// </summary>
    [Parameter] public int? MaxLength { get; set; }

    // Browser behavior
    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    /// <summary>
    /// Enables spell checking on the textarea.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    // Labels & hint (string or RenderFragment via MarkupX)
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

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-textarea");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "name", NameAttributeValue);
        builder.AddAttributeIfNotNullOrEmpty(3, "class", String.Join(' ', Class, CssClass));
        builder.AddAttributeIfNotNullOrEmpty(4, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(5, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(6, "rows", Rows);
        builder.AddAttributeIfNotNull(7, "size", Size?.ToString().ToLowerInvariant());
        builder.AddAttributeIfNotNull(8, "appearance", Appearance?.ToString().ToLowerInvariant());
        builder.AddAttributeIfNotNull(9, "resize", Resize?.ToString().ToLowerInvariant());
        builder.AddAttribute(10, "disabled", Disabled);
        builder.AddAttribute(11, "readonly", Readonly);
        builder.AddAttribute(12, "required", Required);
        builder.AddAttributeIfNotNull(13, "minlength", MinLength);
        builder.AddAttributeIfNotNull(14, "maxlength", MaxLength);
        builder.AddAttributeIfNotNullOrEmpty(15, "autocomplete", Autocomplete);
        builder.AddAttributeIfNotNull(16, "spellcheck", Spellcheck);
        builder.AddAttributeIfNotNullOrEmpty(17, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(18, "hint", Hint);
        builder.AddAttribute(19, "value", CurrentValueAsString);
        builder.AddAttribute(20, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");
        builder.AddElementReferenceCapture(21, __inputReference => Element = __inputReference);
        if (MarkupLabel is not null)
        {
            builder.OpenElement(22, "span");
            builder.AddAttribute(23, "slot", "label");
            builder.AddContent(24, MarkupLabel);
            builder.CloseElement();
        }
        if (MarkupHint is not null)
        {
            builder.OpenElement(25, "span");
            builder.AddAttribute(26, "slot", "hint");
            builder.AddContent(27, MarkupHint);
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

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
