---
name: wa-review-apply
description: Apply a WA Inspector change list to the source code of the reviewed app. The WA Inspector browser extension exports a folder (changes.json, changes.md, page.png, item-NN.png; schema wa-inspector/change-list@1) with the changes and comments a reviewer made to the Web Awesome elements of a running page. Use this skill when the user says "apply the WA review", "apply the inspector export", "apply the change list in Downloads/wa-review/...", or gives you a folder with a changes.json of that schema. Works for WebAwesome.Blazor apps and for apps with plain Web Awesome HTML.
argument-hint: "<export folder, for example Downloads/wa-review/2026-10-20_143012>"
---

# Apply a WA Inspector change list

A reviewer changed a live page with the WA Inspector extension: attributes, CSS custom properties, part styles,
classes, inline style, theme tokens and comments. The export describes each change and where it was made. Your
job: make the source code produce the result that the reviewer wanted.

**The reviewer's intent matters most.** The values in the export are a means to show the intent, not a patch. Read
the comment and look at the screenshot before you read the values. The inspector cannot show everything: hover and
focus states, other breakpoints, the dark theme, empty and error states, other data, other pages that use the same
component. Solve these cases too, in the way the intent requires, and say in the report what you decided.

## 1. Read the export

The argument is the export folder. If the user gives no folder, use the newest folder under `Downloads/wa-review/`
in the user's home folder, and name it in your first message.

1. Read `changes.json`. It is the source of truth. `changes.md` has the same content for people.
2. Look at `page.png`. The numbered, colored outlines are the item numbers. Look at each `item-NN.png` (NN is the
   item number with two digits). An item without a screenshot matched nothing on the page at export time.
3. Make a list of the items. Group the items by the source file that you expect to change.

The schema `wa-inspector/change-list@1`:

| Field | Content |
|---|---|
| `page` | `url`, `title`, `waVersion` (from the page), `formsVersion` (the inspector metadata), `blazorMetadata` (true when the app writes `data-wablazor-*`), `themeClasses` (the `wa-*` classes on `<html>`), `viewport`, `devicePixelRatio` |
| `theme[]` | `{ token, from, to }`: theme tokens (`--wa-*`) the reviewer changed for the whole page; `from` is the theme default |
| `items[]` | `number`, `color`, `scope`, `matchCount` (matches at export), `matchCountAtCreation`, `status`, `target`, `changes[]`, `comment`, `screenshot` |
| `target` | `tag`, `component`, `chain`, `placement`, `site` (Blazor metadata of the element itself), `ownerComponent`, `ownerChain` (the nearest enclosing element with metadata), `structuralKey` (a hash), `path` (a CSS path with positions), `text` (a text snippet), `classes`, `id`, `slot`, `scopeAttributes` (CSS isolation attributes `b-xxxxxxxxxx`), `ordinal`, `url` (the page where the item was made) |
| `changes[]` | `attribute` (`name`, `operation` `set`, `remove` or `remap`, `value`, `remap`, `from` = the page value, `dataDriven`, `observed`); `cssProperty` (`name`, `value`); `partStyle` (`part`, `css`); `classes` (`add[]`, `remove[]`); `style` (`css`) |

- `from: null` means that the page did not set the attribute. In `remap` and `observed`, the key `∅` means "not set".
- `observed` counts the values of the attribute across all instances of the placement, for example
  `{ "neutral": 7, "danger": 2 }`.
- `status`: `pending` (the code does not do it yet), `applied` (the page already has the values on its own: skip
  the item and report it as already done), `unchecked` (the item has no attribute or class change that the
  inspector can compare), `missing` (no match on the exported page: the item comes from `target.url`, or the page
  changed).
- If two items change the same value on the same element, the item with the higher number wins in the browser.

## 2. Find the source

The placement and site hashes and the `structuralKey` are opaque. You cannot search for them in the source. Use
them only to understand the scope (section 3). Search with the other locator fields.

### Blazor apps with metadata (`page.blazorMetadata: true`)

- `chain` lists the application components from the render root to the element's wrapper, for example
  `MainLayout>OrderList>WaCard>WaButton`. Framework types (router, cascading values) are not in it.
- A component's parent in the chain is the component that renders it. For content in `ChildContent` or another
  `RenderFragment`, that is the component that receives the fragment (`WaCard` above), not the file that holds
  the markup. Walk the chain from the right. Skip the WebAwesome.Blazor wrappers (`Wa*`) and the components that
  only render a fragment. Search the `.razor` files (and `.razor.cs` or `.cs` files) of the remaining candidates for
  the wrapper tag (`<WaButton`) near the `text` snippet.
