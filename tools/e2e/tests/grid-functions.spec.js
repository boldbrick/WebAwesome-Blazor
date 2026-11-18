// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// The function-valued members of WaDataGrid phase 2 (docs\technical\technical.md, "JavaScript function references") in a real
// wa-data-grid: the column members ValueFunction, FooterFunction, CellClassFunction, FilterFunction and
// AggregationFunction, and the grid members RowClassFunction, RowDetailFunction, SearchFunction,
// SelectableRowsFunction and ChildRowsFunction. Pages: the harness /testing/grid-functions (the exports of the
// collocated GridFunctionsHarness.razor.js) and the "More JavaScript Members" example of /components/data-grid.
// wa-data-grid is a Pro component, so every test skips on the free CDN.

const HARNESS = '/testing/grid-functions';
const DATA_GRID_PAGE = '/components/data-grid';

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
 * The text of one column's footer cell.
 *
 * @param {import('@playwright/test').Locator} grid
 * @param {string} columnId
 * @returns {Promise<string | undefined>}
 */
function footerText(grid, columnId) {
  return grid.evaluate((el, id) => el.shadowRoot?.querySelector(`[part~="footer-cell"][data-col-id="${id}"]`)?.textContent?.trim(), columnId);
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

/**
 * Opens the order lines grid of the harness once its computed values show.
 *
 * @param {import('@playwright/test').Page} page
 */
async function openOrders(page) {
  const grid = await openGrid(page, HARNESS, '[data-testid="gf-orders"]', 5);
  await expect.poll(() => columnTexts(grid, 'total'), { message: 'the value function computes the totals' }).toEqual(['300', '450', '60', '250', '100', '120']);
  return grid;
}

test('grid functions: a value function drives sort, search and the CSV export', async ({ page }) => {
  const grid = await openOrders(page);

  // the sort compares the computed values
  await grid.locator('[part~="header-cell"][data-col-id="total"]').click();
  await expect.poll(() => columnTexts(grid, 'total')).toEqual(['60', '100', '120', '250', '300', '450']);

  // the CSV export holds the computed values, in the sorted order
  const csv = await grid.evaluate(el => /** @type {any} */ (el).getDataAsCsv());
  const [header, ...lines] = csv.trim().split(/\r?\n/).map((/** @type {string} */ line) => line.split(',').map(field => field.replace(/^"|"$/g, '')));
  const totalIndex = header.indexOf('Total');
  expect(totalIndex, 'the CSV has the total column').toBeGreaterThan(-1);
  expect(lines.map((/** @type {string[]} */ fields) => fields[totalIndex])).toEqual(['60', '100', '120', '250', '300', '450']);

  // the global search matches the computed value
  await grid.locator('[part="search"] input').fill('450');
  await expect.poll(() => columnTexts(grid, 'customer')).toEqual(['Ada Lovelace']);
});

test('grid functions: a footer function sums the filtered rows', async ({ page }) => {
  const grid = await openOrders(page);

  await expect.poll(() => footerText(grid, 'total')).toBe('Σ 1280');
  expect(await grid.evaluate(el => !!el.shadowRoot?.querySelector('[part~="footer-cell"][data-col-id="total"] wa-badge')?.shadowRoot),
    'the footer badge declared by Uses is upgraded').toBe(true);

  await grid.locator('[part="search"] input').fill('Hopper');
  await expect.poll(() => footerText(grid, 'total'), { message: 'the footer sums the rows that the search keeps' }).toBe('Σ 60');
});

test('grid functions: cellClass and rowClass functions set classes that ShadowStyles styles', async ({ page }) => {
  const grid = await openOrders(page);

  // quantities 2, 10, 3, 1, 25, 4: the cells at or below 3 get gf-low, red through ShadowStyles
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-col-id="quantity"]') ?? [])]
    .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex))
    .map(cell => `${cell.textContent?.trim()}:${cell.classList.contains('gf-low')}:${getComputedStyle(cell).color === 'rgb(200, 0, 0)'}`)))
    .toEqual(['2:true:true', '10:false:false', '3:true:true', '1:true:true', '25:false:false', '4:false:false']);

  // the rows whose total reaches 300 get gf-big, italic through ShadowStyles
  const rows = await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row"]') ?? [])]
    .filter(row => row.querySelector('[data-col-id="customer"]'))
    .map(row => `${row.querySelector('[data-col-id="customer"]')?.textContent?.trim()}:${row.classList.contains('gf-big')}:${getComputedStyle(row).fontStyle}`));
  expect(rows).toEqual([
    'José Álvarez:true:italic', 'Ada Lovelace:true:italic', 'Grace Hopper:false:normal',
    'Alan Turing:false:normal', 'Katherine Johnson:false:normal', 'Hedy Lamarr:false:normal',
  ]);
});

