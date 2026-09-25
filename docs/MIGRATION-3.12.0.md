# Web Awesome Blazor 3.12.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.11.0 to 3.12.0.

The Web Awesome 3.12.0 release itself is small and additive: `wa-dropdown-item` gained link attributes. This version of the bindings is still **breaking**, for two reasons:

- It fixes the defects reported in [GitHub issue #1](https://github.com/boldbrick/WebAwesome-Blazor/issues/1).
- It completes a correctness sweep that makes every enum match the value set Web Awesome accepts, and removes event callbacks that could never fire.

Most of the removed or renamed members never worked: they sent values Web Awesome rejects, or listened for events it never dispatches. Code that compiled against them was silently broken, and the compiler now points it out.

## Breaking Changes

### 1. WaRelativeTime — `Format` and `Numeric` (BREAKING)

**Impact**: High for `WaRelativeTime` users. The old enum didn't match Web Awesome.

`WaFormat { Auto, Relative, Numeric }` is removed. Web Awesome's `format` is `'long' | 'short' | 'narrow'` and `numeric` is `'always' | 'auto'`:
- `WaFormat.Relative`/`WaFormat.Numeric` passed invalid styles to `Intl.RelativeTimeFormat`, which throws, so the element rendered nothing.
- `short`/`narrow` couldn't be requested at all.
- `Numeric="false"` did nothing, because Blazor drops a `false` boolean attribute.

Both parameters are now nullable enums, emitted only when set (unset means Web Awesome's defaults, `long`/`auto`).

**Before (3.11.0):**
```razor
<WaRelativeTime Date="@date" Format="WaFormat.Auto" />
<WaRelativeTime Date="@date" Numeric="false" />
```

**After (3.12.0):**
```razor
<WaRelativeTime Date="@date" />
<WaRelativeTime Date="@date" Numeric="WaRelativeTimeNumeric.Always" />
<WaRelativeTime Date="@date" Format="WaRelativeTimeFormat.Short" />   <!-- "3 hr. ago" -->
<WaRelativeTime Date="@date" Format="WaRelativeTimeFormat.Narrow" />  <!-- "3h ago" -->
```

#### Code Search & Replace
1. Remove `Format="WaFormat.Auto"`. It's the default.
2. Replace `Numeric="false"` with `Numeric="WaRelativeTimeNumeric.Always"`. Remove `Numeric="true"`.
3. `WaFormat.Relative` / `WaFormat.Numeric` never rendered. Choose a `WaRelativeTimeFormat` value instead.

Month names in formatted dates ("August" instead of "8") are a `WaFormatDate` feature: `Month="WaDateTimeStyle.Long"`.

### 2. WaTextArea — `Rows` is `int?` (BREAKING for C# readers)

`Rows` was a non-nullable `int` that always rendered `rows="0"`, overriding Web Awesome's default of 4. It is now `int?` and emitted only when set. Assignments such as `Rows="6"` compile unchanged. Only code that reads `Rows` as an `int` needs a change.

`WaTextArea` now derives from `WaInputBase<string?>` like the other text inputs. Its members keep their names and types. It gains `OnKeyDown`/`OnKeyUp`/`OnKeyPress` and `Immediate`, and its `Size`/`Appearance`/`Resize` now render the correct Web Awesome values (`xs`/`xl`, `filled-outlined`), which were previously misspelled.

### 3. Enums now match Web Awesome's value sets (BREAKING)

Every enum parameter now accepts exactly the values its Web Awesome attribute accepts. Removed members sent values that Web Awesome ignores or rejects.

| Change | Affects | Migration |
|---|---|---|
| `WaAppearance.Text` removed (`text` is accepted by no component) | `WaButton`, `WaCallout`, `WaCard` | use `WaAppearance.Plain` |
| New `WaDetailsAppearance` (`Filled`, `Outlined`, `FilledOutlined`, `Plain`) | `WaAccordion.Appearance`, `WaDetails.Appearance` | `WaAppearance.X` → `WaDetailsAppearance.X`; `OutlinedFilled` → `FilledOutlined` |
| New `WaBadgeAppearance` (`Accent`, `Filled`, `Outlined`, `FilledOutlined`) | `WaBadge.Appearance`, `WaTag.Appearance` | `WaAppearance.X` → `WaBadgeAppearance.X`; `OutlinedFilled` → `FilledOutlined` |
| `WaPlacement.Start` / `WaPlacement.End` removed (`start`/`end` accepted nowhere) | tooltip, popover, popup, dropdown, color picker | use a side (`Top`, `Bottom`, ...) or an aligned value (`TopStart`, ...) |
| New `WaListboxPlacement` (`Top`, `Bottom`) | `WaSelect.Placement`, `WaCombobox.Placement` | `WaPlacement.Top/Bottom` → `WaListboxPlacement.Top/Bottom` |
| New `WaTooltipSide` (`Top`, `Right`, `Bottom`, `Left`) | `WaSlider.TooltipPlacement`, `WaCopyButton.TooltipPlacement` | `WaPlacement.X` → `WaTooltipSide.X` |
| New `WaPickerPlacement` (`Top`, `TopStart`, `TopEnd`, `Bottom`, `BottomStart`, `BottomEnd`) | `WaDateInput.Placement`, `WaTimeInput.Placement` | `WaPlacement.X` → `WaPickerPlacement.X` |
| `WaRadioAppearance.Normal` → `Default` (emits `default`; `normal` was invalid) | `WaRadio.Appearance` | rename, or leave `Appearance` unset |
| `WaAutoSize.Width`/`Height` → `Horizontal`/`Vertical` (Web Awesome takes `horizontal`/`vertical`; `width`/`height` silently did nothing) | `WaPopup.AutoSize` | rename |
| `WaDropdownItemType.Radio` removed (never valid) | `WaDropdownItem.Type` | use `Checkbox`, or a `WaRadioGroup` |
| New `WaDropdownItemVariant` (`Default`, `Danger`) | `WaDropdownItem.Variant` | `WaVariant.Danger` → `WaDropdownItemVariant.Danger`; other `WaVariant` values were ignored |
| `WaFormatDate` per-option enums: `Weekday`/`Era` → `WaDateTimeTextStyle`; `Year`/`Day`/`Hour`/`Minute`/`Second` → `WaDateTimeNumericStyle`; `TimeZoneName` → `WaTimeZoneNameStyle`; `Month` keeps `WaDateTimeStyle` | `WaFormatDate` | `WaDateTimeStyle.Numeric` → `WaDateTimeNumericStyle.Numeric`, `WaDateTimeStyle.Long` → `WaDateTimeTextStyle.Long` / `WaTimeZoneNameStyle.Long`. Out-of-range values made `Intl.DateTimeFormat` throw. |
| `WaHourFormat.Auto` added | `WaFormatDate.HourFormat` | none (additive) |
| `WaRange.TooltipPlacement` is `WaTooltipSide?` (was `string?`) | `WaRange` | `TooltipPlacement="top"` → `TooltipPlacement="WaTooltipSide.Top"` |
| `WaTrigger` is a `[Flags]` enum with `Focus` added; `WaTooltip.Trigger` is `WaTrigger?`. Unset means Web Awesome's default `hover focus`, which the old `WaTrigger.Hover` default silently meant. | `WaTooltip` | remove `Trigger="WaTrigger.Hover"`; combine with `\|`, e.g. `WaTrigger.Hover \| WaTrigger.Click` |

Missing union values were added (additive): `WaIconAnimation` gains the nine newer Font Awesome animations (`Flip360`, `SpinSnap`, `SpinSnap4`, `SpinSnap8`, `Buzz`, `Wag`, `Float`, `Swing`, `Jello`); `WaAnimationFill.Auto`; `WaInputType.DateTimeLocal`/`Time`; `WaCurrencyDisplay.NarrowSymbol`; `WaDisplay.Narrow`.

`WaTrigger.Hover` set explicitly now renders `trigger="hover"` (hover only). Previously it omitted the attribute and so meant `hover focus`. Leave `Trigger` unset to keep `hover focus`. The unused `WaDropdownTrigger` and `WaTriggerType` enums are removed (`wa-dropdown` has no `trigger` attribute; nothing consumed either enum).

`WaFormatNumber.Notation`, `CompactDisplay` and `UseGrouping` are removed, along with the `WaNotation`/`WaCompactDisplay` enums. `wa-format-number` has never had these attributes, so they did nothing. Use `WithoutGrouping` to turn off digit grouping.

`WaSize.Small`/`Medium`/`Large` now render `s`/`m`/`l`. Web Awesome 3.12.0 deprecates `small`/`medium`/`large` and will remove them in its next major version. The C# API is unchanged. Only CSS or tests that match the rendered `size` attribute text need updating.

### 4. Event callbacks that never fired (BREAKING)

These callbacks were bound to events Web Awesome never dispatches. No such event name appears anywhere in the compiled Web Awesome 3.0.0 or 3.12.0 sources, so they never fired. They're removed.

| Removed | Use instead |
|---|---|
| `WaInput.OnPasswordToggle`, `WaInput.OnPasswordVisibilityChange` | none. Web Awesome reports no password-visibility event; `PasswordVisible` is one-way |
| `WaCopyButton.OnSuccess` | `OnCopy` (`wa-copy` is the success notification) |
| `WaDialog.OnInitialFocus`, `WaDrawer.OnInitialFocus` | none |
| `WaOption.OnSelectedChange`, `WaRadio.OnCheckedChange` | the parent's binding: `WaSelect`/`WaCombobox`/`WaRadioGroup` `@bind-Value` |
| `WaZoomableFrame.OnZoomChange` and `ZoomChangeEventArgs` | none |

These callbacks are **kept and now work**:
- `WaCheckbox.OnCheckedChange`, `WaSwitch.OnCheckedChange`, `WaRadioGroup.OnValueChange` and `WaSlider.OnValueChange` fire on every user change.
- `WaZoomableFrame.OnLoad`/`OnError` now listen to the element's real `load`/`error` events.

### 5. Parameters for attributes the element doesn't have (BREAKING)

A render-based check now compares every attribute a wrapper renders with the attributes Web Awesome declares. These parameters rendered an attribute the element doesn't have, so they did nothing. They're removed.

| Removed | Why it did nothing | Use instead |
|---|---|---|
| `WaRadio.Checked` | `wa-radio`'s checked state is internal and set by its radio group; the element ignores a `checked` attribute | `WaRadioGroup` `@bind-Value` (or `Value`) |
| `WaTab.Closable` | `wa-tab` has had no `closable` since Web Awesome 3.0 | none |
| `WaMutationObserver.Subtree` | the element always observes the whole subtree | remove it |
| `WaMutationObserver.AttributeOldValue`, `CharacterDataOldValue` | rendered `attribute-old-value`/`character-data-old-value`, which don't exist | `AttrOldValue`, `CharDataOldValue` |
| `WaPopup.FlipBoundary`, `ShiftBoundary`, `AutoSizeBoundary` | Web Awesome takes these as live `Element` objects set from JavaScript; the rendered `flip-boundary`/`shift-boundary`/`auto-size-boundary` attributes don't exist | set the element property from JavaScript |

Two `WaMutationObserver` parameters are **kept and now work**:
- `Attr="true"` rendered an empty `attr`, which watches no attributes. It now renders `attr="*"` (all attributes).
- `AttributeFilter` rendered a nonexistent `attribute-filter`. It now sets `attr` to the list, e.g. `AttributeFilter="class id"`, and takes precedence over `Attr`.

## Behavioral Changes (non-breaking, but visible)

- **Numbers render in the invariant culture.** Blazor formats a number passed to an attribute with the current culture, so under a culture such as cs-CZ `Distance="0.5"` rendered `distance="0,5"` (and negative numbers could get a U+2212 minus), which Web Awesome can't parse. Every number attribute (`WaPopup.Distance`, `WaAnimation.PlaybackRate`, `WaSlider.Step`, `WaNumberInput`'s value, and about 40 more) and `WaRelativeTime.Date` now use the invariant culture.

- **C# value changes reach the element after the user has edited it.** In Web Awesome 3 the `value`/`checked` attribute sets only the default, and the element ignores it once the user has interacted. The form controls now also assign the live property after a C#-side change. This makes reset-after-submit, normalizing setters, and "clear"/"select all" buttons work. Affected: `WaInput`, `WaTextArea`, `WaNumberInput`, `WaColorPicker`, `WaDateInput`, `WaKnownDate`, `WaOtpInput`, `WaRadioGroup`, `WaTimeInput`, `WaSlider`, `WaRange`, `WaCheckbox` and `WaSwitch`.
- **`WaSlider`, `WaRange` and `WaRating` user edits now reach `@bind-Value`.** These elements report their value as a number, which Blazor's built-in change event can't carry. The server rejected every change event, so the bound model never updated. The wrappers now listen on a string-valued alias of the change event.
- **`SetRangeTextAsync` updates the bound value** on `WaInput` and `WaTextArea` (Web Awesome dispatches no event for it). `EditContext` is notified.
- **Range-mode `WaSlider` no longer requires `@bind-Value`.** Bind `MinValue`/`MaxValue` only.

## New Features

### Immediate binding — `Immediate` on `WaInput`, `WaTextArea`, `WaNumberInput`

By default the bound value updates when the control commits (`change`, on blur). With `Immediate="true"` it updates on every keystroke. Your `OnInput` handler still runs, after the value has been updated. So a `OnKeyDown` handler such as Ctrl+Enter "send" sees the current text:

```razor
<WaTextArea @bind-Value="message" Immediate="true" OnKeyDown="HandleKeyDown" />
```

### WaDropdownItem link items — `Href`, `Target`, `Rel`, `Download`

```razor
<WaDropdownItem Href="/docs">Documentation</WaDropdownItem>
<WaDropdownItem Href="https://github.com/boldbrick/WebAwesome-Blazor" Target="_blank" Rel="noopener noreferrer">GitHub</WaDropdownItem>
<WaDropdownItem Href="/files/report.pdf" Download="report.pdf">Download report</WaDropdownItem>
```

The item stays a menu item for assistive technology, so the label should describe where the link goes. `Href` is ignored on items with a submenu. `WaDropdownItem.OnBlur`/`OnFocus` now actually fire; they were previously bound under the wrong attribute names.

## Packaging

The package's license file is now resolved relative to `Directory.Build.props`. Building the repository as a git submodule under a consuming solution (with its own `SolutionDir`) no longer fails with NU5019/NU5030.

## Migration Checklist

- [ ] Replace `WaFormat` on `WaRelativeTime` (see section 1)
- [ ] Update C# code that reads `WaTextArea.Rows` as `int`
- [ ] Rebuild and fix every compile error against the enum table in section 3. Each one was a value Web Awesome didn't accept, or a type that now matches the component
- [ ] Remove handlers for the callbacks listed in section 4
- [ ] Remove the parameters listed in section 5 (`WaRadio.Checked`, `WaTab.Closable`, the `WaMutationObserver` and `WaPopup` boundary parameters)
- [ ] If you worked around the value-sync bug (forcing a re-render with `@key`, JS interop to set `.value`), remove the workaround
- [ ] Update CSS selectors or tests that match `size="small|medium|large"` to `s|m|l`
- [ ] (Optional) Adopt `Immediate` where the model must be current during typing
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.12.0+
- **Breaking Changes**: Yes (sections 1–5)
- **New Dependencies**: None
