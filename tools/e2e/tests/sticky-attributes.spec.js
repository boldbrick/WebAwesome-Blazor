// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Browser proof of the sticky attribute rule (3.12.0, docs\technical.md): when Blazor removes an attribute, Lit sets
// the element's property to null instead of back to its default, so a parameter returning to its default after it
// was rendered must render the element default explicitly. Each case sets a parameter away from its default and back
// on the harness /testing/sticky-attributes and reads the element's own property: with the attribute removed, the
// slider's max would be null (0 in its arithmetic), the input's type null, and the tooltip's distance null.

const ROUTE = '/testing/sticky-attributes';

/**
 * Opens the harness and waits until Blazor is interactive and the elements have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 */
async function open(page) {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-slider', 'wa-input', 'wa-tooltip', 'wa-button']);
}

test('sticky attributes: WaSlider Max 50 -> 100 (the default) keeps the slider max at 100', async ({ page }) => {
  await open(page);
  const slider = page.getByTestId('sa-slider');
  await expect(slider, 'the unset default renders no attribute').not.toHaveAttribute('max');
  await expect(slider).toHaveJSProperty('max', 100);

  await page.getByTestId('sa-slider-max-50').click();
  await expect(slider).toHaveJSProperty('max', 50);

  await page.getByTestId('sa-slider-max-default').click();
  await expect(slider, 'the element max is the default again, not null').toHaveJSProperty('max', 100);
  await expect(slider, 'the default is rendered, not the attribute removed').toHaveAttribute('max', '100');
});

test('sticky attributes: WaInput Type Password -> Text keeps the type "text"', async ({ page }) => {
  await open(page);
  const input = page.getByTestId('sa-input');
  // wa-input reflects type, so the element itself sets the attribute; the property is what counts
  await expect(input).toHaveJSProperty('type', 'text');

  await page.getByTestId('sa-input-password').click();
  await expect(input).toHaveJSProperty('type', 'password');

  await page.getByTestId('sa-input-text').click();
  await expect(input).toHaveAttribute('type', 'text');
  await expect(input, 'the element type is text again, not null').toHaveJSProperty('type', 'text');
  await expect(input.locator('input'), 'the native input is a text field').toHaveAttribute('type', 'text');
});

test('sticky attributes: a nullable WaTooltip Distance 20 -> null goes back to the default 8', async ({ page }) => {
  await open(page);
  const tooltip = page.getByTestId('sa-tooltip');
  await expect(tooltip).not.toHaveAttribute('distance');
  await expect(tooltip).toHaveJSProperty('distance', 8);

  await page.getByTestId('sa-tooltip-distance-20').click();
  await expect(tooltip).toHaveJSProperty('distance', 20);

  await page.getByTestId('sa-tooltip-distance-unset').click();
  await expect(tooltip, 'the element distance is the default again, not null').toHaveJSProperty('distance', 8);
  await expect(tooltip).toHaveAttribute('distance', '8');
});
