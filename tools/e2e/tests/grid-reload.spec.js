// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// ReloadAsync of the server-mode wrappers in a real browser (harness /testing/grid-reload, GridReloadHarness.razor):
// WaDataGrid.ReloadAsync after a row create and after an external filter change, and WaDataGrid.ReloadAsync and
// WaCombobox.ReloadAsync on the first render, while the autoloader still imports the element modules. A method call
// before the element is defined must wait for the definition, not fail with "Method 'reload' does not exist".
// wa-data-grid and wa-combobox are Pro components, so every test skips on the free CDN.

const HARNESS = '/testing/grid-reload';
const NONE = 'none';

// the delay of the element modules in the first-render test, so that the first render certainly comes before the
// elements are defined
const MODULE_DELAY_MS = 1500;

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
 * The number in a counter of the harness.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} testId
 */
async function count(page, testId) {
  return Number(await page.getByTestId(testId).textContent());
}

/**
 * Opens the harness and waits until the given Pro elements are defined.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string[]} tags
 */
async function openHarness(page, route, tags) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, tags);
}

test('grid reload: ReloadAsync sends a new wa-data-request and the grid shows the new data', async ({ page }) => {
  await openHarness(page, HARNESS, ['wa-data-grid']);

  const grid = page.getByTestId('gr-grid');
  await expect.poll(() => columnTexts(grid, 'id'), { message: 'the first page arrived', timeout: WA_READY_TIMEOUT_MS }).toEqual(['1', '2', '3']);
  await expect.poll(() => count(page, 'gr-request-count')).toBe(1);

  // count the requests the element itself sends
  await grid.evaluate(el => {
    /** @type {any} */ (window).__grDataRequests = 0;
    el.addEventListener('wa-data-request', () => { /** @type {any} */ (window).__grDataRequests++; });
  });
  const elementRequests = () => page.evaluate(() => /** @type {any} */ (window).__grDataRequests);

  // a reload with unchanged parameters still sends a request (the grid skips an identical request without force)
  await page.getByTestId('gr-reload').click();
  await expect.poll(() => count(page, 'gr-reload-count')).toBe(1);
  await expect(page.getByTestId('gr-error')).toHaveText(NONE);
  await expect.poll(elementRequests).toBe(1);
  await expect.poll(() => count(page, 'gr-request-count')).toBe(2);

  // a row create: the reload brings the new first row
  await page.getByTestId('gr-create').click();
  await expect.poll(() => columnTexts(grid, 'id'), { message: 'the created order shows' }).toEqual(['6', '1', '2']);
  await expect.poll(elementRequests).toBe(2);

  // an external filter change: the reload brings the open orders only
  await page.getByTestId('gr-filter').click();
  await expect.poll(() => columnTexts(grid, 'state'), { message: 'the filtered page shows' }).toEqual(['open', 'open', 'open']);
  await expect.poll(() => columnTexts(grid, 'id')).toEqual(['6', '1', '3']);
  await expect.poll(elementRequests).toBe(3);
  await expect(page.getByTestId('gr-error')).toHaveText(NONE);
  expect(await count(page, 'gr-reload-count')).toBe(3);
});

test('grid reload: ReloadAsync on the first render waits until the grid and the combobox are defined', async ({ page }) => {
  // the modules arrive late, so the first-render calls certainly come before the definitions
  for (const module of ['data-grid', 'combobox']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }

  await openHarness(page, `${HARNESS}?early=true`, ['wa-data-grid', 'wa-combobox']);

  // the grid: the call completed without an error, and the grid shows its data
  await expect.poll(() => count(page, 'gr-reload-count'), { message: 'the grid reload completed', timeout: WA_READY_TIMEOUT_MS }).toBe(1);
  await expect(page.getByTestId('gr-error')).toHaveText(NONE);
  await expect.poll(() => columnTexts(page.getByTestId('gr-grid'), 'id'), { message: 'the first page arrived' }).toEqual(['1', '2', '3']);
  expect(await count(page, 'gr-request-count')).toBeGreaterThanOrEqual(1);

  // the combobox: it requests options only on open and on reload, so a request proves that the reload ran
  await expect.poll(() => count(page, 'cb-reload-count'), { message: 'the combobox reload completed', timeout: WA_READY_TIMEOUT_MS }).toBe(1);
  await expect(page.getByTestId('cb-error')).toHaveText(NONE);
  await expect.poll(() => count(page, 'cb-request-count')).toBe(1);
});

test('grid reload: WaCombobox.ReloadAsync sends a new wa-options-request', async ({ page }) => {
  await openHarness(page, HARNESS, ['wa-combobox']);

  await page.getByTestId('cb-reload').click();
  await expect.poll(() => count(page, 'cb-reload-count')).toBe(1);
  await expect(page.getByTestId('cb-error')).toHaveText(NONE);
  await expect.poll(() => count(page, 'cb-request-count')).toBe(1);
});
