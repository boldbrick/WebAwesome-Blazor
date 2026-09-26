using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that formats a number as a human readable bytes value.
/// Corresponds to the wa-format-bytes Web Awesome component.
/// </summary>
public class WaFormatBytes : ComponentBase
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

    // Format properties
    /// <summary>
    /// The Web Awesome default of <see cref="Value"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const long DefaultValue = 0;

    /// <summary>
    /// The number to format in bytes.
    /// </summary>
    [Parameter] public long Value { get; set; } = DefaultValue;

    /// <summary>
    /// The Web Awesome default of <see cref="Unit"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaByteUnit DefaultUnit = WaByteUnit.Byte;

    /// <summary>
    /// The type of unit to display.
    /// </summary>
    [Parameter] public WaByteUnit Unit { get; set; } = DefaultUnit;

    /// <summary>
    /// The locale (BCP 47 language tag) to use when formatting the value. When unset, the browser's default locale is used.
    /// </summary>
    [Parameter] public string? Lang { get; set; }

    /// <summary>
    /// The unit label style to use when displaying the value.
    /// </summary>
    [Parameter] public WaDisplay? Display { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-format-bytes");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddNumberAttribute(attributes, 4, "value", Value);
        builder.AddDefaultedAttribute(attributes, 5, "unit", Unit.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 6, "lang", Lang);
        builder.AddAttributeIfNotNull(attributes, 7, "display", Display?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __formatBytesReference => Element = __formatBytesReference);

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
