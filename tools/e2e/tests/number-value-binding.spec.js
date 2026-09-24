// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Number-valued form controls (follow-up to value-sync-binding.spec.js, GitHub issue #1 / WA 3.12.0).
// wa-rating and wa-slider expose their live value as a JS number, and their change events carry
// that number. Blazor's ChangeEventArgs only carries strings, booleans and string arrays, so a
// binder that relies on the change event's own value (CreateBinder on "onchange") never reaches
// the model. These tests cover UI -> model for WaRating, WaRange and WaSlider range mode, and
// model -> UI after a user edit for WaRange (whose value attribute maps to defaultValue, the same
// sync bug as the item-2 wrappers) and WaRating (whose value attribute maps to the live property,
// so that half is expected to hold once the user edit reaches the model). The driving controls
// are the WaRange/WaRating/WaSlider (range mode) rows of the e2e-only harness /testing/value-sync.

const HARNESS_ROUTE = '/testing/value-sync';

/**
 * Opens the harness and waits until Blazor is interactive and the given elements have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags
 */
async function openHarness(page, tags) {
  await page.goto(HARNESS_ROUTE);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, tags);
}

/**
 * Resolves the harness row's control and model display, waiting for the control's first render.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} id
 */
async function row(page, id) {
  const element = page.getByTestId(id);
  const model = page.getByTestId(`${id}-model`);
  await element.evaluate(el => /** @type {any} */ (el).updateComplete);
  return { element, model };
}

/**
 * Clicks the centre of the fourth star, which commits a rating of 4 at the default precision of 1.
 * The individual symbols don't take pointer events (their .symbols container does, and derives the
 * value from the click's x coordinate), so the real mouse is aimed at the star's bounding box.
 *
 * @param {import('@playwright/test').Locator} rating
 */
async function clickFourthStar(rating) {
  // page.mouse does not scroll, so bring the rating into the viewport before measuring
  await rating.scrollIntoViewIfNeeded();
  const box = await rating.locator('.symbol').nth(3).boundingBox();
  if (!box) throw new Error('fourth rating symbol has no bounding box');
  await rating.page().mouse.click(box.x + box.width / 2, box.y + box.height / 2);
}

test('number binding: WaRating user edit (star click) reaches the bound model', async ({ page }) => {
  await openHarness(page, ['wa-rating']);
  const { element, model } = await row(page, 'rating');
  await expect(model).toHaveText('2');

  await clickFourthStar(element);

  await expect(element, 'the element took the click').toHaveJSProperty('value', 4);
  await expect(model, 'user edit reaches the bound model').toHaveText('4');
});

test('number binding: WaRange user edit (ArrowRight) reaches the bound model', async ({ page }) => {
  await openHarness(page, ['wa-slider']);
  const { element, model } = await row(page, 'range');
  await expect(model).toHaveText('30');

  await element.locator('[role="slider"]').press('ArrowRight');

  await expect(element, 'the element took the key press').toHaveJSProperty('value', 31);
  await expect(model, 'user edit reaches the bound model').toHaveText('31');
});

test('value sync: WaRange reflects C# model change after user edit', async ({ page }) => {
  await openHarness(page, ['wa-slider']);
  const { element, model } = await row(page, 'range');
  await expect(model).toHaveText('30');

  // (a) the user edits the control
  await element.locator('[role="slider"]').press('ArrowRight');
  await expect(model, 'user edit reaches the bound model').toHaveText('31');

  // (b) C# changes the model
  await page.getByTestId('range-set').click();
  await expect(model, 'C# change is rendered in the model display').toHaveText('70');

  // (c) the element's live property follows the model
  await expect(element, 'live "value" property follows the C# model').toHaveJSProperty('value', 70);
});

// wa-rating's value attribute maps to the live property (not to a default* field), so step (c)
// holds as soon as the user edit in step (a) reaches the model
test('value sync: WaRating reflects C# model change after user edit', async ({ page }) => {
  await openHarness(page, ['wa-rating']);
  const { element, model } = await row(page, 'rating');
  await expect(model).toHaveText('2');

  // (a) the user edits the control
  await clickFourthStar(element);
  await expect(model, 'user edit reaches the bound model').toHaveText('4');

  // (b) C# changes the model
  await page.getByTestId('rating-set').click();
  await expect(model, 'C# change is rendered in the model display').toHaveText('1');

  // (c) the element's live property follows the model
  await expect(element, 'live "value" property follows the C# model').toHaveJSProperty('value', 1);
});

test('number binding: WaSlider range mode user edit (min thumb) reaches the bound MinValue', async ({ page }) => {
  await openHarness(page, ['wa-slider']);
  const { element, model } = await row(page, 'slider-range');
  await expect(model).toHaveText('20-80');

  await element.locator('#thumb-min').press('ArrowRight');

  await expect(element, 'the element took the key press').toHaveJSProperty('minValue', 21);
  await expect(model, 'user edit reaches the bound MinValue/MaxValue').toHaveText('21-80');
});
