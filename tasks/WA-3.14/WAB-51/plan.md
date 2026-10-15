# WAB-51 — Version scheme: binding revision as the fourth version part (3.14.0.1)

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, section 17.3, decision D9.

## Rule

- The first three version parts are the bound Web Awesome version. The fourth part is a revision of
  the bindings. NuGet sorts 3.14.0 < 3.14.0.1 < 3.14.1.
- A bindings revision is developed and tagged (`wa-blazor-3.14.0.1`) on the train subtrunk
  (`/main/WA-3.14`). It is not promoted to `/main` alone. Before the next train starts, the subtrunk
  is merged into `/main`.
- The Web Awesome version for the CDN, the parity harness and the docs stays 3.14.0.

## Scope

1. `src\Version.props`: `Version`, `AssemblyVersion`, `FileVersion` = 3.14.0.1.
2. `WebAwesomeOptions`: the default CDN version is the Web Awesome part of the package version.
   One internal static helper (`ToWebAwesomeVersion`) does the parsing. Unit tests cover it.
3. Demo: `WaVersionInfo` shows the bound Web Awesome version through
   `new WebAwesomeOptions().ResolveVersion()`, so the demo does not parse the version a second time.
   The "one-to-one" texts in the demo, the package description and `PACKAGE.md` change.
4. Preflight (`tools\release\Test-WaReleasePreflight.ps1`): derive the Web Awesome version from
   `Version.props`. The gates `parity-version`, `surface-version`, `readme-cdn-version` and the
   default Pro dist (`temp\wa-src\<version>`) use the Web Awesome version. The changelog, migration
   and nuspec gates keep the package version.
5. Skills: `/wa-upgrade` compares the downloaded versions with the Web Awesome version of
   `Version.props` (so `next` finds 3.15.0 above 3.14.0.1), and the next train drops the fourth part.
   `/wa-release-preflight` describes the revision release (label on the subtrunk).
6. Docs: README, CONTRIBUTING, UPGRADE-PROCESS, CHANGELOG (`[Unreleased]` line).

## Decisions

- Prerelease suffix on a four-part version: the suffix belongs to the bindings revision, not to
  Web Awesome. `3.14.0.1-beta.1` is a prerelease of the revision over the released Web Awesome
  3.14.0, so the default CDN version is `3.14.0`. Reason: NuGet sorts `3.14.0.1-beta.1` above
  `3.14.0`, and `webawesome@3.14.0-beta.1` is not the release that the package binds (on jsDelivr it
  is a different build or a 404). A three-part version keeps its suffix (`3.0.0-beta.6` binds the Web
  Awesome prerelease `3.0.0-beta.6`), as before. A bindings revision over a Web Awesome prerelease
  cannot be expressed; it is not supported.
- Testability: `InternalsVisibleTo` applies to Debug builds only (`src\Directory.Build.props`), and
  the suite also runs in Release. The tests find the internal helper by name, as
  `TemporalAttributeGuardTests` does. No public API change.
- The tag guard in `release.yml` compares the tag with `Version.props`. It accepts 3.14.0.1 as is.
  No change.

## Test approach

- xUnit theory over the helper: `3.14.0.1` -> `3.14.0`, `3.14.0.1+abc` -> `3.14.0`,
  `3.14.0.1-beta.1` -> `3.14.0`, `3.14.0` -> `3.14.0`, `3.0.0-beta.6+abc` -> `3.0.0-beta.6`,
  `3.14.0.12-rc.1+sha` -> `3.14.0`.
- The existing `ResolveVersion_Default_UsesLibraryVersion` test proves the end result: the package
  is 3.14.0.1 and the default CDN version equals `targetWaVersion` 3.14.0.
- Preflight in docs-only mode (`-SkipBuild -SkipE2E`) proves the gates accept 3.14.0.1.

## Result

- Changesets on `/main/WA-3.14/WAB-51`: cs:334-335 (plan), cs:336 (code, tests, demo), cs:337 (preflight,
  skills, comments), cs:338 (README, CONTRIBUTING, UPGRADE-PROCESS, CHANGELOG).
- Code: `src\Version.props` 3.14.0.1. `WebAwesomeOptions.ToWebAwesomeVersion` (internal) gives the
  default CDN version 3.14.0. The demo `WaVersionInfo` uses `new WebAwesomeOptions().ResolveVersion()`.
  No public API change (the snapshot test is green).
- Preflight gates: new `assembly-version` gate, stricter `version-props` (version shape). The gates
  `parity-version`, `surface-version`, `readme-cdn-version` and the default Pro dist use the Web
  Awesome version 3.14.0. The gates `changelog-entry`, `changelog-unreleased-folded`, `migration-doc`
  and `nupkg-dependency-floors` keep the package version (the nuspec is
  `WebAwesome.Blazor.3.14.0.1.nuspec`).
- Verification: Debug and Release builds green, 0 warnings. Tests 1274 passed on net9.0 and 1274 on
  net10.0, in Debug and in Release. Preflight `-SkipBuild -SkipE2E`: 12 gates OK. The two changelog
  gates fail as expected until the release step folds `[Unreleased]` into `[3.14.0.1]`; a simulation
  of the folded changelog passes them. E2E not run: no gate change needs it.
- Deviation: a prerelease suffix after a fourth part is dropped (`3.14.0.1-beta.1` -> `3.14.0`), not
  kept (see Decisions).
- `/wa-upgrade` release gate: a released patch or bindings revision on the previous subtrunk must reach
  `/main` before the next train (as in the design, section 17.3, and the owner rule for sequential
  patch promotion). Pending, unreleased patch work still does not block.
- Open points: `.github\workflows\release.yml` needs no change (the tag guard compares the full
  version). `tools\upgrade\*.ps1` read no package version, so they did not change.
