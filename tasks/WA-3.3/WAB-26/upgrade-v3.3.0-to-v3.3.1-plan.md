# Web Awesome 3.3.0 to 3.3.1 Upgrade Implementation Plan

## Overview
Implementation plan for upgrading the WebAwesome.Blazor wrapper library from Web Awesome 3.3.0 to 3.3.1. This is a **patch release on the existing WA-3.3 train**, developed on the subtrunk `/main/WA-3.3` (task branch `/main/WA-3.3/WAB-26`); per the patch release rule `/main` is not involved.

## Analysis Summary
The authoritative worklist is the CEM-derived API surface diff
`temp\wa-api\changes_3.3.0_to_3.3.1.json` / `.md`, produced from
`surface_3.3.0.json` vs `surface_3.3.1.json`:

| Metric | Count |
|---|---|
| New components | 0 |
| Removed components | 0 |
| Modified components | 0 |
| Breaking changes | 0 |

The two surface snapshots are **byte-identical apart from the `version` field** — 3.3.1 introduces **no CEM-visible API changes** (73 components in both). Confirmed by direct diff of the surface JSONs (only the `version` line differs).

Upstream, 3.3.1 is a packaging-only patch. The sole entry in the Web Awesome changelog for 3.3.1:

> Removed a `preinstall` script in `webawesome-pro` that was causing issues in some package managers.

The GitHub compare `v3.3.0...v3.3.1` touches 7 files, only one of which lives under the documentation folder (`resources/changelog.md`); the rest are packaging/CI metadata (`package.json`, `package-lock.json`, `VERSIONS.txt`, a removed CI workflow). No component `.d.ts`, CSS, or CEM content changed.

**Conclusion:** no wrapper code changes are required. The upgrade is a version/harness/documentation bump plus the mandatory element-method re-verification.

## Breaking Changes to Address
None.

## New Components to Add
None.

## Component Enhancements
None.

## Intentional Deviations
No new deviations. Existing `parity-config.json` deviations carry forward unchanged (surface is identical, so all existing overrides/ignores remain exactly valid).

## Element Method Audit (hard rule — CEM diff cannot see these)
Re-verified every `extraElementMethods` allowlist entry against the extracted 3.3.1 source (`temp\wa-src\3.3.1\dist\components\*`):

- `wa-mutation-observer` → `startObserver`, `stopObserver`: still present as private methods in `mutation-observer/mutation-observer.d.ts` (lines 33-34). Valid.
- `wa-resize-observer` → `startObserver`, `stopObserver`: still present as private methods in `resize-observer/resize-observer.d.ts` (lines 21-22). Valid.
- `wa-relative-time` → `update`: `relative-time.d.ts` still declares only `updateTimeout`; `update()` remains the inherited Lit `ReactiveElement` lifecycle method (not in the component `.d.ts`, as recorded). Valid.

Action: append the 3.3.1 verification date to the two `ignoreReasons` entries covering these allowlists.

## Per-file Actions
- `inputs\WebAwesome\resources\changelog.md` — add the 3.3.1 section (DONE, checked in as its own changeset per `inputs\README.md`).
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` — overwrite with `surface_3.3.1.json`.
- `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json` — `targetWaVersion` → `3.3.1`; append 3.3.1 re-verification note to the two `extraElementMethods` `ignoreReasons`.
- `src\Version.props` — `Version` → `3.3.1`; `AssemblyVersion`/`FileVersion` → `3.3.1.0` (patch changed).
- `README.md` — bump version references to 3.3.1.
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html` — WA CDN version → 3.3.1.
- `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` — overwrite with `surface_3.3.1.json`.
- `docs\CHANGELOG.md` — add `## [3.3.1]` section; fold any `## [Unreleased]` items in.
- `src\WebAwesome.Blazor.Tests\PublicApi\approved-public-api.txt` — expected unchanged (no API delta); promote only if a version-driven diff appears and is fully explained.
- No migration doc (no breaking changes).

## Validation Checklist
- [ ] Element-method allowlist re-verified against 3.3.1 source (done in analysis)
- [ ] Parity harness re-armed to 3.3.1; `ApiSurfaceParityTests` green
- [ ] Version bumped (Version.props, README, demo index.html)
- [ ] Demo api-surface.json refreshed; demo builds
- [ ] `dotnet build` Debug and Release green (incl. demo)
- [ ] `dotnet test` green on net9.0 + net10.0
- [ ] Public API snapshot unchanged (or diff fully explained + promoted)
- [ ] CHANGELOG entry added
- [ ] Browser e2e sweep green against WA 3.3.1 (public jsdelivr CDN, verified available)

## Risk Assessment
- **Very low**: no API surface change, no code change. Risk is confined to the mechanical version/harness bump and demo/CDN sync. Browser e2e is the meaningful runtime check (WA 3.3.1 is on the public CDN; note WA 3.3.0 was never CDN-published, so the sweep must run against 3.3.1).

## Notes for Implementation
- WA 3.3.1 IS available on the public jsdelivr CDN (verified 200); the browser sweep runs against the default free CDN.
- All shell commands use PowerShell (hard rule).
- Pro sources remain under `temp\`; only the API surface JSON is checked in.
