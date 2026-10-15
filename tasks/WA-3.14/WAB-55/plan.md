# WAB-55 — WaDataGrid phase 1 (Extensions): first cell presets and OnCellAction

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 6.1-6.4, 7.3, 9.2, 9.4 (D4), 11.2-11.7 and 17
(section 17 is in control). Package: `WebAwesome.Blazor.Extensions`. Contract: `docs\technical.md`, "JavaScript function
references" (WAB-54). This ticket answers the consumer question "how do we show state badges in grids from C#?".

## Scope

1. Namespace `WebAwesome.Blazor.Extensions.DataGrid` (the main package owns `WebAwesome.Blazor.Extensions`).
2. C# factories that return `WaJsFunction` values (`Registered("wab.dataGrid.<preset>")`, `WithArgs`, `Uses`):
   - `WaDataGridFormatters`: text presets `Number`, `Currency`, `Percent`, `Date`, `DateTime`, `Time`, `RelativeTime`,
     `Bytes`, `Duration`, `Lookup`, `Boolean`, `Template`, `List`; visual presets `Badge`, `Tag`, `Tags`, `Icon`,
     `BooleanIcon`; links `Link`, `ActionLink`, `Email`, `Phone`; typed `Badge<TEnum>`, `Lookup<TEnum>`.
   - `WaDataGridComparators`: `Ordinal(values)`, `Ordinal<TEnum>(order)`.
   - Common options `nullText` and `culture` (default `CultureInfo.CurrentCulture` when the factory runs).
3. JavaScript: `wwwroot\data-grid-presets.js` (the preset functions and the cell action dispatch), registered by the
   Extensions initializer through `_content/WebAwesome.Blazor/webawesome-functions.js`.
4. Event bridge: `wablazor-cell-action` on the grid host (bubbles, composed), registered with
   `registerCustomEventType`; `[EventHandler("onwablazor-cell-action", ...)]` on `WaDataGridEventHandlers`;
   `WaDataGridCellActionEventArgs` with `GetRow<T>()` and `GetPayload<T>()`; decision D4.
5. Demo section "Extensions: cell presets" on `/components/data-grid`; e2e harness `/testing/cell-presets`.
6. Tests (xUnit in the Extensions test project, Playwright e2e with Pro assets), public API baseline.
7. CHANGELOG, Extensions `PACKAGE.md`, README, `docs\technical.md`.

## Decisions

- Preset names are `wab.dataGrid.<preset>` (the `wab.` prefix of the package, then the element, so that later presets
  for other wrappers do not collide). One registered function per factory: the descriptor names the preset.
- Arguments: a factory writes only the options that are set (no nulls), plus `culture`. Map keys stay verbatim (the
  serializer has no dictionary key policy). Enum options are written as the strings that JavaScript reads.
- Culture: the name of `culture ?? CultureInfo.CurrentCulture`. The invariant culture has no BCP 47 name; it is written
  as `en-US`, whose formats it follows. An invalid locale or time zone in the browser: one `console.error`, then the
  browser default.
- Type names carry the `WaDataGrid` prefix (`WaDataGridDateStyle`, `WaDataGridIcon`, ...) and live in the Extensions
  namespace, so they cannot collide with a later enum of the main package for a `wa-format-*` attribute.
- Intl formatters: one per resolved column function. The cache key is the `ctx.args` object, which the resolver
  creates once per column and push.
- Node cache (6.1): `WeakMap` row object -> (column binding -> { key, node }). A node that the current render already
  returned (the grouped cell of a group row shows the value of its first leaf row) is not reused in the same render: a
  new node is made, so a node never moves between two cells.
- Calendar values: a `yyyy-MM-dd` string (`DateOnly`) is formatted as that calendar day (UTC parts), and an `HH:mm[:ss]`
  string (`TimeOnly`) as that time of day; `TimeZone` applies to instants only. `Duration` reads numbers (seconds or
  milliseconds), ISO 8601 strings and the .NET `TimeSpan` JSON form.
- Enum keys (11.4): `JsonSerializer.Serialize(value, JsonSerializerOptions.Web)`, a JSON string becomes its text and a
  number its digits, so the key equals `String(value)` in JavaScript.
- `Link` with `HrefTemplate`: an `a` element. Each placeholder value is URL-encoded. C# rejects whitespace, control
  characters and a literal scheme other than http, https, mailto, tel; the browser checks the scheme of the built URL
  again and renders plain text for any other scheme. `Target="_blank"` adds `rel="noopener noreferrer"`.
- `ActionLink` (the `Action` form of `Link`): a `button` element with link styles, not an `a` element. The element
  performs an action, so its role is a button; the grid's keyboard navigation also lets Enter and Space through to a
  button (it consumes Enter on other cell content).
- Event bridge: the preset's click listener dispatches `wablazor-cell-action` on the grid host at once; `rowIndex` is
  the `data-row-index` of the cell (the index `wa-cell-click` reports). The same listener marks the grid, and a
  `wa-cell-click` listener on the grid host stops the propagation of the matching `wa-cell-click` (D4): Blazor, which
  listens on the document, does not see it; listeners on the grid element still do. The mark is cleared when the click
  reaches the host, or after one task at the latest.
