// @ts-check
const { expect, blazorErrorUiProblems } = require('./test');
const { waitForWaReady, WA_READY_TIMEOUT_MS } = require('./wa-ready');
const { collectProblems } = require('./page-health');

// Shared steps of the showcase flows (showcase-*.spec.js): open a showcase with its problems collected and, where a
// flow waits on the page's timers (toast durations, the dashboard refresh, autoplay), the page clock installed, and
// check that the page is still healthy.

/**
 * Opens a showcase and waits until the elements the flow drives have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string[]} tags the free Web Awesome elements the flow drives
 * @param {{ clock?: boolean }} [options] clock: install the page clock before the app starts, so the flow can
 * fast-forward the page's timers with page.clock.fastForward instead of waiting for them
 * @returns {Promise<string[]>} the page errors and console problems, filled while the test runs
 */
async function openShowcase(page, route, tags, options = {}) {
  const problems = collectProblems(page);
  if (options.clock) await page.clock.install();
  await page.goto(route);
  await page.waitForSelector('.demo-shell', { timeout: WA_READY_TIMEOUT_MS });
  await waitForWaReady(page, tags);
  return problems;
}

/**
 * Asserts that the page is healthy: no Blazor error UI, no page error, no console error or warning beyond noise.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} problems
 */
async function expectHealthy(page, problems) {
  expect(blazorErrorUiProblems(page), 'the Blazor error UI').toEqual([]);
  await expect(page.locator('#blazor-error-ui'), 'the Blazor error UI').toBeHidden();
  expect(problems, 'no page or console errors').toEqual([]);
}

/**
 * Whether a Web Awesome overlay (dialog, drawer, popover, dropdown, popup) is open, by its open or active property.
 *
 * @param {import('@playwright/test').Locator} overlay
 */
function isOpen(overlay) {
  return overlay.evaluate(el => Boolean(/** @type {any} */ (el).open ?? /** @type {any} */ (el).active));
}

/**
 * Picks an option of a wa-select by its text.
 *
 * @param {import('@playwright/test').Locator} select
 * @param {string} optionText
 */
async function pickOption(select, optionText) {
  await select.locator('[part~="combobox"]').click();
  await select.locator('wa-option', { hasText: optionText }).click();
  await expect.poll(() => isOpen(select), 'the listbox closed').toBe(false);
}

/**
 * Types into the text control of a wa-input or wa-textarea and tabs out, so the change reaches the bound model.
 *
 * @param {import('@playwright/test').Locator} field
 * @param {string} text
 */
async function typeInto(field, text) {
  const control = field.locator('input, textarea').first();
  await control.fill(text);
  await control.press('Tab');
}

/**
 * The text of an element with its whitespace collapsed, as a user reads it (for a regular expression match, which
 * toHaveText does not normalize).
 *
 * @param {import('@playwright/test').Locator} locator
 */
async function readableText(locator) {
  return ((await locator.textContent()) ?? '').replace(/\s+/g, ' ').trim();
}

/**
 * Stops the page clock (installed by openShowcase with clock: true), so a pending timer fires only when the flow
 * fast-forwards past it and a state the page shows until then (a reload's skeletons) can be asserted without racing
 * the timer.
 *
 * @param {import('@playwright/test').Page} page
 */
async function pauseClock(page) {
  const now = await page.evaluate(() => Date.now());
  await page.clock.pauseAt(now + CLOCK_PAUSE_STEP_MS);
}

// how far pauseClock moves the clock before it stops it: pauseAt needs a time still ahead of the running clock when
// the call arrives
const CLOCK_PAUSE_STEP_MS = 1_000;

module.exports = { openShowcase, expectHealthy, isOpen, pickOption, typeInto, readableText, pauseClock };
