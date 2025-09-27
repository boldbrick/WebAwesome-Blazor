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
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // --- Visual & behavior props ---
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public int Rows { get; set; }
    [Parameter] public WaSize? Size { get; set; }
    [Parameter] public WaInputAppearance? Appearance { get; set; }
    [Parameter] public WaResize? Resize { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Required { get; set; }

    // Validation
    [Parameter] public int? MinLength { get; set; }
    [Parameter] public int? MaxLength { get; set; }

    // Browser behavior
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public bool? Spellcheck { get; set; }

    // Labels & hint (string or RenderFragment via MarkupX)
    [Parameter] public string? Label { get; set; }
    [Parameter] public RenderFragment? MarkupLabel { get; set; }
    [Parameter] public string? Hint { get; set; }
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
