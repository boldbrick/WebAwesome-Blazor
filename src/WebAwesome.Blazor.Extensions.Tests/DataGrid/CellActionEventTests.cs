using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The cell action event needs three coordinated pieces, and a missing one fails silently (the handler never runs): the
/// [EventHandler] declaration that lets Razor compile <c>@onwablazor-cell-action</c>, the registration of the event type
/// with Blazor in the JS initializer of this package, and the browser event that the presets dispatch under the same
/// name with a payload that matches <see cref="WaDataGridCellActionEventArgs"/>. The main package's
/// EventBindingRegistrationTests read only the main initializer, so these tests check this package.
/// </summary>
public class CellActionEventTests
{
    [Fact]
    public void EventHandler_DeclaresTheAttributeTheArgsAndBothDirectives()
    {
        var declaration = typeof(EventHandlers).GetCustomAttribute<EventHandlerAttribute>();

        Assert.NotNull(declaration);
        Assert.Equal("onwablazor-cell-action", declaration.AttributeName);
        Assert.Equal("on" + EventHandlers.CellActionEventName, declaration.AttributeName);
        Assert.Equal(typeof(WaDataGridCellActionEventArgs), declaration.EventArgsType);
        Assert.True(declaration.EnableStopPropagation);
        Assert.True(declaration.EnablePreventDefault);
        Assert.True(typeof(EventArgs).IsAssignableFrom(declaration.EventArgsType), "Blazor rejects event args that do not inherit EventArgs");
    }

    [Fact]
    public void EventHandler_IsDeclaredOnAPublicClassNamedEventHandlers()
    {
        // the Razor compiler reads [EventHandler] only from public classes named EventHandlers; on another class, Razor
        // passes @onwablazor-cell-action on as a text attribute and the handler never runs
        Assert.Equal("EventHandlers", typeof(EventHandlers).Name);
        Assert.True(typeof(EventHandlers).IsPublic);
    }

    [Fact]
    public void Presets_DispatchTheEventUnderTheDeclaredName()
    {
        var presets = PresetScripts.Read(PresetScripts.PresetsFileName);
        var name = Regex.Match(presets, @"export const cellActionEventName = '([^']+)';");

        Assert.True(name.Success, "data-grid-presets.js does not export cellActionEventName");
        Assert.Equal(EventHandlers.CellActionEventName, name.Groups[1].Value);
        Assert.Contains("new CustomEvent(cellActionEventName, {", presets);
        Assert.Matches(@"new CustomEvent\(cellActionEventName, \{\s*bubbles: true,\s*composed: true,", presets);
    }

    [Fact]
    public void Initializer_RegistersTheEventTypeWithBlazor()
    {
        var initializer = PresetScripts.Read(PresetScripts.InitializerFileName);

        Assert.Matches(@"import \{[^}]*\bcellActionEventName\b[^}]*\} from '\./data-grid-presets\.js';", initializer);
        Assert.Matches(@"registerCustomEventType\(cellActionEventName, \{\s*createEventArgs: cellActionArgs,", initializer);

        // both start callbacks register it: afterWebStarted for Blazor Web, afterStarted for the classic hosts
        foreach (var callback in new[] { "afterWebStarted", "afterStarted" })
        {
            Assert.Matches($@"export function {callback}\(blazor\) \{{[^}}]*registerEventTypes\(blazor\);", initializer);
        }
    }

    [Fact]
    public void EventArgsPayload_HasAKeyForEachProperty()
    {
        var presets = PresetScripts.Read(PresetScripts.PresetsFileName);
        var body = Regex.Match(presets, @"export function cellActionArgs\(event\) \{(?<body>.*?)\n\}", RegexOptions.Singleline);
        Assert.True(body.Success, "data-grid-presets.js does not export cellActionArgs");

        var returned = Regex.Match(body.Groups["body"].Value, @"return \{(?<keys>.*?)\};", RegexOptions.Singleline).Groups["keys"].Value;
        var keys = Regex.Matches(returned, @"^\s*(\w+)(?=[,:])", RegexOptions.Multiline).Select(match => match.Groups[1].Value).ToHashSet();
        var properties = typeof(WaDataGridCellActionEventArgs).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name))
            .ToHashSet();

        Assert.Equal(properties.Order(), keys.Order());
    }

    [Fact]
    public void EventArgs_ReadTheRowAndThePayloadAsTypes()
    {
        // the JSON that the initializer's createEventArgs builds, read as Blazor reads event args (web defaults)
        const string json = """
            {"column":"id","action":"open","rowKey":42,"row":{"id":42,"customer":"Ada","state":2,"due":"2026-10-15"},
             "value":42,"payload":{"reason":"test","count":3},"rowIndex":7}
            """;
        var args = JsonSerializer.Deserialize<WaDataGridCellActionEventArgs>(json, JsonSerializerOptions.Web)!;

        Assert.Equal("id", args.Column);
        Assert.Equal("open", args.Action);
        Assert.Equal(42, args.RowKey!.Value.GetInt32());
        Assert.Equal(7, args.RowIndex);
        Assert.Equal(new OrderRow(42, "Ada", OrderState.Active, new DateOnly(2026, 10, 15)), args.GetRow<OrderRow>());
        Assert.Equal(new ActionPayload("test", 3), args.GetPayload<ActionPayload>());
    }

    [Fact]
    public void EventArgs_WithoutRowOrPayload_GiveTheDefaults()
    {
        var args = JsonSerializer.Deserialize<WaDataGridCellActionEventArgs>("""{"column":"c","action":"a","row":null,"payload":null}""", JsonSerializerOptions.Web)!;

        Assert.Null(args.GetRow<OrderRow>());
        Assert.Null(args.GetPayload<ActionPayload>());
        Assert.Equal(WaDataGridCellActionEventArgs.UnknownRowIndex, args.RowIndex);
        Assert.Null(new WaDataGridCellActionEventArgs().GetPayload<ActionPayload>());
    }

    #region ------ Internals ------

    private enum OrderState
    {
        New = 1,
        Active = 2,
    }

    private sealed record OrderRow(int Id, string Customer, OrderState State, DateOnly Due);

    private sealed record ActionPayload(string Reason, int Count);

    #endregion
}
