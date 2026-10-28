// @ts-check
const { expect } = require('../test');
const { callMethod, callOk } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "pickers" (tools\e2e\data\method-groups.json): WaColorPicker, WaSelect and WaTimeInput (free),
// WaDateInput, WaDateRangeInput, WaDatePicker and WaDateRangePicker (Pro) on /testing/methods-pickers
// (MethodsPickersHarness.razor). Every call runs from .NET through the wrapper's @ref (a MethodCall button); the case
// asserts the effect on the element, and the returned value where there is one. The date cases skip on the free CDN.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const COLOR_PICKER = 'wa-color-picker';
const SELECT = 'wa-select';
const TIME_INPUT = 'wa-time-input';
const DATE_INPUT = 'wa-date-input';
const DATE_PICKER = 'wa-date-picker';

// the custom validity message of the harness
const VALIDITY_MESSAGE = 'Pick one, please';

// the initial color of the harness (#ff0000) in the formats the cases ask for
const INITIAL_RGB = 'rgb(255, 0, 0)';
const INITIAL_HSL = 'hsl(0, 100%, 50%)';

// the swatches the harness sets
const SWATCH_COLORS = ['#ff0000', '#00ff00', '#0000ff'];

// the dates of the harness: the fixed "today", the initial value, the initial range start and the GoToDateAsync target
const TODAY = '2026-06-10';
const INITIAL_DATE = '2026-06-12';
const INITIAL_RANGE_START = '2026-06-08';
const GO_TO_TARGET = '2027-03-15';

// the month of the initial values, as the start of an ISO date
const INITIAL_MONTH_PREFIX = '2026-06-';

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
 * The open property of a popup element.
 *
 * @param {Page} page
 * @param {string} testId
 * @returns {Promise<boolean>}
 */
function isOpen(page, testId) {
  return page.getByTestId(testId).evaluate(el => /** @type {any} */ (el).open);
}

/**
 * The value property of an element.
 *
 * @param {Page} page
 * @param {string} testId
 * @returns {Promise<string>}
 */
function valueOf(page, testId) {
  return page.getByTestId(testId).evaluate(el => /** @type {any} */ (el).value);
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
 * Starts to record the next event of a type on an element; returns a function that tells whether it has come.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} type
 * @returns {Promise<() => Promise<boolean>>}
 */
async function recordNextEvent(page, testId, type) {
  const key = `${testId}:${type}`;
  await page.getByTestId(testId).evaluate((el, k) => {
    const w = /** @type {any} */ (window);
    w.pickerMethodEvents = w.pickerMethodEvents ?? new Set();
    w.pickerMethodEvents.delete(k);
    el.addEventListener(k.slice(k.lastIndexOf(':') + 1), () => w.pickerMethodEvents.add(k), { once: true });
  }, key);
  return () => page.evaluate(k => /** @type {any} */ (window).pickerMethodEvents?.has(k) ?? false, key);
}

/**
 * Focus, blur, show, hide and the custom validity of a popup form control: the methods every popup input shares.
 *
 * @param {Page} page
 * @param {string} prefix the test id prefix of the MethodCalls
 * @param {string} testId the test id of the element
 */
async function provePopupInput(page, prefix, testId) {
  await callOk(page, `${prefix}-focus`, QUIET);
  await expect.poll(() => hasFocus(page, testId), { message: `${testId} has the focus` }).toBe(true);
  await callOk(page, `${prefix}-blur`, QUIET);
  await expect.poll(() => hasFocus(page, testId), { message: `${testId} has lost the focus` }).toBe(false);

  // the Promise of show and hide settles after the animation, so the call completes with the element open or closed
  await callOk(page, `${prefix}-show`, QUIET);
  expect(await isOpen(page, testId), `${testId} is open`).toBe(true);
  await callOk(page, `${prefix}-hide`, QUIET);
  expect(await isOpen(page, testId), `${testId} is closed`).toBe(false);

  await callOk(page, `${prefix}-set-validity`);
  expect(await validityOf(page, testId)).toEqual({ message: VALIDITY_MESSAGE, customError: true });
  await callOk(page, `${prefix}-reset-validity`);
  expect(await validityOf(page, testId)).toEqual({ message: '', customError: false });
}

