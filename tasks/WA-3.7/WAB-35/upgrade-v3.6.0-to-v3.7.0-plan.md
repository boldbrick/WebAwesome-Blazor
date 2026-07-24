# Web Awesome 3.6.0 to 3.7.0 Upgrade Implementation Plan

## Overview
Upgrade the WebAwesome.Blazor bindings from Web Awesome **3.6.0** to **3.7.0**.
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.7.0
JIRA: WAB-35 (epic WAB-34 "Web Awesome 3.7"). Branch: `/main/WA-3.7/WAB-35` (new WA-3.7 train — subtrunk `/main/WA-3.7` created off `/main`, previous train WA-3.6 released at cs:185, release gate satisfied).

## Analysis Summary
Authoritative worklist: `temp\wa-api\changes_3.6.0_to_3.7.0.json` (CEM surface diff, 3.6.0 → 3.7.0).

| Metric | Count |
|---|---|
| New components | 2 |
| Removed components | 0 |
| Modified components | 1 |
| Breaking changes | 0 |

This is an additive upgrade with **no breaking changes and no removed components**:

1. **Two new Pro components**: `wa-video` (WaVideo) and `wa-video-playlist` (WaVideoPlaylist).
2. **One modified component**: `wa-copy-button` gains a `tooltip` attribute (`'full' | 'copy' | 'none'`, default `'full'`); CSS parts churn (tooltip sub-parts replaced by a `feedback` part — docs only, not wrapper-relevant); status experimental → stable (metadata only, badge is data-driven).

### Tooling note (non-blocking)
The generated markdown report `temp\wa-api\changes_3.6.0_to_3.7.0.md` renders the `wa-copy-button` cssParts diff incorrectly — it reflects over the .NET `List<string>` object's own properties (`Length`, `Count`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`, `IsSynchronized`, `Rank`) instead of the list elements. The **JSON** report is correct and authoritative (cssParts added `feedback`, removed the four `tooltip__*` parts). This is a defect in `tools\upgrade\Compare-WaApiSurface.ps1`'s markdown writer only; it does not affect the JSON worklist or any implementation. Flagged for a later tooling fix; out of scope for this upgrade.

### Parity-harness impact
`ApiSurfaceParityTests` checks attribute/event/method **existence** (not enum unions or defaults). The forcing changes are:
- new component wrappers `WaVideo` / `WaVideoPlaylist` with all their attributes, events, methods;
- new `Tooltip` parameter on `WaCopyButton`.

`ElementMethodInvocationTests`: every JS method a wrapper invokes must be CEM-documented or allowlisted; the new wrappers invoke only documented methods (plus the `getVideoElement`/`getState` handling below).

## Event delivery analysis (load-bearing)

**wa-video** documents 7 events: `play`, `pause`, `timeupdate`, `volumechange`, `error`, `ended`, `loadedmetadata`. Source inspection (`temp\wa-src\3.7.0\dist\chunks\chunk.ZA3QCVZ6.js` + base `chunk.L42WI6IM.js`) shows the component re-dispatches each via the inherited `relayNativeEvent(event, eventOptions)` helper:
```
relayNativeEvent(event, eventOptions) {
  event.stopImmediatePropagation();
  this.dispatchEvent(new event.constructor(event.type, { ...event, ...eventOptions }));
}
```
All call sites pass no `eventOptions`, and spreading a native `Event` does not carry `bubbles`/`composed` — so these are **non-bubbling, non-composed** events re-fired on the `wa-video` host. All seven are in Blazor's built-in **non-bubbling** event registry (`onplay`, `onpause`, `ontimeupdate`, `onvolumechange`, `onerror`, `onended`, `onloadedmetadata`), for which Blazor attaches a direct (non-delegated) listener on the element. Therefore:
- Bind them as native Blazor events (`AddAttributeIfHasDelegate(seq, "onplay", OnPlay)`, etc.) delivering `EventArgs`.
- **No `registerCustomEventType` / JS-module change** for these (they are Blazor built-ins; re-registering could clobber the built-in mapping).
- `EventBindingRegistrationTests` scans only `wa-*` bindings, so native names neither trigger nor violate it.

**wa-video-playlist** documents 1 event: `wa-video-change` (a real `wa-` custom event). Its detail is `{ previousIndex, currentIndex, video }` where `video` is a live DOM node. This one:
- binds as `onwa-video-change`,
- must be registered in `WebAwesome.Blazor.lib.module.js` (`eventNames` array: `'wa-video-change',`),
- gets a `specialArgs` mapping that projects the marshalable fields and drops the DOM node: `{ previousIndex, currentIndex, videoTitle: event.detail?.video?.title }`.

## Phase 1 — Breaking changes
None. (No `breakingChanges` in the report.)

## Phase 2 — New components (delegate to wa-wrapper-engineer)

One group of 2 components (well under the 10-per-agent cap).

### `WaVideo` — `src\WebAwesome.Blazor\Components\WaVideo.cs` (Pro, `ComponentBase`)
Model on plain-element wrappers (e.g. `WaCopyButton`): `ComponentBase`, `Element`/`AdditionalAttributes`/`Class`/`Style`, `BuildRenderTree`, JS-interop method calls.

