# WAB-65: browser test suite for the wrapper imperative methods

Read this file and `temp\wab-methods-brief.md` (the shared brief). They are all a group agent needs. The lead phase
built the framework and the exemplar group `pro-data`; the group agents write the other six groups in parallel; an
integration agent then runs everything and edits the shared files.

## Design

The suite calls every public wrapper method that reaches the browser from .NET, through the wrapper's `@ref`, on a
real upgraded element, and asserts the effect: element state, DOM, returned value or event. A call that only does not
throw proves nothing.

| Part | File | Owner |
|---|---|---|
| Manifest exporter (bUnit) | `src\WebAwesome.Blazor.Tests\ApiParity\WrapperMethodManifestTests.cs` | lead |
| Manifest | `tools\e2e\data\wrapper-methods.json` (224 methods, 54 wrappers) | generated |
| Group partition | `tools\e2e\data\method-groups.json` | lead |
| Exemptions | `tools\e2e\data\method-coverage-exemptions.json` | each group: its own entries only |
| Coverage gate (no browser) | `tools\e2e\tests\method-coverage.spec.js` | lead |
| Shared helpers | `tools\e2e\tests\helpers\method-harness.js` | lead |
| Call button component | `src\WebAwesome.Blazor.Demo\Pages\Testing\MethodCall.razor` | lead |
| Harness page per group | `src\WebAwesome.Blazor.Demo\Pages\Testing\Methods<Group>Harness.razor` | group |
| Spec per group | `tools\e2e\tests\methods-<group>.spec.js` | group |
| Case module per group | `tools\e2e\tests\helpers\method-cases\<group>.js` | group |
| Routes, expected skips, `minimumTests` | `tests\helpers\routes.js`, `data\expected-skips.json` | lead / integration only |

- **Manifest.** `WrapperMethodManifestTests` takes every public concrete wrapper (`RenderedWrapperCatalog`), runs each
  public Task-returning method (inherited ones too, overloads together) once on a wrapper that looks rendered
  (`RecordingJSRuntime.CreateRendered`), with sample arguments, and records the module calls. Each entry has `id`
  (`Wrapper.Method`), `component`, `member`, `tag`, `pro`, `kind` (module function: `invokeMethod`, `setProperty`,
  `getProperty`, `setCustomValidity`, ...), `js` (element member) and `also` (further calls). The test fails when the
  file is stale; copy `wrapper-methods.received.json` from the test output over it. A method that always throws
  `NotSupportedException` (the obsolete `SetSymbolFunctionAsync`, `SetGetTagFunctionAsync`, `SetValueFormatterAsync`)
  reaches nothing and is not listed.
