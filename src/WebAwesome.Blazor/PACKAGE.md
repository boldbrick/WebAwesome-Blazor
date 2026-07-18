# Web Awesome Blazor Bindings

Strongly-typed Blazor wrappers for the [Web Awesome](https://webawesome.com) web components — idiomatic C# parameters and enums, EditForm/validation integration, named slots as render fragments, minimal JS interop. MIT-licensed. Versions follow Web Awesome releases one-to-one.

## Install

```bash
dotnet add package WebAwesome.Blazor
```

## Setup

Register the services in `Program.cs`:

```csharp
using WebAwesome.Blazor.Extensions;

builder.Services.AddWebAwesome(builder.Configuration);
```

Include the Web Awesome assets in your head content (e.g. in `App.razor`):

```razor
<WebAwesomeAssets />
```

The component loads the matching Web Awesome version from the official CDN by default; asset source, version, URLs, and a Font Awesome kit code are configurable via the `WebAwesome` configuration section.

## Use

```razor
@using WebAwesome.Blazor.Components

<WaButton Variant="WaVariant.Brand" OnClick="() => Count++">Click me</WaButton>
```

## Links

- [Repository & documentation](https://github.com/boldbrick/WebAwesome-Blazor)
- [Changelog](https://github.com/boldbrick/WebAwesome-Blazor/blob/main/docs/CHANGELOG.md)
- [Web Awesome documentation](https://webawesome.com/docs)

Web Awesome, Font Awesome, and related marks are property of their respective owners. This is an independent binding, not affiliated with or endorsed by the Web Awesome authors.
