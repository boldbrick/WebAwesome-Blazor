// @ts-check
const { expect } = require('@playwright/test');
const { countOf, expectFired, payloadOf } = require('./event-log');

// The cases of event-payload.spec.js (review X6): events whose payload the library's JS initializer builds by
// hand (specialArgs in src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js), because the element's
// detail holds live DOM nodes, Dates, Errors or AbortSignals, or no detail at all. bUnit hands the wrapper a
// ready-made C# args object and never runs the projection, so a renamed or dropped field (a JS "videoTitle"
// renamed to "title") leaves the typed args at their defaults without any test noticing. Each case drives the
// element for real and asserts the non-default field values .NET received, as the harness's event log shows
// them. Cases marked `pro` need a Pro asset override and skip visibly on the free CDN.

const PAYLOADS = '/testing/event-payloads';

/** @typedef {import('@playwright/test').Page} Page */

/**
 * @typedef {object} PayloadCase
 * @property {string} name test title suffix
 * @property {string[]} tags custom elements to wait for (for `pro` cases: to require)
 * @property {boolean} [pro] needs Pro components
 * @property {string[]} callbacks "Wrapper.Callback" ids whose payload the case asserts
 * @property {(page: Page) => Promise<void>} run the interaction and the payload assertions
 */

/**
 * Waits for the callback to have run and returns its last payload.
 *
 * @param {Page} page
 * @param {string} id
 * @param {number} [atLeast]
 */
async function firedPayload(page, id, atLeast = 1) {
  await expectFired(page, id, atLeast);
  return payloadOf(page, id);
}

