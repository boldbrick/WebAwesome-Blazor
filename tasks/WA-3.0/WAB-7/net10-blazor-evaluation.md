# .NET 10 Blazor Evaluation for the Web Awesome Wrapper Library

Evaluation of ASP.NET Core / Blazor changes in .NET 10 (GA November 2025) relevant to a pure-C# (`BuildRenderTree`) Blazor wrapper library for Web Awesome custom elements, multi-targeting `net9.0;net10.0`.

Date of evaluation: 2026-07-18. Sources: learn.microsoft.com release notes and breaking-changes docs, and the official per-preview API diffs in `dotnet/core` (preview1–rc2).

## Summary Verdict

**No `#if NET10_0_OR_GREATER` code paths are warranted.** Nothing in .NET 10 changes `RenderTreeBuilder`, `InputBase<TValue>`, `EditForm`/`EditContext`, or the JS-interop surface the library actually uses (`IJSObjectReference.InvokeAsync` on a module). The verified API diffs show zero changes to `RenderTreeBuilder` and zero changes to `InputBase`/`EditContext` across all .NET 10 previews and RCs.

Multi-targeting `net9.0;net10.0` is still worth doing — but for **dependency hygiene** (referencing `Microsoft.AspNetCore.Components.Web` 10.x on the net10.0 target instead of forcing a 9.x reference onto net10 consumers), not for API divergence. A single shared code path compiles unchanged on both targets; no source-incompatible breaking change in .NET 10 touches the APIs this library consumes.

The only genuinely tempting new API — JS constructor/property interop (`InvokeConstructorAsync`, `GetValueAsync`, `SetValueAsync`) — does not eliminate the library's JS module, because there is no way to get an `IJSObjectReference` for a DOM element without JS help (`ElementReference` is not an `IJSObjectReference`). SKIP unless a concrete shim in the module becomes replaceable.

Separate strategic note: **.NET 9 (STS) went out of support on May 12, 2026.** The net9.0 target should be treated as legacy-compatibility; net10.0 (LTS, supported until November 2028) should be the primary/tested target.

## Findings Table

| # | Area | Finding | Verdict |
|---|------|---------|---------|
| 1 | Rendering | No changes to `RenderTreeBuilder` in .NET 10 (verified via API diffs) | SKIP |
| 2a | JS interop | New `IJSRuntime`/`IJSObjectReference` members: `InvokeConstructorAsync`, `GetValueAsync<T>`, `SetValueAsync<T>` (+ sync in-process variants) | SKIP (revisit) |
| 2b | JS interop | Custom `IJSRuntime`/`JSRuntime` implementations (e.g. test fakes) may need updating for new members / `JSInvocationInfo` overloads | CONSUMER-DOC (verify own test mocks) |
| 3a | Forms | New validation system: `AddValidation()`, `[ValidatableType]`, source-generated nested-object/collection validation | CONSUMER-DOC |
| 3b | Forms | `InputBase<TValue>`, `EditForm`, `EditContext` unchanged in .NET 10; new built-in `InputHidden` component | SKIP |
| 3c | Forms | Async validation (`EditContext.ValidateAsync`, `AddValidationTask`, `OnValidationRequestedAsync`) is **.NET 11 preview, NOT .NET 10** | SKIP (not in scope) |
| 4a | Components | `[PersistentState]` attribute, `RestoreBehavior`, `PersistentComponentStateSerializer<T>` | CONSUMER-DOC |
| 4b | Components | `NavigationManager.NotFound()` / `OnNotFound`, `Router.NotFoundPage`; `Router.NotFound` fragment obsolete | CONSUMER-DOC |
| 4c | Components | `NavLink.ShouldMatch(string)` now `protected virtual`; NavLink matching ignores query/fragment with `NavLinkMatch.All` | SKIP / ADOPT-CONDITIONAL only if the library subclasses `NavLink` |
| 4d | Components | Circuit state persistence, pause/resume, `ReconnectModal` template, `components-reconnect-state-changed` event, "retrying" state | CONSUMER-DOC |
| 4e | Components | `OwningComponentBase` now implements `IAsyncDisposable` | SKIP |
| 5 | Static assets | Blazor scripts served as fingerprinted static web assets; `ResourcePreloader` component; standalone WASM client-side fingerprinting; `blazor.boot.json` inlined | CONSUMER-DOC |
| 6a | WASM perf | No library-actionable trimming/AOT/WASM changes verified in .NET 10 runtime notes | SKIP |
| 6b | WASM behavior | `HttpClient` response streaming enabled by default in browser (breaking behavioral change) | CONSUMER-DOC |
| 7a | Multi-target | `NavigationManager.NavigateTo` during static SSR: `NavigationException` opt-out (`BlazorDisableThrowNavigationException`) | CONSUMER-DOC |
| 7b | Multi-target | SDK 10 changes: `DefineConstants` not available at MSBuild evaluation time; NU1510 pruning warnings; C# 14 span overload resolution | SKIP (monitor build) |

