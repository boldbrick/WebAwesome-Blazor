# WebAwesome.Blazor Technical Standards

The engineering standards and practices for authoring and maintaining the Web Awesome Blazor wrappers. This is the durable wrapper authoring contract; general C# style lives in `CLAUDE.md`, the upgrade pipeline and testing strategy in `docs\UPGRADE-PROCESS.md`, and contribution process in `CONTRIBUTING.md`. Task-specific plans and evaluations that shaped these standards are archived under `tasks\<epic>\<task>\`.

## Architecture principles

- **Pure C# render trees.** Wrappers are `ComponentBase` descendants implementing `BuildRenderTree` by hand — no `.razor` markup. This keeps attribute emission, event wiring, and sequence numbering under explicit control.
- **Web Awesome components are self-contained.** The upstream web components handle their own initialization, internal state, DOM manipulation, and third-party library integrations. A wrapper renders the correct tag with the correct attributes, slots, and event subscriptions — nothing more. Never re-implement behavior the custom element already provides.
- **Naming**: tag `wa-foo-bar` → class `WaFooBar`, one class per file in `src\WebAwesome.Blazor\Components\`. Web Awesome's CSS-only layout utilities (no `wa-*` element, just markup + classes) are wrapped as components too, in `src\WebAwesome.Blazor\Layouts\`. Shared infrastructure lives in `src\WebAwesome.Blazor\Base\`.
- **Completeness**: every documented attribute, event, slot, and method of the bound Web Awesome version has a wrapper counterpart (mechanically enforced — see the parity suite in `docs\UPGRADE-PROCESS.md`). Intentional deviations are recorded in `parity-config.json` with a reason, never silently.
- **Every element wrapper** exposes `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes` pass-through and captures `ElementReference? Element`.

## Render tree discipline

- **Constant sequence numbers, never `sequence++`.** Incrementing sequence numbers defeats Blazor's diffing and produces false-positive DOM changes. Number statically with gaps between sections (attributes, events, element ref, slots); gaps are fine. When a helper method builds a portion, pass it a constant base and number `sequence + 0`, `sequence + 1`, … inside.
- **Conditional emission via the shared extensions** (`Base\RenderTreeBuilderExtensions.cs`): `AddAttributeIfNotNullOrEmpty` / `AddAttributeIfNotNull` for attributes, `AddAttributeIfHasDelegate` for events. Inline `if (X.HasDelegate)` guards are reserved for cases where the emitted callback differs from the guarded one (factory-wrapped handlers) or sequence numbers increment conditionally.
- **An unset parameter emits no attribute**, so the Web Awesome default applies — wrappers never bake upstream defaults into C#.
- **Editable values use explicit bindings** in the pattern of Blazor's built-in inputs: emit `value`, an `onchange` handler created with `EventCallback.Factory.CreateBinder`, and call `builder.SetUpdatesAttributeName("value")`. This is mandatory — Blazor's change binder special-cases native `INPUT`/`SELECT` elements only (e.g. it reads `.checked` solely for native checkboxes), so custom elements get no two-way binding for free.

## Public API conventions

The public API is C# — it must feel idiomatic to .NET consumers, not mirror DOM attribute spellings or JavaScript typing:

- **Natural PascalCase word splits**, not mechanical kebab-case conversion: `MaxLength` (not `Maxlength`), `SrcDoc`, `AllowFullScreen`, `AutoFocus`, `InputMode`. Each such rename is recorded in `parity-config.json` `attributeOverrides`.
- **Strong typing throughout.** Union string types (`'a' | 'b'`) become nullable C# enums in `Components\Enums.cs` (or `Layouts\Enums.cs`) with a `ToHtmlValue()` switch-expression extension; existing enums (`WaSize`, `WaVariant`, `WaPlacement`, …) are reused whenever the value set matches, never duplicated.
- **Numbers**: properties measured in pixels or milliseconds are `int?`; `double?` only where fractional values are meaningful (e.g. animation iteration start).
- **Events**: `wa-x` → `[Parameter] EventCallback<TArgs> OnX`, with typed event args classes in `Components\EventArgs.cs` when the event carries a `detail`. Event args classes inherit `System.EventArgs` (the runtime rejects them at dispatch otherwise).
- **Element methods**: documented instance methods become `XxxAsync()` wrapper methods.
- **Slots**: named slots → `RenderFragment` parameters (`XxxContent`), the default slot → `ChildContent`.

## Event delivery contract

Binding a Web Awesome custom event requires two coordinated pieces; missing either fails silently (no build error, no runtime error, the event just never fires):

1. In the render tree, the event is bound under the **`on`-prefixed attribute name**: `builder.AddAttributeIfHasDelegate(seq, "onwa-show", OnShow)`.
2. The event name is **registered with `Blazor.registerCustomEventType`** in the JS initializer `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js` — in the plain `eventNames` list when the event's `detail` is JSON-safe and maps 1:1 onto the typed args, or with a `specialArgs` mapping when the detail carries DOM nodes or the typed args need derived values (payload shapes must stay in sync with `Components\EventArgs.cs`; deserialization is case-insensitive and ignores extra properties).

Only bind events the bound Web Awesome version actually emits (check the CEM). `EventBindingRegistrationTests` enforces both halves of the contract.

## Icon slot convenience

Components exposing an icon-shaped slot — a `start`/`end` slot documented as accepting "an element such as `wa-icon`", or a dedicated `*-icon` slot (`expand-icon`, `copy-icon`, …) — get a `<Slot>IconName` string parameter (`StartIconName`, `ExpandIconName`, …) alongside the slot's `RenderFragment`:

- Rendering goes through `RenderTreeBuilderExtensions.AddIconSlot(builder, sequence, slotName, iconName)`, which emits a `wa-icon` with a `name` attribute (and `slot` when not the default) for a non-empty name.
- **Fragment wins**: when the slot has a `RenderFragment` counterpart, the icon parameter renders only while that fragment is `null`; the parameter's doc comment states "Convenience alternative to `<FragmentName>`; ignored when the fragment is set."
- Pane-like `start`/`end` slots holding arbitrary panel content (e.g. split-panel panes) are excluded from this convention.

## Form controls

- Form-associated components inherit **`WaInputBase<TValue>`** (itself deriving from Blazor's `InputBase<TValue>`), giving typed values, `@bind-Value`, full `EditForm`/`EditContext` participation, DataAnnotations validation, and validation CSS classes. Use its `AddCommonAttributes` / `AddCommonEventHandlers` / `AddLabelAndHintSlots` helpers.
- **Constraint validation**: controls whose element supports `setCustomValidity()` implement the `WebAwesomeFormControl` interface and expose `SetCustomValidityAsync` (routed through the shared interop service), so new form controls gain custom-validity support via the common ancestor without per-component plumbing.

## JavaScript interop

The entire JS surface is deliberately two small files; there are no per-component JS modules, no `document.getElementById` lookups, and no library-generated element ids:

- **`webawesome-interop.js`** — an ES module lazily imported by the `WebAwesomeJSInterop` service (`Base\WebAwesomeJSInterop.cs`). Everything is driven by `ElementReference`: generic `invokeMethod` / `getProperty` / `setProperty`, `setCustomValidity`, and Web Awesome global helpers (icon library registration, default icon family). `JSDisconnectedException` is swallowed (returning a default where a value is expected) — circuits legitimately disconnect.
- **`WebAwesome.Blazor.lib.module.js`** — the Blazor JS initializer, auto-loaded by the runtime, whose sole job is custom-event registration (see the event delivery contract above).

Rules:

- **Interop is a last resort.** Prefer attribute binding; add an `XxxAsync` method only for element methods the CEM documents (or allowlisted in `parity-config.json` `extraElementMethods` with a reason — these allowlisted names are invisible to the CEM diff and must be re-verified against upstream source every upgrade). Never call methods that merely duplicate an attribute (`show()`/`hide()` vs. an `Open` parameter).
- Method wrappers guard a null `Element` (component not yet rendered).
- New .NET JS-interop APIs replace the module only if they can address a DOM element directly; `ElementReference` is not an `IJSObjectReference`, so the module stays (standing verdict: `tasks\WA-3.0\WAB-7\net10-blazor-evaluation.md`).

## Dependencies

All versions live in `src\Directory.Packages.props` (Central Package Management — see `CLAUDE.md`), with two opposing policies:

- **The library ships minimal floors.** NuGet dependency versions are minimums, so every package the library carries into the nupkg (currently `Microsoft.AspNetCore.Components.Web`, `Microsoft.Extensions.Configuration.Abstractions`) stays at the **base release of the target major** (`9.0.0`, `10.0.0`) — a higher floor would force consumers to upgrade their framework packages for no reason. Raise a floor only when the library genuinely needs a later API or fix, and document why at the version entry. A side effect worth keeping: the test suite then validates against the minimum versions consumers can actually resolve.
- **The test harness and tooling track latest stable.** Test-only and build-only packages (test SDK/framework, bUnit, mocking, analyzers, SourceLink, and framework packages referenced only by tests or the demo apps) are kept current — there is no consumer impact, and staying recent picks up runner fixes and security patches. Known-vulnerability warnings (NU1902) are fixed by lifting the affected package, never by suppressing the warning.

## Multi-targeting

The library targets `net9.0;net10.0` with **a single shared code path — no `#if` divergence in library code**. Multi-targeting exists for dependency hygiene (framework package references conditioned per TFM via Central Package Management), not API divergence; net10.0 (LTS) is the primary, fully tested target. `#if` conditional compilation requires a documented reason; the standing evaluation of why none is warranted is archived at `tasks\WA-3.0\WAB-7\net10-blazor-evaluation.md` and is re-evaluated per .NET release (async validation in .NET 11 is the known future interaction point).
