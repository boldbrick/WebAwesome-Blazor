# WAB-62 WA Inspector: DevTools extension for UI review of wa- elements

Epic: WAB-49. Branch: `/main/WA-3.14/WAB-62` off cs:385. Version: the owner decided on 2026-10-21 to bump the packages to 3.14.0.2 in this
ticket (the first plan had no NuGet version bump).

## Scope

A reviewer (developer or tester) opens a running page in Chrome or Edge, opens DevTools, and uses the
"Web Awesome" panel to:

1. See the tree of `wa-` elements and of plain elements with Web Awesome utility classes (`wa-stack`,
   `wa-cluster`, `wa-gap-*` and so on). Other markup is collapsed.
2. Edit attributes, CSS custom properties, CSS part styles and utility classes. The forms come from the
   Web Awesome custom elements manifest (CEM). Changes show live and stay applied while the reviewer navigates.
3. Change theme tokens (`--wa-*`) for the whole page.
4. Add a comment to any element, also to elements that are not Web Awesome elements.
5. Give each change-list item a color and a number. Show or hide numbered outlines on the page.
6. Export the change list (JSON and Markdown) with screenshots for a coding agent.

The Blazor wrappers emit opt-in, dev-only placement metadata (`data-wablazor-*`). A Claude Code plugin with
a skill applies an exported change list to the source code.

## Architecture

