// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// Phase 3 of WebAwesome.Blazor.Extensions (docs\technical\technical.md, "Cell presets and cell actions (Extensions)"): requests
// from grid content to .NET (ctx.invoke, WaDataGridRequestHandler, data-grid-requests.js) and static Razor cell
// templates (WaDataGridRazorTemplates, the razorTemplate preset). Pages: the harness /testing/cell-requests and the two
// phase 3 examples of /components/data-grid. The request tests without a grid run on the free CDN too (a real .NET
// round trip through the module); the grid tests need the Pro wa-data-grid and skip on the free CDN. Run the spec
// against both demo hosts (WebAssembly and Server).

const HARNESS = '/testing/cell-requests';
const DATA_GRID_PAGE = '/components/data-grid';
const REQUESTS_MODULE = '/_content/WebAwesome.Blazor.Extensions/data-grid-requests.js';
const CONTEXT_MODULE = '/_content/WebAwesome.Blazor.Extensions/data-grid-context.js';

// the wait of a request for a handler (handlerWaitTimeoutMs in data-grid-requests.js)
const HANDLER_WAIT_MS = 2000;

/**
 * Opens a page with a Pro data grid and returns the grid once its columns have arrived.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string} selector
 * @param {number} columnCount
 */
async function openGrid(page, route, selector, columnCount) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, ['wa-data-grid']);

  const grid = page.locator(selector);
  await expect.poll(() => grid.evaluate(el => /** @type {any} */ (el).columns?.length ?? 0), { message: 'the columns arrived', timeout: WA_READY_TIMEOUT_MS }).toBe(columnCount);
  return grid;
}

/**
 * The texts of one column's data cells, in display order.
 *
 * @param {import('@playwright/test').Locator} grid
 * @param {string} columnId
 * @returns {Promise<string[]>}
 */
function columnTexts(grid, columnId) {
  return grid.evaluate((el, id) => [...(el.shadowRoot?.querySelectorAll(`[part~="cell"][data-col-id="${id}"]`) ?? [])]
    .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex))
    .map(cell => (cell.textContent ?? '').replace(/\s+/g, ' ').trim()), columnId);
}

test('requests: the .NET handler answers with its value, rejects with its message, and keeps concurrent calls apart', async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(async url => {
    const { invokeCellRequest } = await import(url);
    const probe = /** @type {HTMLElement} */ (document.querySelector('[data-testid="cr-probe"]'));
    const settle = promise => promise.then(value => ({ value }), error => ({ error: String(error?.message ?? error) }));

    const echo = await settle(invokeCellRequest(probe, {
      column: 'c', action: 'echo', rowKey: 7, row: { id: 7, name: 'Ada' }, value: 1, payload: { a: 1 }, rowIndex: 3,
    }));
    const fail = await settle(invokeCellRequest(probe, { column: '', action: 'fail', payload: { reason: 'test' }, rowIndex: -1 }));
    const crash = await settle(invokeCellRequest(probe, { column: '', action: 'crash', rowIndex: -1 }));

    // a slow and a fast request at once: each gets its own answer, the fast one first
    const order = [];
    const [slow, fast] = await Promise.all([
      invokeCellRequest(probe, { column: '', action: 'delay', payload: { ms: 400, value: 'slow' }, rowIndex: -1 }).then(value => { order.push(value); return value; }),
      invokeCellRequest(probe, { column: '', action: 'delay', payload: { ms: 10, value: 'fast' }, rowIndex: -1 }).then(value => { order.push(value); return value; }),
    ]);
    return { echo, fail, crash, slow, fast, order };
  }, REQUESTS_MODULE);

  expect(result.echo).toEqual({ value: { action: 'echo', column: 'c', rowKey: '7', payload: '{"a":1}', rowIndex: 3, rowKeys: ['id', 'name'] } });
  expect(result.fail).toEqual({ error: 'Not allowed: test' });
  expect(result.crash.error).toContain("WebAwesome.Blazor.Extensions: the request 'crash' did not complete in .NET:");
  // the message of another exception is Blazor's: the exception text on WebAssembly, a general text on Blazor Server
  expect([result.slow, result.fast]).toEqual(['slow', 'fast']);
  expect(result.order).toEqual(['fast', 'slow']);
});

