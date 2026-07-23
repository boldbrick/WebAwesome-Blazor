# Web Awesome 3.1.0 → 3.2.0 Upgrade Implementation Plan

Ticket: WAB-22 (epic WAB-21 "Web Awesome 3.2"). Branch: `/main/WA-3.2/WAB-22` (new train, off `/main` cs:122).
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.2.0

## Analysis summary

Authoritative worklist: `temp\wa-api\changes_3.1.0_to_3.2.0.json` (3 added, 0 removed, 5 modified, 4 breaking).
Component count 59 → 62. All three new components are `pro: false`, `status: experimental`.

The change report's Markdown rendering for `wa-progress-ring` shows spurious ".NET array" entries
(`Length`, `Rank`, `SyncRoot`, …) — a display artifact of the report generator reflecting a C# array's
properties. The JSON is authoritative: the only real change is cssParts `indicator`/`track` added, which
do not affect the C# wrapper surface (parity tracks attributes/events/methods, not cssParts). No code change.

## Phase 1 — Breaking changes

The four "breaking" entries are all upstream attribute *default* changes (no removals or renames). Wrapper impact:

### 1. `wa-qr-code` — `fill` default `'black'` → `''`, `background` default `'white'` → `''` (BREAKING)
File: `src\WebAwesome.Blazor\Components\WaQrCode.cs`
- Current wrapper hardcodes `public string Fill { get; set; } = "black";` and `Background = "white"`, always emitted.
  Upstream now defaults both to empty (inherit theme colors). Forcing black/white diverges from upstream.
- Action: change `Fill` and `Background` to `string?` (default `null`) and emit via `AddAttributeIfNotNullOrEmpty`
  so the attribute is only set when the consumer explicitly provides a color.
- This is a public-API + behavioral change (non-null `string` → nullable, default removed): record in MIGRATION + CHANGELOG,
  update WaQrCode tests, and expect a `PublicApiSnapshotTests` diff.

### 2. `wa-radio` — `size` default `'medium'` → `null` (BREAKING, no code change)
File: `src\WebAwesome.Blazor\Components\WaRadio.cs`
- Wrapper already exposes `WaSize? Size` (nullable, no forced default, emitted via `AddAttributeIfNotNull`).
  The wrapper never forced `'medium'`, so it already matches the new upstream behavior. Doc-description drift only.

### 3. `wa-radio-group` — `size` default `'medium'` → `null` (BREAKING, no code change)
File: `src\WebAwesome.Blazor\Components\WaRadioGroup.cs` (inherits `WaSize? Size` from `WaInputBase`)
- Same as above: `Size` is nullable in the base with no forced default. No code change.

## Phase 2 — New components (via wa-wrapper-engineer)

All three are experimental, non-Pro. Group in one engineer batch (≤10).

### 4. `wa-sparkline` (WaSparkline) — display component
File (new): `src\WebAwesome.Blazor\Components\WaSparkline.cs`
- Model on `WaQrCode` (derive `ComponentBase`; Class/Style/AdditionalAttributes/Element; no form integration).
- Attributes: `appearance` (enum), `curve` (enum), `data` (string), `label` (string), `trend` (enum, nullable).
- No events, slots, or methods.
- New enums in `Enums.cs` + `ToHtmlValue`:
  - `WaSparklineAppearance` { Gradient, Line, Solid }
  - `WaSparklineCurve` { Linear, Natural, Step }
  - `WaSparklineTrend` { Positive, Negative, Neutral }

### 5. `wa-file-input` (WaFileInput) — form control (no scalar value)
File (new): `src\WebAwesome.Blazor\Components\WaFileInput.cs`
- The CEM exposes no bindable `value` attribute (files are runtime `File[]`), so do NOT derive from `WaInputBase<T>`.
  Model on `WaRadio`: `ComponentBase, IFormValidation` with `[Inject] WebAwesomeJSInterop`, `Element`, Class/Style/AdditionalAttributes.
- Attributes: `accept`, `hint`, `label`, `multiple`, `required`, `size` (WaSize), `with-hint`, `with-label`.
  (Optionally a `Name` parameter for form submission — not in CEM, harmless extra; keep only if trivial.)
- Slots: `dropzone`, `file-icon`, `hint`, `label` (RenderFragment content + label/hint string convenience).
- Events (callbacks): `OnChange` (onchange), `OnInput` (oninput), `OnFocus`/`OnBlur` (onfocus/onblur), `OnInvalid` (onwa-invalid).
- Methods: `FocusAsync()`/`BlurAsync()` → element `focus`/`blur` (native, allowlisted).
- Not `WaInputBase`-derived, so the EditForm coverage rule does not apply; integration test only.

### 6. `wa-number-input` (WaNumberInput) — numeric form control
File (new): `src\WebAwesome.Blazor\Components\WaNumberInput.cs`
- Model closely on `WaInput`; derive `WaInputBase<decimal?>` (idiomatic numeric value; `TryParseValueFromString`
  parses invariant decimal, empty → null).
- Component-specific attributes: `appearance` (WaInputAppearance), `autofocus`, `enterkeyhint`, `inputmode`,
  `max`, `min`, `pill`, `placeholder`, `step`, `without-steppers`, `with-hint`, `with-label`.
  Common (label/hint/size/required/readonly/autocomplete/disabled) come from `WaInputBase`; `value`→`Value`,
  `name`→`NameAttributeValue`, `title`→global ignore.
- Slots: `start`, `end`, `label`, `hint`, `increment-icon`, `decrement-icon`.
- Events: blur/focus (base), input (base OnInput), change (value binding), wa-invalid (OnInvalid).
- Methods: `FocusAsync`, `BlurAsync`, `SelectAsync`, `StepUpAsync`, `StepDownAsync` (all in CEM `methods`).