test('grid functions: a filter function decides the column filter', async ({ page }) => {
  const grid = await openOrders(page);

  // the text filter would keep only the quantity 25 for "5"; the function keeps the quantities of at least 5
  await grid.evaluate(el => /** @type {any} */ (el).setState({ version: 1, filters: [{ id: 'quantity', value: '5' }] }));
  await expect.poll(() => columnTexts(grid, 'customer')).toEqual(['Ada Lovelace', 'Katherine Johnson']);
  await expect.poll(() => footerText(grid, 'total')).toBe('Σ 550');
});

test('grid functions: selectableRows locks a row', async ({ page }) => {
  const grid = await openOrders(page);

  const locks = await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row"]') ?? [])]
    .filter(row => row.querySelector('[data-col-id="customer"]'))
    .map(row => `${row.querySelector('[data-col-id="customer"]')?.textContent?.trim()}:${/** @type {any} */ (row.querySelector('wa-checkbox'))?.disabled}`));
  expect(locks).toEqual([
    'José Álvarez:false', 'Ada Lovelace:false', 'Grace Hopper:false',
    'Alan Turing:true', 'Katherine Johnson:false', 'Hedy Lamarr:false',
  ]);

  // select-all skips the locked row; the header checkbox is found by the part it exports, whatever its inner part (WA
  // 3.15.0 exports "checkbox:select-all-checkbox", 3.14.0 exported "base:select-all-checkbox")
  await grid.locator('wa-checkbox[exportparts$=":select-all-checkbox"]').click();
  await expect.poll(() => grid.evaluate(el => [.../** @type {any} */ (el).selectedKeys].map(Number).sort((a, b) => a - b))).toEqual([1, 2, 3, 5, 6]);
});

test('grid functions: a search function replaces the global search', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="gf-search"]', 2);
  await expect.poll(() => columnTexts(grid, 'customer')).toHaveLength(6);

  // the default search would not find "José" for "jose"; the function ignores case and diacritics
  await grid.locator('[part="search"] input').fill('jose alv');
  await expect.poll(() => columnTexts(grid, 'customer')).toEqual(['José Álvarez']);
  await grid.locator('[part="search"] input').fill('LAMP');
  await expect.poll(() => columnTexts(grid, 'customer')).toEqual(['Grace Hopper']);
});

test('grid functions: rowDetail renders a detail panel on expand', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="gf-search"]', 2);
  await expect.poll(() => columnTexts(grid, 'customer')).toHaveLength(6);
  expect(await grid.evaluate(el => el.shadowRoot?.querySelectorAll('[part~="row-detail"]').length), 'no panel before the expand').toBe(0);

  await grid.locator('[part~="expand-button"]').nth(1).click();
  const detail = () => grid.evaluate(el => {
    const panel = el.shadowRoot?.querySelector('[part~="row-detail"] .gf-detail');
    const tag = panel?.querySelector('wa-tag');
    return panel ? `${panel.textContent?.replace(/\s+/g, ' ').trim()}|${!!tag?.shadowRoot}|${getComputedStyle(panel).color}` : null;
  });
  await expect.poll(detail, { message: 'the panel of the second row, with an upgraded tag (Uses) and ShadowStyles' })
    .toBe('EU 10 × Chair (key 2)|true|rgb(0, 100, 0)');
});

test('grid functions: a custom aggregation fills the group rows', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="gf-groups"]', 3);

  // the groups in the order of their first rows: EU (2, 10, 1), NA (3, 25), APAC (4)
  await expect.poll(() => columnTexts(grid, 'quantity')).toEqual(['1–10', '3–25', '4–4']);
});

test('grid functions: a childRows function builds a tree', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="gf-tree"]', 1);
  await expect.poll(() => columnTexts(grid, 'name')).toEqual(['src', 'docs']);

  // the first render: the wrapper assigns the data only after the childRows function has resolved, so the tree exists
  // without a second Data push
  await expect(grid.locator('[part~="expand-button"]'), 'each folder has an expand toggle').toHaveCount(2);
  await grid.evaluate(el => /** @type {any} */ (el).expandAllRows());
  await expect.poll(() => columnTexts(grid, 'name'), { message: 'the children come from ctx.args by ctx.rowKey' })
    .toEqual(['src', 'app.js', 'lib.js', 'docs', 'readme.md']);
});

test('data grid page: the "More JavaScript Members" example computes, sums, marks and details the lines', async ({ page }) => {
  const grid = await openGrid(page, DATA_GRID_PAGE, 'wa-data-grid[label="Billed work"]', 4);

  await expect.poll(() => columnTexts(grid, 'amount')).toEqual(['3600', '13200', '2240', '1440']);
  await expect.poll(() => footerText(grid, 'hours')).toBe('132');
  expect(await footerText(grid, 'amount')).toBe('20480');
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="hours"].over-budget') ?? [])].map(cell => cell.textContent?.trim())),
    'only the hours over the budget are marked').toEqual(['80']);

  await grid.locator('[part~="expand-button"]').first().click();
  await expect.poll(() => grid.evaluate(el => el.shadowRoot?.querySelector('[part~="row-detail"]')?.textContent?.trim()))
    .toBe('Design sprint: 24 h at 150 per hour');
});