test('requests: no handler rejects after the wait, a late handler answers, and ctx.invoke sends the call', async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(async ({ requestsUrl, contextUrl }) => {
    const { invokeCellRequest, registerRequestHandler, unregisterRequestHandler } = await import(requestsUrl);
    const { decorateContext } = await import(contextUrl);
    const settle = promise => promise.then(value => ({ value }), error => ({ error: String(error?.message ?? error), name: error?.name }));

    // no WaDataGridRequestHandler holds the orphan element
    const orphan = /** @type {HTMLElement} */ (document.querySelector('[data-testid="cr-orphan"]'));
    const started = performance.now();
    const none = await settle(invokeCellRequest(orphan, { action: 'price' }));
    const waited = performance.now() - started;

    // a handler that registers while the request waits; a reference whose call fails, as after a Blazor Server
    // disconnect; an unregistration with another reference leaves the handler
    const late = document.createElement('div');
    document.body.append(late);
    const lateRequest = settle(invokeCellRequest(late, { action: 'late' }));
    const lateHandler = { invokeMethodAsync: (_name, request) => Promise.resolve({ value: `late ${request.action}`, error: null }) };
    setTimeout(() => registerRequestHandler(late, lateHandler), 300);
    const lateAnswer = await lateRequest;
    unregisterRequestHandler(late, {});
    const stillThere = await settle(invokeCellRequest(late, { action: 'again' }));
    registerRequestHandler(late, { invokeMethodAsync: () => Promise.reject(new Error('Cannot send data if the connection is not in the Connected State.')) });
    const disconnected = await settle(invokeCellRequest(late, { action: 'gone' }));
    late.remove();

    // ctx.invoke of a formatter in a grid element (not upgraded on the free CDN) inside the probe: the request
    // carries the call, and the row leaves out the HTML of the Razor templates
    const probe = /** @type {HTMLElement} */ (document.querySelector('[data-testid="cr-probe"]'));
    const grid = document.createElement('wa-data-grid');
    /** @type {any} */ (grid).rowKey = 'id';
    probe.append(grid);
    const proto = Object.create({ args: undefined, element: grid, grid, column: 'price', rowKey: 7 });
    decorateContext(proto, { element: grid, member: 'formatter', label: 'test', args: undefined, rowArgument: 1 });
    const row = { id: 7, name: 'Ada', $wabTemplateHtml: { state: '<b>x</b>' } };
    const ctx = Object.create(proto);
    ctx[Symbol.for('WebAwesome.Blazor.callArguments')] = [12, row];
    const echo = await settle(ctx.invoke('echo', { from: 'ctx' }));
    const noAction = await settle(ctx.invoke(''));
    grid.remove();

    return { none, waited, lateAnswer, stillThere, disconnected, echo, noAction };
  }, { requestsUrl: REQUESTS_MODULE, contextUrl: CONTEXT_MODULE });

  expect(result.none.error).toBe("WebAwesome.Blazor.Extensions: ctx.invoke('price') found no request handler. Put the WaDataGrid inside a WaDataGridRequestHandler.");
  expect(result.waited).toBeGreaterThanOrEqual(HANDLER_WAIT_MS - 50);
  expect(result.lateAnswer).toEqual({ value: 'late late' });
  expect(result.stillThere).toEqual({ value: 'late again' });
  expect(result.disconnected.error).toBe("WebAwesome.Blazor.Extensions: the request 'gone' did not complete in .NET: Cannot send data if the connection is not in the Connected State.");
  expect(result.echo).toEqual({ value: { action: 'echo', column: 'price', rowKey: '7', payload: '{"from":"ctx"}', rowIndex: -1, rowKeys: ['id', 'name'] } });
  expect(result.noAction.name).toBe('TypeError');
});

test('ctx.invoke in a cell: a pending state, then the answer of .NET, once per row key; a changed row asks again', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cr-grid"]', 3);

  await expect.poll(() => columnTexts(grid, 'price'), { timeout: WA_READY_TIMEOUT_MS }).toEqual(['14 EUR', 'Error: Sold out', '42 EUR']);
  await expect(page.getByTestId('cr-request-count')).toHaveText('3');

  // new data from .NET with a changed first row: a pending state while .NET answers, then the new answer; the other
  // rows keep their answers
  await page.getByTestId('cr-reload').click();
  await expect.poll(() => columnTexts(grid, 'price'), { intervals: [20] }).toEqual(['Loading…', 'Error: Sold out', '42 EUR']);
  await expect.poll(() => columnTexts(grid, 'price')).toEqual(['15 EUR', 'Error: Sold out', '42 EUR']);
  await expect(page.getByTestId('cr-request-count')).toHaveText('4');
});

