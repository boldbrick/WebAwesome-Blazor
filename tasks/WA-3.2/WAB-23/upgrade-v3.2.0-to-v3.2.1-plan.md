# Web Awesome 3.2.0 to 3.2.1 Upgrade Implementation Plan

## Overview
This document describes the plan for upgrading the Web Awesome Blazor wrapper library
from version 3.2.0 to 3.2.1. WA 3.2.1 is a **patch release**, developed on the existing
3.2 train subtrunk (`/main/WA-3.2`); `/main` is not involved.

## Analysis Summary
The API-surface diff between 3.2.0 and 3.2.1 was produced with the CEM tooling
(`Export-WaApiSurface` / `Compare-WaApiSurface`, report
`temp\wa-api\changes_3.2.0_to_3.2.1.md`):

| Metric | Count |
|---|---|
| New components | 0 |
| Removed components | 0 |
| Modified components | 0 |
| Breaking changes | 0 |

The two surface snapshots (`surface_3.2.0.json`, `surface_3.2.1.json`) are byte-identical
apart from the embedded `version` string — there are **no CEM-visible API changes** to any
component (62 components on both sides).

Per the upstream changelog (`inputs\WebAwesome\resources\changelog.md`), the sole functional
change in 3.2.1 is a build-script fix so that `llms.txt` and `dist/skills` are no longer
omitted from the Web Awesome Pro packages ([pr:2022]). This has no effect on the public
component API and therefore no effect on the Blazor wrappers.

The public documentation refresh at tag `v3.2.1` changed only three files:
- `components/animation.md` — example rewritten to drive the sandbox with `wa-combobox`
- `components/popup.md` — placement example rewritten to use `wa-combobox` instead of `wa-select`
- `resources/changelog.md` — the 3.2.1 entry

All three changes are documentation-example edits (no attribute, event, slot, method, or
enum changes).

## Breaking Changes to Address
None. `breakingChanges` in the change report is empty.

## New Components to Add
None. `addedComponents` in the change report is empty.

## Modified Components
None. `modifiedComponents` in the change report is empty.

## Element-method audit (CEM-invisible surface)
The `extraElementMethods` allowlist in `parity-config.json` was re-verified against the
extracted 3.2.1 source (`temp\wa-src\3.2.1\dist\components\*.d.ts`), as required each upgrade
because the CEM diff cannot see these:
- `wa-mutation-observer` / `wa-resize-observer`: `startObserver`, `stopObserver` — still present
  (`private startObserver;` / `private stopObserver;` in both `.d.ts`).
- `wa-relative-time`: `update` — inherited Lit `ReactiveElement` lifecycle method
  (only `updateTimeout` appears in `relative-time.d.ts`); still valid.
- `wa-page`: `visiblePixelsInViewport` — still present in `page.d.ts` (ignored method).

No allowlist entries need to change; the `ignoreReasons` notes are extended to record the
3.2.1 re-verification date.

## Per-file actions

### Parity harness (Phase 3)
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` — overwrite with
  `temp\wa-api\surface_3.2.1.json`.
- `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json` — set `targetWaVersion` to
  `3.2.1`, keep `enabled: true`; extend the two observer/`relative-time` `ignoreReasons`
  notes with the 3.2.1 re-verification date.

### Version bump (Phase 3)
- `src\Version.props` — `Version` 3.2.0 → 3.2.1; `AssemblyVersion`/`FileVersion`
  3.2.0.0 → 3.2.1.0 (patch changes). `InformationalVersion` derives automatically.
- `README.md` — update the three `3.2.0` references (alignment line, CDN CSS/loader URLs,
  wrapper-count sentence).

### Demo app sync (Phase 3)
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html` — CDN version 3.2.0 → 3.2.1.
- `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` — overwrite with the target surface.

### Documentation (Phase 5)
- `docs\CHANGELOG.md` — add a `## [3.2.1]` section (no breaking changes, no new components;
  Library/Changed notes: version alignment, upstream Pro packaging fix, doc refresh).
- No `docs\MIGRATION-3.2.1.md` — there are no breaking changes.
- Demo pages: `tools\demo\New-WaDemoPages.ps1 -PruneRemoved` — expected no-op (no added/removed
  components); demo project must still build.
- `src\WebAwesome.Blazor.Tests\PublicApi\approved-public-api.txt` — expected unchanged
  (no wrapper source changes); confirm `PublicApiSnapshotTests` stays green.

## Validation Checklist
- [ ] `extraElementMethods` re-verified against 3.2.1 source
- [ ] Parity harness re-armed to 3.2.1, `ApiSurfaceParityTests` green
- [ ] Version bumped in `Version.props` and `README.md`
- [ ] Demo CDN + `api-surface.json` synced to 3.2.1
- [ ] `CHANGELOG.md` 3.2.1 entry added
- [ ] Public API snapshot unchanged (no unexplained diff)
- [ ] Debug + Release build green (incl. demo)
- [ ] `dotnet test` green (net9.0 + net10.0)
- [ ] Playwright e2e sweep green

## Risk Assessment
- **Low risk overall.** No wrapper code changes; this is a version-alignment/patch upgrade.
  The only substantive verification is confirming the parity harness stays green against the
  re-armed 3.2.1 surface and that the public API snapshot does not drift.

## Notes for Implementation
- No `wa-wrapper-engineer` delegation is required (no added/modified/removed components).
- No new form controls, so no additional EditForm test coverage is required.
- Keep the Pro-source rule: nothing from `temp\` is checked in except the API surface JSON.
