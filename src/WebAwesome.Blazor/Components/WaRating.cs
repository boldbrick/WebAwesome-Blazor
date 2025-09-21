using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A rating input component that allows users to provide feedback using stars or custom symbols.
/// Corresponds to the wa-rating Web Awesome component.
/// </summary>
public class WaRating : WaInputBase<decimal>
{
    #region ------ Rating Properties ------

    [Parameter] public int Max { get; set; } = 5;
    [Parameter] public decimal Precision { get; set; } = 1;

    #endregion

    #region ------ Events ------

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
        {
            // TODO: This requires custom event handling for wa-hover events
            // builder.AddAttribute(40, "wa-hover", OnHover);
        }

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
    /// <param name="jsFunction">JavaScript function that returns HTML for the symbol</param>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-rating's getSymbol property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void SetSymbolFunction(string jsFunction)
    {
        // TODO: Implement JavaScript interop call
        // Should set Element.getSymbol = jsFunction on the underlying wa-rating element
        throw new NotImplementedException("SetSymbolFunction requires JavaScript interop implementation. " +
            "This should set the underlying wa-rating element's getSymbol property.");
    }

    #endregion
}

/// <summary>
/// Event arguments for rating hover events
/// </summary>
public class WaRatingHoverEventArgs : EventArgs
{
    public string Phase { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
