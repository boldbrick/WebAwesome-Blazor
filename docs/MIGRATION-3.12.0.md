# Web Awesome Blazor 3.12.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.11.0 to 3.12.0.

The Web Awesome 3.12.0 release itself is small and additive: `wa-dropdown-item` gained link attributes. This version of the bindings is still **breaking**, for these reasons:

- It fixes the defects reported in [GitHub issue #1](https://github.com/boldbrick/WebAwesome-Blazor/issues/1).
- It completes a correctness sweep that makes every enum match the value set Web Awesome accepts, and removes event callbacks that could never fire.
- It types the date and time controls (`DateOnly`, `TimeOnly`, `DateTimeOffset`, `WaDateRange`, sets of dates and weekdays) instead of wire-format strings, and splits the range mode into wrappers of its own (section 10).
- It types every other parameter whose value set or shape is known: closed value sets are enums, the `number | 'any'` step is `WaStep`, lists are collections, event payloads are records, and `WaInput`'s bounds get typed accessors (sections 11 to 14).
- Parameters at Web Awesome's default no longer render, and a rendered attribute is never removed again (section 15).

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
- `AttributeFilter` rendered a nonexistent `attribute-filter`. It now sets `attr` to the list, and takes precedence over `Attr`; it is a set of attribute names since this release (section 13).

### 6. Form control parameters the element doesn't have (BREAKING)

`WaInputBase` declared `Readonly`, `Required`, `MinLength`, `MaxLength`, `Autocomplete`, `Label` and `Hint` for every form control and rendered all seven attributes, but many elements don't declare some of them and ignore them, so the parameters did nothing there. Each parameter now lives on exactly the wrappers whose element declares the attribute; there it keeps its name, type and behaviour.

| Wrapper | Removed | Kept |
|---|---|---|
| `WaCheckbox` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete`, `Label` | `Required`, `Hint` |
| `WaSwitch` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete`, `Label` | `Required`, `Hint` |
| `WaColorPicker` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete` | `Required`, `Label`, `Hint` |
| `WaSelect` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete` | `Required`, `Label`, `Hint` |
| `WaCombobox` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete` | `Required`, `Label`, `Hint` |
| `WaRadioGroup` | `Readonly`, `MinLength`, `MaxLength`, `Autocomplete` | `Required`, `Label`, `Hint` |
| `WaSlider` | `Required`, `MinLength`, `MaxLength`, `Autocomplete` | `Readonly`, `Label`, `Hint` |
| `WaRange` | `Required`, `MinLength`, `MaxLength`, `Autocomplete` | `Readonly`, `Label`, `Hint` |
| `WaRating` | `Hint`, `MinLength`, `MaxLength`, `Autocomplete` | `Readonly`, `Required`, `Label` |
| `WaNumberInput` | `MinLength`, `MaxLength` | `Readonly`, `Required`, `Autocomplete`, `Label`, `Hint` |
| `WaDateInput` | `MinLength`, `MaxLength` | `Readonly`, `Required`, `Autocomplete`, `Label`, `Hint` |
| `WaTimeInput` | `MinLength`, `MaxLength` | `Readonly`, `Required`, `Autocomplete`, `Label`, `Hint` |
| `WaKnownDate` | `MinLength`, `MaxLength` | `Readonly`, `Required`, `Autocomplete`, `Label`, `Hint` |
| `WaOtpInput` | `MinLength`, `MaxLength` | `Readonly`, `Required`, `Autocomplete`, `Label`, `Hint` |
| `WaInput`, `WaTextArea` | none | all seven |

What to do:
- `WaCheckbox`/`WaSwitch` `Label`: put the label text in the content, `<WaCheckbox>Accept terms</WaCheckbox>`.
- `WaRating` `Hint`: render the hint next to the rating yourself.
- `WaSlider`/`WaRange` `Required`: a slider always has a value; validate the bound model instead.
- `WaNumberInput` `MinLength`/`MaxLength`: use `Min`/`Max` for the value range.
- Everything else in the "Removed" column: remove it. It never had an effect.

C# code that sets a removed parameter (for example through `nameof(WaCheckbox.Label)` or a parameter builder) no longer compiles. **Razor markup still compiles**: `<WaCheckbox Label="...">` is captured by `AdditionalAttributes` and rendered as a plain attribute, which the element ignores just as before. Search your markup for the combinations in the table and remove them.

Derived components that relied on `WaInputBase.AddCommonAttributes` rendering these attributes must render the ones their element declares themselves, at the sequence numbers + 6..12 the method leaves free.

### 7. Slot content the element doesn't have (BREAKING)

A render-based check now compares every slot a wrapper renders content into with the slots Web Awesome declares. These parameters rendered their content into a slot the element doesn't have, so it was never shown. They're removed.

| Wrapper | Removed | Why it was never shown | Use instead |
|---|---|---|---|
| `WaCheckbox` | `MarkupLabel` | `wa-checkbox` has no `label` slot; its label is the default-slot content | `ChildContent`: `<WaCheckbox>Accept <b>terms</b></WaCheckbox>` |
| `WaSwitch` | `MarkupLabel` | `wa-switch` has no `label` slot; its label is the default-slot content | `ChildContent` |
| `WaRating` | `MarkupLabel`, `MarkupHint` | `wa-rating` has no slots at all | `Label` (plain text); render a hint next to the rating yourself |
| `WaComparison` | `ChildContent` | `wa-comparison` has no default slot, only `before`, `after` and `handle` | `BeforeContent`, `AfterContent` |
| `WaSlider` | `ChildContent` | `wa-slider` has no default slot; the reference labels it was documented for go into the `reference` slot | `ReferenceContent` (new), e.g. `<ReferenceContent><span>Low</span><span>High</span></ReferenceContent>` |

`MarkupLabel` and `MarkupHint` moved from `WaInputBase` into the form controls whose element declares the `label`/`hint` slot, with the same name, type and behaviour: `MarkupHint` stays on `WaCheckbox` and `WaSwitch`, and both stay on every other form control. The protected `WaInputBase.AddLabelAndHintSlots(builder, sequence)` now takes the fragments, `AddLabelAndHintSlots(builder, sequence, markupLabel, markupHint)`; a derived component passes its own parameters (or `null` for a slot its element lacks).

C# that sets a removed parameter no longer compiles, and neither does Razor child content for `WaSlider`, `WaComparison` or `WaRating`, which now accept none. A `<MarkupLabel>` child element inside `<WaCheckbox>`/`<WaSwitch>` still compiles, with Razor warning RZ10012 ("Found markup element with unexpected name"), and ends up as ordinary label content; move its content into the checkbox's own content.

### 8. Parameter defaults now match Web Awesome's (BREAKING)

Every wrapper parameter now defaults to what the element itself does when the attribute is absent: an unset parameter renders nothing, and the element's own default applies. These parameters had a different C# default, which either rendered a value the element wouldn't pick, or wasn't rendered and so made the value it named unreachable.

| Wrapper | Parameter | Old default | New default (Web Awesome's) | To keep the old behaviour |
|---|---|---|---|---|
| `WaAnimation` | `Fill` | `WaAnimationFill.None`, rendered `fill="none"` | `null` (type `WaAnimationFill?`): the element's `auto` | `Fill="WaAnimationFill.None"` |
| `WaAnimation` | `Iterations` | `1`, rendered (type `decimal`) | `null` (type `double?`, see section 13): the element's `Infinity`, so an animation loops until stopped | `Iterations="1"` |
| `WaAnimatedImage` | `Play` | `true`, rendered `play` | `false`: the image starts paused, like the element, until the user plays it | `Play="true"` |
| `WaQrCode` | `ErrorCorrection` | `WaErrorCorrection.M`, rendered (type `WaErrorCorrection`) | `null` (type `WaErrorCorrection?`): the element's `H` | `ErrorCorrection="WaErrorCorrection.M"` |
| `WaCopyButton` | `CopyLabel`, `SuccessLabel`, `ErrorLabel` | the English `"Copy to clipboard"`, `"Copied!"`, `"Copy failed"`, rendered, which overrode the element's localized labels | `null`: the element's labels in the page's language | set the labels explicitly |
| `WaPopup` | `FlipFallbackStrategy` | `"initial"`, rendered | `null` (type `WaFlipFallbackStrategy?`, see section 11): the element's `best-fit` | `FlipFallbackStrategy="WaFlipFallbackStrategy.Initial"` |
| `WaPage` | `MobileBreakpoint` | `"768px"`, rendered | `null`: the element's own `768px` | nothing: the page behaves as before |
| `WaCallout` | `Variant` | `WaVariant.Neutral`, not rendered (so the callout was `brand`, and an explicit `Neutral` could never be set) | `null` (type `WaVariant?`): `brand`, or the variant of an enclosing element | nothing: an unset callout looks as before, and `Variant="WaVariant.Neutral"` now renders neutral |
| `WaCallout` | `Appearance` | `WaAppearance.OutlinedFilled`, not rendered (an explicit `OutlinedFilled` could never be set) | `null` (type `WaAppearance?`): the element's own styling | nothing; `Appearance="WaAppearance.OutlinedFilled"` now renders |
| `WaDropdown` | `Distance` | `8`, not rendered (so the gap was the element's 0, and an explicit 8 could never be set) | `null` (type `int?`): the element's `0` | nothing: an unset dropdown looks as before, and `Distance="8"` now renders an 8px gap |

The first six change what an unset parameter does; set the value shown to keep the previous look. C# that reads one of the parameters whose type became nullable, or `WaPage.MobileBreakpoint`, must handle `null`.

### 9. Renamed parameters (BREAKING)

The form control hierarchy now shares one label and hint cluster (`IWaLabeledControl`), and `WaFileInput` was the only control whose markup label and hint had different names.

| Wrapper | Old name | New name | Slot |
|---|---|---|---|
| `WaFileInput` | `LabelContent` | `MarkupLabel` | `label` |
| `WaFileInput` | `HintContent` | `MarkupHint` | `hint` |

Rename them; the type (`RenderFragment?`) and behaviour are unchanged. C# that sets the old names no longer compiles. In Razor, an old `<LabelContent>`/`<HintContent>` child element still compiles, with warning RZ10012 ("Found markup element with unexpected name"), but it becomes child content, which `WaFileInput` doesn't take, so rendering fails at runtime.

The other moves of the hierarchy (the label, popup and slider parameters now declared by `WaLabeledInputBase<TValue>`, `WaPopupInputBase<TValue>` and `WaSliderBase<TValue>`) keep every name, type and behaviour and need no change.

### 10. Date and time values are strongly typed (BREAKING)

The date and time controls took their values, bounds and disabled days as the strings Web Awesome parses. They now take .NET types, and the wrapper converts them culture-free into exactly the form the element reads: ISO `yyyy-MM-dd` dates, 24-hour `HH:mm`/`HH:mm:ss` times, `sun`..`sat` weekday tokens, and instants with their offset. Every change fails at compile time except the `WaRelativeTime.Date` one (see the note below the table).

| Wrapper | Member | Old type | New type |
|---|---|---|---|
| `WaDateInput` | `Value` (`@bind-Value`) | `string?` | `DateOnly?` |
| `WaDateInput`, `WaDatePicker` | `Mode`, `MinRange`, `MaxRange` | `WaDateSelectionMode` / `WaDateSelectionMode?`, `int?` | removed: use `WaDateRangeInput` / `WaDateRangePicker` (below); the `WaDateSelectionMode` enum is removed |
| new `WaDateRangeInput`, `WaDateRangePicker` | `Value` (`@bind-Value`) | (the range mode of the above, `string?` `from/to`) | `WaDateRange?`, with `From`/`To` as `DateOnly?`; plus `MinRange`/`MaxRange` (`int?`) |
| `WaDatePicker` | `Value` / `ValueChanged` (`@bind-Value`) | `string?` / `EventCallback<string?>` | `DateOnly?` / `EventCallback<DateOnly?>` |
| `WaDateInput`, `WaDatePicker` (and the range wrappers) | `Min`, `Max`, `Today` | `string?` (`YYYY-MM-DD`) | `DateOnly?` |
| same | `DisabledDates` | `string?` (space-separated ISO dates) | `IReadOnlySet<DateOnly>?` |
| same | `DisabledDaysOfWeek` | `string?` (`"sat sun"`) | `IReadOnlySet<DayOfWeek>?` |
| `WaDatePicker` | `FocusedDate` | `string?` | `DateOnly?` |
| `WaDatePicker` | `GoToDateAsync(date)` | `string` | `DateOnly` |
| `WaDatePickerFocusDayEventArgs`, `WaDatePickerViewChangeEventArgs` | `Date` | `string?` | `DateOnly?` (and `View` is `WaDatePickerView?`, section 11) |
| `WaTimeInput` | `Value` (`@bind-Value`) | `string?` | `TimeOnly?` |
| `WaTimeInput` | `Min`, `Max` | `string?` | `TimeOnly?` |
| `WaKnownDate` | `Value` (`@bind-Value`), `Min`, `Max` | `string?` | `DateOnly?` |
| `WaRelativeTime` | `Date` | `DateTime?` | `DateTimeOffset?` |
| `WaRelativeTime` | `DateString` | `string?` | removed: pass a `DateTimeOffset` (e.g. `DateTimeOffset.Parse(text)`) |
| `WaFormatDate` | `Date` | `string?` | `DateTimeOffset?` |

An empty set (`DisabledDates`, `DisabledDaysOfWeek`) renders no attribute, like null. A value the element reports that isn't in the wire form (never the case for what Web Awesome emits) keeps the model and adds a validation message ("The Birthday field must be a date.", "... must be a date range.", "... must be a time.") to the `EditContext`, like Blazor's `InputDate`; an empty element value binds `null`.

Single date:

```razor
@* before *@
<WaDateInput Label="Arrival" Min="2026-03-12" Max="2026-03-14" DisabledDaysOfWeek="sat sun" @bind-Value="arrival" />
@code { private string? arrival = "2026-03-12"; }

@* after *@
<WaDateInput Label="Arrival" Min="ConferenceStart" Max="ConferenceEnd" DisabledDaysOfWeek="Weekend" @bind-Value="arrival" />
@code {
    private static readonly DateOnly ConferenceStart = new(2026, 3, 12);
    private static readonly DateOnly ConferenceEnd = new(2026, 3, 14);
    private static readonly IReadOnlySet<DayOfWeek> Weekend = new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };
    private DateOnly? arrival = new(2026, 3, 12);
}
```

Range mode is a wrapper of its own, split by value type (the `Mode` parameter is gone):

```razor
@* before *@
<WaDateInput Label="Stay" Mode="WaDateSelectionMode.Range" MaxRange="14" @bind-Value="stay" />
<WaDatePicker Mode="WaDateSelectionMode.Range" @bind-Value="sprint" />
@code {
    private string? stay;                              // "2026-05-01/2026-05-07"
    private string? sprint = "2026-05-11/2026-05-18";
}

