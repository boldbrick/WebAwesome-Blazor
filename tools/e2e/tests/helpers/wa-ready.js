// @ts-check

/**
 * Waits until the given Web Awesome custom elements are defined and the first instance of each
 * has finished its initial render. Both demo hosts deliver the Web Awesome loader through Blazor
 * HeadContent, so element upgrade races the first interactive render — a click landing on a
 * not-yet-upgraded element does nothing (a human just clicks again; a test must wait first).
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags custom element tag names the test is about to interact with
 */
async function waitForWaReady(page, tags) {
  await page.evaluate(async (tagNames) => {
    for (const tag of tagNames) {
      await customElements.whenDefined(tag);
      const el = document.querySelector(tag);
      if (el && 'updateComplete' in el) await /** @type {any} */ (el).updateComplete;
    }
  }, tags);
}

module.exports = { waitForWaReady };
