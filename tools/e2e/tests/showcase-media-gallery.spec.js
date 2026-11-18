// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, isOpen, readableText } = require('./helpers/showcase');

// The Media Gallery showcase (/showcases/media-gallery) as a user works it: the carousel's navigation, pagination
// and keyboard move between the launch renders, and the slide badge Blazor renders from OnSlideChange follows,
// looping at both ends. The comparison handle moves with the keyboard. The clip stills open in a lightbox at the
// clicked thumbnail; the status Blazor renders from OnShow and OnItemChange names the still on show, the approve
// button in the lightbox's toolbar updates the approval badge, and the button below reopens the lightbox at the
// latest still once the user has closed it (the page follows the element's own closing with OnHide).

const ROUTE = '/showcases/media-gallery';
const TAGS = ['wa-carousel', 'wa-carousel-item', 'wa-badge', 'wa-comparison', 'wa-lightbox', 'wa-lightbox-item', 'wa-button'];

// the captions of the clip stills, in the lightbox's order
// the approve button's text before and after the approval (whole, since one contains the other)
const APPROVE = /^\s*Approve\s*$/;
const APPROVED = /^\s*Approved\s*$/;

const STILLS = ['Chapter 1 — setting up a kit', 'Chapter 2 — kits in a project', 'Kit library overview', 'Team workspace'];

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

/**
 * The review status Blazor renders below the stills.
 *
 * @param {import('@playwright/test').Page} page
 */
function stillsStatus(page) {
  return page.locator('p.stills-status');
}

/**
 * The approval badge Blazor renders in the stills card's header.
 *
 * @param {import('@playwright/test').Page} page
 */
function approvalBadge(page) {
  return page.locator('wa-badge', { hasText: /approved/ });
}

test('media gallery showcase: the lightbox opens at a still and reports the still on show', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const lightbox = page.locator('wa-lightbox');

  await expect.poll(() => readableText(stillsStatus(page))).toBe('No still reviewed yet.');
  await expect.poll(() => readableText(approvalBadge(page))).toBe(`0 / ${STILLS.length} approved`);

  // a thumbnail opens the lightbox at its still
  await lightbox.locator('wa-lightbox-item').nth(1).click();
  await expect.poll(() => isOpen(lightbox), 'the thumbnail opened the lightbox').toBe(true);
  await expect.poll(() => readableText(stillsStatus(page)), 'the clicked still').toBe(`Last reviewed: ${STILLS[1]}`);

  // the next button moves on, and the status follows OnItemChange
  await lightbox.locator('[part~="navigation-button-next"]').click();
  await expect.poll(() => readableText(stillsStatus(page)), 'the next button').toBe(`Last reviewed: ${STILLS[2]}`);

  // the approve button in the lightbox's toolbar approves the still on show
  await lightbox.locator('wa-button', { hasText: APPROVE }).click();
  await expect.poll(() => readableText(approvalBadge(page)), 'one still approved').toBe(`1 / ${STILLS.length} approved`);
  await expect(lightbox.locator('wa-button', { hasText: APPROVED }), 'the button shows the approval').toHaveCount(1);

  // Escape closes the lightbox; the page follows with OnHide
  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(lightbox), 'Escape closed the lightbox').toBe(false);

  // the button reopens it at the latest still, which only works because the page followed the closing
  await page.locator('wa-button', { hasText: 'Open the latest still' }).click();
  await expect.poll(() => isOpen(lightbox), 'the button opened the lightbox').toBe(true);
  await expect.poll(() => readableText(stillsStatus(page)), 'the latest still').toBe(`Last reviewed: ${STILLS[3]}`);
  await expect(lightbox.locator('wa-button', { hasText: APPROVE }), 'the latest still is not approved').toHaveCount(1);

  // the lightbox loops past the last still
  await lightbox.locator('[part~="navigation-button-next"]').click();
  await expect.poll(() => readableText(stillsStatus(page)), 'next loops to the first still').toBe(`Last reviewed: ${STILLS[0]}`);

  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(lightbox), 'Escape closed the lightbox again').toBe(false);

  await expectHealthy(page, problems);
});
