// @ts-check
const { expect } = require('../test');
const { allowEvent, callMethod, callOk, cancelEvent } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "navigation" (tools\e2e\data\method-groups.json): WaAccordion, WaAccordionItem, WaDetails,
// WaTab, WaTabGroup, WaStepper, WaCarousel, WaTreeItem and WaSplitPanel on /testing/methods-navigation
// (MethodsNavigationHarness.razor). Every call runs from .NET through the wrapper's @ref (a MethodCall button); the case
// asserts the effect on the element, and the returned value where there is one. All elements are free components.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const ACCORDION = 'wa-accordion';
const ACCORDION_ITEM = 'wa-accordion-item';
const DETAILS = 'wa-details';
const TAB_GROUP = 'wa-tab-group';
const TAB = 'wa-tab';
const TAB_PANEL = 'wa-tab-panel';
const STEPPER = 'wa-stepper';
const STEP = 'wa-step';
const CAROUSEL = 'wa-carousel';
const CAROUSEL_ITEM = 'wa-carousel-item';
const TREE = 'wa-tree';
const TREE_ITEM = 'wa-tree-item';
const SPLIT_PANEL = 'wa-split-panel';

// the accordion items of the harness, in order; the last one is disabled
const ACCORDION_ITEMS = ['accordion-item-first', 'accordion-item-second', 'accordion-item-disabled'];

// the shadow parts that hold the collapsible content of an accordion item and of a details
const ITEM_PANEL = '[part~="panel"]';
const DETAILS_BODY = '.body';

// the tab panels and steps of the harness
const GENERAL_PANEL = 'general';
const ADVANCED_PANEL = 'advanced';
const SHIPPING_STEP = 'shipping';
const PAYMENT_STEP = 'payment';

// the carousel of the harness: four slides, GoToSlideAsync goes to index 2
const TARGET_SLIDE = 2;
const LAST_SLIDE = 3;

// the share of a slide in the viewport that counts as shown
const SHOWN_RATIO = 0.9;

// the tree of the harness: the parent item has two child items, the first with an id, the second disabled
const TREE_CHILDREN = [
  { id: 'tree-apple', text: 'Apple', selected: false, expanded: false, disabled: false, lazy: false },
  { id: null, text: 'Banana', selected: false, expanded: false, disabled: true, lazy: false },
];

// the split panel of the harness: 400 px wide, divider at 25 %
const SPLIT_WIDTH_PX = 400;
const INITIAL_SPLIT_POSITION = 25;
const FULL_SPLIT_POSITION = 100;

// calls that must not move the focus or close a popup (see callMethod)
const QUIET = { quiet: true };

// how long a call may take before the cancelled-show case calls it hanging
const SETTLE_TIMEOUT_MS = 5_000;

// the delay of the element modules in the first-render case, so the first render certainly comes before the definitions
// (long enough for the circuit of a prerendered page to start first)
const MODULE_DELAY_MS = 4_000;

/**
 * Whether the element with the test id is the document's focused element (focus inside its shadow root included).
 *
 * @param {Page} page
 * @param {string} testId
 */
function hasFocus(page, testId) {
  return page.getByTestId(testId).evaluate(el => document.activeElement === el);
}

/**
 * The expanded state of each accordion item of the harness, in order.
 *
 * @param {Page} page
 */
async function expandedItems(page) {
  const states = [];
  for (const id of ACCORDION_ITEMS) states.push(await page.getByTestId(id).evaluate(el => /** @type {any} */ (el).expanded));
  return states;
}

/**
 * The rendered height of a part of an element's shadow root, in whole pixels (0 when the part is collapsed or not
 * rendered); a collapsed accordion item or details keeps its content in the DOM, so the height shows what a user sees.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} selector
 */
function shadowHeight(page, testId, selector) {
  return page.getByTestId(testId).evaluate((el, s) => Math.round(el.shadowRoot?.querySelector(s)?.getBoundingClientRect().height ?? 0), selector);
}

/**
 * The name of the active tab panel of the tab group, as the panels and the tabs show it.
 *
 * @param {Page} page
 */
function activeTab(page) {
  return page.getByTestId('tab-group').evaluate(el => {
    const panels = [...el.querySelectorAll('wa-tab-panel')].filter(p => /** @type {any} */ (p).active).map(p => p.getAttribute('name'));
    const tabs = [...el.querySelectorAll('wa-tab')].filter(t => /** @type {any} */ (t).active).map(t => t.getAttribute('panel'));
    return { group: /** @type {any} */ (el).active, panels, tabs };
  });
}

