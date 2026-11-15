using System;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// One action of <see cref="WaDataGridFormatters.Actions(WaDataGridAction[])"/> or
/// <see cref="WaDataGridFormatters.Menu(WaDataGridAction[])"/>: a button in the cell, or an item of the menu of the
/// cell. A click raises the cell action <see cref="Id"/> (see <see cref="WaDataGridCellActionEventArgs"/>).
/// </summary>
public sealed record WaDataGridAction
{
    /// <summary>
    /// The action id that the cell action event reports, for example <c>delete</c>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The text of the action: the text of a menu item or a text button, and the accessible name of an icon button.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The icon of the action. A button with an icon shows the icon only, with <see cref="Label"/> as its accessible
    /// name; a menu item shows the icon in front of the label.
    /// </summary>
    public WaDataGridIcon? Icon { get; init; }

    /// <summary>
    /// The variant of the button. A menu item uses only <see cref="WaVariant.Danger"/>.
    /// </summary>
    public WaVariant? Variant { get; init; }

    /// <summary>
    /// A tooltip of the button; not used in the menu.
    /// </summary>
    public string? Tooltip { get; init; }

    /// <summary>
    /// The path of a row field: the action is disabled in the rows where it holds a true value.
    /// </summary>
    public string? DisabledField { get; init; }

    /// <summary>
    /// The path of a row field: the action is not shown in the rows where it holds a true value, for example
    /// <c>"locked"</c> for a delete action.
    /// </summary>
    public string? HiddenField { get; init; }

    /// <summary>
    /// Whether the action is an item of the menu of the cell instead of a button.
    /// </summary>
    public bool InMenu { get; init; }

    #region ------ Constructors ------

    /// <summary>
    /// Creates an action.
    /// </summary>
    /// <param name="id">The action id, for example <c>delete</c></param>
    /// <param name="label">The text of the action, also the accessible name of an icon button</param>
    /// <exception cref="ArgumentException">Thrown when the id is not a valid action id or the label is empty</exception>
    public WaDataGridAction(string id, string label)
    {
        PresetArgs.ValidateActionId(id, nameof(id));
        PresetArgs.ValidateLabel(label, nameof(label));
        Id = id;
        Label = label;
    }

    #endregion
}
