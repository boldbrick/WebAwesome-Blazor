---
description: "Use when writing or changing code that uses WebAwesome.Blazor: strongly typed Blazor wrappers for the Web Awesome web components (C# parameters and enums, EditForm validation, named slots as render fragments), asset delivery, icon libraries, layout utilities and the data grid; key namespaces WebAwesome.Blazor.Components, WebAwesome.Blazor.Extensions, WebAwesome.Blazor.Models, WebAwesome.Blazor.Layout, WebAwesome.Blazor.Base, WebAwesome.Blazor.Services; key types ServiceCollectionExtensions, WebAwesomeAssets, WebAwesomeOptions, WaButton, WaInput, WaDataGrid, WaJsFunction, WaIconLibraryService, WaInputBase."
---

# WebAwesome.Blazor

## Capabilities

- Blazor components that wrap the Web Awesome elements (`WaButton`, `WaInput`, `WaSelect`, `WaDialog`, `WaIcon`, `WaDataGrid` and more) in WebAwesome.Blazor.Components, with enums such as `WaVariant`, `WaSize` and `WaAppearance`.
- Form controls derive from `WaInputBase<TValue>` (an InputBase<TValue>), so they bind with `@bind-Value` and work in an EditForm; custom validity through `IFormValidation`.
- Asset delivery: `WebAwesomeAssets` writes the stylesheet, the autoloader module and an optional Font Awesome kit code, from `WebAwesomeOptions` (CDN by default, or self-hosted).
- Icon libraries at run time through `WaIconLibraryService` (Lucide, Heroicons, Font Awesome Pro, custom libraries).
- Layout utilities in WebAwesome.Blazor.Layout: `WaStack`, `WaCluster`, `WaGrid`, `WaFlank`, `WaSplit` and more.
- JavaScript function references (`WaJsFunction`) for members that take a function, for example `WaDataGridColumn.Formatter`.

## Entry points

- `ServiceCollectionExtensions.AddWebAwesome` (namespace WebAwesome.Blazor.Extensions): registers the services; overloads take nothing, an Action of `WebAwesomeOptions` or an IConfiguration (section `ServiceCollectionExtensions.ConfigurationSection`, "WebAwesome").
- `WebAwesomeAssets`: put once in the head content (for example in App.razor); `WebAwesomeAssets.Options` overrides the registered options for one instance.
- `WebAwesomeOptions`: `AssetSource`, `Version`, `CdnBaseUrl`, `BasePath`, `StylesheetUrl`, `LoaderUrl`, `FontAwesomeKitCode`.
- `WaIconLibraryService`: `RegisterLucideAsync`, `RegisterHeroiconsAsync`, `RegisterFontAwesomeProAsync`, `RegisterIconLibraryAsync` with `IconLibraryOptions`, `SetDefaultIconFamilyAsync`.
- `WaInputBase<TValue>`, `WaLabeledInputBase<TValue>`: shared form-control members (`Label`, `Hint`, `Size`, `Disabled`, `OnInput`, `SetCustomValidityAsync`, `ResetValidityAsync`).
- `WaDataGrid` with `WaDataGridColumn`, `WaDataGridState` and the CSV and copy options in WebAwesome.Blazor.Models.
- `WaJsFunction.Registered`, `WaJsFunction.Module`, `WaJsFunction.Global`, then `WithArgs` and `Uses`.
- Building wrappers of your own custom elements (a sister wrapper set): namespace WebAwesome.Blazor.Infrastructure (for wrapper and extension authors only; app code does not need it) with `RenderTreeBuilderExtensions.OpenWaElement` and `WaAttributeMemory` (sticky attributes), `AddOwnEventIfHasDelegate`, the typed attribute helpers, `ChangeEventArgsExtensions`, `WaWireFormat`, `FormControlRendering` and `WaEnumJsonConverter<TEnum>`; `WaInputBase.ElementDefaultSize` and `FormatValidationMessage`; and `registerEventTypes` of `_content/WebAwesome.Blazor/webawesome-events.js`. See "Building a sister wrapper set" in the package readme.

