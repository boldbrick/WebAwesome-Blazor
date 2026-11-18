---
description: "Use when writing or changing code that uses WebAwesome.Blazor: strongly typed Blazor wrappers for the Web Awesome web components (C# parameters and enums, EditForm validation, named slots as render fragments), asset delivery, icon libraries, layout utilities and the data grid; key namespaces WebAwesome.Blazor.Components, WebAwesome.Blazor.Extensions, WebAwesome.Blazor.Models, WebAwesome.Blazor.Layout, WebAwesome.Blazor.Base, WebAwesome.Blazor.Services; key types ServiceCollectionExtensions, WebAwesomeAssets, WebAwesomeOptions, WaButton, WaInput, WaDataGrid, WaJsFunction, WaIconLibraryService, WaInputBase."
---

# WebAwesome.Blazor

## Capabilities

- Blazor components that wrap the Web Awesome elements (`WaButton`, `WaInput`, `WaSelect`, `WaDialog`, `WaIcon`, `WaDataGrid`, `WaRichTextEditor`, `WaLightbox` and more) in WebAwesome.Blazor.Components, with enums such as `WaVariant`, `WaSize` and `WaAppearance`.
- Form controls derive from `WaInputBase<TValue>` (an InputBase<TValue>): `@bind-Value` and EditForm validation; custom validity through `IFormValidation`.
- Asset delivery: `WebAwesomeAssets` writes the stylesheet, the autoloader and the optional Font Awesome kit or self-hosted icon path from `WebAwesomeOptions` (CDN by default, or self-hosted).
- Icon libraries at run time: `WaIconLibraryService` (Lucide, Heroicons, Font Awesome Pro, custom).
- Layout utilities in WebAwesome.Blazor.Layout: `WaStack`, `WaCluster`, `WaGrid`, `WaFlank`, `WaSplit`.
- JavaScript function references (`WaJsFunction`) for members that take a function, for example `WaDataGridColumn.Formatter` or `WaSelect.GetTagFunction`.

## Entry points

- `ServiceCollectionExtensions.AddWebAwesome` (namespace WebAwesome.Blazor.Extensions): no argument, an Action of `WebAwesomeOptions`, or an IConfiguration (section `ServiceCollectionExtensions.ConfigurationSection`, "WebAwesome").
- `WebAwesomeAssets`: once in the head content (App.razor); `WebAwesomeAssets.Options` overrides the options for one instance.
- `WebAwesomeOptions`: `AssetSource`, `Version`, `CdnBaseUrl`, `BasePath`, `StylesheetUrl`, `LoaderUrl`, `FontAwesomeKitCode`, `IconPath`, `EmitInspectorMetadata`.
- `WaIconLibraryService`: `RegisterLucideAsync`, `RegisterHeroiconsAsync`, `RegisterFontAwesomeProAsync`, `RegisterIconLibraryAsync` with `IconLibraryOptions`, `SetDefaultIconFamilyAsync`, `SetIconPathAsync`.
- `WaInputBase<TValue>`, `WaLabeledInputBase<TValue>`: `Label`, `Hint`, `Size`, `Disabled`, `OnInput`, `SetCustomValidityAsync`, `ResetValidityAsync`.
- `WaDataGrid` with `WaDataGridColumn`, `WaDataGridState` and the CSV and copy options in WebAwesome.Blazor.Models.
- `WaJsFunction.Registered`, `WaJsFunction.Module`, `WaJsFunction.Global`, then `WithArgs` and `Uses`.
- Wrappers of your own custom elements (not for app code): WebAwesome.Blazor.Infrastructure, `RenderTreeBuilderExtensions.OpenWaElement`, `WaAttributeMemory`; see "Building a sister wrapper set" in the package readme.

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

- `AddWebAwesome` is in this package, not in the WebAwesome.Blazor.Extensions package. It registers TimeProvider.System when no TimeProvider is registered; the date components read the time from it.
- `WebAwesomeOptions.Version` defaults to the first three parts of the package version. `StylesheetUrl` and `LoaderUrl` override the derived URLs; `BasePath` applies only with `WaAssetSource.SelfHosted`.
- Never hard-code `FontAwesomeKitCode` or the Pro kit code; take it from configuration or user secrets. To keep icons off the Font Awesome CDN, set `WebAwesomeOptions.IconPath`: `SetIconPathAsync` comes after the first icons loaded. Turn `EmitInspectorMetadata` on in development and test only.
- `WaIconLibraryService` acts on the page: call it once interactive (`OnAfterRenderAsync`), not during prerendering.
- Form controls bind on commit (the change event). `WaInput.Immediate` binds on every keystroke; `OnInput` then runs after the value update.
- `Element` is null before the first render. `SetCustomValidityAsync` with an empty string clears the error. A subclass of `WaInputBase<TValue>` that overrides `OnAfterRenderAsync` must call the base.
- An unset parameter renders no attribute, so the Web Awesome default applies.
- `WaDialog.Open`, `WaLightbox.Open` and `WaLightbox.Index` are one-way: the element changes them and they are not written back. Follow `OnHide`/`OnAfterHide` and `WaLightbox.OnItemChange`.
- `WaDataGrid` and `WaRichTextEditor` are Pro components. `WaDataGrid.Data` is pushed on every parameter set, `Columns` only when its JSON changes. For server data set `WaDataGrid.Server`, handle `WaDataGrid.OnDataRequest`, and assign `Data`, `Total` and `Loading`.
- A function member takes a `WaJsFunction`, never source text; the browser calls it with no round trip to .NET. An unresolved reference writes one console.error and the member is left out. Output in a shadow root needs `Uses` for its elements and `WaDataGrid.ShadowStyles` for styles. `WithArgs` serializes at once (camelCase). Do not keep or spread ctx.

## Full documentation

`docs/dependencies/synced/WebAwesome.Blazor/INDEX.md`; paths below are relative to it.

- Signatures: `api/<Namespace>/<Type>.md`; `api/symbols.tsv` lists every public name.
- Component behavior: `content/web-awesome/components/<name>.md` (the API page summary names the tag). Upstream HTML maps: attribute to PascalCase parameter, wa-x event to OnX, named slot to XxxContent, default slot to ChildContent, method to XxxAsync. Check each name on the API page.
- Styling: `content/web-awesome/customizing.md`, `theming-overview.md`, `utilities/`, `tokens/`.
- Upgrades: `content/CHANGELOG.md`, then each `content/migration/MIGRATION-<version>.md` above your version, in order.
- Wrapper conventions: `content/technical/technical.md`.
