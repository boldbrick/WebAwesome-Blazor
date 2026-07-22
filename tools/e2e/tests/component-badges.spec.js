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
// (pro flag from tools\upgrade\pro-components.json, status from the CEM)
test('Pro badge and experimental flask render in nav and page header', async ({ page }) => {
  await page.goto('/components/combobox');
  await page.waitForSelector('.demo-shell');

  // page header: combobox is Pro AND experimental
  const header = page.locator('h1 .component-badges');
  await expect(header.locator('wa-badge.pro-badge')).toHaveText('Pro');
  await expect(header.locator('wa-icon.experimental-flask')).toHaveCount(1);

  // nav: Page is Pro only, Carousel is experimental only, Button carries no markers
  const pageItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/page"]') });
  await expect(pageItem.locator('wa-badge.pro-badge')).toHaveCount(1);
  await expect(pageItem.locator('wa-icon.experimental-flask')).toHaveCount(0);

  const carouselItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/carousel"]') });
  await expect(carouselItem.locator('wa-badge.pro-badge')).toHaveCount(0);
  await expect(carouselItem.locator('wa-icon.experimental-flask')).toHaveCount(1);

  const buttonItem = page.locator('.demo-sidebar nav li', { has: page.locator('a[href="components/button"]') });
  await expect(buttonItem.locator('.component-badges')).toHaveCount(0);
});