@* after *@
<WaDateRangeInput Label="Stay" MaxRange="14" @bind-Value="stay" />
<WaDateRangePicker @bind-Value="sprint" />
@code {
    private WaDateRange? stay;                         // stay?.From, stay?.To
    private WaDateRange? sprint = new(new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 18));
}
```

`WaDateRange` is a value type with structural equality: `From` and `To` are `DateOnly?`, `IsComplete`/`IsEmpty` tell how far the user got, `ToString()` gives the wire form (`2026-05-11/2026-05-18`) and `WaDateRange.TryParse`/`Parse` read it. While only the first date is picked, Web Awesome reports that one date, so the range binds half-filled: `From` set, `To` null (`WaDateRangeInput` reports it as the user types; `WaDateRangePicker` commits on the second click, and its `OnInput` carries the first). The element orders a complete range, so the earlier date always binds as `From`, and a range with only `To` set reads back with that date as `From` once the element reports it.

Time:

```razor
@* before *@
<WaTimeInput Min="09:00" Max="17:00" @bind-Value="meeting" />
@code { private string? meeting = "14:30"; }

@* after *@
<WaTimeInput Min="OfficeOpens" Max="OfficeCloses" @bind-Value="meeting" />
@code {
    private static readonly TimeOnly OfficeOpens = new(9, 0);
    private static readonly TimeOnly OfficeCloses = new(17, 0);
    private TimeOnly? meeting = new(14, 30);
}
```

The value is sent as `HH:mm`, or `HH:mm:ss` when the `Step` shows seconds (below 60, not a whole number of minutes, or `WaStep.Any`; `Step` is a `WaStep?`, section 12), which is the form the element emits itself; fractions of a second are not sent. A bound with seconds renders them (`06:30:45`).

Instants (`WaRelativeTime`, `WaFormatDate`):

```razor
@* before *@
<WaRelativeTime DateString="2025-12-02T00:00:00-05:00" />
<WaFormatDate Date="@deadline.ToString("o")" />

