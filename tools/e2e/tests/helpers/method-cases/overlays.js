// @ts-check
const { expect } = require('../test');
const { callMethod, callOk } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "overlays" (tools\e2e\data\method-groups.json): WaDialog, WaDrawer, WaDropdown,
// WaDropdownItem, WaPopover, WaPopup, WaTooltip, WaToast, WaToastItem and WaPage on /testing/methods-overlays
// (MethodsOverlaysHarness.razor). Every call runs from .NET through the wrapper's @ref (a MethodCall button); the case
// asserts the effect on the element. Every show, hide and focus call is quiet: a real click moves the focus and closes
// an open popup, which hides or fakes the effect.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('@playwright/test').Locator} Locator */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const DIALOG = 'wa-dialog';
const DRAWER = 'wa-drawer';
const DROPDOWN = 'wa-dropdown';
const DROPDOWN_ITEM = 'wa-dropdown-item';
const POPOVER = 'wa-popover';
const POPUP = 'wa-popup';
const TOOLTIP = 'wa-tooltip';
const TOAST = 'wa-toast';
const PAGE = 'wa-page';

// the message of the harness's CreateAsync calls
const TOAST_MESSAGE = 'Toast created from .NET';

// calls that must not move the focus or close a popup (see callMethod)
const QUIET = { quiet: true };

// how long a call may take before a cancelled-show case calls it hanging
const SETTLE_TIMEOUT_MS = 5_000;

// the delay of the element modules in the first-render case, so the first render certainly comes before the definitions
// (long enough for the circuit of a prerendered page to start first)
const MODULE_DELAY_MS = 4_000;

/**
 * The open property of an element.
 *
 * @param {Locator} locator
 * @returns {Promise<boolean>}
 */
function isOpen(locator) {
  return locator.evaluate(el => Boolean(/** @type {any} */ (el).open));
}

/**
 * Whether the focus is on the element with the test id or inside it (its shadow root or its light DOM children).
 *
 * @param {Page} page
 * @param {string} testId
 */
function hasFocusWithin(page, testId) {
  return page.getByTestId(testId).evaluate(el => el.matches(':focus-within'));
}

/**
 * Takes the focus away from whatever has it, down to the focused element inside shadow roots, so the focus is on the
 * document body.
 *
 * @param {Page} page
 */
async function blurAll(page) {
  await page.evaluate(() => {
    let active = document.activeElement;
    while (active?.shadowRoot?.activeElement) active = active.shadowRoot.activeElement;
    if (active instanceof HTMLElement || active instanceof SVGElement) active.blur();
  });
}

/**
 * Makes the element cancel every wa-show it dispatches, as a consumer handler that calls preventDefault does.
 *
 * @param {Page} page
 * @param {string} testId
 */
async function cancelShow(page, testId) {
  await page.getByTestId(testId).evaluate(el => el.addEventListener('wa-show', event => {
    if (event.target === el) event.preventDefault();
  }));
}

/**
 * Waits until a first-render call of the harness has completed, and expects it to have succeeded.
 *
 * @param {Page} page
 * @param {string} testId
 */
async function expectEarlyOutcome(page, testId) {
  const output = page.getByTestId(testId);
  await expect(output, `${testId} was called`).not.toHaveText('not called', { timeout: WA_READY_TIMEOUT_MS });
  await expect(output, `${testId} completed without an error`).toHaveText('done');
}

/**
 * Shows a dialog or drawer through its wrapper and hides it again; both set the open property, so the call returns at
 * once and the case waits for the content.
 *
 * @param {Page} page
 * @param {string} prefix test id prefix of the element and its calls ("dialog" or "drawer")
 */
async function showAndHide(page, prefix) {
  const element = page.getByTestId(prefix);
  const content = page.getByTestId(`${prefix}-content`);
  await expect(content).toBeHidden();

  await callOk(page, `${prefix}-show`, QUIET);
  await expect.poll(() => isOpen(element)).toBe(true);
  await expect(content).toBeVisible();

  await callOk(page, `${prefix}-hide`, QUIET);
  await expect(content, 'the content hides after the hide animation').toBeHidden();
  await expect.poll(() => isOpen(element)).toBe(false);
}

