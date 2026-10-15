# WaDataGrid: rich cell content from C#

| | |
|---|---|
| Status | Draft for owner review. No code is written. |
| Date | 2026-10-15 |
| Trigger | NLAI question Q11: how do we show state badges in grids? |
| Baseline | WebAwesome.Blazor at WA 3.14.0 (`/main` cs:331) |
| Scope | `WaDataGrid`, `WaDataGridColumn`, `webawesome-interop.js`, `WebAwesome.Blazor.lib.module.js` |

## 1. Summary

Today, every `WaDataGrid` cell shows its raw value. Upstream renders rich cells with JavaScript
functions (`formatter` and similar column members). A JavaScript function cannot cross the JS
interop boundary, so the wrapper does not expose these members.

This design adds rich cells in three layers:

1. **Presets.** The library has a catalog of built-in cell renderers. The developer configures them
   in C# only, for example a badge from a value-to-variant map, a localized number, or an actions
   column. No JavaScript is necessary.
2. **JS function references.** This is the "escape hatch". The developer writes a small JavaScript
   function and binds it to a column from C#, by name or by module export. The library resolves the
   reference and gives the function a helper context.
3. **The event bridge.** Cell content, from a preset or from a custom function, can raise events.
   The events arrive in .NET as one typed callback, `WaDataGrid.OnCellAction`.

The same reference mechanism also covers the other function-valued members of the grid:
`cellClass`, `rowClass`, `footer`, `comparator`, `filterFn`, `searchFn`, `rowDetail`, and so on.

## 2. Context and verified facts

| # | Fact | Source |
|---|---|---|
| F1 | The wrapper leaves out `formatter`, `aggregatedFormatter`, `value`, `filterFn`, `comparator`, and the function forms of `footer`, `aggregation` and `cellClass`. | `src/WebAwesome.Blazor/Models/WaDataGridColumn.cs:9-16` |
| F2 | The wrapper pushes `data` and `columns` with `SetPropertyAsync` on the first render and on each `OnParametersSetAsync`. | `src/WebAwesome.Blazor/Components/WaDataGrid.cs:471-490, 798-804` |
| F3 | A formatter has the signature `(value, row) => string \| TemplateResult \| Node`. It runs on each render. A new `Node` replaces the old one. A reused `Node` keeps its DOM state. | `temp/wa-src/3.14.0/dist-cdn/components/data-grid/data-grid.d.ts:115` |
| F4 | CSV export and copy use a formatter only when it returns a string. For a `TemplateResult` or a `Node`, they use the raw value. | `data-grid.md:316` |
| F5 | Each data cell has an `@click` handler that dispatches `wa-cell-click` with `column`, `value`, `row` and `rowIndex`. A click on any element inside the cell bubbles to this handler. The dispatch is synchronous. | `chunk.6ELXLNTM.js` (3.14.0), cell template |
| F6 | Cell content is in the shadow root of the grid. The autoloader does not see it, so a component that only a formatter renders can stay undefined. | `data-grid.md:2000-2018` |
| F7 | The Web Awesome entry point exports `discover(root)`. It registers each undefined `wa-*` element under `root`, and each tag in a `data-wa-preload` attribute. `root` can be a `ShadowRoot` or a detached element. | `dist-cdn/utilities/autoloader.d.ts`, `discover` in the chunks |
| F8 | Page CSS does not reach the cell content. To style it, adopt a stylesheet into the shadow root. `rowClass` and the function form of `cellClass` need this. | `data-grid.md:2020-2057`, `data-grid.md:281` |
| F9 | Other function-valued grid members: `rowClass`, `rowDetail`, `searchFn`, `selectableRows`, `childRows` (function form), `dataSource`. | `data-grid.md:259-289` |
| F10 | `webawesome-interop.js` already resolves a JavaScript function from a global dotted path (`resolveGlobalFunction`) for the icon library mutator. It also imports the Web Awesome entry point of the page (`importWebAwesome`). | `src/WebAwesome.Blazor/wwwroot/webawesome-interop.js:198-301` |
| F11 | The initializer registers each `wa-*` event with `registerCustomEventType`. It already relays events under private `wablazor-*` names, with the `Symbol.for` pattern for shared state. | `WebAwesome.Blazor.lib.module.js` |
| F12 | `WaDataGridColumn` does not expose `filterOptions` (new in 3.14). This design uses it for lookup labels (section 7.4). | `data-grid.d.ts:74`, `WaDataGridColumn.cs` |

### 2.1 Why a Razor `RenderFragment` cell template is not possible

- Blazor cannot render into the shadow root of the grid. Lit owns that DOM.
- If Blazor renders nodes elsewhere and the formatter moves them into a cell, the Blazor renderer
  breaks. Blazor must be the only owner of its nodes. The toast ownership guard in
  `lib.module.js` exists because of the same class of defect.
- The grid creates and removes cells when it virtualizes, sorts, filters and pages. A Blazor
  component per cell cannot follow that life cycle.

Section 10 examines a related option that does work: Razor markup rendered to static HTML.

## 3. Goals and non-goals

Goals:

- Rich cells (badges, tags, icons, progress, links, buttons, menus) with C# configuration only.
- A supported way to bind custom JavaScript, with a small API and a good development experience.
- Events from cell content into .NET, for presets and for custom JavaScript.
- Correct sort, filter, search and CSV behavior. A formatter changes only what the cell shows.
- No new dependencies. Specifically, no Lit dependency in the library.

Non-goals:

- An inline editing model (edit mode, validation, commit and cancel). Upstream has no editing
  model. The bridge makes simple edits possible (section 9.6), but a full model is out of scope.
- Inline JavaScript text in C# (`new Function`, `eval`). Section 11.6 gives the reasons.
- Workarounds for upstream defects. Record them and check again at the next upgrade.

## 4. Architecture

```
 C# (WaDataGridColumn, WaDataGrid)            webawesome-interop.js                     wa-data-grid
 ─────────────────────────────────            ─────────────────────                     ────────────
 Formatter = WaDataGridFormatter.Badge(..) ┐
 Formatter = WaDataGridFormatter.Custom(   ├─► JSON descriptor ─► setDataGridColumns() ─► columns[i].formatter = fn
             WaJsFunction.Module(..))      │                       • resolve descriptors        │
 CellClassRules, Comparator, Footer, ...   ┘                       • import modules (cached)    │ renders cells
                                                                   • preload tags (discover)    ▼
 ShadowStyles, FormatterCulture,  ──────────────────────────────► adopt stylesheet, ctx   cell content (shadow DOM)
 FormatterContext                                                                               │ click, change, ctx.emit
                                                                                                ▼
 OnCellAction(WaDataGridCellActionEventArgs) ◄── registerCustomEventType ◄── 'wablazor-cell-action' on the grid host
```

