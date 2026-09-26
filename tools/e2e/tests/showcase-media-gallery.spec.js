// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, readableText } = require('./helpers/showcase');

// The Media Gallery showcase (/showcases/media-gallery) as a user works it: the carousel's navigation, pagination
// and keyboard move between the launch renders, and the slide badge Blazor renders from OnSlideChange follows,
// looping at both ends. The comparison handle moves with the keyboard.

const ROUTE = '/showcases/media-gallery';
const TAGS = ['wa-carousel', 'wa-carousel-item', 'wa-badge', 'wa-comparison'];

/**
 * The slide badge Blazor renders above the carousel.
 *
 * @param {import('@playwright/test').Page} page
 */
function slideBadge(page) {
  return page.locator('wa-badge', { hasText: /slide \d/ });
}

test('media gallery showcase: the carousel reports the slide it shows', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const carousel = page.locator('wa-carousel');

  await expect.poll(() => readableText(slideBadge(page))).toBe('slide 1 / 3');
  await carousel.locator('[part~="navigation-button-next"]').click();
  await expect.poll(() => readableText(slideBadge(page)), 'the next button').toBe('slide 2 / 3');
  await carousel.locator('[part~="pagination-item"]').nth(2).click();
  await expect.poll(() => readableText(slideBadge(page)), 'the third pagination dot').toBe('slide 3 / 3');
  await carousel.locator('[part~="navigation-button-next"]').click();
  await expect.poll(() => readableText(slideBadge(page)), 'next loops to the first slide').toBe('slide 1 / 3');
  await carousel.locator('[part~="navigation-button-previous"]').click();
  await expect.poll(() => readableText(slideBadge(page)), 'previous loops to the last slide').toBe('slide 3 / 3');

  // the keyboard on the focused scroll container
  await carousel.locator('[part~="scroll-container"]').focus();
  await page.keyboard.press('ArrowLeft');
  await expect.poll(() => readableText(slideBadge(page)), 'ArrowLeft').toBe('slide 2 / 3');

  await expectHealthy(page, problems);
});

test('media gallery showcase: the comparison handle moves with the keyboard', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const comparison = page.locator('wa-comparison');

  await expect.poll(() => comparison.evaluate(el => /** @type {any} */ (el).position), 'the initial Position').toBe(40);
  await comparison.locator('[part~="handle"]').focus();
  await page.keyboard.press('ArrowRight');
  await expect.poll(() => comparison.evaluate(el => /** @type {any} */ (el).position), 'the handle moved').toBeGreaterThan(40);

  await expectHealthy(page, problems);
});
