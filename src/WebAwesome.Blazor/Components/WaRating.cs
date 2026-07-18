using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A rating input component that allows users to provide feedback using stars or custom symbols.
/// Corresponds to the wa-rating Web Awesome component.
/// </summary>
public class WaRating : WaInputBase<decimal>
{
    #region ------ Rating Properties ------

    /// <summary>
    /// The highest rating to show.
    /// </summary>
    [Parameter] public int Max { get; set; } = 5;

    /// <summary>
    /// The precision at which the rating will increase and decrease. For example, to allow half-star ratings, set this to 0.5.
    /// </summary>
    [Parameter] public decimal Precision { get; set; } = 1;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the user hovers over a value. The event args indicate the hover phase and the value that would be committed if the user were to select it.
    /// </summary>
    [Parameter] public EventCallback<WaRatingHoverEventArgs> OnHover { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-rating");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add rating-specific attributes
        builder.AddAttribute(20, "max", Max);
        builder.AddAttribute(21, "precision", Precision);
        builder.AddAttribute(22, "readonly", Readonly);
        builder.AddAttribute(23, "value", BindConverter.FormatValue(CurrentValue));

        // Add value binding
        builder.AddAttribute(30, "onchange", EventCallback.Factory.CreateBinder<decimal>(this, __value => CurrentValue = __value, CurrentValue));
        builder.SetUpdatesAttributeName("value");

        // Add rating-specific event handlers
        if (OnHover.HasDelegate)
            builder.AddAttribute(40, "wa-hover", OnHover);

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __ratingReference => Element = __ratingReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (decimal.TryParse(value, out result))
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
}

