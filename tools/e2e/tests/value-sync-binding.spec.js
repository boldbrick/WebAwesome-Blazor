// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');

// Browser acceptance for GitHub issue #1 (Web Awesome 3.12.0 upgrade). In WA 3 the value/checked
// attribute maps to defaultValue/defaultChecked: once the user has interacted, the element
// ignores the attribute, and Blazor only calls setAttribute on custom elements (it assigns the
// value/checked property on native INPUT/SELECT/TEXTAREA only). So after a user edit, a C#-side
// model change never reaches the element. Each value-sync test therefore (a) lets the user edit
// the control and checks the model followed, (b) changes the model from C# through a button and
// (c) checks that the element's live property followed the model - step (c) is the one the bug
// breaks. The driving controls live on the Input/Textarea/Checkbox demo pages ("Programmatic
// Value / Reset") and on the e2e-only harness /testing/value-sync for the other nine wrappers.
// Also covered: SetRangeTextAsync feeding the bound model, opt-in Immediate binding updating the
// model before blur, and WaRelativeTime format/numeric reaching Intl.RelativeTimeFormat.

const HARNESS_ROUTE = '/testing/value-sync';

/**
 * Types into the shadow-DOM text control (Playwright CSS pierces open shadow roots) and tabs out,
 * so the native change event fires and the element relays it.
 *
 * @param {import('@playwright/test').Locator} element
 * @param {string} selector
 * @param {string} text
 */
async function typeAndBlur(element, selector, text) {
  const inner = element.locator(selector);
  await inner.fill(text);
  await inner.press('Tab');
}

/**
 * One row per affected wrapper. `action` is the C# button suffix (set/reset), `expected` is the
 * live property value in the type the element actually returns (string, number or boolean).
 */