- **Coverage gate.** `method-coverage.spec.js` loads every case module and checks: every manifest wrapper is in
  exactly one group; every method is proven (by a case's `methods` or an `external` proof) or exempted, never both;
  a case proves only methods of its own group; every listed id exists; known-defect rules (below); a pending-group
  exemption exists only while the group has no case module. It prints the summary and `pending group <name>` lines.
- **Harness.** `<MethodCall Id="..." Action="() => x!.DoAsync()" />` (or `Function="async () => (object?)await
  x!.GetAsync()"` for a value) renders a button `data-testid=<Id>` and an output `<Id>-result` with `data-calls` and
  the outcome: the value as camelCase JSON, `done`, or `error: <message>`. The button is disabled until the page is
  interactive (prerender). `callOk(page, id)` clicks, waits for its own call and returns the value (fails on an
  error); `callMethod` returns `{ done, value, error }`. `{ quiet: true }` triggers the button with a click event only:
  the focus and an open popup stay as they are. Use it for every focus, blur, show, hide, open and close call (a real
  click moves the focus and closes popups, which hides or fakes the effect). Use the real click (default) for a call
  that needs a user gesture (clipboard, fullscreen, picker).
- **Spec.** Copy `methods-pro-data.spec.js` and change only the group name and the module path. The test titles
  (`method: <name>`, `method known defect: <name>`) come from the module; the test is located in the spec file, so the
  report keys are `methods-<group>.spec.js > methods <group> > method: <name>`.
- **Routes.** `routes.js` adds `/testing/methods-<group>` to `HARNESS_ROUTES` (and so to `sweep.spec.js`) as soon as
  `method-cases\<group>.js` exists. The interaction sweep does not visit harness pages.
- **Render modes.** Demo.Server reads `Demo:Prerender` (`$env:Demo__Prerender='true'`; default off). The suite runs
  against WASM (5000), Demo.Server (5100) and Demo.Server with prerendering.

## Groups

Totals: 224 methods. Pro wrappers are marked (P).

| Group | Route | Components | Methods |
|---|---|---|---|
| `forms-text` | `/testing/methods-forms-text` | WaInput, WaTextArea, WaNumberInput, WaOtpInput, WaTagInput, WaKnownDate | 39 |
| `forms-choice` | `/testing/methods-forms-choice` | WaButton, WaCheckbox, WaSwitch, WaRadio, WaRadioGroup, WaRating, WaSlider, WaRange | 36 |
| `pickers` | `/testing/methods-pickers` | WaColorPicker, WaSelect, WaTimeInput, WaDateInput (P), WaDateRangeInput (P), WaDatePicker (P), WaDateRangePicker (P) | 44 |
| `overlays` | `/testing/methods-overlays` | WaDialog, WaDrawer, WaDropdown, WaDropdownItem, WaPopover, WaPopup, WaTooltip, WaToast, WaToastItem, WaPage | 20 |
| `navigation` | `/testing/methods-navigation` | WaAccordion, WaAccordionItem, WaDetails, WaTab, WaTabGroup, WaStepper, WaCarousel, WaTreeItem, WaSplitPanel | 20 |
| `media-content` | `/testing/methods-media-content` | WaAnimatedImage, WaAnimation, WaZoomableFrame, WaVideo (P), WaVideoPlaylist (P), WaMarkdown, WaMutationObserver, WaResizeObserver, WaRandomContent, WaRelativeTime, WaCopyButton | 32 |
| `pro-data` (done) | `/testing/methods-pro-data` | WaDataGrid (P), WaCombobox (P), WaFileInput (P) | 33 |

The exact method list of a group: the entries of `wrapper-methods.json` whose `component` is in the group. The
audit `temp\wab-63-audit.md` table B gives the JS name and the preconditions of each.

WAB-64 findings by group: `forms-text` F10 (WA side), F11 (`ShowPickerAsync`), F12; `forms-choice` none (R
preconditions on button, checkbox, switch); `pickers` F1, F8, F9 (WaSelect, WA side); `overlays` F3, F8, F13;
`navigation` F5, F8; `media-content` F2, F4, F11 (video fullscreen); `pro-data` F6, F7, F8, F9, F11.

## Conventions for a group agent

You do not start servers or browsers. You write code and run `dotnet build src\WebAwesome.slnx` (zero warnings). The
integration agent runs the specs and fixes what the browser shows. PowerShell only.

1. **Harness page** `src\WebAwesome.Blazor.Demo\Pages\Testing\Methods<Group>Harness.razor`, `@page
   "/testing/methods-<group>"` (PascalCase group in the file name, e.g. `MethodsFormsTextHarness.razor`). Start with
   the header comment of `MethodsProDataHarness.razor` (adapted), a `<PageTitle>`, an `<h1>`, one `ExampleSection` per
   component with its `MethodCall`s above the element. Test ids: `<component-prefix>-<call>`, unique on the page
   (for example `input-step-up`, `dialog-show`). One `@ref` field per wrapper. Constants for every literal used twice
   or with a meaning (`private const` in `@code`; CLAUDE.md "no magic constants"); code comments lowercase, no period.
   Show a value through `Function=`; shape a large value into a short one (row keys, a count). Give each element the
   state its methods need (a number input type for `StepUpAsync`, slides for a carousel, a submenu for
   `OpenSubmenuAsync`, a `<script type="text/markdown">` child for `RenderMarkdownAsync`, and so on).
2. **Case module** `tools\e2e\tests\helpers\method-cases\<group>.js`, `// @ts-check`, exporting
   `{ group: '<group>', cases: [...], knownDefects: [...], external: [...] }` (typedefs in `method-harness.js`). A case:
   `{ name, methods: ['Wrapper.Method', ...], tags: ['wa-...'], pro?, clipboard?, route?, noReadyWait?, setup?, run }`.
   `run(page)` calls `callOk`/`callMethod` and asserts the effect with polling `expect`. Read state through the DOM or
   element properties (`locator.evaluate(el => el.open)`) - never call the method under test in the page. A Pro case
   sets `pro: true` (the runner calls `skipUnlessProUpgrades`). Every method of the group must be in some case's
   `methods`, in an `external` proof, or in an exemption.
3. **Spec** `tools\e2e\tests\methods-<group>.spec.js`: a copy of `methods-pro-data.spec.js` with the group name and
   module path changed. `test` must come from `./helpers/test` (`test-base.spec.js` checks it).
4. **Pending exemption.** In the same change as the case module, delete the entry `pending-group-<group>` from
   `tools\e2e\data\method-coverage-exemptions.json` (one Edit of your own entry; other agents edit the same file). Add
   your own exemptions there, each with a `key` prefixed with the group (`forms-text-...`), a reason and the method
   ids. Do not touch other entries.
5. **Known defects (WAB-64).** Write the case for the CORRECT behavior in `knownDefects` with `defect: 'WAB-64 F<n>'`
   and the affected `methods`; it runs as an expected failure. If the method has a working path, prove it with a
   normal case too (as `WaCombobox.ShowAsync`: a normal show and hide case, plus the F8 cancelled-show defect case). If
   the method has no working path (F1 `GetHexStringAsync`, F4 `UpdateAsync`), exempt it with `"knownDefect": true`, a
   reason starting with `KNOWN DEFECT`, `"defect": "WAB-64 F<n>"`, and give the known-defect case `exemption: <key>`.
   F2, F3 and F5 are "browser check" items: write them as known-defect cases; the integration agent moves a case that
   passes ("Expected to fail, but passed") into `cases` and reports the finding as refuted.
6. **WA-side bugs** (F9 WaSelect, F10 WaTextArea): no workaround. Assert the current Web Awesome behavior in a normal
   case with a comment `// WAB-64 F<n> (Web Awesome side, recheck on upgrade): ...`, as the F9 combobox step does.
7. **User gesture and clipboard** (F11): trigger the call with the real click (`callOk` without `quiet`), set
   `clipboard: true` where the call writes the clipboard. Exempt only what cannot run headless, with that reason.
8. **Timing cases.** Add one first-render case per group where it fits: the harness calls one or two methods in
   `OnAfterRenderAsync(firstRender)` when `?early=true`, shows the outcome (`not called`, `done` or `error: ...`), and
   the case delays the element modules (`page.route`, see `delayModules` in `pro-data.js`), sets `noReadyWait: true`,
   and asserts `done` and the effect. Prefer a method that needs the first Lit update (`FocusAsync`). Do not repeat
   `SetCustomValidityAsync` on the first render: it is the known defect `WAB-65 L1`, proven once in `pro-data`. Groups
   with show/hide methods (pickers, overlays, navigation) add ONE `WAB-64 F8` known-defect case for a cancelled show
   (`el.addEventListener('wa-show', e => e.preventDefault())`, then `callMethod(page, id, { quiet: true, timeout:
   5000 })` and expect `done`); only the methods that wait for an event have F8 (table B), not dialog, drawer and
   dropdown.
9. **Shared files.** Do not edit `routes.js`, `expected-skips.json` (skips, `minimumTests`), `method-groups.json`,
   `method-harness.js`, `MethodCall.razor`, `method-coverage.spec.js` or other groups' files. If you need a helper
   change, report it.
10. **Report** (your final message): files, the methods per case, the exemptions with reasons, the known-defect cases,
    the Pro test titles the integration agent must add to the `free-cdn` expected skips, the number of new tests, and
    open points.

## Conventions for the integration agent

- Run the group specs on WASM and Demo.Server with Pro, and Demo.Server with `Demo__Prerender=true`; run with
  `E2E_SHOW_DEFECTS=1` to check that each known-defect case fails for its stated reason.
- Add the Pro skips to the `free-cdn` mode of `expected-skips.json`, raise `minimumTests` in both modes, run the
  free-CDN and Pro passes of the release gate (or `npx playwright test` with the JSON report) and the coverage spec.
- `method-coverage.spec.js` must print no pending group at the end.

## Results of the lead phase

- `pro-data`: 12 cases, 3 known-defect cases; all 33 methods proven. Green on WASM, Demo.Server and Demo.Server with
  prerendering (Pro 3.14.0 dist).
- WAB-64 confirmed: F7 (local `WaCombobox.ReloadAsync` does not re-run the filter), F8 (`WaCombobox.ShowAsync` with a
  cancelled `wa-show` never completes), F9 (WA side: a second `ShowAsync` closes the combobox). F6: client-mode
  `ReloadAsync` completes and changes nothing (as documented: "server mode only"). F11: `CopySelectedRowsAsync` works
  from a real click with the clipboard permission on both hosts.
- New finding `WAB-65 L1`: `SetCustomValidityAsync` on the first render throws "does not implement
  WebAwesomeFormControl interface" before the element module has loaded; the interop's `setCustomValidity` (like
  `setProperty`/`getProperty`/`syncProperty`) does not wait for the definition as `invokeMethod` does since WAB-63.
  Library fix out of scope here; the expected-failure case flips when it is fixed.
- A call on a disposed wrapper fails with `InvalidOperationException` ("Failed to invoke method ..."), and Blazor keeps
  running.
- Interaction sweep on Demo.Server: the "Server timeout elapsed" failures came from the fake page clock firing the
  SignalR client's server timeout timer; `helpers\page-clock.js` holds that one timer. No library change.