- An element without its own metadata (a plain element, raw `<wa-*>` markup, a slot icon) has `ownerComponent` and
  `ownerChain`: the nearest enclosing wrapper. The element is in the content of that wrapper, or the wrapper
  renders the element itself (see "Convenience icons").

### CSS isolation scope (`scopeAttributes`, for example `b-3xx5ti8xyz`)

The attribute shows that the element is markup in a `.razor` file with a `.razor.css` file. Wrapper root elements
never carry it: a wrapper renders its own element. So a scope attribute on a `wa-*` element means raw `<wa-*>`
markup in that file. To find the file:

- Search the scoped CSS output of the build: `obj/<Configuration>/<TargetFramework>/scopedcss/`. Each
  `<path>.razor.rz.scp.css` file holds the selectors of one `.razor` file with its `[b-xxxxxxxxxx]` attribute; the
  file path names the `.razor` file. The bundle `scopedcss/bundle/<Assembly>.styles.css` has the source path in a
  comment above each block. Example:
  `Get-ChildItem -Path <project>/obj -Recurse -Filter *.rz.scp.css | Select-String -SimpleMatch '[b-3xx5ti8xyz]' -List`
- If there is no `obj` folder, build the project first. A `<CssScope>` item in the project file can set the
  attribute by hand: search the project files for the value too.
- A rule in a `.razor.css` file does not match the element of a wrapper (or anything inside a child component)
  unless the selector uses `::deep` below an element of that file, for example
  `.toolbar ::deep wa-button::part(base)`.

### Other apps (no Blazor metadata)

Use `tag`, `id`, `classes`, `slot`, the `text` snippet and the `path` (for example
`main > section.orders > ul > li:nth-of-type(3) > wa-badge`). Search the templates of the app (HTML, Razor, JSX,
Vue, Svelte, and so on) for the classes and the text. Then edit the attributes and classes in the markup directly.
The rest of this skill applies too.

## 3. Map the scope to the code

| Scope | Matches | Change in the code |
|---|---|---|
| `placement` | All instances that one source position renders through this path from the root (all rows of one loop) | If the position is in the page's own markup, edit that line. If it is in a shared component that other places use too (the site count is higher, or the chain shows a shared component), the reviewer wants this usage only: add a parameter to the shared component, with a default that keeps the current result, and set it at the call site, which is the component one level up in the chain. |
| `site` | The same markup line wherever its component is used | Edit the line in the shared component. All usages change. |
| `tag` | Every element with this tag on every page | An app-wide default: a CSS rule (`wa-card { ... }`), a part rule, or a theme token in the app's theme CSS. For an attribute, change the app's own shared component for the tag if it has one; else change every usage and list the files in the report. |
| `instance` | One element of a placement group | Usually data-driven. Find what makes this instance different (the text, the data in the screenshot, the comment) and express it as a rule, for example `Variant="@(order.IsLate ? WaVariant.Danger : null)"`. If no rule fits, report the item as not done and ask. If `matchCountAtCreation` is 1, treat it as `placement`. |

Make the change at the narrowest place that gives the scope the reviewer chose. Do not change other usages of a
shared component without a reason. If you do, write the reason in the report.

## 4. Map each change to the code

### Attributes and WebAwesome.Blazor parameters

- The parameter name is the attribute name in natural PascalCase: `with-caret` is `WithCaret`, `maxlength` is
  `MaxLength`, `srcdoc` is `SrcDoc`. Verify each name on the wrapper.
- A closed value set is an enum. Select the member through its `ToHtmlValue()` mapping, not through the spelling:
  `size="s"` is `WaSize.Small`, `appearance="filled-outlined"` is `WaAppearance.OutlinedFilled`.
- A boolean attribute is a `bool` parameter: `set` is `true`, `remove` is `false` or no parameter.
- `remove`, or a value equal to the element default: delete the parameter from the markup. An unset parameter
  renders nothing, so the Web Awesome default applies. A wrapper's `Default<Name>` constants (for example
  `WaButton.DefaultVariant`) hold the element defaults; use one only where the code must state the default.
- Pixel and millisecond values are `int?`; dates and times are `DateOnly`, `TimeOnly` or `DateTimeOffset`.
- An attribute without a parameter goes through the wrapper's attribute pass-through: write it in the markup as it
  is. Report it, because it can be a gap in the wrapper.
- For raw `<wa-*>` markup, write the attribute directly.
- Sources for names and types: the `pkg-webawesome.blazor` skill if it is installed, else the package docs
  (`docs/dependencies/synced/WebAwesome.Blazor/` when DocPack synced them), the IntelliSense XML docs, or the
  wrapper source.

### Data-driven attributes and remaps

`dataDriven: true` means that the page sets different values at this placement. The code gives the parameter an
expression, not a literal. Find the mapping code: a switch expression, a helper method (`GetVariant(status)`), a
dictionary, a conditional, or a model property. Then:

- `remap` (`{ "warning": "danger" }`): change the mapping for the listed values only. Keep the other values.
  `∅` as a target means "not set" (`null`).
- `set` on a data-driven attribute: the reviewer gave one value to all instances. Read the comment. If the intent is
  one fixed value, replace the expression with the literal. If not clear, change only the value that the reviewer
  saw (`from`) and report the decision.
- The same value on all instances on the page looks like a literal (`dataDriven: false`). If the code gives the
  parameter an expression, treat the change as a remap of that value.

### Classes and layout wrappers

- On a layout wrapper (`WaStack`, `WaCluster`, `WaGrid`, `WaFlank`, `WaSplit`), map the utility classes to
  parameters: `wa-gap-l` is `Gap="GapSize.Gap_L"`, `wa-align-items-center` is `AlignItems="AlignItems.Center"`.
- `wa-flank:end` is `WaFlank Position="FlankPosition.End"` (also `FlankSize`, `ContentPercentage`).
  `wa-split:column` is `WaSplit Direction="SplitDirection.Column"`. `wa-frame:landscape` is
  `WaFrame AspectRatio="FrameAspectRatio.Landscape"`. `WaGrid` has `MinColumnSize`.
- `wa-heading-l` is `WaText Variant="TextVariant.Heading" Size="TextSize.L"`. `wa-visually-hidden-force` is
  `WaVisuallyHidden Force="true"`.
- Other classes go into the wrapper's `Class` parameter, or into the `class` attribute of plain markup.
- Keep the style of the file. Do not replace a plain `<div class="wa-stack">` with `<WaStack>` if the file does
  not use the layout wrappers.

### CSS custom properties, part styles and inline style

- `cssProperty` (the element's own CSS API, for example `--indicator-color`): add a rule in the app CSS with a
  selector for the scope: a class that you add at the call site for `placement`, the tag for `tag`. Use the
  wrapper's `Style` parameter only if the code base does so already.
- `partStyle`: a rule `<selector>::part(<part>) { ... }` in the app CSS. With CSS isolation, use `::deep` (see
  section 2). `ShadowStyles` (on some wrappers, for example `WaSelect` and `WaDataGrid`) styles the output of the
  app's own JavaScript functions only. Do not use it for the parts of Web Awesome.
- `style`: free CSS of the reviewer. Express it with the correct means: a parameter, a utility class, a theme
  token, or a rule in the app CSS. Inline style is the last choice.

### Theme tokens

`theme[]` changes `--wa-*` tokens for the whole page. Put them into the app's theme CSS: the `:root` rule or the
rule of the theme class in `page.themeClasses`. Look for an existing override file first. If the app has a dark
mode and the token has another value there, decide the dark value from the intent, and report it.

### Comments without changes

A comment alone is the full task. The screenshot shows the target. If the request needs a product decision or is
not clear, do not guess: report the item as not done, with your question.

### Convenience icons

A `wa-icon` in a wrapper often comes from a convenience parameter of the owner wrapper: `StartIconName`,
`EndIconName` or another `<Slot>IconName`. These icons have no metadata of their own; `ownerComponent` names the
wrapper. Change the parameter on the wrapper. If the wrapper has the slot fragment (for example `StartContent`),
the fragment wins over the parameter, so change the icon in the fragment.

## 5. Limits of the inspector

The export cannot express these cases. Use the comment and the screenshots.

- A data-driven value that is the same on all instances on the current page looks like a literal value.
- One instance of a loop cannot get its own literal value. The reviewer uses a comment or a value remap.
- Property-only settings (no attribute) and slot content are read-only in the inspector. A comment describes them.
- `wa-` elements inside the shadow root of another `wa-` element are internal and are not edited.
- Placements are stable for one build. A change of the Razor markup can change them. The chain, the classes and the
  text snippet stay usable in older exports.

## 6. Verify and report

1. Build the app. Fix what your changes broke. Run the tests that cover the changed components.
2. Report each item in this form:
   - `#<number> (<color>) <target>: done` and the changed files;
   - `#<number> (<color>) <target>: changed differently` and why (for example "a remap instead of a fixed
     value");
   - `#<number> (<color>) <target>: not done` and why, with the question for the reviewer.
   Add the theme tokens as one line, and list each edge case that you decided (states, breakpoints, dark mode,
   other usages).
3. Tell the user to restart or reload the app and then reload the page with the inspector open. Items whose
   attribute and class changes are now in the code show `applied` in the Changes tab; the "Remove applied items"
   button removes them. Items with CSS, part, style, remap or comment changes stay `unchecked`: the reviewer checks
   them on the page and then deletes them.
