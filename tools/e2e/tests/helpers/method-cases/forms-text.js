// @ts-check
const { expect } = require('../test');
const { callMethod, callOk } = require('../method-harness');
const { WA_READY_TIMEOUT_MS } = require('../wa-ready');

// Method cases of the group "forms-text" (tools\e2e\data\method-groups.json): WaInput, WaTextArea, WaNumberInput,
// WaOtpInput, WaTagInput and WaKnownDate on /testing/methods-forms-text (MethodsFormsTextHarness.razor). Every call
// runs from .NET through the wrapper's @ref (a MethodCall button); the case asserts the effect on the element, and the
// returned value where there is one. All elements are free components.

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('../method-harness').MethodGroupModule} MethodGroupModule */

const INPUT = 'wa-input';
const TEXTAREA = 'wa-textarea';
const NUMBER_INPUT = 'wa-number-input';
const OTP_INPUT = 'wa-otp-input';
const TAG_INPUT = 'wa-tag-input';
const KNOWN_DATE = 'wa-known-date';

// the custom validity message of the harness
const VALIDITY_MESSAGE = 'Check this field, please';

// the texts of the harness: the initial text, the replacement of the selected first word, and the text after the
// replacement of the second word
const INITIAL_TEXT = 'Hello world';
const AFTER_SELECTION_REPLACED = 'Howdy world';
const AFTER_RANGE_REPLACED = 'Howdy there';

// the selection the harness sets (the first word) and the range it replaces (the second word)
const FIRST_WORD = { start: 0, end: 5 };
const SECOND_WORD = { start: 6, end: 11 };

// the number inputs start at 4 and step by 2
const INITIAL_NUMBER = '4';
const NUMBER_AFTER_STEP_UP = '6';

// the code of the OTP input
const INITIAL_OTP = '1234';

// the vertical scroll position the harness sets on the long textarea
const SCROLL_TOP = 40;

// calls that must not move the focus or close a popup (see callMethod)
const QUIET = { quiet: true };

// the delay of the element modules in the first-render case, so the first render certainly comes before the definitions
// (long enough for the circuit of a prerendered page to start first)
const MODULE_DELAY_MS = 4_000;

// the harness with the calls on the first render
const EARLY_ROUTE = '/testing/methods-forms-text?early=true';

// how long a call without a user activation may take: the page waits until the transient activation of the last
// Playwright call has expired (5 s in Chromium) before it clicks
const NO_ACTIVATION_CALL_TIMEOUT_MS = 20_000;

/**
 * Runs a MethodCall without a user activation and returns its outcome text. Every Playwright evaluation (also
 * dispatchEvent and the polling of expect) runs with a user gesture and gives the page a transient user activation,
 * so a click event from the test always has one. Here the page clicks the button itself once the activation has
 * expired, and the test waits with one in-page poll (waitForFunction), which does not renew it.
 *
 * @param {Page} page
 * @param {string} id test id of the MethodCall
 */
async function callWithoutUserActivation(page, id) {
  const button = page.getByTestId(id);
  const output = page.getByTestId(`${id}-result`);
  await expect(button).toBeEnabled();
  const expectedCalls = String(Number(await output.getAttribute('data-calls')) + 1);
  await button.evaluate(el => {
    const clickWhenInactive = () => {
      if (navigator.userActivation.isActive) setTimeout(clickWhenInactive, 100);
      else /** @type {HTMLElement} */ (el).click();
    };
    setTimeout(clickWhenInactive, 0);
  });
  await page.waitForFunction(([resultId, calls]) =>
    document.querySelector(`[data-testid="${resultId}"]`)?.getAttribute('data-calls') === calls,
  [`${id}-result`, expectedCalls], { timeout: NO_ACTIVATION_CALL_TIMEOUT_MS });
  return (await output.textContent()) ?? '';
}

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
 * The value property of a Web Awesome element, as text.
 *
 * @param {Page} page
 * @param {string} testId
 */
function valueOf(page, testId) {
  return page.getByTestId(testId).evaluate(el => String(/** @type {any} */ (el).value ?? ''));
}

/**
 * The value and the text selection of the native text control inside a Web Awesome element's shadow root.
 *
 * @param {Page} page
 * @param {string} testId
 * @param {string} selector the native control in the shadow root
 */
