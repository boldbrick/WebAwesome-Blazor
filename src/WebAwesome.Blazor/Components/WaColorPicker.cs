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

    /// <summary>
    /// Indicates whether the color picker's dropdown is open.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Renders the color format toggle and hex input using uppercase letters.
    /// </summary>
    [Parameter] public bool Uppercase { get; set; }

    /// <summary>
    /// Reserves space for the hint even when it is not populated.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Reserves space for the label even when it is not populated.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    /// <summary>
    /// The preferred placement of the color picker's popup. The actual placement may vary to keep the panel
    /// inside the viewport.
    /// </summary>
    [Parameter] public WaPlacement? Placement { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the color picker's dropdown opens.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the color picker's dropdown closes.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the color picker's dropdown opens and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the color picker's dropdown closes and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

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
        builder.AddAttribute(25, "open", Open);
        builder.AddAttribute(26, "uppercase", Uppercase);
        builder.AddAttribute(27, "with-hint", WithHint);
        builder.AddAttribute(28, "with-label", WithLabel);
        builder.AddAttributeIfNotNull(29, "placement", Placement?.ToHtmlValue());

        // Add value binding
        builder.AddAttribute(30, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add color picker-specific event handlers
        builder.AddAttributeIfHasDelegate(56, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(57, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(58, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(59, "onwa-after-hide", OnAfterHide);
        builder.AddAttributeIfHasDelegate(60, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(65, __colorPickerReference => Element = __colorPickerReference);

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

    /// <summary>
    /// Removes focus from the color picker.
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
    /// Sets focus on the color picker.
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
    /// Gets the current value as a hex string, regardless of the configured <see cref="Format"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the hex string value</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<string> GetHexStringAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get hex string: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<string>(Element.Value, "getHexString");
    }

    /// <summary>
    /// Hides the color picker's dropdown.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Checks the validity of the color picker and shows the browser's validation message if it is invalid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the value is valid</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<bool> ReportValidityAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot report validity: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<bool>(Element.Value, "reportValidity");
    }

    /// <summary>
    /// Shows the color picker's dropdown.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    #endregion
}
