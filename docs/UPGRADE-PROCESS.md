# Automated Web Awesome Upgrade Process

How WebAwesome.Blazor tracks new releases of the upstream [Web Awesome](https://github.com/shoelace-style/webawesome) library, with no human interaction from ingesting a release to a review-ready task branch.

## Overview

The library is a thin, hand-authored Blazor wrapper layer; its life-cycle follows the upstream WA release train one version at a time (no skipping). Each upgrade is driven by the **Custom Elements Manifest** (CEM, `dist/custom-elements.json`) shipped inside every WA release — the authoritative, machine-readable description of every component's attributes, events, slots, documented methods, and CSS parts.

```
temp\download\webawesome_<ver>.zip
        │  Export-WaApiSurface.ps1
        ▼
   API surface JSON  ──┬── Compare-WaApiSurface.ps1 → change report (worklist)
                       └── expected-api-surface.json → parity tests (red/green driver)
```

The pipeline is orchestrated by the **`/wa-upgrade` skill** (`.claude\skills\wa-upgrade\SKILL.md`). The skill runs forked (`context: fork`) under the dedicated **`wa-orchestrator`** agent with the **Opus model pinned**, so the whole upgrade executes in an isolated context regardless of the invoking session's model, and delegates to:

| Piece | Location | Role |
|---|---|---|
| `wa-orchestrator` agent | `.claude\agents\` | Executes the forked skill (Opus); phases, delegation, verification, delivery |
| `Expand-WaRelease.ps1` | `tools\upgrade\` | Extracts a release zip to `temp\wa-src\<ver>` |
| `Export-WaApiSurface.ps1` | `tools\upgrade\` | CEM → deterministic per-component API surface JSON, plus each component's `@event` JSDoc names (`jsDocEvents`) and the `dist\events` event classes (`declaredEventTypes`) that corroborate the CEM events, plus each alias-typed attribute's resolved union (`resolvedType`, from the release's `.d.ts` files and, for lib.dom/chart.js types, the curated `tools\upgrade\external-type-aliases.json` - re-verify it every upgrade) |
| `Compare-WaApiSurface.ps1` | `tools\upgrade\` | Diffs two surfaces → JSON change report + Markdown summary, breaking changes flagged |
| `ApiSurfaceParityTests` | `src\WebAwesome.Blazor.Tests\ApiParity\` | Reflection tests: every CEM attribute/event/method has a wrapper counterpart |
| `wa-wrapper-engineer` agent | `.claude\agents\` | Implements wrappers from change-report excerpts, per the authoring contract |
| `New-WaDemoPages.ps1` | `tools\demo\` | Generates skeleton demo pages per component from a surface JSON (idempotent) |
| `WebAwesome.Blazor.Demo` | `src\` | WA-docs-shaped demo site (WASM, GitHub Pages via `demo.yml`); nav + API tables driven by the surface JSON |
| `wa-test-engineer` agent | `.claude\agents\` | Integration + breaking-change tests, suite to green |
| `infra-ops:jira-ops` / `infra-ops:plastic-ops` | plugins | Ticketing (WAB project) and VCS (Plastic branches, check-ins, merges) |

## Running an upgrade

Prerequisite: the release zip exists at `temp\download\webawesome_<version>.zip` (Pro sources, downloaded manually — the only step requiring a human, until download automation exists).

```
/wa-upgrade next            # upgrade to the next downloaded version above Version.props
/wa-upgrade 3.1.0           # explicit target (must be the immediate next version)
/wa-upgrade 3.1.0 --dry-run # analysis and plan document only
/wa-upgrade 3.1.0 --publish # additionally merge the task branch to the subtrunk when green
```

To walk the whole train (e.g. 3.0.0 → 3.10.0), run `/wa-upgrade next --publish` repeatedly; each run starts from the subtrunk, so a green, merged run leaves the workspace ready for the next.

## Pipeline phases

Phase numbers match the skill (0–6). `<major.minor>` is always taken from the **target** version; when a new train starts, the setup order is fixed: create the subtrunk off `/main`, check in the epic folder `tasks\WA-<major.minor>\` on the subtrunk, only then create the task branch, and check in the task folder with the plan there — so every changeset lands on the branch it belongs to. The WAB epic is created alongside. Versions are compared by SemVer precedence (`3.0.0-beta.6 < 3.0.0`, `3.2.0 < 3.10.0`).

A new train's subtrunk is created **off `/main`**, and only behind the **release gate**: the previous train must have been released first (its subtrunk head promoted to `/main`); otherwise the pipeline refuses to start the new train. Pending *patch* work (`x.y.z` on top of an already-released version) on the old subtrunk is the one allowed exception — patch releases are developed and tagged on their train's subtrunk (never promoted to `/main`), and a fix that matters to newer trains is merged from the older subtrunk directly into the newer one.

0. **Preflight** — clean Plastic workspace, on the subtrunk (`/main/WA-<major.minor>`) or this upgrade's own task branch (resume), baseline build + tests green, target version validated against the gradual-upgrade rule.
1. **Ticket & branch** (idempotent — reruns reuse existing) — JIRA Task `WA bindings for <version>` under the train's epic (e.g. WAB-1) with a `Source tag:` link, moved to In Progress; Plastic task branch `/main/WA-<major.minor>/WAB-<n>` created and switched to.
2. **Ingest & analyze** — surfaces exported for current and target versions (`Export-WaApiSurface.ps1` derives Pro-ness from the `[Pro]` markers in the reference docs bundled in the release zip since 3.3.0; `tools\upgrade\pro-components.json` is the pre-3.3.0 fallback), change report generated, plan document written to `tasks\WA-<major.minor>\WAB-<n>\upgrade-v<from>-to-v<to>-plan.md`. `inputs\WebAwesome` is refreshed to the target version's documentation by `Sync-WaDocs.ps1`: from the public GitHub tag (`packages/webawesome/docs/docs`) first, with components absent there (the Pro components) filled from the reference docs bundled in the release zip (since 3.3.0), and only then carried forward or flagged NEEDS CAPTURE for manual capture from `webawesome.com/docs/components/<name>`; `-DocsTagVersion` covers a release published before its public tag (see `inputs\README.md`). The refresh is checked in as its own changeset. `--dry-run` stops here, with the plan document checked in and the ticket/branch left ready for the real run.
3. **Arm parity & bump** — target surface copied to `ApiParity\expected-api-surface.json`, `parity-config.json` enabled and retargeted, `src\Version.props` + `README.md` bumped, demo app synced (CDN version in `index.html`, surface copy to `wwwroot\data\api-surface.json`). Failing parity tests now enumerate the exact remaining work.
4. **Implement** — breaking changes first (including deleting wrappers of removed components), then new components, then additive modifications; wrapper work fanned out to `wa-wrapper-engineer` agents in groups of ≤10 components.
5. **Test & docs** — `wa-test-engineer` adds integration and version-scoped breaking-change tests; `docs\MIGRATION-<version>.md` written when there are breaking changes; the `docs\CHANGELOG.md` entry for the version is drafted from the change report; demo page skeletons generated for new components (`tools\demo\New-WaDemoPages.ps1 -PruneRemoved`; the skeleton heading carries `ComponentBadges` and the emitted `ApiTable` renders the Pro/experimental badges and the canonical upstream doc link automatically — no per-page work for either); every new component assigned its upstream docs category in the demo's `ComponentCategoryMap` (source: `webawesome.com/docs/components`; an unmapped component falls into the "Other" nav bucket, which the sidebar e2e spec rejects); Debug + Release builds and full suite green.
6. **Deliver** — phased check-ins on the task branch (comment style: no leading ticket keys — the branch carries the ticket; 1–2 summary sentences plus bullet-pointed details), JIRA comment + transition to In Review; with `--publish`, merge to the subtrunk (never forced on conflict) and transition to Done.

Releasing to NuGet stays a deliberate maintainer act, gated by the **`/wa-release-preflight`** skill (automated gates: `tools\release\Test-WaReleasePreflight.ps1`): label `wa-blazor-<version>` on the released changeset — on `/main` after promotion for `<major.minor>.0` releases, directly on the train's subtrunk for patch releases — and sync the tag to the GitHub mirror. The mirror's release workflow (`.github\workflows\release.yml`, tag-triggered, independent of when the changeset itself was mirrored) rebuilds the tagged changeset, runs the tag-version guard and the test suite, and packs; when the `NUGET_PUBLISH_ENABLED` repository variable is `true`, it also publishes to nuget.org via Trusted Publishing (`NuGet/login`, policy on nuget.org for this repo and the `release.yml` workflow) and cuts the GitHub Release. Branch CI (`build.yml`) verifies pushes and PRs only and cannot publish.

## Testing strategy: functional equivalence

A wrapper is "functionally equal" to the original component when it renders the correct custom-element tag with the correct attributes, slots, and event subscriptions — the WA JavaScript then provides identical behavior by construction (the shadow DOM is upstream's, not ours). Verification is therefore layered:

1. **API surface parity (automated side-by-side, primary):** `ApiSurfaceParityTests` compare, per component, the CEM-declared surface of the *exact bound WA version* against the wrapper's reflected API — attributes ↔ `[Parameter]` properties (kebab→Pascal), documented methods ↔ `XxxAsync` wrappers. Events are checked on the rendered output instead (item 5). `EnumValueParityTests` checks the values: every `ToHtmlValue()` output of an enum parameter must be in the attribute's alias-resolved string-literal union (a `tokenListAttributes` entry such as `wa-tooltip` `trigger` is checked token by token, with every `[Flags]` combination), every union literal must be reachable, and an enum bound to an attribute whose type still resolves to no union fails unless listed in `unresolvedEnumAttributes`; a new external alias therefore fails until it is added to `external-type-aliases.json`. Intentional deviations must be recorded in `parity-config.json` with a reason (`ignoreReasons`); nothing is silenced implicitly. The tests are inert (`"enabled": false`) between upgrades so a stale surface never blocks unrelated work, and `ParityDataFiles_AreWellFormed` still guards the data files.
2. **Integration tests** (`Wa*IntegrationTests.cs`): defaults, parameter behavior, enum `ToHtmlValue()` mappings, event wiring.
3. **Breaking-change validation tests** (per version, pattern of `WaBreakingChangesValidationTests`): assert the post-upgrade API shape and defaults.
4. **EditForm integration tests** (bUnit, `src\WebAwesome.Blazor.Tests\Base\`): every `WaInputBase<T>`/`InputBase<T>`-derived form control is exercised inside a real EditForm — binding, change propagation, DataAnnotations validation lifecycle, validation CSS classes, custom validity interop. New form controls added by an upgrade must join this suite (Phase 5).
5. **Event binding and delivery guards** (render-based): `RenderedWrapperCatalog` renders every wrapper with bUnit, once plain and once per `EventCallback` with a no-op delegate, and reads the event handlers on the root element from the render tree. Wrappers are resolved by the tag they render, so secondary wrappers (`WaRange` → `wa-slider`) are covered, and bindings through constants, computed sequence numbers or base-class helpers are seen.
   - `EventCallbackBindingParityTests`: each callback must bind exactly the CEM event its name stands for (`wa-x` → `OnX`, plus `eventOverrides`), and each CEM event must be bound by every wrapper of the element; an `ignoredEvents` entry (`change`) must be bound by the value handling. A handler without the `on` prefix fails (Blazor's browser renderer rejects it). Native DOM events outside the CEM (`nativeDomEvents`), callbacks raised from another handler (`derivedEventCallbacks`), deliberately unbound inherited callbacks (`unboundEventCallbacks`) and bound events the CEM misses (`undeclaredBoundEvents`) are allowlisted with a reason; stale entries fail.
   - `EventBindingRegistrationTests`: every custom event a wrapper renders must be registered with `Blazor.registerCustomEventType` in `wwwroot\WebAwesome.Blazor.lib.module.js` (Blazor built-ins excepted), every registration must be bound by some wrapper and declared by the element's CEM, no registration may shadow a built-in, and the `numericValueEventAliases` keys must match `Constants.*EventAttribute`. An unregistered event is dropped silently in the browser, which is how the 3.0.0 event-delivery bug shipped.
   - `CemEventCorroborationTests`: the CEM events these checks rely on must be confirmed by the component's `@event` JSDoc and, for `wa-*` events, by an event class in `dist\events` (both recorded in `expected-api-surface.json` by the export, so the check runs from versioned inputs). The CEM analyzer invents events (`wa-data-grid` `request`); such an event goes into `cemOnlyEvents` and no check relies on it. An event the JSDoc omits but the compiled source dispatches goes into `sourceVerifiedEvents` and must be re-verified against the source every upgrade. Both need a reason; stale entries fail.
   - `ParityGuardSelfTests` proves each defect class (swapped name, deleted binding, constant name, base-class swap, missing `on` prefix, derived callback on a non-event, invented CEM event) on synthetic wrappers and data.
6. **Element-method invocation audit** (`ElementMethodInvocationTests`): every JS element method a wrapper invokes must be CEM-documented, a native DOM method, or allowlisted in `parity-config.json` (`extraElementMethods`) with a reason. Allowlisted methods are exactly the ones the CEM diff cannot track — re-verify each against the target source every upgrade; an entry no wrapper invokes fails. The scan covers every C# file of the library and any generic arguments, traces a method name passed through a helper's `string` parameter to the helper's call sites (`WaVideo`), and checks each file against the tags its classes or their descendants actually render (`WaInputBase` → every input element, `WaChartBase` → each chart); a file with invocations and no resolvable element fails.
7. **Interop module contract** (`InteropModuleContractTests`): every identifier the C# side invokes on `wwwroot\webawesome-interop.js` must be exported by it, every export must be invoked (or allowlisted with a reason), and no other global JS identifier may be invoked. A missing export fails only in the browser, and every test that mocks the interop layer passes regardless (the 3.11.0 icon library service).
8. **Public API snapshot** (`PublicApiSnapshotTests` + `approved-public-api.txt`): parity verifies us against upstream; the snapshot verifies our own consumers against us. Intentional changes are diffed, promoted into the baseline, and surfaced in the CHANGELOG (Phase 5).
9. **Browser end-to-end** (`tools\e2e\`, Playwright): the sweep visits every demo page asserting no unhandled errors; targeted specs cover checkbox/switch binding, theme switching, custom-event payload delivery and icon library registration. Run against the upgraded build before delivery (Phase 5).

## Pro-source containment

No Web Awesome Pro source code is ever committed. Release zips and everything extracted from them live exclusively under `temp\` (`temp\download`, `temp\wa-src`, `temp\wa-api`), which is ignored by both Plastic (`ignore.conf`) and git (`.gitignore`). The only CEM-derived artifact checked into the repo is `ApiParity\expected-api-surface.json` — pure API metadata (tag/attribute/event/slot/method names, types, defaults, and doc-string descriptions), no implementation code; the same information is publicly documented at webawesome.com/docs. Plan documents and migration guides describe API changes but must never embed upstream JS/TS/CSS source.

## Conventions honored

- **Branching** (`CONTRIBUTING.md`): work on `/main/WA-<train>/WAB-<n>`; monotonic promotion of subtrunks into `main`; release tags `wa-blazor-<version>`.
- **Ticketing**: WAB project, Tasks named `WA bindings for <version>` under the train epic, `Source tag:` GitHub link in the description; states In Progress → In Review → Done.
- **Code style**: `CLAUDE.md` (regions, explicit usings, central package management) and the wrapper technical standards in `docs\technical.md` (constant render-tree sequence numbers, API conventions, event contract).

## Coverage gates for newly added components and features

Every upgrade that adds components or major features must leave the following covered — the first three are enforced mechanically by tests, the rest are explicit Phase 5 steps in the skill:

| Addition | Required coverage | Enforced by |
|---|---|---|
| New component | Wrapper + integration tests + skeleton demo page | `ApiSurfaceParityTests` (wrapper), Phase 5 (tests, `New-WaDemoPages.ps1`) |
| New bound `wa-*` event | `onwa-` binding on the `OnX` callback + JS initializer registration (+ payload mapping if detail carries data) | `EventCallbackBindingParityTests`, `EventBindingRegistrationTests` |
| New element method wrapper | CEM-documented or allowlisted with reason | `ElementMethodInvocationTests` |
| New interop JS function | exported by `webawesome-interop.js`, invoked through the module, never as a global | `InteropModuleContractTests` (+ an e2e spec when it reaches Web Awesome module state) |
| New form control (`WaInputBase<T>`) | bUnit EditForm integration coverage | Phase 5 coverage rule (skill) |
| Any public API change | Snapshot baseline promotion + CHANGELOG mention | `PublicApiSnapshotTests` |
| New/removed components vs. curated showcases | Removed components pruned from `Pages\Showcases\`; new ones queued as curation follow-up (new form controls added to the form showcase immediately) | Phase 5 (skill) + demo build |
| Runtime behavior | Playwright sweep + targeted e2e specs green against the upgraded build | Phase 5 (skill) |

## Pending workarounds to re-verify every upgrade

Some fixes in this codebase work around upstream Web Awesome/Blazor-runtime behavior rather than a wrapper defect, and were discovered by manually driving the demo app in a real browser (bUnit and the parity tests cannot catch them — they're runtime/DOM-semantics issues, not API-surface mismatches). They are logged under `## [Unreleased]` → `### Fixed` in `docs\CHANGELOG.md` as they're found, each with a "Next-release check" note; at release time the `[Unreleased]` section is folded into the version's entry, so the notes live in the most recent release section between releases. **Phase 2 (Ingest and analyze) must search `docs\CHANGELOG.md` for all "Next-release check" notes not yet closed (the `[Unreleased]` section and the most recent release entry) and carry forward or close each item**:

- If the target release's source confirms the underlying JS method/behavior a workaround exists for is now correct upstream (e.g. `initialize()` genuinely exists on an element now, or `disconnect`/`reconnect` are real method names), remove the workaround and note the closure when drafting the CHANGELOG entry for the target version (Phase 5).
- If still broken upstream, carry the item forward unchanged into the new `[Unreleased]` section (or the target version's entry if the workaround needed a code change for this release).
- Run the demo app's Playwright suite (`tools\e2e\`) against the upgraded build before assuming any of these areas still work — the sweep test (`tools\e2e\sweep.spec.ts`) exercises every demo page for unhandled console/page errors, and the targeted tests cover the checkbox/switch binding and theme switching specifically.

## Troubleshooting

- **Parity test fails after regenerating the surface** — expected: that failure list *is* the worklist. Only edit `parity-config.json` for deviations that are deliberate.
- **`ConvertFrom-Json` errors in the scripts** — the scripts target Windows PowerShell 5.1; keep them ASCII-only (5.1 misreads BOM-less non-ASCII `.ps1` files) and remember the default slot is keyed `(default)` in surface JSON.
- **Current version has no zip** (e.g. a beta baseline): diff the target surface against the checked-in `ApiParity\expected-api-surface.json` instead of exporting the current version.
- **Betas in the future**: the tooling is version-string agnostic (`3.11.0-beta.1` works); only the download naming convention `webawesome_<version>.zip` must be kept.
