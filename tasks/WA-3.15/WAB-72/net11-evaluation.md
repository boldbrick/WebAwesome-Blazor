# .NET 11 evaluation: is any `#if` divergence warranted?

Re-run of the "no `#if` divergence" evaluation (`tasks\WA-3.0\WAB-7\net10-blazor-evaluation.md`) for the net11.0
target added in WAB-72. Date: 2026-11-18. SDK 11.0.100, runtime 11.0.0.

Method: the public and protected API of `Microsoft.AspNetCore.Components`, `.Components.Web`, `.Components.Forms`
and `Microsoft.JSInterop` was listed from the reference packs (`Microsoft.AspNetCore.App.Ref` 10.0.15 and 11.0.0,
read with `MetadataLoadContext`) and diffed. 112 lines differ. In addition, the whole solution was built and tested
on net11.0 (0 warnings after the two items under "Build" below, all tests green).

## Verdict

**One divergence, the owner-approved one: `WaInput.Min`/`Max` as the C# 15 union `WaInputBound`.** Nothing else in
.NET 11 needs a net11.0-only code path for the library to work. One opportunity (item 2) could justify a second
divergence later; it needs an owner decision and is not implemented.

## Findings

| # | Area | .NET 11 change (verified in the 10.0.15 -> 11.0.0 API diff) | Verdict |
|---|------|------|------|
| 1 | Forms, async validation | `EditContext.ValidateAsync(CancellationToken)`, `IsValidationPending`/`IsValidationFaulted` (all, per field, per expression), `RegisterAsyncFieldValidator`, `ValidationRequestedEventArgs.AddAsyncValidator`; `EditContext.Validate()` is now `[Obsolete("Validate is obsolete. Use ValidateAsync instead.")]` | NO DIVERGENCE. The wrappers take their field CSS classes from `InputBase<TValue>.CssClass` (`WaInputBase.GetCombinedCssClass`), so whatever field classes .NET 11 computes flow through unchanged (whether the default `FieldCssClassProvider` now adds pending/faulted classes was not verified; if it does, they reach the `wa-*` element's `class` with no code change). The library never calls `Validate()`. CONSUMER-DOC: consumers who call `editContext.Validate()` get CS0618 on net11.0 (the demo's `RegistrationFormShowcase.razor` does, but the demo targets net10.0 only) |
| 2 | Forms, labels | New `Label<TValue>` and `DisplayName<TValue>` components; new protected `InputBase<TValue>.IdAttributeValue`, which the built-in inputs render as `id` so that `<Label For>` points at them | CANDIDATE (owner decision). The wrappers do not render `id` from `IdAttributeValue` (they render `name` from `NameAttributeValue`). A net11.0-only `id` attribute would make `<Label For="() => model.X">` work with the form-associated `wa-*` controls. It would be a second `#if` divergence (the property does not exist before .NET 11), so it is reported, not implemented. Web Awesome controls have their own `label` attribute, so the gain is small |
| 3 | Forms, client validation for static SSR | `ClientValidationProvider.RenderClientValidationRules(EditContext, ...)`, `DataAnnotationsValidator.DisableClientValidation` | NO DIVERGENCE now. The client rules target native inputs; whether they reach a form-associated custom element was not verified. Re-check if static SSR forms with Web Awesome controls become a supported scenario |
| 4 | Forms, removals | Obsolete `EditContextDataAnnotationsExtensions.AddDataAnnotationsValidation`/`EnableDataAnnotationsValidation`, `RemoteBrowserFileStreamOptions` and `WebEventCallbackFactoryEventArgsExtensions` are removed | NO IMPACT. The library and its tests use none of them (the net11.0 build is clean) |
| 5 | Rendering | `RenderTreeBuilder.SetAttributeValue(int, object)` is new; nothing removed or changed | SKIP |
| 6 | JS interop | `Microsoft.JSInterop` has no public API change between 10.0.15 and 11.0.0 | SKIP |
| 7 | Components, other | Output-cache attributes (`CacheBehavior`, `CacheConditionAttribute`, `CacheVaryBy`), `EnvironmentView`, `SupplyParameterFromSession`/`SupplyParameterFromTempData`, `AssetPathAttribute`, `IComponentPropertyActivator`, `CascadingParameterSubscription`, `NavigationOptions.RelativeToCurrentUri`, `NavLink.RelativeToCurrentUri`, `Virtualize` anchoring (`AnchorMode`, `InitialItemIndex`, `ScrollToItemAsync`, `ItemComparer`); `Router.PreferExactMatches` removed | CONSUMER-DOC at most; nothing the wrappers use or wrap |
| 8 | Language | C# 15 union types | ADOPTED for `WaInput.Min`/`Max` (owner decision, WAB-72; with an `int` case so that an integer constant binds); no other wrapper parameter is a value-type union today (the `number | 'any'` step is `WaValueStep`, a struct with implicit conversions that works on every target) |

