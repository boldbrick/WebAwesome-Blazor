using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Sort presets for <see cref="WaDataGridColumn.ComparatorFunction"/>. Each factory returns a <see cref="WaJsFunction"/>
/// reference to a comparator that the JS initializer of the Extensions package registers.
/// </summary>
public static class WaDataGridComparators
{
    /// <summary>
    /// Sorts by the position of the value in a list, for example the states of a workflow (<c>new</c> before
    /// <c>active</c> before <c>done</c>) instead of their alphabetic order. A value that is not in the list, and a null
    /// value, sorts after the listed values; such values sort among themselves by their text.
    /// </summary>
    /// <param name="values">The values in their sort order, as text (<c>String(value)</c> in the browser)</param>
    /// <returns>The comparator reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when a value is null or occurs twice</exception>
    public static WaJsFunction Ordinal(params string[] values)
    {
        return Ordinal((IEnumerable<string>)values);
    }

    /// <summary>
    /// Sorts by the position of the value in a list (see <see cref="Ordinal(string[])"/>).
    /// </summary>
    /// <param name="values">The values in their sort order, as text</param>
    /// <returns>The comparator reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when a value is null or occurs twice</exception>
    public static WaJsFunction Ordinal(IEnumerable<string> values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));

        var order = values.ToArray();
        if (order.Any(value => value is null))
            throw new ArgumentException("A value of the sort order must not be null.", nameof(values));
        if (order.Distinct(StringComparer.Ordinal).Count() != order.Length)
            throw new ArgumentException("A value occurs twice in the sort order.", nameof(values));

        return new PresetArgs()
            .Set(ValuesKey, order)
            .ToFunction(PresetNames.Ordinal);
    }

    /// <summary>
    /// Sorts the values of an enum in a given order. The keys are the JSON form of each value: its number, or its name
    /// when the enum has a JSON converter attribute such as <c>[JsonConverter(typeof(JsonStringEnumConverter))]</c>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the column values</typeparam>
    /// <param name="order">The values in their sort order; when empty, the declared values in the order of their
    /// numbers</param>
    /// <returns>The comparator reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when order is null</exception>
    /// <exception cref="ArgumentException">Thrown when a value occurs twice</exception>
    public static WaJsFunction Ordinal<TEnum>(params TEnum[] order) where TEnum : struct, Enum
    {
        if (order is null) throw new ArgumentNullException(nameof(order));

        var values = order.Length == 0 ? EnumKeys.ValuesOf<TEnum>() : order;
        return Ordinal(values.Select(EnumKeys.Of));
    }

    #region ------ Internals ------

    // the option name that the comparator in data-grid-presets.js reads
    private const string ValuesKey = "values";

    #endregion
}
