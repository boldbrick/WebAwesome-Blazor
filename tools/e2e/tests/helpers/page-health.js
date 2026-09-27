// @ts-check
const fs = require('fs');
const path = require('path');
const { WA_READY_TIMEOUT_MS } = require('./wa-ready');

// The health checks the route sweeps share (sweep.spec.js, interaction-sweep.spec.js): which console messages are a
// problem of the page, and the wait until every free Web Awesome element on a page has rendered.

// browser network noise, reported per request: the optional appsettings.Local.json the WASM host probes (404),
// Font Awesome Pro icons without a kit code (403, by the no-hardcoded-credentials rule), the harness's
// deliberately unreachable resources; failed resources a spec relies on are asserted by that spec
const NETWORK_NOISE = /^Failed to load resource: /;

// Web Awesome's warning for a component module that is not on the CDN; expected for Pro components on the free
// CDN only (with a Pro asset override the Pro specs fail when a Pro component does not upgrade)
const AUTOLOAD_WARNING = /Unable to autoload <(wa-[a-z-]+)>/;

// the Pro components, from the api-surface document that drives the demo navigation
const PRO_TAGS = (() => {
  const surfacePath = path.resolve(__dirname, '..', '..', '..', '..', 'src', 'WebAwesome.Blazor.Demo', 'wwwroot', 'data', 'api-surface.json');
  const surface = JSON.parse(fs.readFileSync(surfacePath, 'utf8').replace(/^﻿/, ''));
  return new Set(Object.entries(surface.components).filter(([, c]) => c.pro).map(([tag]) => tag));
})();

/**
 * Whether a console message is a problem of the page: every error and warning except known noise.
 *
 * @param {import('@playwright/test').ConsoleMessage} msg
 */
function isProblem(msg) {
  if (msg.type() !== 'error' && msg.type() !== 'warning') return false;

  const text = msg.text();
  if (NETWORK_NOISE.test(text)) return false;

  const autoload = AUTOLOAD_WARNING.exec(text);
  return !(autoload && PRO_TAGS.has(autoload[1]));
}

/**
 * Collects the problems of a page from now on: page errors and the console messages isProblem accepts.
 *
 * @param {import('@playwright/test').Page} page
 * @returns {string[]} the problems, filled while the test runs
 */
function collectProblems(page) {
  const problems = [];
  page.on('pageerror', err => problems.push(`[pageerror] ${err.message}`));
  page.on('console', msg => {
    if (isProblem(msg)) problems.push(`[console.${msg.type()}] ${msg.text()}`);
  });
  return problems;
}

/**
 * Waits until every free Web Awesome element on the page is defined and has rendered once (a Pro one on the free CDN
 * never is).
 *
 * @param {import('@playwright/test').Page} page
 * @returns {Promise<string[]>} the tags that were not defined after WA_READY_TIMEOUT_MS (empty when all are ready)
 */
async function waitForAllWaReady(page) {
  return page.evaluate(async ({ proTags, timeoutMs }) => {
    const tags = [...new Set([...document.querySelectorAll('*')].map(e => e.localName))]
      .filter(tag => tag.startsWith('wa-') && !proTags.includes(tag));
    const rendered = Promise.all(tags.map(async tag => {
      await customElements.whenDefined(tag);
      await Promise.all([...document.querySelectorAll(tag)].map(el => /** @type {any} */ (el).updateComplete));
    })).then(() => true);
    const done = await Promise.race([rendered, new Promise(resolve => setTimeout(() => resolve(false), timeoutMs))]);
    return done ? [] : tags.filter(tag => !customElements.get(tag));
  }, { proTags: [...PRO_TAGS], timeoutMs: WA_READY_TIMEOUT_MS });
}

module.exports = { NETWORK_NOISE, PRO_TAGS, isProblem, collectProblems, waitForAllWaReady };
