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
