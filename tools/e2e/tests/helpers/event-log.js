// @ts-check
const { expect } = require('@playwright/test');

// Reads the EventLog of the e2e harness pages (src\WebAwesome.Blazor.Demo\Pages\Testing\EventLogView.razor):
// one row per callback that ran in .NET, addressed by "Wrapper.Callback", with its call count and the JSON of
// the payload it last received. Everything here polls, because the callback runs after the browser event,
// across the Blazor dispatch.

/**
 * Returns the call count of a callback as rendered by the harness (0 while it has not run).
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} id "Wrapper.Callback"
 */
async function countOf(page, id) {
  const cell = page.getByTestId(`count-${id}`);
  if (await cell.count() === 0) return 0;
  return Number(await cell.textContent());
}

/**
 * Waits until a callback has run at least `atLeast` times in .NET.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} id "Wrapper.Callback"
 * @param {number} [atLeast]
 */
async function expectFired(page, id, atLeast = 1) {
  await expect.poll(() => countOf(page, id), { message: `${id} reached .NET at least ${atLeast} time(s)` })
    .toBeGreaterThanOrEqual(atLeast);
}

/**
 * Waits until a callback has run exactly `times` times in .NET.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} id "Wrapper.Callback"
 * @param {number} times
 */
async function expectFiredTimes(page, id, times) {
  await expect.poll(() => countOf(page, id), { message: `${id} reached .NET exactly ${times} time(s)` }).toBe(times);
}

/**
 * Returns the last payload a callback received, parsed from the harness's JSON rendering.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} id "Wrapper.Callback"
 * @returns {Promise<any>}
 */
async function payloadOf(page, id) {
  const text = await page.getByTestId(`payload-${id}`).textContent();
  return JSON.parse(text ?? 'null');
}

module.exports = { countOf, expectFired, expectFiredTimes, payloadOf };
