using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A color picker input component that allows users to select colors.
/// Corresponds to the wa-color-picker Web Awesome component.
/// </summary>
public class WaColorPicker : WaPopupInputBase<string?>
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    #endregion

    #region ------ Color Picker Properties ------

    /// <summary>
    /// Shows the opacity slider. Enabling this causes the formatted value to be HEXA, RGBA, or HSLA.
    /// </summary>
    [Parameter] public bool Opacity { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Format"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaColorFormat DefaultFormat = WaColorFormat.Hex;

    /// <summary>
    /// The color format to use. If <see cref="Opacity"/> is enabled, formats translate to their alpha-channel
    /// equivalent (HEXA, RGBA, HSLA, or HSVA). The color picker accepts user input in any format, including CSS
    /// color names, and converts it to the desired format.
    /// </summary>
    [Parameter] public WaColorFormat Format { get; set; } = DefaultFormat;

    /// <summary>
    /// Removes the button that lets users toggle between formats.
    /// </summary>
    [Parameter] public bool WithoutFormatToggle { get; set; }

    /// <summary>
    /// Predefined color swatches to display as presets, in list order (rendered separated by a semicolon, so a swatch
    /// cannot contain one; null or empty shows none). Can include
    /// any format the color picker can parse, such as HEX(A), RGB(A), HSL(A), HSV(A), or CSS color names.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? Swatches { get; set; }

    /// <summary>
    /// Renders the color format toggle and hex input using uppercase letters.
    /// </summary>
    [Parameter] public bool Uppercase { get; set; }

    /// <summary>
    /// The preferred placement of the color picker's popup. The actual placement may vary to keep the panel
    /// inside the viewport.
    /// </summary>
    [Parameter] public WaPlacement? Placement { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-color-picker");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(8, "required", Required);
        AddLabelAndHintAttributes(builder, 12);

        // Add color picker-specific attributes
        builder.AddAttribute(20, "opacity", Opacity);
        builder.AddDefaultedAttribute(attributes, 21, "format", Format.ToHtmlValue());
        builder.AddAttribute(22, "without-format-toggle", WithoutFormatToggle);
        builder.AddTokenListAttribute(attributes, 23, "swatches", Swatches, WaWireFormat.SemicolonSeparator, WaWireFormat.SemicolonSeparators);
        builder.AddAttribute(24, "value", CurrentValueAsString);
        builder.AddAttribute(25, "open", Open);
        builder.AddAttribute(26, "uppercase", Uppercase);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttributeIfNotNull(attributes, 29, "placement", Placement?.ToHtmlValue());

        // Add value binding
        builder.AddAttribute(30, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add color picker-specific event handlers; the popup events are relayed, because the element dispatches
        // them as non-bubbling events, which Blazor never receives
        AddPopupEventHandlers(builder, 56);
        builder.AddAttributeIfHasDelegate(64, "onwa-invalid", OnInvalid);

        // the keydown is relayed too, because the element stops the propagation of Escape while it is open
        AddRelayedKeyDownHandler(builder, 65);

        // Add element reference capture
        builder.AddElementReferenceCapture(67, __colorPickerReference => Element = __colorPickerReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    /// <inheritdoc />
    internal override bool RelaysKeyDown => true;

    /// <summary>
    /// wa-color-picker dispatches its popup events as non-bubbling CustomEvents, which Blazor never receives, so
    /// their relays are bound.
    /// </summary>
    internal override bool RelaysPopupEvents => true;

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets the color picker's swatches programmatically.
    /// </summary>
    /// <param name="colors">Array of color values</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when colors is null</exception>
    public async Task SetSwatchesAsync(IEnumerable<string> colors)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set swatches: component has not been rendered yet.");

        if (colors == null)
            throw new ArgumentNullException(nameof(colors));

        await JSInterop.SetPropertyAsync(Element.Value, "swatches", colors.ToArray());
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

    #endregion
}
