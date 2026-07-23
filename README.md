# Web Awesome Blazor Bindings

[![build](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/build.yml/badge.svg)](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/build.yml)
[![release](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/release.yml/badge.svg)](https://github.com/boldbrick/WebAwesome-Blazor/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/WebAwesome.Blazor.svg)](https://www.nuget.org/packages/WebAwesome.Blazor)

Blazor-first wrappers for the **Web Awesome (WA)** web components, providing idiomatic C# APIs, eventing, and attributes that play nicely with Blazor (Server & WebAssembly). Focused on seamless integration into the Blazor ecosystem. No additional application logic and no additional / extension components.

> **Status**
> - Active train: **WA 3.3**
> - Current alignment: **WA 3.3.0** (tagged releases use `wa-blazor-<version>`)

**[Live demo](https://boldbrick.github.io/WebAwesome-Blazor/)** — every component, rendered from this library; published to GitHub Pages as a Blazor WebAssembly app. It can also be built and run locally in server mode: `dotnet run --project src/WebAwesome.Blazor.Demo.Server`.

## Why this library

⚖️ **MIT-licensed, commercial-friendly.** Free for closed-source and commercial applications, no copyleft obligations. Backed by [BoldBrick](https://www.boldbrick.com).

🧩 **Blazor-native architecture — no JavaScript layer to fight.** Wrappers are pure C# render trees; Web Awesome's `wa-*` custom events bind directly to `EventCallback<T>` handlers with typed event args. There are no per-component JS modules, no `document.getElementById` lookups, no library-generated element ids. The entire JS interop surface is a single ~100-line module driven by `ElementReference` — collision-proof, prerender-safe, and trivially auditable.

📝 **Real form integration, not string soup.** Form controls derive from Blazor's `InputBase<TValue>`: typed values (`bool`, `decimal`, not just strings), `@bind-Value`, full `EditForm`/`EditContext` participation, DataAnnotations validation, validation CSS classes, and `SetCustomValidityAsync` for constraint validation — the same programming model as Blazor's built-in inputs.

🔒 **Strongly typed all the way down.** String attributes with fixed value sets are nullable C# enums; an unset parameter omits the attribute entirely so Web Awesome's own defaults apply. Enum-to-attribute mapping lives in one place, not scattered per component.

✅ **Machine-verified completeness.** A reflection-based API parity suite checks every wrapper against Web Awesome's published custom-elements manifest — attributes, slots, events. "Did we miss a parameter?" is a red/green test signal, not a code-review hope. 229 automated tests plus a Playwright-based end-to-end sweep of the demo app back every release.

🚄 **Built to track Web Awesome releases fast.** A CEM-driven upgrade pipeline diffs upstream API surfaces, generates change reports, and drives wrapper updates and tests (see [docs/UPGRADE-PROCESS.md](docs/UPGRADE-PROCESS.md)). Package versions are **identical** to the Web Awesome version they bind — no parallel version scheme to decode.

📐 **Covers what others skip.** Web Awesome's CSS-only layout system ships as components too (`WaStack`, `WaCluster`, `WaFlank`, `WaGrid`, …), alongside utility elements like `WaFormatDate`, `WaFormatNumber`, and the observer components.

✨ **Ergonomic where it counts.** Common slot usage is one attribute (`StartIconName="gear"`), with `RenderFragment` slots (`StartContent`, `EndContent`, …) always available as the general mechanism.

📦 **Clean packaging.** Multi-targeted `net9.0`/`net10.0`, XML IntelliSense docs on every public member (missing docs fail the build), symbol packages (snupkg) with SourceLink, deterministic CI builds.

🎯 **Pure bindings, no lock-in.** By deliberate architectural decision, the package contains no application logic, no extension components, and redistributes no Web Awesome Pro source — you bring WA assets via CDN or self-hosting, exactly as upstream documents.

## Package
- NuGet: `WebAwesome.Blazor` *(published from release tags)*
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
In standalone WebAssembly apps `index.html` is static, so add the equivalent tags directly (adjust the version to the release you use — it always matches this package's version):

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@3.3.0/dist-cdn/styles/webawesome.css" />
<script type="module" src="https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@3.3.0/dist-cdn/webawesome.loader.js"></script>
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

## Version alignment
We align binding versions to WA versions. Use the **same** semantic version when possible; for prereleases we follow WA suffixes.

| Web Awesome | Bindings Tag                          | Branch (GitHub)       |
|-------------|----------------------------------------|-----------------------|
| 3.0.0       | `wa-blazor-3.0.0`           | `master` (promoted)     |
| 3.0.0-beta.6| `wa-blazor-3.0.0-beta.6`     | `master` (promoted)     |
| 3.0 train   | rolling prereleases → tags as above    | `master-WA-3.0`         |
| 3.0 patches | `wa-blazor-3.0.x` tagged on the subtrunk | `master-WA-3.0`       |

> Development happens in PlasticSCM on hierarchical branches (`/main`, `/main/WA-3.0`); the GitHub mirror is produced by GitSync, whose fixed naming maps `/main` → `master` and flattens hierarchy with dashes (`/main/WA-3.0` → `master-WA-3.0`).

**Promotion model**
- Active development stays in a **subtrunk**: `/main/WA-<ver>` (e.g., `/main/WA-3.0`).
- When ready, we *promote* the subtrunk to `/main` and **tag** (e.g., `wa-blazor-3.0.0`).
- A new subtrunk (minor `WA-3.<x+1>` or major `WA-4.0`) is branched **from `/main`**, and only after the previous train's release has been promoted to `/main`.
- **Patch releases** (e.g., `3.0.1`) stay on the train's subtrunk: developed and tagged there, never promoted to `/main`. A fix that matters to newer trains is merged from the older subtrunk directly into the newer one (e.g., `/main/WA-3.0` → `/main/WA-3.1`).

**Monotonic merge rule**
`/main` only accepts merges from the **current** active subtrunk or a **newer** one. After `/main` merges from `WA-N+1`, it **no longer** accepts merges from `WA-N`.

## Branches & contributions (high level)
- **Open PRs to the active subtrunk** (on GitHub: `master-WA-3.0`); patch fixes for a shipped train target that same subtrunk, new/breaking work targets the next one (e.g., `master-WA-3.1`).
- **Web Awesome version upgrades are maintainer work**, executed by the automated upgrade pipeline ([docs/UPGRADE-PROCESS.md](docs/UPGRADE-PROCESS.md)) — contributions are welcome for bug fixes and supplemental improvements (demo app, docs, tests), not for implementing WA releases.
- See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## Build & pack locally
```bash
 dotnet restore
 dotnet build -c Release
 dotnet pack -c Release
```

The repo uses a single-output-directory build system and produces nugets into `src/output/packages/<Configuration>`.

> CI packs on tags named `wa-blazor-*` and attaches the `.nupkg` to the GitHub Release.

## Demo
A Blazor WebAssembly gallery app (`src/WebAwesome.Blazor.Demo`) exercises every component with source-visible examples, grouped into the same categories Web Awesome's own documentation uses. It is deployed to GitHub Pages on every merge to `main`: **<https://boldbrick.github.io/WebAwesome-Blazor/>**. It doubles as a manual regression harness for upgrades and is swept by automated Playwright tests (`tools/e2e`).

## Component coverage
- **74 component wrappers** — one Blazor wrapper per WA 3.3.0 component, following WA naming (`WaButton` ↔ `wa-button`). Every public member carries XML documentation that ships in the package, so the full API reference is available as IntelliSense; for component behavior, use the [official Web Awesome docs](https://webawesome.com/docs) together with the [live demo](https://boldbrick.github.io/WebAwesome-Blazor/) for the Blazor-specific syntax.
- **8 CSS layout components** covering Web Awesome's CSS-only layout system (`WaStack`, `WaCluster`, `WaFlank`, `WaFrame`, `WaGrid`, `WaSplit`, `WaText`, `WaVisuallyHidden`).
- Coverage is enforced by the API parity test suite against Web Awesome's custom-elements manifest.
- Components introduced after WA 3.3.0 (date/time pickers, …) arrive with the corresponding version upgrades on the release train.
- Missing something? File an issue or PR against the active subtrunk.

## Roadmap (contributions welcome!)
- Ride the release train to current Web Awesome (3.1 → 3.10.x) via the automated upgrade pipeline

> **Non-goal:** app-level convenience components (toast dispatch services, confirm dialogs, dark-mode toggles). It is an architectural decision that this library ships pure bindings only — no extensions will be bundled.

## License
MIT — see `LICENSE.md`.

## Trademarks
Web Awesome, Font Awesome, and any related marks are property of their respective owners. This project is an independent community binding and not affiliated with or endorsed by the Web Awesome authors.

