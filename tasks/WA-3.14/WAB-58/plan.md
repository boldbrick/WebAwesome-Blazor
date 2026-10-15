# WAB-58 — WaDataGrid phase 2 (main): the remaining function-valued members

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 7 and 17.1 (section 17 is in control).
Package: `WebAwesome.Blazor` only. Base: WAB-54 (`WaJsFunction`, `setDataGridColumns`, `dataGridColumnFunctions`), WAB-56
(`setElementFunction`, `elementFunctions`, `WaJsPropertyPush`, `ShadowStyles`) and WAB-57 (the conflict rule of the
icon library). The Extensions package (WAB-59) is out of scope.

## Upstream (WA 3.14.0, `dist-cdn\components\data-grid\data-grid.d.ts` and `custom-elements.json`)

| Kind | Member | Upstream signature | C# member | JSON key / JS member | Renders |
|---|---|---|---|---|---|
| Column | `footer` (function form) | `(rows) => string \| TemplateResult \| Node` | `FooterFunction` | `footer` | yes |
| Column | `cellClass` (function form) | `(value, row) => string` | `CellClassFunction` | `cellClass` | no |
| Column | `value` | `(row) => unknown` | `ValueFunction` | `value` | no |
| Column | `filterFn` | `(value, filterValue, row) => boolean` | `FilterFunction` | `filterFn` | no |
| Column | `aggregation` (function form) | `(values, rows) => unknown` | `AggregationFunction` | `aggregation` | no |
| Grid | `rowClass` | `(row) => string \| null \| undefined` | `RowClassFunction` | `rowClass` | no |
| Grid | `rowDetail` | `(row) => string \| TemplateResult \| Node` | `RowDetailFunction` | `rowDetail` | yes |
| Grid | `searchFn` | `(value, searchTerm, row) => boolean` | `SearchFunction` | `searchFn` | no |
| Grid | `selectableRows` | `(row) => boolean` | `SelectableRowsFunction` | `selectableRows` | no |
| Grid | `childRows` (function form) | `(row) => Row[] \| undefined` | `ChildRowsFunction` | `childRows` | no |

`dataSource` stays out: server mode with `OnDataRequest` covers it. The grid renders the footer cells and the detail
panel in its shadow root (`part="footer-cell"`, `part="row-detail"`), and sets the `rowClass` classes on the row element
there. `filterFn` applies only to a column with `filterable`. `value` takes precedence over `field`.

## Decisions

- **Names (D2).** The ticket names: every member has the `Function` suffix. `Value` and `Filter` would read as data, and
  the static forms `Footer`, `CellClass`, `Aggregation` and `ChildRows` exist.
- **Column members** go through `setDataGridColumns` under the upstream JSON key; `dataGridColumnFunctions` gets the
  five arities. A static string (or the built-in aggregation) under the same key is not a descriptor, so the resolver
  leaves it alone.
- **One JSON key for two C# members.** System.Text.Json rejects two properties with one name, so the static and the
  function form of `footer`, `cellClass` and `aggregation` are `[JsonIgnore]`, and one private `[JsonInclude]` property
  per key writes the one that is set.
- **Conflict rule: fail.** Both forms of one upstream member set (`Footer` and `FooterFunction`, `CellClass` and
  `CellClassFunction`, `Aggregation` and `AggregationFunction`, `ChildRows` and `ChildRowsFunction`): `WaDataGrid`
  throws `InvalidOperationException` in `OnParametersSet` with the column and the two members. Reason: both write the
  same element property, so a silent precedence hides a mistake; `WaPagination.HrefTemplate` and the icon library
  resolver already fail the same way.
- **Grid members** are `[Parameter]`s pushed on change through `WaJsPropertyPush` and `setElementFunction`;
  `elementFunctions` gets a `wa-data-grid` entry with the five arities.
- **ctx.** As for the formatter: column members get `args`, `element`, `column` and `grid`; grid members get `args`,
  `element` and `grid`. A member with a row argument also gets `rowKey` (cellClass, value, filterFn, rowClass,
  rowDetail, searchFn, selectableRows, childRows). The tables carry the position of the row argument
  (`rowArgument`); `elementFunctions` members of `wa-data-grid` get `grid` and `rowKey` through a small per-tag table.
- **Shadow DOM.** `footer` and `rowDetail` render: their `Uses` tags are registered before the assignment, and
  `discover(grid.shadowRoot)` runs after it. A detail panel renders only when a row expands, after that discovery, so
  its elements need `Uses` (documented). `ShadowStyles` reaches the footer, the detail panel and the row classes,
  because all are in the grid's shadow root.

## Test approach

- xUnit/bUnit: the JSON keys and descriptors of the five column members; a static form still serializes under its key;
  the four conflicts throw and name both members; the grid members are listed in `ElementJsPropertyTests` (push on
  change, the arity table in both directions).
- E2E (Pro, self-skip on the free CDN), new harness `/testing/grid-functions` with a collocated `.razor.js` and the spec
  `grid-functions.spec.js`: a footer function sums the filtered rows; a value function drives sort, search and the CSV
  export; cellClass and rowClass functions styled through `ShadowStyles`; a filter function; a custom aggregation on a
  grouped grid; rowDetail renders a detail panel with an upgraded element on expand; searchFn; selectableRows locks a
  row; a childRows function builds a tree; the demo example. `expected-skips.json` and `minimumTests` updated.
- Demo: a compact "More JavaScript Members" example on the data grid page, functions in `DataGridPage.razor.js`.