/**
 * Opens a dialog or drawer, moves the focus out of it, and expects FocusAsync to bring it back into the element.
 *
 * @param {Page} page
 * @param {string} prefix test id prefix of the element and its calls ("dialog" or "drawer")
 */
async function focusOpenOverlay(page, prefix) {
  await callOk(page, `${prefix}-show`, QUIET);
  await expect(page.getByTestId(`${prefix}-content`)).toBeVisible();
  // the open moves the focus into the element one frame later; take it away only after that
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
  await blurAll(page);
  expect(await hasFocusWithin(page, prefix), 'the focus is outside the element').toBe(false);

  // "focuses the dialog or first focusable element within it"
  await callOk(page, `${prefix}-focus`, QUIET);
  await expect.poll(() => hasFocusWithin(page, prefix), { message: 'FocusAsync moves the focus into the element' }).toBe(true);
}

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-overlays?early=true';

/**
 * Delays the element modules of the first-render calls, so they certainly come before the definitions.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['dialog', 'toast']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'overlays',

  cases: [
    {
      name: 'WaDialog opens and closes',
      methods: ['WaDialog.ShowAsync', 'WaDialog.HideAsync'],
      tags: [DIALOG],
      run: page => showAndHide(page, 'dialog'),
    },
    {
      name: 'WaDrawer opens and closes',
      methods: ['WaDrawer.ShowAsync', 'WaDrawer.HideAsync'],
      tags: [DRAWER],
      run: page => showAndHide(page, 'drawer'),
    },
    {
      name: 'WaDropdown opens and closes its menu',
      methods: ['WaDropdown.ShowAsync', 'WaDropdown.HideAsync'],
      tags: [DROPDOWN, DROPDOWN_ITEM],
      run: async page => {
        const dropdown = page.getByTestId('dropdown');
        const item = page.getByTestId('dropdown-copy');
        await expect(item).toBeHidden();

        // the calls set the open property and return at once
        await callOk(page, 'dropdown-show', QUIET);
        await expect.poll(() => isOpen(dropdown)).toBe(true);
        await expect(item).toBeVisible();

        await callOk(page, 'dropdown-hide', QUIET);
        await expect.poll(() => isOpen(dropdown)).toBe(false);
        await expect(item, 'the menu hides after the hide animation').toBeHidden();
      },
    },
    {
      name: 'WaDropdownItem opens and closes its submenu',
      methods: ['WaDropdownItem.OpenSubmenuAsync', 'WaDropdownItem.CloseSubmenuAsync'],
      tags: [DROPDOWN, DROPDOWN_ITEM],
      run: async page => {
        const share = page.getByTestId('dropdown-share');
        const email = page.getByTestId('dropdown-share-email');

        await callOk(page, 'dropdown-show', QUIET);
        await expect(share).toBeVisible();
        await expect(email).toBeHidden();

        // the Promise of openSubmenu and closeSubmenu settles after the animation
        await callOk(page, 'dropdown-open-submenu', QUIET);
        expect(await share.evaluate(el => /** @type {any} */ (el).submenuOpen)).toBe(true);
        await expect(share).toHaveAttribute('aria-expanded', 'true');
        await expect(email).toBeVisible();

        await callOk(page, 'dropdown-close-submenu', QUIET);
        expect(await share.evaluate(el => /** @type {any} */ (el).submenuOpen)).toBe(false);
        await expect(share).toHaveAttribute('aria-expanded', 'false');
        await expect(email).toBeHidden();
        await expect(share, 'the menu stays open').toBeVisible();
      },
    },
    {
      name: 'WaPopover opens and closes',
      methods: ['WaPopover.ShowAsync', 'WaPopover.HideAsync'],
      tags: [POPOVER],
      run: async page => {
        const popover = page.getByTestId('popover');
        const content = page.getByTestId('popover-content');
        await expect(content).toBeHidden();

        // the Promise of show and hide settles after the animation, so the call completes with the element open or closed
        await callOk(page, 'popover-show', QUIET);
        expect(await isOpen(popover)).toBe(true);
        await expect(content).toBeVisible();

        // the hide call sits inside the popover, so its click does not close the popover by itself
        await callOk(page, 'popover-hide', QUIET);
        expect(await isOpen(popover)).toBe(false);
        await expect(content).toBeHidden();
      },
    },
    {
      name: 'WaTooltip opens and closes',
      methods: ['WaTooltip.ShowAsync', 'WaTooltip.HideAsync'],
      tags: [TOOLTIP],
      run: async page => {
        const tooltip = page.getByTestId('tooltip');
        const content = page.getByTestId('tooltip-content');
        await expect(content).toBeHidden();

        // the Promise of show and hide settles after the animation, so the call completes with the element open or closed
        await callOk(page, 'tooltip-show', QUIET);
        expect(await isOpen(tooltip)).toBe(true);
        await expect(content).toBeVisible();

        await callOk(page, 'tooltip-hide', QUIET);
        expect(await isOpen(tooltip)).toBe(false);
        await expect(content).toBeHidden();
      },
    },
    {
      name: 'WaPopup.RepositionAsync repositions the active popup',
      methods: ['WaPopup.RepositionAsync'],
      tags: [POPUP],
      run: async page => {
        const repositions = async () => Number(await page.getByTestId('popup-reposition-count').textContent());
        await expect.poll(repositions, { message: 'the popup positioned itself once it was active', timeout: WA_READY_TIMEOUT_MS }).toBeGreaterThan(0);
        await expect(page.getByTestId('popup-content')).toBeVisible();

        // every reposition dispatches wa-reposition, which the harness counts in .NET (OnReposition)
        const before = await repositions();
        await callOk(page, 'popup-reposition', QUIET);
        await expect.poll(repositions).toBeGreaterThan(before);

        // the popup sits below its anchor (Placement Bottom)
        const anchor = await page.getByTestId('popup-anchor').boundingBox();
        const content = await page.getByTestId('popup-content').boundingBox();
        if (!anchor || !content) throw new Error('the popup anchor and the popup content have no box');
        expect(content.y).toBeGreaterThanOrEqual(anchor.y + anchor.height - 1);
      },
    },
    {
      name: 'WaToast.CreateAsync adds a toast item, and the stack still renders its Blazor items',
      methods: ['WaToast.CreateAsync'],
      tags: [TOAST],
      run: async page => {
        const toast = page.getByTestId('toast');
        const created = toast.locator('wa-toast-item', { hasText: TOAST_MESSAGE });
        await expect(created).toHaveCount(0);

        await callOk(page, 'toast-create', QUIET);
        await expect(created).toHaveCount(1);
        await expect(created).toBeVisible();

        // WAB-64 F13: create() prepends its item into the Blazor-owned wa-toast; Blazor still adds and removes its own
        // item next to it
        await page.getByTestId('toast-add-item').click();
        const item = page.getByTestId('toast-item');
        await expect(item).toBeVisible();
        await expect(toast.getByTestId('toast-item'), 'the Blazor item is in the stack').toHaveCount(1);
        await callOk(page, 'toast-item-hide', QUIET);
        await expect(item, 'Blazor removes its item once the model drops it').toHaveCount(0);
      },
    },
    {
      name: 'WaToastItem.HideAsync hides the item, and the model then removes it',
      methods: ['WaToastItem.HideAsync'],
      tags: [TOAST],
      run: async page => {
        await page.getByTestId('toast-add-item').click();
        const item = page.getByTestId('toast-item');
        await expect(item).toBeVisible();
        await expect(page.getByTestId('toast-item-hidden-count')).toHaveText('0');

        // the Promise of hide settles after the animation and wa-after-hide; the wrapper's OnAfterHide drops the item from
        // the model (WAB-64 F13: Web Awesome removes the element itself; the interop keeps it, hidden, for Blazor)
        await callOk(page, 'toast-item-hide', QUIET);
        await expect(page.getByTestId('toast-item-hidden-count')).toHaveText('1');
        await expect(item).toHaveCount(0);
      },
    },
    {
      name: 'WaPage shows, hides and toggles the navigation drawer',
      methods: ['WaPage.ShowNavigationAsync', 'WaPage.HideNavigationAsync', 'WaPage.ToggleNavigationAsync'],
      tags: [PAGE],
      run: async page => {
        const waPage = page.getByTestId('page');
        const navOpen = () => waPage.evaluate(el => Boolean(/** @type {any} */ (el).navOpen));
        const navigation = page.getByTestId('page-nav-content');

        // the page picks its view after a resize observation; a change of view closes the navigation
        await expect.poll(() => waPage.evaluate(el => /** @type {any} */ (el).view), { timeout: WA_READY_TIMEOUT_MS }).toBe('mobile');
        expect(await navOpen()).toBe(false);
        await expect(navigation).toBeHidden();

        await callOk(page, 'page-show-nav', QUIET);
        await expect.poll(navOpen).toBe(true);
        await expect(navigation).toBeVisible();

        await callOk(page, 'page-hide-nav', QUIET);
        await expect.poll(navOpen).toBe(false);
        // the drawer must have finished closing before the next open, or its after-hide closes the navigation again
        await expect(navigation).toBeHidden();

        await callOk(page, 'page-toggle-nav', QUIET);
        await expect.poll(navOpen).toBe(true);
        await expect(navigation).toBeVisible();

        await callOk(page, 'page-toggle-nav', QUIET);
        await expect.poll(navOpen).toBe(false);
        await expect(navigation).toBeHidden();
      },
    },
    {
      // dialog, drawer and dropdown open through the open property: ShowAsync returns at once, so a cancelled show
      // cannot leave the call waiting (unlike WAB-64 F8 below)
      name: 'ShowAsync of dialog, drawer and dropdown completes when wa-show is cancelled, and the element stays closed',
      methods: ['WaDialog.ShowAsync', 'WaDrawer.ShowAsync', 'WaDropdown.ShowAsync'],
      tags: [DIALOG, DRAWER, DROPDOWN],
      run: async page => {
        for (const [prefix, content] of [['dialog', 'dialog-content'], ['drawer', 'drawer-content'], ['dropdown', 'dropdown-copy']]) {
          await cancelShow(page, prefix);
          const outcome = await callMethod(page, `${prefix}-show`, { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
          expect(outcome.done, `${prefix}: the call completes`).toBe(true);
          await expect.poll(() => isOpen(page.getByTestId(prefix)), { message: `${prefix}: the cancelled show resets open` }).toBe(false);
          await expect(page.getByTestId(content)).toBeHidden();
        }
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaDialog.ShowAsync', 'WaToast.CreateAsync'],
      route: EARLY_ROUTE,
      tags: [DIALOG, TOAST],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-dialog-show', 'early-toast-create']) await expectEarlyOutcome(page, id);
        // the dialog's open was set before the element upgraded; the upgrade keeps it
        await expect(page.getByTestId('dialog-content'), 'the dialog is open').toBeVisible({ timeout: WA_READY_TIMEOUT_MS });
        await expect(page.getByTestId('toast').locator('wa-toast-item', { hasText: TOAST_MESSAGE }), 'the toast item shows').toBeVisible();
      },
    },
  ],

  knownDefects: [
    {
      // wa-dialog has no focus() of its own, no delegatesFocus and no tabindex on the host: the native focus() of the
      // host does nothing, and the focus stays on the document body
      name: 'WaDialog.FocusAsync moves the focus into the open dialog',
      defect: 'WAB-64 F3',
      methods: ['WaDialog.FocusAsync'],
      exemption: 'overlays-dialog-drawer-focus',
      tags: [DIALOG],
      run: page => focusOpenOverlay(page, 'dialog'),
    },
    {
      // as for wa-dialog
      name: 'WaDrawer.FocusAsync moves the focus into the open drawer',
      defect: 'WAB-64 F3',
      methods: ['WaDrawer.FocusAsync'],
      exemption: 'overlays-dialog-drawer-focus',
      tags: [DRAWER],
      run: page => focusOpenOverlay(page, 'drawer'),
    },
    {
      name: 'WaPopover.ShowAsync completes when wa-show is cancelled',
      defect: 'WAB-64 F8',
      methods: ['WaPopover.ShowAsync'],
      tags: [POPOVER],
      run: async page => {
        await cancelShow(page, 'popover');
        // the Promise of show() waits for wa-after-show, which a cancelled show never dispatches
        const outcome = await callMethod(page, 'popover-show', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(outcome.done).toBe(true);
      },
    },
  ],

  external: [],
};