@* after *@
<WaRelativeTime Date="release" />
<WaFormatDate Date="deadline" />
@code {
    private readonly DateTimeOffset release = new(2025, 12, 2, 0, 0, 0, TimeSpan.FromHours(-5));
    private readonly DateTimeOffset deadline = DateTimeOffset.UtcNow.AddDays(3);
}
```

The instant renders with its offset (`2025-12-02T00:00:00.000-05:00`), so every browser reads the same instant, whatever its time zone; unset still means now. **Semantic change for `WaRelativeTime`:** code passing a `DateTime` still compiles through the implicit `DateTime` → `DateTimeOffset` conversion. A `DateTimeKind.Utc` value keeps its instant, but an unspecified one (what EF Core returns for a UTC column) now takes the **server's** offset instead of being read as browser-local time. Mark UTC values first: `DateTime.SpecifyKind(value, DateTimeKind.Utc)`, or keep them as `DateTimeOffset`.

Web Awesome builds dates as local JS dates and rejects the years 0-99 in bounds and disabled dates; use the years 100-9999 there.

### 11. Closed value sets are enums (BREAKING)

Parameters whose Web Awesome attribute takes a fixed set of values were strings, so any typo went to the element unchecked. They are now nullable enums (unset still renders nothing); a Razor string literal no longer compiles.

| Wrapper | Parameter | Old type | New type | Before → after |
|---|---|---|---|---|
| `WaInput`, `WaTextArea`, `WaCombobox` | `AutoCapitalize` | `string?` | `WaAutoCapitalize?` | `"sentences"` → `WaAutoCapitalize.Sentences` (also `Off`, `None`, `On`, `Words`, `Characters`) |
| `WaInput`, `WaTextArea`, `WaCombobox` | `AutoCorrect` | `string?` (`"on"`/`"off"`) | `bool?` | `"on"` → `true`, `"off"` → `false` (rendered exactly `on`/`off`) |
| `WaInput`, `WaTextArea`, `WaNumberInput`, `WaCombobox` | `EnterKeyHint` | `string?` | `WaEnterKeyHint?` | `"send"` → `WaEnterKeyHint.Send` |
| `WaInput`, `WaTextArea`, `WaCombobox` | `InputMode` | `string?` | `WaInputMode?` | `"numeric"` → `WaInputMode.Numeric` |
| `WaNumberInput` | `InputMode` | `string?` | `WaNumberInputMode?` | `"decimal"` → `WaNumberInputMode.Decimal` |
| `WaButton`, `WaBreadcrumbItem`, `WaDropdownItem` | `Target` | `string?` | `WaLinkTarget?` | `"_blank"` → `WaLinkTarget.Blank` (also `Parent`, `Self`, `Top`) |
| `WaButton` | `FormMethod` | `string?` | `WaFormMethod?` | `"post"` → `WaFormMethod.Post` |
| `WaButton` | `FormEncType` | `string?` | `WaFormEncType?` | `"multipart/form-data"` → `WaFormEncType.MultipartFormData` (also `UrlEncoded`, `TextPlain`) |
| `WaPopup` | `FlipFallbackStrategy` | `string?` | `WaFlipFallbackStrategy?` | `"initial"` → `WaFlipFallbackStrategy.Initial` |
| `WaPopup` | `Boundary` | `string?` | `WaPopupBoundary?` | `"scroll"` → `WaPopupBoundary.Scroll` |
| `WaAccordion` | `HeadingLevel` | `string?` | `WaHeadingLevel?` | `"2"` → `WaHeadingLevel.H2`, `"none"` → `WaHeadingLevel.None` |
| `WaZoomableFrame` | `ReferrerPolicy` | `string?` | `WaReferrerPolicy?` | `"no-referrer"` → `WaReferrerPolicy.NoReferrer` |
| `WaZoomableFrame` | `Sandbox` | `string?` | `WaIframeSandbox?` (`[Flags]`) | `"allow-scripts allow-forms"` → `WaIframeSandbox.AllowScripts \| WaIframeSandbox.AllowForms`; `""` → `WaIframeSandbox.None` |
| `WaDataGridColumn` (model) | `Align`, `HeaderAlign` | `string?` | `WaDataGridAlign?` | `Align = "end"` → `Align = WaDataGridAlign.End` |
| `WaDataGridColumn` | `SortFn` | `string?` | `WaDataGridSortFn?` | `"alphanumericCaseSensitive"` → `WaDataGridSortFn.AlphanumericCaseSensitive` |
| `WaDataGridColumn` | `SortUndefined` | `string?` (`first`/`last`) | `WaDataGridSortUndefined?` | `"first"` → `First`, `"last"` → `Last`; new `Lower`/`Higher` send Web Awesome's `-1`/`1` |
| `WaDataGridColumn` | `FilterType` | `string?` | `WaDataGridFilterType?` | `"number-range"` → `WaDataGridFilterType.NumberRange`, `"equals"` → `ExactMatch` |
| `WaDataGridColumn` | `Pinned` | `string?` | `WaDataGridPinSide?` | `"left"` → `WaDataGridPinSide.Left` |
| `WaDataGridColumn` | `Aggregation` | `string?` | `WaDataGridAggregation?` | `"sum"` → `WaDataGridAggregation.Sum` |
| `WaDatePickerViewChangeEventArgs` | `View` | `string?` | `WaDatePickerView?` | `e.View == "months"` → `e.View == WaDatePickerView.Months` |
| `WaRatingHoverEventArgs` | `Phase` | `string` | `WaRatingHoverPhase` | `e.Phase == "end"` → `e.Phase == WaRatingHoverPhase.End` |
| `WaDataGridColumnPinEventArgs` | `Side` | `string?` | `WaDataGridPinSide?` | `e.Side == "left"` → `e.Side == WaDataGridPinSide.Left` |

```razor
@* before *@
<WaTextArea AutoCorrect="off" AutoCapitalize="sentences" EnterKeyHint="send" InputMode="text" />
<WaButton Href="https://example.com" Target="_blank">Site</WaButton>
<WaZoomableFrame Src="/preview" Sandbox="allow-scripts" ReferrerPolicy="no-referrer" />