function nativeSelection(page, testId, selector) {
  return page.getByTestId(testId).evaluate((el, sel) => {
    const control = /** @type {HTMLInputElement | HTMLTextAreaElement | null} */ (el.shadowRoot?.querySelector(sel) ?? null);
    if (!control) return null;
    return { value: control.value, start: control.selectionStart, end: control.selectionEnd, direction: control.selectionDirection };
  }, selector);
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
 * Delays the element modules of the first-render calls, so they certainly come before the definitions.
 *
 * @param {Page} page
 */
async function delayModules(page) {
  for (const module of ['input', 'number-input']) {
    await page.route(`**/components/${module}/${module}.js`, async route => {
      await new Promise(resolve => setTimeout(resolve, MODULE_DELAY_MS));
      await route.continue();
    });
  }
}

/**
 * Selects the text, sets a selection range and replaces text of a text control, through its wrapper; the replaced
 * text also reaches the bound .NET value (SetRangeTextAsync reads the value back).
 *
 * @param {Page} page
 * @param {string} prefix the test id prefix of the element and its calls
 * @param {string} selector the native control in the element's shadow root
 */
async function selectAndReplaceText(page, prefix, selector) {
  const selection = () => nativeSelection(page, prefix, selector);
  await expect.poll(async () => (await selection())?.value, { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_TEXT);

  await callOk(page, `${prefix}-select`, QUIET);
  expect(await selection()).toMatchObject({ start: 0, end: INITIAL_TEXT.length });

  await callOk(page, `${prefix}-set-selection`, QUIET);
  expect(await selection()).toMatchObject({ start: FIRST_WORD.start, end: FIRST_WORD.end, direction: 'backward' });

  // the one-argument form replaces the current selection (the first word)
  await callOk(page, `${prefix}-replace-selection`, QUIET);
  expect(await valueOf(page, prefix)).toBe(AFTER_SELECTION_REPLACED);
  await expect(page.getByTestId(`${prefix}-bound-value`)).toHaveText(AFTER_SELECTION_REPLACED);

  // the four-argument form replaces the second word and selects the new text
  await callOk(page, `${prefix}-set-range`, QUIET);
  expect(await valueOf(page, prefix)).toBe(AFTER_RANGE_REPLACED);
  expect(await selection()).toMatchObject({ start: SECOND_WORD.start, end: SECOND_WORD.end });
  await expect(page.getByTestId(`${prefix}-bound-value`)).toHaveText(AFTER_RANGE_REPLACED);
}

/**
 * Focuses and blurs an element through its wrapper.
 *
 * @param {Page} page
 * @param {string} testId test id of the element
 * @param {string} prefix test id prefix of its calls
 */
async function focusAndBlur(page, testId, prefix) {
  await callOk(page, `${prefix}-focus`, QUIET);
  await expect.poll(() => hasFocus(page, testId), { message: `${testId} has the focus` }).toBe(true);
  await callOk(page, `${prefix}-blur`, QUIET);
  await expect.poll(() => hasFocus(page, testId), { message: `${testId} lost the focus` }).toBe(false);
}

/** @type {MethodGroupModule} */
module.exports = {
  group: 'forms-text',

  cases: [
    {
      name: 'WaInput takes and leaves the focus',
      methods: ['WaInput.FocusAsync', 'WaInput.BlurAsync'],
      tags: [INPUT],
      run: async page => focusAndBlur(page, 'input', 'input'),
    },
    {
      name: 'WaInput selects its text, sets the selection range and replaces text',
      methods: ['WaInput.SelectAsync', 'WaInput.SetSelectionRangeAsync', 'WaInput.SetRangeTextAsync'],
      tags: [INPUT],
      run: async page => selectAndReplaceText(page, 'input', 'input'),
    },
    {
      name: 'WaInput steps a number input up and down',
      methods: ['WaInput.StepUpAsync', 'WaInput.StepDownAsync'],
      tags: [INPUT],
      run: async page => {
        await expect.poll(() => valueOf(page, 'input-number'), { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_NUMBER);

        // the step is 2
        await callOk(page, 'input-step-up');
        await expect.poll(() => valueOf(page, 'input-number')).toBe(NUMBER_AFTER_STEP_UP);
        await callOk(page, 'input-step-down');
        await expect.poll(() => valueOf(page, 'input-number')).toBe(INITIAL_NUMBER);

        // WAB-64 F12 (native precondition, undocumented): a text input is not steppable; the native error reaches .NET
        const outcome = await callMethod(page, 'input-text-step-up');
        expect(outcome.error, 'StepUpAsync on a text input fails').toContain('stepUp');
        expect(await valueOf(page, 'input')).toBe(INITIAL_TEXT);
      },
    },
    {
      name: 'WaInput.ShowPickerAsync opens the picker from a real click, and needs the user gesture',
      methods: ['WaInput.ShowPickerAsync'],
      tags: [INPUT],
      run: async page => {
        // WAB-64 F11: without a user activation the native showPicker() refuses; the refusal proves that the call
        // reaches the native picker of the date input
        const refused = await callWithoutUserActivation(page, 'input-show-picker');
        expect(refused, 'showPicker() without a user gesture fails').toMatch(/^error: .*showPicker.*user (gesture|activation)/i);

        // a real click carries the user activation to the call, also over a Blazor Server round trip
        await callOk(page, 'input-show-picker');
      },
    },
    {
      name: 'WaTextArea takes and leaves the focus, selects its text, sets the selection range and replaces text',
      methods: ['WaTextArea.FocusAsync', 'WaTextArea.BlurAsync', 'WaTextArea.SelectAsync', 'WaTextArea.SetSelectionRangeAsync',
        'WaTextArea.SetRangeTextAsync'],
      tags: [TEXTAREA],
      run: async page => {
        await focusAndBlur(page, 'textarea', 'textarea');
        await selectAndReplaceText(page, 'textarea', 'textarea.control');
      },
    },
    {
      name: 'WaTextArea.ScrollPositionAsync scrolls the textarea and returns its scroll position',
      methods: ['WaTextArea.ScrollPositionAsync'],
      tags: [TEXTAREA],
      run: async page => {
        const scrollTop = () => page.getByTestId('textarea-scroll').evaluate(el => el.shadowRoot?.querySelector('textarea.control')?.scrollTop ?? -1);
        await expect.poll(async () => (await page.getByTestId('textarea-scroll').evaluate(el =>
          el.shadowRoot?.querySelector('textarea.control')?.scrollHeight ?? 0)), { message: 'the long text overflows', timeout: WA_READY_TIMEOUT_MS })
          .toBeGreaterThan(SCROLL_TOP * 2);

        expect(await callOk(page, 'textarea-scroll-get')).toEqual({ top: 0, left: 0 });

        await callOk(page, 'textarea-scroll-set', QUIET);
        await expect.poll(scrollTop, { message: 'the textarea scrolled down' }).toBe(SCROLL_TOP);

        // WAB-64 F10 (Web Awesome side, recheck on upgrade): scrollPosition() returns scrollTop as left too; the
        // textarea wraps its lines, so its real horizontal scroll position stays 0; asserted as it is
        expect(await callOk(page, 'textarea-scroll-get'), 'WA 3.14.0: left repeats top').toEqual({ top: SCROLL_TOP, left: SCROLL_TOP });
      },
    },
    {
      name: 'WaNumberInput steps up and down, takes and leaves the focus and selects its value',
      methods: ['WaNumberInput.StepUpAsync', 'WaNumberInput.StepDownAsync', 'WaNumberInput.FocusAsync', 'WaNumberInput.BlurAsync',
        'WaNumberInput.SelectAsync'],
      tags: [NUMBER_INPUT],
      run: async page => {
        await expect.poll(() => valueOf(page, 'number-input'), { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_NUMBER);

        // the step is 2
        await callOk(page, 'number-step-up');
        await expect.poll(() => valueOf(page, 'number-input')).toBe(NUMBER_AFTER_STEP_UP);
        await callOk(page, 'number-step-down');
        await expect.poll(() => valueOf(page, 'number-input')).toBe(INITIAL_NUMBER);

        await callOk(page, 'number-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'number-input')).toBe(true);

        // a native number input has no readable selection range; a key typed over the selected value replaces all of it
        await callOk(page, 'number-select', QUIET);
        await page.keyboard.type('9');
        await expect.poll(() => valueOf(page, 'number-input'), { message: 'the typed digit replaced the selected value' }).toBe('9');

        await callOk(page, 'number-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'number-input')).toBe(false);
      },
    },
    {
      name: 'WaOtpInput selects its code, takes and leaves the focus, and clears its code',
      methods: ['WaOtpInput.SelectAsync', 'WaOtpInput.FocusAsync', 'WaOtpInput.BlurAsync', 'WaOtpInput.ClearAsync'],
      tags: [OTP_INPUT],
      run: async page => {
        const selection = () => nativeSelection(page, 'otp-input', '.hidden-input');
        await expect.poll(async () => (await selection())?.value, { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_OTP);

        await focusAndBlur(page, 'otp-input', 'otp');

        // Web Awesome side, recheck on upgrade: on a field without the focus, the native select() focuses the hidden
        // input, and the element's own focus handling then puts the cursor on the last character (WA 3.14.0);
        // asserted as it is
        await callOk(page, 'otp-select', QUIET);
        await expect.poll(selection, { message: 'WA 3.14.0: the last character is selected' })
          .toMatchObject({ start: INITIAL_OTP.length - 1, end: INITIAL_OTP.length });

        // on the field with the focus, select() selects the whole code
        await callOk(page, 'otp-select', QUIET);
        await expect.poll(selection).toMatchObject({ start: 0, end: INITIAL_OTP.length });

        // clear() empties the code, dispatches wa-clear and returns the focus to the field
        await callOk(page, 'otp-clear', QUIET);
        expect(await valueOf(page, 'otp-input')).toBe('');
        await expect(page.getByTestId('otp-clear-count')).toHaveText('1');
        await expect.poll(() => hasFocus(page, 'otp-input'), { message: 'the cleared field has the focus' }).toBe(true);
      },
    },
    {
      name: 'WaTagInput takes and leaves the focus',
      methods: ['WaTagInput.FocusAsync', 'WaTagInput.BlurAsync'],
      tags: [TAG_INPUT],
      run: async page => focusAndBlur(page, 'tag-input', 'tag'),
    },
    {
      name: 'WaKnownDate takes and leaves the focus',
      methods: ['WaKnownDate.FocusAsync', 'WaKnownDate.BlurAsync'],
      tags: [KNOWN_DATE],
      run: async page => {
        await callOk(page, 'known-date-focus', QUIET);
        await expect.poll(() => hasFocus(page, 'known-date')).toBe(true);
        // focus() goes to the first empty field (all are empty: the first field)
        expect(await page.getByTestId('known-date').evaluate(el => {
          const active = el.shadowRoot?.activeElement;
          return !!active && active === el.shadowRoot?.querySelector('input[part~="field-input"]');
        }), 'the first field has the focus').toBe(true);
        await callOk(page, 'known-date-blur', QUIET);
        await expect.poll(() => hasFocus(page, 'known-date')).toBe(false);
      },
    },
    {
      name: 'every text control reports and resets a custom validity',
      methods: ['WaInput.SetCustomValidityAsync', 'WaInput.ResetValidityAsync', 'WaTextArea.SetCustomValidityAsync',
        'WaTextArea.ResetValidityAsync', 'WaNumberInput.SetCustomValidityAsync', 'WaNumberInput.ResetValidityAsync',
        'WaOtpInput.SetCustomValidityAsync', 'WaOtpInput.ResetValidityAsync', 'WaTagInput.SetCustomValidityAsync',
        'WaTagInput.ResetValidityAsync', 'WaKnownDate.SetCustomValidityAsync', 'WaKnownDate.ResetValidityAsync'],
      tags: [INPUT, TEXTAREA, NUMBER_INPUT, OTP_INPUT, TAG_INPUT, KNOWN_DATE],
      run: async page => {
        // [test id of the element, test id prefix of its calls]
        for (const [testId, prefix] of [['input', 'input'], ['textarea', 'textarea'], ['number-input', 'number'],
          ['otp-input', 'otp'], ['tag-input', 'tag'], ['known-date', 'known-date']]) {
          await callOk(page, `${prefix}-set-validity`);
          expect(await validityOf(page, testId), `${testId} reports the custom error`).toEqual({ message: VALIDITY_MESSAGE, customError: true });
          await callOk(page, `${prefix}-reset-validity`);
          expect(await validityOf(page, testId), `${testId} is valid again`).toEqual({ message: '', customError: false });
        }
      },
    },
    {
      name: 'timing: calls on the first render wait until the elements are defined',
      methods: ['WaInput.FocusAsync', 'WaNumberInput.StepUpAsync'],
      route: EARLY_ROUTE,
      tags: [INPUT, NUMBER_INPUT],
      noReadyWait: true,
      setup: delayModules,
      run: async page => {
        for (const id of ['early-input-focus', 'early-number-step-up']) await expectEarlyOutcome(page, id);
        await expect.poll(() => hasFocus(page, 'input'), { message: 'the input has the focus' }).toBe(true);
        await expect.poll(() => valueOf(page, 'number-input'), { message: 'the number input stepped up' }).toBe(NUMBER_AFTER_STEP_UP);
      },
    },
  ],

  knownDefects: [
    {
      // the wrapper documents "the resulting scroll position" also for a call that sets it; Web Awesome's
      // scrollPosition(position) returns undefined, so the wrapper returns null
      name: 'WaTextArea.ScrollPositionAsync returns the resulting position when it scrolls',
      defect: 'WAB-65 L2',
      methods: ['WaTextArea.ScrollPositionAsync'],
      tags: [TEXTAREA],
      run: async page => {
        await expect.poll(async () => (await page.getByTestId('textarea-scroll').evaluate(el =>
          el.shadowRoot?.querySelector('textarea.control')?.scrollHeight ?? 0)), { message: 'the long text overflows', timeout: WA_READY_TIMEOUT_MS })
          .toBeGreaterThan(SCROLL_TOP * 2);

        const result = await callOk(page, 'textarea-scroll-set', QUIET);
        expect(result, 'the set call returns the resulting position').not.toBeNull();
        expect(result?.top).toBe(SCROLL_TOP);
      },
    },
    {
      // stepUp() and stepDown() change the element value without an input event, and the wrapper does not read the
      // value back (as SetRangeTextAsync does), so the bound .NET value keeps the old number
      name: 'WaInput.StepUpAsync and StepDownAsync update the bound value',
      defect: 'WAB-65 L3',
      methods: ['WaInput.StepUpAsync', 'WaInput.StepDownAsync'],
      tags: [INPUT],
      run: async page => {
        const bound = page.getByTestId('input-number-bound-value');
        await expect.poll(() => valueOf(page, 'input-number'), { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_NUMBER);
        await expect(bound).toHaveText(INITIAL_NUMBER);

        await callOk(page, 'input-step-up');
        await expect.poll(() => valueOf(page, 'input-number')).toBe(NUMBER_AFTER_STEP_UP);
        await expect(bound, 'the bound value follows StepUpAsync').toHaveText(NUMBER_AFTER_STEP_UP);
        await callOk(page, 'input-step-down');
        await expect(bound, 'the bound value follows StepDownAsync').toHaveText(INITIAL_NUMBER);
      },
    },
    {
      // as for WaInput: the bound .NET value keeps the old number
      name: 'WaNumberInput.StepUpAsync and StepDownAsync update the bound value',
      defect: 'WAB-65 L3',
      methods: ['WaNumberInput.StepUpAsync', 'WaNumberInput.StepDownAsync'],
      tags: [NUMBER_INPUT],
      run: async page => {
        const bound = page.getByTestId('number-bound-value');
        await expect.poll(() => valueOf(page, 'number-input'), { timeout: WA_READY_TIMEOUT_MS }).toBe(INITIAL_NUMBER);
        await expect(bound).toHaveText(INITIAL_NUMBER);

        await callOk(page, 'number-step-up');
        await expect.poll(() => valueOf(page, 'number-input')).toBe(NUMBER_AFTER_STEP_UP);
        await expect(bound, 'the bound value follows StepUpAsync').toHaveText(NUMBER_AFTER_STEP_UP);
        await callOk(page, 'number-step-down');
        await expect(bound, 'the bound value follows StepDownAsync').toHaveText(INITIAL_NUMBER);
      },
    },
  ],

  external: [],
};
