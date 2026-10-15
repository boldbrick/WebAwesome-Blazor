# WAB-54 — WaDataGrid phase 1 (main): WaJsFunction and function-valued column members

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 5, 8, 11.1, 11.5, 11.6 and 17 (section 17 is
in control). Package: `WebAwesome.Blazor` only. WAB-55 (presets), WAB-56 (other wrappers), WAB-57 (icon library) and
WAB-58 (other grid members) build on this ticket.

## Scope

1. `WaJsFunction` in `WebAwesome.Blazor.Models`: an immutable reference to a JavaScript function. Factories
   `Registered(name)`, `Module(url, exportName = "default", allowCrossOrigin = false)`, `Global(path)`. Instance methods
   `WithArgs(args)` and `Uses(params tags)` return a new instance. A JSON converter writes the descriptor of design 5.1.
2. JavaScript: a new public module `wwwroot\webawesome-functions.js` with the shared registry on
   `globalThis[Symbol.for('WebAwesome.Blazor.functions')]`, `registerFunction(name, fn)`,
   `registerContextDecorator(decorator)`, and the generic resolver `resolveFunction(descriptor, binding)`.
3. `webawesome-interop.js`: `setDataGridColumns(element, columnsJson, configuredEntryPointUrl)`. It resolves the
   descriptors, preloads the declared tags, assigns `element.columns`, and calls `discover(grid.shadowRoot)` after the
   update. The icon library mutator uses the shared global path logic.
4. `WaDataGridColumn`: `Formatter`, `AggregatedFormatter`, `ComparatorFunction` (`WaJsFunction?`; JSON keys
   `formatter`, `aggregatedFormatter`, `comparator`).
5. `WaDataGrid`: the column JSON alone decides a column push. The push sends the JSON text to `setDataGridColumns`.
6. Tests, demo section, harness page, e2e, `docs\technical.md`, CHANGELOG.

## Decisions

- Descriptor: `{"$wab":"fn","ref":{...},"args":...,"uses":[...]}`. `ref` is `{"kind":"registered","name"}`,
  `{"kind":"module","url","export"}` (plus `"allowCrossOrigin":true` when set) or `{"kind":"global","path"}`. `args`
  and `uses` are left out when not set.
- `WithArgs` serializes the arguments at once (`JsonSerializerOptions.Web`, camelCase) and keeps the JSON. The value
  then cannot change, and a value that cannot be serialized fails at the call. Equality compares the descriptor parts.
- URL rule (D7): C# rejects whitespace, control characters and any scheme other than `http` and `https`. C# cannot
  know the page origin, so the browser rejects a module of another origin unless `allowCrossOrigin` is true.
- The column push sends the JSON text that the change detection already makes. JavaScript parses it once and keeps
  the resolved columns per grid in a `WeakMap` (element to JSON text and columns). Same JSON: no assignment.
- A reference that does not resolve: one `console.error` names the column, the member and the reference. The member is
  left out, so the grid uses its default (raw values, the formatter for an aggregated cell, the built-in sort).
- A function that throws when the grid calls it is application code: the wrapper does not catch it.
- `ctx`: one small object per call. Its prototype holds `args`, `element`, `column`, `grid` and the decorator members;
  `rowKey` is a getter that reads `row[grid.rowKey]` (as upstream `getRowId`) only when the function uses it. The upstream
  arguments are on the symbol `Symbol.for('WebAwesome.Blazor.callArguments')`, so decorators (WAB-55, WAB-59) can read
  the row and the value. Decorators run once per binding, again only when the decorator list changes.
- `ctx` position: the binding declares the upstream arity (formatter 2, aggregatedFormatter 2, comparator 4), so `ctx`
  is always the argument after them.
- Late registration: one animation frame, then up to 2 s (`lateRegistrationTimeoutMs`).
- Autoload: the declared tags go to `discover()` on a detached element with `data-wa-preload` before the assignment.
  After `updateComplete` of a defined grid, `discover(grid.shadowRoot)` runs once per push that has a rendering member.

## Test approach

- xUnit: descriptor JSON of each kind, `WithArgs`, `Uses`, equality, validation (name, URL, export, path, tags, args).
- bUnit: the push rule (first render, same JSON with a new list reference, changed JSON, in-place change) and the JSON
  text that `setDataGridColumns` receives (null trimming, the three new keys).
