// @ts-check
const fs = require('fs');
const path = require('path');
const { test, expect } = require('./test');
const { waitForWaReady, skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./wa-ready');

// The wrapper method suite (WAB-65): shared helpers of the group specs (methods-<group>.spec.js), the case modules
// (method-cases\<group>.js) and method-coverage.spec.js. A harness page renders one MethodCall per call
// (src\WebAwesome.Blazor.Demo\Pages\Testing\MethodCall.razor): a button that runs the wrapper method from .NET through
// the wrapper's @ref, and an output "<id>-result" with the outcome - the returned value as JSON, "done", or
// "error: <message>" - and the number of completed calls in data-calls.

/** @typedef {import('@playwright/test').Page} Page */

/**
 * @typedef {object} MethodCase
 * @property {string} name test title suffix
 * @property {string[]} methods "Wrapper.Method" ids of tools\e2e\data\wrapper-methods.json the case proves (it calls
 * each one through the wrapper and asserts its effect)
 * @property {string[]} tags custom elements to wait for before the case acts
 * @property {string} [route] page to open; the group's route (method-groups.json) when not set
 * @property {boolean} [pro] the tags are Pro components: the case skips visibly unless they upgrade
 * @property {boolean} [clipboard] grant the clipboard permissions first
 * @property {boolean} [noReadyWait] do not wait for the tags (a timing case that acts before they are defined)
 * @property {(page: Page) => Promise<void>} [setup] runs before the page opens (for example a page.route delay)
 * @property {(page: Page) => Promise<void>} run the calls and their assertions
 */

/**
 * @typedef {object} KnownDefectCase
 * @property {string} name test title suffix
 * @property {string} defect the ticket and finding, for example "WAB-64 F8"; the test is an expected failure annotated
 * with it, so it turns red once the defect is fixed and the mark must go
 * @property {string[]} methods "Wrapper.Method" ids the defect affects
 * @property {string} [exemption] key of the "knownDefect" exemption in method-coverage-exemptions.json the case backs
 * @property {string[]} tags
 * @property {string} [route]
 * @property {boolean} [pro]
 * @property {boolean} [clipboard]
 * @property {boolean} [noReadyWait]
 * @property {(page: Page) => Promise<void>} [setup]
 * @property {(page: Page) => Promise<void>} run asserts the CORRECT behavior, which the defect breaks
 */

/**
 * @typedef {object} ExternalProof
 * @property {string} method "Wrapper.Method"
 * @property {string} spec spec file under tools\e2e\tests that calls the method through the wrapper and asserts it
 * @property {string} evidence literal text of that spec that names the proof
 */

/**
 * @typedef {object} MethodGroupModule
 * @property {string} group the group name of tools\e2e\data\method-groups.json (equal to the file name)
 * @property {MethodCase[]} cases
 * @property {KnownDefectCase[]} [knownDefects]
 * @property {ExternalProof[]} [external]
 */

/**
 * @typedef {object} CallOptions
 * @property {number} [timeout] how long the call may take (default: the expect timeout)
 * @property {boolean} [quiet] trigger the button with a click event only, so the focus and an open popup stay where
 * they are (a real click moves the focus to the button and closes a popup first, which hides the effect of a focus,
 * blur, show or hide call); a call that needs a user gesture (clipboard, fullscreen, picker) takes the real click
 */

const DATA_DIR = path.resolve(__dirname, '..', '..', 'data');
const CASES_DIR = path.join(__dirname, 'method-cases');

// set to run the known-defect cases as normal tests, so their failures show
const SHOW_DEFECTS_VARIABLE = 'E2E_SHOW_DEFECTS';

// the outcome texts of MethodCall.razor
const DONE_TEXT = 'done';
const ERROR_PREFIX = 'error: ';

// the button of a MethodCall, disabled until the page is interactive
const INTERACTIVE_CALL_SELECTOR = '.method-call > button';

/**
 * Reads a JSON file of tools\e2e\data.
 *
 * @param {string} name
 * @returns {any}
 */
function readMethodData(name) {
  return JSON.parse(fs.readFileSync(path.join(DATA_DIR, name), 'utf8').replace(/^﻿/, ''));
}

/**
 * The groups of tools\e2e\data\method-groups.json.
 *
 * @returns {{ name: string, title: string, route: string, components: string[] }[]}
 */
function methodGroups() {
  return readMethodData('method-groups.json').groups;
}

/**
 * Every case module under tests\helpers\method-cases, by file name (without .js).
 *
 * @returns {Map<string, MethodGroupModule>}
 */
function loadMethodCaseModules() {
  const modules = new Map();
  if (!fs.existsSync(CASES_DIR)) return modules;
  for (const file of fs.readdirSync(CASES_DIR).filter(name => name.endsWith('.js')).sort()) {
    modules.set(path.basename(file, '.js'), require(path.join(CASES_DIR, file)));
  }
  return modules;
}

/**
 * Clicks a MethodCall button and waits until its call has completed; returns the outcome.
 *
 * @param {Page} page
 * @param {string} id test id of the MethodCall
 * @param {CallOptions} [options]
 * @returns {Promise<{ done: boolean, value?: any, error?: string, text: string }>}
 */
async function callMethod(page, id, options = {}) {
  const output = page.getByTestId(`${id}-result`);
  const before = Number(await output.getAttribute('data-calls'));
  const button = page.getByTestId(id);
  if (options.quiet) {
    // a click event only: no pointer down, no focus change, so an open popup or the focus stays where it is
    await expect(button).toBeEnabled();
    await button.dispatchEvent('click');
  } else {
    await button.click();
  }
  await expect(output, `the call ${id} completes`).toHaveAttribute('data-calls', String(before + 1), { timeout: options.timeout });
  const text = (await output.textContent()) ?? '';
  if (text.startsWith(ERROR_PREFIX)) return { done: false, error: text.slice(ERROR_PREFIX.length), text };
  if (text === DONE_TEXT) return { done: true, text };
  return { done: true, value: JSON.parse(text), text };
}

/**
 * Clicks a MethodCall button, waits for the call, fails on an error and returns the value (undefined for a call
 * without a value).
 *
 * @param {Page} page
 * @param {string} id test id of the MethodCall
 * @param {CallOptions} [options]
 */
async function callOk(page, id, options = {}) {
  const outcome = await callMethod(page, id, options);
  expect(outcome.error, `the call ${id} succeeds`).toBeUndefined();
  return outcome.value;
}

/**
 * Opens a case's page and waits for its elements (or skips a Pro case visibly on the free CDN).
 *
 * @param {Page} page
 * @param {string} route
 * @param {{ tags: string[], pro?: boolean, noReadyWait?: boolean }} c
 */
async function openMethodHarness(page, route, c) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell', { timeout: WA_READY_TIMEOUT_MS });
  if (c.pro) await skipUnlessProUpgrades(page, c.tags);
  if (c.noReadyWait) return;
  // the first interactive render of a prerendered page replaces its elements, which drops a listener or a state a
  // case gives an element before; the MethodCall buttons are enabled from that render on
  await expect(page.locator(INTERACTIVE_CALL_SELECTOR).first(), 'the page is interactive').toBeEnabled({ timeout: WA_READY_TIMEOUT_MS });
  await waitForWaReady(page, c.tags);
}

