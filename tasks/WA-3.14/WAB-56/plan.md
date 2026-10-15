# WAB-56 — Bind upstream function-valued members on WaJsFunction; ShadowStyles

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 17.2 (in control), 11.2 and the decisions D2,
D10 and D11 (section 15). Package: `WebAwesome.Blazor` only. Base: WAB-54 (`WaJsFunction`, `webawesome-functions.js`,
`resolveFunction(descriptor, binding)`).

## Scope

1. Function-valued members as `WaJsFunction?` `[Parameter]`s, pushed as JS properties through the generic resolver.
   Signatures checked against `temp\wa-src\3.14.0\dist-cdn` (`custom-elements.json` and the `.d.ts` files):

   | Element | Member | Upstream signature | Parameter | Wrappers |
   |---|---|---|---|---|
   | `wa-select` | `getTag` | `(option, index)` | `GetTagFunction` | `WaSelect` |
   | `wa-combobox` | `getTag` | `(option, index)` | `GetTagFunction` | `WaCombobox` |
   | `wa-combobox` | `filter` | `(option, query)` | `FilterFunction` | `WaCombobox` |
   | `wa-rating` | `getSymbol` | `(value, isSelected)` | `GetSymbolFunction` | `WaRating` |
   | `wa-slider` | `valueFormatter` | `(value)` | `ValueFormatterFunction` | `WaSliderBase` (`WaSlider`, `WaRange`) |
   | `wa-date-picker` | `isDateDisabled`, `dayContent` | `(date)` | `IsDateDisabledFunction`, `DayContentFunction` | `WaDatePickerBase` (`WaDatePicker`, `WaDateRangePicker`) |
   | `wa-date-input` | `isDateDisabled`, `dayContent` | `(date)` | `IsDateDisabledFunction`, `DayContentFunction` | `WaDateInputBase` (`WaDateInput`, `WaDateRangeInput`) |
   | `wa-pagination` | `hrefTemplate` (function form) | `(page)` | `HrefTemplateFunction` | `WaPagination` |

   Out of scope: `wa-combobox` `dataSource` (server mode with `OnOptionsRequest` covers it), the icon library (WAB-57),
   the other grid members (WAB-58).
2. The defect: `WaSelect.SetGetTagFunctionAsync`, `WaRating.SetSymbolFunctionAsync` and
   `WaSliderBase.SetValueFormatterAsync` send JavaScript source text; `WaSelect.GetTag` and `WaSlider.ValueFormatter` are
   read by nothing. Prove it in the browser first, then `[Obsolete]` (warning) with the replacement and the 3.15.0 removal.
3. Removal notes for the 3.15.0 train: CHANGELOG `### Deprecated` and a "Next-release check" note, `// TODO` at each
   obsolete member, Phase 2 of `docs\UPGRADE-PROCESS.md` and the `/wa-upgrade` skill extended (code TODO/NOTE search,
   obsolete members due, function arity watch list), the four "in WA 3.0" notes reworded.
4. `ShadowStyles` (D10) on `WaDataGrid`, `WaSelect`, `WaCombobox`, `WaRating`, `WaDatePickerBase`, `WaDateInputBase`.
5. Parity config, tests (C# and e2e), demo examples, `docs\technical.md`, CHANGELOG.

## Decisions

- **Names (D2).** Every new member has the `Function` suffix. The natural names are taken (`GetTag`, `ValueFormatter`,
  `HrefTemplate`) or read as something else (`GetSymbol` as a method, `Filter` as a filter text, `IsDateDisabled` as a
  flag, `DayContent` as the `WaDayContent` slot content), so one rule keeps the wrappers consistent. The grid members of
  WAB-54 keep their names (`Formatter` was free; `ComparatorFunction` already follows the rule).
- **The arity table is in JavaScript.** `elementFunctions` in `webawesome-interop.js` lists, per tag, each member with
  the number of arguments the element passes and whether its output renders in the shadow root. `ctx` (`args`,
  `element`) always follows them. This is the watch list item R1 together with `dataGridColumnFunctions`.
- **Push on change.** The wrapper compares the descriptor JSON with the last push (one helper for all wrappers) and calls
  `setElementFunction(element, member, json)` after the render; an unset member that was never pushed sends nothing.
  JavaScript waits for `customElements.whenDefined`, resolves, preloads `Uses` tags, keeps the element's own value of the
  member before the first assignment, and assigns. A later push of the same JSON assigns nothing.
- **Removal.** A member set back to null restores the element's own value (its default renderer, or `''` for
  `hrefTemplate`), but only while the element still holds the function the wrapper assigned. A reference that does not
  resolve: one `console.error` and the element keeps its own value (as the grid leaves the column member out).
- **`HrefTemplate` and `HrefTemplateFunction` are exclusive.** Both set: `InvalidOperationException` in
  `OnParametersSet`, because the attribute and the property write the same element property.
- **Obsolete behavior.** `SetGetTagFunctionAsync` and `SetSymbolFunctionAsync` throw `NotSupportedException` that names
  the replacement, after the existing guard checks; they send nothing. `SetValueFormatterAsync` stays harmless: it sends
  nothing any more (it never had an effect), so it cannot replace a `ValueFormatterFunction`. The two `Func` properties
  stay unread.
