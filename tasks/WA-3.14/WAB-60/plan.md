# WAB-60 — WaDataGrid phase 3 (Extensions): request-response actions and static Razor cell templates

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, sections 9.6, 10 and 17 (section 17 is in control).
Package: `WebAwesome.Blazor.Extensions`. Base: WAB-55 (presets, node cache, cell action bridge), WAB-59 (`ctx` helpers,
declarative actions, D4). No change in the main package is planned.

## Scope

1. Request and response: `ctx.invoke(action, payload?, source?)` returns a Promise. It sends the request to a .NET
   handler and resolves with the JSON result of the handler, or rejects with an `Error`.
2. Static Razor cell templates: C# renders a `RenderFragment<TRow>` per row and template to HTML with `HtmlRenderer`;
   the HTML goes into a separate row property; a registered formatter clones it into the cell.
3. Tests (xUnit and bUnit in the Extensions test project, Playwright e2e), API baseline, demo, docs, CHANGELOG.

## Decisions

- **Request handler: a companion component that wraps the grid.**
  `<WaDataGridRequestHandler OnRequest="HandleAsync"><WaDataGrid ... /></WaDataGridRequestHandler>`. It renders one
  `div` with `display: contents` around its child content, and registers one `DotNetObjectReference` for that element
  in the browser after its first render. `ctx.invoke` looks up the handler from the grid element through its ancestors
  when it is called.
  - Why not `@ref` next to the grid: a component reference is set only after the render, so the companion parameter is
    null until the page renders again; the handler would be missing for the first cells.
  - Why not a registration API that returns `IAsyncDisposable`: the page must call it in `OnAfterRenderAsync`, keep it,
    implement `IAsyncDisposable`, and register again when the grid element changes.
  - The wrapper needs no `@ref`, no disposal code in the page, and survives a replaced grid element (lookup at call
    time). One handler can serve several grids. Cost: one element with `display: contents` (no layout box).
- **Handler type.** `Func<WaDataGridCellActionEventArgs, Task<object?>>? OnRequest`. The request reuses the cell action
  arguments (design 9.6): `Column`, `Action`, `RowKey`, `Row`, `Value`, `Payload`, `RowIndex`, `GetRow<T>`, `GetPayload<T>`.
- **Errors.** A `WaDataGridRequestException` thrown by the handler rejects the Promise with its message (a message that
  the app writes for the browser). Any other exception goes to Blazor's own JS interop error handling (logged on the
  server; the browser gets Blazor's message, with details only when detailed errors are on). No handler, no `OnRequest`,
  a disposed reference or a disconnected circuit: the Promise rejects with a clear message.
- **No handler yet.** The handler registration can arrive after the first cells render (the import of the module and
  the registration are asynchronous, two round trips on Blazor Server). `ctx.invoke` waits for a registration up to
  2 s (as the late registration of a `WaJsFunction`), then rejects.
- **Shared browser state.** The handler map is on `globalThis[Symbol.for('WebAwesome.Blazor.Extensions.cellRequests')]`
  (element to reference), so two copies of the module share it.
- **Concurrency.** Each call is its own `invokeMethodAsync`; results never mix. Blazor Server runs the handler on the
  circuit dispatcher.
- **Disposal.** `DisposeAsync` unregisters in the browser (a `JSDisconnectedException` is ignored: the circuit is gone),
  disposes the module reference and the `DotNetObjectReference`.
- **Razor templates: `WaDataGridRazorTemplates<TRow>`.** `Add(key, RenderFragment<TRow>)`, `Formatter(key)` (a
  `WaJsFunction.Registered("wab.dataGrid.razorTemplate")` with `{ key }`; declare the rendered elements with `Uses`), and
  `RenderAsync(IServiceProvider services, IEnumerable<TRow> rows)` that returns the rows for `Data`.
  - The registered name `wab.dataGrid.template` of the ticket is taken by the text `Template` preset (WAB-55). The new
    name is `wab.dataGrid.razorTemplate`.
  - `RenderAsync` creates one `HtmlRenderer` from the given service provider (the DI scope of the page) per call, renders
    every cell on its dispatcher, and disposes it. A long-lived `HtmlRenderer` from DI keeps every root component it
    rendered until it is disposed, so a scoped renderer would grow with each data push on Blazor Server.
  - Each row becomes an internal wrapper object whose JSON converter writes the row exactly as the serializer of the
    interop would (same options), plus the property `$wabTemplateHtml` (key to HTML). No column reads it; the
    sanitized row of a cell action and a request leaves it out.
  - The formatter parses each HTML text once into a `<template>` (a bounded cache by text), clones its content into a
    `span` with `display: contents`, and keeps the node per row object and key (the node cache of WAB-55). The only HTML
    parse of the package is in `wwwroot\data-grid-templates.js`; the contract test allows exactly that one.
  - Interactivity: `data-wablazor-action` only. `@onclick` and wrappers that need JS interop after the render do not
    work in static HTML.

## Test approach

- xUnit/bUnit (Extensions tests, bUnit added): the handler component (the wrapper markup, the registration with the
  reference, the result and error envelope, no `OnRequest`, disposal with and without a connection); the templates (the
  HTML of a `WaBadge` and a `data-wablazor-action` button, text encoding, the row JSON with the reserved property, the
  formatter descriptor, validation); the JavaScript contract (the invokable method name, the reserved property name, the
  preset table, the one HTML parse).
- E2E: harness `/testing/cell-requests`. Free CDN: the .NET round trip through the request module, error rejection,
  no handler, concurrent calls. Pro (self-skip on the free CDN): `ctx.invoke` from a formatter with a pending state that
  updates, a detail panel and a footer, a replaced grid element; a Razor template column with a `WaBadge` and a
  `data-wablazor-action` button (D4); the demo examples. Run on the WebAssembly and the Server host.