## Per-Finding Detail

### 1. RenderTreeBuilder / rendering APIs — no changes (SKIP)

The official per-preview API diffs for `Microsoft.AspNetCore.Components` (preview1 through rc2) contain **no entries for `RenderTreeBuilder`** — no new methods, no signature changes, no obsoletions. Rendering-adjacent changes are limited to internals: `ComponentState.GetComponentKey()` (protected internal virtual, preview7) and a `Renderer.SignalRendererToFinishRendering()` that was added in preview4 and removed again in preview5. `ComponentBase` is untouched.

Consequence: the library's pure-C# `BuildRenderTree` implementations compile and behave identically on net9.0 and net10.0. Sequence-number discipline, `OpenElement`/`AddAttribute`/`AddContent`/`SetKey` semantics — all unchanged.

Sources:
- https://github.com/dotnet/core/tree/main/release-notes/10.0/preview (api-diff, `Microsoft.AspNetCore.App/10.0-*_Microsoft.AspNetCore.Components.md` per preview)
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 2a. JS interop: constructor and property access (SKIP, revisit)

New in .NET 10 (added preview4 as `InvokeNewAsync`, renamed in preview7):

- `IJSRuntime` / `IJSObjectReference`: `InvokeConstructorAsync(identifier, args)` → `IJSObjectReference`; `GetValueAsync<TValue>(identifier)`; `SetValueAsync<TValue>(identifier, value)` (all with `CancellationToken` overloads, plus `TimeSpan` timeout extension methods).
- `IJSInProcessRuntime` / `IJSInProcessObjectReference`: synchronous `InvokeConstructor`, `GetValue<TValue>`, `SetValue<TValue>`.
- New infrastructure types: `JSCallType` enum (`FunctionCall`, `ConstructorCall`, `GetValue`, `SetValue`), `JSInvocationInfo` struct.

Why SKIP for this library: property access identifiers resolve from `window` (e.g. `"jsInterop.testObject.num"`) or relative to an `IJSObjectReference` you already hold. The library's central need — reading/writing properties and calling methods **on a DOM element** identified by `ElementReference` — is not covered, because `ElementReference` is not an `IJSObjectReference` and .NET 10 adds no conversion. The single small JS interop module remains necessary; adding a NET10-only path would create two code paths with no reduction in shipped JS. Revisit only if a specific module function is a pure property get/set on an object reference the C# side already holds — then a NET10-only fast path could retire that one shim, which still fails the "clear consumer value" bar.

Sources:
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0 (JS interop section)
- https://github.com/dotnet/core/tree/main/release-notes/10.0/preview (api-diff `Microsoft.JSInterop.md`, preview4/preview5/preview7)

### 2b. JS interop implementors / test mocks (CONSUMER-DOC / self-check)

The new members were added to the `IJSRuntime`/`IJSObjectReference` **interfaces** and new abstract-class plumbing was added to `JSRuntime`/`JSInProcessRuntime` (`BeginInvokeJS(in JSInvocationInfo)` / `InvokeJS(in JSInvocationInfo)` — introduced abstract in preview4, softened to `virtual` in preview5, so existing subclasses keep compiling). Could not verify from the API diff alone whether the interface additions are default interface methods; if the library's test suite contains hand-rolled classes implementing `IJSRuntime`/`IJSObjectReference` (rather than using a mocking framework or `JSRuntime` subclass), compilation on net10.0 should be checked — this is the most plausible multi-target compile risk found, and it is confined to test code. Libraries that only *consume* `IJSRuntime`/`IJSObjectReference` (this one) are unaffected.

Source: https://github.com/dotnet/core/tree/main/release-notes/10.0/preview (api-diff `Microsoft.JSInterop.md`)

### 3a. New validation system (CONSUMER-DOC)

.NET 10 adds opt-in, source-generated validation: consumer calls `builder.Services.AddValidation()`, annotates the root form model with `[ValidatableType]`, and gets validation of nested complex objects and collections (with `[SkipValidation]` to exclude properties). It is AOT-friendly (source generator, not reflection) and follows `System.ComponentModel.DataAnnotations.Validator` ordering/short-circuiting. Models must be declared in C# class files, not `.razor` files.