const VALUE_SYNC_CASES = [
  {
    wrapper: 'WaInput', route: '/components/input', tag: 'wa-input', id: 'input-sync', property: 'value',
    initialModel: 'Hello world',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => typeAndBlur(el, 'input', 'Typed by user'),
    userModel: 'Typed by user',
    action: 'set', expected: 'Set from C#', expectedModel: 'Set from C#',
  },
  {
    wrapper: 'WaTextArea', route: '/components/textarea', tag: 'wa-textarea', id: 'textarea-sync', property: 'value',
    initialModel: 'Hello world',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => typeAndBlur(el, 'textarea', 'Typed by user'),
    userModel: 'Typed by user',
    action: 'set', expected: 'Set from C#', expectedModel: 'Set from C#',
  },
  {
    wrapper: 'WaCheckbox', route: '/components/checkbox', tag: 'wa-checkbox', id: 'checkbox-sync', property: 'checked',
    initialModel: 'False',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[part~="control"]').click(),
    userModel: 'True',
    action: 'reset', expected: false, expectedModel: 'False',
  },
  {
    wrapper: 'WaNumberInput', route: HARNESS_ROUTE, tag: 'wa-number-input', id: 'number-input', property: 'value',
    initialModel: '10',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => typeAndBlur(el, 'input', '7'),
    userModel: '7',
    action: 'set', expected: '42', expectedModel: '42',
  },
  {
    wrapper: 'WaColorPicker', route: HARNESS_ROUTE, tag: 'wa-color-picker', id: 'color-picker', property: 'value',
    initialModel: '#336699',
    // opens the popup from its trigger, types a colour into the popup's text field and commits it with Enter
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="trigger"]').click();
      const field = el.locator('[part~="input"] input');
      await field.fill('#ff0000');
      await field.press('Enter');
      await el.page().keyboard.press('Escape');
    },
    userModel: '#ff0000',
    action: 'set', expected: '#00aa55', expectedModel: '#00aa55',
  },
  {
    wrapper: 'WaDateInput', route: HARNESS_ROUTE, tag: 'wa-date-input', id: 'date-input', property: 'value', pro: true,
    initialModel: '2024-01-15',
    // ArrowUp on the day segment commits the next day, like a keyboard user would
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[data-segment="day"]').press('ArrowUp'),
    userModel: '2024-01-16',
    action: 'set', expected: '2025-06-30', expectedModel: '2025-06-30',
  },
  {
    wrapper: 'WaKnownDate', route: HARNESS_ROUTE, tag: 'wa-known-date', id: 'known-date', property: 'value',
    initialModel: '2000-01-15',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => typeAndBlur(el, 'input[data-field="day"]', '20'),
    userModel: '2000-01-20',
    action: 'set', expected: '1999-12-31', expectedModel: '1999-12-31',
  },
  {
    wrapper: 'WaOtpInput', route: HARNESS_ROUTE, tag: 'wa-otp-input', id: 'otp-input', property: 'value',
    initialModel: '123456',
    // the segments are painted over one hidden text input: clicking a segment focuses it and selects
    // that segment's character, so typing overwrites segment by segment; change fires on blur
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('.segment').first().click();
      await el.page().keyboard.type('654321');
      await el.page().keyboard.press('Tab');
    },
    userModel: '654321',
    action: 'set', expected: '111222', expectedModel: '111222',
  },
  {
    wrapper: 'WaRadioGroup', route: HARNESS_ROUTE, tag: 'wa-radio-group', id: 'radio-group', property: 'value',
    initialModel: 'alpha',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('wa-radio', { hasText: 'Beta' }).click(),
    userModel: 'beta',
    action: 'set', expected: 'gamma', expectedModel: 'gamma',
  },
  {
    wrapper: 'WaSlider', route: HARNESS_ROUTE, tag: 'wa-slider', id: 'slider', property: 'value',
    initialModel: '50',
    // wa-slider's change event carries a number (its live value type), which Blazor's ChangeEventArgs
    // cannot carry (strings/booleans only), so step (a) also guards the wrapper's change handling
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[role="slider"]').press('ArrowRight'),
    userModel: '51',
    action: 'set', expected: 80, expectedModel: '80',
  },
  {
    wrapper: 'WaTimeInput', route: HARNESS_ROUTE, tag: 'wa-time-input', id: 'time-input', property: 'value',
    initialModel: '09:30',
    // ArrowUp on the minute segment commits the next minute, like a keyboard user would
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[data-segment="minute"]').press('ArrowUp'),
    userModel: '09:31',
    action: 'set', expected: '14:45', expectedModel: '14:45',
  },
  {
    wrapper: 'WaSwitch', route: HARNESS_ROUTE, tag: 'wa-switch', id: 'switch', property: 'checked',
    initialModel: 'False',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[part~="control"]').click(),
    userModel: 'True',
    action: 'reset', expected: false, expectedModel: 'False',
  },
  // the set direction for the toggles: after the user checks and unchecks, C# checks them
  {
    wrapper: 'WaCheckbox', variant: 'set after unchecking', route: '/components/checkbox', tag: 'wa-checkbox', id: 'checkbox-sync', property: 'checked',
    initialModel: 'False',
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="control"]').click();
      await expect(el.page().getByTestId('checkbox-sync-model')).toHaveText('True');
      await el.locator('[part~="control"]').click();
    },
    userModel: 'False',
    action: 'set', expected: true, expectedModel: 'True',
  },
  {
    wrapper: 'WaSwitch', variant: 'set after unchecking', route: HARNESS_ROUTE, tag: 'wa-switch', id: 'switch', property: 'checked',
    initialModel: 'False',
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="control"]').click();
      await expect(el.page().getByTestId('switch-model')).toHaveText('True');
      await el.locator('[part~="control"]').click();
    },
    userModel: 'False',
    action: 'set', expected: true, expectedModel: 'True',
  },
  // the upgrade plan judged these unaffected: their value attribute feeds the live value, not a default*
  // field; the rows check that claim after a real user edit
  {
    wrapper: 'WaSelect', route: HARNESS_ROUTE, tag: 'wa-select', id: 'select', property: 'value',
    initialModel: 'apple',
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="combobox"]').click();
      await el.locator('wa-option', { hasText: 'Banana' }).click();
    },
    userModel: 'banana',
    action: 'set', expected: 'cherry', expectedModel: 'cherry',
  },
  {
    wrapper: 'WaCombobox', route: HARNESS_ROUTE, tag: 'wa-combobox', id: 'combobox', property: 'value', pro: true,
    initialModel: 'apple',
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="combobox-input"]').click();
      await el.locator('wa-option', { hasText: 'Banana' }).click();
    },
    userModel: 'banana',
    action: 'set', expected: 'cherry', expectedModel: 'cherry',
  },
  // multiple selection keeps an array in the live value property; the initial selection must reach it too,
  // since the wrappers render no value attribute in this mode
  {
    wrapper: 'WaSelect', variant: 'multiple', route: HARNESS_ROUTE, tag: 'wa-select', id: 'select-multiple', property: 'value',
    initialModel: 'cheese', initialProperty: ['cheese'],
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="combobox"]').click();
      await el.locator('wa-option', { hasText: 'Olives' }).click();
      await el.page().keyboard.press('Escape');
    },
    userModel: 'cheese,olives',
    action: 'set', expected: ['ham'], expectedModel: 'ham',
  },
  {
    wrapper: 'WaCombobox', variant: 'multiple', route: HARNESS_ROUTE, tag: 'wa-combobox', id: 'combobox-multiple', property: 'value', pro: true,
    initialModel: 'cheese', initialProperty: ['cheese'],
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('[part~="combobox-input"]').click();
      await el.locator('wa-option', { hasText: 'Olives' }).click();
      await el.page().keyboard.press('Escape');
    },
    userModel: 'cheese,olives',
    action: 'set', expected: ['ham'], expectedModel: 'ham',
  },
  {
    wrapper: 'WaDateRangeInput', route: HARNESS_ROUTE, tag: 'wa-date-input', id: 'date-range-input', property: 'value', pro: true,
    initialModel: '2024-01-10 to 2024-01-12',
    // ArrowUp on the start date's day segment commits the next start day, like a keyboard user would
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.locator('[data-segment="day"][data-group="from"]').press('ArrowUp'),
    userModel: '2024-01-11 to 2024-01-12',
    action: 'set', expected: '2025-06-01/2025-06-07', expectedModel: '2025-06-01 to 2025-06-07',
  },
  {
    wrapper: 'WaDateRangePicker', route: HARNESS_ROUTE, tag: 'wa-date-picker', id: 'date-range-picker', property: 'value', pro: true,
    initialModel: '2024-03-11 to 2024-03-13',
    // the range commits (change) on the second click
    userEdit: async (/** @type {import('@playwright/test').Locator} */ el) => {
      await el.locator('button[data-date="2024-03-18"]').click();
      await el.locator('button[data-date="2024-03-20"]').click();
    },
    userModel: '2024-03-18 to 2024-03-20',
    action: 'set', expected: '2024-03-25/2024-03-28', expectedModel: '2024-03-25 to 2024-03-28',
  },
  {
    wrapper: 'WaDatePicker', route: HARNESS_ROUTE, tag: 'wa-date-picker', id: 'date-picker', property: 'value', pro: true,
    initialModel: '2024-03-15',
    userEdit: (/** @type {import('@playwright/test').Locator} */ el) => el.getByRole('button', { name: 'Wednesday, March 20, 2024' }).click(),
    userModel: '2024-03-20',
    action: 'set', expected: '2024-03-25', expectedModel: '2024-03-25',
  },
];

