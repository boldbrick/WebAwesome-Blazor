// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// WaJsFunction references (docs\technical.md, "JavaScript function references"): the resolver of
// _content/WebAwesome.Blazor/webawesome-functions.js on its own (free CDN and Pro), the Uses preload of a tag that only a
// cell function renders (free CDN and Pro), and the data grid column members Formatter, AggregatedFormatter and
// ComparatorFunction in a real wa-data-grid (Pro; the tests skip on the free CDN). Pages: the "Custom Cell Rendering
// with JavaScript" example of /components/data-grid (a collocated .razor.js module) and the harness
// /testing/js-functions (a function registered by the demo's JS initializer, a global function, the collocated
// JsFunctionsHarness.razor.js, and with ?broken=true the references that do not resolve).

const HARNESS = '/testing/js-functions';
const DATA_GRID_PAGE = '/components/data-grid';

// how long a Registered reference waits for a late registration (lateRegistrationTimeoutMs in webawesome-functions.js)
const LATE_REGISTRATION_TIMEOUT_MS = 2000;

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
    .map(cell => (cell.textContent ?? '').trim()), columnId);
}

/**
 * Opens a page with a Pro data grid and waits until the grid holds its columns.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string} gridSelector
 * @param {number} columnCount
 */
async function openGrid(page, route, gridSelector, columnCount) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, ['wa-data-grid']);

  const grid = page.locator(gridSelector);
  await expect.poll(() => grid.evaluate(el => /** @type {any} */ (el).columns?.length ?? 0), { message: 'the columns arrived', timeout: WA_READY_TIMEOUT_MS }).toBe(columnCount);
  return grid;
}

test('function references: the resolver binds registered, global and module functions with a trailing ctx', async ({ page }) => {
  const errors = [];
  page.on('console', msg => { if (msg.type() === 'error') errors.push(msg.text()); });
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(async ({ lateDelayMs }) => {
    const functions = await import(new URL('_content/WebAwesome.Blazor/webawesome-functions.js', document.baseURI).href);
    const element = document.createElement('div');
    const binding = (/** @type {string} */ label, /** @type {number} */ arity) => ({
      element, member: 'formatter', label, arity, context: { column: 'c1', grid: element },
      rowKey: (/** @type {any[]} */ callArgs) => callArgs[1]?.id,
    });
    const fn = (/** @type {any} */ ref, /** @type {any} */ args) => ({ $wab: 'fn', ref, args });
    const out = /** @type {Record<string, any>} */ ({});

    // registered at start-up by the demo's JS initializer, in the registry every copy of the module shares
    out.shared = /** @type {any} */ (globalThis)[Symbol.for('WebAwesome.Blazor.functions')].functions.has('demo.describeCell');
    const registered = await functions.resolveFunction(fn({ kind: 'registered', name: 'demo.describeCell' }, { prefix: '>' }), binding('registered', 2));
    out.registered = registered('Ada', { id: 7 });

    // a global dotted path, also from the initializer
    const global = await functions.resolveFunction(fn({ kind: 'global', path: 'demoGrid.shout' }), binding('global', 2));
    out.global = global('team', {});

    // a module export, with ctx after the four comparator arguments
    const module = await functions.resolveFunction(fn({ kind: 'module', url: './Pages/Testing/JsFunctionsHarness.razor.js', export: 'byListOrder' }, { order: ['b', 'a'] }), binding('module', 4));
    out.module = [module('a', 'b', {}, {}), module('b', 'a', {}, {})];

    // ctx keeps its position after the declared arity, even when the element passes fewer arguments
    functions.registerFunction('test.arguments', (/** @type {any[]} */ ...callArgs) => `${callArgs.length}:${callArgs[2]?.column}`);
    const position = await functions.resolveFunction(fn({ kind: 'registered', name: 'test.arguments' }), binding('position', 2));
    out.position = position('only');

    // a registration that comes after the reference waits for it
    setTimeout(() => functions.registerFunction('test.late', () => 'late'), lateDelayMs);
    const late = await functions.resolveFunction(fn({ kind: 'registered', name: 'test.late' }), binding('late', 2));
    out.late = late?.('x', {});

    // a context decorator adds members to ctx; this[callArguments] is the call
    const removeDecorator = functions.registerContextDecorator((/** @type {any} */ proto, /** @type {any} */ info) => {
      proto.describeCall = function () { return `${info.member}:${this[functions.callArguments][1].id}`; };
    });
    functions.registerFunction('test.decorated', (/** @type {any} */ value, /** @type {any} */ row, /** @type {any} */ ctx) => ctx.describeCall());
    const decorated = await functions.resolveFunction(fn({ kind: 'registered', name: 'test.decorated' }), binding('decorated', 2));
    out.decorated = decorated('x', { id: 3 });
    removeDecorator();

    // another origin needs the explicit opt-in
    out.crossOrigin = await functions.resolveFunction(fn({ kind: 'module', url: 'https://example.com/cells.js', export: 'cell' }), binding('cross-origin test', 2));
    return out;
  }, { lateDelayMs: LATE_REGISTRATION_TIMEOUT_MS / 4 });

  expect(result.shared, 'the demo initializer registered in the shared registry').toBe(true);
  expect(result.registered).toBe('>Ada | key 7 | column c1 | div');
  expect(result.global).toBe('TEAM!');
  expect(result.module).toEqual([1, -1]);
  expect(result.position, 'ctx is the third argument of a two-argument member').toBe('3:c1');
  expect(result.late).toBe('late');
  expect(result.decorated).toBe('formatter:3');
  expect(result.crossOrigin, 'a module of another origin does not resolve').toBeUndefined();
  expect(errors.filter(text => text.includes('https://example.com/cells.js')), 'the refusal names the reference').toHaveLength(1);
});