Impact on the library: **none in code**. This operates on the model/validator side of `EditContext`; validation messages still flow through `EditContext`/`ValidationMessageStore`, and `InputBase<TValue>`-derived controls receive field CSS classes exactly as before. Document for consumers: Web Awesome form controls work unchanged with the new nested validation; enabling it is purely an app-level decision (`AddValidation()` + `[ValidatableType]`), and a library/`.Client`-project helper can wrap the registration if models live in another assembly.

Sources:
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0 (Blazor validation section)
- https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-10.0

### 3b. InputBase / EditForm unchanged; InputHidden added (SKIP)

The .NET 10 API diffs show no changes to `InputBase<TValue>`, `EditForm`, or `EditContext`. The only addition in `Microsoft.AspNetCore.Components.Forms` is the new `InputHidden : InputBase<string?>` built-in component (preview7) — a convenience component, irrelevant to a wrapper library that already has its own hidden-input capability via native elements. The library's `InputBase<TValue>` inheritance strategy carries to net10.0 unchanged.

Source: https://github.com/dotnet/core/tree/main/release-notes/10.0/preview (api-diff `Microsoft.AspNetCore.Components.Web.md`, preview7)

### 3c. Async validation is NOT in .NET 10 (SKIP — recorded to prevent confusion)

Several third-party blog posts attribute `EditContext.ValidateAsync`, `EditContext.AddValidationTask`, and `OnValidationRequestedAsync` to ".NET 10". Verified against the Microsoft Learn forms-validation page: these are gated to the **`aspnetcore-11.0` moniker** (.NET 11 previews), along with automatic `pending`/`faulted` CSS classes on `InputBase<TValue>` and client-side validation for static SSR forms. Nothing to do for a net9/net10 library; worth re-evaluating when .NET 11 planning starts, since the `pending`/`faulted` CSS classes will then interact with the wrapper controls' class generation.

Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-10.0 (async sections carry `::: moniker range="aspnetcore-11.0"`)

### 4a. Declarative persistent state (CONSUMER-DOC)

New `[PersistentState]` attribute (a `CascadingParameterAttributeBase`; the interim `[SupplyParameterFromPersistentComponentState]` from preview3 was renamed in preview7) persists public component properties across prerendering, enhanced navigation, and circuit resume. Supporting APIs: `RestoreBehavior` (`SkipInitialValue`, `SkipLastSnapshot`), `PersistentStateAttribute.AllowUpdates`, `PersistentComponentState.RegisterOnRestoring`, `PersistentComponentStateSerializer<T>` for custom serialization, `RegisterPersistentService<TService>()`.

Impact on the library: a UI wrapper library should not persist state on behalf of consumers — its components are stateless wrappers whose state lives in consumer-bound parameters. Document that consumers can put `[PersistentState]` on *their* properties bound to Web Awesome control values; nothing in the library needs the attribute. No conditional compilation justified.

Sources:
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0
- api-diff preview3/preview7/rc1 `Microsoft.AspNetCore.Components.md`

### 4b. NotFound handling (CONSUMER-DOC)

`NavigationManager.NotFound()` (works in static SSR → 404, and interactive rendering → router renders not-found content), `NavigationManager.OnNotFound` event with settable `NotFoundEventArgs.Path`, and `Router.NotFoundPage` parameter. **`Router.NotFound` render fragment is `[Obsolete]` in .NET 10** ("NotFound is deprecated. Use NotFoundPage instead."). The library does not ship a router, so no code impact; if any sample/demo app uses `<NotFound>` in its `Router`, that produces a warning on net10.0 — fix in the demo, not with `#if`.

Sources:
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0
- api-diff preview3–preview7 `Microsoft.AspNetCore.Components.md`

### 4c. NavLink changes (SKIP unless the library subclasses NavLink)

`NavLink` gained `protected virtual bool ShouldMatch(string uriAbsolute)` (preview2/preview4), and with `NavLinkMatch.All` it now ignores query string and fragment by default (opt-out AppContext switch `Microsoft.AspNetCore.Components.Routing.NavLink.EnableMatchAllForQueryStringAndFragment`). If the library ever wraps a Web Awesome navigation-item element by deriving from `NavLink`, `ShouldMatch` would be a legitimate ADOPT-CONDITIONAL (`#if NET10_0_OR_GREATER` override to align active-state logic with the custom element). If, as is typical, the library renders anchors itself, SKIP; document the behavioral change for consumers who pair library nav components with `NavLink`.