/**
 * ClearAsync of a date control: the element and the bound .NET value become empty.
 *
 * @param {Page} page
 * @param {string} prefix the test id prefix of the MethodCalls
 * @param {string} testId the test id of the element
 * @param {string} initial a part of the initial bound value
 */
async function proveClear(page, prefix, testId, initial) {
  await expect(page.getByTestId(`${testId}-value`)).toContainText(initial);
  await callOk(page, `${prefix}-clear`);
  await expect.poll(() => valueOf(page, testId), { message: `${testId} is empty` }).toBe('');
  await expect(page.getByTestId(`${testId}-value`), 'the bound value is empty').toHaveText('');
}

/**
 * The calendar navigation and focus of a date picker: FocusAsync, GoToDateAsync, GoToTodayAsync and ClearAsync.
 *
 * @param {Page} page
 * @param {string} prefix the test id prefix of the MethodCalls (equal to the element's test id)
 * @param {string} initial a part of the initial bound value
 */
async function proveDatePicker(page, prefix, initial) {
  const picker = page.getByTestId(prefix);
  const focusedDay = () => picker.evaluate(el => el.shadowRoot?.activeElement?.getAttribute('data-date') ?? null);
  const focusedDate = () => picker.evaluate(el => el.getAttribute('focused-date'));
  const dayCell = (/** @type {string} */ date) => picker.locator(`[part~="day"][data-date="${date}"]`);

  // the calendar takes the focus on a day of the month it shows (the month of the value)
  await callOk(page, `${prefix}-focus`, QUIET);
  await expect.poll(() => hasFocus(page, prefix), { message: `${prefix} has the focus` }).toBe(true);
  expect(await focusedDay(), 'a day of the shown month has the focus').toMatch(new RegExp(`^${INITIAL_MONTH_PREFIX}`));

  await callOk(page, `${prefix}-go-to-date`);
  await expect.poll(focusedDate, { message: 'the target date is the focused day' }).toBe(GO_TO_TARGET);
  await expect(dayCell(GO_TO_TARGET), 'the calendar shows the target month').toHaveAttribute('tabindex', '0');

  await callOk(page, `${prefix}-go-to-today`);
  await expect.poll(focusedDate, { message: 'today (the Today parameter) is the focused day' }).toBe(TODAY);
  await expect(dayCell(TODAY), 'the calendar shows the month of today').toHaveAttribute('data-today', '');

  await proveClear(page, prefix, prefix, initial);
}

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-pickers?early=true';

/**
 * Delays the element modules of the first-render calls, so the calls certainly come before the definitions.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['select', 'color-picker']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/**
 * Waits until a first-render call of the harness has completed, and expects its outcome.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} expected "done", or the returned value
 */
