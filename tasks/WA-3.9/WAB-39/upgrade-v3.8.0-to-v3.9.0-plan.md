# Web Awesome 3.8.0 to 3.9.0 Upgrade Implementation Plan

Ticket: WAB-39 (epic WAB-38 "Web Awesome 3.9"). Source tag: https://github.com/shoelace-style/webawesome/tree/v3.9.0

## Overview

Upgrade the WebAwesome.Blazor bindings from 3.8.0 to 3.9.0. The CEM diff
(`temp\wa-api\changes_3.8.0_to_3.9.0.json`) is the authoritative worklist:

| Metric | Count |
|---|---|
| New components | 1 (`wa-checkbox-group`) |
| Removed components | 0 |
| Modified components | 1 (`wa-tree`) |
| Breaking changes | 1 (`wa-tree.selection` enum extension) |

This is a small, focused release: one new grouping wrapper and one additive
enum value on an existing wrapper. The component count moves 82 -> 83.

## Phase 1 — Breaking changes

### 1. `wa-tree` — `selection` gains `leaf-multiple` (BREAKING per report; additive for the wrapper)

**Upstream:** the `selection` attribute type widened from
`'single' | 'multiple' | 'leaf'` to `'single' | 'multiple' | 'leaf' | 'leaf-multiple'`.
"Leaf-multiple allows multiple leaf nodes to be selected while parent nodes only
expand and collapse." Default unchanged (`'single'`). No existing value removed or
renamed, so this is source-compatible for wrapper consumers — it only adds a new
choice.

**Files:**
- `src\WebAwesome.Blazor\Components\Enums.cs`
  - add `LeafMultiple` member to `enum WaTreeSelection` (after `Leaf`), with doc comment.
  - add `WaTreeSelection.LeafMultiple => "leaf-multiple"` case to the
    `ToHtmlValue(this WaTreeSelection)` switch.
- `src\WebAwesome.Blazor\Components\WaTree.cs` — no code change needed (already
  renders `Selection.ToHtmlValue()`); update the `Selection` XML doc to mention
  the new value.

## Phase 2 — New components

### 2. `wa-checkbox-group` -> `WaCheckboxGroup` (new, non-Pro, stable, since 3.9)

**Nature:** a *grouping* wrapper, not a form control. It carries **no value and
no events** — the grouped `<wa-checkbox>`/`<wa-switch>` items remain independent
form controls with their own `name`/`value`/validation. The group only supplies a
shared label, hint, sizing, and accessible grouping. Therefore it derives from
`ComponentBase` (model on `WaTree`), **not** `WaInputBase<T>` (unlike
`WaRadioGroup`, which does bind a value). Consequently no `@bind-Value` and **no
EditForm coverage** applies to this component.

**File:** `src\WebAwesome.Blazor\Components\WaCheckboxGroup.cs` (new).

**Element:** `<wa-checkbox-group>`.

**Parameters (bound attributes):**

| CEM attribute | Wrapper parameter | Type | Notes |
|---|---|---|---|
| `label` | `Label` | `string?` | plain-text label attribute |
| `hint` | `Hint` | `string?` | plain-text hint attribute |
| `orientation` | `Orientation` | `WaOrientation?` | reuse existing enum + `ToHtmlValue()`; upstream default `vertical` |
| `size` | `Size` | `WaSize?` | reuse existing enum + `ToHtmlValue()`; applies to all items |
| `required` | `Required` | `bool` | visual-only indicator on the label |
| `with-hint` | `WithHint` | `bool` | SSR slot reservation |
| `with-label` | `WithLabel` | `bool` | SSR slot reservation |

**Common wrapper members** (same shape as `WaTree`): `Element`,
`AdditionalAttributes` (CaptureUnmatchedValues), `Class`, `Style`.

**Slots:** default (`ChildContent`), plus `label` and `hint` markup slots exposed
as `MarkupLabel` / `MarkupHint` `RenderFragment?` parameters (span with
`slot="label"`/`slot="hint"`), matching the `WaInputBase.AddLabelAndHintSlots`
idiom but rendered inline since this type does not derive from that base.

**Events / methods:** none in the CEM.

**Globally ignored attributes** (`did-ssr`, `dir`, `lang`) are handled by the
parity global ignore list; not wrapper parameters.

## Phase 3 — Modified components (additive)

None beyond the `wa-tree` enum extension already covered in Phase 1.

## Phase 4 — Intentional deviations (parity-config.json)

Add a `wa-checkbox-group` component entry documenting the deviations, each with a
matching `ignoreReasons` entry:
- `ignoredAttributes`: none expected beyond globals — all seven consumer attributes
  are bound. `size`, `orientation` map to enums; `with-hint`/`with-label` map to
  `WithHint`/`WithLabel`. Confirm the parity test is green with no per-component
  ignores; add entries only if the test reports a genuine deviation.

