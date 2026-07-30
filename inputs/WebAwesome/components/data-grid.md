<!-- Source: reference doc bundled in the Web Awesome 3.11.0 release zip (dist/skills/webawesome/references/components/data-grid.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/data-grid -->

# Data Grid [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-data-grid>`

ProIncluded with Web Awesome Pro Experimental [Since 3.11](https://webawesome.com/docs/resources/changelog#wa_3110)

Data grids display tabular data with sorting, selection, filtering, pinning, tree data, grouping with aggregation, column footers, expandable rows, pagination, CSV export, full keyboard navigation, and virtualization for large datasets.

**[Get Data Grid with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=data-grid)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   Responsive [Layout Tools](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma) Newer additions to Web Awesome, like [`<wa-toast>`](https://webawesome.com/docs/components/toast), aren't included in the currently available kit, but a new version is in the works.  
    Track its progress on GitHub.
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Data Grid!

Like the [chart components](https://webawesome.com/docs/components/chart), data grids are driven by JavaScript properties. Set `data` and `columns` on the element and the grid handles rendering, sorting, filtering, selection, and pagination. Grids are fully keyboard-navigable, and large datasets are virtualized automatically. The body scrolls within a default max height of 30rem; see [Large Datasets](#large-datasets--virtualization) to change it.

The demo below turns on the most common features at once: sort, filter, search, select, reorder, resize, and paginate. Each is opt-in and covered in its own section below.

```html
<wa-data-grid
  id="grid-overview"
  label="Team directory"
  row-key="id"
  selectable
  with-search
  with-columns-menu
  with-column-menu
  reorderable
  resizable
  paginate
  page-size="10"
  style="--max-height: none;"
></wa-data-grid>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const grid = document.querySelector('#grid-overview');
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
  const statusVariant = { active: 'success', remote: 'brand', 'on leave': 'neutral' };

  grid.data = [
    { id: 1, name: 'Ava Mitchell', title: 'Staff Engineer', location: 'Austin, TX', salary: 198000, status: 'active' },
    { id: 2, name: 'Liam Chen', title: 'Senior Engineer', location: 'Seattle, WA', salary: 164500, status: 'active' },
    { id: 3, name: 'Sofia Rossi', title: 'Design Lead', location: 'Remote', salary: 172000, status: 'remote' },
    { id: 4, name: 'Noah Patel', title: 'Product Manager', location: 'New York, NY', salary: 158000, status: 'active' },
    {
      id: 5,
      name: 'Emma Johansson',
      title: 'Engineering Manager',
      location: 'Remote',
      salary: 211000,
      status: 'on leave',
    },
    {
      id: 6,
      name: 'Mateo García',
      title: 'Account Executive',
      location: 'Miami, FL',
      salary: 142000,
      status: 'active',
    },
    { id: 7, name: 'Kenji Tanaka', title: 'Senior Engineer', location: 'Remote', salary: 167500, status: 'remote' },
    {
      id: 8,
      name: 'Zoe Anderson',
      title: 'Account Executive',
      location: 'London, UK',
      salary: 147000,
      status: 'active',
    },
    { id: 9, name: 'Priya Sharma', title: 'Data Scientist', location: 'Austin, TX', salary: 176000, status: 'active' },
    {
      id: 10,
      name: 'Lucas Weber',
      title: 'Platform Engineer',
      location: 'Berlin, DE',
      salary: 154000,
      status: 'remote',
    },
    { id: 11, name: 'Amara Okafor', title: 'Product Designer', location: 'Remote', salary: 138000, status: 'active' },
    { id: 12, name: "Jack O'Brien", title: 'Support Lead', location: 'Dublin, IE', salary: 118000, status: 'on leave' },
    {
      id: 13,
      name: 'Mia Laurent',
      title: 'Engineering Manager',
      location: 'Paris, FR',
      salary: 189000,
      status: 'active',
    },
    {
      id: 14,
      name: 'Diego Fernández',
      title: 'Senior Engineer',
      location: 'Mexico City, MX',
      salary: 151000,
      status: 'active',
    },
    { id: 15, name: 'Hana Kim', title: 'QA Engineer', location: 'Seoul, KR', salary: 126000, status: 'remote' },
    { id: 16, name: 'Oliver Novak', title: 'Staff Engineer', location: 'Prague, CZ', salary: 182000, status: 'active' },
    {
      id: 17,
      name: 'Isabela Santos',
      title: 'Product Manager',
      location: 'São Paulo, BR',
      salary: 149000,
      status: 'active',
    },
    {
      id: 18,
      name: 'Elias Lindqvist',
      title: 'Security Engineer',
      location: 'Stockholm, SE',
      salary: 171000,
      status: 'remote',
    },
    { id: 19, name: 'Fatima Al-Rashid', title: 'Data Engineer', location: 'Remote', salary: 163000, status: 'active' },
    { id: 20, name: 'Theo Dubois', title: 'Design Engineer', location: 'Lyon, FR', salary: 144000, status: 'on leave' },
  ];

  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, filterable: true, flex: 2, minWidth: 180 },
    { field: 'title', label: 'Title', sortable: true, flex: 2, minWidth: 150 },
    { field: 'location', label: 'Location', sortable: true, flex: 1.5, minWidth: 175 },
    {
      field: 'salary',
      label: 'Salary',
      align: 'end',
      sortable: true,
      filterable: true,
      filterType: 'number-range',
      width: 180,
      formatter: value => currency.format(value),
    },
    {
      field: 'status',
      label: 'Status',
      filterable: true,
      filterType: 'set',
      width: 185,
      formatter: value => html`<wa-badge variant=${statusVariant[value]} appearance="filled">${value}</wa-badge>`,
    },
  ];
</script>
```

**Reassign `data` and `columns` instead of mutating them.**  
Both are shallowly reactive, so changing an existing array in place won't re-render. Assign a new array to trigger one.

## Accessibility Considerations

Always give the grid an accessible name with the `label` attribute. The grid uses the ARIA `grid` role (or `treegrid` when rows form a hierarchy) with row and column indices, sort state, selection state, and expansion state on the underlying cells, so assistive technology can navigate and announce it like a native table.

Screen readers are kept in the loop as things change: a polite live region announces selection counts, filter results, clipboard copies, and column moves, and every control the grid renders (checkboxes, expand toggles, filter buttons and panels, menus, the pager) carries an accessible name that's localized along with the rest of [Web Awesome's translations](https://webawesome.com/docs/localization).

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.11.0/components/data-grid/data-grid.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/data-grid/data-grid.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/data-grid/data-grid.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaDataGrid from '@awesome.me/webawesome/dist/react/data-grid/index.js';
```

### Slots

| Name | Description |
| --- | --- |
| \`empty\` | Content shown when there are no rows to display. |
| \`loading\` | Content shown in the loading overlay (server mode). |
| \`no-results\` | Content shown when an active search or filter matches no rows (falls back to a localized message). |

### Attributes & Properties

| Name | Description | Reflects |
| --- | --- | --- |
| \`appearance\` appearance | \`'outlined' \\| 'plain'\` The grid's visual appearance. Type Default 'outlined' | |
| \`childRows\` child-rows | \`string \\| ((row: Row) => Row\[\] \\| undefined) \\| null\` Provides each row's child rows for tree data — a field name (dot paths allowed) or a function returning children. Rows with children get an expand toggle; expanded children render indented and join sorting/filtering/selection. Type Default null | |
| \`columnOrder\` | \`string\[\]\` Get/set the column display order as an array of column ids. Empty array = natural order. Type | |
| \`columns\` | \`DataGridColumn\[\]\` The column definitions. Type Default \[\] | |
| \`data\` | \`Row\[\]\` The row objects to display. In client mode this is the full set. Type Default \[\] | |
| \`dataSource\` | \`rows\` An async function that loads data from a server. When set, the grid switches to manual mode: client-side sorting/filtering/pagination are disabled and this runs on any sort/filter/search/page change. Return and total. Type ((request: DataGridRequest) => Promise) \\| null Default null | |
| \`expandedKeys\` | \`row-key\` The row keys of the currently expanded rows. Settable. Without a , ids follow table-core's convention: a top-level row's index ('0'), then dotted index paths for children ('0.1'). Type (string \\| number)\[\] | |
| \`filterDebounce\` filter-debounce | \`number\` How long (in milliseconds) to wait after a search or filter keystroke before requesting data in server mode. Client-side filtering is always immediate. Sort and page changes are never debounced. Type Default 250 | |
| \`filteredCount\` | \`total\` The number of rows in the current result set after filtering and search, across every page (top-level rows for tree and grouped data; the server-reported in server mode). Read-only. Type number | |
| \`filterFromLeafRows\` filter-from-leaf-rows | \`boolean\` When filtering tree data, keeps a parent visible when any descendant matches (the filter runs leaf-up). By default a non-matching parent is removed with its entire subtree. Type Default false | |
| \`filters\` | \`\[{ id: 'category', value: 'Lighting' }\]\` Get/set the column filters declaratively, e.g. . Type { id: string; value: unknown }\[\] | |
| \`groupBy\` group-by | \`string \\| string\[\] \\| null\` Groups rows by column id — a single id, a space/comma-separated list (or array) for multi-level grouping. Each group is an expandable row showing its value, member count, and any column aggregates. Ignored for tree data and in server mode. Type Default null | |
| \`label\` label | \`string \\| null\` An accessible label for the grid. Type Default null | |
| \`loading\` loading | \`dataSource\` Whether a request is in flight. Type boolean Default false | |
| \`maxMultiSort\` max-multi-sort | \`0\` The maximum number of columns that can participate in a multi-column sort. (default) means no limit. Type number Default 0 | |
| \`page\` page | \`number\` The current page index (0-based). Type Default 0 | |
| \`pageCount\` | \`1\` The number of pages in the current result set (always when paginate is off). Read-only. Type number | |
| \`pageSize\` page-size | \`number\` The of rows per page. Type number Default 20 | |
| \`pageSizeOptions\` | \`number\[\]\` The page sizes offered by the pager's page-size selector. Type Default \[10, 20, 50, 100\] | |
| \`paginate\` paginate | \`boolean\` Enables client-side pagination and the pager footer. Type Default false | |
| \`pinnable\` pinnable | \`pinnable\` Enables column pinning (and the pin actions in the column menu). Can be overridden per column with . Type boolean Default false | |
| \`reorderable\` reorderable | \`movable\` Enables drag-to-reorder for columns (can be overridden per column with ). Type boolean Default false | |
| \`resizable\` resizable | \`boolean\` Enables drag-to-resize for columns (can be overridden per column). Type Default false | |
| \`rowClass\` | \`((row: Row) => string \\| null \\| undefined) \\| null\` Returns extra CSS class names for a row (space-separated; falsy for none) — e.g. to flag overdue or archived rows. Classes land on the row element inside the grid's shadow root, so style them with an adopted stylesheet (see the docs' "Styling Rendered Components"). Group rows are skipped. Type Default null | |
| \`rowDetail\` | \`TemplateResult\` Renders an expandable detail panel for a row. When set, each row shows an expand toggle. Return a string (escaped text), a Lit , or a Node. Type ((row: Row) => string \\| TemplateResult \\| Node) \\| null Default null | |
| \`rowKey\` row-key | \`selectable\` The field used as a stable row id for selection. Required in practice when is set. Type string \\| null Default null | |
| \`searchFn\` | \`true\` A custom predicate for the global search box. Receives the cell value, the search term, and the row; return to keep the row (a row matches when any searchable column matches). Client mode only. Type ((value: unknown, searchTerm: string, row: Row) => boolean) \\| null Default null | |
| \`searchTerm\` | \`string\` The current global search term. Type Default '' | |
| \`selectable\` selectable | \`multiple\` Enables row selection. A bare attribute means . Type '' \\| 'single' \\| 'multiple' \\| 'none' Default 'none' | |
| \`selectableRows\` | \`false\` A predicate deciding whether a row can be selected. Return to lock a row: its checkbox is disabled and it's skipped by select-all and range selection. When unset, every row is selectable (subject to selectable). Type ((row: Row) => boolean) \\| null Default null | |
| \`selectedKeys\` | \`rowKey\` The values of the currently selected rows. The source of truth for selection. Type (string \\| number)\[\] | |
| \`selectedRows\` | \`data\` The selected row objects resolvable from the currently loaded (best-effort, any tree depth). Settable, resolved by key. Type Row\[\] | |
| \`server\` server | \`dataSource\` Switches the grid to server mode without a callback: client-side sorting, filtering, and pagination are disabled and the grid emits wa-data-request whenever it needs data. Listen for it, fetch, then set data, total, and loading yourself. Implied when dataSource is set. Type boolean Default false | |
| \`size\` size | \`'xs' \\| 's' \\| 'm' \\| 'l' \\| 'xl' \\| 'small' \\| 'medium' \\| 'large'\` The grid's size. Controls the font scale of grid text and form controls, plus row height and cell padding. Type Default 'm' | |
| \`sort\` | \`\[{ id: 'name', desc: false }\]\` Get/set the sort state declaratively, e.g. . Type SortingState | |
| \`sortDescFirst\` sort-desc-first | \`true\` When , a column's first sort click sorts descending instead of ascending (table-core sortDescFirst). Type boolean Default false | |
| \`striped\` striped | \`boolean\` Renders alternating row background colors. Type Default false | |
| \`total\` total | \`dataSource\` The total row count in server mode. Drives the pager. Set automatically when resolves. Type number Default -1 | |
| \`withColumnMenu\` with-column-menu | \`boolean\` Shows a per-column header menu (kebab button) with pin, sort, hide, and autosize actions. Type Default false | |
| \`withColumnsMenu\` with-columns-menu | \`boolean\` Shows a toolbar menu for toggling column visibility. Type Default false | |
| \`withoutSortRemoval\` without-sort-removal | \`boolean\` Keeps a sorted column always sorted, alternating between ascending and descending. By default, a sorted column's third click clears its sort (the asc → desc → unsorted cycle). Type Default false | |
| \`withSearch\` with-search | \`boolean\` Shows a global search box that filters across all columns. Type Default false | |

### Methods

| Name | Description | Arguments |
| --- | --- | --- |
| \`autoSizeColumn()\` | Resizes one column to fit its widest rendered cell content (the double-click-handle behavior). | \`columnId: string\` |
| \`autoSizeColumns()\` | Resizes every resizable column to fit its content. | |
| \`collapseAllRows()\` | Collapses every row. | |
| \`collapseRow()\` | \`rowKey\` Collapses the row with the given key (its value). | \`key: string \\| number\` |
| \`copySelectedRows()\` | \`format: 'csv'\` Copies the selected rows (or every processed row when nothing is selected) to the clipboard, honoring the active sort, filters, and column visibility/order. The default tab-separated format pastes into spreadsheet cells; copies comma-separated text instead. Also wired to Ctrl+C when the grid has focus. Returns the number of rows copied. | \`options: { columnIds?: string\[\]; includeHeaders?: boolean; format?: 'tsv' \\| 'csv'; escapeFormulas?: boolean; }\` |
| \`expandAllRows()\` | Expands every row (all detail panels, or every branch of a tree). | |
| \`expandRow()\` | \`rowKey\` Expands the row with the given key (its value). | \`key: string \\| number\` |
| \`exportDataAsCsv()\` | \`formatter\` Exports the current rows as a CSV file (browser download). Respects the active sort, filters, search, and column visibility/order, and runs each column's . In server mode, only the currently loaded page is exported. | \`options: { fileName?: string; columnIds?: string\[\]; includeHeaders?: boolean; delimiter?: string; escapeFormulas?: boolean; }\` |
| \`focus()\` | Focuses the grid by focusing the active (roving-tabindex) cell. | \`options: FocusOptions\` |
| \`getColumnFacets()\` | Faceted data for a column — distinct cell values (with counts) and the numeric min/max, computed before this column's own filter applies. Use it to build filter UIs. Client mode only; returns empty facets in server mode. | \`columnId: string\` |
| \`getColumnPin()\` | \`false\` Returns which edge a column is pinned to, or if it isn't pinned. | \`columnId: string\` |
| \`getDataAsCsv()\` | \`formatter\` Returns the current rows as a CSV string, honoring the active sort, filters, search, and column visibility/order. Each column's runs for string output only (TemplateResult/Node cells fall back to the raw value). Every page and tree depth is included; server mode exports only the loaded page. Set escapeFormulas: true when the file may open in a spreadsheet and the data isn't trusted — cells starting with =, +, -, or @ are prefixed with an apostrophe so they can't execute as formulas (plain numbers are left alone). | \`options: { columnIds?: string\[\]; includeHeaders?: boolean; delimiter?: string; escapeFormulas?: boolean; }\` |
| \`getProcessedRows()\` | Every data row in the current result set, in display order — after sorting, filtering, and search, across all pages and tree depths (parents before their children). Group header rows are excluded. In server mode this is the currently loaded page. | |
| \`getState()\` | Returns a serializable snapshot of column order, widths, visibility, sort, filters, search, selection, paging. | |
| \`getVisibleRows()\` | The data rows currently displayed, in display order — after sorting, filtering, expansion, and pagination. Group header rows are excluded. | |
| \`handleColumnsChange()\` | When the columns change, prune any persisted order/sizing/visibility for column ids that no longer exist, and append newly-added column ids to the end of the order so they remain visible. | |
| \`handlePageChange()\` | \`page\` Reacts to changes from ANY source — pager clicks and programmatic sets alike — so grid.page = 3 clamps, keeps the roving tab stop in range, and refetches in server mode. UI paths that already requested data are deduped by the fetch scheduler. | |
| \`handleSearchTermChange()\` | \`searchTerm\` Reacts to changes from any source: returns to the first page, keeps the tab stop in range, and refetches (debounced) in server mode. The UI input handler only sets the property and emits — the behavior lives here so programmatic sets act exactly like typing. | |
| \`pinColumn()\` | \`'left'\` Pins a column to the or 'right' edge, or unpins it with false. | \`columnId: string, side: 'left' \\| 'right' \\| false\` |
| \`reload()\` | Re-runs the current server request (server mode only), even if its parameters haven't changed. | |
| \`resetColumns()\` | Resets column order, widths, visibility, and pinning to the column definitions' defaults, leaving sort, filters, search, selection, and paging untouched (the columns menu's "Reset columns" action). | |
| \`resetState()\` | Resets all user-adjusted view state (order, widths, visibility, pinning, sort, filters, search, expansion) to the column defaults. Selection and paging are left alone — clearing a user's selection is destructive. | |
| \`scrollToIndex()\` | Scrolls the row at the given display index into view (pairs with virtualization). | \`index: number, options: { align?: 'start' \\| 'center' \\| 'end' }\` |
| \`setState()\` | Restores a previously captured state. Unknown column ids are ignored; omitted keys are left unchanged. | \`state: DataGridState\` |
| \`sizeColumnsToFit()\` | Distributes column widths to fill the available horizontal space, honoring each column's min/max. | |
| \`toggleColumn()\` | \`id\` Shows or hes a column by its id (the column's id, or field when no id is set). | \`columnId: string, visible: boolean\` |

### Events

| Name | Description |
| --- | --- |
| \`request\` | |
| \`wa-cell-click\` | Emitted when a data cell is clicked, or Enter is pressed on the active data cell. |
| \`wa-cell-contextmenu\` | Emitted when a data cell is right-clicked (or Shift+F10 / the menu key is pressed on the active cell). Cancel it to suppress the native context menu. |
| \`wa-column-move\` | \`detail.finished\` Emitted when a column is reordered (live during drag; check ). |
| \`wa-column-pin\` | \`pinColumn()\` Emitted when the user pins or unpins a column through the built-in controls. Programmatic calls don't emit. |
| \`wa-column-resize\` | \`detail.finished\` Emitted when a column is resized (live during drag; check ). |
| \`wa-column-visibility-change\` | \`toggleColumn()\` Emitted when the user shows or hides a column through the built-in menus. Programmatic calls don't emit. |
| \`wa-data-error\` | \`dataSource\` Emitted in server mode when a request rejects. |
| \`wa-data-request\` | Emitted in server mode when the grid needs data for the current sort, filters, and page. |
| \`wa-filter-change\` | Emitted when the global search or a column filter changes. |
| \`wa-page-change\` | Emitted when the current page or page size changes. |
| \`wa-row-collapse\` | Emitted when a row collapses (a detail panel or a tree row's children). |
| \`wa-row-expand\` | Emitted when a row expands (a detail panel or a tree row's children). |
| \`wa-row-select\` | Emitted when the row selection changes. |
| \`wa-sort-change\` | Emitted when the sort order changes. |

### CSS Custom Properties

| Name | Description |
| --- | --- |
| \`--accent-color\` | \`var(--wa-color-brand-fill-loud)\` The checkbox accent and pinned-column highlight. Default |
| \`--background-color\` | \`var(--wa-color-surface-default)\` The grid body background. Default |
| \`--border-color\` | \`var(--wa-color-surface-border)\` The gridline and outer border color. Default |
| \`--border-radius\` | \`var(--wa-border-radius-m)\` The outer corner radius (outlined appearance). Default |
| \`--border-width\` | \`var(--wa-border-width-s)\` The gridline thickness. Default |
| \`--cell-padding\` | \`size\` The cell inline padding (also set by ). Default var(--wa-space-m) |
| \`--focus-ring\` | \`var(--wa-focus-ring)\` The active-cell focus ring. Default |
| \`--header-background\` | \`var(--wa-color-surface-lowered)\` The header row background. Default |
| \`--header-row-height\` | \`var(--row-height)\` The height of the header row. Default |
| \`--header-text-color\` | \`var(--wa-color-text-normal)\` The header text color. Default |
| \`--indent-size\` | \`1.25em\` The indentation applied per depth level to child rows in tree data. Default |
| \`--max-height\` | \`none\` The maximum height of the scrollable body. Set for natural height. Default 30rem |
| \`--row-height\` | \`size\` The height of each row (also set by ). Default 3.5rem |
| \`--row-hover-background\` | \`var(--wa-color-neutral-fill-normal)\` The hovered row background. Default |
| \`--selected-background\` | \`var(--wa-color-brand-fill-quiet)\` The selected row background. Default |
| \`--stripe-background\` | \`var(--wa-color-neutral-fill-quiet)\` The zebra (odd row) background. Default |
| \`--text-color\` | \`var(--wa-color-text-normal)\` The cell text color. Default |
| \`--transition-duration\` | \`var(--wa-transition-normal)\` The reorder/resize transition duration. Default |

### CSS Parts

| Name | Description | CSS selector |
| --- | --- | --- |
| \`body\` | The scrollable body container. | \`::part(body)\` |
| \`cell\` | A data cell. | \`::part(cell)\` |
| \`column-menu\` | The per-column header options dropdown. | \`::part(column-menu)\` |
| \`column-menu-button\` | The kebab button that opens a column's options menu. | \`::part(column-menu-button)\` |
| \`columns-menu\` | The column visibility menu. | \`::part(columns-menu)\` |
| \`data-grid\` | The component's outer wrapper. | \`::part(data-grid)\` |
| \`drag-ghost\` | The floating label that follows the pointer while reordering a column (rendered in the top layer). | \`::part(drag-ghost)\` |
| \`ellipsis\` | The collapsed-pages ellipsis in the pager. | \`::part(ellipsis)\` |
| \`empty\` | \`empty\` The -state container shown when there are no rows (wraps the empty slot). | \`::part(empty)\` |
| \`expand-button\` | The expand/collapse toggle button on a row. | \`::part(expand-button)\` |
| \`filter-button\` | The funnel button in a filterable column's header that opens its filter panel. | \`::part(filter-button)\` |
| \`filter-panel\` | The popover panel that contains a column's filter controls. | \`::part(filter-panel)\` |
| \`first-button\` | The "first page" pager button. | \`::part(first-button)\` |
| \`footer\` | The footer that contains the pager. | \`::part(footer)\` |
| \`footer-cell\` | A column footer cell. | \`::part(footer-cell)\` |
| \`footer-row\` | The column footer row pinned to the bottom of the scroll area. | \`::part(footer-row)\` |
| \`group-count\` | The member count shown next to a group's value. | \`::part(group-count)\` |
| \`group-row\` | \`row\` A group (also carries row), when group-by is set. | \`::part(group-row)\` |
| \`group-value\` | The group's value in a group row's grouping cell. | \`::part(group-value)\` |
| \`header\` | The header row container. | \`::part(header)\` |
| \`header-cell\` | A column header cell. | \`::part(header-cell)\` |
| \`last-button\` | The "last page" pager button. | \`::part(last-button)\` |
| \`live-region\` | The visually-hidden polite live region for screen-reader announcements. | \`::part(live-region)\` |
| \`loading-overlay\` | The overlay shown while a server request is in flight. | \`::part(loading-overlay)\` |
| \`next-button\` | The "next page" pager button. | \`::part(next-button)\` |
| \`no-results\` | \`no-results\` The container shown when a search or filter matches no rows (wraps the slot). | \`::part(no-results)\` |
| \`page\` | A page-number button in the pager. | \`::part(page)\` |
| \`page-current\` | The current page-number button in the pager. | \`::part(page-current)\` |
| \`page-size\` | \`\` The page-size in the footer. | \`::part(page-size)\` |
| \`pager\` | \`\` The pagination control (a element). | \`::part(pager)\` |
| \`pager-button\` | \`wa-pagination\` Every button in the pager, including page numbers (exported from 's button part). | \`::part(pager-button)\` |
| \`pin-indicator\` | The pin button shown in a pinned column's header (click to unpin). | \`::part(pin-indicator)\` |
| \`previous-button\` | The "previous page" pager button. | \`::part(previous-button)\` |
| \`resize-handle\` | The drag handle for resizing a column. | \`::part(resize-handle)\` |
| \`row\` | A data row. | \`::part(row)\` |
| \`row-detail\` | The expandable detail panel for a row. | \`::part(row-detail)\` |
| \`search\` | The global search input. | \`::part(search)\` |
| \`select-all-checkbox\` | The header checkbox that selects the current page. | \`::part(select-all-checkbox)\` |
| \`sort-indicator\` | The sort direction arrow icon in a header cell. | \`::part(sort-indicator)\` |
| \`sort-number\` | The numbered priority badge shown next to each column in a multi-column sort. | \`::part(sort-number)\` |
| \`table\` | The grid table element. | \`::part(table)\` |
| \`toolbar\` | The toolbar that contains the search box and columns menu. | \`::part(toolbar)\` |

### Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-button>`](https://webawesome.com/docs/components/button)
-   [`<wa-checkbox>`](https://webawesome.com/docs/components/checkbox)
-   [`<wa-divider>`](https://webawesome.com/docs/components/divider)
-   [`<wa-dropdown>`](https://webawesome.com/docs/components/dropdown)
-   [`<wa-dropdown-item>`](https://webawesome.com/docs/components/dropdown-item)
-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)
-   [`<wa-input>`](https://webawesome.com/docs/components/input)
-   [`<wa-option>`](https://webawesome.com/docs/components/option)
-   [`<wa-pagination>`](https://webawesome.com/docs/components/pagination)
-   [`<wa-popup>`](https://webawesome.com/docs/components/popup)
-   [`<wa-select>`](https://webawesome.com/docs/components/select)
-   [`<wa-spinner>`](https://webawesome.com/docs/components/spinner)
-   [`<wa-tag>`](https://webawesome.com/docs/components/tag)

## Examples

### Columns

Each column is an object. Use `field` to map a column to a property on your row objects (dot paths like `'user.name'` work too), `label` for the header text, and `align` to position cell content. Set `width` to give a column a fixed pixel width; columns without a width share the remaining space. Text that doesn't fit truncates with an ellipsis, and hovering a truncated cell reveals the full text.

```html
<wa-data-grid id="grid-columns" label="Inventory" row-key="sku"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-columns');

  grid.data = [
    { sku: 'CHA-014', product: 'Aeron Office Chair', category: 'Furniture', price: 1395, stock: 18 },
    { sku: 'DSK-220', product: 'Standing Desk, 60"', category: 'Furniture', price: 749, stock: 7 },
    { sku: 'MON-340', product: '27" 4K Monitor', category: 'Electronics', price: 529, stock: 42 },
    { sku: 'KEY-101', product: 'Mechanical Keyboard', category: 'Electronics', price: 159, stock: 0 },
    { sku: 'LMP-076', product: 'LED Desk Lamp', category: 'Lighting', price: 64, stock: 130 },
  ];

  grid.columns = [
    { field: 'sku', label: 'SKU', width: 110 },
    { field: 'product', label: 'Product' },
    { field: 'category', label: 'Category' },
    { field: 'price', label: 'Price', align: 'end', formatter: value => `$${value.toLocaleString()}` },
    { field: 'stock', label: 'In Stock', align: 'end' },
  ];
</script>
```

### Custom Cell Content

Provide a `formatter` function to control how a cell renders. It receives the cell value and the full row; return a **string** for escaped text, or a [Lit](https://lit.dev/) `html` template for rich content like badges and buttons. Interpolated values are escaped automatically.

```html
<wa-data-grid id="grid-format" label="Pull requests" row-key="id"></wa-data-grid>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const grid = document.querySelector('#grid-format');

  grid.data = [
    { id: 1, title: 'Fix focus trap in dialog', author: 'Ava Mitchell', state: 'merged', additions: 84, deletions: 12 },
    { id: 2, title: 'Add CSV export to data grid', author: 'Liam Chen', state: 'open', additions: 312, deletions: 4 },
    { id: 3, title: 'Bump dependencies (again)', author: 'Sofia Rossi', state: 'open', additions: 1, deletions: 1 },
    { id: 4, title: 'Remove legacy theme tokens', author: 'Noah Patel', state: 'closed', additions: 0, deletions: 230 },
  ];

  const stateVariant = { merged: 'brand', open: 'success', closed: 'neutral' };

  grid.columns = [
    { field: 'title', label: 'Title', sortable: true, flex: 3, minWidth: 180 },
    { field: 'author', label: 'Author', flex: 1, minWidth: 130 },
    {
      field: 'state',
      label: 'State',
      width: 110,
      formatter: value => html`<wa-badge variant=${stateVariant[value]} appearance="filled">${value}</wa-badge>`,
    },
    {
      id: 'diff',
      label: 'Changes',
      align: 'end',
      width: 120,
      formatter: (_value, row) => html`
        <span style="color: var(--wa-color-success-on-quiet);">+${row.additions}</span>
        <span style="color: var(--wa-color-danger-on-quiet); margin-inline-start: 0.5em;">−${row.deletions}</span>
      `,
    },
  ];
</script>
```

### Actions Column

A column doesn't need a `field`. Give it an `id` and a `formatter` to render buttons, links, or menus. With no field, the formatter's first argument is `undefined` (use the second to reach the row), and the column isn't sortable or searchable. Because these controls render inside cells, give each icon-only button an accessible name, such as the `label` on its [`<wa-icon>`](https://webawesome.com/docs/components/icon) as in the demo, so assistive technology can tell them apart.

```html
<wa-data-grid id="grid-actions" label="Documents" row-key="id"></wa-data-grid>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const grid = document.querySelector('#grid-actions');
  grid.data = [
    { id: 1, name: 'Q2 Financial Report (Final) (Final v2).pdf', owner: 'Grace Thompson', shared: true },
    { id: 2, name: 'Brand Guidelines.fig', owner: 'Chloe Dubois', shared: true },
    { id: 3, name: 'Onboarding Checklist.docx', owner: 'Daniel Silva', shared: false },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, flex: 3, minWidth: 180 },
    { field: 'owner', label: 'Owner', flex: 1, minWidth: 140 },
    {
      id: 'actions',
      label: 'Actions',
      align: 'end',
      width: 140,
      formatter: (_value, row) => html`
        <wa-tooltip for="dl-${row.id}">Download</wa-tooltip>
        <wa-button id="dl-${row.id}" size="s" appearance="plain">
          <wa-icon name="download" label="Download"></wa-icon>
        </wa-button>
        <wa-tooltip for="sh-${row.id}">Share</wa-tooltip>
        <wa-button id="sh-${row.id}" size="s" appearance="plain">
          <wa-icon name=${row.shared ? 'users' : 'share-nodes'} label="Share"></wa-icon>
        </wa-button>
      `,
    },
  ];
</script>
```

### Computed Columns

Give a column a `value` function to compute its cell value from the row. Unlike a `formatter`, which only changes how a cell renders, the computed value drives sorting, filtering, searching, copying, and CSV export.

```js
grid.columns = [
  { field: 'hours', label: 'Hours' },
  { field: 'rate', label: 'Rate' },
  {
    id: 'amount',
    label: 'Amount',
    align: 'end',
    value: row => row.hours * row.rate,
    formatter: value => currency.format(value),
  },
];
```

### Striped Rows

Add the `striped` attribute for alternating row backgrounds, which can make dense, multi-column grids easier to scan.

```html
<wa-data-grid id="grid-striped" label="Periodic table" row-key="number" striped></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-striped');
  grid.data = [
    { number: 1, symbol: 'H', name: 'Hydrogen', mass: 1.008, category: 'Nonmetal' },
    { number: 2, symbol: 'He', name: 'Helium', mass: 4.0026, category: 'Noble gas' },
    { number: 3, symbol: 'Li', name: 'Lithium', mass: 6.94, category: 'Alkali metal' },
    { number: 6, symbol: 'C', name: 'Carbon', mass: 12.011, category: 'Nonmetal' },
    { number: 8, symbol: 'O', name: 'Oxygen', mass: 15.999, category: 'Nonmetal' },
    { number: 11, symbol: 'Na', name: 'Sodium', mass: 22.99, category: 'Alkali metal' },
    { number: 26, symbol: 'Fe', name: 'Iron', mass: 55.845, category: 'Transition metal' },
    { number: 79, symbol: 'Au', name: 'Gold', mass: 196.97, category: 'Transition metal' },
  ];
  grid.columns = [
    { field: 'number', label: '#', align: 'end', width: 64 },
    { field: 'symbol', label: 'Symbol', width: 110 },
    { field: 'name', label: 'Name', sortable: true, flex: 1, minWidth: 130 },
    { field: 'mass', label: 'Atomic Mass', align: 'end', sortable: true, width: 170 },
    { field: 'category', label: 'Category', sortable: true, flex: 1, minWidth: 150 },
  ];
</script>
```

### Size

Use the `size` attribute (`xs`, `s`, `m`, `l`, `xl`) to control both the grid's text scale and its row height and cell padding together. The size also cascades to the form controls rendered inside the grid (selection checkboxes, the search box, column filters, and the pager).

```html
<div id="grid-size-demo">
  <wa-select value="s" style="max-width: 12rem;">
    <span slot="label" class="wa-visually-hidden">Size</span>
    <wa-option value="xs">Extra Small</wa-option>
    <wa-option value="s">Small</wa-option>
    <wa-option value="m">Medium</wa-option>
    <wa-option value="l">Large</wa-option>
    <wa-option value="xl">Extra Large</wa-option>
  </wa-select>
  <wa-divider></wa-divider>
  <wa-data-grid id="grid-size" label="Releases" row-key="id" size="s" with-search selectable></wa-data-grid>
</div>

<script type="module">
  const demo = document.querySelector('#grid-size-demo');
  const grid = demo.querySelector('wa-data-grid');
  const picker = demo.querySelector('wa-select');
  grid.data = [
    { id: 1, version: '3.0.0', date: 'Jun 24, 2026', changes: 142, type: 'major' },
    { id: 2, version: '2.8.4', date: 'Jun 10, 2026', changes: 12, type: 'patch' },
    { id: 3, version: '2.8.0', date: 'May 29, 2026', changes: 58, type: 'minor' },
    { id: 4, version: '2.7.2', date: 'May 14, 2026', changes: 7, type: 'patch' },
  ];
  grid.columns = [
    { field: 'version', label: 'Version', sortable: true, flex: 1, minWidth: 120 },
    { field: 'date', label: 'Released', align: 'end', width: 140 },
    { field: 'changes', label: 'Changes', align: 'end', sortable: true, width: 120 },
    { field: 'type', label: 'Type', flex: 1, minWidth: 110 },
  ];
  picker.addEventListener('change', () => (grid.size = picker.value));
</script>
```

### Appearance

Use the `appearance` attribute to switch between the default `outlined` look and a borderless `plain` look that blends into the surrounding page.

```html
<div id="grid-appearance-demo">
  <wa-select value="outlined" style="max-width: 12rem;">
    <span slot="label" class="wa-visually-hidden">Appearance</span>
    <wa-option value="outlined">Outlined</wa-option>
    <wa-option value="plain">Plain</wa-option>
  </wa-select>
  <wa-divider></wa-divider>
  <wa-data-grid id="grid-appearance" label="Tickets" row-key="id" appearance="outlined"></wa-data-grid>
</div>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const demo = document.querySelector('#grid-appearance-demo');
  const grid = demo.querySelector('wa-data-grid');
  const picker = demo.querySelector('wa-select');
  grid.data = [
    { id: 'WA-481', summary: 'Resize handle hard to grab', assignee: 'Ava Mitchell', priority: 'high' },
    { id: 'WA-477', summary: 'Add empty-state slot to data grid', assignee: 'Liam Chen', priority: 'medium' },
    { id: 'WA-470', summary: 'Document custom comparators', assignee: 'Sofia Rossi', priority: 'low' },
  ];
  const variant = { high: 'warning', medium: 'neutral', low: 'neutral' };
  grid.columns = [
    { field: 'id', label: 'Key', width: 100 },
    { field: 'summary', label: 'Summary', sortable: true, flex: 2, minWidth: 200 },
    { field: 'assignee', label: 'Assignee', flex: 1, minWidth: 140 },
    {
      field: 'priority',
      label: 'Priority',
      width: 120,
      sortable: true,
      formatter: value => html`<wa-badge variant=${variant[value]}>${value}</wa-badge>`,
    },
  ];
  picker.addEventListener('change', () => (grid.appearance = picker.value));
</script>
```

### Sorting

Columns with a `field` are sortable by default; set `sortable: false` to opt out. Click a header to cycle through ascending, descending, and unsorted, or hold Shift while clicking (or press Shift + Enter on a focused header) to sort by multiple columns, where a numbered badge shows each column's priority. Read or set the order through the `sort` property, and listen for the `wa-sort-change` event.

```html
<wa-data-grid id="grid-sort" label="Standings" row-key="id"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-sort');

  grid.data = [
    { id: 1, team: 'Arsenal', played: 28, won: 19, drawn: 5, lost: 4, points: 62 },
    { id: 2, team: 'Manchester City', played: 28, won: 19, drawn: 4, lost: 5, points: 61 },
    { id: 3, team: 'Liverpool', played: 28, won: 18, drawn: 6, lost: 4, points: 60 },
    { id: 4, team: 'Aston Villa', played: 28, won: 17, drawn: 4, lost: 7, points: 55 },
    { id: 5, team: 'Tottenham', played: 28, won: 16, drawn: 5, lost: 7, points: 53 },
    { id: 6, team: 'Brighton', played: 28, won: 11, drawn: 9, lost: 8, points: 42 },
  ];

  grid.columns = [
    { field: 'team', label: 'Team', flex: 1, minWidth: 160 },
    { field: 'played', label: 'P', align: 'end', width: 70 },
    { field: 'won', label: 'W', align: 'end', width: 70 },
    { field: 'drawn', label: 'D', align: 'end', width: 70 },
    { field: 'lost', label: 'L', align: 'end', width: 70 },
    { field: 'points', label: 'Pts', align: 'end', width: 80 },
  ];
</script>
```

#### Sort Algorithms

By default, the grid infers a sort algorithm from each column's values, and plain `Date` objects are detected and sorted chronologically automatically. Set `sortFn` to pick a specific built-in algorithm, most usefully `'datetime'` for columns holding date _strings_, which would otherwise sort alphabetically:

```js
grid.columns = [
  { field: 'name', label: 'Name' },
  { field: 'created', label: 'Created', sortable: true, sortFn: 'datetime' },
];
```

`sortFn` accepts any of these built-in algorithms:

| sortFn | Best for | Notes |
| --- | --- | --- |
| \`'alphanumeric'\` | Mixed strings and numbers | Case-insensitive |
| \`'alphanumericCaseSensitive'\` | Mixed strings and numbers | Case-sensitive |
| \`'text'\` | Strings only | \`alphanumeric\` Faster than |
| \`'textCaseSensitive'\` | Strings only | Faster; case-sensitive |
| \`'datetime'\` | \`Date\` objects or date strings | Sorts chronologically |
| \`'basic'\` | Simple values | \`>\` Fastest; plain / < |

#### Custom Comparators

For full control, provide a `comparator` to define your own order, for example, to sort a status column by severity rather than alphabetically. It receives the two cell values (and their rows) and returns a negative, zero, or positive number for _ascending_ order; the grid flips it for descending. A `comparator` takes precedence over `sortFn`.

```html
<wa-data-grid id="grid-comparator" label="Incidents" row-key="id"></wa-data-grid>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const grid = document.querySelector('#grid-comparator');
  grid.data = [
    { id: 1, incident: 'API latency spike', severity: 'high', opened: '2026-06-21' },
    { id: 2, incident: 'Typo in the pricing page', severity: 'low', opened: '2026-06-24' },
    { id: 3, incident: 'Checkout returning 500s', severity: 'critical', opened: '2026-06-25' },
    { id: 4, incident: 'Slow image loads', severity: 'medium', opened: '2026-06-23' },
  ];

  const order = { critical: 0, high: 1, medium: 2, low: 3 };
  const variant = { critical: 'danger', high: 'warning', medium: 'neutral', low: 'neutral' };

  grid.columns = [
    { field: 'incident', label: 'Incident', sortable: true, flex: 2, minWidth: 200 },
    {
      field: 'severity',
      label: 'Severity',
      sortable: true,
      width: 130,
      comparator: (a, b) => order[a] - order[b],
      formatter: value => html`<wa-badge variant=${variant[value]} appearance="filled">${value}</wa-badge>`,
    },
    { field: 'opened', label: 'Opened', align: 'end', sortable: true, width: 130, sortFn: 'datetime' },
  ];
</script>
```

#### Sort Behavior

A few attributes tune how sorting behaves:

| Attribute | Effect |
| --- | --- |
| \`without-sort-removal\` | A sorted column alternates between ascending and descending instead of clearing its sort on the third click. |
| \`sort-desc-first\` | \`sortDescFirst\` The first click sorts descending. Override per column with . |
| \`max-multi-sort\` | \`0\` Caps how many columns can multi-sort; adding another drops the oldest. (default) means no limit. |

Per column, `sortUndefined` controls where empty (`null`/`undefined`) values land: `'first'`, `'last'`, `1`, or `-1`.

```html
<wa-data-grid without-sort-removal sort-desc-first max-multi-sort="3"></wa-data-grid>
```

### Selection

Add the `selectable` attribute to render a leading checkbox column with select-all in the header. A bare attribute means `selectable="multiple"`. Set the `row-key` attribute to a field that uniquely identifies each row so selection stays stable across sorting and pagination. Read the selection from the `selectedKeys` or `selectedRows` properties or listen for the `wa-row-select` event; hold Shift while clicking to select a range.

```html
<wa-data-grid id="grid-select" label="Subscribers" row-key="id" selectable></wa-data-grid>
<p>Selected: <span id="grid-select-count">0</span></p>

<script type="module">
  const grid = document.querySelector('#grid-select');

  grid.data = [
    { id: 1, name: 'Ava Mitchell', email: 'ava.mitchell@example.com', plan: 'Pro' },
    { id: 2, name: 'Liam Chen', email: 'liam.chen@example.com', plan: 'Team' },
    { id: 3, name: 'Sofia Rossi', email: 'sofia.rossi@example.com', plan: 'Pro' },
    { id: 4, name: 'Noah Patel', email: 'noah.patel@example.com', plan: 'Enterprise' },
    { id: 5, name: 'Emma Johansson', email: 'emma.johansson@example.com', plan: 'Team' },
  ];

  grid.columns = [
    { field: 'name', label: 'Name', flex: 1, minWidth: 140 },
    { field: 'email', label: 'Email', flex: 2, minWidth: 200 },
    { field: 'plan', label: 'Plan', width: 130 },
  ];

  grid.addEventListener('wa-row-select', event => {
    document.querySelector('#grid-select-count').textContent = event.detail.selectedKeys.length;
  });
</script>
```

When pagination is on, select-all toggles only the current page, leaving other pages' selections intact. To lock specific rows, set the `selectableRows` property to a predicate. Rows that return `false` get a disabled checkbox and are skipped by select-all and range selection.

```js
grid.selectableRows = row => row.status !== 'archived';
```

Set `selectable="single"` for radio-style selection that allows one row at a time.

```html
<wa-data-grid id="grid-single" label="Pick a plan" row-key="id" selectable="single"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-single');

  grid.data = [
    { id: 'starter', name: 'Starter', seats: 'Up to 3', price: 0 },
    { id: 'pro', name: 'Pro', seats: 'Up to 10', price: 12 },
    { id: 'team', name: 'Team', seats: 'Up to 50', price: 24 },
    { id: 'enterprise', name: 'Enterprise', seats: 'Unlimited', price: 96 },
  ];

  grid.columns = [
    { field: 'name', label: 'Plan', flex: 1, minWidth: 130 },
    { field: 'seats', label: 'Seats', flex: 1, minWidth: 130 },
    { field: 'price', label: 'Price/mo', align: 'end', width: 120, formatter: value => (value ? `$${value}` : 'Free') },
  ];
</script>
```

### Pagination

Add the `paginate` attribute to break rows into pages with a pager in the footer. Use `page-size` for rows per page and the `pageSizeOptions` property for the picker's choices, keeping `page-size` in that list. The `wa-page-change` event reports `page` and `pageSize`, and the pager is a [`<wa-pagination>`](https://webawesome.com/docs/components/pagination) element you can style through the `pager` part.

The `page` property is **0-based**, so `page="0"` is the first page. This matches the request payload the grid sends in [server mode](#loading-data-from-a-server), but differs from [`<wa-pagination>`](https://webawesome.com/docs/components/pagination), whose `page` is 1-based.

```html
<wa-data-grid id="grid-page" label="Transactions" row-key="id" paginate page-size="10"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-page');

  const merchants = ['Amazon', 'Spotify', 'Uber', 'Whole Foods', 'Delta Air Lines', 'Apple', 'Shell', 'Netflix'];
  const methods = ['Visa ••4291', 'Amex ••1008', 'Mastercard ••7733'];
  const statuses = ['completed', 'pending', 'refunded'];
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

  grid.data = Array.from({ length: 96 }, (_, i) => ({
    id: 1000 + i,
    merchant: merchants[i % merchants.length],
    method: methods[i % methods.length],
    amount: Math.round((12 + ((i * 37.5) % 480)) * 100) / 100,
    status: statuses[i % statuses.length],
  }));

  grid.pageSizeOptions = [10, 25, 50];
  grid.columns = [
    { field: 'id', label: 'Txn', width: 85 },
    { field: 'merchant', label: 'Merchant', sortable: true, flex: 2, minWidth: 140 },
    { field: 'method', label: 'Payment Method', flex: 1, minWidth: 185 },
    { field: 'amount', label: 'Amount', align: 'end', sortable: true, width: 118, formatter: v => currency.format(v) },
    { field: 'status', label: 'Status', sortable: true, width: 110 },
  ];
</script>
```

### Global Search

Add the `with-search` attribute to show a search box that filters across all columns. Listen for the `wa-filter-change` event to react to the current search term and filters, or read and set the term through the `searchTerm` property.

```html
<wa-data-grid id="grid-search" label="Countries" row-key="code" with-search></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-search');
  grid.data = [
    { code: 'US', country: 'United States', capital: 'Washington, D.C.', region: 'Americas', population: 331893745 },
    { code: 'JP', country: 'Japan', capital: 'Tokyo', region: 'Asia', population: 125681593 },
    { code: 'BR', country: 'Brazil', capital: 'Brasília', region: 'Americas', population: 214326223 },
    { code: 'DE', country: 'Germany', capital: 'Berlin', region: 'Europe', population: 83294633 },
    { code: 'KE', country: 'Kenya', capital: 'Nairobi', region: 'Africa', population: 54027487 },
    { code: 'IN', country: 'India', capital: 'New Delhi', region: 'Asia', population: 1417173173 },
    { code: 'FR', country: 'France', capital: 'Paris', region: 'Europe', population: 64626628 },
    { code: 'AU', country: 'Australia', capital: 'Canberra', region: 'Oceania', population: 26177413 },
  ];
  grid.columns = [
    { field: 'country', label: 'Country', sortable: true, flex: 2, minWidth: 150 },
    { field: 'capital', label: 'Capital', flex: 2, minWidth: 150 },
    { field: 'region', label: 'Region', sortable: true, width: 130 },
    {
      field: 'population',
      label: 'Population',
      align: 'end',
      sortable: true,
      width: 140,
      formatter: v => v.toLocaleString(),
    },
  ];
</script>
```

Every column with a `field` (or computed `value`) participates by default; set `searchable: false` to exclude one. For custom matching, set the `searchFn` property to a predicate that receives the cell value, the search term, and the row. A row matches when any searchable column returns `true`. Read the results with `filteredCount`, `getVisibleRows()` (the rows on screen), or `getProcessedRows()` (every matching row across all pages, sorted).

### Column Filters

Set `filterable: true` on a column to add a filter button to its header. It opens a panel with the column's filter controls, and shows a dot while its filter is active. Column filters and global search can be combined, and the `filters` property reads or sets the active filters programmatically.

```html
<wa-data-grid id="grid-filter" label="Products" row-key="id"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-filter');
  grid.data = [
    { id: 1, name: 'Aurora Floor Lamp', category: 'Lighting', material: 'Brass', price: 249 },
    { id: 2, name: 'Borealis Wool Rug', category: 'Textiles', material: 'Wool', price: 429 },
    { id: 3, name: 'Cascade Glass Vase', category: 'Decor', material: 'Glass', price: 85 },
    { id: 4, name: 'Dusk Wall Sconce', category: 'Lighting', material: 'Steel', price: 139 },
    { id: 5, name: 'Ember Throw Blanket', category: 'Textiles', material: 'Cotton', price: 69 },
    { id: 6, name: 'Fjord Ceramic Bowl', category: 'Decor', material: 'Ceramic', price: 44 },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, filterable: true, flex: 2, minWidth: 180 },
    { field: 'category', label: 'Category', filterable: true, width: 150 },
    { field: 'material', label: 'Material', filterable: true, width: 150 },
    { field: 'price', label: 'Price', align: 'end', sortable: true, width: 110, formatter: v => `$${v}` },
  ];
</script>
```

#### Filter Types

By default a column filter is a case-insensitive substring match. Set `filterType` to choose a different matcher, each rendering an appropriate panel:

| filterType | Matches | Panel controls |
| --- | --- | --- |
| \`'text'\` (default) | case-insensitive substring | a text input |
| \`'equals'\` | exact value | a text input |
| \`'number-range'\` | \`\[min, max\]\` values within | min/max number inputs |
| \`'date-range'\` | \`\[from, to\]\` dates within (inclusive, by calendar day) | from/to date inputs |
| \`'set'\` | value is one of the checked values | a checkbox list of the column's distinct values, with counts |
| \`'includes-any'\` | array-valued cells containing any checked value | a checkbox list of the arrays' distinct values, with row counts |
| \`'includes-all'\` | array-valued cells containing every checked value | a checkbox list of the arrays' distinct values, with row counts |

When a value list has more than ten entries, the panel adds a search box to narrow the options. Every panel ends with a Clear button that resets just that column's filter.

```html
<wa-data-grid id="grid-filter-types" label="Products" row-key="id"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-filter-types');
  grid.data = [
    { id: 1, name: 'Aurora Floor Lamp', category: 'Lighting', price: 249, restocked: '2026-06-02' },
    { id: 2, name: 'Borealis Wool Rug', category: 'Textiles', price: 429, restocked: '2026-05-11' },
    { id: 3, name: 'Cascade Glass Vase', category: 'Decor', price: 85, restocked: '2026-06-19' },
    { id: 4, name: 'Dusk Wall Sconce', category: 'Lighting', price: 139, restocked: '2026-04-27' },
    { id: 5, name: 'Ember Throw Blanket', category: 'Textiles', price: 69, restocked: '2026-06-25' },
    { id: 6, name: 'Fjord Ceramic Bowl', category: 'Decor', price: 44, restocked: '2026-05-30' },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', filterable: true, flex: 2, minWidth: 160 }, // default substring
    { field: 'category', label: 'Category', filterable: true, filterType: 'set', width: 150 },
    {
      field: 'price',
      label: 'Price',
      align: 'end',
      filterable: true,
      filterType: 'number-range',
      width: 130,
      formatter: v => `$${v}`,
    },
    {
      field: 'restocked',
      label: 'Restocked',
      align: 'end',
      filterable: true,
      filterType: 'date-range',
      sortFn: 'datetime',
      width: 160,
    },
  ];
</script>
```

For full control, supply a custom `filterFn(value, filterValue, row)` that returns `true` to keep a row. It takes precedence over `filterType` and runs client-side only.

```js
grid.columns = [
  {
    field: 'price',
    label: 'Under Budget',
    filterable: true,
    filterFn: (value, filterValue) => Number(value) <= Number(filterValue),
  },
];
```

#### Faceted Values

For each column, the grid computes faceted data (the distinct values with counts, and the numeric range) from the rows _before_ that column's own filter is applied. The `'set'` filter uses this to populate its options, and you can read it yourself with `getColumnFacets(columnId)` to build a custom filter UI:

```js
const { uniqueValues, minMax } = grid.getColumnFacets('category');
// uniqueValues: Map { 'Lighting' => 2, 'Textiles' => 2, 'Decor' => 2 }
// minMax: undefined (non-numeric); for 'price' it would be [44, 429]
```

Faceting is client-mode only. In [server mode](#loading-data-from-a-server), the server owns filtering, so `getColumnFacets` returns empty facets and the `'set'` filter's panel falls back to a plain text box (range filters keep their controls and pass their bounds to your server).

### Column Visibility

Add the `with-columns-menu` attribute to render a menu that lets users show and hide columns. Set `hidden: true` on a column to hide it initially, or `hideable: false` to prevent a column from being toggled. The menu ends with a Reset columns item that restores the default order, widths, visibility, and pinning, also available programmatically as `resetColumns()`. User-initiated show/hide changes emit `wa-column-visibility-change`.

```html
<wa-data-grid id="grid-visibility" label="Web analytics" row-key="id" with-columns-menu></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-visibility');
  grid.data = [
    { id: 1, page: '/', views: 48210, visitors: 31044, bounce: 0.38, avgTime: 142 },
    { id: 2, page: '/pricing', views: 21088, visitors: 16730, bounce: 0.51, avgTime: 96 },
    { id: 3, page: '/docs', views: 53902, visitors: 28115, bounce: 0.22, avgTime: 311 },
    { id: 4, page: '/blog/data-grid', views: 9874, visitors: 8442, bounce: 0.46, avgTime: 188 },
    { id: 5, page: '/contact', views: 4120, visitors: 3880, bounce: 0.62, avgTime: 54 },
  ];
  grid.columns = [
    { field: 'page', label: 'Page', hideable: false, flex: 1, minWidth: 160 },
    { field: 'views', label: 'Views', align: 'end', width: 110, formatter: v => v.toLocaleString() },
    { field: 'visitors', label: 'Visitors', align: 'end', width: 110, formatter: v => v.toLocaleString() },
    { field: 'bounce', label: 'Bounce Rate', align: 'end', width: 130, formatter: v => `${(v * 100).toFixed(0)}%` },
    {
      field: 'avgTime',
      label: 'Avg. Time',
      align: 'end',
      width: 120,
      hidden: true,
      formatter: v => `${Math.floor(v / 60)}m ${v % 60}s`,
    },
  ];
</script>
```

### Resizing Columns

Add the `resizable` attribute to let users drag column borders, or set `resizable` per column. Double-click a handle to auto-size a column to its content, press Alt + Left / Right on a focused header, or call `autoSizeColumn()`, `autoSizeColumns()`, or `sizeColumnsToFit()`. Set a `flex` ratio on a column to share leftover space proportionally; the `wa-column-resize` event fires during and after a resize (check `detail.finished`).

```html
<wa-data-grid id="grid-resize" label="Files" row-key="id" resizable></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-resize');
  grid.data = [
    { id: 1, file: 'Q2-financial-report-final.pdf', type: 'PDF', size: '2.4 MB', modified: 'Jun 12, 2026' },
    { id: 2, file: 'marketing-budget-2026.xlsx', type: 'Spreadsheet', size: '1.1 MB', modified: 'Jun 1, 2026' },
    { id: 3, file: 'product-launch-deck.key', type: 'Keynote', size: '18.7 MB', modified: 'Jun 9, 2026' },
    { id: 4, file: 'brand-guidelines.fig', type: 'Figma', size: '8.7 MB', modified: 'May 28, 2026' },
    { id: 5, file: 'demo-walkthrough.mp4', type: 'Video', size: '124 MB', modified: 'Jun 18, 2026' },
  ];
  grid.columns = [
    { field: 'file', label: 'Name', sortable: true, flex: 2, minWidth: 160 },
    { field: 'type', label: 'Type', flex: 1 },
    { field: 'size', label: 'Size', align: 'end', flex: 1 },
    { field: 'modified', label: 'Modified', align: 'end', sortable: true, flex: 1 },
  ];
</script>
```

### Reordering Columns

Add the `reorderable` attribute to let users drag column headers to reorder them, or set `movable` per column to opt individual columns in or out. Focus a header and press Shift + Left / Right to move it from the keyboard. The `wa-column-move` event fires during the drag and once it settles (check `detail.finished`); `detail.columnOrder` holds the new order.

```html
<wa-data-grid id="grid-reorder" label="Roster" row-key="id" reorderable></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-reorder');
  grid.data = [
    { id: 1, name: 'Ava Mitchell', role: 'Staff Engineer', team: 'Platform', location: 'Austin, TX' },
    { id: 2, name: 'Liam Chen', role: 'Senior Engineer', team: 'Growth', location: 'Seattle, WA' },
    { id: 3, name: 'Sofia Rossi', role: 'Design Lead', team: 'Design Systems', location: 'Remote' },
    { id: 4, name: 'Noah Patel', role: 'Product Manager', team: 'Core', location: 'New York, NY' },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, flex: 1, minWidth: 150 },
    { field: 'role', label: 'Role', flex: 1, minWidth: 150 },
    { field: 'team', label: 'Team', flex: 1, minWidth: 130 },
    { field: 'location', label: 'Location', flex: 1, minWidth: 130 },
  ];
</script>
```

### Pinning Columns

Add the `pinnable` attribute to let columns pin to either edge, where they stay visible while the rest of the grid scrolls horizontally. Click a pinned column's pin icon to unpin it, or set `pinnable: false` on a column to opt it out. Add the `with-column-menu` attribute to give each header a menu with pin, sort, hide, and autosize actions.

Start a column pinned with `pinned: 'left'` (or `'right'`) in its definition, or pin programmatically with `pinColumn()` and `getColumnPin()`; pinning is included in `getState()` / `setState()`, and user-initiated pin changes emit the `wa-column-pin` event. Pinned columns keep their natural order, so pin columns already near their edge. Pinning a column without an explicit `width` locks in its current width.

```html
<wa-data-grid
  id="grid-pin"
  label="Team"
  row-key="id"
  pinnable
  with-column-menu
  resizable
  style="max-width: 100%;"
></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-pin');
  grid.data = [
    { id: 1, name: 'Ada Lovelace', team: 'Platform', location: 'London', startDate: '2019-03-01', status: 'Active' },
    { id: 2, name: 'Alan Turing', team: 'Research', location: 'Manchester', startDate: '2018-07-15', status: 'Active' },
    {
      id: 3,
      name: 'Grace Hopper',
      team: 'Infrastructure',
      location: 'New York',
      startDate: '2020-01-10',
      status: 'On leave',
    },
    {
      id: 4,
      name: 'Katherine Johnson',
      team: 'Analytics',
      location: 'Hampton',
      startDate: '2017-11-20',
      status: 'Active',
    },
  ];
  grid.columns = [
    // Pinned left, so the Name column stays put while the rest scrolls.
    { field: 'name', label: 'Name', width: 180, pinned: 'left' },
    { field: 'team', label: 'Team', width: 160 },
    { field: 'location', label: 'Location', width: 175 },
    { field: 'startDate', label: 'Start Date', width: 185 },
    { field: 'status', label: 'Status', width: 160 },
  ];
</script>
```

### Column Footers

Give a column a `footer` to render a totals row pinned to the bottom of the scroll area. A string renders as escaped text; a function receives the current rows (filtered, across every page), so totals always reflect what the user is looking at.

```html
<wa-data-grid id="grid-footers" label="Invoice" row-key="id" with-search></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-footers');
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
  grid.data = [
    { id: 1, item: 'Design sprint', hours: 24, rate: 150 },
    { id: 2, item: 'Component library', hours: 80, rate: 165 },
    { id: 3, item: 'Accessibility audit', hours: 16, rate: 140 },
    { id: 4, item: 'Documentation', hours: 12, rate: 120 },
  ];
  grid.columns = [
    { field: 'item', label: 'Item', footer: 'Total', flex: 2, minWidth: 180 },
    {
      field: 'hours',
      label: 'Hours',
      align: 'end',
      width: 110,
      footer: rows => String(rows.reduce((sum, row) => sum + row.hours, 0)),
    },
    {
      field: 'rate',
      label: 'Rate',
      align: 'end',
      width: 120,
      formatter: value => currency.format(value),
    },
    {
      id: 'amount',
      label: 'Amount',
      align: 'end',
      width: 130,
      formatter: (value, row) => currency.format(row.hours * row.rate),
      footer: rows => currency.format(rows.reduce((sum, row) => sum + row.hours * row.rate, 0)),
    },
  ];
</script>
```

Columns without a `footer` leave their footer cell empty, and hiding a column hides its footer with it. The function receives top-level rows for tree data (so parent and child values aren't double-counted), the underlying data rows for grouped grids (not the group headers), and the currently loaded rows in server mode.

### Grouping Rows

Set the `group-by` attribute to a column id to group rows by that column's values; each group renders as an expandable row showing the value and row count. Give other columns an `aggregation` (`sum`, `min`, `max`, `mean`, `median`, `count`, `unique`, `uniqueCount`, `extent`, or a custom function) to summarize the group on its row. Aggregated values still run through the column's `formatter`.

```html
<wa-data-grid id="grid-grouped" label="Team compensation" group-by="department" row-key="id" selectable></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-grouped');
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
  grid.data = [
    { id: 1, name: 'Ada Lovelace', department: 'Engineering', title: 'Principal Engineer', salary: 186000 },
    { id: 2, name: 'Grace Hopper', department: 'Engineering', title: 'Staff Engineer', salary: 172000 },
    { id: 3, name: 'Margaret Hamilton', department: 'Engineering', title: 'Engineering Lead', salary: 195000 },
    { id: 4, name: 'Katherine Johnson', department: 'Research', title: 'Research Scientist', salary: 158000 },
    { id: 5, name: 'Alan Turing', department: 'Research', title: 'Research Fellow', salary: 164000 },
    { id: 6, name: 'Annie Easley', department: 'Support', title: 'Support Lead', salary: 118000 },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, flex: 2, minWidth: 160 },
    { field: 'department', label: 'Department', flex: 1, minWidth: 150 },
    { field: 'title', label: 'Title', flex: 2, minWidth: 150 },
    {
      field: 'salary',
      label: 'Salary',
      align: 'end',
      sortable: true,
      width: 125,
      aggregation: 'sum',
      formatter: value => currency.format(value),
    },
  ];
  // Start with every group open.
  await customElements.whenDefined('wa-data-grid');
  grid.expandAllRows();
</script>
```

Groups behave like [tree rows](#tree-data): expand them with the toggle (or Enter on the expand cell), or open everything with `expandAllRows()`. Checking a group's checkbox selects its members, though `selectedKeys` only ever contains real rows, never groups. Sorting orders both the groups and their members, filtering regroups the matches, groups paginate as single rows, and CSV export includes the underlying rows without the group headers.

To group by multiple levels, list the columns in the attribute (`group-by="department title"`) or assign an array: `grid.groupBy = ['department', 'title']`. Grouping is ignored with tree data (`child-rows` wins) and in server mode.

To render an aggregated cell with full context, give the column an `aggregatedFormatter`. Unlike `formatter`, it receives the aggregate value and every data row in the group:

```js
grid.columns = [
  {
    field: 'salary',
    label: 'Salary',
    aggregation: 'mean',
    aggregatedFormatter: (value, rows) => `${currency.format(value)} avg of ${rows.length}`,
  },
];
```

**On group rows, use `aggregatedFormatter` for aggregated cells.**  
A column's regular `formatter` receives the group's _first_ row as its second argument (and `wa-row-expand` / `wa-row-collapse` report that row too), so it can't see the whole group.

### Row Detail Panels

Set the `rowDetail` property to a function that returns content for an expandable panel. Each row gains an expand toggle, and the `wa-row-expand` / `wa-row-collapse` events fire as panels open and close. Panels can be any height, and the grid measures each one to keep virtualized scrolling accurate.

```html
<wa-data-grid id="grid-detail" label="Shipments" row-key="id"></wa-data-grid>

<script type="module">
  import { html } from 'https://cdn.jsdelivr.net/npm/lit@3/+esm';

  const grid = document.querySelector('#grid-detail');
  grid.data = [
    {
      id: 1,
      tracking: '1Z999AA10123456784',
      carrier: 'UPS',
      status: 'In transit',
      eta: 'Jun 28',
      origin: 'Austin, TX',
      destination: 'Reno, NV',
      weight: '4.2 lb',
      notes: 'Signature required on delivery.',
    },
    {
      id: 2,
      tracking: '7712 8830 4456',
      carrier: 'FedEx',
      status: 'Out for delivery',
      eta: 'Jun 26',
      origin: 'Miami, FL',
      destination: 'Boise, ID',
      weight: '1.0 lb',
      notes: 'Leave at front door if no answer.',
    },
    {
      id: 3,
      tracking: 'JD0002080000123',
      carrier: 'DHL',
      status: 'Delayed',
      eta: 'Jun 29',
      origin: 'Seattle, WA',
      destination: 'Tampa, FL',
      weight: '12.6 lb',
      notes: 'Fragile — contains glassware.',
    },
  ];
  grid.columns = [
    { field: 'tracking', label: 'Tracking #', flex: 2, minWidth: 180 },
    { field: 'carrier', label: 'Carrier', width: 120 },
    { field: 'status', label: 'Status', flex: 1, minWidth: 140 },
    { field: 'eta', label: 'ETA', align: 'end', width: 100 },
  ];
  grid.rowDetail = row => html`
    <div class="wa-grid" style="--min-column-size: 14ch; gap: var(--wa-space-l);">
      <div>
        <small style="color: var(--wa-color-text-quiet);">Route</small><br />
        <strong>${row.origin}</strong> &rarr; <strong>${row.destination}</strong>
      </div>
      <div><small style="color: var(--wa-color-text-quiet);">Weight</small><br />${row.weight}</div>
      <div style="grid-column: 1 / -1;">
        <small style="color: var(--wa-color-text-quiet);">Notes</small><br />${row.notes}
      </div>
    </div>
  `;
</script>
```

Control expansion programmatically with `expandRow()`, `collapseRow()`, `expandAllRows()`, and `collapseAllRows()`, or read and assign the full set through the `expandedKeys` property.

### Tree Data

To display hierarchical rows, set the `child-rows` attribute to the field that holds each row's children (dot paths work too), or set the `childRows` property to a function for anything more involved. Rows with children get an expand toggle, and expanded children render indented beneath their parent. Adjust the indentation step with the `--indent-size` custom property.

Sorting orders children within their parent, and pagination keeps an expanded subtree on its parent's page. With multiple selection, checking a parent checks all of its descendants (even collapsed ones), a partially selected parent shows an indeterminate checkbox, and select-all reaches into collapsed subtrees too.

```html
<wa-data-grid id="grid-tree" label="Project files" child-rows="children" row-key="id" selectable></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-tree');
  grid.data = [
    {
      id: 1,
      name: 'src',
      kind: 'Folder',
      size: '—',
      children: [
        { id: 11, name: 'index.ts', kind: 'TypeScript', size: '2.1 KB' },
        {
          id: 12,
          name: 'components',
          kind: 'Folder',
          size: '—',
          children: [
            { id: 121, name: 'button.ts', kind: 'TypeScript', size: '6.4 KB' },
            { id: 122, name: 'card.ts', kind: 'TypeScript', size: '3.9 KB' },
          ],
        },
      ],
    },
    {
      id: 2,
      name: 'docs',
      kind: 'Folder',
      size: '—',
      children: [{ id: 21, name: 'getting-started.md', kind: 'Markdown', size: '8.8 KB' }],
    },
    { id: 3, name: 'package.json', kind: 'JSON', size: '1.2 KB' },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, flex: 2, minWidth: 200 },
    { field: 'kind', label: 'Kind', flex: 1, minWidth: 130 },
    { field: 'size', label: 'Size', align: 'end', width: 110 },
  ];
  // Start with the first folder open.
  await customElements.whenDefined('wa-data-grid');
  grid.expandRow(1);
</script>
```

When filtering tree data, a row that doesn't match is removed along with its entire subtree. Add the `filter-from-leaf-rows` attribute to run filters (including the global search) from the leaves up instead, so a parent stays visible whenever any of its descendants match.

Press Enter on a row's expand cell to toggle it from the keyboard. `wa-row-expand` and `wa-row-collapse` fire only when the _user_ toggles a row. Programmatic calls like `expandRow()` don't emit events.

In [server mode](#loading-data-from-a-server), `child-rows` still works when your rows arrive with their children nested, but the request object carries no expansion information, so the grid doesn't lazy-load children on expand.

With tree data, the grid switches to the ARIA `treegrid` role, and rows report `aria-level` and `aria-expanded`. Announced row counts cover only the visible rows, since expanding and collapsing changes what's on screen.

### Empty & Loading States

When there are no rows to display, the grid shows a default empty message. Provide your own content in the `empty` slot to give users guidance or a call to action. When an active search or filter matches nothing, the grid shows a distinct "no results" message instead. Customize it with the `no-results` slot.

```html
<wa-data-grid id="grid-empty" label="Invoices" row-key="id" with-search>
  <div slot="empty" class="wa-stack wa-align-items-center" style="padding: var(--wa-space-2xl) 0; text-align: center;">
    <wa-icon name="file-invoice" style="font-size: 2.5rem; color: var(--wa-color-text-quiet);"></wa-icon>
    <div>
      <strong>No invoices yet</strong>
      <p style="margin: 0.25em 0 0; color: var(--wa-color-text-quiet);">Create your first invoice to see it here.</p>
    </div>
    <wa-button variant="brand">
      <wa-icon slot="start" name="plus"></wa-icon>
      New Invoice
    </wa-button>
  </div>
</wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-empty');
  grid.data = [];
  grid.columns = [
    { field: 'number', label: 'Invoice #' },
    { field: 'client', label: 'Client' },
    { field: 'amount', label: 'Amount', align: 'end' },
  ];
</script>
```

While a [server-mode](#loading-data-from-a-server) request is in flight, the grid sets its `loading` attribute and shows a translucent overlay with a spinner. Slot your own content into the `loading` slot to customize it:

```html
<wa-data-grid label="Orders">
  <div slot="loading" class="wa-stack wa-align-items-center">
    <wa-spinner style="font-size: 2rem;"></wa-spinner>
    <small>Loading orders…</small>
  </div>
</wa-data-grid>
```

### Large Datasets & Virtualization

Rows are virtualized automatically. Only the visible window (plus a small overscan) exists in the DOM, so tens of thousands of rows scroll smoothly without pagination. The body scrolls within a max height of `30rem` by default; set the `--max-height` custom property to change it, or to `none` to let the grid grow with its content. Call `scrollToIndex(index)` to jump to a row programmatically.

```html
<div class="wa-cluster" style="margin-block-end: var(--wa-space-m);">
  <wa-button id="grid-virtual-jump">Jump to Row 5,000</wa-button>
  <wa-button id="grid-virtual-top" appearance="outlined">Back to Top</wa-button>
</div>
<wa-data-grid
  id="grid-virtual"
  label="Sensor readings"
  row-key="id"
  striped
  style="--max-height: 22rem;"
></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-virtual');
  const sensors = ['temp-01', 'temp-02', 'hum-01', 'co2-01', 'lux-01'];

  grid.data = Array.from({ length: 10000 }, (_, i) => ({
    id: i + 1,
    sensor: sensors[i % sensors.length],
    value: Math.round((20 + Math.sin(i / 50) * 5 + (i % 7) * 0.3) * 100) / 100,
    recorded: `2026-06-0${(i % 9) + 1} ${String(i % 24).padStart(2, '0')}:${String(i % 60).padStart(2, '0')}`,
  }));

  grid.columns = [
    { field: 'id', label: 'Reading', align: 'end', width: 110 },
    { field: 'sensor', label: 'Sensor', flex: 1, minWidth: 120 },
    { field: 'value', label: 'Value', align: 'end', sortable: true, width: 120 },
    { field: 'recorded', label: 'Recorded', align: 'end', flex: 1, minWidth: 160 },
  ];

  document.querySelector('#grid-virtual-jump').addEventListener('click', () => {
    grid.scrollToIndex(4999, { align: 'center' });
  });
  document.querySelector('#grid-virtual-top').addEventListener('click', () => grid.scrollToIndex(0));
</script>
```

**Keep a bounded height for very large datasets.**  
Virtualization works because the grid's body has a fixed max height. Setting `--max-height: none` makes the page scroll instead and renders every row, so keep the default (or any fixed value).

### Loading Data from a Server

For large or remote datasets, set the `dataSource` property to an async function. The grid switches to server mode, where sorting, filtering, and pagination become your server's job, and calls `dataSource` whenever they change. Return the current page of `rows` and the `total` row count; the `loading` property reflects whether a request is in flight.

Each request carries an `AbortSignal` that you forward to your `fetch()`. The grid aborts the previous request and ignores out-of-order responses, so rapid changes can't show stale data. Search and filter keystrokes are debounced (250ms by default; tune with the `filter-debounce` attribute), and `reload()` re-runs the current request at any time.

```html
<wa-data-grid id="grid-server" label="Audit log" row-key="id" paginate page-size="10" with-search></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-server');
  grid.columns = [
    { field: 'time', label: 'Time', sortable: true, width: 110 },
    { field: 'user', label: 'User', flex: 1, minWidth: 140 },
    { field: 'action', label: 'Action', flex: 2, minWidth: 170 },
    { field: 'ip', label: 'IP Address', align: 'end', width: 140 },
  ];

  // A fake server. In a real app, fetch() from your API instead.
  const users = ['ava.mitchell', 'liam.chen', 'sofia.rossi', 'noah.patel', 'system'];
  const actions = [
    'Signed in',
    'Updated billing',
    'Invited a member',
    'Exported report',
    'Changed password',
    'Revoked API key',
  ];
  const ALL = Array.from({ length: 480 }, (_, i) => ({
    id: i + 1,
    time: new Date(Date.now() - i * 137000).toISOString().slice(11, 19),
    user: users[i % users.length],
    action: actions[i % actions.length],
    ip: `192.0.2.${i % 255}`,
  }));

  grid.dataSource = async ({ sort, search, page, pageSize }) => {
    await new Promise(r => setTimeout(r, 350)); // simulate latency
    let rows = ALL;
    if (search) {
      const q = search.toLowerCase();
      rows = rows.filter(r => r.user.includes(q) || r.action.toLowerCase().includes(q));
    }
    if (sort[0]) {
      const { id, desc } = sort[0];
      rows = [...rows].sort((a, b) => String(a[id]).localeCompare(String(b[id])) * (desc ? -1 : 1));
    }
    const total = rows.length;
    return { rows: rows.slice(page * pageSize, (page + 1) * pageSize), total };
  };
</script>
```

If a `dataSource` request rejects, the grid keeps the previous rows, emits `wa-data-error` with the error and the failed request, and logs to the console. Listen for it to show a toast or a retry action:

```js
grid.addEventListener('wa-data-error', event => {
  console.error(event.detail.error);
  // Offer a retry:
  // grid.reload();
});
```

#### Event-Driven Server Mode

If you prefer to manage fetching yourself, or you're not using JavaScript callbacks at all, add the `server` attribute instead of setting `dataSource`. The grid disables client-side sorting, filtering, and pagination and emits `wa-data-request` whenever it needs data; fetch in your handler, then set `data`, `total`, and `loading`:

```html
<wa-data-grid server paginate label="Orders"></wa-data-grid>

<script type="module">
  const grid = document.querySelector('wa-data-grid');
  grid.addEventListener('wa-data-request', async event => {
    const { sort, filters, search, page, pageSize, signal } = event.detail;
    grid.loading = true;
    try {
      const response = await fetch(`/api/orders?page=${page}&size=${pageSize}&q=${search}`, { signal });
      const { rows, total } = await response.json();
      grid.data = rows;
      grid.total = total;
    } finally {
      grid.loading = false;
    }
  });
</script>
```

### Responding to Cell Clicks

The grid emits `wa-cell-click` when a data cell is clicked or Enter is pressed on the active cell. Its `detail` carries the `column` id, the cell `value`, the `row` object, and the row's display index, enough to open a record, copy a value, or drive a master–detail view without wiring listeners inside formatters.

```html
<wa-data-grid id="grid-click" label="Customers" row-key="id"></wa-data-grid>
<wa-callout id="grid-click-result" style="margin-block-start: var(--wa-space-m);">
  <wa-icon slot="icon" name="circle-info"></wa-icon>
  Click a cell to inspect it.
</wa-callout>

<script type="module">
  const grid = document.querySelector('#grid-click');
  const result = document.querySelector('#grid-click-result');

  grid.data = [
    { id: 1, company: 'Globex', contact: 'Hank Scorpio', country: 'USA', orders: 42 },
    { id: 2, company: 'Initech', contact: 'Bill Lumbergh', country: 'USA', orders: 17 },
    { id: 3, company: 'Hooli', contact: 'Gavin Belson', country: 'USA', orders: 64 },
    { id: 4, company: 'Pied Piper', contact: 'Richard Hendricks', country: 'USA', orders: 8 },
  ];
  grid.columns = [
    { field: 'company', label: 'Company', flex: 1, minWidth: 130 },
    { field: 'contact', label: 'Contact', flex: 1, minWidth: 150 },
    { field: 'country', label: 'Country', width: 110 },
    { field: 'orders', label: 'Orders', align: 'end', width: 100 },
  ];

  grid.addEventListener('wa-cell-click', event => {
    const { column, value, row } = event.detail;
    result.textContent = `You clicked ${column} = "${value}" in the ${row.company} row.`;
  });
</script>
```

Right-clicks (and Shift + F10 on the active cell) emit `wa-cell-contextmenu` with the same payload plus the `originalEvent`. Cancel the event to suppress the browser's native menu and show your own:

```js
grid.addEventListener('wa-cell-contextmenu', event => {
  event.preventDefault(); // suppress the native context menu
  openMyMenu(event.detail.row, event.detail.originalEvent);
});
```

### Copying to the Clipboard

Press Ctrl + C (or Cmd + C) with rows selected to copy them, or call `copySelectedRows()` yourself, which copies every processed row when nothing is selected. The default tab-separated format pastes straight into spreadsheet cells; pass `format: 'csv'` for comma-separated text. The copy honors the active sort, filters, and column visibility and order.

```html
<wa-data-grid id="grid-copy" label="Monthly expenses" row-key="id" selectable></wa-data-grid>
<wa-button id="grid-copy-button" style="margin-block-start: var(--wa-space-m);">
  <wa-icon slot="start" name="copy"></wa-icon>
  Copy Selected Rows
</wa-button>

<script type="module">
  const grid = document.querySelector('#grid-copy');
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
  grid.data = [
    { id: 1, category: 'Payroll', budget: 480000, actual: 472500 },
    { id: 2, category: 'Cloud hosting', budget: 62000, actual: 68400 },
    { id: 3, category: 'Marketing', budget: 145000, actual: 132900 },
    { id: 4, category: 'Office & travel', budget: 38000, actual: 41200 },
  ];
  grid.columns = [
    { field: 'category', label: 'Category', flex: 1, minWidth: 160 },
    { field: 'budget', label: 'Budget', align: 'end', width: 130, formatter: v => currency.format(v) },
    { field: 'actual', label: 'Actual', align: 'end', width: 130, formatter: v => currency.format(v) },
  ];
  document.querySelector('#grid-copy-button').addEventListener('click', () => grid.copySelectedRows());
</script>
```

### CSV Export

Call `grid.exportDataAsCsv()` to download the current rows as a CSV file, or `getDataAsCsv()` to get the text without a download. The export respects the active sort, filters, search, and column visibility and order, applying each `formatter` that returns a string (rich-template formatters fall back to the raw value). It includes every page and every tree depth, expanded or not; in server mode, only the currently loaded page is exported.

```html
<wa-button id="grid-csv-button" appearance="filled" variant="brand">
  <wa-icon slot="start" name="download"></wa-icon>
  Export CSV
</wa-button>
<wa-data-grid
  id="grid-csv"
  label="Sales by region"
  row-key="id"
  with-search
  style="margin-block-start: var(--wa-space-m);"
></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-csv');
  const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
  grid.data = [
    { id: 1, product: 'Aeron Office Chair', region: 'North America', units: 312, revenue: 435240 },
    { id: 2, product: 'Standing Desk', region: 'Europe', units: 204, revenue: 152796 },
    { id: 3, product: '4K Monitor', region: 'Asia Pacific', units: 588, revenue: 311052 },
    { id: 4, product: 'Mechanical Keyboard', region: 'North America', units: 1140, revenue: 181260 },
    { id: 5, product: 'LED Desk Lamp', region: 'Europe', units: 2030, revenue: 129920 },
  ];
  grid.columns = [
    { field: 'product', label: 'Product', sortable: true, flex: 2, minWidth: 180 },
    { field: 'region', label: 'Region', sortable: true, flex: 1, minWidth: 150 },
    { field: 'units', label: 'Units', align: 'end', sortable: true, width: 120, formatter: v => v.toLocaleString() },
    {
      field: 'revenue',
      label: 'Revenue',
      align: 'end',
      sortable: true,
      width: 130,
      formatter: v => currency.format(v),
    },
  ];
  document.querySelector('#grid-csv-button').addEventListener('click', () => {
    grid.exportDataAsCsv({ fileName: 'sales-by-region.csv' });
  });
</script>
```

**Exporting user-generated data to a spreadsheet? Pass `escapeFormulas: true`.**  
It neutralizes cells that would otherwise execute as formulas (values starting with `=`, `+`, `-`, or `@`); plain numbers are left untouched.

### Saving & Restoring State

Call `grid.getState()` for a serializable snapshot of the column order, widths, visibility, pinning, sort, filters, search, selection, expansion, and paging, and pass it back to `grid.setState()` to restore it, for example from `localStorage`. Use `resetState()` to return the view (including expansion) to the column defaults, leaving selection and paging untouched. On a grouped grid, the snapshot's `expandedKeys` include group ids, so they only restore meaningfully while `group-by` stays the same.

```html
<div class="wa-cluster">
  <wa-button id="grid-state-save">Save Layout</wa-button>
  <wa-button id="grid-state-restore">Restore Layout</wa-button>
  <wa-button id="grid-state-reset">Reset</wa-button>
</div>
<wa-data-grid
  id="grid-state"
  label="Settings"
  row-key="id"
  resizable
  reorderable
  with-columns-menu
  style="margin-block-start: var(--wa-space-m);"
></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-state');
  grid.data = [
    { id: 1, name: 'Ava Mitchell', role: 'Staff Engineer', team: 'Platform', location: 'Austin, TX' },
    { id: 2, name: 'Liam Chen', role: 'Senior Engineer', team: 'Growth', location: 'Seattle, WA' },
    { id: 3, name: 'Sofia Rossi', role: 'Design Lead', team: 'Design Systems', location: 'Remote' },
    { id: 4, name: 'Noah Patel', role: 'Product Manager', team: 'Core', location: 'New York, NY' },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', sortable: true, flex: 1, minWidth: 150 },
    { field: 'role', label: 'Role', sortable: true, flex: 1, minWidth: 150 },
    { field: 'team', label: 'Team', flex: 1, minWidth: 130 },
    { field: 'location', label: 'Location', flex: 1, minWidth: 130 },
  ];

  document.querySelector('#grid-state-save').addEventListener('click', () => {
    localStorage.setItem('grid-state', JSON.stringify(grid.getState()));
  });
  document.querySelector('#grid-state-restore').addEventListener('click', () => {
    const saved = localStorage.getItem('grid-state');
    if (saved) grid.setState(JSON.parse(saved));
  });
  document.querySelector('#grid-state-reset').addEventListener('click', () => grid.resetState());
</script>
```

### Keyboard Navigation

The grid follows the [ARIA grid pattern](https://www.w3.org/WAI/ARIA/apg/patterns/grid/). The whole grid is a single tab stop; once focused, navigate cells with the keyboard:

-   Up Down Left Right — move between cells (and into the header)
-   Home / End — first / last cell in the row
-   Ctrl + Home / End — first / last cell in the grid
-   Page Up / Page Down — move by a page of rows
-   Enter — sort the focused header column, toggle expansion from a row's expand cell, or activate a data cell (emits `wa-cell-click`)
-   Space — toggle selection of the focused row
-   Shift + Up / Down — extend the selection
-   Ctrl + A — select all rows (the current page's rows when paginated)
-   Ctrl + C — copy the selected rows
-   Shift + F10 — open the context menu for the focused cell (emits `wa-cell-contextmenu`)
-   Shift + Left / Right — reorder the focused header column
-   Alt + Left / Right — resize the focused header column

```html
<wa-data-grid id="grid-keyboard" label="Try the keyboard" row-key="id" selectable resizable reorderable></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-keyboard');
  const cities = [
    'Tokyo',
    'Delhi',
    'Shanghai',
    'São Paulo',
    'Cairo',
    'Mumbai',
    'Beijing',
    'Dhaka',
    'Mexico City',
    'Osaka',
  ];
  grid.data = cities.map((city, i) => ({
    id: i + 1,
    rank: i + 1,
    city,
    population: [37400068, 32900000, 28500000, 22400000, 21750000, 20960000, 20460000, 20240000, 21800000, 19060000][i],
  }));
  grid.columns = [
    { field: 'rank', label: 'Rank', align: 'end', width: 80 },
    { field: 'city', label: 'City', sortable: true, flex: 1, minWidth: 150 },
    {
      field: 'population',
      label: 'Population',
      align: 'end',
      sortable: true,
      width: 150,
      formatter: v => v.toLocaleString(),
    },
  ];
</script>
```

### Preloading Rendered Components

A `formatter`'s content renders _inside the grid's shadow DOM_, where Web Awesome's [autoloader](https://webawesome.com/docs/) can't see it, because its observer only watches the main document. A component rendered only by a formatter may never register, leaving cells empty or unstyled.

To avoid this, register every component your formatters render up front. If you import Web Awesome components individually, import the registration module for each one:

```js
import '@awesome.me/webawesome/dist/components/avatar/avatar.js';
import '@awesome.me/webawesome/dist/components/badge/badge.js';
import '@awesome.me/webawesome/dist/components/rating/rating.js';
```

If you rely on the autoloader, list the components your formatters use in a space-separated `data-wa-preload` attribute. `<html>` (or `<body>`) is the most robust spot since it registers regardless of when the grid mounts, but any element in the main document works, including the grid itself:

```html
<html data-wa-preload="wa-avatar wa-badge wa-rating">
  <!-- … -->
</html>
```

This applies to anything rendered into the grid's shadow DOM: formatters, row detail panels, and your own custom elements alike. Components placed directly in your HTML (such as the `empty` and `loading` slots) live in the main document and autoload normally.

### Styling Rendered Components

As with [preloading](#preloading-rendered-components), formatter content lives in the grid's shadow DOM, so page-level CSS can't reach it. To apply custom styles to the components your formatters render, across every row at once, adopt a stylesheet into the grid's shadow root:

```js
const grid = document.querySelector('#grid');

const sheet = new CSSStyleSheet();
sheet.replaceSync(`
  .row-badge::part(base) {
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }
`);

grid.shadowRoot.adoptedStyleSheets = [...grid.shadowRoot.adoptedStyleSheets, sheet];
```

Give the components you render a class (or data attribute) in your formatter so the stylesheet can target them:

```js
grid.columns = [
  {
    field: 'status',
    label: 'Status',
    formatter: value => html`<wa-badge class="row-badge" variant="success">${value}</wa-badge>`,
  },
];
```

To style whole rows conditionally, set the `rowClass` property to a function that returns class names per row, and target them from the same adopted stylesheet:

```js
grid.rowClass = row => (row.overdue ? 'overdue' : null);
sheet.replaceSync(`.row.overdue { background-color: var(--wa-color-danger-fill-quiet); }`);
```

Always scope these selectors to a unique class or data attribute of your own (like `.row-badge` above). The grid's shadow root uses Web Awesome class names and CSS parts for its own markup, so broad selectors such as `wa-badge`, `::part(base)`, or `.cell` collide with those internals and can break the grid's appearance.

### Theming

The grid exposes a curated set of CSS custom properties for theming: `--accent-color` drives the checkboxes and pinned-column highlights, and the rest tune backgrounds, borders, spacing, and the scroll area's `--max-height`. Each defaults to a Web Awesome design token, so grids adapt to light and dark themes automatically.

When theming with a raw palette color, mix it with a neutral surface or text token (as below) so the result follows the surface between light and dark themes. This example tints every customizable region with the same purple; the effect shows on row hover, selection, and keyboard focus.

```html
<wa-data-grid
  id="grid-theme"
  label="Themed"
  row-key="id"
  selectable
  striped
  style="
    --accent-color: var(--wa-color-purple-60);
    --border-color: color-mix(in oklab, var(--wa-color-purple-60) 25%, var(--wa-color-surface-border));
    --border-radius: var(--wa-border-radius-l);
    --header-background: color-mix(in oklab, var(--wa-color-purple-60) 10%, var(--wa-color-surface-default));
    --header-text-color: color-mix(in oklab, var(--wa-color-purple-60) 35%, var(--wa-color-text-normal));
    --stripe-background: color-mix(in oklab, var(--wa-color-purple-60) 6%, var(--wa-color-surface-default));
    --row-hover-background: color-mix(in oklab, var(--wa-color-purple-60) 12%, var(--wa-color-surface-default));
    --selected-background: color-mix(in oklab, var(--wa-color-purple-60) 20%, var(--wa-color-surface-default));
    --focus-ring: solid var(--wa-focus-ring-width) var(--wa-color-purple-60);
  "
></wa-data-grid>

<script type="module">
  const grid = document.querySelector('#grid-theme');
  grid.data = [
    { id: 1, name: 'Ava Mitchell', role: 'Staff Engineer', team: 'Platform' },
    { id: 2, name: 'Liam Chen', role: 'Senior Engineer', team: 'Growth' },
    { id: 3, name: 'Sofia Rossi', role: 'Design Lead', team: 'Design Systems' },
    { id: 4, name: 'Noah Patel', role: 'Product Manager', team: 'Core' },
    { id: 5, name: 'Emma Novak', role: 'Engineering Manager', team: 'Platform' },
    { id: 6, name: 'Lucas Moreau', role: 'UX Researcher', team: 'Design Systems' },
  ];
  grid.columns = [
    { field: 'name', label: 'Name', flex: 1, minWidth: 150 },
    { field: 'role', label: 'Role', flex: 1, minWidth: 150 },
    { field: 'team', label: 'Team', flex: 1, minWidth: 130 },
  ];
</script>
```