async function expectEarlyOutcome(page, testId, expected) {
  const output = page.getByTestId(testId);
  await expect(output, `${testId} was called`).not.toHaveText('not called', { timeout: WA_READY_TIMEOUT_MS });
  await expect(output, `${testId} completed without an error`).toHaveText(expected);
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'pickers',

  cases: [
    {
      name: 'WaColorPicker takes and leaves the focus, opens and closes, and returns its value in a format',
      methods: ['WaColorPicker.FocusAsync', 'WaColorPicker.BlurAsync', 'WaColorPicker.ShowAsync', 'WaColorPicker.HideAsync',
        'WaColorPicker.GetFormattedValueAsync'],
      tags: [COLOR_PICKER],
      run: async page => {
        const panel = page.getByTestId('color-picker').locator('[part~="color-picker"]');

        await callOk(page, 'color-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'color-picker')).toBe(true);
        await callOk(page, 'color-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'color-picker')).toBe(false);

        // the Promise of show and hide settles after the animation, so the call completes with the panel shown or hidden
        await callOk(page, 'color-show', QUIET);
        expect(await isOpen(page, 'color-picker')).toBe(true);
        await expect(panel).toBeVisible();
        await callOk(page, 'color-hide', QUIET);
        expect(await isOpen(page, 'color-picker')).toBe(false);
        await expect(panel).toBeHidden();

        // the value of the harness is #ff0000
        expect(await callOk(page, 'color-format-rgb')).toBe(INITIAL_RGB);
        expect(await callOk(page, 'color-format-hsl')).toBe(INITIAL_HSL);
      },
    },
    {
      name: 'WaColorPicker.SetSwatchesAsync sets and renders the swatches',
      methods: ['WaColorPicker.SetSwatchesAsync'],
      tags: [COLOR_PICKER],
      run: async page => {
        const swatches = page.getByTestId('color-picker').locator('[part~="swatch"]');
        await expect(swatches, 'no swatches before the call').toHaveCount(0);

        await callOk(page, 'color-set-swatches');
        expect(await page.getByTestId('color-picker').evaluate(el => /** @type {any} */ (el).swatches)).toEqual(SWATCH_COLORS);
        await expect(swatches).toHaveCount(SWATCH_COLORS.length);
        expect(await swatches.evaluateAll(cells => cells.map(cell => cell.getAttribute('aria-label')))).toEqual(SWATCH_COLORS);
      },
    },
    {
      name: 'WaColorPicker reports, sets and resets a custom validity',
      methods: ['WaColorPicker.SetCustomValidityAsync', 'WaColorPicker.ResetValidityAsync', 'WaColorPicker.ReportValidityAsync'],
      tags: [COLOR_PICKER],
      run: async page => {
        expect(await callOk(page, 'color-report-validity', QUIET), 'a valid picker').toBe(true);
        expect(await isOpen(page, 'color-picker'), 'a valid picker stays closed').toBe(false);

        await callOk(page, 'color-set-validity');
        expect(await validityOf(page, 'color-picker')).toEqual({ message: VALIDITY_MESSAGE, customError: true });

        // an invalid, closed picker opens its panel to show the message, and reports false
        const shown = await recordNextEvent(page, 'color-picker', 'wa-after-show');
        expect(await callOk(page, 'color-report-validity', QUIET), 'an invalid picker').toBe(false);
        await expect.poll(shown, { message: 'the panel opens' }).toBe(true);
        expect(await isOpen(page, 'color-picker')).toBe(true);
        await callOk(page, 'color-hide', QUIET);
        await expect.poll(() => isOpen(page, 'color-picker'), { message: 'the hide closes the panel' }).toBe(false);

        await callOk(page, 'color-reset-validity');
        expect(await validityOf(page, 'color-picker')).toEqual({ message: '', customError: false });
        expect(await callOk(page, 'color-report-validity', QUIET), 'valid again').toBe(true);
        expect(await isOpen(page, 'color-picker'), 'the valid picker stays closed').toBe(false);
      },
    },
    {
      name: 'WaSelect takes and leaves the focus, opens and closes, and reports a custom validity',
      methods: ['WaSelect.FocusAsync', 'WaSelect.BlurAsync', 'WaSelect.ShowAsync', 'WaSelect.HideAsync',
        'WaSelect.SetCustomValidityAsync', 'WaSelect.ResetValidityAsync'],
      tags: [SELECT],
      run: async page => {
        await provePopupInput(page, 'select', 'select');

        // an open select lists its options
        await callOk(page, 'select-show', QUIET);
        await expect(page.getByTestId('select').locator('wa-option').first()).toBeVisible();
        await callOk(page, 'select-hide', QUIET);

        // WAB-64 F9 (Web Awesome side, recheck on upgrade): show() on an open select closes it; asserted as it is
        await callOk(page, 'select-show-twice', QUIET);
        expect(await isOpen(page, 'select'), 'WA 3.14.0: a second show closes the select').toBe(false);
      },
    },
    {
      name: 'WaTimeInput takes and leaves the focus, opens and closes, and reports a custom validity',
      methods: ['WaTimeInput.FocusAsync', 'WaTimeInput.BlurAsync', 'WaTimeInput.ShowAsync', 'WaTimeInput.HideAsync',
        'WaTimeInput.SetCustomValidityAsync', 'WaTimeInput.ResetValidityAsync'],
      tags: [TIME_INPUT],
      run: async page => provePopupInput(page, 'time', 'time-input'),
    },
    {
      name: 'WaDateInput takes and leaves the focus, opens and closes, reports a custom validity and clears its value',
      methods: ['WaDateInput.FocusAsync', 'WaDateInput.BlurAsync', 'WaDateInput.ShowAsync', 'WaDateInput.HideAsync',
        'WaDateInput.SetCustomValidityAsync', 'WaDateInput.ResetValidityAsync', 'WaDateInput.ClearAsync'],
      tags: [DATE_INPUT],
      pro: true,
      run: async page => {
        await provePopupInput(page, 'date-input', 'date-input');
        await proveClear(page, 'date-input', 'date-input', INITIAL_DATE);
      },
    },
    {
      name: 'WaDateRangeInput takes and leaves the focus, opens and closes, reports a custom validity and clears its value',
      methods: ['WaDateRangeInput.FocusAsync', 'WaDateRangeInput.BlurAsync', 'WaDateRangeInput.ShowAsync', 'WaDateRangeInput.HideAsync',
        'WaDateRangeInput.SetCustomValidityAsync', 'WaDateRangeInput.ResetValidityAsync', 'WaDateRangeInput.ClearAsync'],
      tags: [DATE_INPUT],
      pro: true,
      run: async page => {
        await provePopupInput(page, 'range-input', 'range-input');
        await proveClear(page, 'range-input', 'range-input', INITIAL_RANGE_START);
      },
    },
    {
      name: 'WaDatePicker takes the focus, goes to a date and to today, and clears its value',
      methods: ['WaDatePicker.FocusAsync', 'WaDatePicker.GoToDateAsync', 'WaDatePicker.GoToTodayAsync', 'WaDatePicker.ClearAsync'],
      tags: [DATE_PICKER],
      pro: true,
      run: async page => proveDatePicker(page, 'date-picker', INITIAL_DATE),
    },
    {
      name: 'WaDateRangePicker takes the focus, goes to a date and to today, and clears its value',
      methods: ['WaDateRangePicker.FocusAsync', 'WaDateRangePicker.GoToDateAsync', 'WaDateRangePicker.GoToTodayAsync',
        'WaDateRangePicker.ClearAsync'],
      tags: [DATE_PICKER],
      pro: true,
      run: async page => proveDatePicker(page, 'range-picker', INITIAL_RANGE_START),
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaSelect.FocusAsync', 'WaColorPicker.GetFormattedValueAsync'],
      route: EARLY_ROUTE,
      tags: [SELECT, COLOR_PICKER],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        await expectEarlyOutcome(page, 'early-select-focus', 'done');
        // the value comes from the first update of the element, which parses the value attribute
        await expectEarlyOutcome(page, 'early-color-format', INITIAL_RGB);
        await expect.poll(() => hasFocus(page, 'select'), { message: 'the select has the focus' }).toBe(true);
      },
    },
  ],

  knownDefects: [
    {
      // the JS method is getHexString(hue, saturation, brightness, alpha); the wrapper passes no arguments, so the
      // element parses "hsva(undefined, ...)" and returns "" for every value
      name: 'WaColorPicker.GetHexStringAsync returns the value as a hex string',
      defect: 'WAB-64 F1',
      methods: ['WaColorPicker.GetHexStringAsync'],
      exemption: 'pickers-color-hex-string',
      tags: [COLOR_PICKER],
      run: async page => {
        // the doc promises the current value (#ff0000) as hex; with or without the alpha digits
        expect(await callOk(page, 'color-hex')).toMatch(/^#ff0000(ff)?$/i);
      },
    },
    {
      name: 'WaTimeInput.ShowAsync completes when wa-show is cancelled',
      defect: 'WAB-64 F8',
      methods: ['WaTimeInput.ShowAsync'],
      tags: [TIME_INPUT],
      run: async page => {
        await page.getByTestId('time-input').evaluate(el => el.addEventListener('wa-show', event => event.preventDefault()));
        // the Promise of show() waits for wa-after-show, which a cancelled show never dispatches
        const outcome = await callMethod(page, 'time-show', { ...QUIET, timeout: SETTLE_TIMEOUT_MS });
        expect(outcome.done).toBe(true);
        expect(await isOpen(page, 'time-input'), 'the cancelled show leaves the input closed').toBe(false);
      },
    },
  ],

  external: [],
};
