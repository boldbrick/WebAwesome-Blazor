---
description: "Use when writing or changing code that uses WebAwesome.Blazor.Extensions: optional extras for the WebAwesome.Blazor data grid configured in C# - cell formatter presets, sort comparators, footer aggregates, class rules, search and filter presets, row presets, cell actions, .NET answers to cell requests and Razor cell templates; key namespace WebAwesome.Blazor.Extensions.DataGrid; key types WaDataGridFormatters, WaDataGridComparators, WaDataGridFooters, WaDataGridClassRules, WaDataGridCellActionEventArgs, WaDataGridRequestHandler, WaDataGridRazorTemplates, EventHandlers."
---

# WebAwesome.Blazor.Extensions

## Capabilities

- Cell presets for a data grid column Formatter, configured in C# only (`WaDataGridFormatters`): text (`Number`, `Currency`, `Percent`, `Date`, `DateTime`, `Lookup`, `Template`, ...), visual (`Badge`, `Tag`, `Icon`, `ProgressBar`, `Sparkline`, `DataBar`, ...), links (`Link`, `ActionLink`, `Email`, `Phone`) and interactive presets (`Button`, `Actions`, `Menu`, `Switch`, `Checkbox`).
- Sort presets (`WaDataGridComparators.Ordinal`, `WaDataGridComparators.Collation`), footer aggregates (`WaDataGridFooters`), cell and row classes (`WaDataGridClassRules`, `WaDataGridClassRule`, `WaDataGridCondition`).
- Search, filter and row presets: `WaDataGridSearch.Formatted`, `WaDataGridFilterOptions`, `WaDataGridRows.DetailFields`, `WaDataGridRows.SelectableField`.
- Cell actions into .NET, values from .NET for ctx.invoke, and cells written as Razor.

## Entry points

- `WaDataGridFormatters`: each factory returns a function reference for the column Formatter.
- `WaDataGridAction`: one button or menu item of `Actions` or `Menu`.
- `EventHandlers`: declares the @onwablazor-cell-action Razor attribute.
- `WaDataGridCellActionEventArgs`: `Action`, `Column`, `RowKey`, `Row`, `Value`, `RowIndex`, `Payload`, `GetRow<T>`, `GetPayload<T>`.
- `WaDataGridRequestHandler.OnRequest` and `WaDataGridRequestException`.
- `WaDataGridRazorTemplates<TRow>`: `Add`, `Formatter`, `RenderAsync`.

## Typical usage

```razor
@using WebAwesome.Blazor.Components
@using WebAwesome.Blazor.Models
@using WebAwesome.Blazor.Extensions.DataGrid

<WaDataGrid Data="orders" Columns="columns" RowKey="id" @onwablazor-cell-action="HandleCellAction" />

@code {
    // keep the column list in a field
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
        new()
        {
            Field = "total", Label = "Total", Formatter = WaDataGridFormatters.Currency("EUR"),
            FooterFunction = WaDataGridFooters.Sum(WaDataGridFormatters.Currency("EUR"), "Total")
        },
    ];

    private void HandleCellAction(WaDataGridCellActionEventArgs e)
    {
        var order = e.GetRow<OrderRow>();
        // open the order
    }
}
```

## Pitfalls

- Set up WebAwesome.Blazor first and use the same version of both packages. The Blazor runtime loads the JavaScript of this package automatically.
- Import WebAwesome.Blazor.Extensions.DataGrid in the Razor file: without the `EventHandlers` declaration, Razor passes @onwablazor-cell-action on as a text attribute and the handler never runs.
- A cell action is a notification: the handler cannot cancel it or return a value. When it changes a row, update the grid data. A click that raises an action does not raise OnCellClick of the grid.
- `Switch` and `Checkbox` send `{ "checked": bool }` and show the new state at once: update the row and push the data again (push it unchanged to reject the change).
- A formatter changes only what the cell shows: sort, filter and search use the raw value. Text presets put the formatted text in the CSV export; visual presets and links put the raw value.
- The culture defaults to CultureInfo.CurrentCulture when the factory runs, not the browser locale. A DateTime of kind Unspecified reaches the browser without a zone: use UTC or DateTimeOffset in the row and set timeZone.
- `Badge<TEnum>`, `Lookup<TEnum>` and `Ordinal<TEnum>` key their maps by the JSON form of each value: the number, or the name when the enum has a JSON converter.
- `Link` allows only relative URLs and the http, https, mailto and tel schemes. Enter on a cell is grid navigation, so use `ActionLink` for an action the keyboard must reach.
- Keep the column list in a field: the grid sends the columns again only when their JSON changes.
- ctx.invoke needs the grid inside `WaDataGridRequestHandler`. Return a JSON-serializable value; throw `WaDataGridRequestException` to reject with a user message. With no handler, a null `OnRequest` or a lost circuit, the Promise rejects; a request waits up to 2 seconds for the handler. Grid functions are synchronous: show a pending state and key answers by row key, not row object. On Blazor Server, call StateHasChanged of the page when the handler changes page state.
- `WaDataGridRazorTemplates<TRow>` renders static HTML: only data-wablazor-action is interactive, @onclick does not work, and a MarkupString with user content is not safe. `RenderAsync` renders each template once per row per call: use it for server mode and small pages, and call it again when rows change. Declare the wa-* elements of a template in `Formatter`.

## Full documentation

- `docs/dependencies/synced/WebAwesome.Blazor.Extensions/INDEX.md`
- Exact signatures: the type pages under api/WebAwesome.Blazor.Extensions.DataGrid in that folder; `api/symbols.tsv` lists every public name.
- The grid itself (WaDataGrid, WaDataGridColumn, WaJsFunction) is in the WebAwesome.Blazor skill.
