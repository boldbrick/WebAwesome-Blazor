# Contributing Guide

Thanks for helping improve **Web Awesome Blazor Bindings**! This document explains how to propose changes and which branches to target.

## TL;DR
- **Target PRs to the active subtrunk**: `main/wa-<ver>` (currently `main/wa-3.0`).
- After a stable branch (e.g., `release-3.0`) exists, target **bugfixes** to that branch.
- New/breaking work goes to the next subtrunk (e.g., `main/wa-3.1` or `main/wa-4.0`).
- Releases are cut by promoting the subtrunk to `main` and tagging `wa-blazor-<version>`.

## Repository topology
- `main` — staging for tags; no direct feature work.
- `main/wa-<ver>` — **subtrunk** for a given WA train (e.g., `main/wa-3.0`).
- `main/wa-<ver>/wab-<task>` — short-lived task branches used by maintainers; for our off-GitHub development hosted in PlasticSCM a.k.a. Unity VCS.
- `release-<major.minor>` — maintenance for stable (e.g., `release-3.0`).

**Monotonic promotion rule**: once `main` has merged from `wa-N+1`, it no longer accepts merges from `wa-N`.

## What to contribute
- New component wrappers (one per WA component)
- Bug fixes or parity updates for existing wrappers
- API docs, samples, unit tests

## Before you start
1. **Pick the right branch**
   - Active WA train (e.g., 3.0): open PR to `main/wa-3.0`.
   - Stable maintenance (e.g., 3.0.x): open PR to `release-3.0`.
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
- We tag releases as `wa-blazor-<version>`, e.g., `wa-blazor-3.0.0-beta.4`.
- NuGet packages are produced from tags in CI.

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
- `src/Version.props`: properties describing the current version; only changed in subtrunk and release branches.
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
