using System;
using System.Linq;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// One rule of <see cref="WaDataGridClassRules"/>: when <see cref="Condition"/> matches, the cell or the row gets
/// <see cref="ClassName"/>. Style the class with <c>ShadowStyles</c> of the grid: the cells and rows are in its shadow
/// root, where the styles of the page do not apply.
/// </summary>
public sealed record WaDataGridClassRule
{
    /// <summary>
    /// The class, or several classes separated by spaces.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// The condition.
    /// </summary>
    public WaDataGridCondition Condition { get; }

    /// <summary>
    /// The path of the row field that the condition tests, for example <c>state</c>. A cell rule tests the value of its
    /// cell when it is null; a row rule needs it.
    /// </summary>
    public string? Field { get; init; }

    #region ------ Constructors ------

    /// <summary>
    /// Creates a rule.
    /// </summary>
    /// <param name="className">The class, or several classes separated by spaces</param>
    /// <param name="condition">The condition</param>
    /// <exception cref="ArgumentNullException">Thrown when condition is null</exception>
    /// <exception cref="ArgumentException">Thrown when the class name is empty, has leading or trailing whitespace or a
    /// control character</exception>
    public WaDataGridClassRule(string className, WaDataGridCondition condition)
    {
        if (string.IsNullOrWhiteSpace(className) || className.Trim().Length != className.Length || className.Any(char.IsControl))
            throw new ArgumentException($"'{className}' is not a class name: it must be non-empty, without leading or trailing whitespace and without control characters.", nameof(className));

        ClassName = className;
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    #endregion
}