- **ShadowStyles.** One JS function `setShadowStyles(element, cssText)`: after `whenDefined` and `updateComplete`, one
  constructed `CSSStyleSheet` per element is added to `shadowRoot.adoptedStyleSheets`, replaced in place when the text
  changes, and removed for null or empty text. `wa-date-input` renders its day cells in the shadow root of its inner
  `wa-date-picker`, so the sheet goes there too (`nestedShadowHosts`; the same list drives `discover()` of the rendered
  output). The C# helper pushes on change only.
- **Version notes in code.** `// TODO WA <version> train: ...` for work due in a train, `// NOTE checked against WA
  <version>: ...` for a verified upstream fact. Phase 2 finds both with one case-sensitive search.

## Test approach

- bUnit: per wrapper, the first render pushes a set member, an equal reference pushes nothing, a change and a null push
  again, an unset member never pushes; `ShadowStyles` the same. Obsolete methods: select and rating throw and send
  nothing; slider sends nothing. A contract test checks that every (tag, member) the wrappers push is in
  `elementFunctions` with an arity.
- E2E, harness `/testing/element-functions` and the spec `element-functions.spec.js`: first a failing test that the old
  select and rating methods break the element; then each member in a real element (free CDN: select, rating, slider,
  range, pagination; Pro with self-skip: combobox, date picker, date input, date range input, data grid `ShadowStyles`),
  `ShadowStyles` on a select tag and a grid cell, and a member set back to null restores the default.

## Result

- Changesets on `/main/WA-3.14/WAB-56`: cs:360 (plan), cs:361 (code, tests, demo, harness, e2e, API baseline), and the
  last check-in (docs, this result).
- Defect proven first: `element-functions.spec.js` failed on the unchanged code with
  `TypeError: this.getTag is not a function` after `SetGetTagFunctionAsync`; it passes now.
- Public API: `GetTagFunction` (`WaSelect`, `WaCombobox`), `WaCombobox.FilterFunction`, `WaRating.GetSymbolFunction`,
  `WaSliderBase.ValueFormatterFunction`, `IsDateDisabledFunction` and `DayContentFunction` (`WaDatePickerBase`,
  `WaDateInputBase`), `WaPagination.HrefTemplateFunction` and its `JSInterop`, `ShadowStyles` (`WaDataGrid`, `WaSelect`,
  `WaCombobox`, `WaRating`, `WaDatePickerBase`, `WaDateInputBase`), the `OnAfterRenderAsync`/`OnParametersSet`
  overrides; `[Obsolete]` on `WaSelect.SetGetTagFunctionAsync`, `WaSelect.GetTag`, `WaRating.SetSymbolFunctionAsync`,
  `WaSliderBase.SetValueFormatterAsync`, `WaSlider.ValueFormatter`. Baseline promoted.
- JavaScript (`webawesome-interop.js`): `setElementFunction` with the `elementFunctions` arity table, `setShadowStyles`
  with `nestedShadowHosts`; `discoverShadowContent` also discovers the nested calendar of `wa-date-input`. Both exports
  return at once and finish in the browser, so a Pro element that never loads cannot hold the interop call.
- Verification: Debug and Release builds green, 0 warnings. Tests: main 1379 on net9.0 and 1379 on net10.0 (27 new),
  Extensions 108 and 108, in Debug and in Release. E2E, WebAssembly demo on port 5100: free CDN 360 passed and 62 skipped
  (the listed skips, four new; `minimumTests` 420); local Pro dist (`temp\wa-src\3.14.0`) 421 passed and one failure,
  `event-dispatch` WaVideo `OnEnded`, which this machine cannot play (the media clock stays at 0 with no audio output;
  unrelated to this ticket). Server host, free CDN: `element-functions`, `js-functions` and `sweep` specs, 128 passed and
  9 skipped. Pro override cleared afterwards.

## Deviations and findings

- The C# helper is `Base\WaJsPropertyPush` (functions and shadow styles); the JS side has one function per concern
  (`setElementFunction`, `setShadowStyles`), as the design asks for `ShadowStyles`.
- `ShadowStyles` of `WaDateInput`/`WaDateRangeInput` is adopted by the input's own shadow root and by the shadow root of
  its inner `wa-date-picker`, where the day cells render. Without the second root the styles would not reach the cells.
- `WaDateRangePicker` gains the members through `WaDatePickerBase` (as `WaDateRangeInput` through `WaDateInputBase`).
- Demo examples on nine pages (select with `ShadowStyles`, combobox, rating, slider, date picker, date input, date range
  picker, date range input, pagination), each with a collocated `.razor.js`.
- The four "in WA 3.0" notes now read `// NOTE checked against WA 3.14.0: ...`; the arity tables carry the same note.
- `RenderedWrapperCatalog.RequiredParameters` is internal now (reused by `ElementJsPropertyTests`).

## Open points

- The interaction sweep fails on the server host for unrelated pages (SignalR "Server timeout" under the page clock);
  only the WebAssembly host runs it cleanly. Not caused by this ticket.
- The `WaVideo` dispatch case needs an audio output device on the test machine.