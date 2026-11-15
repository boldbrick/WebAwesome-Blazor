// @ts-check
const fs = require('fs');
const { expect } = require('../test');
const { allowEvent, callMethod, callOk, cancelEvent } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "pro-data" (tools\e2e\data\method-groups.json): WaDataGrid, WaCombobox and WaFileInput on
// /testing/methods-pro-data (MethodsProDataHarness.razor). Every call runs from .NET through the wrapper's @ref (a
// MethodCall button); the case asserts the effect on the element, and the returned value where there is one. All three
// elements are Pro components, so every case skips on the free CDN.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const GRID = 'wa-data-grid';
const COMBOBOX = 'wa-combobox';
const FILE_INPUT = 'wa-file-input';

// the custom validity message of the harness
const VALIDITY_MESSAGE = 'Pick one, please';

// the rows of the tree grid: 60 top-level orders, orders 1 and 2 with two children each
const TOP_LEVEL_ROWS = 60;
const ALL_ROWS = 64;

// the most room a fitted grid leaves at its right edge: its vertical scroll bar and border
const FIT_TOLERANCE_PX = 24;

// calls that must not move the focus or close a popup (see callMethod)
const QUIET = { quiet: true };

// how long a call may take before the cancelled-show case calls it hanging
const SETTLE_TIMEOUT_MS = 5_000;

// the delay of the element modules in the first-render case, so the first render certainly comes before the definitions
// (long enough for the circuit of a prerendered page to start first)
const MODULE_DELAY_MS = 4_000;

/**
 * The texts of one column's rendered data cells of a grid, in row order.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} columnId
 * @returns {Promise<string[]>}
 */
function columnTexts(page, testId, columnId) {
  return page.getByTestId(testId).evaluate((el, id) => [...(el.shadowRoot?.querySelectorAll(`[part~="cell"][data-col-id="${id}"]`) ?? [])]
    .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex))
    .map(cell => (cell.textContent ?? '').trim()), columnId);
}

/**
 * The column ids of the first rendered row of the tree grid, in their visual order (left to right), and the width of
 * each.
 *
 * @param {Page} page
 * @returns {Promise<{ ids: string[], widths: Record<string, number> }>}
 */
function firstRowColumns(page) {
  return page.getByTestId('grid').evaluate(el => {
    const cells = [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-row-index="0"][data-col-id]') ?? [])]
      .map(cell => ({ id: /** @type {HTMLElement} */ (cell).dataset.colId ?? '', rect: cell.getBoundingClientRect() }))
      .sort((a, b) => a.rect.left - b.rect.left);
    return { ids: cells.map(c => c.id), widths: Object.fromEntries(cells.map(c => [c.id, Math.round(c.rect.width)])) };
  });
}

/**
 * Whether the element with the test id is the document's focused element (focus inside its shadow root included).
 *
 * @param {Page} page
 * @param {string} testId
 */
function hasFocus(page, testId) {
  return page.getByTestId(testId).evaluate(el => document.activeElement === el);
}

/**
 * The custom validity state of a form control.
 *
 * @param {Page} page
 * @param {string} testId
 */
function validityOf(page, testId) {
  return page.getByTestId(testId).evaluate(el => {
    const control = /** @type {any} */ (el);
    return { message: control.validationMessage, customError: control.validity.customError };
  });
}

/**
 * The texts of the combobox options a user can see.
 *
 * @param {Page} page
 */
async function visibleOptions(page) {
  const options = page.getByTestId('combobox').locator('wa-option');
  const texts = [];
  for (const option of await options.all()) {
    if (await option.isVisible()) texts.push(((await option.textContent()) ?? '').trim());
  }
  return texts;
}

/**
 * Waits until a first-render call of the harness has completed, and expects it to have succeeded.
 *
 * @param {Page} page
 * @param {string} testId
 */
async function expectEarlyOutcome(page, testId) {
  const output = page.getByTestId(testId);
  await expect(output, `${testId} was called`).not.toHaveText('not called', { timeout: WA_READY_TIMEOUT_MS });
  await expect(output, `${testId} completed without an error`).toHaveText('done');
}

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-pro-data?early=true';

