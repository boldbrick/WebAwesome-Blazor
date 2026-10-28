// @ts-check
const { expect } = require('../test');
const { callOk } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "forms-choice" (tools\e2e\data\method-groups.json): WaButton, WaCheckbox, WaSwitch,
// WaRadio, WaRadioGroup, WaRating, WaSlider and WaRange on /testing/methods-forms-choice
// (MethodsFormsChoiceHarness.razor). Every call runs from .NET through the wrapper's @ref (a MethodCall button); the
// case asserts the effect on the element. All elements are free components. The group has no known WAB-64 defect; the
// focus, blur and click methods of button, checkbox and switch use a shadow DOM part that exists only after the first
// Lit update (audit table B, precondition R), which the timing case covers.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const BUTTON = 'wa-button';
const CHECKBOX = 'wa-checkbox';
const SWITCH = 'wa-switch';
const RADIO = 'wa-radio';
const RADIO_GROUP = 'wa-radio-group';
const RATING = 'wa-rating';
const SLIDER = 'wa-slider';

// the custom validity message of the harness
const VALIDITY_MESSAGE = 'Pick one, please';

// the slider values of the harness: a step of 10, the single slider at 50, the range at 20..60
const SLIDER_START = 50;
const SLIDER_STEP = 10;
const RANGE_START_MIN = 20;
const RANGE_START_MAX = 60;

// calls that must not move the focus (see callMethod)
const QUIET = { quiet: true };

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
 * The custom validity state of a form control.
 *
 * @param {Page} page
 * @param {string} testId
 */
function validityOf(page, testId) {
  return page.getByTestId(testId).evaluate(el => {
    const control = /** @type {any} */ (el);
    return { message: control.validationMessage, customError: control.validity.customError };
  });
}

/**
 * Sets a custom validity through one call and clears it through another, and asserts both on the element.
 *
 * @param {Page} page
 * @param {string} testId test id of the element
 * @param {string} prefix test id prefix of its MethodCall buttons
 */
async function expectValidityCalls(page, testId, prefix) {
  expect(await validityOf(page, testId), `${testId} starts valid`).toEqual({ message: '', customError: false });
  await callOk(page, `${prefix}-set-validity`);
  await expect.poll(() => validityOf(page, testId)).toEqual({ message: VALIDITY_MESSAGE, customError: true });
  await callOk(page, `${prefix}-reset-validity`);
  await expect.poll(() => validityOf(page, testId)).toEqual({ message: '', customError: false });
}

/**
 * Takes and leaves the focus through the wrapper and asserts both on the element.
 *
 * @param {Page} page
 * @param {string} testId test id of the element
 * @param {string} prefix test id prefix of its MethodCall buttons
 */
async function expectFocusCalls(page, testId, prefix) {
  await callOk(page, `${prefix}-focus`, QUIET);
  await expect.poll(() => hasFocus(page, testId)).toBe(true);
  await callOk(page, `${prefix}-blur`, QUIET);
  await expect.poll(() => hasFocus(page, testId)).toBe(false);
}

/**
 * Toggles a checkbox or switch twice through the wrapper; each click changes the element and, through the change event,
 * the bound value.
 *
 * @param {Page} page
 * @param {string} testId test id of the element
 */
async function expectToggleClicks(page, testId) {
  const element = page.getByTestId(testId);
  const bound = page.getByTestId(`${testId}-value`);
  await expect(bound).toHaveText('False');

  await callOk(page, `${testId}-click`);
  await expect.poll(() => element.evaluate(el => /** @type {any} */ (el).checked)).toBe(true);
  await expect(bound, 'the change event updates the bound value').toHaveText('True');

  await callOk(page, `${testId}-click`);
  await expect.poll(() => element.evaluate(el => /** @type {any} */ (el).checked)).toBe(false);
  await expect(bound).toHaveText('False');
}

