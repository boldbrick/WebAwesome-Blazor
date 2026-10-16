using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

// interactive presets (design section 6.4): elements that raise cell actions
public static partial class WaDataGridFormatters
{
    /// <summary>
    /// The default action id of <see cref="Switch"/> and <see cref="Checkbox"/>.
    /// </summary>
    public const string DefaultToggleAction = "toggle";

    /// <summary>
    /// The accessible name of the menu trigger of <see cref="Actions(IReadOnlyList{WaDataGridAction}, WaSize?, WaAppearance?, string?, string?)"/>
    /// and <see cref="Menu(IReadOnlyList{WaDataGridAction}, string?, WaSize?, WaAppearance?, string?)"/> when no label
    /// is given.
    /// </summary>
    public const string DefaultMenuLabel = "More actions";

    /// <summary>
    /// A <c>wa-button</c> that raises a cell action. Handle it with <c>@onwablazor-cell-action</c> on the
    /// <see cref="WaDataGrid"/>; a click on the button does not raise <c>OnCellClick</c>. Enter and Space on the focused
    /// button click it. Output: an element.
    /// </summary>
    /// <param name="action">The action id that the event reports, for example <c>edit</c></param>
    /// <param name="label">The button text, a template as in <see cref="Template"/>; the value of the cell when null.
    /// With <paramref name="iconOnly"/>, it is the accessible name.</param>
    /// <param name="icon">An icon in front of the text</param>
    /// <param name="iconOnly">Whether the button shows the icon only</param>
    /// <param name="variant">The variant of the button</param>
    /// <param name="appearance">The appearance of the button</param>
    /// <param name="size">The size of the button</param>
    /// <param name="disabledField">The path of a row field: the button is disabled in the rows where it holds a true
    /// value</param>
    /// <param name="hiddenField">The path of a row field: the cell is empty in the rows where it holds a true value</param>
    /// <param name="nullText">The text of a cell without a button; empty by default</param>
    /// <param name="culture">The culture of the formats of the label template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the action id, the label template or a field path is not valid,
    /// or iconOnly is set without an icon</exception>
    public static WaJsFunction Button(string action, string? label = null, WaDataGridIcon? icon = null, bool iconOnly = false,
        WaVariant? variant = null, WaAppearance? appearance = null, WaSize? size = null, string? disabledField = null,
        string? hiddenField = null, string? nullText = null, CultureInfo? culture = null)
    {
        PresetArgs.ValidateActionId(action, nameof(action));
        if (label is not null) CellTemplate.ValidateText(label, nameof(label));
        if (iconOnly && icon is null) throw new ArgumentException("A button that shows the icon only needs an icon.", nameof(iconOnly));
        ValidateOptionalField(disabledField, nameof(disabledField));
        ValidateOptionalField(hiddenField, nameof(hiddenField));

        var args = new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(ActionKey, action)
            .Set(LabelKey, label)
            .Set(IconKey, PresetArgs.IconOrNull(icon))
            .Set(IconOnlyKey, iconOnly ? true : null)
            .Set(VariantKey, variant?.ToHtmlValue())
            .Set(AppearanceKey, appearance?.ToHtmlValue())
            .Set(SizeKey, size?.ToHtmlValue())
            .Set(DisabledFieldKey, disabledField)
            .Set(HiddenFieldKey, hiddenField);

        return icon is null ? args.ToFunction(PresetNames.Button, ButtonTag) : args.ToFunction(PresetNames.Button, ButtonTag, IconTag);
    }

    /// <summary>
    /// Action buttons in a cell, for example edit and delete: each <see cref="WaDataGridAction"/> is an icon button (or
    /// a text button without an icon), and the actions with <see cref="WaDataGridAction.InMenu"/> are the items of one
    /// menu behind a trigger button. Each click raises the action id of its action. Output: an element.
    /// </summary>
    /// <param name="actions">The actions, in their order</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when actions is null</exception>
    /// <exception cref="ArgumentException">Thrown when there are no actions, an action is null, two actions have one id,
    /// or a field path is not valid</exception>
    public static WaJsFunction Actions(params WaDataGridAction[] actions)
    {
        return Actions((IReadOnlyList<WaDataGridAction>)actions);
    }