## Phase 3 — Modified components (additive, via wa-wrapper-engineer)

### 7. `wa-icon` (WaIcon) — 3 new attributes
File: `src\WebAwesome.Blazor\Components\WaIcon.cs`
- Add `Animation` (WaIconAnimation?, emit `animation` if not null), `Flip` (WaFlip?, emit `flip` if not null),
  `Rotate` (int, default 0, emit `rotate`).
- New enums in `Enums.cs` + `ToHtmlValue`:
  - `WaIconAnimation` { Beat, Fade, BeatFade("beat-fade"), Bounce, Flip("flip"), Shake, Spin, SpinPulse("spin-pulse"), SpinReverse("spin-reverse") }
  - `WaFlip` { X, Y, Both }

### 8. `wa-progress-ring` — cssParts only, no code change (see analysis note).

## Phase 4 — Intentional deviations (parity-config.json)

- `wa-number-input`: `attributeOverrides` { autofocus→AutoFocus, enterkeyhint→EnterKeyHint, inputmode→InputMode };
  `ignoredEvents` [ change ] (folded into `@bind-Value`). `value`/`name`/`title` covered by existing base/global rules.
- `wa-file-input`: no scalar value; if the CEM lists no deviating members after wrapper authoring, no entry is needed.
  If `change`/`input` end up bound as callbacks they are covered; add `ignoredAttributes`/`ignoreReasons` only for any
  CEM member deliberately not surfaced.
- Any new deviation gets a matching `ignoreReasons` entry.
- The parity tests (Phase 3 of the pipeline) enumerate the exact gaps; this list is finalized against their output.

## Phase 5 — Event delivery contract

New event bindings are `blur`/`focus`/`change`/`input`/`wa-invalid` on the two new form controls. All follow existing
precedent (`onblur`/`onfocus`/`onchange`/`oninput` + `onwa-invalid`, already registered in
`wwwroot\WebAwesome.Blazor.lib.module.js` for wa-input). No new `wa-*` event type is introduced, so no new JS-module
registration is expected. Verify `onwa-invalid` registration during implementation.

## Phase 6 — Element method audit

- New element methods invoked: `focus`, `blur` (native allowlist); `select`, `stepUp`, `stepDown` (WaNumberInput) — all
  present in the `wa-number-input` CEM `methods`. `wa-file-input` invokes only `focus`/`blur`. No new `extraElementMethods` needed.
- Re-verify existing `extraElementMethods` against 3.2.0 source (`temp\wa-src\3.2.0\dist\components\*`):
  `wa-mutation-observer`/`wa-resize-observer` `stopObserver`/`startObserver`, `wa-relative-time` `update`. Update the
  `ignoreReasons` verification date/note accordingly.

## Per-file action list

| File | Action |
|------|--------|
| `Components\WaQrCode.cs` | Fill/Background → nullable, emit-if-set (breaking default change) |
| `Components\WaIcon.cs` | add Animation/Flip/Rotate + attribute emission |
| `Components\Enums.cs` | add WaSparklineAppearance/Curve/Trend, WaIconAnimation, WaFlip + ToHtmlValue |
| `Components\WaSparkline.cs` | new display wrapper |
| `Components\WaFileInput.cs` | new form-control wrapper (ComponentBase + IFormValidation) |
| `Components\WaNumberInput.cs` | new numeric input wrapper (WaInputBase<decimal?>) |
| `ApiParity\expected-api-surface.json` | replace with surface_3.2.0.json |
| `ApiParity\parity-config.json` | targetWaVersion 3.2.0; wa-number-input deviations; refresh extraElementMethods notes |
| `Version.props` | 3.1.0 → 3.2.0 (Version, AssemblyVersion, FileVersion) |
| `README.md` | version references → 3.2.0 |
| `Demo\wwwroot\index.html` | CDN version → 3.2.0 |
| `Demo\wwwroot\data\api-surface.json` | replace with surface_3.2.0.json |
| Tests | integration tests for 3 new components; EditForm coverage for WaNumberInput; WaIcon/WaQrCode updates; breaking-change validation |
| `docs\MIGRATION-3.2.0.md` | new (qr-code fill/background default change) |
| `docs\CHANGELOG.md` | [3.2.0] entry |
| Demo pages | `New-WaDemoPages.ps1 -PruneRemoved`; add WaNumberInput to form showcase (mechanical) |
| `PublicApi\approved-public-api.txt` | promote after diff review |

## Validation checklist

- [ ] Breaking change (qr-code) implemented; radio/radio-group verified as no-op
- [ ] 3 new component wrappers build and pass parity
- [ ] wa-icon 3 attributes added
- [ ] parity-config deviations recorded with reasons; `enabled: true`, target 3.2.0
- [ ] extraElementMethods re-verified against 3.2.0 source
- [ ] `ApiSurfaceParityTests`, `EventBindingRegistrationTests`, `ElementMethodInvocationTests` green
- [ ] integration + EditForm + breaking-change tests green
- [ ] `PublicApiSnapshotTests` promoted (every diff explained by the report)
- [ ] Debug + Release build (incl. demo); full `dotnet test` green
- [ ] Playwright e2e sweep green
- [ ] MIGRATION-3.2.0.md + CHANGELOG [3.2.0]

## Risks

- **Low**: additive component set; the single behavioral breaking change (qr-code colors) is well-contained.
- **Medium**: WaNumberInput value typing (`decimal?`) and EditForm binding — mirror WaInput carefully; ensure culture-invariant parse.
- **Low**: WaFileInput modeled without InputBase (no scalar value) — confirm form participation via `IFormValidation` + native name.

## Follow-up (not part of this upgrade)
- Curated showcase pages: new WaSparkline / WaFileInput demos beyond generated skeletons are deliberate follow-up.
