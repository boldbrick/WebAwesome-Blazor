---
name: wa-upgrade
description: Runs the automated Web Awesome version upgrade pipeline for the Blazor bindings — from JIRA ticket and Plastic branch creation, through CEM-driven analysis and wrapper implementation, to green tests, migration docs, and check-in. Invoke as /wa-upgrade <target-version> (e.g. /wa-upgrade 3.1.0), or /wa-upgrade next to auto-pick the next downloaded release. Add --publish to also merge to the subtrunk after completion, or --dry-run to stop after analysis with just the plan document.
context: fork
agent: wa-orchestrator
argument-hint: [target-version|next] [--dry-run] [--publish]
---

# Web Awesome Bindings Upgrade Pipeline

Execute a **no-touch upgrade** of the WebAwesome.Blazor bindings to a new Web Awesome release, per these instructions and your orchestrator rules. Work autonomously; only stop for genuinely unresolvable conflicts. Full background: `docs\UPGRADE-PROCESS.md`. Wrapper authoring contract: `docs\technical.md` + `CLAUDE.md`.

## Arguments

Invocation arguments: `$ARGUMENTS`

- `<target-version>` — e.g. `3.1.0`. `next` (or no argument) means: the **lowest** version in `temp\download\webawesome_*.zip` that is greater than the current version in `src\Version.props`.
- `--publish` — after everything is green and checked in, also merge the task branch to the subtrunk.
- `--dry-run` — stop at the end of Phase 2 (analysis); produce and check in the plan document only.

**Version ordering:** compare versions by SemVer 2.0 precedence — numeric major.minor.patch (so `3.2.0 < 3.10.0`), and a pre-release suffix precedes its release (`3.0.0-beta.6 < 3.0.0`). Never compare version strings lexically.

**Gradual-upgrade rule (hard):** never skip a downloaded release. If the requested target is not the immediate next downloaded version above the current one, refuse and state which version must be done first.

**Train rule:** `<major.minor>` in branch, epic, and folder names is taken from the **target** version, matching the existing casing convention (`/main/WA-3.0`). If the subtrunk `/main/WA-<major.minor>` does not exist yet:

1. **Release gate (hard):** a new train may only start once the previous train has been **released** — the head of the previous train's subtrunk merged (promoted) into `/main`, so that the version in `src\Version.props` at the head of `/main` equals the previous train's released version. Verify this via `infra-ops:plastic-ops`. If the previous subtrunk carries unreleased, unmerged version work, **refuse to create the new train** and report what is pending — promotion of a subtrunk to `/main` is a deliberate owner release step, never done by this pipeline. Sole exception: pending **patch** work (`x.y.z`, `z > 0`, on top of a version already released to `/main`) on the previous subtrunk does not block a new train; note it in the report.
2. Set up the train in this exact order, so every changeset lands on the branch it belongs to:
   1. Create the new subtrunk **off `/main`** — never off the previous train's subtrunk — and switch to it.
   2. Create `tasks\WA-<major.minor>\` and check it in **on the subtrunk** as its first changeset (Plastic versions directories as first-class items, so the empty epic folder commits fine).
   3. Only then create the task branch (Phase 1) — it inherits the epic folder from the subtrunk.
   4. The task folder `tasks\WA-<major.minor>\WAB-<n>\` and the plan document are checked in **on the task branch** (Phase 2).

   Also create the matching WAB epic (`Web Awesome <major.minor>`).

**Patch release rule:** a patch release is developed on its train's existing subtrunk (e.g. `3.0.1` on `/main/WA-3.0`) and released by labeling `wa-blazor-<x.y.z>` **on that subtrunk** — `/main` is not involved and receives only `<major.minor>.0` promotions. A fix that matters to newer trains is merged from the older subtrunk **directly into** the newer one (e.g. `/main/WA-3.0` → `/main/WA-3.1`), skipping `/main`.

## Phase 0 — Preflight

1. Read current version from `src\Version.props` (`<Version>` property). Determine target version per the rules above; verify `temp\download\webawesome_<target>.zip` exists.
2. Via the `infra-ops:plastic-ops` skill (plastic-operator agent): confirm the workspace is **clean** and get the current branch. The upgrade must start from the subtrunk `/main/WA-<major.minor>` (per the train rule) — with one exception: if already on **this upgrade's own task branch** (from an earlier interrupted or dry run) with no pending changes, stay there and skip branch creation in Phase 1. If on any other task branch with no pending changes, switch to the subtrunk. If the workspace is dirty, stop and report.
3. Baseline: `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` and `dotnet test src/WebAwesome.slnx` must be green before any change. If not, stop and report.

## Phase 1 — Ticket and branch

This phase is **idempotent** — a restarted or post-dry-run pipeline reuses what exists instead of duplicating it.

1. Via the `infra-ops:jira-ops` skill (jira-operator agent):
   - Find the epic for the target WA train in project **WAB** (e.g. WAB-1 "Web Awesome 3.0" for the 3.0 train); create one per the train rule if missing.
   - Search WAB for an existing non-Done Task with summary `WA bindings for <target-version>`; if found, **reuse it** (ensure it is In Progress). Otherwise create a Task under the epic: summary `WA bindings for <target-version>`, description containing a `Source tag:` line linking `https://github.com/shoelace-style/webawesome/tree/v<target-version>` and a short scope note. Transition it to **In Progress**.
