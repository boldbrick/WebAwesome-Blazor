# Web Awesome 3.9.0 to 3.10.0 Upgrade Implementation Plan

## Overview
Implementation plan for upgrading the WebAwesome.Blazor bindings from **3.9.0** to **3.10.0**.
Authoritative worklist: `temp\wa-api\changes_3.9.0_to_3.10.0.json` (the CEM surface diff).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 1 |
| Removed components | 0 |
| Modified components | 2 |
| Breaking changes | 0 |

This is a small, purely additive release. No breaking changes, so no migration document is
required (Phase 5.2 skipped). Component count 83 -> 84.

> Note on the change-report markdown: the `wa-known-date` section of
> `changes_3.9.0_to_3.10.0.md` prints a noisy list of `Length`/`Rank`/`SyncRoot`/`Count`/etc.
> "changed" cssParts. Those are .NET `System.Array` member names — a rendering artifact of the
> compare script enumerating an array's members, not real CSS parts. The **JSON** report is
> authoritative and shows the only real change: cssParts **removed** `error`. See below.

## Phase 1 — Breaking changes
None. (`breakingChanges: []`.)

## Phase 2 — New components

### 1. WaRandomContent (`wa-random-content`) — NEW
**Status:** experimental, since 3.9, **not Pro**.
**File:** `src\WebAwesome.Blazor\Components\WaRandomContent.cs` (new)
**Doc:** `inputs\WebAwesome\components\random-content.md`
**Summary:** Selects one or more slotted children at random and displays them, hiding the rest
(rotating testimonials, tip-of-the-day, featured content).

**Attributes (authorable):**
- `Animation` — enum `WaRandomContentAnimation?` { None, Fade, FadeUp, FadeDown, FadeLeft, FadeRight }, HTML values `none|fade|fade-up|fade-down|fade-left|fade-right`; attribute `animation`; default null (upstream default `none`).
- `Autoplay` — `bool`; attribute `autoplay`. Rotate automatically.
- `AutoplayInterval` — `int?` milliseconds; attribute `autoplay-interval` (upstream default 3000).
- `Items` — `int?`; attribute `items`. Number of children shown simultaneously; clamped [1, childCount] (upstream default 1).
- `Mode` — enum `WaRandomContentMode?` { Random, Unique, Sequence }, HTML values `random|unique|sequence`; attribute `mode`; default null (upstream default `unique`).
- Globals `did-ssr`, `dir`, `lang` — covered by `globalIgnoredAttributes`; not authorable parameters.

**Slot:** default -> `ChildContent` (the pool of children to choose from).

**Event:** `wa-content-change` -> `OnContentChange`, `EventCallback<WaContentChangeEventArgs>`.
Detail is `{ items: Element[] }` — **live DOM nodes, not marshalable**. Follow the
`wa-selection-change` idiom: register a `specialArgs` projection in
`WebAwesome.Blazor.lib.module.js` that projects each element to `{ id, textContent }` and adds a
derived `count`. `WaContentChangeEventArgs : EventArgs` exposes `int Count` and `object[]? Items`
(mirroring `WaTreeSelectionChangeEventArgs`). Register `wa-content-change` in the `eventNames`
list AND add the `specialArgs['wa-content-change']` projection. `onwa-content-change` prefix.

**Method:** `randomize() => Element[]` -> `RandomizeAsync()`. Invokes the element's `randomize()`
via JS interop and **discards the `Element[]` return** (DOM nodes are not marshalable across
Blazor interop). `randomize` is CEM-documented, so no `extraElementMethods` allowlist entry needed.

**CSS custom properties (doc only, not parameters):** `--animation-duration`, `--animation-easing`,
`--animation-translate` — passed through `Style`/`Class`; no wrapper parameters.

## Phase 3 — Modified components (additive)

### 2. WaIcon (`wa-icon`) — add `canvas`
**File:** `src\WebAwesome.Blazor\Components\WaIcon.cs`
- Add attribute `canvas` (`IconCanvas | undefined` upstream). Source union:
  `type IconCanvas = 'fixed' | 'auto' | 'square' | 'roomy'`.
