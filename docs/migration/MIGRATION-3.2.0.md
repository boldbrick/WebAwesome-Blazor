# Web Awesome Blazor 3.2.0 Migration Guide

Migration from `3.1.0` to `3.2.0`, mirroring the Web Awesome 3.2.0 release. A small, mostly additive upgrade: three new component wrappers, three new `WaIcon` attributes, and one behavioral breaking change to `WaQrCode`'s default colors.

## Breaking Changes

### 1. `WaQrCode.Fill` / `WaQrCode.Background` no longer default to black/white (BREAKING)

#### Changes
- Web Awesome 3.2.0 changed the default of `wa-qr-code`'s `fill` and `background` attributes from `'black'`/`'white'` to `''` (empty), so an unset QR code inherits its colors from the current theme instead of being forced to black-on-white.
- To match this, `WaQrCode.Fill` and `WaQrCode.Background` changed from non-nullable `string` (defaulting to `"black"`/`"white"` and always emitted) to nullable `string?` (defaulting to `null` and emitted only when you set them).

#### Migration
- If you relied on the implicit black/white rendering, set the colors explicitly: `<WaQrCode Fill="black" Background="white" ... />`.
- If you already set `Fill`/`Background`, no change is needed.
- Code that read `qrCode.Fill`/`qrCode.Background` as non-null `string` must now treat them as `string?`.

## Not a Breaking Change: `wa-radio` / `wa-radio-group` `size` default

Web Awesome 3.2.0 changed the `size` default of `wa-radio` and `wa-radio-group` from `'medium'` to unset (a radio now inherits its size from the surrounding group; a group applies its size to its radios). The Blazor wrappers already exposed `Size` as a nullable `WaSize?` with no forced default and only emitted the attribute when set, so they already matched the new behavior — no wrapper change and nothing to migrate.

## New Features

- **`WaSparkline`** (new in WA 3.2.0, experimental upstream): a compact inline trend chart. Parameters `Appearance` (`WaSparklineAppearance`: Gradient/Line/Solid), `Curve` (`WaSparklineCurve`: Linear/Natural/Step), `Data` (space-separated numbers, e.g. `"10 20 40 25 35"`), `Label` (accessible label), `Trend` (`WaSparklineTrend`: Positive/Negative/Neutral). Display-only — no events, slots, or methods.
- **`WaNumberInput`** (new in WA 3.2.0, experimental upstream): a numeric input form control (`WaInputBase<decimal?>`, full `@bind-Value` and `EditForm` integration). Parameters include `Appearance`, `Min`/`Max`/`Step`, `Pill`, `Placeholder`, `AutoFocus`, `InputMode`, `EnterKeyHint`, `WithoutSteppers`, `WithHint`/`WithLabel`; start/end/label/hint/increment-icon/decrement-icon slots; `FocusAsync`/`BlurAsync`/`SelectAsync`/`StepUpAsync`/`StepDownAsync` methods.
- **`WaFileInput`** (new in WA 3.2.0, experimental upstream): a file selection form control with a click/drag-and-drop dropzone. Modeled as a `ComponentBase` form control (it exposes no bindable scalar value — selected files are runtime `File` objects), with `Accept`, `Multiple`, `Required`, `Size`, `Label`/`Hint` (+ label/hint/dropzone/file-icon slots), `OnChange`/`OnInput`/`OnFocus`/`OnBlur`/`OnInvalid` events, `FocusAsync`/`BlurAsync`, and `SetCustomValidityAsync`. Read the selected files through the change/input events plus JS interop or your own upload handler.
- **`WaIcon`** gains three attributes from WA 3.2.0: `Animation` (`WaIconAnimation`: Beat, Fade, BeatFade, Bounce, Flip, Shake, Spin, SpinPulse, SpinReverse), `Flip` (`WaFlip`: X, Y, Both), and `Rotate` (degrees, `int`).

## Migration Checklist
- [ ] Update the package reference to `WebAwesome.Blazor` 3.2.0 and your Web Awesome assets to 3.2.0 (CDN URL or self-hosted).
- [ ] If you depend on black/white QR codes, set `Fill`/`Background` explicitly (section 1).
- [ ] Update any code that treated `WaQrCode.Fill`/`Background` as non-nullable `string`.
- [ ] Run your test suite.

## Automated Migration

### Find/Replace Patterns
| Find | Replace | Scope |
|---|---|---|
| `<WaQrCode ` (relying on default colors) | `<WaQrCode Fill="black" Background="white" ` | `WaQrCode` usages needing the old look |

## Support
File issues at <https://github.com/boldbrick/WebAwesome-Blazor/issues> with the WA version, bindings version, and a minimal repro.

## Version Compatibility
| Bindings | Web Awesome | .NET |
|---|---|---|
| 3.2.0 | 3.2.0 | net10.0 (primary), net9.0 |