/**
 * Opens a demo route and waits until Blazor is interactive and the given elements have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string[]} tags
 */
async function open(page, route, tags) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, tags);
}

for (const c of VALUE_SYNC_CASES) {
  test(`value sync: ${c.wrapper}${c.variant ? ` (${c.variant})` : ''} reflects C# model change after user edit`, async ({ page }) => {
    await open(page, c.route, c.pro ? [] : [c.tag]);
    if (c.pro) await skipUnlessProUpgrades(page, [c.tag]);

    const element = page.getByTestId(c.id);
    const model = page.getByTestId(`${c.id}-model`);
    await element.evaluate(el => /** @type {any} */ (el).updateComplete);
    await expect(model).toHaveText(c.initialModel);
    if (c.initialProperty !== undefined) {
      await expect(element, `live "${c.property}" property holds the initial model`).toHaveJSProperty(c.property, c.initialProperty);
    }

    // (a) the user edits the control; UI -> model already works before the fix
    await c.userEdit(element);
    await expect(model, 'user edit reaches the bound model').toHaveText(c.userModel);

    // (b) C# changes the model
    await page.getByTestId(`${c.id}-${c.action}`).click();
    await expect(model, 'C# change is rendered in the model display').toHaveText(c.expectedModel);

    // (c) the element's live property follows the model (the bug: it keeps the user-edited value)
    await expect(element, `live "${c.property}" property follows the C# model`).toHaveJSProperty(c.property, c.expected);
  });
}

