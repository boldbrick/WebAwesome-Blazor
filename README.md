# Web Awesome Blazor Bindings

Blazor-first wrappers for the **Web Awesome (WA)** web components, providing idiomatic C# APIs, eventing, and attributes that play nicely with Blazor (Server & WebAssembly). Focused on seamless integration into the Blazor ecosystem. No additional application logic and no additional / extension components.

> **Status**
> - Active train: **WA 3.0**
> - Current alignment: **WA 3.0.0-beta.4** (tagged releases use `wa-blazor-<version>`)

## Why this library
- Use WA components with **strongly-typed** Blazor parameters and events.
- String attributes with specified sets of values are declared as `enum`.
- CSS-only layouts are covered as components, too (e.g. a `div class="wa-flank"` is implemented as a `WaFlank` class).
- Minimal JS interop surface; wrappers mirror WA names while following Blazor conventions.
- Versioned to follow WA releases (beta/rc/stable) with clear mapping.

## Package
- NuGet: `WebAwesome.Blazor` *(published from release tags)*

## Requirements
- .NET 9 (or latest LTS supported by the repo)
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

// Add Web Awesome services
builder.Services.AddWebAwesome();

// ... other services

var app = builder.Build();
```

### 2. Include Web Awesome Assets
Ensure the Web Awesome JavaScript library is loaded in your application (e.g., in `_Host.cshtml` or `wwwroot/index.html`):

```html
<script type="module" src="https://cdn.jsdelivr.net/npm/web-awesome@3.0.0-beta.4/dist/web-awesome.js"></script>
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
| 3.0.0-beta.4| `wa-blazor-3.0.0-beta.4`     | `main` (promoted)     |
| 3.0 train   | rolling prereleases → tags as above    | `main/wa-3.0`         |
| 3.0 stable  | `v3.0.x` maintained on release branch  | `release-3.0` (later) |

**Promotion model**
- Active development stays in a **subtrunk**: `main/wa-<ver>` (e.g., `main/wa-3.0`).
- When ready, we *promote* the subtrunk to `main` and **tag** (e.g., `wa-blazor-3.0.0-beta.4`).
- After WA 3.0 stabilizes, we create `release-3.0` for maintenance; `main` moves on to `main/wa-3.1` or `main/wa-4.0`.

**Monotonic merge rule**
`main` only accepts merges from the **current** active subtrunk or a **newer** one. After `main` merges from `wa-N+1`, it **no longer** accepts merges from `wa-N`.

## Branches & contributions (high level)
- **Open PRs to the active subtrunk** (e.g., `main/wa-3.0`).
- Once a stable release branch exists (e.g., `release-3.0`), 3.0.x fixes target that branch; new/breaking work targets the next subtrunk (e.g., `main/wa-3.1`).
- See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## Build & pack locally
```bash
 dotnet restore
 dotnet build -c Release
 dotnet pack -c Release
```

The repo use a single-output-directory build system and produces nugets into `src/output/package/<Configuration>`.

> CI packs on tags named `wa-blazor-*` and attaches the `.nupkg` to the GitHub Release.

## Component coverage
- We aim for one Blazor wrapper per WA component, following WA naming where sensible.
- _Pro_ components not yet covered.
- Missing component? File an issue or PR against the active subtrunk.

## Roadmap (contributions welcome!)
- Complete WA 3.0 coverage
- Docs site with interactive samples built as a Blazor project
- Unit test coverage

## License
MIT — see `LICENSE`.

## Trademarks
Web Awesome, Font Awesome, and any related marks are property of their respective owners. This project is an independent community binding and not affiliated with or endorsed by the Web Awesome authors.

