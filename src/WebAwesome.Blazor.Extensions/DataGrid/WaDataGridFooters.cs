using System;
using System.Globalization;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Footer aggregates for <see cref="WaDataGridColumn.FooterFunction"/> (design section 7.2): the sum, average, lowest or
/// highest value of the column, or the number of rows, over the filtered rows of every page, as the grid gives them to a
/// footer (in server mode, the loaded page). The values are those of the column: its <c>ValueFunction</c>, else its
/// <c>Field</c>; values that are not numbers are left out. Output: text.
/// </summary>
/// <remarks>
/// <c>format</c> is a text preset of <see cref="WaDataGridFormatters"/> (for example
/// <c>WaDataGridFormatters.Currency("EUR")</c>); without it, the result is a localized number. <c>label</c> goes in front
/// of the result, separated by a space, for example "Total 1,234.50".
/// </remarks>
public static class WaDataGridFooters
{
    /// <summary>
    /// The sum of the values of the column.
    /// </summary>
    /// <param name="format">The text format of the result, a text preset; a localized number when null</param>
    /// <param name="label">A text in front of the result</param>
    /// <param name="nullText">The text of a footer without a result; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.FooterFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Sum(WaJsFunction? format = null, string? label = null, string? nullText = null, CultureInfo? culture = null)
        => Aggregate(SumAggregate, format, label, nullText, culture);

    /// <summary>
    /// The average of the values of the column; no result when there is no value.
    /// </summary>
    /// <param name="format">The text format of the result, a text preset; a localized number when null</param>
    /// <param name="label">A text in front of the result</param>
    /// <param name="nullText">The text of a footer without a result; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.FooterFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Average(WaJsFunction? format = null, string? label = null, string? nullText = null, CultureInfo? culture = null)
        => Aggregate(AverageAggregate, format, label, nullText, culture);

    /// <summary>
    /// The lowest value of the column; no result when there is no value.
    /// </summary>
    /// <param name="format">The text format of the result, a text preset; a localized number when null</param>
    /// <param name="label">A text in front of the result</param>
    /// <param name="nullText">The text of a footer without a result; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.FooterFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Min(WaJsFunction? format = null, string? label = null, string? nullText = null, CultureInfo? culture = null)
        => Aggregate(MinAggregate, format, label, nullText, culture);

    /// <summary>
    /// The highest value of the column; no result when there is no value.
    /// </summary>
    /// <param name="format">The text format of the result, a text preset; a localized number when null</param>
    /// <param name="label">A text in front of the result</param>
    /// <param name="nullText">The text of a footer without a result; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.FooterFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Max(WaJsFunction? format = null, string? label = null, string? nullText = null, CultureInfo? culture = null)
        => Aggregate(MaxAggregate, format, label, nullText, culture);

    /// <summary>
    /// The number of rows.
    /// </summary>
    /// <param name="format">The text format of the result, a text preset; a localized number when null</param>
    /// <param name="label">A text in front of the result</param>
    /// <param name="nullText">The text of a footer without a result; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.FooterFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Count(WaJsFunction? format = null, string? label = null, string? nullText = null, CultureInfo? culture = null)
        => Aggregate(CountAggregate, format, label, nullText, culture);

    #region ------ Internals ------

    // the option names of the footer preset, and its aggregates (data-grid-presets.js, footerAggregate)
    private const string AggregateKey = "aggregate";
    private const string FormatKey = "format";
    private const string LabelKey = "label";
    private const string SumAggregate = "sum";
    private const string AverageAggregate = "average";
    private const string MinAggregate = "min";
    private const string MaxAggregate = "max";
    private const string CountAggregate = "count";

    private static WaJsFunction Aggregate(string aggregate, WaJsFunction? format, string? label, string? nullText, CultureInfo? culture)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(AggregateKey, aggregate)
            .Set(FormatKey, PresetArgs.TextFormatOrNull(format, nameof(format)))
            .Set(LabelKey, label)
            .ToFunction(PresetNames.FooterAggregate);
    }

    #endregion
}
