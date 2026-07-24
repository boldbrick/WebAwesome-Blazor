# Web Awesome Blazor 3.8.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.7.0 to 3.8.0. The upgrade is **additive** — no wrapper APIs were removed or renamed. The only behavioral change to be aware of is the realigned `WaDrawer` light-dismiss default; the rest is new components and new attributes.

## Behavioral Changes

### WaDrawer — `light-dismiss` now defaults to *off*

**Impact**: Low, but observable — affects drawers that relied on the previous "click outside to close" default without setting `LightDismiss`.

Web Awesome 3.8.0 changed the upstream default of the drawer's `light-dismiss` attribute from `true` to `false`. The Blazor `WaDrawer.LightDismiss` parameter has always defaulted to `false` and only emits the attribute when set to `true`, so previously the upstream `true` default applied whenever you left `LightDismiss` unset — drawers closed on an outside click. As of 3.8.0 the upstream default matches the wrapper's `false`, so an unset drawer no longer light-dismisses.

This is not an API change (no code will fail to compile), but if you relied on the implicit outside-click-to-close behavior, set it explicitly:

```razor
<!-- 3.7.0: closed on outside click by default -->
<WaDrawer Label="Settings">…</WaDrawer>

<!-- 3.8.0: opt in explicitly to keep that behavior -->
<WaDrawer Label="Settings" LightDismiss="true">…</WaDrawer>
```

### WaVideo — `timeupdate` event type annotation (no action)

The `timeupdate` event's Custom Elements Manifest type was annotated (`null` → `Event`) upstream. The event name is unchanged and remains bound as `OnTimeUpdate`; no migration is required.

## New Components

Six new experimental components arrive in 3.8.0. All are additive — no existing usage changes.

### Accordion — `WaAccordion` / `WaAccordionItem`

```razor
<WaAccordion Mode="WaAccordionMode.Single">
    <WaAccordionItem Label="First" Expanded="true">First panel body.</WaAccordionItem>
    <WaAccordionItem Label="Second">Second panel body.</WaAccordionItem>
</WaAccordion>
```

`WaAccordion` exposes `Appearance`, `HeadingLevel`, `IconPlacement`, `Mode` (`Single` / `SingleCollapsible` / `Multiple`), the `OnExpand`/`OnCollapse`/`OnAfterExpand`/`OnAfterCollapse` callbacks, and the `ExpandAllAsync()`/`CollapseAllAsync()` methods. `WaAccordionItem` exposes `Label`/`Expanded`/`Disabled`, the `label` and `icon` slots, and `ExpandAsync()`/`CollapseAsync()`/`ToggleAsync()`/`FocusAsync()`.

### Date & time form controls — `WaDateInput` (Pro), `WaTimeInput`, `WaKnownDate`

These are form-associated controls; bind them with `@bind-Value` inside an `EditForm`, exactly like `WaInput`/`WaSelect`.

```razor
<EditForm Model="model">
    <WaDateInput @bind-Value="model.When" Label="When" Mode="WaDateSelectionMode.Single" WithClear="true" />
    <WaTimeInput @bind-Value="model.At" Label="At" HourFormat="WaTimeHourFormat.TwentyFour" WithNow="true" />
    <WaKnownDate @bind-Value="model.Birthday" Label="Birthday" />
</EditForm>
```

`WaDateInput` (Pro) and `WaTimeInput` are popup controls sharing the `Show*`/`Hide*`/`Clear*` methods and the `OnShow`/`OnHide`/`OnAfterShow`/`OnAfterHide`/`OnClear`/`OnInvalid` events. `WaKnownDate` is a simple day/month/year field group.

### Standalone calendar — `WaDatePicker`

`WaDatePicker` is the inline calendar behind `WaDateInput`, usable on its own. It is **not** a form-associated control — bind its value with `@bind-Value` directly:

```razor
<WaDatePicker @bind-Value="selectedDate"
              Mode="WaDateSelectionMode.Range"
              WeekdayFormat="WaWeekdayFormat.Short"
              OnViewChange="HandleViewChange" />
```

It exposes `Mode`/`View`/`Min`/`Max`/`FirstDayOfWeek`/`FocusedDate`/`Locale`/`Months`/`PageBy`/`Size`/`WeekdayFormat`, the typed `OnFocusDay` (`WaDatePickerFocusDayEventArgs`) and `OnViewChange` (`WaDatePickerViewChangeEventArgs`) callbacks (each carrying an ISO date string), and the `ClearAsync()`/`FocusAsync()`/`GoToDateAsync()`/`GoToTodayAsync()` methods.

## New Attributes on Existing Components

- **`WaCard`** — `WithHeaderActions` / `WithFooterActions` (SSR hydration hints; set automatically when the corresponding actions content is provided).
- **`WaFileInput`** — `Capture` (`WaCaptureMode.User` / `WaCaptureMode.Environment`) to pick the mobile camera/microphone.
- **`WaQrCode`** — `Image`, `ImageBackground`, `ImageCoverage` (0–1), `ImagePadding` (px) to render a centered logo inside the QR code.

## Migration Checklist

- [ ] Review `WaDrawer` usages that relied on the implicit light-dismiss default; add `LightDismiss="true"` where outside-click-to-close is desired
- [ ] (Optional) Adopt the new date/time controls for date entry
- [ ] (Optional) Use `WaQrCode.Image` for branded QR codes; `WaFileInput.Capture` for mobile capture
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.8.0+
- **Breaking Changes**: None (one behavioral realignment: `WaDrawer` light-dismiss default)
- **New Dependencies**: None
