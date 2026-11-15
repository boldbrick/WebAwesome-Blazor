# Contributing Guide

Thanks for helping improve **Web Awesome Blazor Bindings**! This document explains how to propose changes and which branches to target.

## TL;DR
- **Target PRs to the active subtrunk**: on GitHub `master-WA-<ver>` (currently `master-WA-3.0`).
- **Bugfixes for a released train** also target that train's subtrunk (e.g., 3.0.x fixes → `master-WA-3.0`) — there are no separate release branches.
- New/breaking work goes to the next subtrunk (e.g., `master-WA-3.1` or `master-WA-4.0`).
- **Web Awesome version upgrades are maintainer work** (automated pipeline, see [docs/contributing/UPGRADE-PROCESS.md](docs/contributing/UPGRADE-PROCESS.md)) — please don't send PRs implementing a new WA release.
- `<major.minor>.0` releases are cut by promoting the subtrunk to the main line and tagging `wa-blazor-<version>`; **patch releases** (`x.y.z`) and **bindings revisions** (`x.y.z.r`) are tagged directly on the train's subtrunk.

## Repository topology

Development is hosted in PlasticSCM (a.k.a. Unity VCS) on hierarchical branches; this GitHub repository is a mirror produced by GitSync, whose fixed naming maps the Plastic `/main` branch to git `master` and flattens hierarchy with dashes (`/main/WA-3.0` → `master-WA-3.0`).

- `master` (Plastic `/main`) — staging for `<major.minor>.0` release tags; no direct feature work.
- `master-WA-<ver>` (Plastic `/main/WA-<ver>`) — **subtrunk** for a given WA train (e.g., `master-WA-3.0`); also carries that train's patch maintenance and patch release tags after the train has shipped.
- `master-WA-<ver>-WAB-<task>` (Plastic `/main/WA-<ver>/WAB-<task>`) — short-lived task branches used by maintainers.

**Monotonic promotion rule**: once the main line has merged from train `N+1`, it no longer accepts merges from train `N`.

**New-train rule**: a new subtrunk (`WA-3.<x+1>`, `WA-4.0`) is branched **from the main line**, never from the previous subtrunk, and only after the previous train's release has been promoted to the main line. A released patch or bindings revision on the previous subtrunk is merged into the main line first, so the new train carries it. Pending, unreleased patch work (e.g., `3.0.1` on `master-WA-3.0`) is the one allowed exception.

**Patch releases and bindings revisions**: developed and tagged (`wa-blazor-x.y.z`, `wa-blazor-x.y.z.r`) on the train's subtrunk — they are never promoted to the main line by themselves. When a fix matters to newer trains, it is merged from the older subtrunk directly into the newer one (e.g., `master-WA-3.0` → `master-WA-3.1`).

## What to contribute
- Bug fixes or parity corrections for existing wrappers
- Supplemental improvements: demo app, API docs, samples, tests
- **Not**: wrapper work for a new Web Awesome release (new components, version upgrades) — that arrives via the maintainers' automated upgrade pipeline; file an issue instead if a release seems overdue

### Where extras go
The repository ships two packages with one version (see [docs/technical/technical.md](docs/technical/technical.md), "Package split"):
- `src/WebAwesome.Blazor` — the bindings. A change that binds an upstream member, or a behavior that the Web Awesome documentation describes, goes here.
- `src/WebAwesome.Blazor.Extensions` — optional extras. A feature that adds behavior that upstream Web Awesome does not have goes here, with its tests in `src/WebAwesome.Blazor.Extensions.Tests`. It extends the bindings through their public API only, never through a subclass of a wrapper or a new wrapper parameter.
- If you are not sure where a change goes, open an issue first.

## Before you start
1. **Pick the right branch**
   - Active WA train (e.g., 3.0): open PR to `master-WA-3.0`.
   - Patch fix for a shipped train (e.g., 3.0.x): also `master-WA-3.0`.
2. **Open an issue** if your change is non-trivial or potentially breaking.

## Branch & commit conventions
- Branch name (contributors): `feat/<area>-<short-desc>` or `fix/<area>-<short-desc>`
- Commit message style (conventional-ish):
  - `feat(wa-button): support loading state`
  - `fix(wa-tabs): correct SelectedIndex change event`
  - `docs(readme): add version alignment table`

