using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Builds <see cref="WaDataGridColumn.FilterOptions"/> from display texts (design section 7.4): the value picker of a set
/// filter then shows the same labels as the cells of a lookup or badge column, while it filters by the codes.
/// </summary>
public static class WaDataGridFilterOptions
{
    /// <summary>
    /// One option per label, in the order of the map.
    /// </summary>
    /// <param name="labels">The labels, keyed by the value as text</param>
    /// <returns>The filter options</returns>
    /// <exception cref="ArgumentNullException">Thrown when labels is null</exception>
    /// <exception cref="ArgumentException">Thrown when a label is null</exception>
    public static IReadOnlyList<WaDataGridFilterOption> FromLabels(IReadOnlyDictionary<string, string> labels)
    {
        if (labels is null) throw new ArgumentNullException(nameof(labels));

        return PresetArgs.CopyTexts(labels, nameof(labels))!
            .Select(label => new WaDataGridFilterOption(label.Key, label.Value))
            .ToArray();
    }

    /// <summary>
    /// One option per declared value of an enum, in the order of the values. The values are the JSON form of each value
    /// (its number, or its name with a JSON converter attribute), as the row data holds them.
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the column values</typeparam>
    /// <param name="label">The label of a value, for example from the resources of the app; the picker shows the value
    /// when null or when it returns null</param>
    /// <returns>The filter options</returns>
    public static IReadOnlyList<WaDataGridFilterOption> FromEnum<TEnum>(Func<TEnum, string?>? label = null) where TEnum : struct, Enum
    {
        return EnumKeys.ValuesOf<TEnum>()
            .Select(value => new WaDataGridFilterOption(EnumKeys.Of(value), label?.Invoke(value)))
            .ToArray();
    }

    /// <summary>
    /// One option per label of a lookup, badge, tag, tags or icon preset of <see cref="WaDataGridFormatters"/>, so the
    /// picker shows the texts of the cells.
    /// </summary>
    /// <param name="formatter">The formatter of the column</param>
    /// <returns>The filter options</returns>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null</exception>
    /// <exception cref="ArgumentException">Thrown when the formatter is not such a preset or has no labels</exception>
    public static IReadOnlyList<WaDataGridFilterOption> FromFormatter(WaJsFunction formatter)
    {
        if (formatter is null) throw new ArgumentNullException(nameof(formatter));
        if (PresetArgs.LabelsOf(formatter) is not { } labels)
            throw new ArgumentException($"The formatter {formatter} is not a lookup, badge, tag, tags or icon preset with labels.", nameof(formatter));

        return labels.Labels.Select(label => new WaDataGridFilterOption(label.Key, label.Value)).ToArray();
    }
}