- E2E (Pro, self-skip on the free CDN): Module formatter from the collocated `DataGridPage.razor.js`; a Registered
  formatter from the demo JS initializer with `ctx.args`, `ctx.rowKey`, `ctx.column`, `ctx.grid`; a comparator sorts;
  an aggregatedFormatter on a grouped grid; `discover(shadowRoot)` for a tag without `Uses`; failed references show
  raw values; a new list with the same JSON is not pushed.
- E2E (free CDN and Pro): the resolver without a grid (registry, late registration, global path, module, cross-origin
  refusal) and the `Uses` preload of a tag that nothing else on the page uses.
- Performance: per-call cost of the binding wrapper against a direct call.

## Result

- Changesets on `/main/WA-3.14/WAB-54`: cs:350 (plan), cs:351 (code, tests, API baseline), cs:352 (demo, harness, e2e),
  and the last check-in (docs, this result).
- Public API: `WaJsFunction` (`Registered`, `Module(url, exportName = "default", allowCrossOrigin = false)`, `Global`,
  `WithArgs`, `Uses`, `DefaultExportName`, `Kind`, `Reference`, `ExportName`, `AllowCrossOrigin`, `Args`, `Tags`, value
  equality, `ToString` = descriptor), `WaJsFunctionKind`, and `WaDataGridColumn.Formatter`, `AggregatedFormatter`,
  `ComparatorFunction`. Baseline promoted.
- JavaScript: new public module `webawesome-functions.js` (registry, `registerFunction`, `registerContextDecorator`,
  `callArguments`, `resolveFunction`, `isFunctionDescriptor`, `findGlobalFunction`). `webawesome-interop.js`:
  `setDataGridColumns`; the icon library mutator uses `findGlobalFunction`.
- Verification: Debug and Release builds green, 0 warnings. Tests: main 1352 on net9.0 and 1352 on net10.0 (69 new),
  Extensions 7 and 7, in Debug and in Release. E2E on the WebAssembly demo (port 5100): with the local Pro dist
  (`temp\wa-src\3.14.0`) 404 passed; on the free CDN 352 passed and 52 skipped (the listed skips, five new). The server
  host with the Pro dist: `js-functions`, `data-grid-columns` and `extensions-package` specs, 10 passed. A mutation that
  skips the `Uses` preload makes the free-CDN `Uses` test fail.
- Performance (R7), Node 24, 10,000 rows, one formatter: a direct call 15-24 ns, the wrapper with `ctx.args` 37-46 ns, with
  `ctx.rowKey` 66-94 ns, with one decorator 49-68 ns. The first version (rest parameters and a spread call) cost about
  200 ns; fixed parameters per arity removed most of it. A visible window of about 400 cells costs about 20 µs per render.

## Deviations and decisions to note

- The resolver is in `webawesome-functions.js`, next to the registry, not in `webawesome-interop.js` (design 4). The
  interop module keeps the element work (`setDataGridColumns`) and imports the resolver.
- The public module is `webawesome-functions.js`, not `webawesome-datagrid.js` (design 8.2): section 17.1 makes the
  registry general for all wrappers.
- The cross-origin opt-in is a parameter of `Module`. C# cannot know the page origin, so C# rejects only unsafe schemes;
  the browser checks the origin.
- The column JSON goes to JavaScript as text (one serialization in C#, also used for the change check). The JSON tests now
  read the argument of `setDataGridColumns`.
- Convention in `docs\technical.md`: apps prefix their registered names (`myapp.`), the packages use `wab.`.
- `ctx.args` is `undefined` when `WithArgs` was not called. An exception that the function throws is not caught.

## Open points

- R1: the formatter, aggregatedFormatter and comparator signatures (the arity in `dataGridColumnFunctions`) must be
  checked on each upgrade. Add them to the `/wa-upgrade` watch list (owner or WAB-56).
- `discover(grid.shadowRoot)` sees only the rows rendered at the push; a tag that first appears in rows rendered later
  needs `Uses` (documented).
- R2 (a Lit `TemplateResult` from the app's copy of Lit) is not verified; the presets and the demo use DOM nodes.
- The other function members (`footer`, `cellClass`, `value`, `filterFn`, `aggregation`, grid members) are WAB-58.