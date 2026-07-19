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

    // Date/time properties
    /// <summary>
    /// The date/time to format. If not set, the current date and time is used. When passing a string, it's
    /// strongly recommended to use the ISO 8601 format to ensure time zones are handled correctly.
    /// </summary>
    [Parameter] public string? Date { get; set; }

    /// <summary>
    /// The locale (BCP 47 language tag) to use when formatting the date. When unset, the browser's default locale is used.
    /// </summary>
    [Parameter] public string? Lang { get; set; }

    /// <summary>
    /// Whether to use 12-hour or 24-hour time when displaying the hour.
    /// </summary>
    [Parameter] public WaHourFormat? HourFormat { get; set; }

    /// <summary>
    /// The time zone to express the date/time in, e.g. "America/New_York". When unset, the browser's default
    /// time zone is used.
    /// </summary>
    [Parameter] public string? TimeZone { get; set; }

    // Intl.DateTimeFormat options
    /// <summary>
    /// The format for displaying the weekday.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Weekday { get; set; }

    /// <summary>
    /// The format for displaying the era.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Era { get; set; }

    /// <summary>
    /// The format for displaying the year.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Year { get; set; }

    /// <summary>
    /// The format for displaying the month.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Month { get; set; }

    /// <summary>
    /// The format for displaying the day.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Day { get; set; }

    /// <summary>
    /// The format for displaying the hour.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Hour { get; set; }

    /// <summary>
    /// The format for displaying the minute.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Minute { get; set; }

    /// <summary>
    /// The format for displaying the second.
    /// </summary>
    [Parameter] public WaDateTimeStyle? Second { get; set; }

    /// <summary>
    /// The format for displaying the time zone name.
    /// </summary>
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
        builder.AddAttributeIfNotNullOrEmpty(7, "time-zone", TimeZone);

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
