# WAB-59 — WaDataGrid phase 2 (Extensions): ctx helpers, actions, rules and remaining presets

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 6.3, 6.4, 7, 8.4-8.6, 9.3-9.5, 11, 16 and 17
(section 17 is in control). Package: `WebAwesome.Blazor.Extensions`; the main package gets one small generic hook only.
Base: WAB-55 (presets, node cache, cell action bridge, D4), WAB-58 (the grid function members).

## Scope

1. `ctx` helpers through `registerContextDecorator` (new module `wwwroot\data-grid-context.js`): `emit`, `h`, `preset`,
   `format`, `cache`, for the members of `wa-data-grid` (formatter, aggregatedFormatter, footer, rowDetail and the others).
2. Declarative actions: `data-wablazor-action` and `data-wablazor-action-payload` (JSON), clicks only, through the bridge of
   WAB-55. D4 for every action.
3. Interactive presets: `Button`, `Actions` (with `WaDataGridAction`), `Menu`, `Switch`, `Checkbox`.
4. Rule and member presets: `WaDataGridClassRules.Cell`/`Row` (with `WaDataGridClassRule`, `WaDataGridCondition`),
   `WaDataGridFooters` (`Sum`, `Average`, `Min`, `Max`, `Count`), `WaDataGridComparators.Collation`,
   `WaDataGridSearch.Formatted`, `WaDataGridFilterOptions` (from labels, an enum, a formatter),
   `WaDataGridRows.DetailFields` and `WaDataGridRows.SelectableField`.
5. Visual presets: `ProgressBar`, `ProgressRing`, `Rating`, `Avatar`, `Image`, `ColorSwatch`, `Sparkline`, `Trend`,
   `DataBar`, `CopyButton`. No `QrCode`.
6. Risks: R2 (a Lit 3 `TemplateResult` from the app's own Lit), R4 (Tab and Enter in an action cell), R6 (a dropdown in a
   pinned column and in the last row), R7 (per-call cost), the 10,000-row budget (11.5).
7. Tests, demo, docs, CHANGELOG.

## Decisions

- **Main package hook.** `resolveFunction` passes the position of the row argument of the member (`rowArgument`, already
  in `dataGridColumnFunctions` and `elementFunctions`) to the context decorators. The `emit` and `cache` helpers need the
  row of the call; nothing else changes in the main package.
- **Helpers only for the grid.** The decorator adds the helpers when `binding.element` is a `wa-data-grid`; other elements
  get no new members. `emit` without a row (footer) sends `row: null`, `rowKey: null`, `column` of the footer's column.
- **`ctx.emit(action, payload?, source?)`.** The optional third argument (an element in the cell, or an event) gives the
  row index; without it the index is -1. `ctx.h(..., { action, payload })` passes its element.
- **`ctx.h`.** A string, number or boolean prop is an attribute (`true` an empty attribute, `false`/null none), an `on*` key
  with a function adds a listener (`onClick` -> `click`, `'onwa-select'` -> `wa-select`), an object or array is a property,
  `action`/`payload` add the action click. `innerHTML`, `outerHTML` and `srcdoc` are refused; `href`, `src`, `action` and
  `formaction` take only relative URLs and http, https, mailto, tel. Text children are text nodes.
- **`ctx.preset(name, options)`, `ctx.format.<preset>(value, options)`.** The options object is normalized once per binding
  and option JSON, so the column caches (Intl formatters, nodes) of the preset still work for a new object literal per call.
  The culture is `options.culture`, else `ctx.args.culture`, else the browser default.
- **`ctx.cache(key, create)`.** The node cache of WAB-55: row object, then the resolved function, then the key; the same
  render never gets one node twice. Without a row (footer) the cache is per resolved function.
- **D4 for every action, and for preset controls.** The bridge keeps the click in flight per grid (a capture listener on the
  grid's shadow root). An action raised during that click, a declarative action, or a click on a control that a preset
  renders (menu trigger, switch, checkbox, copy button) stops the `wa-cell-click` of that click at the host. An action
  raised outside a click (a `change`, a keyboard selection in a menu) marks nothing.
- **Declarative actions.** The capture listener keeps the nearest `data-wablazor-action` element of the click path inside
  the shadow root. The `wa-cell-click` of that click gives the column, row, value and row index. A click with no
  `wa-cell-click` (detail panel, footer, group row) is dropped with one `console.warn`; use `ctx.emit` there (design 9.3).
- **Switch and checkbox.** The action is raised on `change` with `{ checked }`; the D4 mark comes from the click before it
  (the elements dispatch `change` after their update). The element shows the new state at once; .NET must update its row
  and push `Data` (design 9.5, documented).
- **Condition names.** `WaDataGridCondition.EqualTo` instead of `Equals`: a static `Equals` hides `object.Equals`.
  Equality compares `String(value)` with the key, as the lookup presets do; a number key is the JSON form of the number.
  `LessThan`, `GreaterThan` and `Between` (inclusive) take a number, a `DateOnly` or a `DateTimeOffset`. `Matches` takes a
  JavaScript pattern; the browser reports an invalid one once and the rule does not match.
- **Text format of the composite presets.** `WaDataGridFooters`, `Trend`, `DataBar` and `DetailFields` take a text preset
  of `WaDataGridFormatters` (`Number`, `Currency`, `Percent`, `Date`, ...) as `format`; any other reference throws
  `ArgumentException`.
- **Column values.** The footer aggregates and the computed `DataBar` limits read the column of `ctx.column` from
  `grid.columns`: its `value` function, else its `field`. `DataBar` limits come from `grid.data` (in server mode only the
  loaded page; set `min` and `max` there, documented).
- **SearchFormatted.** `WaDataGridSearch.Formatted(columns)` reads the labels (and the lookup fallback) of the columns whose
  formatter is a lookup, badge, tag, tags or icon preset. The grid calls `searchFn` once per row and column without the
  column id, so the function matches the raw value (case-insensitive substring, as the default search) or any label of
  the row (cached per row and term).
- **Image.** `srcTemplate` (null: the cell value is the URL) and `altTemplate` (null: empty alt, a decorative image);
  only relative URLs and http, https. Design: `SrcField`/`AltField`; the column `Field` and a template cover them.
- **Defaults.** Optional options are null and the element default applies. The preset texts that have no element default
  are public constants (`DefaultMenuLabel`, `DefaultToggleAction`, the trend icons).

## Test approach

- xUnit (Extensions): the descriptor of every new factory, the rule JSON (each condition), enum keys of the typed
  overloads, validation (action ids, field paths, URL schemes, text presets), the filter options and the search columns,
  each preset name in the JavaScript table.
- E2E with the local Pro dist (`temp\wa-src\3.14.0`), self-skip on the free CDN: harness `/testing/cell-actions` (interactive
  presets, D4, R4, R6, declarative actions, `ctx` helpers in formatter, rowDetail and footer, rules, footers, search, filter
  options, detail fields, selectable field), the remaining visual presets on `/testing/cell-presets`, R2 on
  `/testing/lit-cells`, the demo example. The free CDN tests: the presets and helpers without a grid.
- Performance: R7 in Node 24 (decorated `ctx`), and the 10,000-row grid with 8 preset columns (`/testing/cell-actions?perf=1`)
  scrolled in Chromium with a long-task observer.