/**
 * Scrolls the tab group into view: it activates its first tab (or the one named by active) only once it intersects the
 * viewport.
 *
 * @param {Page} page
 */
async function showTabGroup(page) {
  await page.getByTestId('tab-group').scrollIntoViewIfNeeded();
}

/**
 * The active step of the stepper, as the stepper and its steps show it.
 *
 * @param {Page} page
 */
function activeStep(page) {
  return page.getByTestId('stepper').evaluate(el => ({
    stepper: /** @type {any} */ (el).active,
    steps: [...el.querySelectorAll('wa-step')].filter(s => /** @type {any} */ (s).active).map(s => s.getAttribute('name')),
  }));
}

/**
 * The active slide index of the carousel.
 *
 * @param {Page} page
 */
function activeSlide(page) {
  return page.getByTestId('carousel').evaluate(el => /** @type {any} */ (el).activeSlide);
}

/**
 * The live position of the split panel (percent and pixels).
 *
 * @param {Page} page
 */
function splitPosition(page) {
  return page.getByTestId('split-panel').evaluate(el => ({
    position: /** @type {any} */ (el).position,
    pixels: /** @type {any} */ (el).positionInPixels,
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

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-navigation?early=true';

/**
 * Delays the element modules of the first-render calls, so the calls certainly come before the definitions
 * (accordion.js imports the accordion item and tab-group.js the tab, so both modules of each pair wait).
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['accordion', 'accordion-item', 'tab-group', 'tab', 'tab-panel']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'navigation',

  cases: [
    {
      name: 'WaAccordion expands and collapses all items, the disabled one excepted',
      methods: ['WaAccordion.ExpandAllAsync', 'WaAccordion.CollapseAllAsync'],
      tags: [ACCORDION, ACCORDION_ITEM],
      run: async page => {
        expect(await expandedItems(page)).toEqual([false, false, false]);

        // expandAll() does not wait for the animations, so the state is polled
        await callOk(page, 'accordion-expand-all', QUIET);
        await expect.poll(() => expandedItems(page), { message: 'every enabled item expands' }).toEqual([true, true, false]);
        await expect.poll(() => shadowHeight(page, 'accordion-item-second', ITEM_PANEL), { message: 'the content opens' }).toBeGreaterThan(0);

        await callOk(page, 'accordion-collapse-all', QUIET);
        await expect.poll(() => expandedItems(page)).toEqual([false, false, false]);
      },
    },
    {
      name: 'WaAccordionItem expands, collapses, toggles and takes the focus',
      methods: ['WaAccordionItem.ExpandAsync', 'WaAccordionItem.CollapseAsync', 'WaAccordionItem.ToggleAsync', 'WaAccordionItem.FocusAsync'],
      tags: [ACCORDION, ACCORDION_ITEM],
      run: async page => {
        const item = page.getByTestId('accordion-item-first');
        const contentHeight = () => shadowHeight(page, 'accordion-item-first', ITEM_PANEL);
        expect(await contentHeight()).toBe(0);

        // the Promise of expand and collapse settles after the animation, so the call completes with the item open or closed
        await callOk(page, 'accordion-item-expand', QUIET);
        expect(await item.evaluate(el => /** @type {any} */ (el).expanded)).toBe(true);
        expect(await contentHeight(), 'the content is open').toBeGreaterThan(0);
        expect(await expandedItems(page), 'the other items stay as they are').toEqual([true, false, false]);

        await callOk(page, 'accordion-item-collapse', QUIET);
        expect(await item.evaluate(el => /** @type {any} */ (el).expanded)).toBe(false);
        expect(await contentHeight(), 'the content is closed').toBe(0);

        await callOk(page, 'accordion-item-toggle', QUIET);
        expect(await item.evaluate(el => /** @type {any} */ (el).expanded), 'the toggle expands a collapsed item').toBe(true);
        await callOk(page, 'accordion-item-toggle', QUIET);
        expect(await item.evaluate(el => /** @type {any} */ (el).expanded), 'the toggle collapses an expanded item').toBe(false);

        // the focus goes to the item's trigger button in its shadow root
        await callOk(page, 'accordion-item-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'accordion-item-first')).toBe(true);
      },
    },
    {
      name: 'WaDetails shows and hides its content',
      methods: ['WaDetails.ShowAsync', 'WaDetails.HideAsync'],
      tags: [DETAILS],
      run: async page => {
        const details = page.getByTestId('details');
        const contentHeight = () => shadowHeight(page, 'details', DETAILS_BODY);
        expect(await contentHeight()).toBe(0);

        // the Promise of show and hide settles after the animation, so the call completes with the element open or closed
        await callOk(page, 'details-show', QUIET);
        expect(await details.evaluate(el => /** @type {any} */ (el).open)).toBe(true);
        expect(await contentHeight(), 'the content is open').toBeGreaterThan(0);

        await callOk(page, 'details-hide', QUIET);
        expect(await details.evaluate(el => /** @type {any} */ (el).open)).toBe(false);
        expect(await contentHeight(), 'the content is closed').toBe(0);
      },
    },
    {
      name: 'WaTabGroup shows a tab panel by name and dispatches wa-tab-show',
      methods: ['WaTabGroup.ShowTabAsync'],
      tags: [TAB_GROUP, TAB, TAB_PANEL],
      run: async page => {
        await showTabGroup(page);
        await expect.poll(() => activeTab(page)).toEqual({ group: GENERAL_PANEL, panels: [GENERAL_PANEL], tabs: [GENERAL_PANEL] });

        await callOk(page, 'tab-group-show');
        await expect.poll(() => activeTab(page)).toEqual({ group: ADVANCED_PANEL, panels: [ADVANCED_PANEL], tabs: [ADVANCED_PANEL] });
        await expect(page.getByTestId('tab-group').getByText('The advanced panel.')).toBeVisible();
        await expect(page.getByTestId('tab-shown'), 'OnTabShow receives the shown panel').toHaveText(ADVANCED_PANEL);

        await callOk(page, 'tab-group-show-general');
        await expect.poll(() => activeTab(page)).toEqual({ group: GENERAL_PANEL, panels: [GENERAL_PANEL], tabs: [GENERAL_PANEL] });
        await expect(page.getByTestId('tab-shown')).toHaveText(GENERAL_PANEL);
      },
    },
    {
      name: 'WaTab takes and leaves the focus',
      methods: ['WaTab.FocusAsync', 'WaTab.BlurAsync'],
      tags: [TAB_GROUP, TAB],
      run: async page => {
        await showTabGroup(page);
        await expect.poll(() => activeTab(page).then(t => t.group)).toBe(GENERAL_PANEL);

        await callOk(page, 'tab-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'tab-custom')).toBe(true);
        // a focused tab does not activate its panel; only a key press or a click does
        expect((await activeTab(page)).group).toBe(GENERAL_PANEL);

        await callOk(page, 'tab-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'tab-custom')).toBe(false);
      },
    },
    {
      name: 'WaStepper goes to a named step, back and forward',
      methods: ['WaStepper.GoToAsync', 'WaStepper.PreviousAsync', 'WaStepper.NextAsync'],
      tags: [STEPPER, STEP],
      run: async page => {
        await expect.poll(() => activeStep(page)).toEqual({ stepper: '', steps: ['cart'] });

        await callOk(page, 'stepper-go-to');
        await expect.poll(() => activeStep(page)).toEqual({ stepper: PAYMENT_STEP, steps: [PAYMENT_STEP] });

        await callOk(page, 'stepper-previous');
        await expect.poll(() => activeStep(page)).toEqual({ stepper: SHIPPING_STEP, steps: [SHIPPING_STEP] });

        await callOk(page, 'stepper-next');
        await expect.poll(() => activeStep(page)).toEqual({ stepper: PAYMENT_STEP, steps: [PAYMENT_STEP] });
      },
    },
    {
      name: 'WaCarousel goes to a slide, forward and back',
      methods: ['WaCarousel.GoToSlideAsync', 'WaCarousel.NextAsync', 'WaCarousel.PreviousAsync'],
      tags: [CAROUSEL, CAROUSEL_ITEM],
      run: async page => {
        const carousel = page.getByTestId('carousel');
        const slide = (/** @type {number} */ index) => carousel.getByText(`Slide ${index + 1}`, { exact: true });
        // a slide counts as shown when it is in the page viewport (the scroll container clips the others); quiet calls
        // keep the page scrolled to the carousel
        await carousel.scrollIntoViewIfNeeded();
        expect(await activeSlide(page)).toBe(0);

        // the scroll is smooth, so each slide change is polled before the next call
        await callOk(page, 'carousel-go-to', QUIET);
        await expect.poll(() => activeSlide(page)).toBe(TARGET_SLIDE);
        await expect(slide(TARGET_SLIDE)).toBeInViewport({ ratio: SHOWN_RATIO });

        await callOk(page, 'carousel-next', QUIET);
        await expect.poll(() => activeSlide(page)).toBe(LAST_SLIDE);
        await expect(slide(LAST_SLIDE)).toBeInViewport({ ratio: SHOWN_RATIO });

        await callOk(page, 'carousel-previous', QUIET);
        await expect.poll(() => activeSlide(page)).toBe(TARGET_SLIDE);
        await expect(slide(TARGET_SLIDE)).toBeInViewport({ ratio: SHOWN_RATIO });
      },
    },
    {
      name: 'WaTreeItem.GetChildrenItemsAsync returns no items for a leaf',
      methods: ['WaTreeItem.GetChildrenItemsAsync'],
      tags: [TREE, TREE_ITEM],
      run: async page => {
        expect(await callOk(page, 'tree-leaf-children'), 'the number of child items').toBe(0);
      },
    },
    {
      name: 'WaSplitPanel returns its live position in percent and in pixels',
      methods: ['WaSplitPanel.GetPositionAsync', 'WaSplitPanel.GetPositionInPixelsAsync'],
      tags: [SPLIT_PANEL],
      run: async page => {
        await expect.poll(() => splitPosition(page)).toEqual({ position: INITIAL_SPLIT_POSITION, pixels: SPLIT_WIDTH_PX * INITIAL_SPLIT_POSITION / 100 });
        expect(await callOk(page, 'split-position')).toBe(INITIAL_SPLIT_POSITION);
        expect(await callOk(page, 'split-position-px')).toBe(SPLIT_WIDTH_PX * INITIAL_SPLIT_POSITION / 100);

        // the user moves the divider to the end: the getters read the element, not the Position parameter
        await page.getByTestId('split-panel').locator('[part~="divider"]').focus();
        await page.keyboard.press('End');
        await expect.poll(() => splitPosition(page)).toEqual({ position: FULL_SPLIT_POSITION, pixels: SPLIT_WIDTH_PX });
        expect(await callOk(page, 'split-position')).toBe(FULL_SPLIT_POSITION);
        expect(await callOk(page, 'split-position-px')).toBe(SPLIT_WIDTH_PX);
      },
    },
    {
      // the child items are live elements whose fields point back to them, so they do not serialize; the wrapper
      // returns a plain projection of each one
      name: 'WaTreeItem.GetChildrenItemsAsync returns the child items of a parent',
      methods: ['WaTreeItem.GetChildrenItemsAsync'],
      tags: [TREE, TREE_ITEM],
      run: async page => {
        expect(await callOk(page, 'tree-parent-children')).toEqual(TREE_CHILDREN);
      },
    },
    {
      // the Promise of show() and hide() waits for the after-event, which a cancelled action never dispatches; the
      // wrapper completes the call when the action is cancelled, as dialog, drawer and dropdown do
      name: 'WaDetails.ShowAsync and HideAsync complete when wa-show and wa-hide are cancelled',
      methods: ['WaDetails.ShowAsync', 'WaDetails.HideAsync'],
      tags: [DETAILS],
      run: async page => {
        const isOpen = () => page.getByTestId('details').evaluate(el => /** @type {any} */ (el).open);
        await cancelEvent(page, 'details', 'wa-show');
        const shown = await callMethod(page, 'details-show', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(shown.done, 'the cancelled show completes').toBe(true);
        expect(await isOpen(), 'the cancelled show leaves the details closed').toBe(false);

        await allowEvent(page, 'details', 'wa-show');
        await callOk(page, 'details-show', QUIET);
        expect(await isOpen(), 'the details open').toBe(true);

        await cancelEvent(page, 'details', 'wa-hide');
        const hidden = await callMethod(page, 'details-hide', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(hidden.done, 'the cancelled hide completes').toBe(true);
        expect(await isOpen(), 'the cancelled hide leaves the details open').toBe(true);
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaAccordionItem.FocusAsync', 'WaTabGroup.ShowTabAsync'],
      route: EARLY_ROUTE,
      tags: [ACCORDION, ACCORDION_ITEM, TAB_GROUP, TAB, TAB_PANEL],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-accordion-focus', 'early-tab-show']) await expectEarlyOutcome(page, id);
        await expect.poll(() => hasFocus(page, 'accordion-item-first'), { message: 'the accordion item has the focus' }).toBe(true);
        await showTabGroup(page);
        await expect.poll(() => activeTab(page), { message: 'the advanced tab is active', timeout: WA_READY_TIMEOUT_MS })
          .toEqual({ group: ADVANCED_PANEL, panels: [ADVANCED_PANEL], tabs: [ADVANCED_PANEL] });
      },
    },
  ],

  knownDefects: [],

  external: [],
};