Sources:
- api-diff preview2/preview4 `Microsoft.AspNetCore.Components.Web.md`
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 4d. Reconnection UX and circuit persistence (CONSUMER-DOC)

Server interactivity gains: circuit state persisted across connection loss (tab throttling, mobile app switching, network drops, proactive pausing of inactive circuits), a template `ReconnectModal` component with collocated CSS/JS (CSP-friendly), a new `components-reconnect-state-changed` JS event, and a new "retrying" reconnect state/CSS class. Nothing here is library code — but it is worth a consumer-doc note: a Web Awesome-styled reconnection modal is a natural consumer recipe (style the template's `ReconnectModal` with `<wa-dialog>`/`<wa-spinner>`), and `[PersistentState]` (4a) is what makes control values survive circuit eviction.

Source: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 4e. OwningComponentBase implements IAsyncDisposable (SKIP)

`OwningComponentBase` adds `DisposeAsyncCore()` and async disposal of its service scope (preview7). Only relevant if the library derives from `OwningComponentBase` — a wrapper library normally derives from `ComponentBase`. No action.

Source: api-diff preview7 `Microsoft.AspNetCore.Components.md`

### 5. Static asset delivery (CONSUMER-DOC)

Verified .NET 10 changes:
- `blazor.web.js`/`blazor.server.js` are now served as fingerprinted, precompressed **static web assets** instead of embedded resources. Apps hosting only interop-heavy pages without Razor components may need `<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>`.
- New `<ResourcePreloader />` component in `App.razor` `<head>` replaces `Link`-header preloading for framework assets (correct base-path handling).
- Standalone Blazor WebAssembly: opt-in client-side fingerprinting via `#[.{fingerprint}]` placeholders + `<OverrideHtmlAssetPlaceholders>true</OverrideHtmlAssetPlaceholders>`, and `StaticWebAssetFingerprintPattern` for custom JS modules.
- `blazor.boot.json` inlined into `dotnet.js`; `BlazorCacheBootResources` MSBuild property removed.

Impact on the library: none in code. The library's JS module and CSS ship as RCL static web assets (`_content/{PackageId}/...`); that pipeline, `MapStaticAssets`, `ImportMap`, and `@Assets[...]` all already exist in .NET 9 and are unchanged in shape. Fingerprinting of the RCL's module continues to work through the same mechanism on both TFMs. Consumer docs should mention: on .NET 10, consumers using standalone WASM can fingerprint the library's module via `StaticWebAssetFingerprintPattern`, and Blazor Web App consumers get framework-asset preloading via `<ResourcePreloader />`.

Source: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 6a. WASM performance / trimming / AOT (SKIP)

The .NET 10 runtime what's-new (JIT devirtualization, escape analysis/stack allocation, loop inversion, Arm64 write barriers, NativeAOT preinitializer improvements) contains **nothing WASM- or ILLink-specific that a library must act on**; the headline JIT items don't apply to the WASM interpreter/AOT pipeline. No new trimming annotations or `IsTrimmable`-related requirements were found for .NET 10 affecting Blazor libraries. Blazor WASM gains diagnostics (browser dev-tools profiling, Event Pipe) and general Hot Reload (`WasmEnableHotReload`), which are app/tooling concerns. The library's existing trim-safety posture (no reflection over consumer types beyond what `InputBase` itself does) carries over unchanged. Could not verify any WASM download-size or interpreter-performance claims from official docs, so none are asserted here.

Sources:
- https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/runtime
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 6b. HttpClient streaming default (CONSUMER-DOC)

Breaking behavioral change in browser/WASM: HTTP response streaming is enabled by default; `HttpContent.ReadAsStreamAsync()` returns a `BrowserHttpReadStream` that does not support synchronous reads. Opt-outs: `<WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>`, `DOTNET_WASM_ENABLE_STREAMING_RESPONSE=false`, or per-request `SetBrowserResponseStreamingEnabled(false)`. The wrapper library does not use `HttpClient`, so this is purely a consumer note for WASM apps.

Sources:
- https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0 (networking/default-http-streaming)
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

### 7. Breaking changes / multi-target compile risk (LOW overall)

**ASP.NET Core 10 breaking-changes list** (official): cookie login redirects for API endpoints, `WithOpenApi` deprecation, exception-handler diagnostics, `IActionContextAccessor` obsolete, OpenAPI analyzers deprecated, `IPNetwork`/`KnownNetworks` obsolete, `ApiDescription.Client` deprecated, Razor runtime compilation obsolete, `WebHostBuilder` obsolete. **None touch Blazor component, forms, or JS-interop APIs** — no compile risk for this library.

Blazor-specific breaks (from release notes, all outside this library's compiled surface):
- `Router.NotFound` fragment obsolete/unsupported (4b) — demo apps only.
- `NavigationManager.NavigateTo` during static SSR: template opts into non-throwing behavior via `<BlazorDisableThrowNavigationException>`; code catching `NavigationException` or relying on `[DoesNotReturn]` semantics may need updating — CONSUMER-DOC (the library should not be catching `NavigationException`).
- `BlazorCacheBootResources` MSBuild property removed — app-level.

General .NET 10 items worth monitoring for a multi-target build (verified on the breaking-changes index, all low probability for this codebase):
- **`DefineConstants` for target frameworks not available at MSBuild evaluation time** — only matters if `.csproj`/props files branch on `$(DefineConstants)` during evaluation; `#if NET10_0_OR_GREATER` in C# is unaffected.
- **C# 14 overload resolution with span parameters** (behavioral, on recompile with SDK 10) — could theoretically pick different overloads (e.g. `MemoryExtensions` vs LINQ) when the net10.0 target compiles with C# 14; keep an eye on any `params`/span-adjacent call sites. Note the net9.0 target keeps LangVersion 13 by default, so avoid C# 14 syntax in shared files unless `LangVersion` is pinned to 14 for both targets.
- **`System.Linq.AsyncEnumerable` moved into core libraries** — source-incompatible only if the library references the `System.Linq.Async` package (it should not).
- **NU1510** (direct references pruned by NuGet raise a warning/error) and **PackageReference-without-version error** — the solution already uses Central Package Management, so the latter is moot; NU1510 may flag redundant framework-overlapping package references on the net10.0 target and is fixed by conditioning those references per-TFM.

Sources:
- https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/overview
- https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0
- https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

## Recommended Actions

1. **Multi-target `net9.0;net10.0` with zero `#if` divergence in library code.** Condition only the `Microsoft.AspNetCore.Components.Web` (and related) package references per TFM (9.x for net9.0, 10.x for net10.0) via CPM conditions — that is the entire net10 deliverable.
2. **Check test mocks** implementing `IJSRuntime`/`IJSObjectReference` compile against the net10.0 interfaces (finding 2b) — the one plausible compile snag.
3. **Consumer documentation additions**: new nested/collection validation (`AddValidation()` + `[ValidatableType]`) works with the library's form controls unchanged; `[PersistentState]` recipe for persisting bound control values; reconnection modal styling recipe; WASM HttpClient streaming default; `NavigateTo` static-SSR exception behavior; `ResourcePreloader`/fingerprinting notes for static assets.
4. **Do not adopt** JS constructor/property interop now; re-evaluate per-shim if any module function is a trivial property proxy. Re-evaluate async validation (`EditContext.ValidateAsync` and `pending`/`faulted` classes) when .NET 11 planning begins — that one *will* interact with `InputBase`-derived controls' CSS class handling.
5. **Treat net10.0 as the primary target** in CI/testing: .NET 9 (STS) left support on May 12, 2026; .NET 10 is LTS (supported to November 2028).

## Verification Notes

- "No RenderTreeBuilder / InputBase / EditContext changes" was verified directly against the official cumulative set of per-preview API diffs in `dotnet/core` (preview1 through rc2) for `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Components.Web`, and `Microsoft.JSInterop` — not inferred from release-notes silence.
- The async-validation-is-.NET-11 correction was verified against moniker ranges in the Microsoft Learn source (`::: moniker range="aspnetcore-11.0"`); several third-party posts misattribute it to .NET 10.
- Whether the new `IJSRuntime` interface members are default interface methods could not be conclusively verified from the API diff format; hence the recommendation to compile-check test doubles rather than assert safety.
- No official claim of WASM-specific trimming/AOT/download-size improvements in .NET 10 was found; that absence is reported rather than guessed around.

## Sources

- [What's new in ASP.NET Core in .NET 10 (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0)
- [Breaking changes in ASP.NET Core 10 (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/overview)
- [Breaking changes in .NET 10 (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [What's new in the .NET 10 runtime (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/runtime)
- [ASP.NET Core Blazor forms validation, aspnetcore-10.0 view (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-10.0)
- [.NET 10 API diffs, dotnet/core release notes (GitHub)](https://github.com/dotnet/core/tree/main/release-notes/10.0/preview) — files `10.0-{preview}_Microsoft.AspNetCore.Components.md`, `...Components.Web.md`, `...Microsoft.JSInterop.md`