const RANGE_TEXT_CASES = [
  { wrapper: 'WaInput', route: '/components/input', tag: 'wa-input', id: 'input-range' },
  { wrapper: 'WaTextArea', route: '/components/textarea', tag: 'wa-textarea', id: 'textarea-range' },
];

for (const c of RANGE_TEXT_CASES) {
  test(`set range text: ${c.wrapper}.SetRangeTextAsync updates the bound model`, async ({ page }) => {
    await open(page, c.route, [c.tag]);

    const element = page.getByTestId(c.id);
    const model = page.getByTestId(`${c.id}-model`);
    await expect(model).toHaveText('Hello world');

    await page.getByTestId(`${c.id}-apply`).click();

    // the element took the replacement, which proves the JS call itself ran
    await expect(element, 'element value after setRangeText').toHaveJSProperty('value', 'Hello Blazor');
    // WA's setRangeText dispatches no input/change event, so the wrapper must read the value back
    await expect(model, 'bound model after SetRangeTextAsync').toHaveText('Hello Blazor');
  });
}

const IMMEDIATE_CASES = [
  { wrapper: 'WaInput', route: '/components/input', tag: 'wa-input', id: 'input-immediate', control: 'input' },
  { wrapper: 'WaTextArea', route: '/components/textarea', tag: 'wa-textarea', id: 'textarea-immediate', control: 'textarea' },
];

for (const c of IMMEDIATE_CASES) {
  test(`immediate: ${c.wrapper} with Immediate updates the model while typing, before blur`, async ({ page }) => {
    await open(page, c.route, [c.tag]);

    const element = page.getByTestId(c.id);
    const model = page.getByTestId(`${c.id}-model`);
    const hasFocus = () => element.evaluate(el => el.matches(':focus-within'));
    await expect(model).toHaveText('');

    const inner = element.locator(c.control);
    await inner.click();
    await inner.pressSequentially('abc');
    expect(await hasFocus(), 'control still focused while typing').toBe(true);

    await expect(model, 'model updated from input events, without blur').toHaveText('abc');
    expect(await hasFocus(), 'no blur happened before the model updated').toBe(true);
  });
}

// expected text is computed in the browser, so it matches the ICU data the element formats with
const RELATIVE_TIME_CASES = [
  { id: 'rt-always', description: 'numeric "always" (long)', options: { numeric: 'always', style: 'long' }, value: -1, unit: 'day' },
  { id: 'rt-short', description: 'format "short"', options: { numeric: 'auto', style: 'short' }, value: -3, unit: 'hour' },
  { id: 'rt-narrow', description: 'format "narrow"', options: { numeric: 'auto', style: 'narrow' }, value: -3, unit: 'hour' },
];

for (const c of RELATIVE_TIME_CASES) {
  test(`relative time: ${c.id} renders ${c.description}`, async ({ page }) => {
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    await open(page, HARNESS_ROUTE, ['wa-relative-time']);

    const [expected, defaultStyle] = await page.evaluate(({ options, value, unit }) => [
      new Intl.RelativeTimeFormat('en-US', /** @type {Intl.RelativeTimeFormatOptions} */ (options)).format(value, /** @type {Intl.RelativeTimeFormatUnit} */ (unit)),
      new Intl.RelativeTimeFormat('en-US', { numeric: 'auto', style: 'long' }).format(value, /** @type {Intl.RelativeTimeFormatUnit} */ (unit)),
    ], c);
    // the requested style must render differently from the element's default (long, auto),
    // otherwise the assertion below could pass without the option ever reaching the element
    expect(expected, 'requested style differs from the default rendering').not.toBe(defaultStyle);

    const element = page.getByTestId(c.id).locator('wa-relative-time');
    await expect
      .poll(() => element.evaluate(el => (el.shadowRoot?.textContent ?? '').trim()), { message: `rendered text of ${c.id}` })
      .toBe(expected);
    // an invalid format or numeric value makes Intl.RelativeTimeFormat throw a RangeError on the page
    expect(pageErrors, 'no page errors').toEqual([]);
  });
}
