---
name: wa-upgrade
description: Runs the automated Web Awesome version upgrade pipeline for the Blazor bindings — from JIRA ticket and Plastic branch creation, through CEM-driven analysis and wrapper implementation, to green tests, migration docs, and check-in. Invoke as /wa-upgrade <target-version> (e.g. /wa-upgrade 3.1.0), or /wa-upgrade next to auto-pick the next downloaded release. Add --publish to also merge to the subtrunk after completion.
---

# Web Awesome Bindings Upgrade Pipeline

You are executing a **no-touch upgrade** of the WebAwesome.Blazor bindings to a new Web Awesome release. Work autonomously; only stop for genuinely unresolvable conflicts. Full background: `docs\UPGRADE-PROCESS.md`. Wrapper authoring contract: `docs\prompts\WA-3.0\build-wa-blazor-wrappers.md` + `CLAUDE.md`.

## Arguments

- `<target-version>` — e.g. `3.1.0`. `next` (or no argument) means: the **lowest** version in `temp\download\webawesome_*.zip` that is greater than the current version in `src\Version.props`.
- `--publish` — after everything is green and checked in, also merge the task branch to the subtrunk.
- `--dry-run` — stop after Phase 3 (analysis); produce the plan document only.

**Gradual-upgrade rule (hard):** never skip a downloaded release. If the requested target is not the immediate next downloaded version above the current one, refuse and state which version must be done first.

## Phase 0 — Preflight

1. Read current version from `src\Version.props` (`<Version>` property). Determine target version per the rules above; verify `temp\download\webawesome_<target>.zip` exists.
2. Via the `infra-ops:plastic-ops` skill (plastic-operator agent): confirm the workspace is **clean** and get the current branch. The upgrade must start from the subtrunk `/main/WA-<major.minor>` (e.g. `/main/WA-3.0`). If on a task branch with no pending changes, switch to the subtrunk. If the workspace is dirty, stop and report.
3. Baseline: `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` and `dotnet test src/WebAwesome.slnx` must be green before any change. If not, stop and report.

## Phase 1 — Ticket and branch

1. Via the `infra-ops:jira-ops` skill (jira-operator agent):
   - Find the epic for the current WA train in project **WAB** (e.g. WAB-1 "Web Awesome 3.0" for 3.x). Create one if a new major/minor train starts.
   - Create a Task under that epic: summary `WA bindings for <target-version>`, description containing a `Source tag:` line linking `https://github.com/shoelace-style/webawesome/tree/v<target-version>` and a short scope note. Transition it to **In Progress**.
2. Via `infra-ops:plastic-ops`: create the task branch `/main/WA-<major.minor>/WAB-<n>` (n = the JIRA issue number just created) off the subtrunk and switch to it.

## Phase 2 — Ingest and analyze

Run the tooling (Windows PowerShell, from repo root):

```powershell
tools\upgrade\Expand-WaRelease.ps1 -Version <target>          # optional full source tree for reference
tools\upgrade\Export-WaApiSurface.ps1 -Version <current>
tools\upgrade\Export-WaApiSurface.ps1 -Version <target>
tools\upgrade\Compare-WaApiSurface.ps1 `
    -FromPath temp\wa-api\surface_<current>.json `
    -ToPath temp\wa-api\surface_<target>.json `
    -MarkdownPath temp\wa-api\changes_<current>_to_<target>.md
```

Notes:
- If the current version has no zip (e.g. a beta), export the current surface is impossible — instead diff against the previous **surface snapshot** kept at `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json`.
- The JSON change report is the authoritative worklist; the CEM descriptions usually suffice. For components whose semantics are unclear, consult the extracted sources under `temp\wa-src\<target>\dist\components\<name>\*.d.ts` or fetch the component doc from the GitHub tag.

Produce the plan document `docs\prompts\WA-<major.minor>\upgrade-v<current>-to-v<target>-plan.md` following the structure of `upgrade-v3-beta-4-to-beta-6-plan.md`: phased change list (breaking → new components → enhancements), per-file actions, validation checklist, risks. If `--dry-run`, stop here and report.

## Phase 3 — Arm the parity harness and bump the version

1. Copy `temp\wa-api\surface_<target>.json` over `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json`.
2. In `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json`: set `targetWaVersion` to the target and `"enabled": true`.
3. Bump `src\Version.props` (`Version`, and `AssemblyVersion`/`FileVersion`/`InformationalVersion` when the major/minor/patch changes) and the version references in `README.md`.
4. Run `dotnet test` — the failing **ApiSurfaceParityTests** now enumerate every gap. This is the red/green driver: the upgrade is code-complete when they pass.

## Phase 4 — Implement

Follow the plan document, in this order, checking in per phase (Phase 5 rules):

1. **Breaking changes** (from `breakingChanges` in the report): remove/rename parameters, enums, `ToHtmlValue()` cases, event args. Update affected tests in the same step.
2. **New components**: delegate to the **wa-wrapper-engineer** agent (`.claude\agents\wa-wrapper-engineer.md`), in groups of at most 10 components per agent, giving each agent the relevant `addedComponents` excerpt of the change report. Run agents for independent groups in parallel.
3. **Modified components** (additive changes): new attributes/events/slots/methods on existing wrappers — also via wa-wrapper-engineer with the `modifiedComponents` excerpts.
4. **Intentional deviations**: where the Blazor wrapper deliberately deviates (e.g. an attribute covered by `InputBase.Value`, an event folded into one callback, a native attribute passed through via `AdditionalAttributes`), record it in `parity-config.json` under the component with an entry in `ignoreReasons` — never silence a gap without a reason.
5. Iterate `dotnet build` + `dotnet test` until parity tests and the whole suite are green.

## Phase 5 — Tests and docs

1. Delegate to the **wa-test-engineer** agent (`.claude\agents\wa-test-engineer.md`): integration tests for each new component (pattern: existing `Wa*IntegrationTests.cs`), breaking-change validation tests for this version, updates to affected existing tests.
2. If there are breaking changes, write `docs\MIGRATION-<target-version>.md` following `docs\MIGRATION-3.0.0-beta.6.md` (breaking changes, new features, checklist, find/replace patterns).
3. Full suite green: `dotnet build` Debug **and** Release, `dotnet test`.

## Phase 6 — Check in and deliver

1. Via `infra-ops:plastic-ops`, check in on the task branch. Use one check-in per completed phase where practical, comments in the established style, e.g.:
   - `Version bumped to <target>`
   - `Upgrade to Web Awesome <target> Phase 1 - Breaking changes (<short list>)`
   - `Upgrade to Web Awesome <target> Phase 2 - New features (<short list>)`
   - `Additional tests and migration guide`
2. Via `infra-ops:jira-ops`: add a comment to the WAB task summarizing changes (counts from the change report, notable deviations, test totals) and transition it to **In Review**.
3. Only with `--publish`: via `infra-ops:plastic-ops`, merge the task branch to the subtrunk `/main/WA-<major.minor>`, then transition the JIRA task to **Done**. (Promotion of the subtrunk to `/main` and tagging `wa-blazor-<version>` for the GitHub release pipeline remains a separate, deliberate release step — do not do it here.)

## Failure handling

- Any phase failing its verification (build, tests) → fix within the phase; do not proceed with a red suite.
- If a change cannot be mapped mechanically (ambiguous semantics, conflicting docs), prefer the CEM + `.d.ts` sources over the markdown docs; note the decision in the plan document.
- If truly blocked, check in completed work-in-progress phases, comment the blocker on the JIRA task, and report to the user.