test('ctx.invoke from a detail panel and a footer, and from a grid element that Blazor replaced', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cr-grid"]', 3);
  await expect.poll(() => columnTexts(grid, 'price'), { timeout: WA_READY_TIMEOUT_MS }).toEqual(['14 EUR', 'Error: Sold out', '42 EUR']);

  // the footer: no row, the column of the footer
  await grid.locator('.cr-footer-ask').click();
  await expect(grid.locator('.cr-footer-answer')).toHaveText('Total stock: 16');

  // the detail panel of the first row
  await grid.locator('[part~="expand-button"]').first().click();
  await grid.locator('.cr-detail-ask').click();
  await expect(grid.locator('.cr-detail-answer')).toHaveText('Stock: 4');

  // a new grid element (a new @key) inside the same handler finds the handler too
  await page.getByTestId('cr-replace').click();
  await expect(page.getByTestId('cr-generation')).toHaveText('1');
  const replaced = page.locator('[data-testid="cr-grid"]');
  await expect.poll(() => replaced.evaluate(el => /** @type {any} */ (el).columns?.length ?? 0)).toBe(3);
  await expect.poll(() => columnTexts(replaced, 'price')).toEqual(['14 EUR', 'Error: Sold out', '42 EUR']);
  await replaced.locator('.cr-footer-ask').click();
  await expect(replaced.locator('.cr-footer-answer')).toHaveText('Total stock: 16');
  await expect(page.getByTestId('cr-request-count')).toHaveText('6');
});

test('Razor templates: a WaBadge and an action button in cells; the action reaches .NET, not OnCellClick; new data updates the cell', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="crt-grid"]', 4);

  // the badges are upgraded wa-badge elements (declared with Uses), and the text of the row stays text
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="state"] wa-badge') ?? [])]
    .map(badge => `${badge.getAttribute('variant')}:${badge.textContent?.trim()}:${badge.matches(':defined')}`)), { timeout: WA_READY_TIMEOUT_MS })
    .toEqual(['brand:open:true', 'success:closed:true', 'brand:open:true']);
  await expect.poll(() => columnTexts(grid, 'title')).toEqual(['Printer <jammed>', 'VPN access', 'New laptop']);
  expect(await grid.evaluate(el => el.shadowRoot?.querySelectorAll('[data-col-id="title"] jammed').length)).toBe(0);
  expect(await grid.evaluate(el => /** @type {any} */ (el).data[0].$wabTemplateHtml.title)).toBe('<span class="crt-title" title="Printer &lt;jammed&gt;">Printer &lt;jammed&gt;</span>');

  // the declarative action of the template's button: OnCellAction with the payload, the row without the HTML, and no
  // OnCellClick (D4)
  await grid.locator('wa-button', { hasText: 'Open #1' }).click();
  await expect(page.getByTestId('crt-action')).toHaveText('open|open|1|{"title":"Printer <jammed>"}|Printer <jammed>|False');
  await expect(page.getByTestId('crt-click-count')).toHaveText('0');
  await grid.evaluate(el => /** @type {HTMLElement} */ (el.shadowRoot?.querySelector('[part~="cell"][data-col-id="id"]')).click());
  await expect(page.getByTestId('crt-click-count')).toHaveText('1');

  // new data: the template renders again for the new state
  await page.getByTestId('crt-advance').click();
  await expect.poll(() => grid.evaluate(el => el.shadowRoot?.querySelector('[data-col-id="state"] wa-badge')?.getAttribute('variant'))).toBe('success');
  await expect.poll(() => columnTexts(grid, 'state')).toEqual(['closed', 'closed', 'open']);
});

test('data grid page: the request and Razor template examples show answers and raise the template action', async ({ page }) => {
  const requests = await openGrid(page, DATA_GRID_PAGE, '[data-testid="request-grid"]', 4);
  await expect.poll(() => columnTexts(requests, 'stock'), { timeout: WA_READY_TIMEOUT_MS }).toEqual(['12 in stock', 'Sold out', '3 in stock']);

  const templates = page.locator('[data-testid="template-grid"]');
  await expect.poll(() => templates.evaluate(el => el.shadowRoot?.querySelectorAll('[data-col-id="state"] wa-badge:defined').length ?? 0), { timeout: WA_READY_TIMEOUT_MS }).toBe(3);
  await templates.locator('wa-button', { hasText: 'Restart' }).first().click();
  await expect(page.getByTestId('template-action')).toContainText('restart');
});
