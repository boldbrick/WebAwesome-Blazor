# Web Awesome Blazor 3.15.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.14.0.x to 3.15.0.

The Web Awesome 3.15.0 release is additive: six new components (a code highlighter, a counter, a number ticker, a lightbox with its items, and the Pro rich text editor), a declared `disable-sticky` attribute on `wa-page`, and `wa-data-grid`, `wa-step` and `wa-stepper` leave experimental status. Nothing is removed upstream.

The bindings make **one breaking change** of their own on every target: 3.15.0 removes the members that 3.14.0.1 deprecated. Each of them had a replacement since 3.14.0.1, and most of them did nothing or threw, so code that compiled without `CS0618` warnings on 3.14.0.x needs no change.

3.15.0 also adds a **`net11.0`** target. On net11.0 only, `WaInput.Min` and `Max` take the C# 15 union type `WaInputBound`, which replaces the typed accessors such as `MinDate` (section 2, breaking for net11.0 projects only). net9.0 and net10.0 projects keep the 3.14 API. **3.15 is the last release with a `net9.0` assembly** (see [Target frameworks](#target-frameworks)).

## Breaking Changes

### 1. Members deprecated in 3.14.0.1 are removed (BREAKING)

**Impact**: Low. You're only affected if your code still uses one of these members; the compiler has flagged each use with an obsolete warning (`CS0618`) since 3.14.0.1, and now it fails to compile, so it finds every place to change.

| Removed | What it did in 3.14.0.x | Use instead |
|---|---|---|
| `WaSelect.GetTag` (a `Func<WaOption, int, string>`) | nothing read it | `WaSelect.GetTagFunction` |
| `WaSelect.SetGetTagFunctionAsync(string)` | threw `NotSupportedException` | `WaSelect.GetTagFunction` |
| `WaRating.SetSymbolFunctionAsync(string)` | threw `NotSupportedException` | `WaRating.GetSymbolFunction` |
| `SetValueFormatterAsync(string)` of `WaSlider` and `WaRange` | sent nothing | `ValueFormatterFunction` |
| `WaSlider.ValueFormatter` (a `Func<decimal, string>`) | nothing read it | `WaSlider.ValueFormatterFunction` |
| `IconLibraryOptions.Mutator` (a global function path) | worked | `IconLibraryOptions.MutatorFunction = WaJsFunction.Global(path)` |

The replacements take a `WaJsFunction`: a reference to a JavaScript function that the browser calls directly, with no round trip to .NET. Refer to a function in one of three ways:
- `WaJsFunction.Registered("name")`: a function that your app registers with `registerFunction` of the module `_content/WebAwesome.Blazor/webawesome-functions.js`
- `WaJsFunction.Module("./Pages/Prices.razor.js", "formatPrice")`: an export of an ES module, for example a collocated `.razor.js` file
- `WaJsFunction.Global("myApp.formatPrice")`: a function on `window`

The function gets the element's own arguments, followed by a `ctx` object (`ctx.args` holds the value of `WithArgs`, `ctx.element` the element).

**Before (3.14.0.x):**
```razor
<WaSelect @ref="select" @bind-Value="color">...</WaSelect>
<WaSlider @bind-Value="volume" ValueFormatter="@(v => $"{v}%")" />

@code {
    private WaSelect? select;
    private string? color;
    private decimal volume;

    private readonly IconLibraryOptions icons = new()
    {
        Resolver = "/icons/{name}.svg",
        Mutator = "myApp.icons.thinStroke"
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await select!.SetGetTagFunctionAsync("(option) => ...");
    }
}
```

**After (3.15.0):**
```razor
<WaSelect @bind-Value="color" GetTagFunction="@(WaJsFunction.Module("./Pages/Colors.razor.js", "colorTag"))">...</WaSelect>
<WaSlider @bind-Value="volume" ValueFormatterFunction="@(WaJsFunction.Global("myApp.percent"))" />

@code {
    private string? color;
    private decimal volume;

    private readonly IconLibraryOptions icons = new()
    {
        Resolver = "/icons/{name}.svg",
        MutatorFunction = WaJsFunction.Global("myApp.icons.thinStroke")
    };
}
```

