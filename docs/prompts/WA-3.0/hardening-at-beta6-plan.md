# Hardening Plan: WebAwesome.Blazor at 3.0.0-beta.6 — "Set the Stage Before the Train"

Date: 2026-07-18 · Baseline: `/main/WA-3.0` at 3.0.0-beta.6

## 1. Objective

Put all foundational capability — packaging, demo app, changelog, onboarding ergonomics, icon sugar, repo/mirror hygiene, and the pipeline hooks that keep those artifacts alive — in place **at 3.0.0-beta.6**, so that the subsequent automated `/wa-upgrade` train (3.0.0 → 3.10.0; all zips already in `temp\download\` **except beta.6 itself**) lands each release onto an already-set stage. Every work package below therefore has two halves: the beta.6 deliverable, and the pipeline hook (`.claude\skills\wa-upgrade\SKILL.md`, agents, `docs\UPGRADE-PROCESS.md`, `tools\`) that maintains it automatically.

## 2. Verified current state (what exists, what is broken)

### Already covered — explicitly NO work needed
- **Pack capability**: `src\WebAwesome.Blazor\WebAwesome.Blazor.csproj` has `IsPackable=true` + `GeneratePackageOnBuild=True`; `src\Directory.Build.props` sets `PackageOutputPath` (`output\packages\<Configuration>\`), `PackageLicenseFile` and packs `LICENSE.md`; `Version.props` centralizes `Version` 3.0.0-beta.6, Company, Product, Copyright. A real `.nupkg` was produced and attached to the GitHub release `wa-blazor-3.0.0-beta.4`. Packing works — only metadata and push are missing.
- **CI pack-on-tag**: `.github\workflows\build.yml` builds/packs on `wa-blazor-*` tags and uploads the artifact (verified: runs exist on the mirror, last one green on the beta.4 tag).
- **Onboarding DI**: `Extensions\ServiceCollectionExtensions.cs` already has `AddWebAwesome()` registering `WebAwesomeJSInterop` + `WaIconLibraryService`. Only the options/asset-tag half is missing.
- **Test & parity infrastructure**: 139 tests, `ApiParity\` harness, `tools\upgrade\*.ps1`, `/wa-upgrade` skill + three agents — untouched by this plan except for additive hooks.
- **Out of scope, stays out**: no Extras package, no new JS (single `wwwroot\webawesome-interop.js` stays), all competitive-review P2 "reject" items stay rejected.

### Verified gaps / defects found during exploration
1. **No NuGet metadata anywhere** (grep over all `.props`/`.csproj`: zero hits for `Description|PackageTags|RepositoryUrl|PackageIcon|PackageReadme|GenerateDocumentationFile|SourceLink`).
2. **Package is NOT on nuget.org**: `nuget.org/packages/WebAwesome.Blazor` and the flat-container index both 404. "Production works" = pack + GitHub Release asset only. A push step, an org API key secret, and (owner action) reserving the `WebAwesome.Blazor` id are genuinely new work.
3. **CI never runs tests** — `build.yml` has restore/build/pack only.
4. **README.md is wrong at lines 50–51**: mixes `webawesome@3.0.0-beta.5` CSS with a `beta.6` loader on the retired `early.webawesome.com` host. Also: links `CONTRIBUTING.md` which doesn't exist (file is `CONTRIBUING.md`), says packages land in `src/output/package/<Configuration>` (actual: `output\packages\<Configuration>`), "MIT — see `LICENSE`" (file is `LICENSE.md`).
5. **`CONTRIBUING.md`** misspelled at repo root; referenced by that wrong name in `docs\UPGRADE-PROCESS.md` (Conventions section).
6. **`inputs\WebAwesome` is beta.4 content while `inputs\README.md` claims beta.6 ingestion** — confirmed stale (58 component docs; no beta.6-era pages).
7. **Parity baseline mislabeled**: `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` has `"version": "3.0.0"` and `parity-config.json` has `"targetWaVersion": "3.0.0"`, while `Version.props` is `3.0.0-beta.6`. Per the skill's Phase-2 baseline note ("only after confirming its version field equals the current version"), the very first `/wa-upgrade 3.0.0` run will find **no valid diff baseline** — or worse, silently diff 3.0.0 against itself. Must be investigated and fixed before the train runs.
8. **Public mirror `github.com/boldbrick/WebAwesome-Blazor` is stale and inconsistent**: default branch is **`master`** (with `master-WA-3.0`), while `build.yml` triggers on `main` and all internal docs say `main`; last push 2025-09-27; README on the mirror still says **beta.4** with a `web-awesome@3.0.0-beta.4` jsdelivr URL; only release/tag is `wa-blazor-3.0.0-beta.4`; description+topics are fine; no homepage URL; the internal beta.6 promotion was never mirrored.
9. **XML docs are incomplete** — e.g. most `[Parameter]` properties in `Components\WaButton.cs` (Variant, Size, Pill, Href…) have no `///` docs, and no project emits a documentation XML, so the package ships no IntelliSense.
10. **Demo app, changelog, per-component docs**: none exist (competitive review P1.4–P1.6).
11. **Demo project classification hazard**: `src\Version.props` classifies any project not containing Test/Benchmark/Mock as `Libs` + `BbForProductionUse=True` → a `WebAwesome.Blazor.Demo` project would be **packable and get `InternalsVisibleTo` behavior meant for libs**. Needs a new classification branch before the demo project is added.

## 3. Work packages (one WAB Task each, under epic WAB-1, branch `/main/WA-3.0/WAB-<n>`)

Ticket numbers are assigned by JIRA at creation; the WP letters below are stable references. All branches fork off `/main/WA-3.0` and merge back (In Progress → In Review → Done), matching `docs\UPGRADE-PROCESS.md` conventions. Standard verification for every WP: `dotnet build src/WebAwesome.slnx` (Debug **and** Release) + `dotnet test src/WebAwesome.slnx` green before check-in.

---

### WP-A — Repo hygiene, baseline integrity, and mirror sync (S/M) — **first, blocks everything**

**Goal**: truthful, consistent repo state internally and on the public mirror; a valid diff baseline for the first upgrade run.

Changes:
1. Rename `CONTRIBUING.md` → `CONTRIBUTING.md` (Plastic move); fix the reference in `docs\UPGRADE-PROCESS.md` ("Conventions honored" section). README's existing link then just works.
2. `README.md`: replace lines 50–51 with the correct beta.6 CDN snippet (derive the exact URL from the refreshed beta.6 `inputs\WebAwesome\index.md` — the docs use `{% cdnUrl %}` templating and the mirror's old `web-awesome@3.0.0-beta.4` npm name proves the package id/host changed across betas; **verify, do not guess**). Fix output-path text (`src\output\packages\<Configuration>`), `LICENSE` → `LICENSE.md`, and add an explicit licensing sentence (MIT) plus the version-mapping statement prominently. Placeholder badges (NuGet, build) — activated by WP-B.
3. **Refresh `inputs\WebAwesome` to beta.6 now** (decision: refresh, don't wait — WP-F authors demo examples from these docs at beta.6, and `inputs\README.md` currently states a falsehood). Reuse the exact procedure from SKILL.md Phase 2 (fetch `packages/webawesome/docs/docs` from the public GitHub tag `v3.0.0-beta.6`; no Pro components existed pre-3.1.0 so no webawesome.com fallback needed). Check in as its own changeset (`Web Awesome 3.0.0-beta.6 documentation added`), same as the pipeline would.
4. **Fix the parity baseline label** (defect 7): export a surface from `temp\download\webawesome_3.0.0.zip` via `tools\upgrade\Export-WaApiSurface.ps1 -Version 3.0.0` and diff it against the checked-in `expected-api-surface.json`:
   - If **identical** → the checked-in file is really 3.0.0's surface (exported ahead of time); then the beta.6→3.0.0 delta was never analyzed. Correct course: obtain/re-download `webawesome_3.0.0-beta.6.zip` and regenerate the expected surface stamped `3.0.0-beta.6`; if the beta.6 zip is unobtainable, document in this WP (and in the plan doc for the 3.0.0 run) that the parity tests are the sole worklist for the first run — the skill already sanctions that fallback.
   - If **different** → the file is beta.6 content mis-stamped; restamp `version` to `3.0.0-beta.6` and `parity-config.json.targetWaVersion` likewise.
   - Verification: `ParityDataFiles_AreWellFormed` test green; `expected-api-surface.json.version == Version.props Version`.
5. **Mirror sync + presentation** (owner-assisted where credentials are needed): push current beta.6 state to `github.com/boldbrick/WebAwesome-Blazor` (`master` + `master-WA-3.0` per the established Plastic→GitHub mapping); publish release `wa-blazor-3.0.0-beta.6` with the packed nupkg and real notes (from WP-C's changelog entry); reconcile `build.yml` **branch triggers with the actual mirrored branch names** (`main` in triggers vs `master`/`master-WA-3.0` in reality — recommend adding `master`, `master-**` to the trigger list rather than renaming history); set the repo homepage to the demo site URL once WP-F deploys it. Document the actual release flow (Plastic promote → mirror push → tag → CI pack/push) in `CONTRIBUTING.md`/`docs\UPGRADE-PROCESS.md` so it's written down, not tribal.

Pipeline hook: none beyond step 4's guarantee that Phase 2's baseline check passes. Effort: **S/M**. No dependencies.

---

### WP-M — .NET 9 + .NET 10 multi-targeting and Blazor 10 evaluation (M) — **immediately after WP-A, before the wave**

**Goal**: the library builds, tests, and packs for **both** `net9.0` and `net10.0` (both SDKs are installed); every later WP is born multi-targeted instead of retrofitted.

Changes:
1. `src\Version.props`: `TargetFramework=net9.0` → `TargetFrameworks=net9.0;net10.0` for library/tests (single-TFM stays acceptable for the Demo app — decide there); review the pinned `RuntimeFrameworkVersion` (9.0.x) — either make it conditional per TFM or drop the pin in favor of SDK defaults.
2. `src\Directory.Packages.props`: per-TFM package versions via conditional `ItemGroup`s (`Condition="'$(TargetFramework)'=='net9.0'"` → 9.0.x, `net10.0` → 10.0.x) for AspNetCore.Components/JSInterop/DI; test packages stay TFM-neutral where possible.
3. `.github\workflows\build.yml`: `actions/setup-dotnet` with both SDK versions; build/test cover both TFMs (matrix or single build of multi-TFM solution).
4. **Blazor-in-.NET-10 evaluation** (research deliverable, agent-driven): what's new in Blazor/ASP.NET Core 10 relevant to a component wrapper library (rendering APIs, `RenderTreeBuilder` changes, JS interop improvements, form integration changes); recommend per feature: adopt via `#if NET10_0_OR_GREATER`, adopt unconditionally when net9-compatible, or skip. Output: `docs\prompts\WA-3.0\net10-blazor-evaluation.md` with recommendations; actual adoption of any non-trivial feature becomes its own ticket — this WP only establishes the multi-target scaffolding and the documented decision.
5. Pipeline hook: `.claude\agents\wa-wrapper-engineer.md` + `wa-test-engineer.md` gain one line: code must compile for all TFMs in `TargetFrameworks`; conditional compilation only with a documented reason.

Verification: `dotnet build` Debug+Release and `dotnet test` green **for both TFMs**; `dotnet pack` yields a nupkg with both `lib\net9.0` and `lib\net10.0`. Effort: **M**. Depends on WP-A only.

---

### WP-B — NuGet packaging metadata + CI preparation (S) — parallel with WP-C after WP-M

**Goal**: the package is discoverable and trusted, and CI is *ready* to publish — but **nothing is pushed to nuget.org**; go-live is deliberately deferred to WP-I at the very end.

Changes:
1. `src\WebAwesome.Blazor\WebAwesome.Blazor.csproj` (keep it in the csproj — it's package-specific; `Directory.Build.props` stays generic):
   - `PackageId` (explicit `WebAwesome.Blazor`), `Title`, `Description` (mention: strongly-typed Blazor wrappers for Web Awesome, EditForm integration, MIT), `Authors`, `PackageTags` (`blazor;webawesome;web-awesome;shoelace;web-components;components;ui`), `PackageProjectUrl` + `RepositoryUrl` (`https://github.com/boldbrick/WebAwesome-Blazor`), `RepositoryType=git`, `PackageIcon` + `PackageReadmeFile` with `<None Pack>` items, `PackageReleaseNotes` pointing at the changelog URL.
   - New assets: `assets\icon.png` (128×128, **original BoldBrick/neutral art — do not use the Web Awesome logo**, per the README trademark note) and `src\WebAwesome.Blazor\PACKAGE.md` (short NuGet-front-page readme: install, `AddWebAwesome()`, `<WebAwesomeAssets/>` snippet from WP-D, link to demo/docs).
   - `GenerateDocumentationFile=true` (pairs with WP-G), `IncludeSymbols=true` + `SymbolPackageFormat=snupkg`, `PublishRepositoryUrl=true`, `EmbedUntrackedSources=true`, and `Microsoft.SourceLink.GitHub` (add to `Directory.Packages.props` per central-package-management hard rule) **conditioned on CI** (`Condition="'$(GITHUB_ACTIONS)'=='true'"`) — local packs happen from a Plastic (non-git) workspace where SourceLink would fail.
2. `.github\workflows\build.yml`:
   - Add `dotnet test` step after build (defect 3).
   - Add a tag-guard step: on `wa-blazor-*` tags, assert tag version == `Version.props` version (fail loudly instead of silently shipping a mismatched version); delete the commented-out version-override block.
   - Add a `publish` job gated on `startsWith(github.ref, 'refs/tags/wa-blazor-')` **AND a repository variable `NUGET_PUBLISH_ENABLED == 'true'` that stays unset until WP-I**: `dotnet nuget push src/output/packages/Release/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate` (+ snupkg), then create/update the GitHub Release with the matching `docs\CHANGELOG.md` section as body and the nupkg attached (`gh release create`). The job is prepared and reviewable but inert.
3. README badges: build badge goes live; NuGet badge added commented-out/placeholder (activated in WP-I).

Verification: local `dotnet pack -c Release` → inspect nupkg (icon, readme, XML doc, license present); CI dry run via `workflow_dispatch` proves build/test/pack; the publish job is verified to **skip** while the gate variable is unset. **No package leaves the building.** Pipeline hook: none. Effort: **S**.

---

### WP-C — CHANGELOG.md, seeded + pipeline-drafted (S) — parallel with WP-B

**Goal**: `docs\CHANGELOG.md` in Keep-a-Changelog form keyed to WA versions, auto-maintained by every upgrade run.

Changes:
1. Create `docs\CHANGELOG.md`, seeded retroactively from Plastic history and existing docs:
   - **3.0.0-beta.4** (2025-09-27): initial release, ~57 wrappers + 8 layout components.
   - **3.0.0-beta.6** (2026-07-18): breaking changes from `docs\MIGRATION-3.0.0-beta.6.md` and `docs\prompts\WA-3.0\upgrade-v3-beta-4-to-beta-6-plan.md` (WaIcon FA7 `FixedWidth`→`AutoWidth`/`SwapOpacity`, WaDetails `IconPosition`→`IconPlacement`, WaButtonGroup `Size` removal, …), JS-interop feature set + tests, upgrade pipeline tooling.
   - An **Unreleased** section that this hardening effort's WPs append to.
2. Section template fixed as: `## [<wa-version>] — <date>` with `### Breaking changes / ### New components / ### Changed / ### Library` subsections and a trailing link to `MIGRATION-<version>.md` when one exists.
3. **Pipeline hook** — edit `.claude\skills\wa-upgrade\SKILL.md` Phase 5: add step "Draft the `docs\CHANGELOG.md` entry for `<target>` from the Compare-WaApiSurface change report (`temp\wa-api\changes_*.md`): counts and names of added/removed/modified components, breaking changes verbatim from `breakingChanges`, link to the migration doc" — checked in with the "Additional tests and migration guide" changeset. Mirror the same sentence into `docs\UPGRADE-PROCESS.md` Phase 5 description. WP-B's GitHub-Release step then reuses the section as release notes with zero extra work per version.

Verification: markdown review; dry-run confirmation happens in WP-H. Effort: **S**.

---

### WP-D — Clean onboarding: options + `<WebAwesomeAssets/>` (M)

**Goal**: consumer setup = `AddWebAwesome()` + one root component; CDN/self-host and Pro kit code fully configuration-driven (CLAUDE.md security rules — placeholders only, never real keys).

Changes:
1. `src\WebAwesome.Blazor\Models\WebAwesomeOptions.cs`: `AssetSource` enum (`Cdn`, `SelfHosted`), `CdnBaseUrl` (default template resolved against `Version`), `Version` (default: the library's own `InformationalVersion` — the version-identity scheme makes "our version == WA version" the correct default and self-maintains on every `Version.props` bump), `BasePath` for self-hosting, `StylesheetUrls`, `LoaderUrl`, `FontAwesomeKitCode` (nullable string; docs show it coming from `builder.Configuration["WebAwesome:FontAwesomeKitCode"]`).
2. `Extensions\ServiceCollectionExtensions.cs`: add `AddWebAwesome(Action<WebAwesomeOptions> configure)` and `AddWebAwesome(IConfiguration configuration)` (binds section `WebAwesome`) overloads; register the options instance. Keep the parameterless overload exactly as-is (back-compat).
3. `Components\WebAwesomeAssets.cs` (pure `BuildRenderTree`, constant sequence numbers, no new JS): renders the `<link rel="stylesheet">` tag(s), the `<script type="module" src="…loader.js">` tag, and — when a kit code is configured — the inline module calling `setKitCode` per the WA docs pattern (see refreshed `inputs\WebAwesome\index.md`). Intended placement: `App.razor`/`<HeadContent>` for Blazor Web App / Server. **Documented limitation**: standalone WASM's `index.html` is static — for that model the README documents the equivalent static snippet (and WP-F's demo shows it working); the static snippet is the recommended path there.
4. `README.md`: rewrite Setup as the minimal path (install → `AddWebAwesome(builder.Configuration)` → `<WebAwesomeAssets/>` → first `WaButton`), with the manual-CDN snippet demoted to "advanced/self-hosted".
5. Tests: `src\WebAwesome.Blazor.Tests\Components\WebAwesomeAssetsTests.cs` — rendered markup for CDN default, explicit version, self-hosted, kit-code-present variants; options-binding tests in `Services\`.

Pipeline hook: none needed — version default flows from `Version.props`, which Phase 3 already bumps. If a future WA release changes CDN URL shape, that surfaces in the refreshed `inputs\WebAwesome\index.md` and is an ordinary upgrade change. Effort: **M**. Depends on WP-A (correct CDN facts). Parallel-safe with WP-B/C/E.

---

### WP-E — Icon convenience parameters (M)

**Goal**: `StartIconName`/`EndIconName`-style sugar rendering the `wa-icon` slot content — strictly additive over the 1:1 wrapper, fragments remain the general mechanism and win on conflict.

Changes:
1. **Audit first** (deliverable: a table in the ticket): enumerate slot names per component from `ApiParity\expected-api-surface.json` (components → slots); candidates from the beta.6 surface: `WaButton` (start/end), `WaInput` (start/end), `WaSelect` (start/end), `WaCallout` (icon), `WaDropdownItem`, `WaDetails` (expand/collapse icons), plus others where slots exist. Only add sugar where the slot's dominant real-world content is an icon.
2. Shared rendering helper in `Base\RenderTreeBuilderExtensions.cs`: `AddIconSlot(builder, seq, slotName, iconName)` emitting `<wa-icon slot="…" name="…">` (optionally `family`/`variant` overloads later — start name-only, minimal).
3. Per component: `[Parameter] public string? StartIconName { get; set; }` etc.; in `BuildRenderTree`, render the icon only when the corresponding `RenderFragment` is null (fragment precedence documented in XML docs).
4. Tests: per touched component, markup assertions for icon-only, fragment-wins, and neither.
5. **Pipeline hook**: update the wrapper authoring contract `docs\prompts\WA-3.0\build-wa-blazor-wrappers.md` and `.claude\agents\wa-wrapper-engineer.md` with the rule ("components exposing start/end/icon slots also get `<Slot>IconName` string sugar via `AddIconSlot`; fragment wins"), so every new component the train adds (combobox, date pickers, file input…) is born with the sugar. Parity tests are unaffected (extra parameters are not parity violations; no `parity-config.json` entries needed).

Effort: **M**. Depends on WP-A (surface file trustworthy). Should merge **before WP-G** (so new params get documented) and before WP-F polish (demo shows the sugar).

---

### WP-F — Demo app: WA-docs-shaped showcase + manual test bed (L)

**Goal**: `src\WebAwesome.Blazor.Demo` — standalone Blazor WASM app mirroring the original Web Awesome documentation site structure (sidebar of components, one page per component with live examples + API table), deployed to GitHub Pages, auto-extended per upgrade.

Changes:
1. **Build-system prerequisite** (defect 11): add a `<When Condition="$(MSBuildProjectName.Contains('Demo'))">` branch to `src\Version.props` classification → `BbComponentGroup=Apps`, `BbForProductionUse=False` (existing `Directory.Build.props` logic then makes it non-packable; set `IsPublishable=true` explicitly in the demo csproj if the props condition would also kill publishing — verify and override locally in the csproj).
2. Project `src\WebAwesome.Blazor.Demo\` (added to `src\WebAwesome.slnx`): `Microsoft.NET.Sdk.BlazorWebAssembly`, project-ref to the library, `Microsoft.AspNetCore.Components.WebAssembly` (+ DevServer) added to `Directory.Packages.props`.
   - `wwwroot\index.html`: WA CDN links for the current version (kept in sync by the pipeline hook below), base-href token for Pages.
   - `wwwroot\appsettings.json`: `"WebAwesome": { "FontAwesomeKitCode": "" }` placeholder — **never a real key**; Pro rendering in the public deployment simply degrades to default icons without a key (Pro-source containment: only CDN references, nothing committed).
   - `Layout\`: docs-site-style shell — sidebar grouped exactly like webawesome.com (Getting Started / Components / Layout), built with the library's own components (dogfooding: `WaStack`, `WaCluster`, etc.).
   - `Pages\Components\<Name>Page.razor` — route `/components/<kebab-name>` (mirrors `webawesome.com/docs/components/<name>`); `Pages\Layout\` for the 8 CSS layout components; `Pages\Index.razor` = getting-started page reusing the README path (doubles as an onboarding test of WP-D).
   - `Shared\ExampleSection.razor`: renders a live example beside its Razor source snippet (source embedded as a string constant in the page — no runtime compilation, keeps it static-host friendly).
   - `Shared\ApiTable.razor` + `wwwroot\data\api-surface.json`: the API reference table per page is rendered from a copy of the parity surface JSON — **this is the per-component reference documentation**, generated not hand-written, so it can never drift (design decision: fold "per-component docs" into the demo instead of a parallel `docs\components\` tree — one artifact to maintain).
3. **Skeleton generator** `tools\demo\New-WaDemoPages.ps1` (same conventions as `tools\upgrade\*.ps1`: Windows PowerShell 5.1, ASCII-only): reads a surface JSON, and for each component **without an existing page** emits `Pages\Components\<Name>Page.razor` (title, CEM description, `<ApiTable Tag="wa-x"/>`, one minimal live example synthesized from required attributes, `@* TODO: curated examples *@` marker) and inserts the nav entry. Idempotent; never overwrites hand-polished pages.
4. **Beta.6 scope**: run the generator over all 57 components + 8 layout pages; hand-polish a representative set (~12–15: Button, Input, Select, Checkbox, Dialog, Drawer, Icon, TabGroup, Card, Callout, Tooltip, RadioGroup, Details, Stack/Flank) using examples translated from the refreshed beta.6 `inputs\WebAwesome\components\*.md`; the rest ship as generated skeletons — acceptable, they still show a live default rendering plus the full API table.
5. **Hosting**: new `.github\workflows\demo.yml` — on push to default branch (and manually): `dotnet publish` the demo (Release), rewrite `<base href>` to `/WebAwesome-Blazor/`, add `.nojekyll` + `404.html` copy of `index.html` (SPA fallback), deploy via `actions/deploy-pages`. Owner action: enable Pages on the mirror; set repo homepage to the Pages URL (WP-A item 5 closes then).
6. **Pipeline hook** — edit `.claude\skills\wa-upgrade\SKILL.md`:
   - Phase 3 gains: "update the demo's `wwwroot\index.html` WA CDN version and copy `surface_<target>.json` to `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json`".
   - Phase 5 gains: "run `tools\demo\New-WaDemoPages.ps1` against the target surface — new components get skeleton demo pages (TODO-marked); removed components' pages are deleted; demo project must build". Skeleton-only automation keeps upgrade runs unblocked; curation is deliberate follow-up.
   - Mirror both into `docs\UPGRADE-PROCESS.md`.

Verification: `dotnet build` includes demo; `dotnet run` demo locally, click through polished pages (manual test-bed baptism); `pack` output **must not** contain a Demo nupkg; Pages deployment renders with working WA assets. Effort: **L** (the largest WP; the generator makes it mostly mechanical). Depends on: WP-A (beta.6 inputs, classification facts), WP-D (getting-started page), ideally WP-E (showcase sugar).

---

### WP-G — XML documentation completeness (M)

**Goal**: every non-private member documented (CLAUDE.md already mandates it; reality lags — e.g. `WaButton` parameters), documentation XML shipped in the package, future wrappers held to it mechanically.

Changes:
1. With WP-B's `GenerateDocumentationFile=true` in place, elevate CS1591 to error **for the Libs group only** (`Directory.Build.props`, condition `'$(BbComponentGroup)'=='Libs'`) — the parity-style "can't regress" guarantee.
2. Sweep all `Components\*.cs`, `Layout\*.cs`, `Base\*.cs`, `Services\*.cs`, `Models\*.cs`: fill missing `///` docs. Source of truth for parameter descriptions: the CEM doc-strings already present in `expected-api-surface.json` (attribute descriptions) — copy/adapt, don't invent. Mechanical and fan-out-able (folder-per-agent if delegated).
3. Pipeline hook: `.claude\agents\wa-wrapper-engineer.md` already writes XML docs per the authoring contract; add one sentence making CS1591-as-error the acceptance criterion so upgrade-generated components can't merge undocumented.

Verification: Release build green (CS1591 clean). Effort: **M**. Ordered **after WP-D/WP-E merge** (their new members get documented once, not raced).

---

### WP-I — NuGet publication go-live (S) — **the very last step, owner-triggered**

**Goal**: flip the switch. Nothing before this step publishes anything to nuget.org.

Steps (mostly owner actions):
1. Owner: create the nuget.org account/API key, reserve the `WebAwesome.Blazor` package id, add the `NUGET_API_KEY` secret and set the `NUGET_PUBLISH_ENABLED=true` repository variable on the mirror.
2. Activate the README NuGet badge.
3. Decide the first published version (e.g. after the train reaches 3.10.0, or earlier if desired), tag `wa-blazor-<version>`, watch the prepared WP-B publish job push package + snupkg and cut the GitHub Release.

Timing: after WP-H, likely after the 3.0.0→3.10.0 train — explicitly **not** part of the hardening wave. Effort: **S**.

---

### WP-H — Dress rehearsal: pipeline coherence + `--dry-run` gate (S) — **last before the train**

**Goal**: prove the hardened stage works with the pipeline before the real train departs.

Changes/steps:
1. Re-read the modified `.claude\skills\wa-upgrade\SKILL.md` end-to-end after WP-C/E/F/G edits — the three hooks (changelog draft, demo skeletons + CDN bump, doc-strictness) must sit in the correct phases and not contradict the check-in comment scheme; sync `docs\UPGRADE-PROCESS.md` "Pipeline phases" table/text.
2. Run `/wa-upgrade 3.0.0 --dry-run` from a clean `/main/WA-3.0`: verifies preflight, ticket/branch idempotence, the WP-A-repaired baseline (surface version now matches `Version.props` — the compare step must produce a real beta.6→3.0.0 change report, or the documented parity-only fallback engages cleanly), inputs refresh, and plan-document generation. The dry run's ticket/branch are then reused by the real 3.0.0 run — by design.
3. Fix whatever the rehearsal exposes; check fixes in on the WP-H branch.

Effort: **S** (plus fix fallout). Depends on all other WPs merged.

## 4. Execution order and parallelism

```
WP-A (hygiene/baseline/mirror)          -- first, alone
   +--> WP-M (net9+net10 multi-targeting, Blazor 10 eval) -- second, alone (touches the whole build system)
          +--> WP-B (packaging prep, no publish) \
          +--> WP-C (changelog)                   +-- parallel wave 1 (disjoint files)
          +--> WP-D (onboarding)                  |
          +--> WP-E (icon sugar)                 /
                 +--> WP-G (XML docs)  -- after D+E merged (documents their new members; serialize)
                 +--> WP-F (demo)      -- after D (+E preferred) merged; can overlap WP-G
                        +--> WP-H (dry-run rehearsal) -- last before the train
                               +--> [3.0.0 -> 3.10.0 upgrade train]
                                      +--> WP-I (NuGet publication go-live, owner-triggered)
```

Owner-side actions to schedule (not agent work): mirror push credentials + GitHub Pages enablement (WP-A/WP-F), optional re-download of `webawesome_3.0.0-beta.6.zip` (WP-A item 4), icon artwork approval (WP-B), nuget.org account/API key/package-id reservation + gate variable (WP-I, at the very end).

## 5. Definition of "done, ready for the 3.0.0 upgrade run"

- `/main/WA-3.0` green (Debug+Release build **for net9.0 and net10.0**, full test suite incl. new WebAwesomeAssets/icon-sugar tests, CS1591-clean).
- `expected-api-surface.json.version` == `Version.props` version, so `/wa-upgrade 3.0.0` gets a valid baseline (verified by the WP-H dry run producing a real plan document).
- `dotnet pack` yields a nupkg with `lib\net9.0` + `lib\net10.0`, icon, readme, tags, license, XML docs, snupkg; CI on a `wa-blazor-*` tag builds, **tests**, packs, and has an inert, reviewed publish job awaiting WP-I's gate variable — **nothing is pushed to nuget.org before WP-I**.
- Public mirror shows beta.6: synced branches, corrected README (single correct beta.6 CDN snippet, badges, MIT statement), `CONTRIBUTING.md` spelled right, `wa-blazor-3.0.0-beta.6` release published, homepage → live GitHub Pages demo.
- Demo app deployed: every component has at least a generated page (live default + API table), ~12–15 curated; `tools\demo\New-WaDemoPages.ps1` proven idempotent.
- `docs\CHANGELOG.md` seeded (beta.4, beta.6); SKILL.md + UPGRADE-PROCESS.md contain the three maintenance hooks (changelog entry, demo skeletons + CDN/version bump + surface copy, XML-doc gate) so every subsequent automated upgrade extends changelog, demo, and docs without human touch.
- All WAB tickets Done under WAB-1; workspace clean on `/main/WA-3.0`. Then: `/wa-upgrade next --publish`, eleven times.