- The event payload is sanitized as in the main initializer (row depth 4).

## Test approach

- xUnit: descriptor JSON of each factory, enum keys (number and `JsonStringEnumConverter` enums), template and URL
  validation, culture default, the registered event type against the `[EventHandler]` declaration, each C# preset name
  registered in JavaScript, no `innerHTML`/`eval`/`new Function` in the package JavaScript.
- E2E with the local Pro dist (`temp\wa-src\3.14.0`), self-skip on the free CDN: each preset renders; the CSV export
  holds the formatted text of the string presets; a badge autoloads; the action link reaches `OnCellAction` and does not
  raise `OnCellClick`; keyboard Enter on the action link; a `DateOnly` value shows its calendar day in a time zone west
  of UTC; the `@onwablazor-cell-action` attribute reaches the element through `AdditionalAttributes`.

## Result

- Changesets on `/main/WA-3.14/WAB-55`: cs:355 (plan), cs:356 (code, tests, demo, harness, e2e, API baseline), and the
  last check-in (docs, this result).
- Public API (Extensions, namespace `WebAwesome.Blazor.Extensions.DataGrid`): `WaDataGridFormatters` (22 factories,
  `DefaultTrueIconName`, `DefaultFalseIconName`, `DefaultListSeparator`), `WaDataGridComparators` (`Ordinal(string[])`,
  `Ordinal(IEnumerable<string>)`, `Ordinal<TEnum>`), `WaDataGridCellActionEventArgs` (`GetRow<T>`, `GetPayload<T>`,
  `UnknownRowIndex`), `EventHandlers` (`CellActionEventName`, `CellActionAttributeName`), `WaDataGridIcon`, and the enums
  `WaDataGridDateStyle`, `WaDataGridNumberNotation`, `WaDataGridSignDisplay`, `WaDataGridPercentScale`,
  `WaDataGridDurationUnit`, `WaDataGridDurationStyle`. Baseline `approved-extensions-public-api.txt` promoted. No change
  in the main package.
- JavaScript: `wwwroot\data-grid-presets.js` (22 preset functions, `raiseCellAction`, `cellActionArgs`); the initializer
  registers the presets in `beforeWebStart`/`beforeStart` and the event type in `afterWebStarted`/`afterStarted`.
- Verification: Debug and Release builds green, 0 warnings. Tests: Extensions 108 on net9.0 and 108 on net10.0 (101
  new), main 1352 and 1352, in Debug and in Release. E2E, WebAssembly demo on port 5100: with the local Pro dist
  (`temp\wa-src\3.14.0`) 412 passed; on the free CDN 354 passed and 58 skipped, which matches `expected-skips.json`
  (six new skips, `minimumTests` 410). Server host with the Pro dist: `cell-presets`, `js-functions` and
  `extensions-package` specs, 15 passed. A mutation that removes the D4 `stopPropagation` makes the action test fail.
  The Extensions nupkg holds `staticwebassets/data-grid-presets.js`.
- Performance: one Intl formatter per column; creating one per cell costs about 125 µs a cell in Node 24, the cached
  formatter a few µs (mostly `Intl.format` itself).

## Deviations and findings

- The `[EventHandler]` declaration is on a class named `EventHandlers`, not `WaDataGridEventHandlers`: the Razor compiler
  reads the attribute only from public classes with that name. With another name, Razor passed
  `@onwablazor-cell-action="Handler"` on as a text attribute (seen in the browser and in the generated code). With the
  name, the forwarding through `WaDataGrid.AdditionalAttributes` works on both hosts (design: "unverified").
- `ActionLink` is the `Action` form of `Link` and renders a `button` with link styles, not an `a`: it performs an
  action, and the grid's keyboard navigation lets Enter and Space through to a button only.
- Web Awesome behavior, recorded (no workaround): the grid's `GridNavigationController` consumes Enter on cell content
  that is not a button or a form control, so Enter on a focused `a` in a cell raises `wa-cell-click` and does not follow
  the link. Recheck on the next upgrade (`TOGGLE_TAGS`).
- R3 answered for the presets: the Blazor router intercepts a relative `Link` in the grid's shadow root (no page load).
- The event bridge gets `rowIndex` from the cell's `data-row-index` and dispatches at once, instead of the click pairing
  of design 9.3 (that pairing is for `data-wablazor-action`, WAB-59). A microtask could not end the mark: after each
  listener of a user click the browser runs the microtasks, before the cell's own click listener.
- `Currency` takes the fixed code and the row field in one factory (`currency`, `currencyField`; at least one).

## Open points

- Enter on a `Link` in a cell (Web Awesome keyboard navigation, above): report upstream if the owner agrees.
- The node cache helps only within the grid's own renders (sort, scroll): `WaDataGrid` pushes `data` on every parameter
  set, which gives new row objects (phase 0 rule).
