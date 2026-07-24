// @ts-check
const { test, expect } = require('@playwright/test');

// the Web Awesome asset tags are emitted by the WebAwesomeAssets component from configuration
// (WebAwesomeOptions) instead of being hard-coded in index.html; with no override the free CDN
// at the library's own version applies
test('WA asset tags are emitted from configuration into head', async ({ page }) => {
  await page.goto('/components/button');
  await page.waitForSelector('.demo-shell');

  await expect(page.locator('head link[href*="webawesome.css"]')).toHaveCount(1);
  await expect(page.locator('head script[src*="webawesome.loader.js"]')).toHaveCount(1);
});

// Pro / experimental markers mirror the official docs: driven by the api-surface document
// (pro flag derived from the [Pro] markers in the release zip's bundled reference docs since
// WA 3.3.0, tools\upgrade\pro-components.json as the pre-3.3.0 fallback; status from the CEM)
test('Pro badge and experimental flask render in nav and page header', async ({ page }) => {
  await page.goto('/components/combobox');
  await page.waitForSelector('.demo-shell');

  // page header: combobox is Pro AND experimental
  const header = page.locator('h1 .component-badges');
  await expect(header.locator('wa-badge.pro-badge')).toHaveText('Pro');
  await expect(header.locator('wa-icon.experimental-flask')).toHaveCount(1);

  // nav: Line Chart is Pro + experimental (Pro since 3.3.0), Page carries no markers
  // (free/stable upstream as of 3.5.0), Carousel is experimental only, Button has no markers
  const lineChartItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/line-chart"]') });
  await expect(lineChartItem.locator('wa-badge.pro-badge')).toHaveCount(1);
  await expect(lineChartItem.locator('wa-icon.experimental-flask')).toHaveCount(1);

  const pageItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/page"]') });
  await expect(pageItem.locator('.component-badges')).toHaveCount(0);

  const carouselItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/carousel"]') });
  await expect(carouselItem.locator('wa-badge.pro-badge')).toHaveCount(0);
  await expect(carouselItem.locator('wa-icon.experimental-flask')).toHaveCount(1);

  const buttonItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/button"]') });
  await expect(buttonItem.locator('.component-badges')).toHaveCount(0);
});

// every component page's API Reference links the canonical upstream documentation
test('canonical documentation link renders in the API reference', async ({ page }) => {
  await page.goto('/components/button');
  await page.waitForSelector('.api-reference');

  const link = page.locator('.api-reference .api-docs-link a');
  await expect(link).toHaveAttribute('href', 'https://webawesome.com/docs/components/button');
});