Parts and their homes:

| Part | Home | Job |
|---|---|---|
| Descriptor model | C# (`Models/`) | Typed presets and function references. They serialize to JSON descriptors. |
| Column resolver | `webawesome-interop.js`, new `setDataGridColumns(element, columns, options)` | Replaces descriptors with real functions. Then it assigns `columns`. |
| Preset library | New module `wwwroot/webawesome-datagrid.js`, imported by the interop module | Builds the cell content of each preset with DOM APIs. |
| Function registry | `globalThis[Symbol.for('WebAwesome.Blazor.dataGrid')]` and the public module `webawesome-datagrid.js` | Holds custom functions that the app registers by name. |
| Event bridge | `webawesome-datagrid.js` (dispatch), `lib.module.js` (registration) | Turns cell interactions into one `wablazor-cell-action` event. |
| Shadow styles | `setDataGridColumns` options | Adopts a CSS text into the shadow root of the grid. |

Note: the first answer to Q11 put the preset code in the JS initializer. The initializer only
registers event types. The column push path is in the interop module, so the resolver goes there.

## 5. Wire format and resolution

### 5.1 Descriptors

Each function-valued member serializes to a JSON object with a reserved marker key. The resolver
looks for the marker only on the known function-valued keys. Data values are never examined.

```jsonc
// a preset
{ "field": "state", "formatter": { "$wab": "preset", "name": "badge",
  "options": { "variants": { "active": "success", "failed": "danger" }, "fallback": "neutral" } } }

// a custom function
{ "field": "state", "formatter": { "$wab": "fn",
  "ref": { "kind": "module", "url": "./Pages/Orders.razor.js", "export": "stateCell" },
  "args": { "showIcon": true }, "uses": ["wa-badge", "wa-icon"] } }
```

Known keys for a column: `formatter`, `aggregatedFormatter`, `footer`, `cellClass`, `value`,
`comparator`, `filterFn`, `aggregation`. Known keys for the grid: `rowClass`, `rowDetail`, `searchFn`,
`selectableRows`.

### 5.2 Resolution steps

`setDataGridColumns(element, columns, options)` does these steps:

1. Collect the module URLs of all references. Import each URL once. Keep the import promise in a
   cache that the URL identifies.
2. Collect the tags that the descriptors use. Presets know their tags. A custom reference declares
   its tags in `uses`. Register them with `discover()` on a detached element with
   `data-wa-preload` (F7).
3. Replace each descriptor with a wrapper function. The wrapper calls the preset or the custom
   function with a context object (section 8.4).
4. Adopt the shadow stylesheet if it changed (section 11.2).
5. Assign `element.columns`.
6. After `element.updateComplete`, call `discover(element.shadowRoot)`. This step registers tags
   that no descriptor declared. An element that is defined later upgrades in place, so the cell
   shows correctly without a second render.

If a reference does not resolve, the resolver writes one `console.error` that names the column and
the reference. The column then shows raw values. The grid continues to operate.

### 5.3 Re-push behavior

Today, each `OnParametersSetAsync` pushes `data` and `columns` again (F2). With descriptors, each
push creates new formatter functions, so the grid renders all cells again. The design changes this:

- The wrapper pushes `columns` only when the `Columns` reference changes, or when the serialized
  JSON changes. A hash of the JSON is enough.
- The resolver keeps the resolved columns for each grid in a `WeakMap`, keyed by the JSON text.

This change is useful without presets too. It is a small fix that can go first.

## 6. Preset catalog

### 6.1 Output types and their effects

| Output | CSV and copy | DOM state | Cost |
|---|---|---|---|
| String | Uses the formatted text | None | Lowest |
| `Node` | Uses the raw value (F4) | Kept when the preset reuses the node | Higher |

Rule: a preset returns a string when the result is only text. A preset returns a `Node` only when
it needs an element.

Node cache: Node presets keep the created node in a `WeakMap` keyed by the row object, then by the
column id. If the value did not change, the preset returns the same node. This keeps the focus and
the open state of interactive content (F3). New `Data` from .NET creates new row objects, so the
cache clears itself.

Common options of all presets:

- `NullText`: the text for `null`, `undefined` and an empty string. The default is an empty cell.
- `Culture`: overrides the grid-level `FormatterCulture` (section 11.3).

### 6.2 Text and value presets (output: string)

| Preset | Options | WA counterpart | Prior art |
|---|---|---|---|
| `Number` | `Style` (decimal, percent, currency, unit), `Currency`, `CurrencyField`, `Unit`, `MinFractionDigits`, `MaxFractionDigits`, `UseGrouping`, `Notation` (standard, compact), `SignDisplay` | `wa-format-number` (same `Intl.NumberFormat` model) | AG Grid `valueFormatter`, Kendo `{0:c}`, Tabulator `money` |
| `Currency` | Shortcut of `Number` with `Style = currency`. `CurrencyField` reads the currency code from the row. | `wa-format-number type="currency"` | All grids |
| `Percent` | `Scale` (fraction 0–1 or percent 0–100), digits | `wa-format-number type="percent"` | DevExpress, Syncfusion |
| `Date`, `DateTime`, `Time` | `DateStyle`, `TimeStyle`, or explicit parts. `TimeZone` (IANA id, for example `UTC` or `Europe/Prague`) | `wa-format-date` | MUI `date`, `dateTime` column types |
| `RelativeTime` | `Numeric` (always, auto), `Style`. Text is computed at render time. | `wa-relative-time` (the Node variant can use `sync`) | Tabulator `datetimediff` |
| `Bytes` | `Unit` (byte, bit), `Display` (long, short, narrow) | `wa-format-bytes` | Common in file grids |
| `Duration` | `Input` (seconds, milliseconds, ISO 8601), `Style` (long, short, narrow, digital) | None. Uses `Intl.DurationFormat` with a simple fallback. | Monitoring grids |
| `Lookup` | `Labels` (value → display text), `Fallback` | None | MUI `singleSelect`, DevExpress lookup, Tabulator `lookup` |
| `Boolean` | `TrueText`, `FalseText`, `NullText` | None | Most grids |
| `Template` | A text template with row placeholders: `"{firstName} {lastName}"`. A placeholder can name a format: `"{amount:currency}"`. | None | Kendo templates |
| `List` | `Separator`. Joins an array value. | None | Tabulator `array` |

