using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A color picker input component that allows users to select colors.
/// Corresponds to the wa-color-picker Web Awesome component.
/// </summary>
public class WaColorPicker : WaInputBase<string>
{
    #region ------ Color Picker Properties ------

    [Parameter] public bool Opacity { get; set; }
    [Parameter] public WaColorFormat Format { get; set; } = WaColorFormat.Hex;
    [Parameter] public bool WithoutFormatToggle { get; set; }
    [Parameter] public string? Swatches { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-color-picker");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add color picker-specific attributes
        builder.AddAttribute(20, "opacity", Opacity);
        builder.AddAttribute(21, "format", Format.ToHtmlValue());
        builder.AddAttribute(22, "without-format-toggle", WithoutFormatToggle);
        builder.AddAttributeIfNotNullOrEmpty(23, "swatches", Swatches);
        builder.AddAttribute(24, "value", CurrentValueAsString);

        // Add value binding
        builder.AddAttribute(30, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __colorPickerReference => Element = __colorPickerReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out string result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value!;
        validationErrorMessage = null;
        return true;
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets the color picker's swatches programmatically.
    /// </summary>
    /// <param name="colors">Array of color values</param>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to set the underlying wa-color-picker's swatches property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void SetSwatches(string[] colors)
    {
        // TODO: Implement JavaScript interop call
        // Should set Element.swatches = colors on the underlying wa-color-picker element
        throw new NotImplementedException("SetSwatches requires JavaScript interop implementation. " +
            "This should set the underlying wa-color-picker element's swatches property.");
    }

    /// <summary>
    /// Gets the current color value in the specified format.
    /// </summary>
    /// <param name="format">The desired color format</param>
    /// <returns>The color value in the specified format</returns>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-color-picker's getFormattedValue method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public string GetFormattedValue(WaColorFormat format)
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.getFormattedValue(format) on the underlying wa-color-picker element
        throw new NotImplementedException("GetFormattedValue requires JavaScript interop implementation. " +
            "This should call the underlying wa-color-picker element's getFormattedValue method.");
    }

    #endregion
}