/**
 * The route of a case: its own, or its group's (method-groups.json).
 *
 * @param {MethodGroupModule} module
 * @param {{ route?: string }} c
 */
function routeOf(module, c) {
  if (c.route) return c.route;
  const group = methodGroups().find(g => g.name === module.group);
  if (!group) throw new Error(`method case module '${module.group}' names no group of method-groups.json`);
  return group.route;
}

/**
 * The body of the test of one method case (the group spec registers the test, so the report names the spec file).
 *
 * @param {MethodGroupModule} module
 * @param {MethodCase} c
 * @param {Page} page
 * @param {import('@playwright/test').BrowserContext} context
 */
async function runMethodCase(module, c, page, context) {
  const pageErrors = [];
  page.on('pageerror', err => pageErrors.push(err.message));
  if (c.clipboard) await context.grantPermissions(['clipboard-read', 'clipboard-write']);
  if (c.setup) await c.setup(page);
  await openMethodHarness(page, routeOf(module, c), c);

  await c.run(page);

  expect(pageErrors, 'no page errors').toEqual([]);
}

/**
 * The body of the test of one known-defect case: an expected failure, annotated with the defect.
 *
 * @param {MethodGroupModule} module
 * @param {KnownDefectCase} c
 * @param {Page} page
 * @param {import('@playwright/test').BrowserContext} context
 */
async function runKnownDefectCase(module, c, page, context) {
  test.info().annotations.push({ type: 'known defect', description: `${c.defect}: ${c.methods.join(', ')}` });
  // E2E_SHOW_DEFECTS=1 runs the case as a normal test, to see where and why it fails
  test.fail(!process.env[SHOW_DEFECTS_VARIABLE], `known defect ${c.defect}; the correct behavior is asserted, so a pass means the defect is fixed and the mark must go`);
  if (c.clipboard) await context.grantPermissions(['clipboard-read', 'clipboard-write']);
  if (c.setup) await c.setup(page);
  await openMethodHarness(page, routeOf(module, c), c);

  await c.run(page);
}

module.exports = {
  callMethod, callOk, openMethodHarness, runMethodCase, runKnownDefectCase, loadMethodCaseModules, methodGroups, readMethodData,
};
