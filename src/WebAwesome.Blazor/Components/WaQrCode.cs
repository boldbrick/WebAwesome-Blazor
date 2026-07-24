using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that generates QR codes and renders them using the Canvas API.
/// Corresponds to the wa-qr-code Web Awesome component.
/// </summary>
public class WaQrCode : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // QR Code properties
    /// <summary>
    /// The QR code's value.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// The label for assistive devices to announce. If unspecified, <see cref="Value"/> is used instead.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The size of the QR code, in pixels.
    /// </summary>
    [Parameter] public int Size { get; set; } = 128;

    /// <summary>
    /// The fill color. This can be any valid CSS color, but not a CSS custom property. When unset, the
    /// underlying element inherits its color from the current theme.
    /// </summary>
    [Parameter] public string? Fill { get; set; }

    /// <summary>
    /// The background color. This can be any valid CSS color or <c>transparent</c>. It cannot be a CSS
    /// custom property. When unset, the underlying element inherits its background from the current theme.
    /// </summary>
    [Parameter] public string? Background { get; set; }

    /// <summary>
    /// The edge radius of each module. Must be between 0 and 0.5.
    /// </summary>
    [Parameter] public decimal Radius { get; set; } = 0;

    /// <summary>
    /// The level of error correction to use.
    /// </summary>
    [Parameter] public WaErrorCorrection ErrorCorrection { get; set; } = WaErrorCorrection.M;

    /// <summary>
    /// A URL to an image (typically a logo) rendered in the center of the QR code. When set, the error
    /// correction level is automatically raised to keep the code scannable.
    /// </summary>
    [Parameter] public string? Image { get; set; }

    /// <summary>
    /// The background color drawn behind the center <see cref="Image"/>. Any valid CSS color, or <c>transparent</c>.
    /// </summary>
    [Parameter] public string? ImageBackground { get; set; }

    /// <summary>
    /// How much of the QR code the center <see cref="Image"/> is allowed to cover, from 0 to 1. Higher values make
    /// the code harder to scan.
    /// </summary>
    [Parameter] public decimal? ImageCoverage { get; set; }

    /// <summary>
    /// The padding, in pixels, around the center <see cref="Image"/>.
    /// </summary>
    [Parameter] public int? ImagePadding { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-qr-code");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "value", Value);
        builder.AddAttributeIfNotNullOrEmpty(5, "label", Label);
        builder.AddAttribute(6, "size", Size);
        builder.AddAttributeIfNotNullOrEmpty(7, "fill", Fill);
        builder.AddAttributeIfNotNullOrEmpty(8, "background", Background);
        builder.AddAttribute(9, "radius", Radius);
        builder.AddAttribute(10, "error-correction", ErrorCorrection.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(11, "image", Image);
        builder.AddAttributeIfNotNullOrEmpty(12, "image-background", ImageBackground);
        builder.AddAttributeIfNotNull(13, "image-coverage", ImageCoverage);
        builder.AddAttributeIfNotNull(14, "image-padding", ImagePadding);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __qrCodeReference => Element = __qrCodeReference);

        builder.CloseElement();
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