/**
 * The value, the range bounds and the focused shadow part of a slider.
 *
 * @param {Page} page
 * @param {string} testId
 */
function sliderState(page, testId) {
  return page.getByTestId(testId).evaluate(el => {
    const slider = /** @type {any} */ (el);
    return { value: slider.value, minValue: slider.minValue, maxValue: slider.maxValue, focusedPart: slider.shadowRoot?.activeElement?.id ?? null };
  });
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
const EARLY_ROUTE = '/testing/methods-forms-choice?early=true';

/**
 * Delays the element modules of the first-render calls, so the calls certainly come before the definitions. Nothing
 * else on the harness page or in the demo shell imports wa-slider or wa-checkbox.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['slider', 'checkbox']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'forms-choice',

  cases: [
    {
      name: 'WaButton takes and leaves the focus, clicks and reports a custom validity',
      methods: ['WaButton.FocusAsync', 'WaButton.BlurAsync', 'WaButton.ClickAsync', 'WaButton.SetCustomValidityAsync',
        'WaButton.ResetValidityAsync'],
      tags: [BUTTON],
      run: async page => {
        await expectFocusCalls(page, 'button', 'button');

        // click() clicks the inner button; the click reaches the wrapper's OnClick
        const clicks = page.getByTestId('button-clicks');
        await expect(clicks).toHaveText('0');
        await callOk(page, 'button-click');
        await expect(clicks).toHaveText('1');

        await expectValidityCalls(page, 'button', 'button');
      },
    },
    {
      name: 'WaCheckbox takes and leaves the focus, toggles on a click and reports a custom validity',
      methods: ['WaCheckbox.FocusAsync', 'WaCheckbox.BlurAsync', 'WaCheckbox.ClickAsync', 'WaCheckbox.SetCustomValidityAsync',
        'WaCheckbox.ResetValidityAsync'],
      tags: [CHECKBOX],
      run: async page => {
        await expectFocusCalls(page, 'checkbox', 'checkbox');
        await expectToggleClicks(page, 'checkbox');
        await expectValidityCalls(page, 'checkbox', 'checkbox');
      },
    },
    {
      name: 'WaSwitch takes and leaves the focus, toggles on a click and reports a custom validity',
      methods: ['WaSwitch.FocusAsync', 'WaSwitch.BlurAsync', 'WaSwitch.ClickAsync', 'WaSwitch.SetCustomValidityAsync',
        'WaSwitch.ResetValidityAsync'],
      tags: [SWITCH],
      run: async page => {
        await expectFocusCalls(page, 'switch', 'switch');
        await expectToggleClicks(page, 'switch');
        await expectValidityCalls(page, 'switch', 'switch');
      },
    },
    {
      name: 'WaRadioGroup moves the focus to its checked radio and reports a custom validity',
      methods: ['WaRadioGroup.FocusAsync', 'WaRadioGroup.SetCustomValidityAsync', 'WaRadioGroup.ResetValidityAsync'],
      tags: [RADIO_GROUP, RADIO],
      run: async page => {
        await expect.poll(() => page.getByTestId('radio-beta').evaluate(el => /** @type {any} */ (el).checked), { timeout: WA_READY_TIMEOUT_MS }).toBe(true);

        // the group passes the focus on to the checked radio (beta), not to the first one
        await callOk(page, 'radio-group-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'radio-beta')).toBe(true);
        expect(await hasFocus(page, 'radio-alpha')).toBe(false);

        await expectValidityCalls(page, 'radio-group', 'radio-group');
      },
    },
    {
      name: 'WaRadio reports a custom validity',
      methods: ['WaRadio.SetCustomValidityAsync', 'WaRadio.ResetValidityAsync'],
      tags: [RADIO_GROUP, RADIO],
      run: async page => {
        await expectValidityCalls(page, 'radio-alpha', 'radio');
      },
    },
    {
      name: 'WaRating takes and leaves the focus and reports a custom validity',
      methods: ['WaRating.FocusAsync', 'WaRating.BlurAsync', 'WaRating.SetCustomValidityAsync', 'WaRating.ResetValidityAsync'],
      tags: [RATING],
      run: async page => {
        await expectFocusCalls(page, 'rating', 'rating');
        await expectValidityCalls(page, 'rating', 'rating');
      },
    },
    {
      name: 'WaSlider takes and leaves the focus, steps its value and reports a custom validity',
      methods: ['WaSlider.FocusAsync', 'WaSlider.BlurAsync', 'WaSlider.StepUpAsync', 'WaSlider.StepDownAsync',
        'WaSlider.SetCustomValidityAsync', 'WaSlider.ResetValidityAsync'],
      tags: [SLIDER],
      run: async page => {
        await expect.poll(async () => (await sliderState(page, 'slider')).value, { timeout: WA_READY_TIMEOUT_MS }).toBe(SLIDER_START);

        await expectFocusCalls(page, 'slider', 'slider');

        // a step moves the value by the step attribute (10)
        await callOk(page, 'slider-step-up');
        await expect.poll(async () => (await sliderState(page, 'slider')).value).toBe(SLIDER_START + SLIDER_STEP);
        await callOk(page, 'slider-step-down');
        await callOk(page, 'slider-step-down');
        await expect.poll(async () => (await sliderState(page, 'slider')).value).toBe(SLIDER_START - SLIDER_STEP);

        await expectValidityCalls(page, 'slider', 'slider');
      },
    },
    {
      name: 'WaRange in range mode focuses the minimum thumb, steps its bounds and reports a custom validity',
      methods: ['WaRange.FocusAsync', 'WaRange.BlurAsync', 'WaRange.StepUpAsync', 'WaRange.StepDownAsync',
        'WaRange.SetCustomValidityAsync', 'WaRange.ResetValidityAsync'],
      tags: [SLIDER],
      run: async page => {
        await expect.poll(async () => {
          const { minValue, maxValue } = await sliderState(page, 'range');
          return [minValue, maxValue];
        }, { timeout: WA_READY_TIMEOUT_MS }).toEqual([RANGE_START_MIN, RANGE_START_MAX]);

        // range mode: the focus goes to the minimum thumb
        await callOk(page, 'range-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'range')).toBe(true);
        expect((await sliderState(page, 'range')).focusedPart).toBe('thumb-min');
        await callOk(page, 'range-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'range')).toBe(false);

        // range mode: a step up moves the maximum, a step down the minimum
        await callOk(page, 'range-step-up');
        await expect.poll(async () => (await sliderState(page, 'range')).maxValue).toBe(RANGE_START_MAX + SLIDER_STEP);
        expect((await sliderState(page, 'range')).minValue).toBe(RANGE_START_MIN);
        await callOk(page, 'range-step-down');
        await expect.poll(async () => (await sliderState(page, 'range')).minValue).toBe(RANGE_START_MIN - SLIDER_STEP);
        expect((await sliderState(page, 'range')).maxValue).toBe(RANGE_START_MAX + SLIDER_STEP);

        await expectValidityCalls(page, 'range', 'range');
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined and rendered',
      methods: ['WaSlider.FocusAsync', 'WaCheckbox.ClickAsync'],
      route: EARLY_ROUTE,
      tags: [SLIDER, CHECKBOX],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-slider-focus', 'early-checkbox-click']) await expectEarlyOutcome(page, id);
        await expect.poll(() => hasFocus(page, 'slider'), { message: 'the slider has the focus' }).toBe(true);
        await expect.poll(() => page.getByTestId('checkbox').evaluate(el => /** @type {any} */ (el).checked), { message: 'the checkbox is checked' }).toBe(true);
        await expect(page.getByTestId('checkbox-value'), 'the change event updates the bound value').toHaveText('True');
      },
    },
  ],

  knownDefects: [],

  external: [],
};