@* after *@
<WaTextArea AutoCorrect="false" AutoCapitalize="WaAutoCapitalize.Sentences" EnterKeyHint="WaEnterKeyHint.Send" InputMode="WaInputMode.Text" />
<WaButton Href="https://example.com" Target="WaLinkTarget.Blank">Site</WaButton>
<WaZoomableFrame Src="/preview" Sandbox="WaIframeSandbox.AllowScripts" ReferrerPolicy="WaReferrerPolicy.NoReferrer" />
```

`WaButton.FormTarget` stays a string: its union ends in `| string` (named browsing contexts). The event-args enums deserialize from exactly Web Awesome's strings; an unknown value fails the event instead of becoming a default member.

### 12. `Step` is `WaStep` (BREAKING)

`step` takes a number or `any`. `WaInput.Step` (`decimal?`) could not send `any`, and `WaNumberInput.Step`/`WaTimeInput.Step` (`string?`) took anything. All three are now `WaStep?`: a positive number, or `WaStep.Any`. Numbers convert implicitly, so numeric Razor values keep compiling (`0.5` now works without the `m` suffix); `"any"` becomes `WaStep.Any`, and zero or a negative step throws `ArgumentOutOfRangeException`.

```razor
@* before *@
<WaNumberInput Step="any" @bind-Value="amount" />
<WaInput Type="WaInputType.Number" Step="0.5m" @bind-Value="text" />