## Typical usage

```csharp
// Program.cs
using WebAwesome.Blazor.Extensions;

builder.Services.AddWebAwesome(builder.Configuration);   // binds the "WebAwesome" section
```

```razor
@* App.razor head: <WebAwesomeAssets /> *@
@using WebAwesome.Blazor.Components
@using WebAwesome.Blazor.Services
@inject WaIconLibraryService Icons

<EditForm Model="model" OnValidSubmit="Save">
    <WaInput @bind-Value="model.Name" Label="Name" Immediate="true" />
    <WaButton Type="WaButtonType.Submit" Variant="WaVariant.Brand">Save</WaButton>
</EditForm>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await Icons.RegisterLucideAsync();   // interactive only, not in prerendering
    }
}
```

## Pitfalls

- `AddWebAwesome` is in the namespace WebAwesome.Blazor.Extensions of this package, not in the separate WebAwesome.Blazor.Extensions package. It also registers TimeProvider.System when no TimeProvider is registered; the date components read the time from it.
- `WebAwesomeOptions.Version` defaults to the first three parts of the package version (3.14.0.1 binds Web Awesome 3.14.0). `StylesheetUrl` and `LoaderUrl` override the URLs derived from `AssetSource`; `BasePath` is used only with `WaAssetSource.SelfHosted`.
- Never hard-code `FontAwesomeKitCode` or the kit code of `RegisterFontAwesomeProAsync`; supply it from configuration or user secrets.
- In a standalone WebAssembly app with a static index.html, prefer the static asset tags of the README to `WebAwesomeAssets`.
- `WaIconLibraryService` acts on the Web Awesome instance in the page: call it once the app is interactive (`OnAfterRenderAsync`), not during prerendering.
- Form controls bind on commit (the change event, e.g. on blur). Set `WaInput.Immediate` to bind on every keystroke; `OnInput` then runs after the value update.
- `Element` is null before the first render. `SetCustomValidityAsync` with an empty string clears the error; `ResetValidityAsync` clears it without a new submit.
- A subclass of `WaInputBase<TValue>` that overrides `OnAfterRenderAsync` must call the base implementation.
- An unset parameter renders no attribute, so the Web Awesome default applies.
- `WaDataGrid` is a Web Awesome Pro component. `Data` and `Columns` are JavaScript properties: `Data` is pushed on every parameter set, `Columns` only when its serialized JSON changes. The upstream dataSource is not bound: set `WaDataGrid.Server`, handle `WaDataGrid.OnDataRequest`, and assign `Data`, `Total` and `Loading` yourself.
- `WaJsFunction` never sends source text. An unresolved reference writes one console.error; the member is left out. Function output renders in a shadow root, where the autoloader does not look: declare its elements with `Uses` and style it with `WaDataGrid.ShadowStyles`. `WithArgs` serializes at once (camelCase). `Module` refuses another origin unless allowCrossOrigin is true. Read the members of ctx; do not keep or spread it.

## Full documentation

`docs/dependencies/synced/WebAwesome.Blazor/INDEX.md`; paths below are relative to it.

- Signatures: `api/<Namespace>/<Type>.md`; `api/symbols.tsv` lists every public name.
- Component behavior: read `content/web-awesome/components/<name>.md` beside the wrapper API page, whose summary names the tag (`WaTextArea`: wa-textarea, textarea.md). The upstream page shows HTML: an attribute is a PascalCase parameter, a wa-x event OnX, a named slot a XxxContent RenderFragment, the default slot ChildContent, a method XxxAsync. Check each name on the API page.
- Styling: `content/web-awesome/customizing.md` (CSS parts, custom properties), `theming-overview.md`, `utilities/`, `tokens/`.
- Upgrades: `content/CHANGELOG.md`, then each `content/migration/MIGRATION-<version>.md` above your version, in order.
- Wrapper conventions: `content/technical/technical.md`.