`wa-tree` needs no parity change — the enum value is invisible to the attribute-name
parity check (the attribute name `selection` is unchanged).

## Phase 5 — Element method audit

No wrapper method changes. Re-verify the existing `extraElementMethods` allowlist
entries against the 3.9.0 source (`temp\wa-src\3.9.0\dist`):
- `wa-mutation-observer` / `wa-resize-observer`: `stopObserver` / `startObserver`
- `wa-relative-time`: `update`

Update the re-verification stamps in `ignoreReasons` to include 3.9.0.

## Phase 6 — Tests

- New: `WaCheckboxGroupIntegrationTests.cs` — element name, label/hint attributes
  and markup slots, orientation, size, required, with-hint/with-label, child
  content, additional attributes / class / style passthrough. Pattern: existing
  `Wa*IntegrationTests.cs` (grouping-component style, e.g. tree/radio-group minus
  the value binding). **No EditForm test** (not a form control).
- Update: `WaTree` tests — add coverage for `WaTreeSelection.LeafMultiple` ->
  `selection="leaf-multiple"`, and the `ToHtmlValue` enum test if one enumerates
  all values.
- Breaking-change validation: the tree enum change is additive; a test asserting
  the new value renders is sufficient (no removal to guard).

## Phase 7 — Docs

- No breaking removals/renames -> a full `docs\MIGRATION-3.9.0.md` is optional;
  because the report flags `wa-tree.selection` as "breaking", write a short
  migration note documenting the additive `leaf-multiple` option (no consumer
  action required) plus the new `WaCheckboxGroup`.
- `docs\CHANGELOG.md`: add `## [3.9.0]` with `### New components`
  (`WaCheckboxGroup`), `### Changed` (`WaTree.Selection` gains `LeafMultiple`), and
  `### Library` (version bump, parity re-verification) subsections. Fold any
  `## [Unreleased]` items in.

## Phase 8 — Demo

- Run `tools\demo\New-WaDemoPages.ps1 -PruneRemoved` (adds a `WaCheckboxGroup`
  skeleton; nothing to prune).
- Curate `CheckboxGroupPage.razor` from `inputs\WebAwesome\components\checkbox-group.md`:
  ComponentBadges, intro, ExampleSection blocks (labels, hint, orientation, sizes,
  disabling via child checkboxes, switches, required), ApiTable, TODO removed.
- Showcase: `WaCheckboxGroup` is a form control grouping element -> fold into the
  **form showcase** where checkboxes appear, if a natural spot exists; otherwise
  note deferral here with reason.

## Phase 9 — Snapshots and verification

- Copy `surface_3.9.0.json` over the parity expected surface and the demo
  `api-surface.json`; bump `Version.props` (3.9.0) and `README.md`; set demo
  `index.html` CDN to 3.9.0.
- `PublicApiSnapshotTests`: promote `received-public-api.txt` after confirming the
  diff is exactly `WaCheckboxGroup` + `WaTreeSelection.LeafMultiple`.
- Build Debug + Release, `dotnet test`, Playwright e2e sweep green.

## Validation checklist

- [ ] `WaTreeSelection.LeafMultiple` added + mapped to `leaf-multiple`
- [ ] `WaCheckboxGroup` wrapper created (ComponentBase, no value/events)
- [ ] parity harness armed (expected surface = 3.9.0, enabled=true, target=3.9.0)
- [ ] `ApiSurfaceParityTests` green (no unexplained gaps)
- [ ] `ElementMethodInvocationTests` green; extra-method allowlist re-verified for 3.9.0
- [ ] `EventBindingRegistrationTests` green
- [ ] integration tests added; existing tree tests updated
- [ ] public API snapshot promoted (diff explained)
- [ ] CHANGELOG + migration note written
- [ ] demo page curated; showcase updated or deferral noted
- [ ] Debug + Release build green; full test suite green; e2e green

## Risk assessment

- **Low risk overall.** The `wa-tree` change is purely additive (new enum value).
  `WaCheckboxGroup` is a thin grouping wrapper with no value binding, no events, and
  no JS methods — the lowest-complexity class of new component.
- Only nuance: correctly modeling `WaCheckboxGroup` as a non-input grouping wrapper
  (ComponentBase) rather than copying `WaRadioGroup`'s `WaInputBase<string?>` shape.
  Getting this wrong would introduce a spurious value binding the upstream component
  does not have.

## Notes

- Windows/PowerShell only for all tooling and VCS operations.
- Pro-source rule: nothing from `temp\` leaves `temp\` except the sanctioned API
  surface JSON and bundled reference docs. `wa-checkbox-group` is non-Pro anyway.
