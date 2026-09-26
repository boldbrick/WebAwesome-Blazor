// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades } = require('./helpers/wa-ready');
const { openShowcase, expectHealthy, readableText, pauseClock } = require('./helpers/showcase');

// The Dashboard showcase (/showcases/dashboard) as a user works it: Refresh swaps the feed and the issue list for
// skeletons while it reloads and brings them back, and the pagination control drives the page caption and (with the
// Pro data grid) the rows the grid receives. Every outcome asserted is one Blazor renders from the model.

const ROUTE = '/showcases/dashboard';
const TAGS = ['wa-button', 'wa-pagination', 'wa-badge', 'wa-card'];

// past the dashboard's 1.2 s simulated reload
const REFRESH_JUMP_MS = 1_500;

// the dashboard's 21 issues on pages of 6
const PAGE_COUNT = 4;

// the first issue of each page of the grid
const FIRST_ISSUES = ['NOVA-812', 'NOVA-749', 'NOVA-690', 'NOVA-619'];

// the issues of the last page
const LAST_PAGE_ISSUES = ['NOVA-619', 'NOVA-624', 'NOVA-631'];

/**
 * The page caption Blazor renders under the issue grid.
 *
 * @param {import('@playwright/test').Page} page
 */
function pageCaption(page) {
  return page.locator('main .wa-caption-m', { hasText: /^\s*Page \d+ of \d+\s*$/ });
}

test('dashboard showcase: Refresh shows skeletons while it reloads and brings the data back', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS, { clock: true });
  const refresh = page.locator('wa-button', { hasText: 'Refresh' });
  const main = page.locator('main');
  const feed = main.getByText(/build 3\.2\.0-rc\.1 to staging/);

  const feedHeaderSpinner = main.locator('wa-card').filter({ hasText: 'Live deployment feed' }).locator('wa-spinner');

  await expect(feed).toBeVisible();
  await expect(main.locator('wa-skeleton')).toHaveCount(0);

  // the reload waits on a timer; with the clock stopped the loading state holds until the flow moves on
  await pauseClock(page);
  await refresh.click();
  await expect(refresh, 'the button shows it is loading').toHaveAttribute('loading', '');
  await expect(main.locator('wa-skeleton'), 'the feed and the issue list are skeletons').toHaveCount(7);
  await expect(feedHeaderSpinner, 'the feed header spins').toHaveCount(1);
  await expect(feed).toHaveCount(0);
  await expect(main.locator('wa-data-grid')).toHaveCount(0);

  await page.clock.fastForward(REFRESH_JUMP_MS);
  await expect(main.locator('wa-skeleton'), 'the skeletons are gone').toHaveCount(0);
  await expect(refresh).not.toHaveAttribute('loading', '');
  await expect(feedHeaderSpinner).toHaveCount(0);
  await expect(feed).toBeVisible();
  await expect(main.locator('wa-data-grid')).toHaveCount(1);

  await expectHealthy(page, problems);
});

test('dashboard showcase: the pagination drives the page caption', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const pagination = page.locator('wa-pagination');

  await expect.poll(() => readableText(pageCaption(page))).toBe(`Page 1 of ${PAGE_COUNT}`);
  await pagination.locator('[part~="next-button"]').click();
  await expect.poll(() => readableText(pageCaption(page)), 'next page').toBe(`Page 2 of ${PAGE_COUNT}`);
  await expect(pagination).toHaveAttribute('page', '2');
  for (let target = 3; target <= PAGE_COUNT; target++) {
    await pagination.locator('[part~="next-button"]').click();
    await expect.poll(() => readableText(pageCaption(page)), `page ${target}`).toBe(`Page ${target} of ${PAGE_COUNT}`);
  }
  await pagination.locator('[part~="previous-button"]').click();
  await expect.poll(() => readableText(pageCaption(page)), 'previous page').toBe(`Page ${PAGE_COUNT - 1} of ${PAGE_COUNT}`);
  await expect(pagination).toHaveAttribute('page', String(PAGE_COUNT - 1));

  await expectHealthy(page, problems);
});

test('dashboard showcase: the pagination drives the rows of the data grid', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  await skipUnlessProUpgrades(page, ['wa-data-grid']);
  const grid = page.locator('wa-data-grid');
  const pagination = page.locator('wa-pagination');

  await expect(grid.getByText(FIRST_ISSUES[0], { exact: true }), 'the first page').toBeVisible();
  for (let index = 1; index < PAGE_COUNT; index++) {
    await pagination.locator('[part~="next-button"]').click();
    await expect(grid.getByText(FIRST_ISSUES[index], { exact: true }), `page ${index + 1} shows its first issue`).toBeVisible();
    await expect(grid.getByText(FIRST_ISSUES[index - 1], { exact: true }), `page ${index} is gone`).toHaveCount(0);
  }

  // the last page holds the remaining 3 issues
  for (const key of LAST_PAGE_ISSUES) await expect(grid.getByText(key, { exact: true }), `${key} on the last page`).toBeVisible();

  await expectHealthy(page, problems);
});