@* after *@
<WaNumberInput Step="WaStep.Any" @bind-Value="amount" />
<WaInput Type="WaInputType.Number" Step="0.5" @bind-Value="text" />
<WaTimeInput Step="1" @bind-Value="time" />   @* seconds, as before *@
```

`wa-slider`'s `step` is typed `number` only (no `any`), so `WaSlider.Step`/`WaRange.Step` stay `decimal`.

### 13. Lists are collections, event payloads are records (BREAKING)

List attributes took one string in the element's own list syntax; they now take typed collections, which the wrapper joins exactly as the element splits them. A null or empty collection renders nothing; an item the element would split in two (a swatch with `;`, a group-by id with a comma or space, an attribute name with a space) or a non-finite number throws.

| Wrapper | Parameter | Old type | New type | Rendered as |
|---|---|---|---|---|
| `WaIntersectionObserver` | `Threshold` | `string?` (`"0 0.5 1"`) | `IReadOnlyList<double>?` | invariant numbers, space-separated |
| `WaSparkline` | `Data` | `string?` (`"10 20 40"`) | `IReadOnlyList<double>?` | invariant numbers, space-separated |
| `WaZoomableFrame` | `ZoomLevels` | `string?` (`"25% 50% 100%"`) | `IReadOnlyList<double>?`, as factors (`0.25`, `0.5`, `1`) | invariant numbers, space-separated |
| `WaPopup` | `FlipFallbackPlacements` | `string?` (`"bottom top"`) | `IReadOnlyList<WaPlacement>?` | the placements in order, space-separated |
| `WaDataGrid` | `GroupBy` | `string?` (`"region, country"`) | `IReadOnlyList<string>?` (outermost first) | space-separated |
| `WaColorPicker` | `Swatches` | `string?` (`"#f00; #0f0"`) | `IReadOnlyList<string>?` | `;`-separated (a swatch may contain commas) |
| `WaFileInput` | `Accept` | `string?` (`"image/*,.pdf"`) | `IReadOnlyList<string>?` | `,`-separated |
| `WaMutationObserver` | `AttributeFilter` | `string?` (`"class id"`) | `IReadOnlySet<string>?` | ordinal order, space-separated; empty falls back to `Attr` |
| `WaColorPicker` | `SetSwatchesAsync(colors)` | `string[]` | `IEnumerable<string>` | (source-compatible) |
| `WaSelect`, `WaCombobox` | `SelectedValues` / `SelectedValuesChanged` | `string[]?` / `EventCallback<string[]?>` | `IReadOnlyList<string>?` / `EventCallback<IReadOnlyList<string>?>` | (bound selection) |
| `WaColorPicker` | `Value` (`@bind-Value`) | `string` | `string?` | (the only non-nullable text value) |
| `WaAnimation` | `Iterations` | `decimal?` (`decimal.MaxValue` for Infinity) | `double?` (`double.PositiveInfinity` renders `Infinity`) | invariant number |

```razor
@* before *@
<WaSparkline Data="10 25 15 40" />
<WaIntersectionObserver Threshold="0 0.5 1" />
<WaColorPicker Swatches="#d0021b; #f5a623" @bind-Value="color" />
<WaFileInput Accept="image/png,image/jpeg" />
<WaMutationObserver AttributeFilter="class id" />
<WaZoomableFrame ZoomLevels="25% 50% 100%" />
<WaSelect Multiple="true" @bind-SelectedValues="toppings" />
@code { private string[]? toppings; private string color = "#d0021b"; }