    /// <summary>
    /// Action buttons in a cell, with options (see <see cref="Actions(WaDataGridAction[])"/>). Output: an element.
    /// </summary>
    /// <param name="actions">The actions, in their order</param>
    /// <param name="size">The size of the buttons</param>
    /// <param name="appearance">The appearance of the buttons, for example <see cref="WaAppearance.Plain"/></param>
    /// <param name="menuLabel">The accessible name of the menu trigger; <see cref="DefaultMenuLabel"/> when null</param>
    /// <param name="nullText">The text of a cell whose actions are all hidden; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when actions is null</exception>
    /// <exception cref="ArgumentException">Thrown when there are no actions, an action is null, two actions have one id,
    /// or a field path is not valid</exception>
    public static WaJsFunction Actions(IReadOnlyList<WaDataGridAction> actions, WaSize? size = null, WaAppearance? appearance = null,
        string? menuLabel = null, string? nullText = null)
    {
        var items = ActionItems(actions, nameof(actions));
        var hasMenu = actions.Any(action => action.InMenu);
        var tags = new List<string> { ButtonTag };
        if (hasMenu || actions.Any(action => action.Icon is not null)) tags.Add(IconTag);
        if (actions.Any(action => !action.InMenu && action.Tooltip is not null)) tags.Add(TooltipTag);
        if (hasMenu) tags.AddRange([DropdownTag, DropdownItemTag]);

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(ItemsKey, items)
            .Set(SizeKey, size?.ToHtmlValue())
            .Set(AppearanceKey, appearance?.ToHtmlValue())
            .Set(MenuLabelKey, hasMenu ? MenuLabel(menuLabel) : null)
            .ToFunction(PresetNames.Actions, [.. tags]);
    }

    /// <summary>
    /// A menu of actions behind one trigger button (a vertical ellipsis): each <see cref="WaDataGridAction"/> is a
    /// <c>wa-dropdown-item</c>, and the selection raises its action id. A click on the trigger or in the menu does not
    /// raise <c>OnCellClick</c>. Output: an element.
    /// </summary>
    /// <param name="items">The items, in their order</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when items is null</exception>
    /// <exception cref="ArgumentException">Thrown when there are no items, an item is null, two items have one id, or a
    /// field path is not valid</exception>
    public static WaJsFunction Menu(params WaDataGridAction[] items)
    {
        return Menu((IReadOnlyList<WaDataGridAction>)items);
    }

    /// <summary>
    /// A menu of actions behind one trigger button, with options (see <see cref="Menu(WaDataGridAction[])"/>). Output:
    /// an element.
    /// </summary>
    /// <param name="items">The items, in their order</param>
    /// <param name="menuLabel">The accessible name of the trigger; <see cref="DefaultMenuLabel"/> when null</param>
    /// <param name="size">The size of the trigger button</param>
    /// <param name="appearance">The appearance of the trigger button</param>
    /// <param name="nullText">The text of a cell whose items are all hidden; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when items is null</exception>
    /// <exception cref="ArgumentException">Thrown when there are no items, an item is null, two items have one id, or a
    /// field path is not valid</exception>
    public static WaJsFunction Menu(IReadOnlyList<WaDataGridAction> items, string? menuLabel = null, WaSize? size = null,
        WaAppearance? appearance = null, string? nullText = null)
    {
        return new PresetArgs()
            .SetNullText(nullText)
            .Set(ItemsKey, ActionItems(items, nameof(items)))
            .Set(SizeKey, size?.ToHtmlValue())
            .Set(AppearanceKey, appearance?.ToHtmlValue())
            .Set(MenuLabelKey, MenuLabel(menuLabel))
            .ToFunction(PresetNames.Menu, DropdownTag, DropdownItemTag, ButtonTag, IconTag);
    }

    /// <summary>
    /// A <c>wa-switch</c> for a boolean value. A change raises the cell action with the payload <c>{ "checked": bool }</c>
    /// (read it with <c>GetPayload&lt;T&gt;()</c>); a click on the switch does not raise <c>OnCellClick</c>. Output: an
    /// element.
    /// </summary>
    /// <param name="label">The accessible name of the switch, a template as in <see cref="Template"/>, for example
    /// <c>"Active: {name}"</c></param>
    /// <param name="action">The action id that the event reports</param>
    /// <param name="showLabel">Whether the label shows next to the switch; else only assistive technology reads it</param>
    /// <param name="disabledField">The path of a row field: the switch is disabled in the rows where it holds a true
    /// value</param>
    /// <param name="size">The size of the switch</param>
    /// <param name="culture">The culture of the formats of the label template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the label template, the action id or the field path is not
    /// valid</exception>
    /// <remarks>The switch shows the new state at once, but the row in .NET keeps the old value: update the row in the
    /// handler and push <c>Data</c> again. To reject the change, push the unchanged data; the cell then shows the old
    /// state again.</remarks>
    public static WaJsFunction Switch(string label, string action = DefaultToggleAction, bool showLabel = false,
        string? disabledField = null, WaSize? size = null, CultureInfo? culture = null)
    {
        return ToggleArgs(label, action, showLabel, disabledField, size, culture).ToFunction(PresetNames.Switch, SwitchTag);
    }