Lookup and localization: `Labels` come from C#, so .NET resources can supply the localized text.
With `Lookup`, the cell shows the label but the value stays the code. Section 7.4 explains how
search and the filter picker can use the labels too.

### 6.3 Visual presets (output: Node)

| Preset | Options | WA component | Prior art |
|---|---|---|---|
| `Badge` | `Variants` (value → `WaVariant`), `Fallback`, `Appearance`, `Pill`, `Labels`, `Icons` (value → icon name) | `wa-badge` | MUI chips, Tabulator `traffic` |
| `Tag`, `Tags` | Same maps as `Badge`. `Tags` renders one tag per array item. `Size`. | `wa-tag` | Chips columns in MUI and AG Grid |
| `Icon` | `Icons` (value → name, family, variant), `Colors` (value → `WaVariant`), `Labels` (accessible text) | `wa-icon` | Excel icon sets, DevExpress |
| `BooleanIcon` | Shortcut of `Icon`: check and cross icons, optional colors, accessible labels | `wa-icon` | Tabulator `tickCross`, AG Grid checkbox renderer |
| `ProgressBar` | `Scale` (0–1 or 0–100), `ShowLabel`, `Variants` by threshold | `wa-progress-bar` | DevExpress progress column, Tabulator `progress` |
| `ProgressRing` | Same as `ProgressBar`, compact | `wa-progress-ring` | |
| `Rating` | `Max`, `Precision`. Always read-only. | `wa-rating readonly` | Tabulator `star` |
| `Avatar` | `ImageField`, `InitialsField`, `LabelField`, `Shape`, `ShowLabel` (avatar and name side by side) | `wa-avatar` | "Person" cells in AG Grid and MUI samples |
| `Image` | `SrcTemplate` or `SrcField`, `AltField`, `Width`, `Height` | `img` | Tabulator `image`, DevExpress image column |
| `ColorSwatch` | `ShowText` | Inline `span` | Tabulator `color` |
| `Sparkline` | `Appearance`, `Curve`, `Trend`, `TrendField`. The value is a number array. | `wa-sparkline` (`data` is space-separated, `label`) | AG Grid sparklines, DevExpress |
| `Trend` | `Format` (a number preset), `PositiveVariant`, `NegativeVariant`, `Icons` | `wa-icon` and text | Financial grids |
| `DataBar` | `Min`, `Max` (fixed, or computed from the loaded rows), `Variant`, `ShowValue` | Inline styled `div` | Excel and DevExpress data bars |
| `CopyButton` | `ShowValue`, `CopyLabel`, `SuccessLabel` | `wa-copy-button` | Common in id and key columns |
| `QrCode` | `Size` | `wa-qr-code` | Rare. Include only on request. |

All visual presets set an accessible name. For example, `Icon` uses the `label` attribute of
`wa-icon`, and `DataBar` sets `role="img"` and an `aria-label` with the formatted value.

### 6.4 Interactive presets (output: Node, raise `OnCellAction`)

| Preset | Options | Action raised | WA component | Prior art |
|---|---|---|---|---|
| `Link` | `HrefTemplate` (`"/orders/{id}"`), `TextTemplate`, `Target`. Or `Action` instead of `HrefTemplate`. | None for `HrefTemplate`. `Action` for the action form. | `a` | Tabulator `link`, DevExpress hyperlink column |
| `Email`, `Phone` | Shortcuts of `Link` with `mailto:` and `tel:` | None | `a` | Tabulator `link` with `urlPrefix` |
| `Button` | `Action`, `Label`, `Icon`, `Variant`, `Appearance`, `Size`, `DisabledField`, `HiddenField` | `Action` | `wa-button` | Kendo command column |
| `Actions` | A list of `WaDataGridAction` (`Id`, `Label`, `Icon`, `Variant`, `Tooltip`, `DisabledField`, `HiddenField`, `InMenu`) | The `Id` of the action | `wa-button`, `wa-tooltip`, `wa-dropdown`, `wa-dropdown-item` | MUI `GridActionsCellItem` with `showInMenu` |
| `Menu` | Same items as `Actions`, all in one dropdown with a kebab button | The `Id` of the item | `wa-dropdown` | MUI, AG Grid context menus |
| `Switch`, `Checkbox` | `Action` (default `"toggle"`), `DisabledField`, `Label` (accessible) | `Action` with the payload `{ "checked": bool }` | `wa-switch`, `wa-checkbox` | AG Grid checkbox renderer, Tabulator `toggle` |

Notes:

- `HiddenField` and `DisabledField` name a boolean field in the row. The row decides per row if an
  action shows. Example: "Delete" only for rows where `canDelete` is true.
- `Link` with `HrefTemplate` is a real anchor. URL-encode each placeholder. Reject a template that
  resolves to a scheme other than `http`, `https`, `mailto`, `tel`, or a relative URL.
- Blazor enhanced navigation intercepts anchor clicks at the document. The click on an anchor in a
  shadow root is retargeted to the grid host. It is not verified that Blazor finds the anchor
  through `composedPath()`. If it does not, the link does a full page load. The link works in both
  cases. Verify this in the prototype (risk R3).

## 7. Other function-valued members as presets

A badge column is often not enough. Grids also need conditional styles, footers and sort orders.
These presets use the same descriptor mechanism.

### 7.1 Conditional classes (`cellClass`, `rowClass`)

`WaDataGridColumn.CellClassRules` and `WaDataGrid.RowClassRules` take a list of rules. The first
rule that matches gives the class. A rule has a condition and a class:

| Condition | Meaning |
|---|---|
| `Equals(value)`, `In(values)` | Exact match on the value (or on a field for row rules) |
| `LessThan`, `GreaterThan`, `Between` | Numeric or date comparison |
| `IsNull`, `IsTrue`, `IsFalse` | State checks |
| `Matches(pattern)` | A regular expression, for text |

Classes need styles in the shadow root (F8). `WaDataGrid.ShadowStyles` supplies them (section 11.2).
Prior art: AG Grid `cellClassRules` and `rowClassRules`, Excel conditional formats.

### 7.2 Footers

`WaDataGridColumn.Footer` today accepts static text only. Add `FooterAggregate`:
`Sum`, `Average`, `Min`, `Max`, `Count`, each with an optional text preset (for example `Currency`)
and an optional prefix label. The function runs on the filtered rows over all pages, as upstream
specifies.

### 7.3 Sort order

`WaDataGridColumn.Comparator` presets:

- `Ordinal(values)`: sort by the position in a list. Example: `New < Active < Done`. Badge and
  lookup columns need this, because alphabetic order of state codes is rarely correct.
- `Collation(culture)`: sort text with `Intl.Collator` for a culture. The default `alphanumeric` sort
  does not sort Czech correctly. For example, "ch" sorts after "h" in Czech.

### 7.4 Search and filters on display text

With a `Lookup` or a `Badge` with `Labels`, the cell shows a label but the value is a code. The
global search compares the code, so a search for "Active" does not find "ACT". Two additions:

- `WaDataGridColumn.SearchFormatted = true`. The resolver builds a grid-level `searchFn` that
  compares the search term with the label of such columns. For other columns, it uses the default
  comparison.
- `WaDataGridColumn.FilterOptions` (F12). The `set` filter picker shows the labels. When a lookup
  preset exists and `FilterOptions` is not set, the wrapper can generate it from the labels.

### 7.5 Other grid-level members

| Member | Preset | Custom function |
|---|---|---|
| `selectableRows` | `SelectableField = "canSelect"` | Yes |
| `rowDetail` | `Fields`: a list of label and field pairs, with text presets, shown as a description list | Yes |
| `childRows` (function form) | None | Yes |
| `value` (computed value) | `Template` (the computed text also drives sort and search) | Yes |
| `filterFn` | None | Yes |
| `dataSource` | Not applicable. Server mode with `OnDataRequest` already covers it. | No |

## 8. The escape hatch: custom JavaScript functions

### 8.1 C# reference type

One type refers to a JavaScript function. It is not specific to the grid, so other wrappers can
use it later (for example the icon library mutator).

```csharp
public sealed record WaJsFunction
{
    // a function that the app registered by name (section 8.2)
    public static WaJsFunction Registered(string name);

    // an export of an ES module; the URL is relative to the document base
    public static WaJsFunction Module(string url, string exportName = "default");

    // a function on a global dotted path, as for the icon library mutator
    public static WaJsFunction Global(string path);

    // JSON-serializable options that the function receives as ctx.args
    public WaJsFunction WithArgs(object args);

    // the wa-* tags the function renders, so the library registers them before the first render
    public WaJsFunction Uses(params string[] tags);
}
```

Usage on a column:

```csharp
new WaDataGridColumn
{
    Field = "state",
    Label = "State",
    Formatter = WaDataGridFormatter.Custom(
        WaJsFunction.Module("./Pages/Orders.razor.js", "stateCell")
            .WithArgs(new { showIcon = true })
            .Uses("wa-badge", "wa-icon")),
}
```

### 8.2 Binding modes

| Mode | How the app supplies the function | Advantages | Disadvantages |
|---|---|---|---|
| `Module` | An export of an ES module. A collocated `Orders.razor.js` file next to `Orders.razor` works directly: Blazor serves it at `./Pages/Orders.razor.js`, or at `./_content/<Rcl>/Pages/Orders.razor.js` in a Razor class library. | No globals. The JS is next to the page that uses it. No start-up code. | The URL is a string. A rename of the file breaks it at run time, not at build time. |
| `Registered` | The app calls `registerCellFunction('nlai.state', fn)` once, for example in its own JS initializer `wwwroot/<App>.lib.module.js` (`afterWebStarted`). | Short names. One function is shared by many pages. | Needs start-up code. The name must be registered before the grid renders. |
| `Global` | A function on `window`, for example `window.nlai.grid.state` | No module system | Pollutes the global scope. Kept for parity with the icon mutator. |

Recommendation: document `Module` with collocated files as the main path. It has the best
development experience in Blazor. Offer `Registered` for shared functions in a component library.

Registry details: the registry lives on `globalThis[Symbol.for('WebAwesome.Blazor.dataGrid')]`.
The public module `_content/WebAwesome.Blazor/webawesome-datagrid.js` exports helpers that write to
it. Two copies of the module (for example with different base paths) still share one registry. This
uses the same pattern as the relay and the ownership guard in `lib.module.js`.

If the grid renders before a `Registered` name exists, the resolver waits a short time for the
registration (one animation frame, then up to a fixed limit). Then it reports the error of
section 5.2.

### 8.3 JavaScript signature

```js
// formatter, aggregatedFormatter, footer, rowDetail
(value, row, ctx) => string | Node | TemplateResult
// cellClass, rowClass
(value, row, ctx) => string | null
// comparator
(a, b, rowA, rowB, ctx) => number
// filterFn, searchFn
(value, term, row, ctx) => boolean
```

Upstream calls the wrapper. The wrapper adds `ctx` as the last argument. Upstream arguments keep
their order, so an upstream example works without changes.

### 8.4 The context object (`ctx`)

| Member | Purpose |
|---|---|
| `ctx.args` | The `WithArgs` object from C# |
| `ctx.column` | The column id |
| `ctx.grid` | The `wa-data-grid` element |
| `ctx.culture` | The effective culture name (section 11.3) |
| `ctx.context` | The grid-level `FormatterContext` object from C#, for example lookup tables or user permissions |
| `ctx.rowKey` | The key of the row, from the `row-key` path |
| `ctx.emit(action, payload?)` | Raises `OnCellAction` for this cell (section 9) |
| `ctx.h(tag, props, ...children)` | A small DOM builder. `props` sets properties and attributes. `on*` keys add listeners. `action: 'edit'` adds a click that calls `ctx.emit('edit')`. Children are nodes or text. Text is always set with `textContent`. |
| `ctx.preset(name, options)` | Calls a built-in preset, so a custom cell can combine presets. Example: an avatar next to a badge. |
| `ctx.format.number(value, options)`, `ctx.format.date(value, options)`, ... | The text presets as plain functions, with the grid culture |
| `ctx.cache(key, create)` | Returns a node from the cache of this cell (section 6.1) or creates it with `create()` |

`ctx` exists once for each call, because `emit` must know the row. The object is small. Measure
the cost in the prototype with a virtualized grid of 10,000 rows.

### 8.5 Lit or no Lit

- The library does not depend on Lit. The Web Awesome entry point does not export `html`.
- A developer who wants Lit imports it (`lit@3`) in the module and returns a `TemplateResult`.
  The grid renders it. This assumes that a template from a separate copy of Lit 3 renders in the
  Lit of the grid. Verify this in the prototype (risk R2).
- A developer without Lit uses `ctx.h` and returns a `Node`.

### 8.6 Examples

A collocated module, without Lit:

```js
// Pages/Orders.razor.js
export function stateCell(value, row, ctx) {
  return ctx.cache(value, () => ctx.h('span', { style: 'display:inline-flex;gap:.25em' },
    ctx.preset('badge', { variants: ctx.context.stateVariants, value }),
    row.overdue ? ctx.h('wa-icon', { name: 'clock', label: 'Overdue' }) : null));
}

export function orderActions(_value, row, ctx) {
  return ctx.h('span', {},
    ctx.h('wa-button', { size: 's', appearance: 'plain', action: 'open' }, 'Open'),
    row.canCancel ? ctx.h('wa-button', { size: 's', variant: 'danger', action: 'cancel' }, 'Cancel') : null);
}
```

The same with Lit:

```js
import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

export const orderActions = (_value, row, ctx) => html`
  <wa-button size="s" appearance="plain" @click=${() => ctx.emit('open')}>Open</wa-button>
  <wa-button size="s" variant="danger" data-wablazor-action="cancel">Cancel</wa-button>`;
```

The second button uses the declarative attribute of section 9.3. Both forms raise the same event.

## 9. The event bridge

### 9.1 The problem

Cell content runs in the browser, in the shadow root of the grid. Blazor sees only the events that
it registered and that reach the document. Today, the only cell event is `wa-cell-click`. It does
not tell which element in the cell got the click. It also does not fire for changes, for example
when a switch toggles.

### 9.2 The event

The bridge dispatches one custom event on the grid host element:

- name: `wablazor-cell-action`;
- `bubbles: true`, `composed: true`;
- detail: `{ column, action, rowKey, row, value, payload, rowIndex }`.

`lib.module.js` registers it with `registerCustomEventType`. The payload goes through the existing
`sanitize` function, with the depth increased to 4 for nested rows. The event goes out on the host,
not inside the shadow root. This is the same model as the relayed events (F11).

.NET side:

```csharp
[Parameter] public EventCallback<WaDataGridCellActionEventArgs> OnCellAction { get; set; }

public class WaDataGridCellActionEventArgs : EventArgs
{
    public string Column { get; set; } = string.Empty;   // column id; empty for rowDetail and footer content
    public string Action { get; set; } = string.Empty;   // action id from the preset or the custom code
    public JsonElement? RowKey { get; set; }              // string or number, as in the row data
    public IReadOnlyDictionary<string, object>? Row { get; set; }
    public object? Value { get; set; }                    // the raw cell value
    public JsonElement? Payload { get; set; }             // optional data from the emitter
    public int RowIndex { get; set; }                     // display index; -1 when not known

    public T? GetRow<T>();                                // deserializes Row into the DTO of the app
    public T? GetPayload<T>();
}
```

`Row` keeps the dictionary type for consistency with `WaDataGridCellClickEventArgs`. `GetRow<T>`
gives typed access. A common handler only needs `RowKey` and `Action`.

### 9.3 Three ways to raise an action

| Way | For | Mechanism |
|---|---|---|
| Presets | `Button`, `Actions`, `Menu`, `Link` with `Action`, `Switch`, `Checkbox` | The preset adds a listener to the node it creates. The closure knows the row and the column. |
| `ctx.emit(action, payload)` and `ctx.h(..., { action })` | Custom functions | The same closure model. Works for any DOM event, not only clicks. Works in `rowDetail` and `footer` content too. |
| Declarative attribute `data-wablazor-action="id"` (optional `data-wablazor-action-payload` with JSON) | Lit templates and nodes made without `ctx` | The click pairing below. Clicks only. |

Click pairing for the declarative attribute:

1. A capture listener on `grid.shadowRoot` sees the `click`. It looks in `composedPath()` for the
   nearest element with `data-wablazor-action` that is inside a cell. It keeps the action and the payload.
2. The click bubbles to the cell. The cell dispatches `wa-cell-click` synchronously (F5).
3. A listener on the grid host gets `wa-cell-click`. It takes the kept action. It dispatches
   `wablazor-cell-action` with the column, the row and the value from the `wa-cell-click` detail.
4. A microtask clears the kept action. If no `wa-cell-click` came (for example on a group row or in
   a detail panel), the bridge drops the action and writes a `console.warn`. Use `ctx.emit` there.

Keyboard: Enter or Space on a focused button makes a native `click`, so the same path applies.
Enter on the cell itself (not on a button) makes a normal `wa-cell-click` without an action.

### 9.4 Relation to `OnCellClick`

An action click also bubbles to the cell, so upstream also sends `wa-cell-click`. Without a rule,
a click on "Delete" also opens the record if the app opens records on `OnCellClick`.

Proposal: when a click raises an action, the bridge stops the propagation of that `wa-cell-click`
at the grid host. Blazor then does not see it. Page listeners on the grid element itself still see
it, so upstream behavior for non-Blazor code does not change. This is decision D4.

### 9.5 Limits

- Fire and forget. Blazor dispatches a registered event after the DOM event completes. .NET cannot
  cancel the action or give a value back to the JavaScript code. This is the same limit as for all
  cancelable `wa-*` events.
- Stale state. A toggle shows the new state at once, but `Data` in .NET still has the old value.
  The app must update its row and push `Data` again. If the app rejects the change, it pushes the
  unchanged data, and the cell shows the old state again. Document this pattern.
- Ordering. Events from the bridge come in DOM order. The Blazor Server circuit keeps that order.

### 9.6 Optional, later: request and response

Some cases need an answer from .NET in the JavaScript code. Examples: a confirmation, a value that
only the server can compute, or a validation of an inline edit. For these cases, add:

- `ctx.invoke(action, payload): Promise<JSON>`;
- a `DotNetObjectReference<WaDataGrid>` that the wrapper passes to `setDataGridColumns`, with one
  `[JSInvokable]` method;
- a grid parameter `Func<WaDataGridCellActionEventArgs, Task<object?>>? OnCellRequest`.

`WaDataGrid` then must implement `IAsyncDisposable` to dispose the reference. The development
experience is not smooth: the formatter is synchronous, so it must render a pending state, then
render again when the promise resolves (`ctx.cache` plus `grid.requestUpdate()`). Keep this for a
later phase, and build it only if NLAI has a concrete case.

## 10. Option examined: Razor markup rendered to static HTML

Idea: the developer writes the cell in Razor, as a `RenderFragment<TRow>`. .NET renders it to an HTML
string with `HtmlRenderer` (.NET 8 and later). The row data carries the HTML. A formatter parses it
once into a `<template>` and clones it into the cell.

