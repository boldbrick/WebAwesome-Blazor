using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that formats a date/time using the specified locale and options.
/// Corresponds to the wa-format-date Web Awesome component.
/// </summary>
public class WaFormatDate : ComponentBase
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

    // Common styling parameters
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // Date/time properties
    [Parameter] public string? Date { get; set; }
    [Parameter] public string? Lang { get; set; }
    [Parameter] public WaHourFormat? HourFormat { get; set; }

    // Intl.DateTimeFormat options
    [Parameter] public WaDateTimeStyle? Weekday { get; set; }
    [Parameter] public WaDateTimeStyle? Era { get; set; }
    [Parameter] public WaDateTimeStyle? Year { get; set; }
    [Parameter] public WaDateTimeStyle? Month { get; set; }
    [Parameter] public WaDateTimeStyle? Day { get; set; }
    [Parameter] public WaDateTimeStyle? Hour { get; set; }
    [Parameter] public WaDateTimeStyle? Minute { get; set; }
    [Parameter] public WaDateTimeStyle? Second { get; set; }
    [Parameter] public WaDateTimeStyle? TimeZoneName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-format-date");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "date", Date);
        builder.AddAttributeIfNotNullOrEmpty(5, "lang", Lang);
        builder.AddAttributeIfNotNull(6, "hour-format", HourFormat?.ToHtmlValue());

        // Add formatting options
        builder.AddAttributeIfNotNull(10, "weekday", Weekday?.ToHtmlValue());
        builder.AddAttributeIfNotNull(11, "era", Era?.ToHtmlValue());
        builder.AddAttributeIfNotNull(12, "year", Year?.ToHtmlValue());
        builder.AddAttributeIfNotNull(13, "month", Month?.ToHtmlValue());
        builder.AddAttributeIfNotNull(14, "day", Day?.ToHtmlValue());
        builder.AddAttributeIfNotNull(15, "hour", Hour?.ToHtmlValue());
        builder.AddAttributeIfNotNull(16, "minute", Minute?.ToHtmlValue());
        builder.AddAttributeIfNotNull(17, "second", Second?.ToHtmlValue());
        builder.AddAttributeIfNotNull(18, "time-zone-name", TimeZoneName?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __formatDateReference => Element = __formatDateReference);

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
