// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades } = require('./helpers/wa-ready');

// The column model of WaDataGrid as the element receives it, on the "Global Search & Column Filters" example of
// /components/data-grid: the region column's FilterOptions fill the set filter's picker (order and labels from C#,
// filtering by the values), the pushed columns hold no null member, and a parameter set that leaves the columns
// unchanged does not push them again. wa-data-grid is a Pro component, so the tests skip on the free CDN.

const ROUTE = '/components/data-grid';

// the grid of the example: the only "Countries" grid with the global search box
const GRID = 'wa-data-grid[label="Countries"][with-search]';

// the FilterOptions of the region column in DataGridPage.razor, as the picker shows them
const REGION_LABELS = ['Africa', 'North and South America', 'Asia', 'Europe', 'Australia and Oceania', 'Antarctica'];

// the FilterOptions of the region column as the element holds them: null members are left out
const REGION_OPTIONS = [
  { value: 'Africa' },
  { value: 'Americas', label: 'North and South America' },
  { value: 'Asia' },
  { value: 'Europe' },
  { value: 'Oceania', label: 'Australia and Oceania' },
  { value: 'Antarctica' },
];

/**
 * Opens the data grid page and returns the example's grid once its columns have arrived.
 *
 * @param {import('@playwright/test').Page} page
 */
async function openGrid(page) {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, ['wa-data-grid']);

  const grid = page.locator(GRID);
  await expect.poll(() => grid.evaluate(el => /** @type {any} */ (el).columns?.length ?? 0), { message: 'the columns arrived' }).toBe(4);
  return grid;
}

test('data grid: FilterOptions fill the set filter picker with their labels and filter by their values', async ({ page }) => {
  const grid = await openGrid(page);

  const columns = await grid.evaluate(el => /** @type {any} */ (el).columns);
  const nullMembers = columns.flatMap(column => Object.entries(column).filter(([, value]) => value === null).map(([key]) => `${column.field}.${key}`));
  expect(nullMembers, 'the pushed columns hold no null member').toEqual([]);
  expect(columns.find(column => column.field === 'region').filterOptions).toEqual(REGION_OPTIONS);

  await grid.locator('#filter-trigger-region').click();
  const labels = grid.locator('.filter-option-label');
  await expect(labels, 'the picker shows the options in order, with their labels').toHaveText(REGION_LABELS);

  await grid.locator('wa-checkbox', { has: page.locator('.filter-option-label', { hasText: 'North and South America' }) }).click();
  await expect(page.getByText('active column filters: 1', { exact: false }), 'Blazor received the filter').toBeVisible();
  await expect(grid.getByText('Brazil', { exact: true }), 'a row of the checked value stays').toBeVisible();
  await expect(grid.getByText('United States', { exact: true })).toBeVisible();
  await expect(grid.getByText('Japan', { exact: true }), 'a row of another value is filtered out').toHaveCount(0);
});

test('data grid: a parameter set that leaves the columns unchanged does not push them again', async ({ page }) => {
  const grid = await openGrid(page);

  // keep the column array the element holds now; a new push would replace it
  await grid.evaluate(el => { /** @type {any} */ (window).__waColumnsBefore = /** @type {any} */ (el).columns; });

  // the filter change re-renders the page (OnFilterChange), which sets the grid's parameters again
  await grid.locator('#filter-trigger-region').click();
  await grid.locator('wa-checkbox', { has: page.locator('.filter-option-label', { hasText: 'Europe' }) }).click();
  await expect(page.getByText('active column filters: 1', { exact: false }), 'the page rendered again').toBeVisible();

  const sameColumns = await grid.evaluate(el => /** @type {any} */ (el).columns === /** @type {any} */ (window).__waColumnsBefore);
  expect(sameColumns, 'the element still holds the columns of the first push').toBe(true);
  await expect(grid.getByText('Germany', { exact: true }), 'the filter still applies').toBeVisible();
  await expect(grid.getByText('Japan', { exact: true })).toHaveCount(0);
});