@* after *@
<WaSparkline Data="@(new double[] { 10, 25, 15, 40 })" />
<WaIntersectionObserver Threshold="@(new double[] { 0, 0.5, 1 })" />
<WaColorPicker Swatches="@(new[] { "#d0021b", "#f5a623" })" @bind-Value="color" />
<WaFileInput Accept="@(new[] { "image/png", "image/jpeg" })" />
<WaMutationObserver AttributeFilter="@(new HashSet<string> { "class", "id" })" />
<WaZoomableFrame ZoomLevels="@(new double[] { 0.25, 0.5, 1 })" />
<WaSelect Multiple="true" @bind-SelectedValues="toppings" />
@code { private IReadOnlyList<string>? toppings; private string? color = "#d0021b"; }
```

Event payloads that were `object[]` of raw `JsonElement`s are records now:

| Event args | Property | Old type | New type |
|---|---|---|---|
| `MutationEventArgs` | `MutationRecords` | `object[]?` | `IReadOnlyList<WaMutationRecord>?` (`Type` as `WaMutationType`, `AttributeName`, `OldValue`) |
| `ResizeEventArgs` | `ResizeObserverEntries` | `object[]?` | `IReadOnlyList<WaResizeEntry>?` (`ContentRect` as `WaRect`: `X`, `Y`, `Width`, `Height`, `Top`, `Right`, `Bottom`, `Left`) |
| `WaTreeSelectionChangeEventArgs` | `Selection` | `object[]?` | `IReadOnlyList<WaElementInfo>?` (`Id`, `TextContent`) |
| `WaContentChangeEventArgs` | `Items` | `object[]?` | `IReadOnlyList<WaElementInfo>?` |

```csharp
// before
if (args.ResizeObserverEntries is [JsonElement entry, ..] && entry.TryGetProperty("contentRect", out var rect))
    width = rect.GetProperty("width").GetDouble();
var count = args.MutationRecords?.Length ?? 0;

// after
if (args.ResizeObserverEntries is [{ ContentRect: { } rect }, ..])
    width = rect.Width;
var count = args.MutationRecords?.Count ?? 0;
```

### 14. `WaInput.Min` and `Max` take typed accessors (BREAKING)

`wa-input`'s `min`/`max` are a number for `Type="WaInputType.Number"` and an ISO date, time or local date-time for the date and time types. `Min`/`Max` were `decimal?`, which could not express the date and time bounds. `Min`/`Max` are now the raw `string?` attribute text, joined by one typed accessor per value type, each converted culture-free into the form the native input reads:

| Accessor (and its `Max` twin) | Type | Rendered as |
|---|---|---|
| `Min` | `string?` | as given |
| `MinDecimal` | `decimal?` | invariant (`2.5`) |
| `MinLong`, `MinULong` | `long?`, `ulong?` | invariant integer |
| `MinDate` | `DateOnly?` | `yyyy-MM-dd` |
| `MinTime` | `TimeOnly?` | `HH:mm`, or `HH:mm:ss` with seconds |
| `MinDateTime` | `DateTime?` | `yyyy-MM-ddTHH:mm`, with `:ss` and `.fff` when not zero; the kind is ignored |

Set at most one accessor per bound; two throw `InvalidOperationException` naming them.

```razor
@* before *@
<WaInput Type="WaInputType.Number" Min="0" Max="10.5m" @bind-Value="quantity" />

