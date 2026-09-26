// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Declarative toast items whose model entry is removed in OnAfterHide (/testing/toast-items and the Overlays
// showcase). Web Awesome removes a hidden toast item from the DOM itself (wa-toast in its wa-after-hide handler,
// wa-toast-item after dispatching it), so Blazor later removed a node that had no parent any more ("Cannot read
// properties of null (reading 'removeChild')", the Blazor error UI). Blazor must stay the sole owner of the nodes it
// renders: the item stays hidden in place until the model drops it. Each test closes an item one way (close button,
// Duration, HideAsync), then checks that the page is healthy, the element is gone, the empty stack has closed and a
// toast added afterwards shows. An item the model keeps after hiding stays hidden in place, and its stack closes
// all the same.

const ROUTE = '/testing/toast-items';
const SHOWCASE_ROUTE = '/showcases/overlays';

// console noise of the demo that says nothing about the page (the sweep ignores it as well)
const NETWORK_NOISE = /^Failed to load resource: /;

// the longest a timed item (Duration 1000 on the harness) may take to show, count down and hide
const AUTO_DISMISS_TIMEOUT_MS = 15_000;

/**
 * Opens a page with a toast stack, collecting page errors and console errors, and waits for the toast elements.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @returns {Promise<string[]>} the problems seen on the page, filled while the test runs
 */
async function open(page, route) {
  const problems = [];
  page.on('pageerror', err => problems.push(`[pageerror] ${err.message}`));
  page.on('console', msg => {
    if (msg.type() === 'error' && !NETWORK_NOISE.test(msg.text())) problems.push(`[console.error] ${msg.text()}`);
  });

  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-toast', 'wa-toast-item', 'wa-button']);
  return problems;
}

/**
 * Whether the toast stack (a manual popover) is open.
 *
 * @param {import('@playwright/test').Locator} toast
 */
function isStackOpen(toast) {
  return toast.evaluate(el => el.matches(':popover-open'));
}

/**
 * Asserts that Blazor did not fail: no error UI, no page error, no console error.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} problems
 */
async function expectHealthy(page, problems) {
  await expect(page.locator('#blazor-error-ui'), `the Blazor error UI (${problems.join(' | ')})`).toBeHidden();
  expect(problems, 'no page or console errors').toEqual([]);
}

/**
 * Adds a toast on the harness and waits until it has shown in the open stack.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} button test id of the add button
 * @param {number} id the id the harness gives the new item
 */
async function addToast(page, button, id) {
  await page.getByTestId(button).click();
  const item = page.getByTestId(`ti-item-${id}`);
  await expect(item, `toast ${id} shows`).toBeVisible();
  await expect.poll(() => isStackOpen(page.getByTestId('ti-toast')), 'the stack is open').toBe(true);
  return item;
}

/**
 * The checks after the only item has hidden: the model dropped it, Blazor removed its element without an error, the
 * empty stack closed, and a toast added afterwards (the render after the removal) shows.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} problems
 * @param {number} nextId the id of the toast added afterwards
 */
async function expectRemovedAndReusable(page, problems, nextId) {
  const toast = page.getByTestId('ti-toast');
  await expect(page.getByTestId('ti-hidden-count'), 'OnAfterHide reached .NET').toHaveText(String(nextId - 1));
  await expect(page.getByTestId('ti-model-count'), 'the model dropped the item').toHaveText('0');
  await expect(toast.locator('wa-toast-item'), 'the element is gone').toHaveCount(0);
  await expect.poll(() => isStackOpen(toast), 'the empty stack closed').toBe(false);

  const next = await addToast(page, 'ti-add-persistent', nextId);
  await expect(toast.locator('wa-toast-item'), 'only the new item is in the stack').toHaveCount(1);
  await expect(next).toHaveText(`Toast ${nextId}`);
  await expectHealthy(page, problems);
}