test('function references: Uses registers an element that only a cell function renders', async ({ page }) => {
  await page.goto(DATA_GRID_PAGE);
  await page.waitForSelector('.demo-shell');

  // nothing on the page renders wa-progress-bar in the light DOM; only the progress column's formatter does, so the
  // element is registered by its Uses declaration (free CDN: the grid never upgrades, but the preload still runs)
  expect(await page.evaluate(() => document.querySelectorAll('wa-progress-bar').length)).toBe(0);
  await page.waitForFunction(() => !!customElements.get('wa-progress-bar'), null, { timeout: WA_READY_TIMEOUT_MS });
});

test('data grid: a collocated module formats cells, declares their elements and sorts a column', async ({ page }) => {
  const grid = await openGrid(page, DATA_GRID_PAGE, 'wa-data-grid[label="Sprint tasks"]', 3);

  await expect.poll(() => columnTexts(grid, 'status')).toEqual(['done', 'doing', 'blocked', 'todo', 'doing']);
  const badges = await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="status"] wa-badge') ?? [])]
    .map(badge => `${/** @type {any} */ (badge).variant}:${!!badge.shadowRoot}`));
  expect(badges, 'each status is an upgraded badge with the variant from ctx.args').toEqual(['success:true', 'brand:true', 'danger:true', 'neutral:true', 'brand:true']);

  const bars = await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="progress"] wa-progress-bar') ?? [])]
    .map(bar => `${/** @type {any} */ (bar).value}:${!!bar.shadowRoot}`));
  expect(bars, 'each progress value is an upgraded progress bar').toEqual(['100:true', '60:true', '35:true', '0:true', '80:true']);

  // the comparator sorts in workflow order, not alphabetically
  await grid.locator('[part~="header-cell"]').nth(1).click();
  await expect.poll(() => columnTexts(grid, 'status')).toEqual(['todo', 'doing', 'doing', 'blocked', 'done']);
});

