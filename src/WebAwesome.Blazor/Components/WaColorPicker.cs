using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A color picker input component that allows users to select colors.
/// Corresponds to the wa-color-picker Web Awesome component.
/// </summary>
public class WaColorPicker : WaInputBase<string>
{
    #region ------ Color Picker Properties ------

    /// <summary>
    /// Shows the opacity slider. Enabling this causes the formatted value to be HEXA, RGBA, or HSLA.
    /// </summary>
    [Parameter] public bool Opacity { get; set; }

    /// <summary>
    /// The color format to use. If <see cref="Opacity"/> is enabled, formats translate to their alpha-channel
    /// equivalent (HEXA, RGBA, HSLA, or HSVA). The color picker accepts user input in any format, including CSS
    /// color names, and converts it to the desired format.
    /// </summary>
    [Parameter] public WaColorFormat Format { get; set; } = WaColorFormat.Hex;

    /// <summary>
    /// Removes the button that lets users toggle between formats.
    /// </summary>
    [Parameter] public bool WithoutFormatToggle { get; set; }

    /// <summary>
    /// One or more predefined color swatches to display as presets, separated by a semicolon (<c>;</c>). Can include
    /// any format the color picker can parse, such as HEX(A), RGB(A), HSL(A), HSV(A), or CSS color names.
    /// </summary>
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
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when colors is null</exception>
    public async Task SetSwatchesAsync(string[] colors)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set swatches: component has not been rendered yet.");

        if (colors == null)
            throw new ArgumentNullException(nameof(colors));

        await JSInterop.SetPropertyAsync(Element.Value, "swatches", colors);
    }

    /// <summary>
    /// Gets the current color value in the specified format.
    /// </summary>
    /// <param name="format">The desired color format</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the color value in the specified format</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    public async Task<string> GetFormattedValueAsync(WaColorFormat format)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get formatted value: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<string>(Element.Value, "getFormattedValue", format.ToHtmlValue());
    }

    #endregion
}