Attributes (all additive; mechanical PascalCase is clean for every one — **no attributeOverrides needed**):
- `Controls` — new enum `WaVideoControls { Standard, None, Full }` → `standard`/`none`/`full` (default `standard`); `ToHtmlValue()`.
- `Preload` — new enum `WaVideoPreload { Metadata, Auto, None }` → `metadata`/`auto`/`none` (default `metadata`); `ToHtmlValue()`.
- `Src` (string), `Poster` (string), `Thumbnails` (string), `IconLibrary` (string).
- `Title` (string) — genuine WA attribute ("The video's title"); render as `title`. Note: `title` is in `globalIgnoredAttributes`, so parity does not require it, but expose it for functionality.
- `Playing` (bool), `Muted` (bool), `Autoplay` (bool), `Loop` (bool), `AutoplayMuted` (bool → `autoplay-muted`), `AutoplayOnVisible` (bool → `autoplay-on-visible`).
- `Volume` (double, default 1), `Duration` (double), `CurrentTime` (double) — numeric state/reflected attributes; render with invariant formatting.

Slots (RenderFragment params + `slot="..."`): default `ChildContent` (place `<source>`/`<track>`), plus icon slots `poster-icon`, `play-icon`, `pause-icon`, `volume-icon`, `mute-icon`, `fullscreen-icon`, `exit-fullscreen-icon`, and `controls-start`, `controls-after-play`. Follow the `WaCopyButton` icon-slot pattern (RenderFragment + optional `*IconName` convenience where it fits).

Events (native, `EventArgs`, per the delivery analysis above — natural PascalCase names, multi-word ones mapped in parity-config `eventOverrides`):
- `OnPlay` (`onplay`), `OnPause` (`onpause`), `OnEnded` (`onended`), `OnError` (`onerror`) — mechanical == natural, no override.
- `OnTimeUpdate` (`ontimeupdate`), `OnVolumeChange` (`onvolumechange`), `OnLoadedMetadata` (`onloadedmetadata`) — natural PascalCase; add `eventOverrides` + `ignoreReasons`.

Methods (JS interop via injected `WebAwesomeJSInterop`; parity checks name existence only):
- Void: `PlayAsync`, `PauseAsync`, `TogglePlayAsync`, `ToggleMuteAsync`, `SeekAsync(double time)`, `SetVolumeAsync(double volume)`, `SetPlaybackRateAsync(double rate)` — `InvokeMethodAsync(Element, "play"|...)`.
- Async void-returning promises: `RequestFullscreenAsync`, `ExitFullscreenAsync` — `InvokeMethodAsync(Element, "requestFullscreen"|"exitFullscreen")`.
- `GetStateAsync` → returns a `WaVideoState` record (`Playing`, `CurrentTime`, `Duration`, `Volume`, `Muted`, `PlaybackRate`) via `InvokeMethodAsync<WaVideoState>(Element, "getState")` — the state object is JSON-safe.
- `getVideoElement()` → **ignoredMethod**: returns a live `HTMLVideoElement`, not marshalable across JS interop.

### `WaVideoPlaylist` — `src\WebAwesome.Blazor\Components\WaVideoPlaylist.cs` (Pro, `ComponentBase`)
- Attributes: `Controls` (reuse `WaVideoControls`; default here is `full`), `IconLibrary` (string).
- Slot: default `ChildContent` (place `<wa-video>` elements).
- Event: `OnVideoChange` bound as `onwa-video-change` → new event args `WaVideoChangeEventArgs : EventArgs { int PreviousIndex; int CurrentIndex; string? VideoTitle }`.
- Methods: `NextAsync`, `PreviousAsync`, `GoToAsync(int index)`.

### Supporting types
- `src\WebAwesome.Blazor\Components\Enums.cs`: add `WaVideoControls`, `WaVideoPreload` with `ToHtmlValue()` cases.
- `src\WebAwesome.Blazor\Components\EventArgs.cs`: add `WaVideoChangeEventArgs` (and a `WaVideoState` record — in a Models file or alongside, per repo convention).
- `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js`: add `'wa-video-change',` to `eventNames`; add the `wa-video-change` `specialArgs` mapping.

## Phase 3 — Modified component (additive)

### `WaCopyButton` — `src\WebAwesome.Blazor\Components\WaCopyButton.cs`
- Add enum `WaCopyButtonTooltip { Full, Copy, None }` → `full`/`copy`/`none` (default `Full`).
- Add `[Parameter] public WaCopyButtonTooltip Tooltip { get; set; } = WaCopyButtonTooltip.Full;` and render `builder.AddAttributeIfNotNull(seq, "tooltip", Tooltip.ToHtmlValue())` (or always-emit since it has a default). Mechanical PascalCase `tooltip` → `Tooltip`, no override needed.