2. Via `infra-ops:plastic-ops`: if the task branch `/main/WA-<major.minor>/WAB-<n>` (n = the JIRA issue number from step 1) already exists, switch to it; otherwise create it off the subtrunk and switch to it. For a brand-new train, the subtrunk must already carry the `tasks\WA-<major.minor>\` changeset (train rule step 2) **before** the task branch is created — never branch off an empty, just-created subtrunk head.

## Phase 2 — Ingest and analyze

Run the tooling (Windows PowerShell, from repo root):

```powershell
tools\upgrade\Expand-WaRelease.ps1 -Version <target>          # optional full source tree for reference
tools\upgrade\Export-WaApiSurface.ps1 -Version <current>      # only when temp\download\webawesome_<current>.zip exists — otherwise see baseline note below
tools\upgrade\Export-WaApiSurface.ps1 -Version <target>
tools\upgrade\Compare-WaApiSurface.ps1 `
    -FromPath temp\wa-api\surface_<current>.json `
    -ToPath temp\wa-api\surface_<target>.json `
    -MarkdownPath temp\wa-api\changes_<current>_to_<target>.md
```

Notes:
- **Baseline note:** if the current version has no zip (e.g. a beta), use the surface snapshot `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` as `-FromPath` — but **only after confirming its `version` field equals the current version** in `Version.props`. If it does not match, no valid diff baseline exists: skip the compare step, state so in the plan document, and rely on the armed parity tests (Phase 3) as the sole worklist.
- The JSON change report is the authoritative worklist; the CEM descriptions usually suffice. For components whose semantics are unclear, consult the extracted sources under `temp\wa-src\<target>\dist\components\<name>\*.d.ts` (never copy their contents anywhere outside `temp\`) or the documentation refreshed below.

**Refresh `inputs\WebAwesome`** (versioned reference documentation, see `inputs\README.md`):
1. Replace the contents of `inputs\WebAwesome` with the `packages/webawesome/docs/docs` folder of the **public GitHub repo at the target tag** (`https://github.com/shoelace-style/webawesome/tree/v<target>/packages/webawesome/docs/docs`). Fetch via the GitHub API/raw URLs; do not clone into the repo.
2. The public repo does **not** document Pro components (as of 3.1.0: `combobox`, `page`). For every component present in the target CEM but missing from the fetched docs, fetch its doc page from `https://webawesome.com/docs/components/<name>` (public web docs cover Pro components) and save it as `inputs\WebAwesome\components\<name>.md`, noting the source URL at the top.
3. Check in the documentation refresh as its own changeset (comment: `Web Awesome <target> documentation added`) — this keeps the doc diff reproducible per `inputs\README.md`.

**Pro-source rule (hard):** everything extracted from the Pro release zips stays under `temp\` (ignored by Plastic and git). The only CEM-derived artifact ever checked in is the API surface JSON (names, types, defaults, doc descriptions — no implementation code); plan documents and migration guides may describe APIs but must never embed upstream JS/TS/CSS source.

Produce the plan document `tasks\WA-<major.minor>\WAB-<n>\upgrade-v<current>-to-v<target>-plan.md` (n = this upgrade's JIRA task from Phase 1; create the task folder if missing) and check it in on the task branch together with its folder, following the structure of `tasks\WA-3.0\WAB-4\upgrade-v3-beta-4-to-beta-6-plan.md`: phased change list (breaking → new components → enhancements), per-file actions, validation checklist, risks.

If `--dry-run`, stop here — but leave a clean, resumable state: check in the plan document on the task branch (comment: `Upgrade plan for Web Awesome <target>`), add a JIRA comment linking the plan, leave the task In Progress, and report that ticket `WAB-<n>` and branch `/main/WA-<major.minor>/WAB-<n>` were created and will be reused by the real run.

## Phase 3 — Arm the parity harness and bump the version

1. Copy `temp\wa-api\surface_<target>.json` over `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json`.
2. In `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json`: set `targetWaVersion` to the target and `"enabled": true`.
3. Bump `src\Version.props` (`Version`, and `AssemblyVersion`/`FileVersion` when the major/minor/patch changes; `InformationalVersion` derives from `Version` automatically) and the version references in `README.md`.
4. Demo app sync: update the Web Awesome CDN version in `src\WebAwesome.Blazor.Demo\wwwroot\index.html` to `<target>`, and copy the target surface over `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` (it drives the demo navigation and API tables).
5. Run `dotnet test` — the failing **ApiSurfaceParityTests** now enumerate every gap. This is the red/green driver: the upgrade is code-complete when they pass.

## Phase 4 — Implement

Follow the plan document, in this order, checking in per phase (check-in rules and comment style: Phase 6):

1. **Breaking changes** (from `breakingChanges` in the report): remove/rename parameters, enums, `ToHtmlValue()` cases, event args. For removed components, delete the wrapper class, its now-unused enums/event args, and its test file. Update affected tests in the same step.
2. **New components**: delegate to the **wa-wrapper-engineer** agent (`.claude\agents\wa-wrapper-engineer.md`), in groups of at most 10 components per agent, giving each agent the relevant `addedComponents` excerpt of the change report. Run agents for independent groups in parallel.
3. **Modified components** (additive changes): new attributes/events/slots/methods on existing wrappers — also via wa-wrapper-engineer with the `modifiedComponents` excerpts.
4. **Intentional deviations**: where the Blazor wrapper deliberately deviates (e.g. an attribute covered by `InputBase.Value`, an event folded into one callback, a native attribute passed through via `AdditionalAttributes`), record it in `parity-config.json`: add the member to the component's `ignoredAttributes`/`ignoredEvents`/`ignoredMethods` list (or an `attributeOverrides`/`eventOverrides` mapping for a deliberate rename), **and** add a matching entry with the rationale to the top-level `ignoreReasons` map — never silence a gap without a reason.
5. **Event delivery contract**: every newly bound `wa-*` event uses the `onwa-` attribute prefix and is registered in `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js` (with a `specialArgs` payload mapping when the detail carries DOM nodes or the typed args need derived values); event args classes inherit `System.EventArgs`. `EventBindingRegistrationTests` fails on violations.
6. **Element method audit**: every JS element method a wrapper invokes must exist in the target CEM or be allowlisted in `parity-config.json` (`extraElementMethods`) with a reason; **re-verify every existing `extraElementMethods` entry against the target version's source** (these are exactly the methods the CEM diff cannot see — the observers' `stopObserver`/`startObserver` rename shipped through this hole). `ElementMethodInvocationTests` enforces the allowlist.
7. Iterate `dotnet build` + `dotnet test` until parity tests and the whole suite are green.

## Phase 5 — Tests and docs

1. Delegate to the **wa-test-engineer** agent (`.claude\agents\wa-test-engineer.md`): integration tests for each new component (pattern: existing `Wa*IntegrationTests.cs`), breaking-change validation tests for this version, updates to affected existing tests. **Coverage rule for new form controls:** every new wrapper deriving from `WaInputBase<T>`/`InputBase<T>` additionally gets bUnit EditForm integration coverage (pattern: the form-control tests under `src\WebAwesome.Blazor.Tests\Base\` — binding, change propagation, validation lifecycle, custom validity).
2. If there are breaking changes, write `docs\MIGRATION-<target-version>.md` following `docs\MIGRATION-3.0.0-beta.6.md` (breaking changes, new features, checklist, find/replace patterns).
3. Draft the `docs\CHANGELOG.md` entry for `<target>` from the change report (`temp\wa-api\changes_*.json`/`.md`): a `## [<target>] — <date>` section with `### Breaking changes` (verbatim from `breakingChanges`), `### New components`, `### Changed`, `### Library` subsections per the existing entries' style, plus a link to the migration doc when one exists. Fold any accumulated `## [Unreleased]` items into the new section.
4. Demo pages: run `tools\demo\New-WaDemoPages.ps1 -PruneRemoved` — new components get skeleton demo pages (TODO-marked; curating them is deliberate follow-up work, not part of the upgrade), removed components' pages are deleted. The demo project must build. Additionally review the curated showcase pages (`src\WebAwesome.Blazor.Demo\Pages\Showcases\`, when present): components **removed** by the upgrade must be removed from showcases (they would break the build); components **added** are noted in the plan document as showcase-curation follow-up work, and any new *form control* should be added to the form showcase in the same run when the addition is mechanical.
5. **Public API snapshot**: the intentional API changes of this upgrade will fail `PublicApiSnapshotTests` — diff the emitted `received-public-api.txt` against the baseline, confirm every difference is explained by the change report, promote it to `src\WebAwesome.Blazor.Tests\PublicApi\approved-public-api.txt`, and mention notable surface changes in the CHANGELOG entry. Never promote a diff containing changes the report does not explain.
6. Full suite green: `dotnet build` Debug **and** Release (includes the demo app), `dotnet test`.
7. **Browser verification**: start the demo (`dotnet run --project src\WebAwesome.Blazor.Demo --configuration Debug --no-build`) and run the Playwright suite from `tools\e2e` (`npm test`; first run may need `npm install` + `npm run install-browsers`). The sweep must be green — it catches the runtime/DOM classes of bug (JS interop failures, event delivery, real rendering) that neither the build nor bUnit can see. Stop the demo afterwards.

## Phase 6 — Check in and deliver

1. Via `infra-ops:plastic-ops`, check in on the task branch. Use one check-in per completed phase where practical.

   **Check-in comment convention (applies to every check-in this pipeline makes):**
   - Never start with ticket keys (`WAB-<n> ...`) — the branch name already carries the ticket; the key is superfluous in the comment.
   - Format: 1–2 sentences summarizing what and why, followed by bullet-pointed details where the change has more than one facet. Example:

     ```
     Upgrade to Web Awesome <target>: new WaCombobox wrapper and the two upstream attribute removals.
     - WaCombobox (Pro, modeled on WaSelect) with integration + EditForm tests and demo page
     - BREAKING: WaButton.Form and WaButtonGroup.Variant removed per upstream
     - parity deviations recorded; public API baseline promoted
     - 312 tests green on net9.0+net10.0
     ```

   - Single-facet changesets keep a short one-liner (`Version bumped to <target>`).
2. Via `infra-ops:jira-ops`: add a comment to the WAB task summarizing changes (counts from the change report, notable deviations, test totals) and transition it to **In Review**.
3. Only with `--publish`: via `infra-ops:plastic-ops`, merge the task branch to the subtrunk `/main/WA-<major.minor>`, then transition the JIRA task to **Done**. If the merge conflicts, do not force it: leave the task branch checked in and In Review, comment the conflict on the JIRA task, and report. (Promotion of the subtrunk to `/main` and tagging `wa-blazor-<version>` for the GitHub release pipeline remains a separate, deliberate release step — do not do it here.)

## Failure handling

- Any phase failing its verification (build, tests) → fix within the phase; do not proceed with a red suite.
- If a change cannot be mapped mechanically (ambiguous semantics, conflicting docs), prefer the CEM + `.d.ts` sources over the markdown docs; note the decision in the plan document.
- If truly blocked, check in completed work-in-progress phases, comment the blocker on the JIRA task, and report to the user.
