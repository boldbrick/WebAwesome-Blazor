# Web Awesome Blazor 3.15.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.14.0.x to 3.15.0.

The Web Awesome 3.15.0 release is additive: six new components (a code highlighter, a counter, a number ticker, a lightbox with its items, and the Pro rich text editor), a declared `disable-sticky` attribute on `wa-page`, and `wa-data-grid`, `wa-step` and `wa-stepper` leave experimental status. Nothing is removed upstream.

The bindings make **one breaking change** of their own: 3.15.0 removes the members that 3.14.0.1 deprecated. Each of them had a replacement since 3.14.0.1, and most of them did nothing or threw, so code that compiled without `CS0618` warnings on 3.14.0.x needs no change.

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
- [ ] Move the logic of `GetTag`, `ValueFormatter` and the string-based `Set...Async` methods into JavaScript functions referenced with `WaJsFunction`
- [ ] Replace `IconLibraryOptions.Mutator = "path"` with `MutatorFunction = WaJsFunction.Global("path")`
- [ ] Check slider values where `Min` is not a multiple of `Step`, and submit buttons with a cancelled click
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.15.0+
- **Breaking Changes**: Yes (section 1, removal of members deprecated in 3.14.0.1)
- **New Dependencies**: None