```javascript
// Pages/Colors.razor.js
export function colorTag(option, index, ctx) {
    return `<wa-tag size="small">${option.label}</wa-tag>`;
}
```

A function that renders Web Awesome elements declares them with `Uses("wa-tag")`, so the autoloader registers them; see the XML documentation of `WaJsFunction`.

**Mechanical fix.** Search your code for `GetTag =`, `ValueFormatter =`, `SetGetTagFunctionAsync`, `SetSymbolFunctionAsync`, `SetValueFormatterAsync` and `Mutator =`, and move the logic into a JavaScript function referenced as above. `Mutator = "path"` becomes `MutatorFunction = WaJsFunction.Global("path")` with no other change; `RegisterIconLibraryAsync` no longer checks for both being set.

### 2. On net11.0, `WaInput.Min` and `Max` are the union type `WaInputBound` (BREAKING, net11.0 only)

**Impact**: Medium for projects that target net11.0 and set the bounds of a `WaInput`; none for net9.0 and net10.0 projects, which keep the string plus typed accessor API of 3.14.

**Why.** `wa-input`'s `min` and `max` take a number for a number input and an ISO date, time or local date-time for the date and time types. C# before version 15 cannot type one parameter as "a number or a date or a time", so 3.12.0 introduced a raw `string?` `Min`/`Max` plus one typed accessor per value type (`MinDecimal`, `MinLong`, `MinULong`, `MinDate`, `MinTime`, `MinDateTime`, and the `Max` equivalents), with a rule that at most one per bound may be set. C# 15 (.NET 11) has union types, so on net11.0 each bound is now one parameter of one type:

```csharp
public union WaInputBound(string, decimal, int, long, ulong, DateOnly, TimeOnly, DateTime);

[Parameter] public WaInputBound? Min { get; set; }
[Parameter] public WaInputBound? Max { get; set; }
```

**What changes on net11.0.**

- `Min` and `Max` are `WaInputBound?` instead of `string?`.
- The accessors `MinDecimal`, `MinLong`, `MinULong`, `MinDate`, `MinTime`, `MinDateTime`, `MaxDecimal`, `MaxLong`, `MaxULong`, `MaxDate`, `MaxTime` and `MaxDateTime` do not exist. In C#, each use fails to compile. **In Razor markup it does not**: `WaInput` captures unmatched attributes (`AdditionalAttributes`), so `<WaInput MinDate="@day" />` still compiles, renders a stray `MinDate` HTML attribute (formatted with the current culture) and sets no bound at all. Search the markup for the accessor names; the compiler will not find them for you.
- The `InvalidOperationException` for two accessors of one bound is gone: a bound has one value by construction.
- Rendering is unchanged. Each case renders exactly the text the matching accessor rendered, culture-free: a `decimal`, `long` or `ulong` in the invariant culture (`2.5`, never `2,5`), a `DateOnly` as `yyyy-MM-dd`, a `TimeOnly` as `HH:mm` (or `HH:mm:ss` with seconds), a `DateTime` as the local `yyyy-MM-ddTHH:mm` (with seconds and milliseconds when not zero; the kind is ignored), and a `string` unchanged. Null renders no attribute, and so does a default `WaInputBound`.

**Before (3.14, and net9.0/net10.0 in 3.15) and after (net11.0):**

```razor
@* before *@
<WaInput Type="WaInputType.Date" @bind-Value="start" MinDate="@(new DateOnly(2026, 1, 1))" MaxDate="@lastDay" />
<WaInput Type="WaInputType.Number" @bind-Value="amount" MinDecimal="0.5m" MaxLong="100" />
<WaInput Type="WaInputType.Time" @bind-Value="time" MinTime="@opening" MaxTime="@closing" />
<WaInput Type="WaInputType.DateTimeLocal" @bind-Value="meeting" MinDateTime="@DateTime.Today" />
<WaInput Type="WaInputType.Number" @bind-Value="count" Min="0" Max="10" />

@* after, net11.0 *@
<WaInput Type="WaInputType.Date" @bind-Value="start" Min="@(new DateOnly(2026, 1, 1))" Max="@lastDay" />
<WaInput Type="WaInputType.Number" @bind-Value="amount" Min="@(0.5m)" Max="@(100)" />
<WaInput Type="WaInputType.Time" @bind-Value="time" Min="@opening" Max="@closing" />
<WaInput Type="WaInputType.DateTimeLocal" @bind-Value="meeting" Min="@DateTime.Today" />
<WaInput Type="WaInputType.Number" @bind-Value="count" Min="0" Max="10" />   @* unchanged *@

@code {
    private DateOnly lastDay = new(2026, 12, 31);   // a DateOnly, not a DateOnly? (see "Nullable values" below)
    private TimeOnly opening = new(9, 0);
    private TimeOnly closing = new(17, 30);
}
```

