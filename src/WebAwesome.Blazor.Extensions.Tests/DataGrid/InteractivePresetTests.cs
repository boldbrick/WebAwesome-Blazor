using System;
using System.Collections.Generic;
using System.Globalization;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The descriptors of the interactive presets (design section 6.4): Button, Actions, Menu, Switch and Checkbox. Each
/// writes the options the preset function in data-grid-presets.js reads, and declares the elements it renders, so they
/// are registered before the first render.
/// </summary>
public class InteractivePresetTests
{
    [Fact]
    public void Button_WritesAllOptionsUnderTheirJavaScriptNames()
    {
        var button = WaDataGridFormatters.Button("edit", "Edit #{id}", "pen", iconOnly: true, WaVariant.Brand, WaAppearance.Plain, WaSize.Small,
            "locked", "archived", "-", English);

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.button"},"args":{"culture":"en-US","nullText":"-","action":"edit","label":"Edit #{id}","icon":{"name":"pen"},"iconOnly":true,"variant":"brand","appearance":"plain","size":"s","disabledField":"locked","hiddenField":"archived"},"uses":["wa-button","wa-icon"]}""",
            button.ToString());
    }

    [Fact]
    public void Button_WithoutAnIcon_DeclaresTheButtonOnly()
    {
        var button = WaDataGridFormatters.Button("open", culture: English);

        Assert.Equal("""{"culture":"en-US","action":"open"}""", ArgsOf(button));
        Assert.Equal(["wa-button"], button.Tags);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" open")]
    [InlineData("open\n")]
    public void Button_RejectsAnInvalidActionId(string action)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Button(action));
    }

    [Fact]
    public void Button_RejectsAnInvalidLabelFieldOrIconOnlyWithoutAnIcon()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Button("edit", "{"));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Button("edit", disabledField: "is locked"));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Button("edit", hiddenField: ""));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Button("edit", iconOnly: true));
    }

    [Fact]
    public void Actions_WritesTheItemsInTheirOrderAndDeclaresWhatTheyRender()
    {
        var actions = WaDataGridFormatters.Actions(
        [
            new WaDataGridAction("view", "View") { Icon = "eye", Tooltip = "View it" },
            new WaDataGridAction("delete", "Delete") { Icon = "trash", Variant = WaVariant.Danger, HiddenField = "locked" },
            new WaDataGridAction("export", "Export") { InMenu = true, DisabledField = "row.locked" }
        ], WaSize.Small, WaAppearance.Plain);

        Assert.Equal(
            """{"items":[{"id":"view","label":"View","icon":{"name":"eye"},"tooltip":"View it"},{"id":"delete","label":"Delete","icon":{"name":"trash"},"variant":"danger","hiddenField":"locked"},{"id":"export","label":"Export","disabledField":"row.locked","inMenu":true}],"size":"s","appearance":"plain","menuLabel":"More actions"}""",
            ArgsOf(actions));
        Assert.Equal(["wa-button", "wa-icon", "wa-tooltip", "wa-dropdown", "wa-dropdown-item"], actions.Tags);
        Assert.Equal(PresetName("actions"), actions.Reference);
    }

    [Fact]
    public void Actions_WithoutAMenu_HasNoMenuLabelAndNoDropdown()
    {
        var actions = WaDataGridFormatters.Actions(new WaDataGridAction("open", "Open"));

        Assert.Equal("""{"items":[{"id":"open","label":"Open"}]}""", ArgsOf(actions));
        Assert.Equal(["wa-button"], actions.Tags);
    }

    [Fact]
    public void Actions_RejectNoItemsANullItemAndDuplicateIds()
    {
        Assert.Throws<ArgumentNullException>(() => WaDataGridFormatters.Actions((IReadOnlyList<WaDataGridAction>)null!));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Actions());
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Actions(new WaDataGridAction("a", "A"), null!));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Actions(new WaDataGridAction("a", "A"), new WaDataGridAction("a", "B")));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Actions(new WaDataGridAction("a", "A") { HiddenField = "a b" }));
    }

    [Fact]
    public void Action_RejectsAnInvalidIdOrLabel()
    {
        Assert.Throws<ArgumentException>(() => new WaDataGridAction(" a", "A"));
        Assert.Throws<ArgumentException>(() => new WaDataGridAction("a", ""));
        Assert.Throws<ArgumentException>(() => new WaDataGridAction("a", "A\u0001"));
    }

    [Fact]
    public void Menu_WritesTheItemsTheLabelAndDeclaresTheDropdown()
    {
        var menu = WaDataGridFormatters.Menu([new WaDataGridAction("open", "Open"), new WaDataGridAction("remove", "Remove") { Variant = WaVariant.Danger }],
            "Row actions", WaSize.Small);

        Assert.Equal(
            """{"items":[{"id":"open","label":"Open"},{"id":"remove","label":"Remove","variant":"danger"}],"size":"s","menuLabel":"Row actions"}""",
            ArgsOf(menu));
        Assert.Equal(["wa-dropdown", "wa-dropdown-item", "wa-button", "wa-icon"], menu.Tags);
        Assert.Equal("""{"items":[{"id":"open","label":"Open"}],"menuLabel":"More actions"}""", ArgsOf(WaDataGridFormatters.Menu(new WaDataGridAction("open", "Open"))));
    }

    [Fact]
    public void SwitchAndCheckbox_WriteTheLabelAndTheDefaultToggleAction()
    {
        var toggle = WaDataGridFormatters.Switch("Active: {name}", culture: English);
        var checkbox = WaDataGridFormatters.Checkbox("Done", "complete", showLabel: true, disabledField: "locked", size: WaSize.Large, culture: English);

        Assert.Equal("""{"culture":"en-US","label":"Active: {name}","action":"toggle"}""", ArgsOf(toggle));
        Assert.Equal(["wa-switch"], toggle.Tags);
        Assert.Equal(PresetName("switch"), toggle.Reference);
        Assert.Equal("""{"culture":"en-US","label":"Done","action":"complete","showLabel":true,"disabledField":"locked","size":"l"}""", ArgsOf(checkbox));
        Assert.Equal(["wa-checkbox"], checkbox.Tags);
        Assert.Equal("toggle", WaDataGridFormatters.DefaultToggleAction);
    }

    [Fact]
    public void SwitchAndCheckbox_RejectAnInvalidLabelOrAction()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Switch(""));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Checkbox("Done", " x"));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Checkbox("Done", disabledField: "1x"));
    }

    #region ------ Internals ------

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private static string ArgsOf(WaJsFunction function) => function.Args!.Value.GetRawText();

    private static string PresetName(string name) => "wab.dataGrid." + name;

    #endregion
}