@* after *@
<WaInput Type="WaInputType.Number" MinLong="0" MaxDecimal="10.5m" @bind-Value="quantity" />
<WaInput Type="WaInputType.Date" MinDate="Opening" MaxDate="Closing" @bind-Value="day" />
<WaInput Type="WaInputType.Number" Min="0" Max="10.5" @bind-Value="quantity" />   @* raw text also works *@
```

A number literal on `Min`/`Max` no longer compiles in C# (it is a string now); in Razor, `Min="0"` still compiles and renders `0`, but `Min="@count"` with a numeric variable does not: use the typed accessor. On a future net11.0 target this pair becomes a C# 15 union type (a planned breaking change there).

### 15. Default values render nothing, and a rendered attribute stays (visible in the markup)

Following the owner rule that an unset parameter emits nothing:

- A non-nullable parameter holding Web Awesome's default no longer renders its attribute: `WaSlider` renders no `min`/`max`/`step` by default, `WaInput` no `type="text"`, `WaTooltip` no `placement="top"`, `WaCopyButton` no `feedback-duration="1000"` or `tooltip="full"`, and so on. The element's own default applies, so nothing changes in the browser. Each such default is a public constant on the wrapper, e.g. `WaSlider.DefaultMax`, `WaInput.DefaultType`, `WaCopyButton.DefaultFeedbackDuration`; use it instead of a copied literal.
- Once a wrapper has rendered an attribute, it never removes it: returning a parameter to its default (or a nullable one to null) renders Web Awesome's default explicitly. Removing the attribute made Lit set the element property to null instead of back to its default: a `WaSlider` whose `Max` went from 50 back to 100 got `max = null` (0 in its arithmetic), a `WaTooltip` whose `Distance` went from 20 back to unset got `distance = null`. Boolean attributes are still removed (a removed boolean reads as false, its default), and so are attributes without a Web Awesome default (removal restores the unset state).

Only CSS selectors or tests that match a default attribute in the rendered markup (for example `wa-slider[max="100"]` on first render) need a change.

## Behavioral Changes (non-breaking, but visible)

- **Numbers render in the invariant culture.** Blazor formats a number passed to an attribute with the current culture, so under a culture such as cs-CZ `Distance="0.5"` rendered `distance="0,5"` (and negative numbers could get a U+2212 minus), which Web Awesome can't parse. Every number attribute (`WaPopup.Distance`, `WaAnimation.PlaybackRate`, `WaSlider.Step`, `WaNumberInput`'s value, and about 40 more) now uses the invariant culture; the dates and times use their explicit wire formats (section 10).

- **C# value changes reach the element after the user has edited it.** In Web Awesome 3 the `value`/`checked` attribute sets only the default, and the element ignores it once the user has interacted. The form controls now also assign the live property after a C#-side change. This makes reset-after-submit, normalizing setters, and "clear"/"select all" buttons work. Affected: `WaInput`, `WaTextArea`, `WaNumberInput`, `WaColorPicker`, `WaDateInput`, `WaKnownDate`, `WaOtpInput`, `WaRadioGroup`, `WaTimeInput`, `WaSlider`, `WaRange`, `WaCheckbox` and `WaSwitch`.
- **`WaSlider`, `WaRange` and `WaRating` user edits now reach `@bind-Value`.** These elements report their value as a number, which Blazor's built-in change event can't carry. The server rejected every change event, so the bound model never updated. The wrappers now listen on a string-valued alias of the change event.
- **`SetRangeTextAsync` updates the bound value** on `WaInput` and `WaTextArea` (Web Awesome dispatches no event for it). `EditContext` is notified.
- **Range-mode `WaSlider` no longer requires `@bind-Value`.** Bind `MinValue`/`MaxValue` only.
- **`OnFocus`/`OnBlur` now fire on the form controls, `WaButton` and `WaFileInput`.** They never did where the focus lands inside the control's shadow root (every text control, toggle and picker, `WaRadioGroup`, `WaButton`, `WaFileInput`). They now listen to `focusin`/`focusout`, so `FocusEventArgs.Type` is `"focusin"`/`"focusout"` instead of `"focus"`/`"blur"`. Moving the focus inside one control (OTP segments, the two thumbs of a range `WaSlider`/`WaRange`) raises nothing; moving it between a control and its slotted content (from one radio of a `WaRadioGroup` to the next, or into a `WaSelect`'s option list from the keyboard) raises `OnBlur` followed by `OnFocus`, although the focus stays within the control. The callbacks' docs state both cases. `WaRadio`, `WaTab` and `WaDropdownItem` are unchanged.
- **`WaColorPicker.OnShow`/`OnHide`/`OnAfterShow`/`OnAfterHide`, `WaIntersectionObserver.OnIntersect` and `WaCombobox.OnKeyDown` now fire**, and `WaSelect.OnKeyDown` (and `WaColorPicker.OnKeyDown` for Escape while open) fire for keys pressed in the control itself. Web Awesome dispatches these events where Blazor doesn't listen (non-bubbling, or with the keydown's propagation stopped); the library's JS initializer relays them under private event names, so other listeners on the page see no duplicate events.

## New Features

### Immediate binding — `Immediate` on `WaInput`, `WaTextArea`, `WaNumberInput`

By default the bound value updates when the control commits (`change`, on blur). With `Immediate="true"` it updates on every keystroke. Your `OnInput` handler still runs, after the value has been updated. So a `OnKeyDown` handler such as Ctrl+Enter "send" sees the current text:

```razor
<WaTextArea @bind-Value="message" Immediate="true" OnKeyDown="HandleKeyDown" />
```

### WaDropdownItem link items — `Href`, `Target`, `Rel`, `Download`

```razor
<WaDropdownItem Href="/docs">Documentation</WaDropdownItem>
<WaDropdownItem Href="https://github.com/boldbrick/WebAwesome-Blazor" Target="WaLinkTarget.Blank" Rel="noopener noreferrer">GitHub</WaDropdownItem>
<WaDropdownItem Href="/files/report.pdf" Download="report.pdf">Download report</WaDropdownItem>
```

The item stays a menu item for assistive technology, so the label should describe where the link goes. `Href` is ignored on items with a submenu. `WaDropdownItem.OnBlur`/`OnFocus` now actually fire; they were previously bound under the wrong attribute names.

### WaPage sticky sections — `DisableSticky`

The page's banner, header, subheader, menu and aside stick while the page scrolls. The new `DisableSticky` (`WaPageSections?`, a `[Flags]` enum) turns that off per section, rendering Web Awesome's CSS-only `disable-sticky` attribute:

```razor
<WaPage DisableSticky="WaPageSections.Header | WaPageSections.Aside"> ... </WaPage>
```

### A parameter for every slot

Every slot Web Awesome declares now has a `RenderFragment` parameter. New:

| Wrapper | New parameter | Slot |
|---|---|---|
| `WaAnimatedImage` | `PlayIconContent`, `PauseIconContent` | `play-icon`, `pause-icon` |
| `WaBreadcrumbItem` | `SeparatorContent` | `separator` (this item only) |
| `WaCard` | `ActionsContent` | `actions` (horizontal card) |
| `WaCarousel` | `NextIconContent`, `PreviousIconContent` | `next-icon`, `previous-icon` |
| `WaDialog`, `WaDrawer` | `LabelContent` | `label` (rich label; takes precedence over `Label`) |
| `WaInput` | `ClearIconContent`, `ShowPasswordIconContent`, `HidePasswordIconContent` | `clear-icon`, `show-password-icon`, `hide-password-icon` |
| `WaSelect` | `ClearIconContent`, `ExpandIconContent` | `clear-icon`, `expand-icon` |
| `WaSlider` | `ReferenceContent` | `reference` (replaces `ChildContent`, see section 7) |
| `WaTree` | `ExpandIconContent`, `CollapseIconContent` | `expand-icon`, `collapse-icon` |
| `WaZoomableFrame` | `ZoomInIconContent`, `ZoomOutIconContent` | `zoom-in-icon`, `zoom-out-icon` |

Where the wrapper already had an icon-name shortcut for the slot (`PlayIconName`, `NextIconName`, `ExpandIconName`, `ZoomInIconName`, ...), the fragment wins when both are set. The per-day `day-YYYY-MM-DD` slots of the date controls, whose names are dates, take the new `WaDayContent` (next section).

`WaSlider.ReferenceContent` and `WaRange.ReferenceContent` now spread the labels along the track: each element of the fragment is a label of its own, as with plain Web Awesome markup. Before, `WaRange` wrapped them all into a single label at the start of the track.

### Per-day content for the date controls — `WaDayContent`

Place a `WaDayContent` for a date in the `ChildContent` of `WaDatePicker`, `WaDateRangePicker`, `WaDateInput` or `WaDateRangeInput` to replace that day's number in the calendar (the cell stays a clickable day). Content added or removed later shows up too, also in the date inputs' popup calendar. A `WaDayContent` anywhere else throws `InvalidOperationException`.

```razor
<WaDatePicker @bind-Value="day">
    <WaDayContent Date="new DateOnly(2026, 12, 25)"><strong>Xmas</strong></WaDayContent>