| Part | Location | Notes |
|---|---|---|
| Placement metadata | `src/WebAwesome.Blazor/Base/WaInspectorMetadata.cs` | Opt-in through `WebAwesomeOptions.EmitInspectorMetadata` |
| Extension (MV3, pure JS, no build step) | `tools/wa-inspector/extension/` | DevTools panel, standalone tab, side panel |
| Metadata generator | `tools/wa-inspector/Update-WaInspectorMetadata.ps1` | CEM + utility CSS + theme tokens to one JSON |
| Claude Code plugin | `tools/wablazor-marketplace/plugins/wablazor/` | Plugin `wablazor`, skill `wa-review-apply`; marketplace `tools/wablazor-marketplace/` |
| Browser tests | `tools/e2e/inspector/` | Own Playwright config, the extension loaded in Chromium |
| Documentation | `docs/inspector/README.md`, `docs/inspector/user-guide.md` | Owner instruction 2026-10-20: docs under `docs\` with a simple user guide |

### Placement identity (the spike)

Each wrapper instance computes, once, the path from the render root to its own component frame:
for each level, the type of the rendering parent and the sequence numbers of the frames (elements,
regions) that enclose the child frame in the parent's current render tree. Razor emits sequence numbers
as compile-time constants per source position, and a loop reuses them. So:

- all instances that one loop renders share one placement;
- the same markup at another position gets another placement.

Emitted attributes on the wrapper's root element:

| Attribute | Content |
|---|---|
| `data-wablazor-component` | Wrapper type name, for example `WaButton` |
| `data-wablazor-chain` | Readable component chain without framework types, for example `MainLayout>OrderPage>WaCard>WaButton` |
| `data-wablazor-placement` | Hash of the full path (all levels) |
| `data-wablazor-site` | Hash of the last level only (the source site in the rendering parent) |

The renderer internals are read through reflection (`ComponentBase._renderHandle`, `RenderHandle._renderer`,
`Renderer.GetComponentState`, `Renderer.GetCurrentRenderTreeFrames`). If one lookup fails (another .NET
version, trimming), the wrapper emits only `data-wablazor-component`.

## Autopilot decisions (owner away 2026-10-20)

Basis rungs: 1 = owner instruction, 2 = owner constraint, 3 = repo rule, 4 = pattern, 5 = project decision,
6 = best practice.

| # | Question | Decision | Basis | Rejected | Reversal cost | Confidence |
|---|---|---|---|---|---|---|
| A1 | Epic for the Jira task | WAB-62 under WAB-49 (open epic of the WA-3.14 train) | 1 (use the WA-3.14 branch), 5 | A new epic | Low (move issue) | Medium |
| A2 | Merge into `/main/WA-3.14` | Not done. The task branch is checked in and ready to merge | Park list: shared integration branch; 3.14.0.1 waits for its label | Merge as earlier tickets did | None | High |
| A3 | Opt-in switch | `WebAwesomeOptions.EmitInspectorMetadata` (config `WebAwesome:EmitInspectorMetadata`), default false; a process-wide flag that `AddWebAwesome` sets | 6: zero cost when off; wrappers have no injected services | Per-renderer lookup through reflection on every component | Low | Medium |
| A4 | Exact `file:line` source map from generated Razor code | Not built. Fuzzy locator only | 1 (fuzzy locator is enough) | Build-time map from `.razor.g.cs` | Medium | High |
| A5 | Data-driven attributes | Detected at runtime in the browser: distinct values across the instances of one placement, and value changes that the page makes. Change list offers "remap value X to Y". No public mapping helper API | 1 (unsupported cases may be documented), 6 (smallest thing) | A `WaValueMap` public API | Low | Medium |
| A6 | MCP bridge | Not built. File hand-off: the export goes to the Downloads folder, the skill reads the folder | 1 (simple, no dependencies) | A Node MCP server | Low | High |
| A7 | Side panel | Added. The same panel page runs in DevTools, in a tab (`?tabId=`), and in the browser side panel | 6 (cheap, same code) | DevTools only | Low | Medium |
| A8 | Element metadata in the extension | Generated from the Pro dist CEM of 3.14.0 (all components, descriptions included) | 5 (WAB-61 D4: the owner accepts shipping Pro-zip docs) | Free CEM only | Low (regenerate) | Medium |
| A9 | C# parameter names in the change list | Not generated. The skill maps attribute names to wrapper parameters (PascalCase, enum members) and reads the package docs | 6 | A generated attribute-to-parameter map | Low | High |
| A10 | Browser tests | Separate Playwright config `tools/e2e/playwright.inspector.config.js`, `npm run test:inspector`; not part of the standard e2e sweep and the preflight | 4 (existing Playwright project), 6 (a persistent context with the extension cannot share the standard fixture) | Specs in `tools/e2e/tests/` | Low | Medium |
| A11 | Demo apps | Metadata stays off in the demo configuration. Turn it on for Demo.Server with the environment variable `WebAwesome__EmitInspectorMetadata=true` (the inspector browser tests do this) | 6: the standard e2e sweep and the preflight keep testing the default configuration | On in `appsettings.Development.json` | Low | Medium |
| A12 | Convenience icons (`StartIconName` and so on) | No own metadata. The extension shows the parent wrapper as owner | 6 | Metadata on each slot icon | Low | High |
| A13 | `docs/inspector/` in the NuGet docs bundle | Not bundled. The `DocPackContent` list of `WebAwesome.Blazor.csproj` keeps repository and maintainer docs out | 4 (`docs\contributing\` stays out too) | Bundle it with the package docs | Low | Medium |
| A14 | Name and place of the plugin marketplace | `wablazor-marketplace` in its own folder `tools/wablazor-marketplace/` (`.claude-plugin/marketplace.json`, plugins under `plugins/`); install with `claude plugin install wablazor@wablazor-marketplace`. Changed 2026-10-21 by the owner (was `webawesome-blazor-tools` in `tools/wa-inspector/`) | 1 | A marketplace inside `tools/wa-inspector/` | Low | High |
| A15 | Skill invocation | `/wablazor:wa-review-apply <export folder>` (plugin `wablazor`, skill `wa-review-apply`). Changed 2026-10-21 by the owner: the plugin `wablazor` holds all developer skills of WebAwesome.Blazor (was `wa-inspector`) | 1, 4 (plugin skill names) | A plugin per tool; a project skill under `.claude/skills` | Low | High |
| A16 | Screenshots in tab mode | The export brings the inspected tab to the front while it takes the screenshots, then shows the panel tab again; background.js refuses to capture a hidden tab | 6 (`captureVisibleTab` takes the active tab of the window) | Tell the reviewer to use DevTools mode for an export | Low | High |
| A17 | Removal of a non-boolean attribute (an override "(not set)", or the page value "not set" after an override is deleted) | The panel stores `kind` and `elementDefault` in each attribute operation; content.js writes the element default instead of a removal when the attribute is on a `wa-` element now and the default is a literal. It shows that value as "not set". Booleans keep present and absent. Operations without these fields (older sessions) remove the attribute | 3 (the sticky attribute rule, `WaAttributeMemory`) | Remove the attribute (Lit sets the property to null) | Low | High |
| A18 | Publish into a shared marketplace folder | `tools/wablazor-marketplace/Publish-WaBlazorPlugin.ps1`, after the deploy scripts of the team: the target folder is required (`-Destination` or the environment variable `WABLAZOR_PLUGIN_MARKETPLACE`), with no default path; versioned folders `<name>_v<version>`; one `marketplace.json` entry per plugin; `-BumpVersion`; `-WhatIf`; `claude plugin validate --strict` on the plugin. Not taken: plugin imports (no plugin here uses them) and the side-by-side mode (`claude plugin validate` refuses two entries with the same name) | 1 (owner request 2026-10-21), 4 (team scripts) | A copy of the team scripts with their default target | Low | High |

## Unsupported (documented)

- A data-driven value that is the same on all instances on the current page looks like a literal value.
- One instance of a loop cannot get its own literal value. Use a comment or a value remap.
- Property-only settings (no attribute) and slot content are read-only.
- `wa-` elements inside the shadow root of another `wa-` element are shown as internal and are not edited.
- Placements are stable for one build. A change of the Razor markup can change them; old exports stay
  readable because the locator also carries the chain, the classes and a text snippet.

## Test approach

- bUnit: metadata off by default; on: attributes present; loop instances share a placement; two call sites
  differ; the same component inside two parents has the same site but another placement; the chain hides
  framework types. Runs in a non-parallel collection because the switch is process-wide.
- Playwright (Chromium with the extension, against Demo.Server): tree, attribute override, persistence across
  navigation, CSS custom property, utility class, theme token, comment on a plain element, markers, data-driven
  detection, export content.
- Build Debug and Release, full test run on both TFMs, public API snapshot.

## Result

### What changed

- **Blazor side.** `WebAwesomeOptions.EmitInspectorMetadata` (default `false`), the process-wide switch that
  `AddWebAwesome` sets, and `Base\WaInspectorMetadata.cs`. `OpenWaElement`, `WaLayoutBase` and the layout wrappers
  with their own root element (`WaFlank`, `WaFrame`, `WaGrid`, `WaSplit`, `WaText`, `WaVisuallyHidden`) write the
  `data-wablazor-*` attributes. `WaInspectorMetadataTests`; public API snapshot updated.
- **Extension.** `tools/wa-inspector/extension/` (MV3, plain JavaScript): DevTools panel, side panel and tab mode,
  the element tree, the forms from `metadata/wa-metadata.json`, change-list items with numbers, colors and outlines,
  the session per origin, value remaps, theme tokens, comments, the export with screenshots.
- **Panel split.** On the Elements tab, a divider between the tree and the details (`panel.html`, `panel.css`,
  `panel.js`): drag it, or focus it and use the arrow keys; a double-click resets it. The halves are stacked in the
  side panel and in a narrow panel, and side by side otherwise. The panel keeps one size for each layout in its
  `localStorage`. Test: `tools/e2e/inspector/layout.spec.js`.
- **Version.** `src/Version.props` 3.14.0.2 (`Version`, `AssemblyVersion`, `FileVersion`); the default CDN version
  stays 3.14.0. `docs/CHANGELOG.md`: `[Unreleased]` folded into `[3.14.0.2] — 2026-10-21`. `README.md` "Version
  alignment": the tag row of the 3.14.0 bindings revisions is `wa-blazor-3.14.0.x`.
- **Generator.** `tools/wa-inspector/Update-WaInspectorMetadata.ps1` (ASCII-only) makes the metadata from the API
  surface and the 3.14.0 release.
- **Claude Code plugin.** `tools/wablazor-marketplace/plugins/wablazor/` (plugin `wablazor`) with the skill
  `wa-review-apply`, the marketplace file `tools/wablazor-marketplace/.claude-plugin/marketplace.json`, the publish
  script `tools/wablazor-marketplace/Publish-WaBlazorPlugin.ps1` and `tools/wablazor-marketplace/README.md`.
- **Demo harness.** `/testing/inspector` (`Pages/Testing/InspectorHarness.razor`, `InspectorHarnessAction.razor`),
  in `HARNESS_ROUTES`, so the standard e2e sweep visits it too.
- **Browser tests.** `tools/e2e/inspector/` (14 tests), `tools/e2e/playwright.inspector.config.js`,
  `npm run test:inspector`.
- **Docs.** `docs/inspector/README.md` and `docs/inspector/user-guide.md`; `tools/wa-inspector/README.md` points to
  them. `docs/technical/technical.md` ("Inspector metadata", and `docs\inspector\` out of the package docs),
  `docs/contributing/UPGRADE-PROCESS.md` and `.claude/skills/wa-upgrade/SKILL.md` (regenerate the metadata after an
  upgrade), `docs/CHANGELOG.md` (`[3.14.0.2]`, two `Added` lines), `README.md` (one paragraph), `tools/e2e/README.md`.

### Verification

- `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` and `-p:Configuration=Release`: 0 warnings, 0 errors.
- `dotnet test src/WebAwesome.slnx`: WebAwesome.Blazor.Tests 1407 passed on net9.0 and on net10.0;
  WebAwesome.Blazor.Extensions.Tests 191 passed on net9.0 and on net10.0; 0 failed, 0 skipped.
- Inspector suite against Demo.Server on port 5100 with `WebAwesome__EmitInspectorMetadata=true`:
  `npx playwright test -c playwright.inspector.config.js`, 12 passed, two runs. The new sticky-default test fails
  without the fix (the property is null) and passes with it. After the panel split and the version bump: 14 passed
  (the two tests of `layout.spec.js` included), Demo.Server on port 5117 (`--urls`, `DEMO_BASE_URL`).
- After the version bump (3.14.0.2): Debug and Release builds with 0 warnings, the packages
  `WebAwesome.Blazor.3.14.0.2.nupkg` and `WebAwesome.Blazor.Extensions.3.14.0.2.nupkg`; the same test counts as above.
  Release preflight in docs-only mode (`Test-WaReleasePreflight.ps1 -SkipBuild -SkipE2E`): all version, changelog and
  migration gates pass (`version-props`, `assembly-version`, `changelog-entry`, `changelog-unreleased-folded`,
  `migration-doc` and the others). Before the check-in, only `workspace-clean` fails, because of the pending changes.
- Plugin and marketplace: `claude plugin validate <folder> --strict` passes for `tools/wablazor-marketplace` and
  `tools/wablazor-marketplace/plugins/wablazor`. `Publish-WaBlazorPlugin.ps1` ran end to end in Windows PowerShell 5.1
  and PowerShell 7 against throwaway marketplace folders: `-WhatIf`, first publication, no-op rerun, same version with
  other files, `-BumpVersion`, collapse of duplicate entries, a new `marketplace.json`, and the error cases. An install
  from the result with `claude plugin install` in an isolated configuration folder worked.

### Deviations from the plan

- The documentation is under `docs/inspector/` (owner instruction), not under `tools/wa-inspector/`.
- The decisions A13 to A18 above came up during the work.
- 2026-10-21, owner request: the marketplace moved to its own folder `tools/wablazor-marketplace/` with the name
  `wablazor-marketplace`. The plugin `wa-inspector` became the general plugin `wablazor` (the developer skills of
  WebAwesome.Blazor) in `tools/wablazor-marketplace/plugins/wablazor/`, moved with `cm mv`. The skill invocation is now
  `/wablazor:wa-review-apply`. `tools/wa-inspector/` keeps the extension, the generator and the README. The publish
  script was added (A14, A15, A18).
- The skill tells the coding agent to change part styles with app CSS `::part()`, not with `ShadowStyles`
  (`ShadowStyles` styles only the output of the app's own JavaScript functions).
- The browser tests check the downloads through wrappers of `chrome.downloads.download` and `captureVisibleTab` in
  the service worker, because Playwright saves downloads under GUID names. The tests need the full Chromium build
  (channel `chromium`); the headless shell loads no extensions.
- Test hook `page.drawnMarkers` in content.js: the overlay shadow root is closed, so a test cannot count the outlines.

### Defects the browser tests found, and their fixes

1. content.js ignored a page write of the value that the override showed already (a Blazor render, a hot reload).
   The source value stayed old, the item stayed `pending`, and a delete of the change restored the old value. Fix:
   `apply()` discards the mutation records of its own writes (`observer.takeRecords`), and an echo guard ignores Lit
   writing the override value back when the element upgrades or updates.
2. The structural key contained the classes of `<html>` and `<body>`, so a theme or color-scheme switch made the
   items on plain elements match nothing. Fix: `<html>` and `<body>` give their tag only.
3. In tab mode, the export screenshots showed the panel tab (`captureVisibleTab` takes the active tab). Fix: A16.
4. `refreshTree` in panel.js lost a tree change that came while a tree read was open. Fix: it reads once more.
   No test covers this.
5. A restore of the page value "not set" after a delete of an override, and an override "(not set)", removed a
   non-boolean attribute, and Lit set the property to null. Fix: A17. Test: "a removed non-boolean attribute gets the
   element default, not null" (`overrides.spec.js`).

### Open points

- Echo guard heuristic: it waits at most 15 s for an element to upgrade after the inspector writes an override. An
  element that upgrades later can show its echo as a page write. A page write of the same value within 50 ms of the
  inspector's write on an upgraded element counts as an echo.
- `item-NN.png` crops to the first element that the item matches.
- The DevTools mode and the side panel have no automated tests; the tests use the tab mode.
- `PACKAGE-SKILL.md` of WebAwesome.Blazor does not mention `EmitInspectorMetadata`. The owner refreshes it with
  `/docpack:docpack-skill`.
- The DocPack comment in `WebAwesome.Blazor.csproj` names only `docs\contributing` as excluded; `docs\inspector` is
  out too (A13).
- `docs/technical/technical.md` line 9 says `Layouts\`, but the folder is `Layout\` (before this task).
- The scoped-CSS `obj` path in the skill comes from SDK knowledge; this repository has no `.razor.css` to verify it.
