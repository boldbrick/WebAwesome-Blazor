// @ts-check
const { test } = require('@playwright/test');

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

/**
 * Skips the running test, visibly and with a reason, when one of the given Pro components does not upgrade: the
 * free CDN has no Pro components, so they only run with a Pro asset override (tools\demo\Set-WaProAssets.ps1, or
 * the release gate's Pro pass, which runs by default when temp\wa-src\<version> holds the local Pro build). The release gate lists every such skip in
 * tools\e2e\data\expected-skips.json and fails on any other.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags Pro custom element tag names the test needs
 */
async function skipUnlessProUpgrades(page, tags) {
  for (const tag of tags) {
    const upgraded = await page
      .waitForFunction(name => !!customElements.get(name), tag, { timeout: 5_000 })
      .then(() => true, () => false);
    test.skip(!upgraded, `${tag} is a Pro component and does not upgrade from the free CDN - run with a Pro asset override (tools\\demo\\Set-WaProAssets.ps1)`);
  }
}

module.exports = { waitForWaReady, skipUnlessProUpgrades };