test('data grid: registered and global formatters get ctx, a comparator sorts, discover() registers undeclared elements', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="jf-people"]', 4);

  await expect.poll(() => columnTexts(grid, 'name')).toEqual([
    'N:Ada | key 1 | column name | wa-data-grid',
    'N:Grace | key 2 | column name | wa-data-grid',
    'N:Alan | key 3 | column name | wa-data-grid',
    'N:Katherine | key 4 | column name | wa-data-grid',
  ]);
  expect(await columnTexts(grid, 'team')).toEqual(['ENGINEERING!', 'ENGINEERING!', 'RESEARCH!', 'RESEARCH!']);

  // the score column declares no Uses: discover() on the grid's shadow root registers wa-rating after the render
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('wa-rating') ?? [])]
    .map(rating => `${/** @type {any} */ (rating).value}:${!!rating.shadowRoot}`))).toEqual(['2:true', '5:true', '4:true', '3:true']);
  expect(await page.evaluate(() => document.querySelectorAll('wa-rating').length), 'no wa-rating in the light DOM').toBe(0);

  await grid.locator('[part~="header-cell"]').nth(2).click();
  await expect.poll(() => columnTexts(grid, 'priority'), { message: 'sorted by the list order of ctx.args' }).toEqual(['high', 'high', 'medium', 'low']);
});

test('data grid: only a change of the column JSON pushes the columns again', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="jf-people"]', 4);
  await expect.poll(() => columnTexts(grid, 'name')).toContain('N:Ada | key 1 | column name | wa-data-grid');
  await grid.evaluate(el => { /** @type {any} */ (window).__jfColumns = /** @type {any} */ (el).columns; });

  // a new list with the same definitions: the page renders, the grid keeps its columns
  await page.getByTestId('jf-same-columns').click();
  await expect(page.getByTestId('jf-column-lists')).toHaveText('2');
  expect(await grid.evaluate(el => /** @type {any} */ (el).columns === /** @type {any} */ (window).__jfColumns), 'no push for the same JSON').toBe(true);

  // other arguments: a new push with a new function
  await page.getByTestId('jf-change-prefix').click();
  await expect.poll(() => columnTexts(grid, 'name')).toContain('P:Ada | key 1 | column name | wa-data-grid');
  expect(await grid.evaluate(el => /** @type {any} */ (el).columns === /** @type {any} */ (window).__jfColumns)).toBe(false);
});

test('data grid: an aggregated formatter renders the group rows', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="jf-teams"]', 3);

  await expect.poll(() => columnTexts(grid, 'salary')).toEqual(['total 300 in 2 rows', 'total 700 in 2 rows']);
});

test('data grid: references that do not resolve leave raw values and a working grid', async ({ page }) => {
  const errors = [];
  page.on('console', msg => { if (msg.type() === 'error') errors.push(msg.text()); });
  const grid = await openGrid(page, `${HARNESS}?broken=true`, '[data-testid="jf-broken"]', 5);

  await expect.poll(() => columnTexts(grid, 'name'), { timeout: WA_READY_TIMEOUT_MS }).toEqual(['Ada', 'Grace', 'Alan', 'Katherine']);
  expect(await columnTexts(grid, 'missing-module')).toEqual(['Engineering', 'Engineering', 'Research', 'Research']);
  expect(await columnTexts(grid, 'missing-export')).toEqual(['low', 'high', 'medium', 'high']);
  expect(await columnTexts(grid, 'missing-global')).toEqual(['2', '5', '4', '3']);

  // each failure names its column member and its reference
  for (const reference of ["'demo.notRegistered'", 'missing.js', "'noSuchExport'", "'demoGrid.noSuchFunction'", 'https://example.com/cells.js']) {
    expect(errors.some(text => text.includes(reference)), `an error names ${reference}`).toBe(true);
  }

  // the column without its comparator sorts with the built-in sort
  await grid.locator('[part~="header-cell"]').nth(4).click();
  await grid.locator('[part~="header-cell"]').nth(4).click();
  await expect.poll(() => columnTexts(grid, 'salary')).toEqual(['400', '300', '200', '100']);
});
