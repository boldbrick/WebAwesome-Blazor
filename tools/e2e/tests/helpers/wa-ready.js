// @ts-check
const { test } = require('@playwright/test');

// the longest a page may take to define and render the Web Awesome elements a test needs. Under the default
// worker count (half the logical CPUs, 20 on the release machine) the WASM runtime download and the CDN modules
// of 20 parallel pages compete, so the first render can take well over the 5 s an expect() waits; a ceiling of
// its own makes a page that never gets ready fail with the tag it waited for instead of a bare test timeout
const WA_READY_TIMEOUT_MS = 45_000;

// the HTTP status of a missing module: the free CDN answers 404 for a Pro component (any other status, e.g. a kit
// refusing HEAD requests, decides nothing)
const HTTP_NOT_FOUND = 404;

// the Web Awesome script the page loads, whose URL the component modules are resolved against
const LOADER_SCRIPT_PATTERN = 'webawesome(\\.loader)?\\.js(\\?|$)';

/**
 * Waits until the given Web Awesome custom elements are defined and the first instance of each
 * has finished its initial render. Both demo hosts deliver the Web Awesome loader through Blazor
 * HeadContent, so element upgrade races the first interactive render — a click landing on a
 * not-yet-upgraded element does nothing (a human just clicks again; a test must wait first).
 * Fails after WA_READY_TIMEOUT_MS naming the elements that are not ready.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags custom element tag names the test is about to interact with
 */
async function waitForWaReady(page, tags) {
  const ready = page.evaluate(async (tagNames) => {
    for (const tag of tagNames) {
      await customElements.whenDefined(tag);
      const el = document.querySelector(tag);
      if (el && 'updateComplete' in el) await /** @type {any} */ (el).updateComplete;
    }
  }, tags);

  let timer;
  const ceiling = new Promise((_, reject) => {
    timer = setTimeout(async () => {
      const pending = await page
        .evaluate(tagNames => tagNames.filter(t => !customElements.get(t)), tags)
        .catch(() => tags);
      reject(new Error(`Web Awesome elements not ready after ${WA_READY_TIMEOUT_MS} ms: ${pending.join(', ') || 'first render of ' + tags.join(', ')}`));
    }, WA_READY_TIMEOUT_MS);
  });

  try {
    await Promise.race([ready, ceiling]);
  } finally {
    clearTimeout(timer);
  }
}

/**
 * Skips the running test, visibly and with a reason, when one of the given Pro components does not upgrade: the
 * free CDN has no Pro components, so they only run with a Pro asset override (tools\demo\Set-WaProAssets.ps1, or
 * the release gate's Pro pass, which runs by default when temp\wa-src\<version> holds the local Pro build). The
 * release gate lists every such skip in tools\e2e\data\expected-skips.json and fails on any other.
 *
 * It decides on the outcome instead of a fixed grace period: the element already upgraded runs the test; otherwise
 * it asks the server for the component's module next to the page's Web Awesome script (the URL the autoloader
 * imports), and a 404 (the free CDN's answer for a Pro component) skips at once. When the module exists, or cannot be
 * probed, it waits for the upgrade up to WA_READY_TIMEOUT_MS and skips only on that timeout, so a slow page under
 * load no longer skips a Pro test that would have run. (Resource timing entries cannot decide it: the WASM boot
 * fills the browser's resource timing buffer, so a late module request may leave no entry.)
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags Pro custom element tag names the test needs
 */
async function skipUnlessProUpgrades(page, tags) {
  for (const tag of tags) {
    const outcome = await page.evaluate(async ({ name, notFound, loaderPattern, timeoutMs }) => {
      if (customElements.get(name)) return 'upgraded';

      // the autoloader imports components/<name without wa->/<name without wa->.js next to the loader script
      const loader = [...document.querySelectorAll('script[src]')]
        .map(s => /** @type {HTMLScriptElement} */ (s).src)
        .find(src => new RegExp(loaderPattern).test(src));
      if (loader) {
        const file = name.replace(/^wa-/, '');
        try {
          const response = await fetch(new URL(`components/${file}/${file}.js`, loader).href, { method: 'HEAD', cache: 'no-store' });
          if (response.status === notFound) return 'unavailable';
        } catch {
          // not probeable (e.g. cross-origin without CORS): decide on the upgrade alone
        }
      }

      const upgraded = await Promise.race([
        customElements.whenDefined(name).then(() => true),
        new Promise(resolve => setTimeout(() => resolve(false), timeoutMs)),
      ]);
      return upgraded ? 'upgraded' : 'timeout';
    }, { name: tag, notFound: HTTP_NOT_FOUND, loaderPattern: LOADER_SCRIPT_PATTERN, timeoutMs: WA_READY_TIMEOUT_MS });

    test.skip(outcome === 'unavailable', `${tag} is a Pro component and its module is not on the free CDN - run with a Pro asset override (tools\\demo\\Set-WaProAssets.ps1)`);
    test.skip(outcome === 'timeout', `${tag} is a Pro component and did not upgrade within ${WA_READY_TIMEOUT_MS} ms - run with a Pro asset override (tools\\demo\\Set-WaProAssets.ps1)`);
  }
}

module.exports = { waitForWaReady, skipUnlessProUpgrades, WA_READY_TIMEOUT_MS };
