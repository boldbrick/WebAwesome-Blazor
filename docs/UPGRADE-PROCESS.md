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
| `Export-WaApiSurface.ps1` | `tools\upgrade\` | CEM → deterministic per-component API surface JSON |
| `Compare-WaApiSurface.ps1` | `tools\upgrade\` | Diffs two surfaces → JSON change report + Markdown summary, breaking changes flagged |
| `ApiSurfaceParityTests` | `src\WebAwesome.Blazor.Tests\ApiParity\` | Reflection tests: every CEM attribute/event/method has a wrapper counterpart |
| `wa-wrapper-engineer` agent | `.claude\agents\` | Implements wrappers from change-report excerpts, per the authoring contract |
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

Phase numbers match the skill (0–6). `<major.minor>` is always taken from the **target** version; when a new train starts, the subtrunk, WAB epic, and `docs\prompts\WA-<major.minor>\` folder are created as part of preflight/ticketing. Versions are compared by SemVer precedence (`3.0.0-beta.6 < 3.0.0`, `3.2.0 < 3.10.0`).

0. **Preflight** — clean Plastic workspace, on the subtrunk (`/main/WA-<major.minor>`) or this upgrade's own task branch (resume), baseline build + tests green, target version validated against the gradual-upgrade rule.
1. **Ticket & branch** (idempotent — reruns reuse existing) — JIRA Task `WA bindings for <version>` under the train's epic (e.g. WAB-1) with a `Source tag:` link, moved to In Progress; Plastic task branch `/main/WA-<major.minor>/WAB-<n>` created and switched to.
2. **Ingest & analyze** — surfaces exported for current and target versions, change report generated, plan document written to `docs\prompts\WA-<major.minor>\upgrade-v<from>-to-v<to>-plan.md`. `inputs\WebAwesome` is refreshed to the target version's documentation: from the public GitHub tag (`packages/webawesome/docs/docs`) for free components, and from `webawesome.com/docs/components/<name>` for Pro components absent there (as of 3.1.0: `combobox`, `page`); the refresh is checked in as its own changeset. `--dry-run` stops here, with the plan document checked in and the ticket/branch left ready for the real run.
3. **Arm parity & bump** — target surface copied to `ApiParity\expected-api-surface.json`, `parity-config.json` enabled and retargeted, `src\Version.props` + `README.md` bumped. Failing parity tests now enumerate the exact remaining work.
4. **Implement** — breaking changes first (including deleting wrappers of removed components), then new components, then additive modifications; wrapper work fanned out to `wa-wrapper-engineer` agents in groups of ≤10 components.
5. **Test & docs** — `wa-test-engineer` adds integration and version-scoped breaking-change tests; `docs\MIGRATION-<version>.md` written when there are breaking changes; Debug + Release builds and full suite green.
6. **Deliver** — phased check-ins on the task branch (established comment style), JIRA comment + transition to In Review; with `--publish`, merge to the subtrunk (never forced on conflict) and transition to Done.

Releasing to NuGet stays a deliberate manual act: promote the subtrunk to `/main` and tag `wa-blazor-<version>` — the GitHub mirror's CI (`.github\workflows\build.yml`) packs on that tag.

## Testing strategy: functional equivalence

A wrapper is "functionally equal" to the original component when it renders the correct custom-element tag with the correct attributes, slots, and event subscriptions — the WA JavaScript then provides identical behavior by construction (the shadow DOM is upstream's, not ours). Verification is therefore layered:

1. **API surface parity (automated side-by-side, primary):** `ApiSurfaceParityTests` compare, per component, the CEM-declared surface of the *exact bound WA version* against the wrapper's reflected API — attributes ↔ `[Parameter]` properties (kebab→Pascal), events ↔ `EventCallback` parameters (`wa-x` → `OnX`), documented methods ↔ `XxxAsync` wrappers. Intentional deviations must be recorded in `parity-config.json` with a reason (`ignoreReasons`); nothing is silenced implicitly. The tests are inert (`"enabled": false`) between upgrades so a stale surface never blocks unrelated work, and `ParityDataFiles_AreWellFormed` still guards the data files.
2. **Integration tests** (`Wa*IntegrationTests.cs`): defaults, parameter behavior, enum `ToHtmlValue()` mappings, event wiring.
3. **Breaking-change validation tests** (per version, pattern of `WaBreakingChangesValidationTests`): assert the post-upgrade API shape and defaults.
4. **Render-output tests (future):** bUnit-based golden rendering of each wrapper against expected markup would close the remaining gap (attribute *values*, sequence-number regressions). Not implemented yet; the parity layer covers names and presence.

## Pro-source containment

No Web Awesome Pro source code is ever committed. Release zips and everything extracted from them live exclusively under `temp\` (`temp\download`, `temp\wa-src`, `temp\wa-api`), which is ignored by both Plastic (`ignore.conf`) and git (`.gitignore`). The only CEM-derived artifact checked into the repo is `ApiParity\expected-api-surface.json` — pure API metadata (tag/attribute/event/slot/method names, types, defaults, and doc-string descriptions), no implementation code; the same information is publicly documented at webawesome.com/docs. Plan documents and migration guides describe API changes but must never embed upstream JS/TS/CSS source.

## Conventions honored

- **Branching** (`CONTRIBUTING.md`): work on `/main/WA-<train>/WAB-<n>`; monotonic promotion of subtrunks into `main`; release tags `wa-blazor-<version>`.
- **Ticketing**: WAB project, Tasks named `WA bindings for <version>` under the train epic, `Source tag:` GitHub link in the description; states In Progress → In Review → Done.
- **Code style**: `CLAUDE.md` (regions, explicit usings, central package management, constant render-tree sequence numbers per `docs\prompts\WA-3.0\fix-render-tree-numbering.md`).

## Troubleshooting

- **Parity test fails after regenerating the surface** — expected: that failure list *is* the worklist. Only edit `parity-config.json` for deviations that are deliberate.
- **`ConvertFrom-Json` errors in the scripts** — the scripts target Windows PowerShell 5.1; keep them ASCII-only (5.1 misreads BOM-less non-ASCII `.ps1` files) and remember the default slot is keyed `(default)` in surface JSON.
- **Current version has no zip** (e.g. a beta baseline): diff the target surface against the checked-in `ApiParity\expected-api-surface.json` instead of exporting the current version.
- **Betas in the future**: the tooling is version-string agnostic (`3.11.0-beta.1` works); only the download naming convention `webawesome_<version>.zip` must be kept.
