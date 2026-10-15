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
