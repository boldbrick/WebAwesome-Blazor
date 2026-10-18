# Web Awesome Blazor Bindings

[![build](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/build.yml/badge.svg)](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/build.yml)
[![release](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/release.yml/badge.svg)](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/WebAwesome.Blazor.svg)](https://www.nuget.org/packages/WebAwesome.Blazor)

Blazor-first wrappers for the **[Web Awesome](https://webawesome.com) (WA)** web components ([GitHub](https://github.com/shoelace-style/webawesome)), providing idiomatic C# APIs, eventing, and attributes that play nicely with Blazor (Server & WebAssembly). Focused on seamless integration into the Blazor ecosystem. The main package adds no application logic and no extension components; optional extras ship in a separate package, `WebAwesome.Blazor.Extensions`.

> **Status**
> - Active train: **WA 3.14**
> - Current alignment: **WA 3.14.0** (tagged releases use `wa-blazor-<version>`)

**[Live demo](https://boldbrick.github.io/WebAwesome-Blazor/)** — every component, rendered from this library; published to GitHub Pages as a Blazor WebAssembly app. It can also be built and run locally in server mode: `dotnet run --project src/WebAwesome.Blazor.Demo.Server`.

## Why this library

⚖️ **MIT-licensed, commercial-friendly.** Free for closed-source and commercial applications, no copyleft obligations. Backed by [BoldBrick](https://www.boldbrick.com).

🧩 **Blazor-native architecture — no JavaScript layer to fight.** Wrappers are pure C# render trees; Web Awesome's `wa-*` custom events bind directly to `EventCallback<T>` handlers with typed event args. There are no per-component JS modules, no `document.getElementById` lookups, no library-generated element ids. The entire JS interop surface is a single ~100-line module driven by `ElementReference` — collision-proof, prerender-safe, and trivially auditable.

📝 **Real form integration, not string soup.** Form controls derive from Blazor's `InputBase<TValue>`: typed values (`bool`, `decimal`, not just strings), `@bind-Value`, full `EditForm`/`EditContext` participation, DataAnnotations validation, validation CSS classes, and `SetCustomValidityAsync` for constraint validation — the same programming model as Blazor's built-in inputs.

🔒 **Strongly typed all the way down.** String attributes with fixed value sets are nullable C# enums; an unset parameter omits the attribute entirely so Web Awesome's own defaults apply. Enum-to-attribute mapping lives in one place, not scattered per component.

✅ **Machine-verified completeness.** A reflection-based API parity suite checks every wrapper against Web Awesome's published custom-elements manifest — attributes, slots, events. "Did we miss a parameter?" is a red/green test signal, not a code-review hope. 229 automated tests plus a Playwright-based end-to-end sweep of the demo app back every release.

🚄 **Built to track Web Awesome releases fast.** A CEM-driven upgrade pipeline diffs upstream API surfaces, generates change reports, and drives wrapper updates and tests (see [docs/contributing/UPGRADE-PROCESS.md](docs/contributing/UPGRADE-PROCESS.md)). The first three parts of the package version **are** the Web Awesome version it binds; a fourth part only marks a revision of the bindings (`3.14.0.1`) — no parallel version scheme to decode.

📐 **Covers what others skip.** Web Awesome's CSS-only layout system ships as components too (`WaStack`, `WaCluster`, `WaFlank`, `WaGrid`, …), alongside utility elements like `WaFormatDate`, `WaFormatNumber`, and the observer components.

✨ **Ergonomic where it counts.** Common slot usage is one attribute (`StartIconName="gear"`), with `RenderFragment` slots (`StartContent`, `EndContent`, …) always available as the general mechanism.

📦 **Clean packaging.** Multi-targeted `net9.0`/`net10.0`, XML IntelliSense docs on every public member (missing docs fail the build), symbol packages (snupkg) with SourceLink, deterministic CI builds.

🎯 **Pure bindings, no lock-in.** By deliberate architectural decision, the main package contains no application logic, no extension components, and redistributes no Web Awesome Pro source — you bring WA assets via CDN or self-hosting, exactly as upstream documents. Extras that add behavior Web Awesome does not have are opt-in, in a [separate package](#extensions-package-optional).

## Package
- NuGet: `WebAwesome.Blazor` *(published from release tags)*
- NuGet: `WebAwesome.Blazor.Extensions` — optional extras, same version *(published from the same release tags)*
- [Changelog](docs/CHANGELOG.md) — keyed to Web Awesome versions, breaking changes called out per release

## Requirements
- .NET 10 (LTS, primary target) or .NET 9 (compatibility; in Microsoft support until November 2026)
- WA 3.x assets available to your app (via your chosen delivery method)

## Install
```bash
 dotnet add package WebAwesome.Blazor
```

## Setup

### 1. Register Services
Add Web Awesome services to your dependency injection container in `Program.cs`:

```csharp
using WebAwesome.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Web Awesome services (binds the optional "WebAwesome" configuration section)
builder.Services.AddWebAwesome(builder.Configuration);

// ... other services

var app = builder.Build();
```

Overloads: `AddWebAwesome()` for pure defaults, or `AddWebAwesome(options => ...)` for programmatic configuration.

### 2. Include Web Awesome Assets
Place the `WebAwesomeAssets` component in your application's head content (e.g. in `App.razor`):

```razor
<head>
    ...
    <WebAwesomeAssets />
</head>
```

By default it loads the matching Web Awesome version from the official CDN. Everything is configurable via the `WebAwesome` configuration section or the options delegate — asset source (CDN vs. self-hosted), version, explicit URLs, and a Font Awesome kit code for premium icon packs:

```jsonc
// appsettings.json — all values optional
{
  "WebAwesome": {
    "FontAwesomeKitCode": "your_kit_code_here" // configure via user secrets, never commit real codes
  }
}
```

#### Advanced: static asset tags
In standalone WebAssembly apps `index.html` is static, so add the equivalent tags directly (use the Web Awesome version that your package binds — the first three parts of the package version):

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@3.14.0/dist-cdn/styles/webawesome.css" />
<script type="module" src="https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@3.14.0/dist-cdn/webawesome.loader.js"></script>
```

## Quick start (wrapper usage)
```razor
@page "/demo"
@using WebAwesome.Blazor

<WaButton OnClick="() => Count++">Click me</WaButton>
<p>Clicked @Count times</p>

@code {
    private int Count;
}
```

> The Blazor wrappers are named `WaXxx` (e.g., `WaButton`, `WaTabs`). They internally render the corresponding `wa-xxx` web component and wire up events/attributes so you can use standard Blazor patterns.

> **Form controls need a binding.** Wrappers deriving from `InputBase<T>` (`WaInput`, `WaCheckbox`, `WaSwitch`, `WaSelect`, `WaRadioGroup`, `WaSlider`, `WaRange`, `WaColorPicker`, `WaRating`, `WaTextArea`) follow the standard Blazor input model: use them with `@bind-Value` (or inside an `EditForm`); bare usage throws at runtime, as with Blazor's built-in inputs.

### Using native elements directly (advanced)
If you prefer working with native web components, you can still render `<wa-button>` etc. The wrappers primarily exist to make event binding, parameter casing, and lifecycle interop seamless in Blazor.

## Features

### Custom Validation
All form controls support custom validation via the `SetCustomValidityAsync` method:

```csharp
@inject IFormValidation formValidation

<WaInput @ref="nameInput" Label="Name" required />
<WaButton OnClick="ValidateName">Check Name</WaButton>

@code {
    private WaInput nameInput = default!;

    private async Task ValidateName()
    {
        var value = nameInput.Value;
        if (value == "admin")
        {
            await nameInput.SetCustomValidityAsync("Admin is not allowed as a name");
        }
        else
        {
            await nameInput.SetCustomValidityAsync(""); // Clear validation error
        }
    }
}
```

Supported components:
- All Web Awesome form controls that implement the `WebAwesomeFormControl` interface
- Currently includes: `WaButton`, `WaCheckbox`, `WaColorPicker`, `WaInput`, `WaRadio`, `WaRadioGroup`, `WaSelect`, `WaSlider`, `WaSwitch`, `WaTextarea`
- Future form controls will be automatically supported without requiring updates to this library

## Extensions package (optional)
`WebAwesome.Blazor` binds what Web Awesome has: its components, attributes, events, slots and methods, and the behavior its documentation describes. A feature that adds behavior Web Awesome does not have goes into the separate package **`WebAwesome.Blazor.Extensions`** instead, so the main package stays pure bindings.

```bash
 dotnet add package WebAwesome.Blazor.Extensions
```

- Opt-in: an app that does not reference it gets nothing from it.
- Same version as `WebAwesome.Blazor`, on which it depends with a floor of that version. Use the same version of both packages.
- The Blazor runtime loads its JavaScript automatically (a JS initializer); no setup beyond `WebAwesome.Blazor` is necessary.
- It does not follow Web Awesome's custom-elements manifest, so the API parity suite does not check it. Its own tests and a public API snapshot do.

### Data grid cell presets
`WaDataGridFormatters` and `WaDataGridComparators` (namespace `WebAwesome.Blazor.Extensions.DataGrid`) give `WaDataGrid` columns rich cells from C#, with no JavaScript of your own: numbers, currencies, dates, lookups, badges, tags, icons, links, and sort orders. Each factory returns a `WaJsFunction` for `Formatter` or `ComparatorFunction`. A state badge:

```razor
@using WebAwesome.Blazor.Extensions.DataGrid

<WaDataGrid Data="orders" Columns="columns" RowKey="id" @onwablazor-cell-action="HandleCellAction" />

@code {
    private readonly WaDataGridColumn[] columns =
    [
        new() { Field = "id", Label = "Order", Formatter = WaDataGridFormatters.ActionLink("open", "#{id}") },
        new()
        {
            Field = "state", Label = "State",
            Formatter = WaDataGridFormatters.Badge<OrderState>(state => state switch
            {
                OrderState.Shipped => WaVariant.Success,
                OrderState.Cancelled => WaVariant.Danger,
                _ => WaVariant.Neutral,
            }, label: state => Resources.ResourceManager.GetString($"OrderState_{state}")),
            ComparatorFunction = WaDataGridComparators.Ordinal<OrderState>()
        },
        new() { Field = "total", Label = "Total", Formatter = WaDataGridFormatters.Currency("EUR") },
        new() { Field = "due", Label = "Due", Formatter = WaDataGridFormatters.Date(WaDataGridDateStyle.Medium) },
    ];

    private void HandleCellAction(WaDataGridCellActionEventArgs e)
        => Navigation.NavigateTo($"/orders/{e.GetRow<OrderRow>()!.Id}");
}
```

- Text presets return text, so the CSV export holds the formatted text; badges, tags, icons and links are elements. The culture is `CultureInfo.CurrentCulture` when the factory runs, or the `culture` you pass.
- Keep the column list in a field: the grid sends the columns again only when their JSON changes.
- A `DateOnly` value shows its calendar day in every time zone; for an instant, use UTC or `DateTimeOffset` in the row and set `timeZone`.
- Also in the package: buttons, action menus, switches and checkboxes that raise `@onwablazor-cell-action`; progress, rating, avatar, image, sparkline, trend and data bar cells; class rules, footer sums, a collation sort, a search over lookup labels, detail panels and selectable rows; and, for your own cell functions, the `ctx` helpers (`ctx.emit`, `ctx.invoke`, `ctx.h`, `ctx.preset`, `ctx.format`, `ctx.cache`) and `data-wablazor-action`; `WaDataGridRequestHandler`, which answers `ctx.invoke` requests in .NET; and `WaDataGridRazorTemplates`, which shows cells written in Razor as static HTML. The package readme lists them.

## Version alignment
We align binding versions to WA versions:
- The first three version parts are the bound Web Awesome version, including a WA prerelease suffix (`3.0.0-beta.6`).
- An optional fourth part is a **revision of the bindings**: changes to the bindings only, for the same Web Awesome release. `3.14.0.1` binds Web Awesome `3.14.0`. NuGet sorts `3.14.0` < `3.14.0.1` < `3.14.1`.
- A new Web Awesome release resets the revision (`3.14.0.1` → `3.15.0`). The default CDN version is always the Web Awesome version (`3.14.0`).

| Web Awesome | Bindings Tag                          | Branch (GitHub)       |
|-------------|----------------------------------------|-----------------------|
| 3.0.0       | `wa-blazor-3.0.0`           | `master` (promoted)     |
| 3.0.0-beta.6| `wa-blazor-3.0.0-beta.6`     | `master` (promoted)     |
| 3.0 train   | rolling prereleases → tags as above    | `master-WA-3.0`         |
| 3.0 patches | `wa-blazor-3.0.x` tagged on the subtrunk | `master-WA-3.0`       |
| 3.14.0 (bindings revision) | `wa-blazor-3.14.0.1` tagged on the subtrunk | `master-WA-3.14` |

> Development happens in PlasticSCM on hierarchical branches (`/main`, `/main/WA-3.0`); the GitHub mirror is produced by GitSync, whose fixed naming maps `/main` → `master` and flattens hierarchy with dashes (`/main/WA-3.0` → `master-WA-3.0`).

**Promotion model**
- Active development stays in a **subtrunk**: `/main/WA-<ver>` (e.g., `/main/WA-3.0`).
- When ready, we *promote* the subtrunk to `/main` and **tag** (e.g., `wa-blazor-3.0.0`).
- A new subtrunk (minor `WA-3.<x+1>` or major `WA-4.0`) is branched **from `/main`**, and only after the previous train's release has been promoted to `/main`.
- **Patch releases** (e.g., `3.0.1`) and **bindings revisions** (e.g., `3.14.0.1`) stay on the train's subtrunk: developed and tagged there, never promoted to `/main` by themselves. Before the next train starts, the subtrunk is merged into `/main`. A fix that matters to newer trains is merged from the older subtrunk directly into the newer one (e.g., `/main/WA-3.0` → `/main/WA-3.1`).

**Monotonic merge rule**
`/main` only accepts merges from the **current** active subtrunk or a **newer** one. After `/main` merges from `WA-N+1`, it **no longer** accepts merges from `WA-N`.

## Branches & contributions (high level)
- **Open PRs to the active subtrunk** (on GitHub: `master-WA-3.0`); patch fixes for a shipped train target that same subtrunk, new/breaking work targets the next one (e.g., `master-WA-3.1`).
- **Web Awesome version upgrades are maintainer work**, executed by the automated upgrade pipeline ([docs/contributing/UPGRADE-PROCESS.md](docs/contributing/UPGRADE-PROCESS.md)) — contributions are welcome for bug fixes and supplemental improvements (demo app, docs, tests), not for implementing WA releases.
- See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## Build & pack locally
```bash
 dotnet restore
 dotnet build -c Release
 dotnet pack -c Release
```

The repo uses a single-output-directory build system and produces nugets into `src/output/packages/<Configuration>`.

> CI packs on tags named `wa-blazor-*` and attaches both `.nupkg` files (`WebAwesome.Blazor` and `WebAwesome.Blazor.Extensions`) to the GitHub Release.

**Docs bundle (maintainers).** A local Release build also bundles the docs into both packages under `docs/`: the API reference, a skill and, in the main package, the changelog, the technical documentation, the migration guides and the Web Awesome docs. Packages built on GitHub (nuget.org) carry no docs bundle. The maintainers publish the local packages to the internal NuGet feed before the tag push:
- One-time setup: add the feed as a user-level NuGet source (`dotnet nuget add source <feed> --name <name>`, never in the repository), then run `dotnet tool restore`.
- Release step: `& .\tools\release\Publish-WaLocalFeed.ps1 -Source <name>`, before the tag push.
- Internal consumers map `WebAwesome.*` to the internal feed with `packageSourceMapping`, and clear a cached nuget.org copy of the same version.

See [CONTRIBUTING.md](CONTRIBUTING.md), "Maintainer setup and local release".

## Demo
A Blazor WebAssembly gallery app (`src/WebAwesome.Blazor.Demo`) exercises every component with source-visible examples, grouped into the same categories Web Awesome's own documentation uses. It is deployed to GitHub Pages on every merge to `main`: **<https://boldbrick.github.io/WebAwesome-Blazor/>**. It doubles as a manual regression harness for upgrades and is swept by automated Playwright tests (`tools/e2e`).

## Component coverage
- **90 component wrappers** — one Blazor wrapper per WA 3.14.0 component, following WA naming (`WaButton` ↔ `wa-button`). Every public member carries XML documentation that ships in the package, so the full API reference is available as IntelliSense; for component behavior, use the [official Web Awesome docs](https://webawesome.com/docs) together with the [live demo](https://boldbrick.github.io/WebAwesome-Blazor/) for the Blazor-specific syntax.
- **8 CSS layout components** covering Web Awesome's CSS-only layout system (`WaStack`, `WaCluster`, `WaFlank`, `WaFrame`, `WaGrid`, `WaSplit`, `WaText`, `WaVisuallyHidden`).
- Coverage is enforced by the API parity test suite against Web Awesome's custom-elements manifest.
- Components introduced in later WA releases arrive with the corresponding version upgrades on the release train.
- Missing something? File an issue or PR against the active subtrunk.

## Roadmap (contributions welcome!)
- Ride the release train to current Web Awesome (3.1 → 3.14.x) via the automated upgrade pipeline

> **Non-goal of `WebAwesome.Blazor`:** app-level convenience components (toast dispatch services, confirm dialogs, dark-mode toggles). It is an architectural decision that the main package ships pure bindings only — no extensions will be bundled into it. Features that add behavior upstream Web Awesome does not have go into the optional [`WebAwesome.Blazor.Extensions`](#extensions-package-optional) package.

## License
MIT — see `LICENSE.md`. Third-party content in the packages: see `THIRD-PARTY-NOTICES.md`.

## Trademarks
Web Awesome, Font Awesome, and any related marks are property of their respective owners. This project is an independent community binding and not affiliated with or endorsed by the Web Awesome authors.