## Phase 4 — Intentional deviations & allowlists (parity-config.json)
**File**: `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json`
- `targetWaVersion` → `3.7.0`; keep `"enabled": true`.
- `wa-video`: add `eventOverrides` `{ "timeupdate": "OnTimeUpdate", "volumechange": "OnVolumeChange", "loadedmetadata": "OnLoadedMetadata" }`; add `ignoredMethods` `[ "getVideoElement" ]`. Add matching `ignoreReasons` entries (natural-PascalCase event rename; non-marshalable live `HTMLVideoElement`).
- **Re-verify `extraElementMethods` against 3.7.0 source** (done during analysis — all still present): `wa-mutation-observer`/`wa-resize-observer` `startObserver`/`stopObserver` (private in the `.d.ts`), `wa-relative-time` `update` (inherited Lit lifecycle). Append `3.7.0 (2026-07-24)` to the two `extraElementMethods:*` verification stamps.

## Phase 5 — Version, surface, demo sync
- `src\Version.props`: `Version`/`AssemblyVersion`/`FileVersion` → `3.7.0`.
- `README.md`: version references → `3.7.0`.
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` ← `temp\wa-api\surface_3.7.0.json`.
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html`: WA CDN version → `3.7.0`.
- `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` ← `temp\wa-api\surface_3.7.0.json` (drives nav, API tables, experimental→stable badge for copy-button).

## Phase 6 — Tests & docs
- **Tests** (wa-test-engineer):
  - `WaVideoIntegrationTests` — attributes/enums/events/slots/method interop (pattern: existing `Wa*IntegrationTests.cs`). Neither new component derives from `WaInputBase<T>`/`InputBase<T>`, so **no EditForm coverage is required**.
  - `WaVideoPlaylistIntegrationTests` — attributes, `onwa-video-change` delivery, methods.
  - `WaCopyButton` — add coverage for the `Tooltip` parameter → `tooltip` attribute.
- **MIGRATION**: no breaking changes → **no MIGRATION-3.7.0.md** (note the absence explicitly in the changelog/plan).
- **CHANGELOG.md**: `## [3.7.0]` entry — New components (WaVideo, WaVideoPlaylist), Changed (WaCopyButton.Tooltip; wa-copy-button stabilized), Library. Fold any `## [Unreleased]` items in.
- **Demo**:
  - `New-WaDemoPages.ps1 -PruneRemoved` (adds VideoPage + VideoPlaylistPage skeletons; nothing pruned).
  - Curate `VideoPage.razor` and `VideoPlaylistPage.razor` from `inputs\WebAwesome\components\video.md` / `video-playlist.md` (ComponentBadges, intro, ExampleSection blocks mirroring live markup, ApiTable, TODO removed). Both are Pro — mirror the doc examples using the wrappers. Use only parameters that exist on the wrappers; record any genuine gap here.
  - Showcases: `wa-video`/`wa-video-playlist` are Pro media/content components. Assess the content showcase as a natural home; if none fits cleanly, defer with a note here (media playback has no existing showcase narrative — likely deferred).
- **PublicApiSnapshot**: promote `received-public-api.txt` after confirming the diff = the new `WaVideo`, `WaVideoPlaylist`, `WaVideoChangeEventArgs`, `WaVideoState`, `WaVideoControls`, `WaVideoPreload`, `WaCopyButtonTooltip` public surface + `WaCopyButton.Tooltip`. Never promote unexplained diffs.

## Validation Checklist
- [ ] `WaVideo` + `WaVideoPlaylist` wrappers, enums, event args, state record added
- [ ] `wa-video-change` registered in JS module with specialArgs; native media events bound without registration
- [ ] `WaCopyButton.Tooltip` added
- [ ] parity-config: target 3.7.0, wa-video eventOverrides + getVideoElement ignore + reasons, extraElementMethods re-verified & re-stamped
- [ ] Version.props / README / expected-api-surface / demo index.html / demo api-surface.json updated
- [ ] `dotnet build` Debug + Release green; `dotnet test` green (parity + all suites)
- [ ] CHANGELOG entry written (no migration doc — no breaking changes)
- [ ] Demo pages curated (Video, VideoPlaylist); showcase decision recorded
- [ ] Public API baseline promoted (diff matches report)
- [ ] Playwright e2e sweep green

## Risks
- **Low overall.** Additive only; no breaking changes, no removed components.
- Media event delivery depends on Blazor's built-in non-bubbling registration reaching the WA-relayed (non-bubbling, non-composed) events on the host — verified by source analysis; covered by e2e + bUnit.
- `wa-video-change` delivery depends on JS `registerCustomEventType` + specialArgs (silent failure if missed) — covered by e2e and `EventBindingRegistrationTests`.
- Both components are Pro; e2e needs local assets (WA Pro sources stay under `temp\`, never checked in).

## Notes for Implementation
- Prefer CEM + `.d.ts` over markdown for semantics.
- Pro-source stays under `temp\` — only the API surface JSON and bundled reference docs are checked in.
- `getState()` returns `{ playing, currentTime, duration, volume, muted, playbackRate }` (JSON-safe) → `GetStateAsync` returns a `WaVideoState` record. `getVideoElement()` returns a live DOM node → ignoredMethod.