| Aspect | Assessment |
|---|---|
| Development experience | Best for Razor developers. They write markup with `<WaBadge>` and so on. |
| Interactivity | `@onclick` does not work: the HTML is static. Only `data-wablazor-action` works (section 9.3). |
| Cost | One render for each cell on each data push, on the server or in WASM. More payload for each row. Acceptable for a page of 20–100 rows. Not acceptable for 10,000 client rows. |
| Correctness | Razor encodes text, so the HTML is safe. A `MarkupString` with user content is not safe. |
| Wrapper output | The `wa-*` wrappers render custom elements, so the static HTML contains the correct tags. Wrappers that need JS interop after render (for example `SetPropertyAsync`) do not work in static HTML. |
| Sort, filter, CSV | Not affected. The HTML is in a separate row property that no column reads. |

Verdict: possible, and attractive for server-mode grids with small pages. It needs the event bridge
first. Consider it for phase 3, after NLAI uses the presets. It does not replace the presets.

## 11. Cross-cutting concerns

### 11.1 Autoload

Section 5.2 steps 2 and 6 solve F6. A preset declares its tags. A custom function declares them
with `Uses`. The `discover(shadowRoot)` call after each push registers the rest. The resolver
imports `discover` through the existing `importWebAwesome` path (F10), so it uses the Web Awesome
instance of the page.

### 11.2 Styles in the shadow root

`WaDataGrid.ShadowStyles` (a CSS string) goes into a constructed `CSSStyleSheet`. The resolver
appends it to `grid.shadowRoot.adoptedStyleSheets`. It replaces the sheet only when the text
changes. Class rules (section 7.1) and custom functions need it. Presets use only component
attributes and inline styles, so they do not need it.

### 11.3 Culture and time zone

- `WaDataGrid.FormatterCulture` (string). The default is `CultureInfo.CurrentCulture.Name` at render
  time. The browser locale is not used, so the result matches the server culture.
- `TimeZone` on the date presets. A `DateTime` from .NET serializes without a zone when its kind is
  `Unspecified`, and JavaScript then reads it as local time. Document this: use UTC or
  `DateTimeOffset` in the row DTO, and set `TimeZone` for the display.
- `DateOnly` serializes as `yyyy-MM-dd`. JavaScript reads this format as UTC midnight. The `Date`
  preset must treat such a value as a calendar date, not as an instant. Otherwise a user west of UTC
  sees the day before.

### 11.4 Enum values in row data

Blazor JS interop serializes a C# enum as a number, unless the enum has a `JsonStringEnumConverter`.
A map with the key `"Active"` then never matches the value `2`. The typed factories prevent this:

```csharp
WaDataGridFormatter.Badge<OrderState>(state => state switch
{
    OrderState.Active => WaVariant.Success,
    OrderState.Failed => WaVariant.Danger,
    _ => WaVariant.Neutral,
}, label: state => Resources.ResourceManager.GetString($"OrderState_{state}"));
```

The factory computes each key with `JsonSerializer.Serialize(value)`. This call obeys a
`[JsonConverter]` attribute on the enum, so the key matches the serialized form. Document the limit:
a converter that only the global JS interop options configure is not visible to the factory.

### 11.5 Performance

- Text presets cost one `Intl` call for each cell. The resolver creates the `Intl` formatter object
  once for each column, not for each cell.
- Node presets use the node cache (section 6.1).
- The re-push change (section 5.3) removes most renders of all cells.
- Budget for the prototype: a virtualized grid with 10,000 rows and 8 preset columns scrolls without
  dropped frames on a mid-range laptop.

### 11.6 Security

- Presets build DOM with `createElement`, `setAttribute` and `textContent`. They never use
  `innerHTML`.
- `HrefTemplate` and `SrcTemplate` encode placeholders and allow only safe schemes (section 6.4).
- No `eval` and no `new Function`. A JavaScript text in C# is easy to build from user data by
  mistake (XSS). It also fails under a Content Security Policy without `unsafe-eval`.
- Module URLs come from C# code, not from row data. Keep them same-origin. Reject an absolute URL to
  another origin unless the developer sets an explicit option.
- Custom functions are code of the app. Lit and `ctx.h` escape text. A custom function that writes
  `innerHTML` is the responsibility of the developer. The documentation must say this.

### 11.7 Accessibility

- Each icon-only element has an accessible name (`label` on `wa-icon`, `aria-label` elsewhere).
- Interactive presets use real buttons and links. They stay in the tab order of the cell. The grid
  owns the roving focus, so verify that Tab and Enter work in an action cell (risk R4).
- A badge color is never the only signal. The badge always shows text.

### 11.8 Server mode

All presets work in server mode. `DataBar` with computed limits uses the loaded page only. Document
this, or require fixed `Min` and `Max` in server mode.

## 12. Consolidated C# API sketch

```csharp
// WaDataGridColumn - new members
public WaDataGridFormatter? Formatter { get; set; }
public WaDataGridFormatter? AggregatedFormatter { get; set; }
public WaDataGridFooterAggregate? FooterAggregate { get; set; }   // Footer (string) stays
public IReadOnlyList<WaDataGridClassRule>? CellClassRules { get; set; }
public WaJsFunction? CellClassFunction { get; set; }               // CellClass (string) stays
public WaDataGridComparator? Comparator { get; set; }               // Ordinal, Collation, Custom
public WaJsFunction? ValueFunction { get; set; }
public WaJsFunction? FilterFunction { get; set; }
public bool? SearchFormatted { get; set; }
public IReadOnlyList<WaDataGridFilterOption>? FilterOptions { get; set; }

// WaDataGrid - new parameters
[Parameter] public IReadOnlyList<WaDataGridClassRule>? RowClassRules { get; set; }
[Parameter] public WaJsFunction? RowClassFunction { get; set; }
[Parameter] public WaDataGridFormatter? RowDetail { get; set; }     // Fields preset or Custom
[Parameter] public string? SelectableField { get; set; }
[Parameter] public WaJsFunction? SelectableRowsFunction { get; set; }
[Parameter] public WaJsFunction? SearchFunction { get; set; }
[Parameter] public string? ShadowStyles { get; set; }
[Parameter] public string? FormatterCulture { get; set; }
[Parameter] public object? FormatterContext { get; set; }
[Parameter] public EventCallback<WaDataGridCellActionEventArgs> OnCellAction { get; set; }

// WaDataGridFormatter - static factories, one per preset in section 6, plus Custom(WaJsFunction)
public abstract record WaDataGridFormatter
{
    public static WaDataGridFormatter Number(WaNumberFormat? format = null);
    public static WaDataGridFormatter Currency(string currency, int? fractionDigits = null);
    public static WaDataGridFormatter Date(WaDateFormat? format = null, string? timeZone = null);
    public static WaDataGridFormatter Lookup(IReadOnlyDictionary<string, string> labels);
    public static WaDataGridFormatter Lookup<TEnum>(Func<TEnum, string> label) where TEnum : struct, Enum;
    public static WaDataGridFormatter Badge(IReadOnlyDictionary<string, WaVariant> variants, WaVariant? fallback = null, WaBadgeAppearance? appearance = null);
    public static WaDataGridFormatter Badge<TEnum>(Func<TEnum, WaVariant> variant, Func<TEnum, string>? label = null) where TEnum : struct, Enum;
    public static WaDataGridFormatter Actions(params WaDataGridAction[] actions);
    public static WaDataGridFormatter Switch(string action = "toggle");
    // ... the other presets of section 6
    public static WaDataGridFormatter Custom(WaJsFunction function);
}
```

