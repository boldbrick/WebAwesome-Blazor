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

## Values from .NET in a cell (`ctx.invoke`)

`ctx.invoke(action, payload)` sends a request from a cell function, a detail panel or a footer to .NET and returns a Promise. Put the grid inside a `WaDataGridRequestHandler`:

```razor
<WaDataGridRequestHandler OnRequest="HandleRequestAsync">
    <WaDataGrid Data="items" Columns="columns" RowKey="sku" />
</WaDataGridRequestHandler>

@code {
    private async Task<object?> HandleRequestAsync(WaDataGridCellActionEventArgs request)
    {
        var item = request.GetRow<Item>() ?? throw new WaDataGridRequestException("Unknown item");
        return await stockService.GetStockAsync(item.Sku);   // any JSON-serializable value
    }
}
```

- The Promise resolves with the returned value. `WaDataGridRequestException` rejects it with its message (write that message for the user). Another exception rejects it with the message of Blazor.
- No handler around the grid, a handler without `OnRequest`, or a lost Blazor Server connection: the Promise rejects with a clear message. The handler can register after the first cells render; a request waits up to 2 seconds for it.
- A grid function is synchronous. Show a pending state first, keep the answer, and render again when it arrives. Key the answers by the row key, not by the row object: `WaDataGrid` sends `Data` again on each parameter set, which gives new row objects.

```js
const answers = new Map();

export function stockCell(value, row, ctx) {
  let answer = answers.get(ctx.rowKey);
  if (!answer) {
    answer = { text: null };
    answers.set(ctx.rowKey, answer);
    ctx.invoke('stock')
      .then(stock => { answer.text = `${stock} in stock`; }, error => { answer.text = error.message; })
      .then(() => ctx.grid.requestUpdate());
  }

  return answer.text ?? ctx.cache('pending', () => ctx.h('span', {}, 'Checking…'));
}
```

## Cells in Razor (`WaDataGridRazorTemplates`)

Write a cell as Razor markup. .NET renders it to static HTML for each row, and the column shows the HTML:

```razor
@inject IServiceProvider Services

<WaDataGrid Data="rows" Columns="columns" RowKey="id" @onwablazor-cell-action="HandleCellAction" />

@code {
    private static readonly WaDataGridRazorTemplates<Deployment> templates = new WaDataGridRazorTemplates<Deployment>()
        .Add("state", d => @<WaBadge Variant="@VariantOf(d.State)">@d.State</WaBadge>)
        .Add("actions", d => @<WaButton Size="WaSize.Small" data-wablazor-action="restart">Restart</WaButton>);

    private readonly WaDataGridColumn[] columns =
    [
        new() { Field = "service", Label = "Service" },
        new() { Id = "state", Label = "State", Formatter = templates.Formatter("state", "wa-badge") },
        new() { Id = "actions", Label = "Actions", Formatter = templates.Formatter("actions", "wa-button") },
    ];

    private IReadOnlyList<object>? rows;

    protected override async Task OnInitializedAsync() => rows = await templates.RenderAsync(Services, deployments);
}
```

- **Static HTML.** Only `data-wablazor-action` (and `data-wablazor-action-payload`) makes the content interactive. `@onclick` does not work, and a component that needs JS interop after its render does not get it.
- **Security.** Razor encodes text and attribute values. A `MarkupString` with user content is not safe.
- **Cost.** `RenderAsync` renders each template once per row on each call, and the HTML adds to each row. Use it for server mode and small pages, not for thousands of rows. Call it again when the rows change.
- **Elements.** Declare the `wa-*` elements of a template in `Formatter(key, ...)`: the cells are in the shadow root of the grid, where the autoloader does not look.
- The HTML travels in a separate row property that no column reads, so sort, filter, search and the CSV export use the row values.

## Links

- [Repository & documentation](https://github.com/boldbrick/WebAwesome-Blazor)
- [Changelog](https://github.com/boldbrick/WebAwesome-Blazor/blob/master/docs/CHANGELOG.md)
- [Web Awesome documentation](https://webawesome.com/docs)

Web Awesome, Font Awesome, and related marks are property of their respective owners. This is an independent binding, not affiliated with or endorsed by the Web Awesome authors.
