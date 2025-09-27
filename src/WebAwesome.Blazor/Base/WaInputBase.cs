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

    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    // Common styling parameters
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // Visual & behavior properties
    [Parameter] public WaSize? Size { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Required { get; set; }

    // Labels & hints (string or RenderFragment)
    [Parameter] public string? Label { get; set; }
    [Parameter] public RenderFragment? MarkupLabel { get; set; }
    [Parameter] public string? Hint { get; set; }
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    // Validation
    [Parameter] public int? MinLength { get; set; }
    [Parameter] public int? MaxLength { get; set; }

    // Browser behavior
    [Parameter] public string? Autocomplete { get; set; }

    // Common events
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }
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
    /// <param name="sequence">The starting sequence number</param>
    /// <returns>The next available sequence number</returns>
    protected int AddCommonEventHandlers(RenderTreeBuilder builder, int sequence)
    {
        var currentSequence = sequence;

        if (OnFocus.HasDelegate)
            builder.AddAttribute(currentSequence++, "onfocus", OnFocus);

        if (OnBlur.HasDelegate)
            builder.AddAttribute(currentSequence++, "onblur", OnBlur);

        if (OnKeyDown.HasDelegate)
            builder.AddAttribute(currentSequence++, "onkeydown", OnKeyDown);

        if (OnKeyUp.HasDelegate)
            builder.AddAttribute(currentSequence++, "onkeyup", OnKeyUp);

        if (OnKeyPress.HasDelegate)
            builder.AddAttribute(currentSequence++, "onkeypress", OnKeyPress);

        if (OnInput.HasDelegate)
            builder.AddAttribute(currentSequence++, "oninput", OnInput);

        return currentSequence;
    }

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

    #endregion
}
