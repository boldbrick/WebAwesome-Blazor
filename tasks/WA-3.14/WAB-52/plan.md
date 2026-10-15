# WAB-52 — WebAwesome.Blazor.Extensions package scaffold

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, section 17.1, decision D8.

## Rule

A feature that binds an upstream member, or a behavior that upstream documents, goes into
`WebAwesome.Blazor`. A feature that adds behavior that upstream does not have goes into
`WebAwesome.Blazor.Extensions`. The Extensions package has the same version as the main package and
depends on it with a floor of the same version. This ticket creates the empty, shippable structure.
Later tickets (WAB-55, WAB-59, WAB-60) add the features.

## Scope

1. New Razor class library `src\WebAwesome.Blazor.Extensions` (root namespace
   `WebAwesome.Blazor.Extensions`). The `Directory.Build.props` classification makes it a library:
   `net9.0;net10.0`, CS1591 as error, packable, explicit usings, nullable. NuGet metadata as in the main
   project: `PACKAGE.md` readme, icon, tags, license, snupkg, SourceLink on CI.
   A `ProjectReference` to `WebAwesome.Blazor` packs as the dependency `WebAwesome.Blazor` with the
   floor `3.14.0.1`.
2. JS initializer `wwwroot\WebAwesome.Blazor.Extensions.lib.module.js`: `afterWebStarted` and
   `afterStarted` exports, no behavior yet, a header comment that states the purpose.
3. Test project `src\WebAwesome.Blazor.Extensions.Tests` (xUnit v3, `OutputType` Exe, `UseAppHost`, as
   the main test project): public API snapshot with its own approved baseline, version alignment with
   the main package, and the JS initializer contract.
4. `src\WebAwesome.slnx`: both projects. Both demo hosts reference the Extensions project.
5. Preflight: a new gate `extensions-package` (both nupkgs at the package version, the Extensions
   nuspec depends on `WebAwesome.Blazor` with the floor = package version in every framework group,
   the Extensions nupkg contains the JS initializer as a static web asset). The gate
   `nupkg-dependency-floors` also checks the Extensions nuspec and exempts the sibling dependency.
6. Workflows: `release.yml` already packs, attaches and pushes `*.nupkg`/`*.snupkg`. Add a guard step
   that fails when one of the two packages is missing. `build.yml` builds and tests the whole
   solution. `demo.yml` publishes the demo, which references the Extensions project.
7. Parity and `/wa-upgrade`: the parity harness reads only `typeof(WaButton).Assembly` and
   `src\WebAwesome.Blazor`, so it does not see the Extensions package. State in
   `docs\UPGRADE-PROCESS.md` and the `/wa-upgrade` skill that the Extensions package does not follow
   the custom-elements manifest, and that an upgrade must still build and test it.
8. Docs: README (non-goal for the main package, a section about the Extensions package), CONTRIBUTING
   (where extras go), `docs\technical.md` (the split rule). No CHANGELOG line (no features yet).

## Decisions

- The Extensions project has no direct `PackageReference`. It gets the framework packages through
  `WebAwesome.Blazor`. Its nuspec then has one dependency per framework group. Later tickets add a
  direct reference only when they need one.
- The namespace `WebAwesome.Blazor.Extensions` already exists in the main package
  (`ServiceCollectionExtensions.AddWebAwesome`). The ticket names this root namespace, so it stays.
  Later tickets must not add a type with a name that the main package uses in this namespace.
- Public API snapshot: the assembly has no public type yet, so the approved baseline is empty. The
  test loads the assembly by name. The baseline grows with WAB-55.
- The JS initializer has no behavior and no global marker. A browser test proves that the demo loads
  it (network response 200), so the static web asset chain is proven end to end.

## Test approach

- xUnit: snapshot (empty baseline), same `AssemblyInformationalVersion` and `AssemblyVersion` as
  `WebAwesome.Blazor`, initializer exports `afterWebStarted` and `afterStarted` as functions.
- Pack: inspect the produced `.nuspec` and `.nupkg` (dependency floor, `staticwebassets` entry).
- Preflight `-SkipE2E`: the new gate is green.
- E2E: a spec checks that the demo loads the Extensions initializer.
