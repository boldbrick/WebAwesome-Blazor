# Web Awesome Blazor Extensions

Optional extras for the [WebAwesome.Blazor](https://www.nuget.org/packages/WebAwesome.Blazor) bindings. This package holds features that add behavior that the upstream [Web Awesome](https://webawesome.com) components do not have. The main package `WebAwesome.Blazor` stays pure bindings. MIT-licensed.

This package has the same version as `WebAwesome.Blazor` and depends on it with a floor of that version. Use the same version of both packages.

## Install

```bash
dotnet add package WebAwesome.Blazor.Extensions
```

Set up `WebAwesome.Blazor` first, as its package readme shows. The Blazor runtime loads the JavaScript of this package automatically.

## Data grid cell presets

`WaDataGridFormatters` and `WaDataGridComparators` (namespace `WebAwesome.Blazor.Extensions.DataGrid`) configure rich `WaDataGrid` cells in C#, with no JavaScript of your own. Each factory returns a `WaJsFunction` for a column's `Formatter` or `ComparatorFunction`.

```razor
@using WebAwesome.Blazor.Extensions.DataGrid

<WaDataGrid Data="orders" Columns="columns" RowKey="id" @onwablazor-cell-action="HandleCellAction" />

@code {
    private readonly WaDataGridColumn[] columns =
    [
        new() { Field = "id", Label = "Order", Formatter = WaDataGridFormatters.ActionLink("open", "#{id}") },
        new()
        {
            Field = "state", Label = "State",
            Formatter = WaDataGridFormatters.Badge<OrderState>(state => state switch
            {
                OrderState.Shipped => WaVariant.Success,
                OrderState.Cancelled => WaVariant.Danger,
                _ => WaVariant.Neutral,
            }),
            ComparatorFunction = WaDataGridComparators.Ordinal<OrderState>()
        },
        new() { Field = "total", Label = "Total", Formatter = WaDataGridFormatters.Currency("EUR") },
    ];

    private void HandleCellAction(WaDataGridCellActionEventArgs e)
    {
        var order = e.GetRow<OrderRow>();
        // open the order
    }
}
```

- **Text presets** (`Number`, `Currency`, `Percent`, `Date`, `DateTime`, `Time`, `RelativeTime`, `Bytes`, `Duration`, `Lookup`, `Boolean`, `Template`, `List`) return text, so the CSV export and the copy of rows hold the formatted text.
- **Visual presets** (`Badge`, `Tag`, `Tags`, `Icon`, `BooleanIcon`) and links (`Link`, `ActionLink`, `Email`, `Phone`) render elements. An icon always has an accessible name, and a badge always shows a text.
- **Culture**: `CultureInfo.CurrentCulture` when the factory runs, or the `culture` you pass, not the browser locale.
- **Dates**: a `DateOnly` value shows its calendar day in every time zone. A `DateTime` of the kind `Unspecified` reaches the browser without a zone; use UTC or `DateTimeOffset` in the row and set `timeZone`.
- **Enums**: `Badge<TEnum>`, `Lookup<TEnum>` and `Ordinal<TEnum>` key their maps by the JSON form of each value: the number, or the name when the enum has a `[JsonConverter]` attribute such as `JsonStringEnumConverter`.
- **Cell actions**: bind `@onwablazor-cell-action` on `WaDataGrid` (import the namespace). The handler gets `WaDataGridCellActionEventArgs`: `Action`, `Column`, `RowKey`, `Row`, `Value`, `RowIndex`, `GetRow<T>()` and `GetPayload<T>()`. A click that raises an action does not raise `OnCellClick`. The event is a notification: update your data when the action changes a row.
- **Links**: `Link` URL-encodes each placeholder value and allows only relative URLs and the http, https, mailto and tel schemes. The grid uses Enter on a cell for its keyboard navigation, so use `ActionLink` (a button) for an action that the keyboard must reach.
- **More visual presets**: `ProgressBar`, `ProgressRing` (with color thresholds), `Rating` (read-only), `Avatar`, `Image` (relative, http and https URLs only), `ColorSwatch`, `Sparkline`, `Trend`, `DataBar` (fixed limits, or computed from the loaded rows; in server mode, set `min` and `max`) and `CopyButton`.
- **Interactive presets**: `Button`, `Actions` (icon buttons and a menu, with `HiddenField` and `DisabledField` per row), `Menu`, `Switch` and `Checkbox`. A switch or checkbox sends `{ "checked": bool }` as the payload and shows the new state at once: update your row in the handler and push `Data` again (push it unchanged to reject the change).
- **Other grid members**: `WaDataGridClassRules.Cell`/`Row` (classes by conditions, for `CellClassFunction` and `RowClassFunction`; style them with `ShadowStyles`), `WaDataGridFooters` (`Sum`, `Average`, `Min`, `Max`, `Count`), `WaDataGridComparators.Collation(culture)`, `WaDataGridSearch.Formatted(columns)` (the search also finds lookup and badge labels), `WaDataGridFilterOptions` (filter options from labels), `WaDataGridRows.DetailFields` and `WaDataGridRows.SelectableField`.
- Keep the column list in a field: the grid sends the columns again only when their JSON changes.

```razor
new() { Field = "active", Label = "Active", Formatter = WaDataGridFormatters.Switch("Active: {customer}") },
new()
{
    Field = "monthly", Label = "Monthly", Formatter = WaDataGridFormatters.Currency("EUR"),
    CellClassFunction = WaDataGridClassRules.Cell(new WaDataGridClassRule("high", WaDataGridCondition.GreaterThan(1000))),
    FooterFunction = WaDataGridFooters.Sum(WaDataGridFormatters.Currency("EUR"), "Total")
},
new()
{
    Id = "actions", Label = "Actions",
    Formatter = WaDataGridFormatters.Actions(
    [
        new WaDataGridAction("edit", "Edit") { Icon = "pen" },
        new WaDataGridAction("cancel", "Cancel") { InMenu = true, Variant = WaVariant.Danger }
    ])
},
```

## Your own cell functions

A `WaJsFunction` of your own (for example an export of a collocated `Page.razor.js`) gets these `ctx` helpers when the package is installed:

- `ctx.emit(action, payload)` raises a cell action for the row of the call (also in a detail panel or a footer).
- `ctx.h(tag, props, ...children)` builds DOM safely: text is always text, an `on*` function is a listener, `action: "edit"` adds a click that raises the action.
- `ctx.preset("badge", options)`, `ctx.format.currency(value, options)`, `ctx.cache(key, create)`.
- An element with `data-wablazor-action="edit"` (and optionally `data-wablazor-action-payload` with JSON) raises the action when it is clicked in a cell. This also works in a Lit template.

```js
export function orderCell(value, row, ctx) {
  return ctx.h('span', {},
    ctx.preset('badge', { variants: { open: 'brand', closed: 'neutral' } }),
    ctx.h('button', { type: 'button', action: 'approve' }, 'Approve'));
}
```

## Links

- [Repository & documentation](https://github.com/boldbrick/WebAwesome-Blazor)
- [Changelog](https://github.com/boldbrick/WebAwesome-Blazor/blob/master/docs/CHANGELOG.md)
- [Web Awesome documentation](https://webawesome.com/docs)

Web Awesome, Font Awesome, and related marks are property of their respective owners. This is an independent binding, not affiliated with or endorsed by the Web Awesome authors.