In C#, assign the value directly; every case converts implicitly:

```csharp
// before
input.MinDate = new DateOnly(2026, 1, 1);
input.MaxDecimal = 99.5m;

// after, net11.0
input.Min = new DateOnly(2026, 1, 1);
input.Max = 99.5m;
```

**How Razor binds a union parameter (verified with the .NET 11.0.100 SDK).** Know these rules before you change the markup; some of them are not what you expect from other non-string parameters:

- **A plain attribute value is text.** Because `WaInputBound` has a `string` case, Razor passes a plain value as a string, as it did for the 3.14 `string?` parameter: `Min="2026-01-01"`, `Min="0"` and `Min="2.5"` compile and render that text unchanged. Existing markup with plain values therefore keeps working.
- **A typed value needs `@(...)`.** `Min="@(new DateOnly(2026, 1, 1))"`, `Min="@someDate"`, `Min="@(2.5m)"`, `Min="@(100)"`.
- **Do not write a C# suffix in a plain value.** `Min="2.5m"` is the text `2.5m`, which the native input does not read as a number. Write `Min="2.5"` (text) or `Min="@(2.5m)"` (a decimal).
- **Integers bind as `int` or `long`.** `Min="@(10)"` and `input.Min = 10;` bind the `int` case, and so do `int`, `short` and `sbyte` variables; a `long` value (`@(10L)`, a `long` variable, a constant above the `uint` range) binds `long`, a `ulong` binds `ulong`. All render the plain invariant number. A `uint`, `ushort`, `byte` or `char` value does not compile (`CS0457`, ambiguous between the number cases), and neither does a constant between the `int` and `uint` maximums such as `3_000_000_000`: cast it (`@((long)count)`) or write the `L` suffix.
- **No floating-point case.** A `double` or `float` does not convert (`CS0029`). `wa-input` 3.15.0 only calls `Number()` on the bound and has no notion of floating or fixed point, so a fraction is a `decimal`: `@((decimal)ratio)`.
- **Nullable values do not convert.** The union has no conversion from `DateOnly?`, `decimal?`, `long?` and the other nullable value types (`CS0029`), where the accessors took them. Pass `@(lastDay is { } day ? day : null)` or `@(lastDay.HasValue ? lastDay.Value : null)`; null renders no attribute. A null `string` converts to a null bound.
- **`Min="null"` is the text `null`.** Use `Min="@null"`, or leave the attribute out.

**Mechanical fix.** Rename each accessor to its bound (`MinDate`, `MinDecimal`, `MinLong`, `MinULong`, `MinTime`, `MinDateTime` to `Min`; the same for `Max`), then make the value a typed C# expression: wrap a plain literal in `@(...)` with the right suffix (`MinDecimal="0.5m"` becomes `Min="@(0.5m)"`, `MaxLong="100"` becomes `Max="@(100)"`), and unwrap a nullable variable as shown above. A raw `Min`/`Max` string needs no change. Search the `.razor` and `.cs` files for `\b(Min|Max)(Decimal|Long|ULong|Date|Time|DateTime)\b` (in Razor this search is the only way to find them; see above). If one project multi-targets net10.0 and net11.0, keep plain text values (they compile on both), or select the parameter per framework with `#if NET11_0_OR_GREATER` in a code-behind.

## Target frameworks

