# WAB-63: WaDataGrid.ReloadAsync fails

## Report

A consumer uses WebAwesome.Blazor 3.14.0.1 with the Web Awesome 3.14.0 Pro kit from the CDN, a .NET 10 Blazor Web App
and the interactive server render mode. `await grid.ReloadAsync()` on a server-mode `WaDataGrid` (`Server="true"`,
`OnDataRequest`, `@ref`) throws in the JS interop call to `reload`, and the grid does not refresh. The reporter thinks
that wa-data-grid 3.14 has no `reload` method.

## Reproduction

- Harness `/testing/grid-reload` (`GridReloadHarness.razor`) on Demo.Server (port 5100, interactive server, no
  prerendering), Pro assets from the local 3.14.0 `dist-cdn` (`Set-WaProAssets.ps1`).
- A call after the page has loaded (Reload button, row create, external filter change) works: a new `wa-data-request`
  goes out and the grid shows the new rows.
- A call on the first render (`?early=true`, `OnAfterRenderAsync(firstRender)`) fails with
  `Failed to invoke method 'reload': Method 'reload' does not exist on element wa-data-grid or is not a function.`
  `WaCombobox.ReloadAsync` fails in the same way.
- Element state at the time of the call (probe on `HTMLElement.prototype.reload`): the referenced element is the live
  element, `isConnected` is true, `customElements.get('wa-data-grid')` is undefined, the constructor is `HTMLElement`.
  The element became defined about 240 ms later. After that, `typeof el.reload` is `'function'`.

## Root cause

The fault is in our interop module, not in Web Awesome: 3.11.0 to 3.14.0 all define `reload()`. The autoloader imports
the module of an element only after it finds the element in the DOM. Thus an element that Blazor has just rendered has
none of its methods for some time. `invokeMethod` in `webawesome-interop.js` checked `typeof element[name]` at once and
threw. Every wrapper method that calls an element method through `WebAwesomeJSInterop.InvokeMethodAsync` (143 call sites in
49 wrapper files) had the same fault for a call soon after the first render. The cause is the same in both render modes:
the call comes before the element module has loaded.

## Fix

`invokeMethod` is now async. Before the call, it waits for `customElements.whenDefined` (at most
`elementDefinitionTimeoutMs`, 10 s) when the element is a custom element that is not defined yet. After the wait an
element that is still not defined fails with an error that names it (for example a Pro element on the free CDN).
`invokeMethodIfDefined` keeps its behavior. The public API does not change.

## Tests

- `tools/e2e/tests/grid-reload.spec.js` (3 tests, Pro only, listed as expected skips for the free CDN): reload after
  load, after a row create and after a filter change (new `wa-data-request`, new rows); reload of the grid and the
  combobox on the first render, with the element modules delayed by 1.5 s (red before the fix, green after);
  `WaCombobox.ReloadAsync` sends a `wa-options-request`.
- No bUnit test: bUnit has no browser and no custom element registry, so it cannot show the fault.

## Why the old tests missed it

The unit tests use a fake interop module and assert only the method name. No browser test called `ReloadAsync`, and the
browser tests call element methods only after `waitForWaReady`, i.e. after the element is defined.

## Not tested

The CDN Pro kit loader, prerendering and the exact call timing of the reporter. The follow-up wish (`resetPage`, a
bindable `Page`) is out of scope; the root cause does not touch paging.