/** @type {PayloadCase[]} */
const PAYLOAD_CASES = [
  {
    name: 'wa-reposition (split panel) carries the position read from the element',
    tags: ['wa-split-panel'],
    callbacks: ['WaSplitPanel.OnReposition'],
    run: async page => {
      const panel = page.getByTestId('pl-split-panel');
      const initial = await panel.evaluate(el => /** @type {any} */ (el).positionInPixels);
      await panel.locator('[part~="divider"]').press('ArrowRight');
      await expect.poll(async () => (await payloadOf(page, 'WaSplitPanel.OnReposition')).position, { message: 'position after ArrowRight' }).toBe(41);
      const payload = await payloadOf(page, 'WaSplitPanel.OnReposition');
      const pixels = await panel.evaluate(el => Math.round(/** @type {any} */ (el).positionInPixels));
      expect(payload.positionInPixels, 'positionInPixels, rounded like the projection').toBe(pixels);
      expect(payload.positionInPixels).toBeGreaterThan(initial);
    },
  },
  {
    name: 'wa-mutation carries the mutation records without their DOM nodes',
    tags: ['wa-mutation-observer', 'wa-button'],
    callbacks: ['WaMutationObserver.OnMutation'],
    run: async page => {
      await page.getByTestId('pl-mutation-toggle').click();
      const payload = await firedPayload(page, 'WaMutationObserver.OnMutation');
      expect(payload.mutationRecords).toEqual([{ type: 'attributes', attributeName: 'data-state', oldValue: 'off' }]);
    },
  },
  {
    name: 'wa-resize carries the observed content rectangle',
    tags: ['wa-resize-observer', 'wa-button'],
    callbacks: ['WaResizeObserver.OnResize'],
    run: async page => {
      await page.getByTestId('pl-resize-grow').click();
      await expect.poll(async () => (await payloadOf(page, 'WaResizeObserver.OnResize')).resizeObserverEntries?.[0]?.contentRect?.width,
        { message: 'contentRect.width after the resize' }).toBe(200);
      const [entry] = (await payloadOf(page, 'WaResizeObserver.OnResize')).resizeObserverEntries;
      expect(entry.contentRect.height).toBe(20);
    },
  },
  {
    name: 'wa-content-change (random content) carries the count and identifying data of the shown items',
    tags: ['wa-random-content', 'wa-button'],
    callbacks: ['WaRandomContent.OnContentChange'],
    run: async page => {
      await page.getByTestId('pl-random-next').click();
      await expect.poll(async () => (await payloadOf(page, 'WaRandomContent.OnContentChange')).items?.[0]?.id,
        { message: 'id of the item shown after RandomizeAsync' }).toBe('pl-random-two');
      const payload = await payloadOf(page, 'WaRandomContent.OnContentChange');
      expect(payload).toEqual({ count: 1, items: [{ id: 'pl-random-two', textContent: 'Second item' }] });
    },
  },
  {
    // wa-intersect does not bubble: only the JS initializer's relay delivers it, with the flattened entry
    name: 'wa-intersect (intersection observer, relayed) carries the intersection state and ratio',
    tags: ['wa-intersection-observer'],
    callbacks: ['WaIntersectionObserver.OnIntersect'],
    run: async page => {
      const observed = page.getByTestId('pl-intersection-observer').locator('div').first();
      await observed.scrollIntoViewIfNeeded();
      await expect.poll(async () => (await payloadOf(page, 'WaIntersectionObserver.OnIntersect').catch(() => ({}))).isIntersecting,
        { message: 'isIntersecting after scrolling the observed element into view' }).toBe(true);
      const payload = await payloadOf(page, 'WaIntersectionObserver.OnIntersect');
      expect(payload.intersectionRatio).toBeGreaterThan(0);
      expect(payload.intersectionRatio).toBeLessThanOrEqual(1);
    },
  },
  {
    name: 'wa-focus-day and wa-view-change (date picker) carry ISO dates',
    tags: ['wa-date-picker'], pro: true,
    callbacks: ['WaDatePicker.OnFocusDay', 'WaDatePicker.OnViewChange'],
    run: async page => {
      const picker = page.getByTestId('pl-date-picker');
      const day = picker.getByRole('button', { name: 'Friday, March 15, 2024' });
      await day.focus();
      await day.press('ArrowRight');
      expect(await firedPayload(page, 'WaDatePicker.OnFocusDay')).toEqual({ date: '2024-03-16' });
      await picker.locator('[part~="title"]').click();
      const view = await firedPayload(page, 'WaDatePicker.OnViewChange');
      expect(view.view).toBe('months');
      expect(view.date).toMatch(/^2024-03-\d\d$/);
    },
  },
  {
    name: 'wa-video-change (video playlist) carries the indexes and the title of the new video',
    tags: ['wa-video-playlist', 'wa-video'], pro: true,
    callbacks: ['WaVideoPlaylist.OnVideoChange'],
    run: async page => {
      await page.getByTestId('pl-video-playlist').locator('[part~="playlist-item"]').nth(1).click();
      expect(await firedPayload(page, 'WaVideoPlaylist.OnVideoChange')).toEqual({ previousIndex: 0, currentIndex: 1, videoTitle: 'Second clip' });
    },
  },
  {
    name: 'data grid cell, sort, selection, page and filter events carry their details',
    tags: ['wa-data-grid'], pro: true,
    callbacks: ['WaDataGrid.OnCellClick', 'WaDataGrid.OnCellContextMenu', 'WaDataGrid.OnSortChange', 'WaDataGrid.OnRowSelect', 'WaDataGrid.OnPageChange', 'WaDataGrid.OnFilterChange'],
    run: async page => {
      const grid = page.getByTestId('pl-data-grid');
      const rows = grid.locator('[part~="body"] [part~="row"]');

      await rows.nth(0).locator('[part~="cell"]').nth(0).click();
      expect(await firedPayload(page, 'WaDataGrid.OnCellClick')).toEqual({ column: 'name', value: 'Ada', row: { id: 1, name: 'Ada', score: 90 }, rowIndex: 0 });

      // the projection drops originalEvent (a PointerEvent) and keeps the cell data
      await rows.nth(1).locator('[part~="cell"]').nth(1).click({ button: 'right' });
      expect(await firedPayload(page, 'WaDataGrid.OnCellContextMenu')).toEqual({ column: 'score', value: 85, row: { id: 2, name: 'Grace', score: 85 }, rowIndex: 1 });
      await page.keyboard.press('Escape');

      await rows.nth(0).locator('[part~="cell"]').nth(0).click();
      await page.keyboard.press(' ');
      const selection = await firedPayload(page, 'WaDataGrid.OnRowSelect');
      expect(selection.selectedRows).toEqual([{ id: 1, name: 'Ada', score: 90 }]);
      expect(selection.selectedKeys.map(String)).toEqual(['1']);

      await grid.locator('[part~="header-cell"]').nth(1).click();
      expect(await firedPayload(page, 'WaDataGrid.OnSortChange')).toEqual({ sort: [{ id: 'score', desc: false }] });

      await grid.locator('[part~="pager"]').locator('[part~="next-button"]').click();
      expect(await firedPayload(page, 'WaDataGrid.OnPageChange')).toEqual({ page: 1, pageSize: 2 });

      await grid.locator('[part~="search"] input').fill('Gr');
      await expect.poll(async () => (await payloadOf(page, 'WaDataGrid.OnFilterChange').catch(() => ({}))).search, { message: 'search term in wa-filter-change' }).toBe('Gr');
      expect((await payloadOf(page, 'WaDataGrid.OnFilterChange')).filters).toEqual([]);
    },
  },
  {
    name: 'data grid row expand/collapse and column move, pin, resize, visibility events carry their details',
    tags: ['wa-data-grid'], pro: true,
    callbacks: ['WaDataGrid.OnRowExpand', 'WaDataGrid.OnRowCollapse', 'WaDataGrid.OnColumnMove', 'WaDataGrid.OnColumnPin',
      'WaDataGrid.OnColumnResize', 'WaDataGrid.OnColumnVisibilityChange'],
    run: async page => {
      const grid = page.getByTestId('pl-column-grid');

      // the projection keeps the row's own fields; nested child rows lie beyond its depth limit
      await grid.locator('[part~="expand-button"]').first().click();
      expect((await firedPayload(page, 'WaDataGrid.OnRowExpand')).row).toMatchObject({ id: 10, name: 'Team', score: 80 });
      await grid.locator('[part~="expand-button"]').first().click();
      expect((await firedPayload(page, 'WaDataGrid.OnRowCollapse')).row).toMatchObject({ id: 10, name: 'Team', score: 80 });

      // Shift+ArrowRight on a focused header moves its column one step right
      await grid.locator('[part~="header-cell"]').first().click();
      await page.keyboard.press('Shift+ArrowRight');
      expect(await firedPayload(page, 'WaDataGrid.OnColumnMove')).toEqual({ column: 'name', toIndex: 1, columnOrder: ['score', 'name'], finished: true });

      // the per-column menu of the (now first) score column: pin it left, autosize it, hide it
      const menuAction = async (/** @type {string} */ action) => {
        await grid.locator('[part~="column-menu-button"]').first().click();
        await grid.locator(`[part~="column-menu"] wa-dropdown-item[data-action="${action}"]`).first().click();
      };
      await menuAction('pin-left');
      expect(await firedPayload(page, 'WaDataGrid.OnColumnPin')).toEqual({ column: 'score', side: 'left' });
      await menuAction('autosize');
      const resize = await firedPayload(page, 'WaDataGrid.OnColumnResize');
      expect(resize.column).toBe('score');
      expect(resize.width).toBeGreaterThan(0);
      expect(resize.finished).toBe(true);
      await menuAction('hide');
      expect(await firedPayload(page, 'WaDataGrid.OnColumnVisibilityChange')).toEqual({ column: 'score', visible: false });
    },
  },
  {
    name: 'wa-data-request (server grid) carries sort and paging, wa-data-error the error message and request',
    tags: ['wa-data-grid'], pro: true,
    callbacks: ['WaDataGrid.OnDataRequest', 'WaDataGrid.OnDataError'],
    run: async page => {
      const grid = page.getByTestId('pl-server-grid');
      const request = 'WaDataGrid.OnDataRequest';

      // the grid gets its columns from .NET after its first render; only then are its headers there to sort by
      await expect(grid.locator('[part~="header-cell"]').first()).toBeVisible();

      // each step waits for a request issued after it, never for whichever request came last: the grid's own first
      // request is sent on its first update, whenever the element's module arrives, and under a full worker load it
      // can reach .NET before or after the test starts looking. A fresh request, asked for once the handler is
      // attached, makes the starting point deterministic
      let seen = await countOf(page, request);
      await grid.evaluate(el => /** @type {any} */ (el).reload());
      await expectFired(page, request, ++seen);
      expect(await payloadOf(page, request)).toEqual({ sort: [], filters: [], search: '', page: 0, pageSize: 2 });

      await grid.locator('[part~="header-cell"]').nth(0).click();
      await expectFired(page, request, ++seen);
      await expect.poll(async () => (await payloadOf(page, request)).sort, { message: 'sort in wa-data-request' })
        .toEqual([{ id: 'name', desc: false }]);

      seen = await countOf(page, request);
      await grid.locator('[part~="pager"]').locator('[part~="next-button"]').click();
      await expectFired(page, request, ++seen);
      await expect.poll(async () => payloadOf(page, request), { message: 'the page 1 request' })
        .toEqual({ sort: [{ id: 'name', desc: false }], filters: [], search: '', page: 1, pageSize: 2 });

      // wa-data-error needs a failing JS dataSource, which the wrapper cannot supply (it is a function); the
      // test installs one and reloads, so the element dispatches the real event and the projection runs
      await grid.evaluate(el => {
        /** @type {any} */ (el).dataSource = () => Promise.reject(new Error('harness data source failure'));
        /** @type {any} */ (el).reload();
      });
      const error = await firedPayload(page, 'WaDataGrid.OnDataError');
      expect(error.error).toBe('harness data source failure');
      expect(error.request).toMatchObject({ sort: [{ id: 'name', desc: false }], page: 1, pageSize: 2 });
    },
  },
];

module.exports = { PAYLOAD_CASES, PAYLOADS };