    /// <summary>
    /// A <c>wa-checkbox</c> for a boolean value. A change raises the cell action with the payload
    /// <c>{ "checked": bool }</c>; a click on the checkbox does not raise <c>OnCellClick</c>. Output: an element.
    /// </summary>
    /// <param name="label">The accessible name of the checkbox, a template as in <see cref="Template"/></param>
    /// <param name="action">The action id that the event reports</param>
    /// <param name="showLabel">Whether the label shows next to the checkbox; else only assistive technology reads
    /// it</param>
    /// <param name="disabledField">The path of a row field: the checkbox is disabled in the rows where it holds a true
    /// value</param>
    /// <param name="size">The size of the checkbox</param>
    /// <param name="culture">The culture of the formats of the label template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the label template, the action id or the field path is not
    /// valid</exception>
    /// <remarks>As for <see cref="Switch"/>: update the row in the handler and push <c>Data</c> again.</remarks>
    public static WaJsFunction Checkbox(string label, string action = DefaultToggleAction, bool showLabel = false,
        string? disabledField = null, WaSize? size = null, CultureInfo? culture = null)
    {
        return ToggleArgs(label, action, showLabel, disabledField, size, culture).ToFunction(PresetNames.Checkbox, CheckboxTag);
    }

    #region ------ Internals ------

    // the option names of the interactive presets
    private const string LabelKey = "label";
    private const string IconKey = "icon";
    private const string IconOnlyKey = "iconOnly";
    private const string VariantKey = "variant";
    private const string DisabledFieldKey = "disabledField";
    private const string HiddenFieldKey = "hiddenField";
    private const string ItemsKey = "items";
    private const string MenuLabelKey = "menuLabel";
    private const string ShowLabelKey = "showLabel";

    // the option names of one action item
    private const string ItemIdKey = "id";
    private const string ItemLabelKey = "label";
    private const string ItemIconKey = "icon";
    private const string ItemVariantKey = "variant";
    private const string ItemTooltipKey = "tooltip";
    private const string ItemDisabledFieldKey = "disabledField";
    private const string ItemHiddenFieldKey = "hiddenField";
    private const string ItemInMenuKey = "inMenu";

    // the elements the interactive presets render
    private const string ButtonTag = "wa-button";
    private const string TooltipTag = "wa-tooltip";
    private const string DropdownTag = "wa-dropdown";
    private const string DropdownItemTag = "wa-dropdown-item";
    private const string SwitchTag = "wa-switch";
    private const string CheckboxTag = "wa-checkbox";

    private static void ValidateOptionalField(string? path, string parameterName)
    {
        if (path is not null) CellTemplate.ValidateFieldPath(path, parameterName);
    }

    private static string MenuLabel(string? menuLabel)
    {
        if (menuLabel is null) return DefaultMenuLabel;

        PresetArgs.ValidateLabel(menuLabel, nameof(menuLabel));
        return menuLabel;
    }

    // the items of the actions and menu presets, as the preset functions read them
    private static List<Dictionary<string, object>> ActionItems(IReadOnlyList<WaDataGridAction> actions, string parameterName)
    {
        if (actions is null) throw new ArgumentNullException(parameterName);
        if (actions.Count == 0) throw new ArgumentException("Give at least one action.", parameterName);
        if (actions.Any(action => action is null)) throw new ArgumentException("An action must not be null.", parameterName);
        if (actions.Select(action => action.Id).Distinct(StringComparer.Ordinal).Count() != actions.Count)
            throw new ArgumentException("Two actions have the same id.", parameterName);

        var items = new List<Dictionary<string, object>>();
        foreach (var action in actions)
        {
            ValidateOptionalField(action.DisabledField, parameterName);
            ValidateOptionalField(action.HiddenField, parameterName);

            var item = new Dictionary<string, object> { [ItemIdKey] = action.Id, [ItemLabelKey] = action.Label };
            if (action.Icon is not null) item[ItemIconKey] = PresetArgs.IconOrNull(action.Icon)!;
            if (action.Variant is not null) item[ItemVariantKey] = action.Variant.Value.ToHtmlValue();
            if (action.Tooltip is not null) item[ItemTooltipKey] = action.Tooltip;
            if (action.DisabledField is not null) item[ItemDisabledFieldKey] = action.DisabledField;
            if (action.HiddenField is not null) item[ItemHiddenFieldKey] = action.HiddenField;
            if (action.InMenu) item[ItemInMenuKey] = true;
            items.Add(item);
        }

        return items;
    }

    private static PresetArgs ToggleArgs(string label, string action, bool showLabel, string? disabledField, WaSize? size,
        CultureInfo? culture)
    {
        CellTemplate.ValidateText(label, nameof(label));
        PresetArgs.ValidateActionId(action, nameof(action));
        ValidateOptionalField(disabledField, nameof(disabledField));

        return new PresetArgs()
            .SetCulture(culture)
            .Set(LabelKey, label)
            .Set(ActionKey, action)
            .Set(ShowLabelKey, showLabel ? true : null)
            .Set(DisabledFieldKey, disabledField)
            .Set(SizeKey, size?.ToHtmlValue());
    }

    #endregion
}