Each new property has a separate name, so the existing `string` properties (`Footer`, `CellClass`)
do not change. This avoids a breaking change. With C# 15 union types, `CellClass` and
`CellClassFunction` can merge later (see the .NET 11 union types note in project memory). This is
decision D2.

Serialization: `WaDataGridColumn` today writes unset members as `null`. With about 15 new members,
set `JsonIgnoreCondition.WhenWritingNull`. This follow-up from the 3.11.0 release then becomes
necessary.

## 13. Verification plan

| Level | What | Where |
|---|---|---|
| bUnit and xUnit | The JSON of each descriptor. Enum key computation. URL validation of templates. Default culture. The rule that a column push happens only on change. | `WebAwesome.Blazor.Tests` |
| Browser e2e (Playwright) | Each preset renders. CSV text for string presets. Autoload of `wa-badge` without `data-wa-preload`. Each of the three ways to raise an action reaches `OnCellAction`. Action clicks do not raise `OnCellClick`. Keyboard activation. A collocated module and a registered function resolve. | `tools/e2e`. Needs Pro assets, because `wa-data-grid` is a Pro component. |
| Demo | A new "Cell rendering" section on `DataGridPage.razor`. One collocated `.razor.js` example. | `WebAwesome.Blazor.Demo` |
| Performance | The 10,000-row budget of section 11.5 | Manual, recorded in the plan |
| API parity | Check that the parity harness accepts wrapper-only parameters such as `OnCellAction` and `ShadowStyles` | `ApiParity` tests |

## 14. Phases

| Phase | Content | Size | Value for NLAI |
|---|---|---|---|
| 0 | Re-push only on change (section 5.3). `WhenWritingNull`. `FilterOptions`. | S | Indirect |
| 1 | Resolver, autoload, culture. Presets: `Number`, `Currency`, `Percent`, `Date`, `DateTime`, `Lookup`, `Boolean`, `Badge`, `Tag`, `Icon`, `BooleanIcon`, `Link`. `Comparator.Ordinal`. `OnCellAction` with the preset path. | M | Answers Q11 |
| 2 | `WaJsFunction` (all three modes), `ctx`, `ctx.emit`, declarative `data-wablazor-action`, `ShadowStyles`, class rules, `Actions`, `Menu`, `Switch`, `Checkbox`, `FooterAggregate`, `SearchFormatted`, the remaining presets | M–L | Escape hatch and events |
| 3 | Optional: `ctx.invoke` with request and response. Static Razor templates (section 10). | M each | Only on a concrete need |

Phases 1 and 2 are additive. They need a minor version and a `CHANGELOG` entry, but no migration
guide.

## 15. Decisions for the owner

Owner answers recorded on 2026-10-15.

| # | Decision | Proposal | Owner |
|---|---|---|---|
| D1 | Put presets in a new module `webawesome-datagrid.js`, or in `webawesome-interop.js` | New module. The interop module stays generic. The new module is also the public module for `Registered`. | Agreed |
| D2 | Separate property names (`CellClassFunction`) or a breaking type change of `CellClass` | Separate names now. Merge with C# 15 unions later. | Separate |
| D3 | The name of the reference type: `WaJsFunction` (general) or `WaDataGridFunction` (grid only) | `WaJsFunction`. The icon library and future components can use it. | Agreed. Reuse it for the icon library in a follow-up ticket. |
| D4 | Does an action click also raise `OnCellClick`? | No. The bridge stops it for Blazor (section 9.4). | Agreed |
| D5 | The name of the declarative attribute: `data-wa-action` or `data-wablazor-action` | `data-wablazor-action`. The `wa-` prefix can conflict with a later upstream attribute. The existing private names use `wablazor`. | `wablazor-` prefix |
| D6 | Include phase 3 in the plan now | No. Decide after NLAI uses phases 1 and 2. | All four phases (0–3). Pause for manual tests after phase 1. |
| D7 | Allow module URLs of another origin | No, unless an explicit option is set. | Agreed |
| D8 | Split the extras into a `WebAwesome.Blazor.Extensions` package | Yes, with the same version as the main package (section 17) | Agreed |
| D9 | The version of the release | 3.14.0.1: the fourth part is a revision of the bindings (section 17.3) | Agreed |
| D10 | `ShadowStyles` and `data-wablazor-action`: main package or Extensions | `ShadowStyles` in the main package, only on wrappers whose function members render app content. `data-wablazor-action` and the other `ctx` helpers in Extensions. | Agreed |
| D11 | The broken string-function methods and the unused `Func` properties | `[Obsolete]` in 3.14.0.1, removal in the 3.15.0 train | Agreed |

## 16. Risks to verify in a prototype

| # | Risk | Check |
|---|---|---|
| R1 | Upstream changes the formatter contract or the cell click handler (F3, F5). The grid is experimental. | Add both to the CEM and parity watch list of the `/wa-upgrade` pipeline. |
| R2 | A Lit template from the app's copy of Lit does not render in the Lit of the grid | Render one template from `lit@3` in a grid cell |
| R3 | Blazor enhanced navigation does not intercept an anchor in a shadow root | Click a `Link` preset and watch for a full page load |
| R4 | The roving focus of the grid blocks Tab to buttons in a cell | Keyboard test of the `Actions` preset |
| R5 | `discover(shadowRoot)` after each push costs too much on large grids | Measure. If necessary, run it only when the set of declared tags changes. |
| R6 | `wa-dropdown` in a cell opens in a clipped area, or its click does not reach the cell | Test the `Menu` preset in a pinned column and in the last row |
| R7 | `ctx` for each call costs too much | The 10,000-row budget (section 11.5) |

## 17. Package split, general function binding, and the ticket plan