test('toast items: closing a declarative item by its close button removes it cleanly', async ({ page }) => {
  const problems = await open(page, ROUTE);

  const item = await addToast(page, 'ti-add-persistent', 1);
  await item.locator('[part~="close-button"]').click();

  await expectRemovedAndReusable(page, problems, 2);
});

test('toast items: a declarative item dismissed by its Duration is removed cleanly', async ({ page }) => {
  const problems = await open(page, ROUTE);

  await addToast(page, 'ti-add-timed', 1);
  await expect(page.getByTestId('ti-model-count'), 'the timed item hid on its own')
    .toHaveText('0', { timeout: AUTO_DISMISS_TIMEOUT_MS });

  await expectRemovedAndReusable(page, problems, 2);
});

test('toast items: a declarative item hidden by HideAsync is removed cleanly', async ({ page }) => {
  const problems = await open(page, ROUTE);

  await addToast(page, 'ti-add-persistent', 1);
  await page.getByTestId('ti-hide-latest').click();

  await expectRemovedAndReusable(page, problems, 2);
});

test('toast items: a later item keeps showing while an earlier one is removed', async ({ page }) => {
  const problems = await open(page, ROUTE);
  const toast = page.getByTestId('ti-toast');

  const first = await addToast(page, 'ti-add-persistent', 1);
  const second = await addToast(page, 'ti-add-persistent', 2);
  await first.locator('[part~="close-button"]').click();

  await expect(page.getByTestId('ti-model-count'), 'the model dropped the first item').toHaveText('1');
  await expect(toast.locator('wa-toast-item'), 'only the second element is left').toHaveCount(1);
  await expect(second, 'the second item still shows').toBeVisible();
  expect(await isStackOpen(toast), 'the stack stays open for the second item').toBe(true);

  await second.locator('[part~="close-button"]').click();
  await expect(toast.locator('wa-toast-item')).toHaveCount(0);
  await expect.poll(() => isStackOpen(toast), 'the empty stack closed').toBe(false);
  await expectHealthy(page, problems);
});

test('toast items: an item the model keeps after hiding stays hidden in place and the stack closes', async ({ page }) => {
  const problems = await open(page, ROUTE);
  const toast = page.getByTestId('ti-toast');

  const item = await addToast(page, 'ti-add-kept', 1);
  await item.locator('[part~="close-button"]').click();

  // the model still holds the item (as when it drops it only later, e.g. after an await or over a server circuit)
  await expect(page.getByTestId('ti-hidden-count'), 'OnAfterHide reached .NET').toHaveText('1');
  await expect(page.getByTestId('ti-model-count'), 'the model keeps the item').toHaveText('1');
  await expect(toast.locator('wa-toast-item'), 'the element stays where Blazor rendered it').toHaveCount(1);
  await expect(item, 'the kept element is hidden').toBeHidden();
  await expect.poll(() => isStackOpen(toast), 'the stack of hidden items closed').toBe(false);

  await page.getByTestId('ti-remove-hidden').click();
  await expect(page.getByTestId('ti-model-count'), 'the model dropped the item').toHaveText('0');
  await expect(toast.locator('wa-toast-item'), 'Blazor removed the element').toHaveCount(0);

  const next = await addToast(page, 'ti-add-persistent', 2);
  await expect(next).toHaveText('Toast 2');
  await expectHealthy(page, problems);
});

test('toast items: the Overlays showcase deploy toast closes without a Blazor error', async ({ page }) => {
  const problems = await open(page, SHOWCASE_ROUTE);
  const deploy = page.locator('wa-button', { hasText: 'Deploy to staging' });
  const items = page.locator('wa-toast wa-toast-item');

  await deploy.click();
  await expect(items.first(), 'the deploy toast shows').toBeVisible();
  await items.first().locator('[part~="close-button"]').click();
  await expect(items, 'the deploy toast is gone').toHaveCount(0);

  await deploy.click();
  await expect(items, 'a second deploy toast shows').toHaveCount(1);
  await expect(items.first()).toBeVisible();
  await expectHealthy(page, problems);
});