</WaDatePicker>
```

## Packaging

The package's license file is now resolved relative to `Directory.Build.props`. Building the repository as a git submodule under a consuming solution (with its own `SolutionDir`) no longer fails with NU5019/NU5030.

## Migration Checklist

- [ ] Replace `WaFormat` on `WaRelativeTime` (see section 1)
- [ ] Update C# code that reads `WaTextArea.Rows` as `int`
- [ ] Rebuild and fix every compile error against the enum table in section 3. Each one was a value Web Awesome didn't accept, or a type that now matches the component
- [ ] Remove handlers for the callbacks listed in section 4
- [ ] Remove the parameters listed in section 5 (`WaRadio.Checked`, `WaTab.Closable`, the `WaMutationObserver` and `WaPopup` boundary parameters)
- [ ] Remove the form control parameters listed in section 6, in C# and in Razor markup (markup keeps compiling)
- [ ] Move the slot content listed in section 7 (`MarkupLabel` on `WaCheckbox`/`WaSwitch`/`WaRating`, `WaRating.MarkupHint`, `WaComparison`/`WaSlider` child content) to the parameter the table names
- [ ] Check the parameter defaults listed in section 8 (`WaAnimation` `Fill`/`Iterations`, `WaAnimatedImage.Play`, `WaQrCode.ErrorCorrection`, `WaCopyButton` labels, `WaPopup.FlipFallbackStrategy`) and set the old value where you relied on it
- [ ] Rename `WaFileInput.LabelContent`/`HintContent` to `MarkupLabel`/`MarkupHint` (see section 9)
- [ ] Retype the date and time models, bounds and disabled days (`DateOnly?`, `TimeOnly?`, `DateTimeOffset?`, `IReadOnlySet<DateOnly>`, `IReadOnlySet<DayOfWeek>`), replace `Mode="WaDateSelectionMode.Range"` with `WaDateRangeInput`/`WaDateRangePicker` bound to a `WaDateRange?`, and replace `WaRelativeTime.DateString` (see section 10)
- [ ] Check every `DateTime` you pass to `WaRelativeTime.Date` or `WaFormatDate.Date`: an unspecified `Kind` now means server-local time (see section 10)
- [ ] Replace the string values of the parameters in section 11 with their enum members (`Target="WaLinkTarget.Blank"`, `AutoCorrect="false"`, `EnterKeyHint="WaEnterKeyHint.Send"`, the data grid column model, the event-args comparisons)
- [ ] Replace `Step="any"` with `Step="WaStep.Any"` (section 12)
- [ ] Pass collections to the list parameters, retype `SelectedValues` fields to `IReadOnlyList<string>?`, `WaColorPicker` values to `string?` and `WaAnimation.Iterations` to `double?`, and read the typed event payloads (section 13)
- [ ] Move numeric and date bounds of `WaInput` to the typed accessors (`MinDecimal`, `MinDate`, ...), one per bound (section 14)
- [ ] Update CSS selectors or tests that match a default attribute (`max="100"`, `type="text"`, `placement="top"`) on first render (section 15)
- [ ] If you compare `FocusEventArgs.Type` in an `OnFocus`/`OnBlur` handler of a form control, `WaButton` or `WaFileInput`, expect `"focusin"`/`"focusout"`
- [ ] If you worked around the value-sync bug (forcing a re-render with `@key`, JS interop to set `.value`), remove the workaround
- [ ] Update CSS selectors or tests that match `size="small|medium|large"` to `s|m|l`
- [ ] (Optional) Adopt `Immediate` where the model must be current during typing
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.12.0+
- **Breaking Changes**: Yes (sections 1–14; section 15 changes the rendered markup only)
- **New Dependencies**: None