This section is the agreed result of D8–D11. Where it differs from sections 4–14, this section is
in control.

### 17.1 The rule for the split

A feature that binds an upstream member, or a behavior that upstream documents, goes into
`WebAwesome.Blazor`. A feature that adds behavior that upstream does not have goes into
`WebAwesome.Blazor.Extensions`. The README non-goal ("pure bindings only") stays true for the main
package.

| `WebAwesome.Blazor` | `WebAwesome.Blazor.Extensions` |
|---|---|
| `WaJsFunction` (`Registered`, `Module`, `Global`, `WithArgs`, `Uses`), the shared registry on `globalThis[Symbol.for('WebAwesome.Blazor.functions')]`, the resolver, autoload of declared tags | All presets of section 6 and 7, as C# factories that return a `WaJsFunction` with the name of a registered preset function |
| A minimal `ctx` (`args`, `column`, `grid`, `rowKey`) and a public hook to decorate `ctx` | The `ctx` helpers: `emit`, `h`, `preset`, `format`, `cache` |
| Every upstream function-valued member, typed `WaJsFunction` (grid: `Formatter`, `AggregatedFormatter`, `FooterFunction`, `CellClassFunction`, `ValueFunction`, `ComparatorFunction`, `FilterFunction`, `AggregationFunction`, `RowClassFunction`, `RowDetail`, `SearchFunction`, `SelectableRowsFunction`, `ChildRowsFunction`) | The event bridge: `[EventHandler("onwablazor-cell-action", ...)]`, `WaDataGridCellActionEventArgs`, `data-wablazor-action`, the D4 rule |
| `FilterOptions`, the change-only push of `columns`, `WhenWritingNull` | Phase 3: `ctx.invoke` (request and response) and static Razor templates |
| `ShadowStyles` (section 17.2) | |

Connection points, all without a subclass of `WaDataGrid`:

1. The presets are `WaJsFunction` values. The main package does not know any preset.
2. The JS initializer `WebAwesome.Blazor.Extensions.lib.module.js` registers the preset functions
   and the `ctx` decorator in the shared registry. The resolver looks names up when it pushes.
3. The `[EventHandler]` declaration lives in Extensions. `WaDataGrid.AdditionalAttributes` already
   passes `@onwablazor-cell-action` to the element.
4. Culture and context are arguments of each preset. Extensions adds no grid parameters.

The Extensions package has the same version as the main package and depends on it with a floor of
the same version.

### 17.2 General function binding (all wrappers)

Function-valued members exist on about ten components (3.14.0 manifest): `wa-select` and
`wa-combobox` (`getTag`, combobox `filter`), `wa-rating` (`getSymbol`), `wa-slider` and `wa-range`
(`valueFormatter`), `wa-date-picker` and `wa-date-input` (`isDateDisabled`, `dayContent`),
`wa-pagination` (`hrefTemplate` function form), the icon library registry (`resolver`, `mutator`),
and `wa-data-grid`.

Defect: `WaSelect.SetGetTagFunctionAsync`, `WaRating.SetSymbolFunctionAsync` and
`WaSliderBase.SetValueFormatterAsync` assign a JavaScript source string to a property that must hold
a function. `wa-select` and `wa-rating` then throw a `TypeError` on render. `wa-slider` ignores the
string. `WaSelect.GetTag` and `WaSlider.ValueFormatter` are public `Func` properties that nothing
reads.

Fix: bind these members on `WaJsFunction`. Mark the three methods and the two properties
`[Obsolete]` in 3.14.0.1. Remove them in the 3.15.0 train (D11). Record the removal as a
"Next-release check" note in `docs/CHANGELOG.md`, which Phase 2 of `/wa-upgrade` reads
(`docs/UPGRADE-PROCESS.md`).

`ShadowStyles` (a CSS string adopted into the shadow root) is exposed only on wrappers whose
function members render app content into the shadow DOM: `WaDataGrid`, `WaSelect`, `WaCombobox`,
`WaRating`, `WaDatePicker`, `WaDateInput` (and `WaDateRangeInput`, which shares the base). One
JavaScript function and one C# helper implement it for all of them.

### 17.3 Version 3.14.0.1

- The first three parts are the bound Web Awesome version. The fourth part is a revision of the
  bindings. NuGet sorts 3.14.0 < 3.14.0.1 < 3.14.1.
- `WebAwesomeOptions.ResolveLibraryVersion()` must drop the fourth part, so the default CDN URL
  stays `webawesome@3.14.0`. This change ships in 3.14.0.1 itself.
- Development and the tag `wa-blazor-3.14.0.1` are on `/main/WA-3.14`. Before the 3.15 train
  starts, `/main/WA-3.14` is merged into `/main`.

### 17.4 Tickets (epic WAB-49), in order

| # | Ticket | Package |
|---|---|---|
| T1 | Version scheme: binding revision as the fourth part; Version.props 3.14.0.1; CDN version; preflight, `/wa-upgrade` comparison, README, CONTRIBUTING, UPGRADE-PROCESS | Main, tooling |
| T2 | `WebAwesome.Blazor.Extensions` package scaffold: projects, solution, packaging, dependency floor gate, demo reference, README non-goal text | Both |
| T3 | Phase 0: change-only push of `columns`, `WhenWritingNull`, `FilterOptions` | Main |
| T4 | Phase 1, main: `WaJsFunction`, registry, resolver, minimal `ctx`, autoload; grid `Formatter`, `AggregatedFormatter`, `ComparatorFunction` | Main |
| T5 | Phase 1, Extensions: text presets, `Badge`, `Tag`, `Icon`, `BooleanIcon`, `Link`, `Ordinal`; `OnCellAction` for presets; demo | Extensions |
| — | Pause for manual tests by the owner | |
| T6 | General function binding (section 17.2): defect fix, `[Obsolete]` members, `ShadowStyles` | Main |
| T7 | Icon library: `MutatorFunction` and `ResolverFunction` on `WaJsFunction`; the string `Mutator` becomes `[Obsolete]` | Main |
| T8 | Phase 2, main: the remaining grid function members | Main |
| T9 | Phase 2, Extensions: `ctx` helpers, `data-wablazor-action`, D4, `Actions`, `Menu`, `Switch`, `Checkbox`, class rules, footer aggregates, `SearchFormatted`, the remaining presets | Extensions |
| T10 | Phase 3: `ctx.invoke` and static Razor templates | Extensions (with a small hook in main if necessary) |

Then: changelog fold for 3.14.0.1, `/wa-release-preflight`, owner label.
