using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that formats a number using the specified locale and options.
/// Corresponds to the wa-format-number Web Awesome component.
/// </summary>
public class WaFormatNumber : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // Number formatting properties
    /// <summary>
    /// The number to format.
    /// </summary>
    [Parameter] public decimal Value { get; set; }

    /// <summary>
    /// The formatting style to use.
    /// </summary>
    [Parameter] public WaFormatNumberType Type { get; set; } = WaFormatNumberType.Decimal;

    /// <summary>
    /// The locale (BCP 47 language tag) to use when formatting the number. When unset, the browser's default locale is used.
    /// </summary>
    [Parameter] public string? Lang { get; set; }

    /// <summary>
    /// The <see href="https://en.wikipedia.org/wiki/ISO_4217">ISO 4217</see> currency code to use when formatting.
    /// </summary>
    [Parameter] public string? Currency { get; set; }

    /// <summary>
    /// How to display the currency.
    /// </summary>
    [Parameter] public WaCurrencyDisplay? CurrencyDisplay { get; set; }

    // Intl.NumberFormat options
    /// <summary>
    /// The minimum number of integer digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MinimumIntegerDigits { get; set; }

    /// <summary>
    /// The minimum number of fraction digits to use. Possible values are 0-100.
    /// </summary>
    [Parameter] public int? MinimumFractionDigits { get; set; }

    /// <summary>
    /// The maximum number of fraction digits to use. Possible values are 0-100.
    /// </summary>
    [Parameter] public int? MaximumFractionDigits { get; set; }

    /// <summary>
    /// The minimum number of significant digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MinimumSignificantDigits { get; set; }

    /// <summary>
    /// The maximum number of significant digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MaximumSignificantDigits { get; set; }

    /// <summary>
    /// The formatting notation to use, such as scientific, engineering, or compact notation.
    /// </summary>
    [Parameter] public WaNotation? Notation { get; set; }

    /// <summary>
    /// The display style to use for compact notation when <see cref="Notation"/> is compact.
    /// </summary>
    [Parameter] public WaCompactDisplay? CompactDisplay { get; set; }

    /// <summary>
    /// Whether to use grouping separators, such as thousands separators.
    /// </summary>
    [Parameter] public bool? UseGrouping { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-format-number");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttribute(4, "value", Value);
        builder.AddAttribute(5, "type", Type.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(6, "lang", Lang);
        builder.AddAttributeIfNotNullOrEmpty(7, "currency", Currency);
        builder.AddAttributeIfNotNull(8, "currency-display", CurrencyDisplay?.ToHtmlValue());

        // Add formatting options
        builder.AddAttributeIfNotNull(10, "minimum-integer-digits", MinimumIntegerDigits);
        builder.AddAttributeIfNotNull(11, "minimum-fraction-digits", MinimumFractionDigits);
        builder.AddAttributeIfNotNull(12, "maximum-fraction-digits", MaximumFractionDigits);
        builder.AddAttributeIfNotNull(13, "minimum-significant-digits", MinimumSignificantDigits);
        builder.AddAttributeIfNotNull(14, "maximum-significant-digits", MaximumSignificantDigits);
        builder.AddAttributeIfNotNull(15, "notation", Notation?.ToHtmlValue());
        builder.AddAttributeIfNotNull(16, "compact-display", CompactDisplay?.ToHtmlValue());
        builder.AddAttributeIfNotNull(17, "use-grouping", UseGrouping);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __formatNumberReference => Element = __formatNumberReference);

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
