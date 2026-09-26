using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A rating input component that allows users to provide feedback using stars or custom symbols.
/// Corresponds to the wa-rating Web Awesome component.
/// </summary>
/// <remarks>
/// wa-rating dispatches no input event, so the inherited <see cref="WaInputBase{TValue}.OnInput"/> is never
/// raised; use <c>@bind-Value</c> (the change event) or <see cref="OnHover"/> instead.
/// </remarks>
public class WaRating : WaInputBase<decimal>
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Makes the input read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute (wa-rating has no label slot).
    /// </summary>
    [Parameter] public string? Label { get; set; }

    #endregion

    #region ------ Rating Properties ------

    /// <summary>
    /// The Web Awesome default of <see cref="Max"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultMax = 5;

    /// <summary>
    /// The highest rating to show.
    /// </summary>
    [Parameter] public int Max { get; set; } = DefaultMax;

    /// <summary>
    /// The Web Awesome default of <see cref="Precision"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultPrecision = 1m;

    /// <summary>
    /// The precision at which the rating will increase and decrease. For example, to allow half-star ratings, set this to 0.5.
    /// </summary>
    [Parameter] public decimal Precision { get; set; } = DefaultPrecision;

    /// <summary>
    /// The Web Awesome default of <see cref="DefaultValue"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultDefaultValue = 0m;

    /// <summary>
    /// The default value of the form control. Used to reset the rating to its initial value.
    /// </summary>
    [Parameter] public decimal DefaultValue { get; set; } = DefaultDefaultValue;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the user hovers over a value. The event args indicate the hover phase and the value that would be committed if the user were to select it.
    /// </summary>
    [Parameter] public EventCallback<WaRatingHoverEventArgs> OnHover { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-rating");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "label", Label);

        // Add rating-specific attributes
        builder.AddNumberAttribute(attributes, 20, "max", Max);
        builder.AddNumberAttribute(attributes, 21, "precision", Precision);
        builder.AddAttribute(22, "readonly", Readonly);
        builder.AddNumberAttribute(attributes, 23, "value", CurrentValue);
        builder.AddNumberAttribute(attributes, 24, "default-value", DefaultValue);

        // Add value binding; the element's live value is a JS number, which Blazor's built-in change reader cannot
        // carry, so the handler listens to the "numericchange" alias of the change event that delivers it as an
        // invariant-culture string. No live-property sync is needed: the value attribute maps to the live property.
        builder.AddAttribute(30, Constants.NumericChangeEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleValueChange));
        builder.SetUpdatesAttributeName("value");

        // Add rating-specific event handlers
        builder.AddAttributeIfHasDelegate(40, "onwa-hover", OnHover);
        builder.AddAttributeIfHasDelegate(41, "onwa-invalid", OnInvalid);

        // Add common event handlers; wa-rating dispatches no input event, so the inherited OnInput is not bound
        AddCommonEventHandlers(builder, 50, includeInputHandler: false);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __ratingReference => Element = __ratingReference);

        builder.CloseElement();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Formats with the invariant culture, the form <see cref="TryParseValueFromString"/> reads back (the base class would
    /// use the current culture, e.g. "2,5").
    /// </remarks>
    protected override string? FormatValueAsString(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (ChangeEventArgsExtensions.TryParseJsNumber(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }

        result = default;
        validationErrorMessage = $"The {DisplayName ?? FieldIdentifier.FieldName} field must be a number.";
        return false;
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Removes focus from the rating.
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
    /// Sets focus on the rating.
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
    /// Sets a custom symbol function for rendering icons.
    /// </summary>
    /// <param name="jsFunction">JavaScript function string that returns HTML for the symbol</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when jsFunction is null or empty</exception>
    public async Task SetSymbolFunctionAsync(string jsFunction)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set symbol function: component has not been rendered yet.");

        if (string.IsNullOrEmpty(jsFunction))
            throw new ArgumentNullException(nameof(jsFunction));

        await JSInterop.SetPropertyAsync(Element.Value, "getSymbol", jsFunction);
    }

    #endregion

    #region ------ Internals ------

    // handles the change event, whose value the numericchange alias delivers as a JS-formatted number; leaves the
    // model unchanged when the value cannot be parsed
    private void HandleValueChange(ChangeEventArgs args)
    {
        if (ChangeEventArgsExtensions.TryParseJsNumber(args.GetStringValue(), out var value))
            CurrentValue = value;
    }

    #endregion
}

