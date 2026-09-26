// @ts-check
const base = require('@playwright/test');
const { NETWORK_NOISE } = require('./page-health');

// The common test base of every spec: Playwright's test with the page fixture extended by a Blazor crash guard. A
// spec imports { test, expect } from here instead of '@playwright/test', so no spec can pass while Blazor has
// failed: an unhandled .NET exception (a render, an event handler, a JS interop call) makes Blazor show its error UI
// (#blazor-error-ui, "An unhandled error has occurred") and stop rendering, and the page may look healthy otherwise.
//
// The guard watches the error UI from the first document on (an init script observes the element's style, which
// Blazor sets to display: block), so a crash is caught even when the test navigates or reloads afterwards, and
// checks it once more when the test ends. The failure names the console errors seen before, which carry the .NET
// exception.

// the element Blazor shows on an unhandled error, in both demo hosts (wwwroot\index.html, the server's App.razor)
const ERROR_UI_ID = 'blazor-error-ui';

// the binding the init script reports a shown error UI through
const ERROR_UI_BINDING = '__waE2eBlazorErrorUiShown';

// how many console errors before the crash the failure message quotes (network noise left out)
const QUOTED_CONSOLE_ERRORS = 5;

/**
 * Starts watching the Blazor error UI of a page. Call it before the page's first navigation.
 *
 * @param {import('@playwright/test').Page} page
 * @returns {Promise<{ problems: () => string[] }>} the error UI appearances seen so far, with the console errors
 * logged before each
 */
async function watchBlazorErrorUi(page) {
  const shown = [];
  const consoleErrors = [];
  page.on('console', msg => {
    if (msg.type() === 'error' && !NETWORK_NOISE.test(msg.text())) consoleErrors.push(msg.text());
  });

  await page.exposeBinding(ERROR_UI_BINDING, ({ frame }) => {
    const before = consoleErrors.slice(-QUOTED_CONSOLE_ERRORS).map(text => `    ${text.split('\n')[0]}`).join('\n');
    shown.push(`the Blazor error UI was shown on ${frame.url()}${before ? `, after these console errors:\n${before}` : ''}`);
  });

  await page.addInitScript(({ id, binding }) => {
    let reported = false;
    const check = () => {
      const errorUi = document.getElementById(id);
      if (reported || !errorUi || getComputedStyle(errorUi).display === 'none') return;
      reported = true;
      /** @type {any} */ (window)[binding]();
    };
    const watch = () => {
      const errorUi = document.getElementById(id);
      if (!errorUi) return;
      new MutationObserver(check).observe(errorUi, { attributes: true });
      check();
    };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', watch);
    else watch();
  }, { id: ERROR_UI_ID, binding: ERROR_UI_BINDING });

  return { problems: () => [...shown] };
}

/**
 * Whether the Blazor error UI is visible on the page right now (false once the page is closed).
 *
 * @param {import('@playwright/test').Page} page
 */
async function isBlazorErrorUiVisible(page) {
  if (page.isClosed()) return false;
  return page.locator(`#${ERROR_UI_ID}`).isVisible().catch(() => false);
}

// the error UI watch of each page the fixture created
const watches = new WeakMap();

/**
 * The error UI appearances seen so far on a page of the test fixture, for a spec that checks the page's health
 * between its own steps (the fixture checks it anyway when the test ends).
 *
 * @param {import('@playwright/test').Page} page
 * @returns {string[]}
 */
function blazorErrorUiProblems(page) {
  const watch = watches.get(page);
  return watch ? watch.problems() : [];
}

const test = base.test.extend({
  page: async ({ page }, use) => {
    const errorUi = await watchBlazorErrorUi(page);
    watches.set(page, errorUi);
    await use(page);

    const problems = errorUi.problems();
    if (problems.length === 0 && await isBlazorErrorUiVisible(page)) {
      problems.push(`the Blazor error UI is visible on ${page.url()}`);
    }
    base.expect(problems, `Blazor must not fail during the test:\n${problems.join('\n')}`).toEqual([]);
  },
});

module.exports = { test, expect: base.expect, blazorErrorUiProblems, isBlazorErrorUiVisible, ERROR_UI_ID };
