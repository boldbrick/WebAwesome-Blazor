// @ts-check
const path = require('path');
const { chromium } = require('@playwright/test');
const { test: base, expect } = require('../tests/helpers/test');
const { waitForWaReady } = require('../tests/helpers/wa-ready');

// The test base of the WA Inspector browser tests: the common test base (Blazor crash guard on the page fixture,
// tests\helpers\test.js) with a context that has the extension loaded. Chromium loads an unpacked extension only in a
// persistent context, and only the full Chromium build ('chromium' channel; the headless shell has no extensions)
// runs it headless. The panel opens as a normal tab, panel.html?tabId=<id of the demo page's tab>, and the tests
// drive it through its test hooks (window.__waInspectorPanel) and its DOM.

// the unpacked extension (shipped in the assets of the Claude Code plugin wablazor)
const EXTENSION_PATH = path.resolve(
  __dirname, '..', '..', 'wablazor-marketplace', 'plugins', 'wablazor', 'assets', 'wa-inspector-extension');

// the harness page of the inspector tests (src\WebAwesome.Blazor.Demo\Pages\Testing\InspectorHarness.razor)
const HARNESS_ROUTE = '/testing/inspector';

// the elements of the harness the tests act on
const HARNESS_TAGS = ['wa-button', 'wa-badge', 'wa-card'];

// the storage key prefix of a review session (content.js, panel.js)
const SESSION_KEY_PREFIX = 'wai:';

const test = base.extend({
  // a persistent context with the extension; baseURL and the fake audio output as in the standard config
  context: async ({ baseURL }, use) => {
    const context = await chromium.launchPersistentContext('', {
      channel: 'chromium',
      headless: true,
      baseURL,
      args: [
        `--disable-extensions-except=${EXTENSION_PATH}`,
        `--load-extension=${EXTENSION_PATH}`,
        '--disable-audio-output',
      ],
    });
    await use(context);
    await context.close();
  },

  // the service worker of the extension (background.js)
  serviceWorker: async ({ context }, use) => {
    let [worker] = context.serviceWorkers();
    if (!worker) worker = await context.waitForEvent('serviceworker');
    await use(worker);
  },

  extensionId: async ({ serviceWorker }, use) => {
    await use(new URL(serviceWorker.url()).host);
  },
});

/**
 * Opens the harness page and waits until Blazor rendered it and its elements upgraded.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} [route]
 */
async function openHarness(page, route = HARNESS_ROUTE) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await page.getByTestId('ih-card').waitFor();
  await waitForWaReady(page, HARNESS_TAGS);
}

/**
 * The tab id of a page, from the extension's view of the tabs.
 *
 * @param {import('@playwright/test').Worker} serviceWorker
 * @param {import('@playwright/test').Page} page
 * @returns {Promise<number>}
 */
async function tabIdOf(serviceWorker, page) {
  const url = page.url();
  const tabId = await serviceWorker.evaluate(async pageUrl => {
    const tabs = await chrome.tabs.query({});
    const tab = tabs.find(candidate => candidate.url === pageUrl);
    return tab ? tab.id : null;
  }, url);
  if (tabId === null || tabId === undefined) throw new Error(`no tab for ${url}`);
  return tabId;
}

/**
 * Opens the panel as a normal tab that inspects the given page, and waits until it shows the page's tree.
 *
 * @param {{ context: import('@playwright/test').BrowserContext, serviceWorker: import('@playwright/test').Worker, extensionId: string }} fixtures
 * @param {import('@playwright/test').Page} page the inspected page
 * @returns {Promise<import('@playwright/test').Page>}
 */
async function openPanel({ context, serviceWorker, extensionId }, page) {
  const tabId = await tabIdOf(serviceWorker, page);
  const panel = await context.newPage();
  await panel.goto(`chrome-extension://${extensionId}/panel.html?tabId=${tabId}`);
  await panel.waitForFunction(() => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    return !!(hooks && hooks.state.info && hooks.state.tree && hooks.state.session);
  });
  return panel;
}

/**
 * The flat list of the tree nodes the panel holds (depth first).
 *
 * @param {import('@playwright/test').Page} panel
 * @returns {Promise<any[]>}
 */
async function treeNodes(panel) {
  return panel.evaluate(async () => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    const tree = await hooks.request('page.tree');
    const nodes = [];
    const walk = node => {
      for (const child of node.children) {
        const { children, ...rest } = child;
        nodes.push(rest);
        walk(child);
      }
    };
    walk(tree.root);
    return nodes;
  });
}

/**
 * Selects the element of the page with the given test id in the panel, through the tree id the content script
 * gives it, and returns the details the panel shows.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} testId the data-testid of the element (the element itself, a wa- element or a utility-class element)
 * @returns {Promise<any>}
 */
async function selectByTestId(panel, testId, index = 0) {
  return panel.evaluate(async ({ id, position }) => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    const tree = await hooks.request('page.tree');
    const candidates = [];
    const walk = node => {
      for (const child of node.children) {
        candidates.push(child.id);
        walk(child);
      }
    };
    walk(tree.root);
    const matches = [];
    for (const candidate of candidates) {
      const details = await hooks.request('page.details', { id: candidate });
      if (details.attributes['data-testid'] === id) matches.push(candidate);
    }
    if (!matches[position]) throw new Error(`no tree node with data-testid ${id} at ${position}`);
    await hooks.selectElement(matches[position]);
    return hooks.state.details;
  }, { id: testId, position: index });
}

/**
 * The review session the panel holds.
 *
 * @param {import('@playwright/test').Page} panel
 */
async function sessionOf(panel) {
  return panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.state.session);
}

/**
 * Clears the stored review sessions of every origin.
 *
 * @param {import('@playwright/test').Worker} serviceWorker
 */
async function clearSessions(serviceWorker) {
  await serviceWorker.evaluate(async prefix => {
    const all = await chrome.storage.local.get(null);
    await chrome.storage.local.remove(Object.keys(all).filter(key => key.startsWith(prefix)));
  }, SESSION_KEY_PREFIX);
}

module.exports = {
  test,
  expect,
  HARNESS_ROUTE,
  openHarness,
  openPanel,
  tabIdOf,
  treeNodes,
  selectByTestId,
  sessionOf,
  clearSessions,
};
