# WAB-57 — Icon library: MutatorFunction and ResolverFunction on WaJsFunction

Design: `tasks\WA-3.14\WAB-49\datagrid-cell-rendering-design.md`, decision D3 (section 15) and T7. Package:
`WebAwesome.Blazor` only. Base: WAB-54 (`WaJsFunction`, `webawesome-functions.js`) and WAB-56 (the arity tables, the
obsolete-member procedure).

## Upstream (WA 3.14.0, `dist-cdn\components\icon\library.d.ts` and the `wa-icon` source)

- `resolver: (name, family, variant, autoWidth) => string | Promise<string>`. `wa-icon` awaits the result and treats a
  throw or a rejected Promise as no URL.
- `mutator?: (svg, hostElement?) => void`. `wa-icon` calls it with the SVG and itself, after each load and each update.
- `spriteSheet?: boolean`: the resolver returns a sprite reference (`/sprite.svg#name`), which `wa-icon` renders in a
  `<use>` element. `IconLibraryOptions.SpriteSheet` already binds it.

## Scope

1. `IconLibraryOptions.ResolverFunction` and `MutatorFunction` (`WaJsFunction?`).
2. `IconLibraryOptions.Mutator` (a global path) becomes `[Obsolete]`: `WaJsFunction.Global(path)` replaces it. Removal in
   the 3.15.0 train (CHANGELOG `### Deprecated` line, the "Next-release check" note, `// TODO WA 3.15.0 train`).
   `Resolver` (the URL template) stays.
3. Validation in `WebAwesomeJSInterop.RegisterIconLibraryAsync`: exactly one of `Resolver` and `ResolverFunction`; at
   most one of `Mutator` and `MutatorFunction`. A violation throws `ArgumentException` with a clear message.
4. JavaScript: `registerIconLibrary` resolves both descriptors with `resolveFunction`; a third arity table
   `iconLibraryFunctions` (resolver 4, mutator 2) joins the Phase 2 watch list.
5. Tests (xUnit/bUnit, e2e on the free CDN), demo, `docs\technical.md`, `docs\UPGRADE-PROCESS.md`, the `/wa-upgrade`
   skill, CHANGELOG.
6. Recheck: the full e2e suite with the local Pro dist; analyze the `WaVideo` `OnEnded` case if it fails again.

## Decisions

- **Names.** `ResolverFunction` and `MutatorFunction` follow the `<Name>Function` rule (D2): `Resolver` and `Mutator`
  exist.
- **ctx.** `ctx.args` and `ctx.library` (the library name). `ctx.element` is not set: a library has no element, and the
  mutator gets the `wa-icon` as its second argument. `ctx` is the 5th argument of the resolver and the 3rd of the
  mutator, as the arity table says.
- **Promise and sprite.** The resolved function returns what the referenced function returns, so a Promise reaches
  `wa-icon` unchanged. With `SpriteSheet`, the function returns the sprite reference.
