using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Declares the Razor event attributes of the data grid extensions. <c>@onwablazor-cell-action</c> binds the cell
/// actions of a grid (see <see cref="WaDataGridCellActionEventArgs"/>):
/// <code>&lt;WaDataGrid Data="orders" Columns="columns" @onwablazor-cell-action="HandleCellAction" /&gt;</code>
/// <c>WaDataGrid</c> passes the attribute to its element through its additional attributes. The JS initializer of the
/// Extensions package registers the event type with Blazor. Import the namespace
/// (<c>@using WebAwesome.Blazor.Extensions.DataGrid</c>), so that Razor knows the attribute.
/// </summary>
/// <remarks>
/// The Razor compiler reads <see cref="EventHandlerAttribute"/> declarations only from public classes with the name
/// <c>EventHandlers</c>, as <c>Microsoft.AspNetCore.Components.Web.EventHandlers</c> declares <c>@onclick</c>. Without
/// the declaration, Razor passes <c>@onwablazor-cell-action="Handler"</c> on as a plain text attribute and the handler
/// never runs.
/// </remarks>
[EventHandler(CellActionAttributeName, typeof(WaDataGridCellActionEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
    /// <summary>
    /// The name of the browser event of a cell action, dispatched on the grid element.
    /// </summary>
    public const string CellActionEventName = "wablazor-cell-action";

    /// <summary>
    /// The Razor attribute of a cell action handler: <c>on</c> and <see cref="CellActionEventName"/>.
    /// </summary>
    public const string CellActionAttributeName = "on" + CellActionEventName;
}
