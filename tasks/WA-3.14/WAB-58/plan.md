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

## Result

- Changesets on `/main/WA-3.14/WAB-58`: cs:369 (plan), cs:370 (code, tests, API baseline, demo, harness, e2e), and the
  last check-in (docs, this result).
- Public API: `WaDataGridColumn.ValueFunction`, `FilterFunction`, `FooterFunction`, `AggregationFunction`,
  `CellClassFunction`; `WaDataGrid.ChildRowsFunction`, `RowClassFunction`, `RowDetailFunction`, `SearchFunction`,
  `SelectableRowsFunction` and the `OnParametersSet` override. `Footer`, `Aggregation` and `CellClass` are `[JsonIgnore]`
  now (the JSON keys stay the same through private `[JsonInclude]` properties). Baseline promoted.
- JavaScript (`webawesome-interop.js`): five entries in `dataGridColumnFunctions`, a `wa-data-grid` entry in
  `elementFunctions` (with `rowArgument`), and `elementFunctionContexts` (`ctx.grid`, `ctx.rowKey` for the grid members).
- Demo: the example "More JavaScript Members" (computed amount, footer sums, a class per cell with `ShadowStyles`, a
  detail panel), functions in `DataGridPage.razor.js`; the footer example comment no longer says the function form cannot
  be bound. Harness `/testing/grid-functions` with `GridFunctionsHarness.razor.js`, in `HARNESS_ROUTES`.
- Verification: Debug and Release builds green, 0 warnings. Tests: main 1396 on net9.0 and 1396 on net10.0 (12 new),
  Extensions 108 and 108, in Debug and in Release. E2E, WebAssembly demo on port 5100: local Pro dist
  (`temp\wa-src\3.14.0`) 440 passed; free CDN 368 passed and 72 skipped, exactly the listed skips (10 new;
  `minimumTests` 437). Pro override cleared afterwards.

## Deviations and findings

- **WA 3.14.0: `childRows` applies only with the data.** The grid's row model is keyed on `data` (table-core memo), so a
  `childRows` value assigned after the data has no effect until the next data push. The function resolves in the
  browser after the first `Data` push, so a `ChildRowsFunction` tree shows from the next `Data` push (any parameter set
  of the grid). Recorded, not worked around (`no-wa-bug-workarounds`): a `NOTE checked against WA 3.14.0` in the arity
  table, the remarks of `ChildRowsFunction`, `docs\technical.md`, the CHANGELOG line, and the e2e test reloads the data.
  A string `ChildRows` changed at run time has the same limit.
- A detail panel renders only when its row expands, after the `discover()` of the push: its elements need `Uses`
  (documented). The footer renders with the columns, so `discover()` sees it.
- `ctx.grid` for the grid members duplicates `ctx.element`; it keeps one shape for the column and grid members, which
  the `ctx` decorators of WAB-59 can rely on.
- Design 17.1 names the detail member `RowDetail`; the ticket and the `<Name>Function` rule give `RowDetailFunction`.

## Open points

- Owner: decide whether the binding should push the grid data again after a `childRows` assignment (a workaround of the
  upstream limit above), or wait for an upstream fix; recheck on the 3.15.0 upgrade.