## Coding guidelines
- Primarily see [CLAUDE.md](CLAUDE.md) for detailed rules
- **C#**: .NET 9, nullable enabled, idiomatic async, `IDisposable`/`IAsyncDisposable` where applicable.
- **Wrappers**: keep parameter names close to WA, map casing where needed (UpperCamelCase C# → lower/attribute form expected by WA), and provide XML docs.
- **Events**: expose Blazor event callbacks (`EventCallback`, `EventCallback<T>`). Use minimal JS interop only when WA cannot raise a standard DOM event that Blazor can consume.
- **Interop**: isolate JS in a small, unit-testable module; avoid global mutations.
- **Tests**: add simple Bunit tests where practical (at least for parameters and events).
- **No razor**: We use manually constructed render trees to have attribute and event code under control. Keep this style, for the sake of consistency.

## Adding a new wrapper (checklist)
1. Create component `Wa<Control>.cs`.
2. Map WA attributes to `[Parameter]`s; handle value conversion.
3. Wire events → `EventCallback`s; add `OnInitialized`/`OnAfterRenderAsync` interop hooks if needed.
4. Add XML docs and a minimal usage snippet in the PR description.
5. Update README coverage/notes if the component is notable.

## PR process
1. Fork and branch off the **target subtrunk**.
2. Push and open a PR. CI will build and run analyzers/tests.
3. A maintainer reviews; please address feedback via follow-up commits.
4. We squash or merge-commit (maintainer’s choice per repo policy) into the target branch.
5. Maintainers periodically **promote** the subtrunk to `main` and tag a release.

## Versioning & tags
- The first three version parts are the bound Web Awesome version (with its prerelease suffix, if any). An optional fourth part is a revision of the bindings: changes to the bindings only, for the same Web Awesome release (`3.14.0.1` binds Web Awesome `3.14.0`). A new Web Awesome release resets it (`3.15.0`).
- We tag releases as `wa-blazor-<version>`, e.g., `wa-blazor-3.0.0-beta.4`, `wa-blazor-3.14.0.1`.
- `<major.minor>.0` releases are tagged on the main line (after promotion); patch releases and bindings revisions are tagged on the train's subtrunk.
- NuGet packages are produced from tags in CI. These packages (nuget.org) carry no docs bundle.

## Maintainer setup and local release
Maintainers also publish each release to the internal NuGet feed. Only these packages, from a local Release build, carry the docs bundle.

One-time setup:
1. Add the internal NuGet feed as a user-level NuGet source. Never add it to the repository, and never add a `nuget.config` to the repository:
   ```powershell
   dotnet nuget add source <feed> --name <name>
   ```
2. Restore the local tools (`docpack` and `docview`, pinned in `.config/dotnet-tools.json`):
   ```powershell
   dotnet tool restore
   ```

The docs bundle:
- A local Release build bundles the docs into both packages under `docs/` (DocPack): the API reference and a skill.
- The main package also contains `docs/content/`: the changelog, the technical documentation, the migration guides and the Web Awesome docs (`web-awesome/`, see `THIRD-PARTY-NOTICES.md`). `docs/contributing` and `inputs/WebAwesome/ai` are not bundled.
- GitHub builds (nuget.org packages) and Debug builds have no docs bundle.

Release step: after the release check-in and label, and before the tag push to the GitHub mirror, publish both packages:
```powershell
& .\tools\release\Publish-WaLocalFeed.ps1 -Source <name>
```
The script checks that the workspace is clean, runs `dotnet tool restore` and a Release build, checks the docs bundle of both packages, and pushes them with `--skip-duplicate`.

Internal consumers:
- Map `WebAwesome.*` to the internal feed with `packageSourceMapping` in your own `nuget.config`. Then NuGet takes these packages from the internal feed only:
  ```xml
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <!-- key: the name of your source for the internal feed -->
    <packageSource key="internal-feed">
      <package pattern="WebAwesome.*" />
    </packageSource>
  </packageSourceMapping>
  ```
- NuGet keeps one copy of each package version in the global packages folder. If it has a nuget.org copy of the same version, clear it first: delete `%USERPROFILE%\.nuget\packages\webawesome.blazor\<version>` (and `webawesome.blazor.extensions\<version>`), or run `dotnet nuget locals global-packages --clear`.

## Development environment
- .NET 9 SDK
- Recommended: VS 2022 / Rider / VS Code + C# Dev Kit

Build:
```bash
 dotnet restore
 dotnet build -c Release
 dotnet pack -c Release
```

Build system:
- `src/Directory.Build.props`: establishes the single-output-directory build system.
- `src/Version.props`: properties describing the current version; only changed in subtrunks.
- `src/output`: single-output-directory for all build artifacts, namely `obj`, `bin`, `packages` (both Debug and Release nugets are produced here).

## Filing issues
Please include:
- WA version and bindings version
- Repro steps or a minimal snippet
- Expected vs. actual behavior
- Browser/runtime (Server/WASM) details

## Security
Do not file vulnerabilities in public issues. Email the maintainer (see repo profile) with details and repro if possible.

## Code of Conduct
We follow common-sense standards of professionalism and respect. Be kind. The maintainers reserve the right to moderate discussions and contributions that violate these expectations.
