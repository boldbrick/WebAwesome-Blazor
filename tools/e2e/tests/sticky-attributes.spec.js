// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Browser proof of the sticky attribute rule (3.12.0, docs\technical.md): when Blazor removes an attribute, Lit sets
// the element's property to null instead of back to its default, so a parameter returning to its default after it
// was rendered must render the element default explicitly. Each case sets a parameter away from its default and back
// on the harness /testing/sticky-attributes and reads the element's own property: with the attribute removed, the
// slider's max would be null (0 in its arithmetic), the input's type null, and the tooltip's distance null. The date of
// wa-relative-time and wa-format-date defaults to new Date(): with the attribute removed, new Date(null) is the 1970
// epoch, so the wrappers render the current instant instead.

const ROUTE = '/testing/sticky-attributes';

/**
 * Opens the harness and waits until Blazor is interactive and the elements have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 */
async function open(page) {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-slider', 'wa-input', 'wa-tooltip', 'wa-button', 'wa-relative-time', 'wa-format-date']);
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

// the tolerance between the instant the wrapper renders for "now" and the browser's clock when the test reads it
const NOW_TOLERANCE_MS = 5 * 60 * 1000;

// the relative time an instant long past reads as, in en-US
const YEARS_AGO = /years? ago/;

// the year the epoch that new Date(null) stands for displays
const EPOCH_YEAR = '1970';

/**
 * Reads the text and the datetime of the <time> an element renders in its shadow root.
 *
 * @param {import('@playwright/test').Locator} element
 * @returns {Promise<{ text: string, dateTime: string }>}
 */
async function shownTime(element) {
  return element.evaluate((el) => {
    const time = el.shadowRoot?.querySelector('time');
    return { text: time?.textContent?.trim() ?? '', dateTime: time?.dateTime ?? '' };
  });
}

test('sticky attributes: WaRelativeTime Date 2020 -> null shows the current time again, not the 1970 epoch', async ({ page }) => {
  await open(page);
  const relative = page.getByTestId('sa-relative-time');
  await expect(relative).not.toHaveAttribute('date');

  await page.getByTestId('sa-relative-time-past').click();
  await expect.poll(async () => (await shownTime(relative)).text).toMatch(YEARS_AGO);

  await page.getByTestId('sa-relative-time-unset').click();
  await expect.poll(async () => (await shownTime(relative)).text, 'the element reads now, not 1970').not.toMatch(YEARS_AGO);
  await expect(relative, 'the date is rendered, not the attribute removed').toHaveAttribute('date', /.+/);
  const shown = await shownTime(relative);
  expect(Math.abs(Date.parse(shown.dateTime) - Date.now()), `datetime ${shown.dateTime} is now`).toBeLessThan(NOW_TOLERANCE_MS);
});

test('sticky attributes: WaFormatDate Date 2020 -> null shows the current date again, not the 1970 epoch', async ({ page }) => {
  await open(page);
  const format = page.getByTestId('sa-format-date');
  await expect(format).not.toHaveAttribute('date');

  await page.getByTestId('sa-format-date-past').click();
  await expect.poll(async () => (await shownTime(format)).text).toContain('2020');

  await page.getByTestId('sa-format-date-unset').click();
  await expect.poll(async () => (await shownTime(format)).text, 'the element shows today, not 1970').toContain(String(new Date().getFullYear()));
  await expect(format, 'the date is rendered, not the attribute removed').toHaveAttribute('date', /.+/);
  const shown = await shownTime(format);
  expect(shown.text).not.toContain(EPOCH_YEAR);
  expect(Math.abs(Date.parse(shown.dateTime) - Date.now()), `datetime ${shown.dateTime} is now`).toBeLessThan(NOW_TOLERANCE_MS);
});