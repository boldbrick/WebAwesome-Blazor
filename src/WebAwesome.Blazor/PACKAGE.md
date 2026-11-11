# Web Awesome Blazor Bindings

Strongly-typed Blazor wrappers for the [Web Awesome](https://webawesome.com) web components — idiomatic C# parameters and enums, EditForm/validation integration, named slots as render fragments, minimal JS interop. MIT-licensed. The first three version parts are the bound Web Awesome version; a fourth part marks a bindings revision.

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

## Building a sister wrapper set

A package can wrap custom elements of its own (for example `xyz-*` elements) on top of WebAwesome.Blazor with the rendering infrastructure of the library's own wrappers, in the namespace `WebAwesome.Blazor.Infrastructure` and the JS module `_content/WebAwesome.Blazor/webawesome-events.js`. The namespace is for wrapper and extension authors; app code that only uses the wrappers does not need it.

**1. Sticky attributes.** When Blazor removes an attribute, Lit sets the element property to null, not back to its default. Open the element with `OpenWaElement` and render each attribute that has a literal element default through a sticky overload: nothing while the parameter was never set, the value when set, and the element default (a public `Default<Name>` constant) once the parameter returns to null. An attribute without a literal default uses the plain overload; a plain boolean attribute uses `AddBooleanAttribute`.

```csharp
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Infrastructure;

public sealed class XyzChip : ComponentBase
{
    public const WaVariant DefaultVariant = WaVariant.Neutral;   // the element's own default

    [Parameter] public WaVariant? Variant { get; set; }
    [Parameter] public string? Label { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "xyz-chip");
        builder.AddAttributeIfNotNull(attributes, 1, "variant", Variant?.ToHtmlValue(), DefaultVariant.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(2, "label", Label);   // no literal default: plain
        builder.CloseElement();
    }
}
```

**2. Custom events.** Bind each own event of the element with `AddOwnEventIfHasDelegate`: the handler only when the callback is set, and Blazor's `stopPropagation` always, so the event of a nested element never reaches the callback of an outer wrapper. Declare the event for Razor on a public static class named `EventHandlers`; the event args inherit `System.EventArgs`.

```csharp
[EventHandler("onxyz-remove", typeof(XyzRemoveEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers { }

public sealed class XyzRemoveEventArgs : EventArgs { public string? Key { get; set; } }

// in XyzChip
[Parameter] public EventCallback<XyzRemoveEventArgs> OnRemove { get; set; }
// in BuildRenderTree, before CloseElement
builder.AddOwnEventIfHasDelegate(20, "onxyz-remove", OnRemove);
```

Register the event types with Blazor in the package's JS initializer, `wwwroot/<PackageId>.lib.module.js`. `registerEventTypes` maps each event name to its payload: `null` for the sanitized `event.detail` (`detailArgs`), a `createEventArgs` function, or `{ createEventArgs, browserEventName }`. It registers each name once; a name that is already registered through the module is skipped with a console warning. Event names are global for the page, so register only your own prefix. The module also exports `sanitize(value, depth)` and `isoDate(value)`.

```js
import { registerEventTypes, isoDate } from '../WebAwesome.Blazor/webawesome-events.js';

function register(blazor) {
  registerEventTypes(blazor, {
    'xyz-remove': null,                                             // the sanitized detail: { key }
    'xyz-day-change': event => ({ date: isoDate(event.detail.date) }),
  });
}

export function afterWebStarted(blazor) { register(blazor); }   // Blazor Web
export function afterStarted(blazor) { register(blazor); }      // WebAssembly and Server hosts
```

**3. Size default of form controls.** A form control derived from `WaInputBase<TValue>` (or `WaLabeledInputBase<TValue>`) renders `size` sticky with `ElementDefaultSize`, which is `DefaultSize` (medium). Override it when the element declares another size default, or return `null` when it declares none: `size` then renders plainly and is removed when `Size` returns to null.

```csharp
using WebAwesome.Blazor.Base;             // WaLabeledInputBase
using WebAwesome.Blazor.Infrastructure;   // OpenWaElement

public sealed class XyzChoiceGroup : WaLabeledInputBase<string?>
{
    protected override WaSize? ElementDefaultSize => null;   // xyz-choice-group has no size default
    // BuildRenderTree: OpenWaElement, AddCommonAttributes(builder, 1), ...
}
```

**4. Typed attribute helpers.** Booleans and dates never go through `ToString()`: `AddAttributeIfNotNull` refuses them at compile time (error CS0619, with the helper to use). Use `AddBooleanAttribute` (present/absent), `AddTrueFalseAttribute` ("true"/"false"), `AddOnOffAttribute` ("on"/"off"), `AddDateAttribute` (`yyyy-MM-dd`), `AddTimeAttribute` (`HH:mm[:ss]`), `AddDateTimeOffsetAttribute` (with the offset), `AddDateSetAttribute` and `AddDaysOfWeekAttribute`, each with a sticky overload. Numbers go through `AddNumberAttribute` or `AddAttributeIfNotNull`, in the invariant culture.

```csharp
builder.AddBooleanAttribute(3, "removable", Removable);
builder.AddTrueFalseAttribute(attributes, 4, "editable", Editable, DefaultEditable);
builder.AddDateAttribute(5, "since", Since);   // DateOnly? -> "2026-02-01"
```

`ChangeEventArgsExtensions` reads the value of a change event as the library's form controls do: `GetStringValue`, `TryParseJsNumber`, and `GetStringArrayValue` for a multi-value control, which binds the array value of its `change` event as `WaSelect` does (`var values = args.GetStringArrayValue();`). `WaWireFormat` formats and parses the wire forms of dates, times and lists (`TryParseDate`, `TryParseTime`, `FormatTime`, ...), and a typed control reports a parse failure with the protected `FormatValidationMessage("The {0} field must be a date.")` of `WaInputBase`.

**More helpers.** `FormControlRendering` renders the label cluster (`AddLabelAndHintAttributes`, `AddWithHintAndLabelAttributes`, `AddLabelAndHintSlots`) and the `start`/`end` slots (`AddAffixSlots`) for a control that implements `IWaLabeledControl` or `IWaAffixedControl` on another base. `WaEnumJsonConverter<TEnum>` serializes your own enum as the strings of its element:

```csharp
[JsonConverter(typeof(XyzToneJsonConverter))]
public enum XyzTone { Calm, VeryLoud }

public sealed class XyzToneJsonConverter : WaEnumJsonConverter<XyzTone>
{
    protected override string ToWire(XyzTone value) => value == XyzTone.Calm ? "calm" : "very-loud";
}
```

## Links

- [Repository & documentation](https://github.com/boldbrick/WebAwesome-Blazor)
- [Changelog](https://github.com/boldbrick/WebAwesome-Blazor/blob/master/docs/CHANGELOG.md)
- [Web Awesome documentation](https://webawesome.com/docs)

Web Awesome, Font Awesome, and related marks are property of their respective owners. This is an independent binding, not affiliated with or endorsed by the Web Awesome authors.
