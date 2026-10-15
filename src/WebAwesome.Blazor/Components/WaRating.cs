using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;

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
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

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
    /// The Web Awesome default of the bound value (the "value" attribute), which renders no attribute until the value
    /// first differs from it. Named after <c>CurrentValue</c>, because <c>DefaultValue</c> is the parameter below.
    /// </summary>
    public const decimal DefaultCurrentValue = 0m;

    /// <summary>
    /// The Web Awesome default of <see cref="DefaultValue"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultDefaultValue = 0m;

    /// <summary>
    /// The default value of the form control. Used to reset the rating to its initial value.
    /// </summary>
    [Parameter] public decimal DefaultValue { get; set; } = DefaultDefaultValue;

    #endregion

    #region ------ Function Members ------

    /// <summary>
    /// The JavaScript function that renders each symbol (the element's <c>getSymbol</c>). It is called as
    /// <c>(value, isSelected, ctx)</c>: the value the symbol stands for (1 to <see cref="Max"/>), whether it is shown as
    /// selected, and <c>ctx</c> with <c>args</c> (the <see cref="WaJsFunction.WithArgs"/> value) and <c>element</c>. It
    /// returns a string of trusted HTML, for example a <c>wa-icon</c>. When null, the element's stars apply. Declare the
    /// Web Awesome elements it renders with <see cref="WaJsFunction.Uses"/>.
    /// </summary>
    [Parameter] public WaJsFunction? GetSymbolFunction { get; set; }

    /// <summary>
    /// CSS rules for the output of <see cref="GetSymbolFunction"/>, adopted into the element's shadow root as a
    /// constructed stylesheet and replaced only when the text changes. It is for styling your own function output, not
    /// Web Awesome's internals: style those with <c>::part</c> and the CSS custom properties.
    /// </summary>
    [Parameter] public string? ShadowStyles { get; set; }

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
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "label", Label, DefaultLabel);

        // Add rating-specific attributes
        builder.AddNumberAttribute(attributes, 20, "max", Max, DefaultMax);
        builder.AddNumberAttribute(attributes, 21, "precision", Precision, DefaultPrecision);
        builder.AddAttribute(22, "readonly", Readonly);
        builder.AddNumberAttribute(attributes, 23, "value", CurrentValue, DefaultCurrentValue);
        builder.AddNumberAttribute(attributes, 24, "default-value", DefaultValue, DefaultDefaultValue);

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

    /// <summary>
    /// Pushes <see cref="GetSymbolFunction"/> and <see cref="ShadowStyles"/> to the element when they changed.
    /// </summary>
    /// <param name="firstRender">Whether this is the first time the component has rendered</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        await jsProperties.PushFunctionAsync(JSInterop, Element, GetSymbolMember, GetSymbolFunction);
        await jsProperties.PushShadowStylesAsync(JSInterop, Element, ShadowStyles);
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
    /// Sent JavaScript source text to the element's <c>getSymbol</c>, which the element cannot call. Use
    /// <see cref="GetSymbolFunction"/> instead.
    /// </summary>
    /// <param name="jsFunction">JavaScript source text, which is not sent</param>
    /// <returns>A task that always fails</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    /// <exception cref="ArgumentNullException">Thrown when jsFunction is null or empty</exception>
    /// <exception cref="NotSupportedException">Thrown always otherwise: the replacement is <see cref="GetSymbolFunction"/></exception>
    // TODO WA 3.15.0 train: remove this obsolete member (replaced by GetSymbolFunction)
    [Obsolete(SetSymbolFunctionObsoleteMessage)]
    public Task SetSymbolFunctionAsync(string jsFunction)
    {
        // the element would throw a TypeError on its next render, so the string is never sent
        Exception failure = Element == null
            ? new InvalidOperationException("Cannot set symbol function: component has not been rendered yet.")
            : string.IsNullOrEmpty(jsFunction)
                ? new ArgumentNullException(nameof(jsFunction))
                : new NotSupportedException(SetSymbolFunctionObsoleteMessage);

        return Task.FromException(failure);
    }

    #endregion

    #region ------ Internals ------

    // the function-valued member of wa-rating
    private const string GetSymbolMember = "getSymbol";

    private const string SetSymbolFunctionObsoleteMessage = "JavaScript source text cannot become the function that wa-rating calls, so the element failed to render. Use GetSymbolFunction with a WaJsFunction reference. This method throws NotSupportedException and is removed in the 3.15.0 train.";

    // the function member and the shadow styles last pushed to the element
    private readonly WaJsPropertyPush jsProperties = new();

    // handles the change event, whose value the numericchange alias delivers as a JS-formatted number; leaves the
    // model unchanged when the value cannot be parsed
    private void HandleValueChange(ChangeEventArgs args)
    {
        if (ChangeEventArgsExtensions.TryParseJsNumber(args.GetStringValue(), out var value))
            CurrentValue = value;
    }

    #endregion
}

