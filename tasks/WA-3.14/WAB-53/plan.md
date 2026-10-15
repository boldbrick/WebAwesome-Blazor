# WAB-53 — WaDataGrid phase 0: column push on change, null trimming, FilterOptions

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 5.3, 7.4, 12 and 17 (section 17
is in control). Package: `WebAwesome.Blazor` only.

## Scope

1. Column push on change (section 5.3). `WaDataGrid` pushes `columns` on the first render as today. After
   that, `OnParametersSetAsync` pushes `columns` only when the `Columns` reference changed or its serialized
   JSON changed. An in-place change of the list or of a column object changes the JSON, so it still reaches
   the element. `data` is pushed on every parameter set, as today (consumers can change the list in place).
2. Null trimming. Every nullable member of `WaDataGridColumn` gets
   `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`. The attribute applies with any serializer
   options, also with the options of the Blazor JS interop, which have no global ignore condition.
3. `FilterOptions`. New `IReadOnlyList<WaDataGridFilterOption>? FilterOptions` on `WaDataGridColumn` for the
   3.14 member `filterOptions`. New record `WaDataGridFilterOption(string Value, string? Label = null,
   int? Count = null)` in `WebAwesome.Blazor.Models`, with null trimming on `Label` and `Count`.
4. Interface comparison. Compare every member of the upstream `DataGridColumn` (3.14.0
   `data-grid.d.ts`) with `WaDataGridColumn`. Add any other missing JSON-expressible member the same way.
   Function-valued members stay out (WAB-54, WAB-58).
5. Tests, public API snapshot, demo, e2e, CHANGELOG, `docs\technical.md`.

## Decisions

- Change detection keeps the last pushed reference and the last pushed JSON text, not a hash. A string
  comparison is exact and the column JSON is small. The JSON for the comparison comes from
  `JsonSerializer.Serialize` with `JsonSerializerOptions.Web`; it only has to be deterministic and contain
  every member. The element receives the column objects through the interop as before.
- The snapshot is updated before the push is awaited, so two overlapping parameter sets do not push the same
  columns twice.
- `Value` of `WaDataGridFilterOption` stays `string`: upstream types it `string` and compares it as a string
  (`String(value)`), so the value set is open.
- `Count` is `int?`: it is a row count.
- `WaDataGridFilterOption` is a positional `sealed record`, as the ticket asks for a small record type. The
  namespace is `WebAwesome.Blazor.Models`, next to `WaDataGridColumn`. Nothing goes into
  `WebAwesome.Blazor.Extensions`.

## Test approach

- bUnit (`WaDataGridIntegrationTests`): count the `setProperty` calls for `columns`. One push on the first
  render. No push on a parameter set without a column change (and `data` is still pushed). A push after a new
  `Columns` reference with other content. A push after an in-place change of a column object, and after an
  in-place addition to the list.
- xUnit: the JSON that the interop sends. A test subclass of `Microsoft.JSInterop.JSRuntime` records the
  `argsJson` of `BeginInvokeJS`, which is the exact text that goes to JavaScript. Assert: a column with only
  `Field` serializes as `{"field":"name"}`; `FilterOptions` serializes as
  `[{"value":"a","label":"A","count":3},{"value":"b"}]`.
- E2E (Pro): open the region filter of the countries grid on `/components/data-grid`, read the option labels
  in the picker (order and labels from `FilterOptions`), check one option and see the rows filter. Also read
  `grid.columns` and check that no column member is null. The spec self-skips without Pro assets
  (`skipUnlessProUpgrades`), and `expected-skips.json` lists the skip.

## Result

- Changesets on `/main/WA-3.14/WAB-53`: cs:346 (plan), cs:347 (code, tests, demo, API baseline), and the last
  check-in (e2e spec, skip list, docs, this result).
- Code: `WaDataGrid` pushes `columns` on the first render, then only when the `Columns` reference or the column JSON
  changes. `data` keeps its push on every parameter set. All 25 nullable members of `WaDataGridColumn` and the
  optional members of the new `WaDataGridFilterOption` carry `JsonIgnore(WhenWritingNull)`.
- Public API: `WaDataGridColumn.FilterOptions`, record `WaDataGridFilterOption(string Value, string? Label = null,
  int? Count = null)`, and the `JsonIgnore` attributes on the column members. Baseline promoted.
- Interface comparison (3.14.0 `DataGridColumn`): `filterOptions` was the only JSON-expressible member that was
  missing. The other members that are not in the model are function-valued (`value`, `filterFn`, `comparator`,
  `formatter`, `aggregatedFormatter`, and the function forms of `footer`, `aggregation`, `cellClass`).
- Verification: Debug and Release builds green, 0 warnings. Tests: main 1283 on net9.0 and 1283 on net10.0 (9 new),
  Extensions 7 and 7, in Debug and in Release. The JSON tests read the argument JSON of a real `JSRuntime` call.
- E2E, WebAssembly demo on port 5100: with the local Pro dist (`temp\wa-src\3.14.0`), 396 passed. On the free CDN,
  348 passed, 47 skipped (the listed skips, the two new ones included), 1 failed: the overlays showcase toast test,
  which then passed 12 of 12 times alone (a timing flake under full load, not related). A temporary mutation that
  pushes the columns on every parameter set makes the new browser test fail, so the test proves the rule.
- Demo: the countries example sets `FilterOptions` on its region set filter (order, two labels, and a value that no
  row has).
- Docs: CHANGELOG `[Unreleased]` (one Added line, two Changed lines), `docs\technical.md` (JSON models leave out
  unset members; push on change), `tools\e2e\README.md`.

## Deviations and open points

- The ticket rule keys on the reference too, so a `Columns` list that the consumer creates again on every render
  (for example inline in the markup) is still pushed on every render. A consumer keeps the list in a field to avoid
  this. WAB-54 can decide whether the JSON alone must decide when formatter functions come in.
- The second part of design section 5.3 (the resolver keeps the resolved columns per grid in a `WeakMap`) belongs to
  the resolver ticket (WAB-54).
- The interaction sweep fails on the server host when it runs with the Pro dist: the page clock jumps end the
  SignalR circuit ("Server timeout elapsed"). The release gate runs the WebAssembly host, where the sweep passes.
  Not related to this ticket.
