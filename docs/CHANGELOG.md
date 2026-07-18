# Changelog

All notable changes to the Web Awesome Blazor Bindings. Versions mirror the bound [Web Awesome](https://github.com/shoelace-style/webawesome) release; the format follows [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Library
- Multi-targeting: the package now ships `net9.0` and `net10.0` assemblies; .NET 10 (LTS) is the primary supported target.
- NuGet packaging: full package metadata, icon, package readme, symbol packages (snupkg), per-framework XML documentation for IntelliSense.
- Automated upgrade pipeline: CEM-driven API diff tooling (`tools\upgrade\`), API-surface parity tests, and the `/wa-upgrade` orchestration skill (see `docs\UPGRADE-PROCESS.md`).
- Repository hygiene: corrected README setup snippet (official `@awesome.me/webawesome` CDN), verified 3.0.0-beta.6 documentation inputs, corrected API parity baseline.

## [3.0.0-beta.6] — 2026-07-18

### Breaking changes
- `WaIcon`: removed `FixedWidth` (fixed width is the Font Awesome 7 default); added `AutoWidth` and `SwapOpacity`.
- `WaDetails`: renamed `IconPosition` to `IconPlacement`; enum `WaIconPosition` replaced by `WaIconPlacement`.
- `WaButtonGroup`: removed `Size`.

### New components
- `WaIntersectionObserver` with typed `WaIntersectionEventArgs`.

### Changed
- `WaCard`: added `Orientation` and header/footer action slots.
- `WaDropdownItem`: added variant support.
- Implemented element methods requiring JS interop across components (show/hide, focus/blur, `setCustomValidity`), backed by a single interop module.

### Library
- Integration tests and breaking-change validation tests added.

See [MIGRATION-3.0.0-beta.6.md](MIGRATION-3.0.0-beta.6.md) for the full migration guide.

## [3.0.0-beta.4] — 2025-09-27

Initial release: 57 component wrappers and 8 CSS layout components (`WaCluster`, `WaFlank`, `WaFrame`, `WaGrid`, `WaSplit`, `WaStack`, `WaText`, `WaVisuallyHidden`), strongly-typed enum parameters, `InputBase<TValue>`-based form controls with EditForm integration.
