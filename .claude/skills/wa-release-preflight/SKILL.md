---
name: wa-release-preflight
description: Go/no-go release preflight for WebAwesome.Blazor — runs the automated gate script (version alignment, changelog/migration shape, builds, tests, nupkg dependency floors, browser e2e), verifies VCS and ticketing state, and reports blockers plus the owner-side checklist. Invoke as /wa-release-preflight before merging a task branch for release or promoting the subtrunk to /main. Add --skip-e2e to omit the browser sweep, --docs-only for a quick docs/version check without builds.
argument-hint: [--skip-e2e] [--skip-pro-e2e] [--docs-only]
---

# Web Awesome Blazor Release Preflight

Determine whether the workspace is ready to (a) merge the current task branch to its train subtrunk and (b) release the version in `src\Version.props` (promote to `/main`, label `wa-blazor-<version>`). **Read-only: fix nothing, change nothing, transition nothing — report.** Release model background: `README.md` (promotion model), `CONTRIBUTING.md`, `docs\UPGRADE-PROCESS.md`.

Arguments: `$ARGUMENTS` (`--skip-e2e` → pass `-SkipE2E` to the script; `--skip-pro-e2e` → pass `-SkipProE2E`; `--docs-only` → pass `-SkipBuild -SkipE2E`).

## 1. Automated gates (script)

Run from the repo root (Windows PowerShell):

```powershell
& .\tools\release\Test-WaReleasePreflight.ps1 [-SkipE2E] [-SkipBuild] [-ProDist <path>] [-SkipProE2E] [-E2EPort <port>]
```

Call it directly with `&`; a nested `powershell -File` leaves `$PSScriptRoot` empty. The script runs a second e2e pass (`e2e-pro`) against a self-hosted Pro dist **by default** when the local Pro build of the target version exists at `temp\wa-src\<Version.props version>` (extracted from the release zip by the upgrade pipeline). When it doesn't, it prints a `WARNING` (not a failure, repeated after the gate summary) that the Pro components went unverified in the browser; report that warning prominently next to the verdict. `-ProDist <path>` (or `-ProE2E` with `WA_PRO_DIST`) points the pass at another dist; `-SkipProE2E` turns it off (report the Pro pass as not run then). Always report the `e2e-pro` result, or why the pass didn't run.

The script checks: clean workspace; version alignment across `Version.props`, `parity-config.json` (armed + target version), `expected-api-surface.json`, README CDN snippet; no hard-coded Web Awesome CDN pins in the demo hosts (asset tags come from `WebAwesomeAssets` configuration and default to the library version); no Pro asset leakage (the generated `appsettings.Local.json`/`wwwroot\webawesome` override must not be versioned, no kit-like URLs in sources/workflows, ignore rules present in both `ignore.conf` and `.gitignore`); `docs\CHANGELOG.md` has a dated `## [<version>]` section with no leftover `[Unreleased]` content; `docs\MIGRATION-<version>.md` exists when that section declares breaking changes; Debug and Release builds with zero warnings; full test suite green on all target frameworks with nothing skipped; every dependency in the packed nuspec floored at a base major (`x.0.0`, per `docs\technical.md`); Playwright e2e suite against a demo the script starts on a free port (`e2e-free-cdn`: `CI=1`, `--forbid-only`, every skip must be listed with a reason in `tools\e2e\data\expected-skips.json`, at least `minimumTests` tests; `e2e-pro` likewise, with no skips, against the local Pro build or `-ProDist`). Non-zero exit = blockers; the script prints them as `BLOCKER:` lines.

## 2. Version control state

Using the version-control tooling available in this session (the repository is a PlasticSCM workspace — see `CLAUDE.md` and the repo docs for the branching rules), verify and report:

1. The current branch and changeset. For a merge-readiness verdict, the work must sit checked-in on a task branch `/main/WA-<x.y>/WAB-<n>`; for a release verdict, the subtrunk `/main/WA-<x.y>` head must already contain it.
2. Whether the current task branch has changesets not yet merged to the subtrunk (these are what the merge will carry).
3. That `/main` does not already contain the version being released (the release gate works the other way around — `/main` must be *behind* the subtrunk, not equal to it).

## 3. Ticketing state

Using the ticketing tooling available in this session (JIRA project **WAB**), verify and report — read-only:

1. The task ticket for this version (`WA bindings for <version>`) exists and its status is consistent with the branch state (not Done while unmerged work sits on its branch).
2. No other non-Done WAB tickets contradict the release (e.g. an In Progress task for this same train that represents unfinished scope). Stale tickets from aborted runs are reported as hygiene notes, not blockers.

If no ticketing tooling is available in the session, mark this section "not checked - verify manually" rather than guessing.

## 4. Owner-side checklist (report, cannot be automated here)

List these with their last known state, clearly marked as owner actions:

- GitHub mirror synced up to the changesets being released (branches **and** the `wa-blazor-<version>` tag will need to arrive there; GitSync maps `/main` → `master`, `/main/WA-x.y` → `master-WA-x.y`).
- nuget.org: account 2FA; Trusted Publishing policy targeting repository `boldbrick/WebAwesome-Blazor`, workflow `release.yml`.
- GitHub repo variables: `NUGET_USER` set; `NUGET_PUBLISH_ENABLED` = `true` for go-live (unset/false means the tag builds and packs but does not publish).
- GitHub Pages: source = GitHub Actions (demo deploy).

## 5. Verdict

End with a single unambiguous verdict block:

- **MERGE: GO / NO-GO** — task branch → subtrunk, with the blocker list if NO-GO.
- **RELEASE: GO / NO-GO** — subtrunk → `/main` + label, with the blocker list if NO-GO; owner-side items that cannot be verified from here are listed as "unverified prerequisites", not blockers.

Do not perform the merge, promotion, or labeling — those are deliberate maintainer actions outside this skill.