- **A reference that does not resolve.** The resolver: the registration fails (one `console.error`, then the JS call
  throws, so C# throws `InvalidOperationException`); a library without a resolver cannot draw any icon. The mutator:
  the general rule, one `console.error`, and the library is registered without a mutator. The obsolete `Mutator` keeps
  its behavior (an unknown path fails the registration).
- **`Uses`.** Neither function renders Web Awesome elements, so the tags have no effect here (documented).
- **Configuration.** The library binds no icon library from configuration (`AddWebAwesome(IConfiguration)` binds only
  `WebAwesomeOptions`). A `WaJsFunction` has no public constructor and its converter cannot read JSON, so a
  configuration binder cannot create one. An app makes the reference in code, for example
  `WaJsFunction.Registered(configuration["Icons:Resolver"])`; the factory validates the configured text. Documented on
  the properties.
- **Obsolete use in the library.** The interop reads `Mutator` for the validation under a local
  `#pragma warning disable CS0618`. The demo moves to `MutatorFunction`; the obsolete path stays in a harness page.

## Test approach

- xUnit/bUnit (`WebAwesomeJSInteropIconTests`): no resolver, both resolvers, both mutators throw `ArgumentException`
  and invoke nothing; `ResolverFunction` alone is accepted; the JSON the JS receives holds the descriptors.
- E2E on the free CDN, new harness `/testing/icon-library` and spec `icon-library-functions.spec.js`: a module
  `ResolverFunction` draws icons and gets `ctx.args` and `ctx.library`; a registered resolver that returns a Promise
  draws; a sprite resolver puts its reference into `<use>`; a module `MutatorFunction` changes the SVG and gets the host
  element; a `Global` `MutatorFunction` works; the obsolete `Mutator` still works; a resolver that does not resolve
  fails the registration; a mutator that does not resolve leaves the icon unchanged.
- The Icon page spec keeps its Tabler check, now through `MutatorFunction = WaJsFunction.Global(...)`.

## Result

- Changesets on `/main/WA-3.14/WAB-57`: cs:364 and cs:365 (plan), cs:366 (code, tests, demo, harness, e2e, API
  baseline), and the last check-in (docs, the e2e audio setting, this result).
- Public API: `IconLibraryOptions.ResolverFunction` and `MutatorFunction` (`WaJsFunction?`); `[Obsolete]` on
  `IconLibraryOptions.Mutator` (removal in the 3.15.0 train, `// TODO WA 3.15.0 train` in C# and in JavaScript).
  Baseline promoted.
- JavaScript (`webawesome-interop.js`): `registerIconLibrary` resolves `resolverFunction` and `mutatorFunction` with
  `resolveFunction` (`ctx.args`, `ctx.library`), the arity table `iconLibraryFunctions` (resolver 4, mutator 2).
  `docs\UPGRADE-PROCESS.md` and the `/wa-upgrade` skill list it on the watch list.
- Demo: the Icon page Tabler library uses `MutatorFunction = WaJsFunction.Global(...)`; a new example "Resolver &
  Mutator Functions" registers Bootstrap Icons with a collocated `IconPage.razor.js`. Harness `/testing/icon-library`
  with the local sprite `wwwroot\fragments\icon-sprite.svg`; the demo JS initializer registers `demo.iconUrlAsync`.
- Verification: Debug and Release builds green, 0 warnings. Tests: main 1384 on net9.0 and 1384 on net10.0 (5 new),
  Extensions 108 and 108, in Debug and in Release. E2E, WebAssembly demo on port 5100: free CDN 367 passed and 62
  skipped (429 tests; `minimumTests` 427); local Pro dist (`temp\wa-src\3.14.0`) 429 passed after the audio fix below.
  Pro override cleared afterwards.

## WaVideo OnEnded recheck

- The case failed again with the Pro dist (the first full Pro run: 428 passed, 1 failed).
- Probe (a plain Playwright Chromium on `/testing/events-pro`, `video.play()`, samples every 250 ms): with the
  default audio device, `play()` resolves, `readyState` is 4 and `paused` is false, but `currentTime` stays 0 for 3 s.
  With `--disable-audio-output` (a fake audio sink), the clip plays to 0.5 s and ends in about 0.6 s.
- The machine has no sound hardware; its only render endpoint is "Remote Audio" of the RDP session (session 2, where
  the agent runs). The media clock of Chromium follows the audio sink, and this sink stalls. Earlier passes on this
  machine most likely ran while the RDP audio sink delivered (it depends on the client session).
- Root cause: the e2e setup depended on the audio device of the machine (our test setup, not the wrapper and not Web
  Awesome). Fix: `playwright.config.js` launches Chromium with `--disable-audio-output`; documented in
  `tools\e2e\README.md`. With it, the full Pro suite passed (429) and the free CDN suite stays green.

## Deviations and open points

- The resolver function that does not resolve fails the registration, unlike the general "leave the member out" rule:
  a library without a resolver draws nothing, and the call returns to .NET, which can report it.
- `IconLibraryOptions` is not bound from configuration by the library; a `WaJsFunction` cannot come from configuration
  (documented in `docs\technical.md` and on the class).
- `tasks\WA-3.14\WAB-56\plan.md` names "no audio output" as the cause of the `WaVideo` failure; the device exists, but
  it stalls. Not changed here.