- New parameter `Canvas` of enum `WaIconCanvas?` { Fixed, Auto, Square, Roomy }, `ToHtmlValue`
  -> `fixed|auto|square|roomy`; attribute `canvas`; default null (unset renders as `fixed`, the
  1.25em × 1em box). Emit via `AddAttributeIfNotNull(_, "canvas", Canvas?.ToHtmlValue())`.
- Doc: sets the icon canvas (box the icon is centered within); mirrors Font Awesome's
  `fa-fixed-width` / `fa-width-auto` / `fa-canvas-square` / `fa-canvas-roomy`. Scales with `font-size`.

### 3. WaKnownDate (`wa-known-date`) — cssParts removed `error`
- The only real change is a removed CSS `::part(error)`. CSS parts are not wrapper parameters and
  do not appear in the wrapper's public API. **No wrapper code change.** Action: confirm no demo
  page or showcase targets `::part(error)` on `wa-known-date` (none expected — the wrapper never
  exposed it as a parameter). No parity impact (parity checks attributes/events/methods, not cssParts).

## Phase 4 — Intentional deviations (parity-config.json)
None expected for this release: `WaRandomContent` exposes all authorable attributes, the event, and
the `randomize` method; `WaIcon.Canvas` covers the new attribute. The globals (`did-ssr`/`dir`/`lang`)
are already in `globalIgnoredAttributes`. **Re-verify** the existing `extraElementMethods` entries
(`stopObserver`/`startObserver` on the two observers, `update` on `wa-relative-time`) against the
3.10.0 source and extend their `ignoreReasons` provenance note to include 3.10.0.

## Phase 5 — Tests and docs
- **wa-test-engineer:** `WaRandomContentIntegrationTests` (attributes, enum ToHtmlValue mapping,
  event binding, `RandomizeAsync` interop, ChildContent slot). Update `WaIconTests` for the new
  `Canvas` parameter. WaRandomContent is **not** a form control (does not derive from
  `WaInputBase<T>`/`InputBase<T>`), so no EditForm coverage is required.
- **Migration doc:** none (no breaking changes).
- **CHANGELOG:** add `## [3.10.0] — 2026-07-24` with `### New components` (WaRandomContent),
  `### Changed` (WaIcon.Canvas; wa-known-date removed `::part(error)`), `### Library` (version bump,
  docs refresh, parity target 3.10.0). Fold any `## [Unreleased]` items in.
- **Demo:** `New-WaDemoPages.ps1 -PruneRemoved`; curate `RandomContentPage.razor` from
  `inputs\WebAwesome\components\random-content.md` (ComponentBadges, intro, ExampleSection blocks,
  ApiTable, TODO removed). Add a `Canvas` example to the existing icon page.
- **Showcases:** WaRandomContent is content-rotation — fits the **content showcase** if one exists;
  otherwise note deferral. No removed components, so no showcase deletions.
- **Public API snapshot:** promote `received-public-api.txt` -> `approved-public-api.txt` after
  confirming the diff = { WaRandomContent + its enums + WaContentChangeEventArgs, WaIcon.Canvas +
  WaIconCanvas }.

## Validation checklist
- [ ] WaRandomContent wrapper created (attributes, event, ChildContent, RandomizeAsync)
- [ ] WaContentChangeEventArgs added; wa-content-change registered in the JS module with specialArgs
- [ ] WaIcon.Canvas added with WaIconCanvas enum
- [ ] extraElementMethods re-verified against 3.10.0 source; provenance note extended
- [ ] ApiSurfaceParityTests / EventBindingRegistrationTests / ElementMethodInvocationTests green
- [ ] Integration tests added; full suite green (Debug + Release)
- [ ] Public API baseline promoted
- [ ] Demo page curated; demo builds; e2e sweep green

## Risks
- **Low overall.** Additive release.
- Event marshaling: `wa-content-change` detail carries live DOM nodes — must use the projection
  pattern (like `wa-selection-change`), never attempt to marshal `Element[]` directly.
- `randomize()` return `Element[]` is discarded — the wrapper exposes a fire-and-forget invocation.