## Build (not API divergence)

- **BL0016** (new .NET 11 Blazor analyzer, "JS interop call is not guarded with a try/catch block") fires on net11.0
  only: once in the library (`WebAwesomeJSInterop.LoadModuleAsync`, the module import) and 157 times in
  `WebAwesome.Blazor.Tests`. The library suppresses it at that one call with a `#pragma` and a reason (a failed
  import must fault the module task so that every interop call reports it); the test project adds BL0016 to its
  `NoWarn`, next to BL0005. No other new warning appeared on net11.0.
- The test packages (xunit.v3 3.2.2, xunit.runner.visualstudio 3.1.5, Microsoft.NET.Test.Sdk 18.8.1, bUnit 2.7.2,
  Moq 4.20.72, PublicApiGenerator 11.5.4, AngleSharp 1.5.2) restore and run on net11.0 unchanged.

## Razor binding facts verified for the union (SDK 11.0.100)

Verified with a probe component (`[Parameter] public Bound? Min`, `union Bound(string, decimal, long, ulong,
DateOnly, TimeOnly, DateTime)`) rendered through `HtmlRenderer`, and from the generated Razor source:

- A union with a `string` case makes Razor pass a plain attribute value as a string literal:
  `Min="2026-01-01"` and `Min="5"` compile and bind the `string` case (the generated code is
  `TypeCheck<Bound?>("5")`). `Min="2.5m"` binds the string `"2.5m"`, and `Min="null"` the string `"null"`. For a union
  without a `string` case, and for an ordinary struct with an implicit conversion from `string`, Razor reads the
  value as C# instead.
- `Min="@(2.5m)"`, `Min="@(10L)"`, `Min="@(10)"` (with the `int` case), `Min="@(new DateOnly(2026, 1, 1))"` bind the matching case; `Min="@null"` and no
  attribute bind null.
- Without an `int` case, an `int` constant (`Min="@(10)"`, or `Bound b = 10;` in C#) does not compile: CS0457,
  ambiguous user-defined conversions (an integer constant converts implicitly to `long`, `ulong` and `decimal`). On the
  owner's decision `WaInputBound` therefore has an `int` case, `union WaInputBound(string, decimal, int, long, ulong,
  DateOnly, TimeOnly, DateTime)`. Re-probed with that case list on SDK 11.0.100:
  - `@(10)`, `@(-3)`, `Bound b = 10;` and `int`, `short` and `sbyte` values bind `int`;
  - `@(10L)`, `long` variables, `nint` variables and constants above the `uint` range (`5_000_000_000`) bind `long`;
    `@(10UL)` and `ulong` variables bind `ulong`; `decimal` values bind `decimal`;
  - still ambiguous (CS0457): `uint` values and constants between the `int` and `uint` maximums (`3_000_000_000`,
    `10U`; `decimal` versus `long`), and `byte`, `ushort` and `char` values (`decimal` versus `int`). No ambiguity is
    new: these were ambiguous without the `int` case too (`byte` then between `decimal` and `long`);
  - `double` and `float` do not convert (CS0029).
- The union compiles to a struct implementing `System.Runtime.CompilerServices.IUnion`, with a public
  `object? Value` and one public constructor per case; `default` holds a null `Value`. Pattern matching on a
  `Bound?` is exhaustive over the cases plus `null`, and a default union matches the `null` pattern.
- A nullable value type (`DateOnly?`, `decimal?`, `long?`, ...) does not convert to the nullable union (CS0029);
  `x is { } v ? v : null` does (target-typed conditional). A null `string` converts to a null union.
- A removed accessor name in Razor markup (`<C MinDate="@d" />`) still compiles on a component that captures
  unmatched values (as every `InputBase` wrapper does): it renders a stray, culture-formatted HTML attribute and sets
  no bound. Only a search finds those uses; the migration guide says so.
- A parameter set through a `ParameterView` dictionary needs the union value: a boxed `long` fails with
  `InvalidCastException` (Blazor does not apply the implicit conversion at run time).
- PublicApiGenerator 11.5.4 lists the union as `public struct WaInputBound : IUnion { public object? Value { get; } }`
  without the case constructors, so the net11.0 public API baseline does not pin the case list;
  `WaInputBoundUnionTests` covers each case.