- **`net11.0` is new.** Both packages ship `net9.0`, `net10.0` and `net11.0` assemblies. On net11.0 they depend on `Microsoft.AspNetCore.Components.Web` and `Microsoft.Extensions.Configuration.Abstractions` 11.0.0 or later. net10.0 (LTS) stays the primary target. The only API difference between the targets is section 2.
- **`net9.0` support ends with 3.15.** .NET 9 left Microsoft support on 2026-11-10. 3.15 is the last release that ships a `net9.0` assembly; the next version targets `net10.0` and `net11.0` only. A net9.0 project can stay on 3.15.x; to take later releases, move it to .NET 10 (LTS) or .NET 11.

## New Components

All new components are experimental upstream.

- **`WaCodeHighlighter`** (`wa-code-highlighter`, free) highlights code: pass it as `Code`, or as the text of the child content (dedented unless `WithoutDedent`), with a `Language`. `Wrap`, `WithoutCopy`, `Label`; `OnLoad`, and `OnError` when the language has no grammar.
- **`WaCounter`** (`wa-counter`, free) counts down to (or up from) a `Date`, or down from a `Duration` in milliseconds: `Direction`, `Format`, `LargestUnit`/`SmallestUnit`, `Paused`, `WithoutLabels`, a slot per unit and a `FinishedContent` slot, `OnFinish`, and `ValueFormatterFunction`.
- **`WaNumberTicker`** (`wa-number-ticker`, free) animates a number from `StartValue` to `Value` when it scrolls into view (or at once with `Eager`), with the number options of `WaFormatNumber`, `Duration`, `Delay`, `Easing`, `OnFinish`, `RestartAsync` and `ValueFormatterFunction`.
- **`WaLightbox`** and **`WaLightboxItem`** (`wa-lightbox`, `wa-lightbox-item`, free) show images and videos in a full-screen overlay with navigation, zoom and thumbnails. `Open` and `Index` are one-way like `WaDialog.Open`: follow the element with `OnHide`/`OnAfterHide` and `OnItemChange`.
- **`WaRichTextEditor`** (`wa-rich-text-editor`, **Pro**) is a form control that binds HTML (or Markdown with `Format`) through `@bind-Value`, with a configurable toolbar, heading levels, colors, disabled features, validation, `OnSelectionChange`, editing methods (`RunCommandAsync`, `IsActiveAsync`, `InsertHtmlAsync`, `UndoAsync`, ...) and `ImageProviderFunction`/`EmbedProviderFunction`.

```razor
<WaRichTextEditor Label="Description" @bind-Value="description" MaxLength="500" WithCount="true" />

@code {
    private string? description = "<p>Hello <strong>world</strong></p>";
}
```

## Other changes

- `WaPage.DisableSticky` is unchanged; Web Awesome 3.15.0 now declares the attribute.
- Web Awesome 3.15.0 changes two behaviours you may notice: a submit or reset `WaButton` no longer submits or resets its form when the click is cancelled (`@onclick:preventDefault`), and `WaSlider`/`WaRange` snap values to `Min + k * Step` instead of multiples of `Step`.

See the [changelog](../CHANGELOG.md) for the full list.

## Migration Checklist

- [ ] Build against 3.15.0 and fix each compile error from a removed member with its replacement (section 1)
- [ ] net11.0 projects: replace the `WaInput` accessors (`MinDate`, `MinDecimal`, ...) with `Min`/`Max` and typed values in `@(...)`; check plain values with C# suffixes and nullable variables (section 2)
- [ ] net9.0 projects: plan the move to .NET 10 or .NET 11 before the next release
- [ ] Move the logic of `GetTag`, `ValueFormatter` and the string-based `Set...Async` methods into JavaScript functions referenced with `WaJsFunction`
- [ ] Replace `IconLibraryOptions.Mutator = "path"` with `MutatorFunction = WaJsFunction.Global("path")`
- [ ] Check slider values where `Min` is not a multiple of `Step`, and submit buttons with a cancelled click
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Target frameworks**: net9.0 (last release), net10.0 (primary), net11.0
- **Web Awesome Core**: 3.15.0+
- **Breaking Changes**: Yes (section 1, removal of members deprecated in 3.14.0.1; section 2, `WaInput.Min`/`Max` on net11.0 only)
- **New Dependencies**: None
