using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Class rules for cells and rows (design section 7.1): a list of <see cref="WaDataGridClassRule"/>, and the class of
/// the first rule that matches. <see cref="Cell(WaDataGridClassRule[])"/> is a <see cref="WaDataGridColumn.CellClassFunction"/>,
/// <see cref="Row(WaDataGridClassRule[])"/> a <see cref="WaDataGrid.RowClassFunction"/>. Style the classes with
/// <see cref="WaDataGrid.ShadowStyles"/>:
/// <code>
/// CellClassFunction = WaDataGridClassRules.Cell(
///     new("stock-out", WaDataGridCondition.LessThan(1)),
///     new("stock-low", WaDataGridCondition.Between(1, 9)))
/// </code>
/// </summary>
public static class WaDataGridClassRules
{
    /// <summary>
    /// The class of a cell: the class of the first rule whose condition matches the value of the cell (or the row field
    /// of the rule), else no class.
    /// </summary>
    /// <param name="rules">The rules, in their order</param>
    /// <returns>The reference for <see cref="WaDataGridColumn.CellClassFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when rules is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no rule, a rule is null, or a field path is not
    /// valid</exception>
    public static WaJsFunction Cell(params WaDataGridClassRule[] rules)
    {
        return new PresetArgs()
            .Set(RulesKey, RulesOf(rules, requireField: false))
            .ToFunction(PresetNames.CellClassRules);
    }

    /// <summary>
    /// The class of a row: the class of the first rule whose condition matches its row field, else no class. Each rule
    /// needs <see cref="WaDataGridClassRule.Field"/>.
    /// </summary>
    /// <param name="rules">The rules, in their order</param>
    /// <returns>The reference for <see cref="WaDataGrid.RowClassFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when rules is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no rule, a rule is null or has no field, or a field path
    /// is not valid</exception>
    public static WaJsFunction Row(params WaDataGridClassRule[] rules)
    {
        return new PresetArgs()
            .Set(RulesKey, RulesOf(rules, requireField: true))
            .ToFunction(PresetNames.RowClassRules);
    }

    #region ------ Internals ------

    // the option names of the rule presets
    private const string RulesKey = "rules";
    private const string ClassKey = "class";
    private const string FieldKey = "field";

    private static List<Dictionary<string, object>> RulesOf(WaDataGridClassRule[] rules, bool requireField)
    {
        if (rules is null) throw new ArgumentNullException(nameof(rules));
        if (rules.Length == 0) throw new ArgumentException("Give at least one rule.", nameof(rules));
        if (rules.Any(rule => rule is null)) throw new ArgumentException("A rule must not be null.", nameof(rules));

        var result = new List<Dictionary<string, object>>();
        foreach (var rule in rules)
        {
            if (rule.Field is null && requireField)
                throw new ArgumentException($"The row rule for '{rule.ClassName}' has no field: a row rule tests a field of the row.", nameof(rules));
            if (rule.Field is not null) CellTemplate.ValidateFieldPath(rule.Field, nameof(rules));

            var item = new Dictionary<string, object>(StringComparer.Ordinal) { [ClassKey] = rule.ClassName };
            rule.Condition.WriteTo(item);
            if (rule.Field is not null) item[FieldKey] = rule.Field;
            result.Add(item);
        }

        return result;
    }

    #endregion
}