/**
 * Delays the element modules of the group, so the first-render calls certainly come before the definitions.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['data-grid', 'combobox', 'file-input']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'pro-data',

  cases: [
    {
      name: 'WaDataGrid expands and collapses tree rows and returns the visible and processed rows',
      methods: ['WaDataGrid.ExpandRowAsync', 'WaDataGrid.CollapseRowAsync', 'WaDataGrid.ExpandAllRowsAsync',
        'WaDataGrid.CollapseAllRowsAsync', 'WaDataGrid.GetVisibleRowsAsync', 'WaDataGrid.GetProcessedRowsAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await expect.poll(() => columnTexts(page, 'grid', 'id'), { timeout: WA_READY_TIMEOUT_MS }).toContain('1');

        const collapsed = await callOk(page, 'grid-visible-rows');
        expect(collapsed).toHaveLength(TOP_LEVEL_ROWS);
        expect(collapsed.slice(0, 2)).toEqual(['1', '2']);

        await callOk(page, 'grid-expand-row');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t.slice(0, 4))).toEqual(['1', '101', '102', '2']);
        expect(await callOk(page, 'grid-visible-rows')).toHaveLength(TOP_LEVEL_ROWS + 2);

        await callOk(page, 'grid-expand-all');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t.slice(0, 7))).toEqual(['1', '101', '102', '2', '201', '202', '3']);
        expect(await callOk(page, 'grid-visible-rows')).toHaveLength(ALL_ROWS);

        await callOk(page, 'grid-collapse-row');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t.slice(0, 4))).toEqual(['1', '2', '201', '202']);

        await callOk(page, 'grid-collapse-all');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t.slice(0, 3))).toEqual(['1', '2', '3']);
        expect(await callOk(page, 'grid-visible-rows')).toHaveLength(TOP_LEVEL_ROWS);

        // every tree depth, parents before their children, whatever is expanded
        const processed = await callOk(page, 'grid-processed-rows');
        expect(processed).toHaveLength(ALL_ROWS);
        expect(processed.slice(0, 4)).toEqual(['1', '101', '102', '2']);
      },
    },
    {
      name: 'WaDataGrid hides, toggles, resets and pins columns',
      methods: ['WaDataGrid.ToggleColumnAsync', 'WaDataGrid.ResetColumnsAsync', 'WaDataGrid.PinColumnAsync', 'WaDataGrid.GetColumnPinAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await expect.poll(async () => (await firstRowColumns(page)).ids, { timeout: WA_READY_TIMEOUT_MS }).toEqual(['id', 'name', 'region', 'amount', 'note']);

        await callOk(page, 'grid-hide-region');
        await expect.poll(async () => (await firstRowColumns(page)).ids).toEqual(['id', 'name', 'amount', 'note']);
        await callOk(page, 'grid-toggle-region');
        await expect.poll(async () => (await firstRowColumns(page)).ids).toEqual(['id', 'name', 'region', 'amount', 'note']);

        await callOk(page, 'grid-hide-region');
        await expect.poll(async () => (await firstRowColumns(page)).ids).toEqual(['id', 'name', 'amount', 'note']);
        await callOk(page, 'grid-reset-columns');
        await expect.poll(async () => (await firstRowColumns(page)).ids).toEqual(['id', 'name', 'region', 'amount', 'note']);

        expect(await callOk(page, 'grid-get-pin'), 'an unpinned column').toBeNull();
        await callOk(page, 'grid-pin-region');
        await expect.poll(async () => (await firstRowColumns(page)).ids[0], { message: 'the pinned column moves to the left edge' }).toBe('region');
        expect(await callOk(page, 'grid-get-pin')).toBe('left');
        await callOk(page, 'grid-unpin-region');
        await expect.poll(async () => (await firstRowColumns(page)).ids).toEqual(['id', 'name', 'region', 'amount', 'note']);
        expect(await callOk(page, 'grid-get-pin')).toBeNull();
      },
    },
    {
      name: 'WaDataGrid sizes columns to their content and to the grid width',
      methods: ['WaDataGrid.AutoSizeColumnAsync', 'WaDataGrid.AutoSizeColumnsAsync', 'WaDataGrid.SizeColumnsToFitAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await expect.poll(async () => (await firstRowColumns(page)).ids, { timeout: WA_READY_TIMEOUT_MS }).toHaveLength(5);
        const initial = (await firstRowColumns(page)).widths;

        // the note column (120 px) is narrower than its content
        await callOk(page, 'grid-autosize-column');
        await expect.poll(async () => (await firstRowColumns(page)).widths.note, { message: 'the note column grows to its content' }).toBeGreaterThan(initial.note);
        const afterOne = (await firstRowColumns(page)).widths;
        expect(afterOne.region, 'the other columns keep their width').toBe(initial.region);

        await callOk(page, 'grid-autosize-columns');
        // the id column (200 px) is far wider than its content
        await expect.poll(async () => (await firstRowColumns(page)).widths.id, { message: 'every column fits its content' }).toBeLessThan(initial.id);

        // the gap between the last column and the grid's right edge: negative when the columns overflow
        const gap = () => page.getByTestId('grid').evaluate(el => {
          const cells = [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-row-index="0"][data-col-id]') ?? [])];
          return Math.round(el.getBoundingClientRect().right - Math.max(...cells.map(c => c.getBoundingClientRect().right)));
        });
        const before = await gap();
        await callOk(page, 'grid-size-to-fit');
        await expect.poll(gap, { message: 'the columns end at the grid edge (the vertical scroll bar and the border aside)' }).not.toBe(before);
        expect(await gap()).toBeGreaterThanOrEqual(0);
        expect(await gap()).toBeLessThan(FIT_TOLERANCE_PX);
      },
    },
    {
      name: 'WaDataGrid sets, returns and resets its state',
      methods: ['WaDataGrid.SetStateAsync', 'WaDataGrid.GetStateAsync', 'WaDataGrid.ResetStateAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0]), { timeout: WA_READY_TIMEOUT_MS }).toBe('1');

        // ascending amount: the last order first; rows 3 and 4 selected
        await callOk(page, 'grid-set-state');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0])).toBe('60');
        const state = await callOk(page, 'grid-get-state');
        expect(state.sort).toEqual([{ id: 'amount', desc: false }]);
        expect(state.selectedKeys.map(String).sort()).toEqual(['3', '4']);

        // the view state goes, the selection stays
        await callOk(page, 'grid-reset-state');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0])).toBe('1');
        const reset = await callOk(page, 'grid-get-state');
        expect(reset.sort ?? []).toEqual([]);
        expect(reset.selectedKeys.map(String).sort()).toEqual(['3', '4']);
      },
    },
    {
      name: 'WaDataGrid returns, exports and copies its rows as CSV',
      methods: ['WaDataGrid.GetDataAsCsvAsync', 'WaDataGrid.ExportDataAsCsvAsync', 'WaDataGrid.CopySelectedRowsAsync'],
      tags: [GRID],
      pro: true,
      clipboard: true,
      run: async page => {
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0]), { timeout: WA_READY_TIMEOUT_MS }).toBe('1');

        // every row of every depth, the chosen columns only
        const csv = String(await callOk(page, 'grid-get-csv')).trim().split(/\r?\n/);
        expect(csv[0]).toBe('Id,Name');
        expect(csv.slice(1, 4)).toEqual(['1,Order 1', '101,Order 1.1', '102,Order 1.2']);
        expect(csv).toHaveLength(ALL_ROWS + 1);

        const [download] = await Promise.all([page.waitForEvent('download'), callOk(page, 'grid-export-csv')]);
        expect(download.suggestedFilename()).toBe('orders.csv');
        const exported = fs.readFileSync(await download.path(), 'utf8').replace(/^﻿/, '').split(/\r?\n/);
        expect(exported[0]).toBe('Id,Name');
        expect(exported[1]).toBe('1,Order 1');

        // the selected rows (3 and 4, selected through the state) go to the clipboard, without headers
        await callOk(page, 'grid-set-state');
        expect(await callOk(page, 'grid-copy'), 'the number of copied rows').toBe(2);
        const copied = await page.evaluate(() => navigator.clipboard.readText());
        expect(copied).toContain('Order 3');
        expect(copied).toContain('Order 4');
        expect(copied).not.toContain('Order 5');
        expect(copied).not.toContain('Name');
      },
    },
    {
      name: 'WaDataGrid takes the focus and scrolls a row into view',
      methods: ['WaDataGrid.FocusAsync', 'WaDataGrid.ScrollToIndexAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0]), { timeout: WA_READY_TIMEOUT_MS }).toBe('1');

        await callOk(page, 'grid-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'grid')).toBe(true);

        // display index 50 is order 51, far below the 260 px viewport
        await callOk(page, 'grid-scroll');
        await expect.poll(() => page.getByTestId('grid').evaluate(el => {
          const cell = [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-col-id="id"]') ?? [])].find(c => c.textContent?.trim() === '51');
          if (!cell) return 'not rendered';
          const box = cell.getBoundingClientRect();
          const grid = el.getBoundingClientRect();
          return box.top >= grid.top && box.bottom <= grid.bottom ? 'in view' : 'out of view';
        }), { message: 'order 51 is scrolled into view' }).toBe('in view');
      },
    },
    {
      name: 'WaDataGrid.ReloadAsync and the server-mode state methods send a new wa-data-request',
      methods: ['WaDataGrid.ReloadAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        const requests = async () => Number(await page.getByTestId('server-request-count').textContent());
        await expect.poll(requests, { message: 'the first page was requested', timeout: WA_READY_TIMEOUT_MS }).toBe(1);
        await expect.poll(() => columnTexts(page, 'server-grid', 'id')).toEqual(['1', '2', '3']);

        // unchanged parameters: the reload still sends a request
        await callOk(page, 'server-reload');
        await expect.poll(requests).toBe(2);

        await callOk(page, 'server-set-state');
        await expect.poll(requests).toBe(3);
        await expect(page.getByTestId('server-last-page')).toHaveText('1');
        await expect.poll(() => columnTexts(page, 'server-grid', 'id')).toEqual(['4', '5', '6']);

        // the reset leaves the paging alone, but requests again
        await callOk(page, 'server-reset-state');
        await expect.poll(requests).toBe(4);

        // a client-mode grid has nothing to reload: the call completes and changes nothing (server mode only)
        await callOk(page, 'grid-reload-client');
        await expect.poll(() => columnTexts(page, 'grid', 'id').then(t => t[0])).toBe('1');
      },
    },
    {
      name: 'WaCombobox takes and leaves the focus, opens and closes, and reports a custom validity',
      methods: ['WaCombobox.FocusAsync', 'WaCombobox.BlurAsync', 'WaCombobox.ShowAsync', 'WaCombobox.HideAsync',
        'WaCombobox.SetCustomValidityAsync', 'WaCombobox.ResetValidityAsync'],
      tags: [COMBOBOX],
      pro: true,
      run: async page => {
        const combobox = page.getByTestId('combobox');

        await callOk(page, 'combobox-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'combobox')).toBe(true);
        await callOk(page, 'combobox-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'combobox')).toBe(false);

        // the Promise of show and hide settles after the animation, so the call completes with the element open or closed
        await callOk(page, 'combobox-show', QUIET);
        expect(await combobox.evaluate(el => /** @type {any} */ (el).open)).toBe(true);
        await expect(combobox.locator('wa-option').first()).toBeVisible();
        await callOk(page, 'combobox-hide', QUIET);
        expect(await combobox.evaluate(el => /** @type {any} */ (el).open)).toBe(false);

        // WAB-64 F9 (Web Awesome side, recheck on upgrade): show() on an open combobox closes it; asserted as it is
        await callOk(page, 'combobox-show-twice', QUIET);
        expect(await combobox.evaluate(el => /** @type {any} */ (el).open), 'WA 3.14.0: a second show closes the combobox').toBe(false);

        await callOk(page, 'combobox-set-validity');
        expect(await validityOf(page, 'combobox')).toEqual({ message: VALIDITY_MESSAGE, customError: true });
        await callOk(page, 'combobox-reset-validity');
        expect(await validityOf(page, 'combobox')).toEqual({ message: '', customError: false });
      },
    },
    {
      name: 'WaCombobox.ReloadAsync sends a new wa-options-request in server mode',
      methods: ['WaCombobox.ReloadAsync'],
      tags: [COMBOBOX],
      pro: true,
      run: async page => {
        const requests = async () => Number(await page.getByTestId('server-combobox-request-count').textContent());
        expect(await requests(), 'a closed server combobox requests nothing').toBe(0);
        await callOk(page, 'server-combobox-reload');
        await expect.poll(requests).toBe(1);
      },
    },
    {
      name: 'WaFileInput takes and leaves the focus and reports a custom validity',
      methods: ['WaFileInput.FocusAsync', 'WaFileInput.BlurAsync', 'WaFileInput.SetCustomValidityAsync', 'WaFileInput.ResetValidityAsync'],
      tags: [FILE_INPUT],
      pro: true,
      run: async page => {
        await callOk(page, 'file-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'file-input')).toBe(true);
        await callOk(page, 'file-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'file-input')).toBe(false);

        await callOk(page, 'file-set-validity');
        expect(await validityOf(page, 'file-input')).toEqual({ message: VALIDITY_MESSAGE, customError: true });
        await callOk(page, 'file-reset-validity');
        expect(await validityOf(page, 'file-input')).toEqual({ message: '', customError: false });
      },
    },
    {
      // the Promise of show() and hide() waits for the after-event, which a cancelled action never dispatches; the
      // wrapper completes the call when the action is cancelled, as dialog, drawer and dropdown do
      name: 'WaCombobox.ShowAsync and HideAsync complete when wa-show and wa-hide are cancelled',
      methods: ['WaCombobox.ShowAsync', 'WaCombobox.HideAsync'],
      tags: [COMBOBOX],
      pro: true,
      run: async page => {
        const isOpen = () => page.getByTestId('combobox').evaluate(el => /** @type {any} */ (el).open);
        await cancelEvent(page, 'combobox', 'wa-show');
        const shown = await callMethod(page, 'combobox-show', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(shown.done, 'the cancelled show completes').toBe(true);
        expect(await isOpen(), 'the cancelled show leaves the combobox closed').toBe(false);

        await allowEvent(page, 'combobox', 'wa-show');
        await callOk(page, 'combobox-show', QUIET);
        expect(await isOpen(), 'the combobox opens').toBe(true);

        await cancelEvent(page, 'combobox', 'wa-hide');
        const hidden = await callMethod(page, 'combobox-hide', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(hidden.done, 'the cancelled hide completes').toBe(true);
        expect(await isOpen(), 'the cancelled hide leaves the combobox open').toBe(true);
      },
    },
    {
      // reload() is for server mode only (documented): a local combobox filters its own options as the query changes,
      // and the call returns at once without filtering again
      name: 'WaCombobox.ReloadAsync completes and changes nothing on a local combobox',
      methods: ['WaCombobox.ReloadAsync'],
      tags: [COMBOBOX],
      pro: true,
      run: async page => {
        const input = page.getByTestId('combobox').getByRole('combobox');
        await input.click();
        await input.pressSequentially('ch');
        await expect.poll(() => visibleOptions(page)).toEqual(['Cherry']);

        // Banana becomes Chestnut in place; the filter is not run again, so the renamed option stays filtered out
        await callOk(page, 'combobox-rename-and-reload', QUIET);
        expect(await visibleOptions(page)).toEqual(['Cherry']);
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaDataGrid.ReloadAsync', 'WaCombobox.FocusAsync'],
      route: EARLY_ROUTE,
      tags: [GRID, COMBOBOX, FILE_INPUT],
      pro: true,
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-server-reload', 'early-combobox-focus']) await expectEarlyOutcome(page, id);
        await expect.poll(() => hasFocus(page, 'combobox'), { message: 'the combobox has the focus' }).toBe(true);
        await expect.poll(() => columnTexts(page, 'server-grid', 'id'), { message: 'the server grid shows its first page' }).toEqual(['1', '2', '3']);
      },
    },
    {
      // the interop's setCustomValidity waits for the definition as invokeMethod does (WAB-68): before the element
      // module has loaded, the element has no setCustomValidity yet
      name: 'timing: WaFileInput.SetCustomValidityAsync on the first render waits until the element is defined',
      methods: ['WaFileInput.SetCustomValidityAsync'],
      route: EARLY_ROUTE,
      tags: [FILE_INPUT],
      pro: true,
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        await expectEarlyOutcome(page, 'early-file-validity');
        expect(await validityOf(page, 'file-input')).toEqual({ message: VALIDITY_MESSAGE, customError: true });
      },
    },
    {
      name: 'timing: a call on a disposed wrapper fails with an error and leaves Blazor running',
      methods: ['WaDataGrid.GetStateAsync'],
      tags: [GRID],
      pro: true,
      run: async page => {
        await page.getByTestId('disposable-remove').click();
        await expect(page.getByTestId('disposable-grid')).toHaveCount(0);

        const outcome = await callMethod(page, 'disposed-get-state');
        expect(outcome.error, 'the stale reference fails the call').toContain("Failed to invoke method 'getState'");

        // the page still works after the failed call
        await callOk(page, 'grid-visible-rows');
      },
    },
  ],

  knownDefects: [],

  external: [],
};
