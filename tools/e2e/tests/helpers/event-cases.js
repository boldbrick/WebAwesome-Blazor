// @ts-check
const { expect } = require('@playwright/test');
const { expectFired, expectFiredTimes, payloadOf } = require('./event-log');

// The dispatch cases of event-dispatch.spec.js: each drives a component with real input (mouse, keyboard,
// pointer) and lists the EventCallbacks it proves. After a case has run, the spec checks that every listed
// callback reached .NET (the harness pages' event log), so listing a callback here is the proof that Web
// Awesome dispatches its event on the host and that the wrapper's binding and the JS initializer's
// registration deliver it. event-coverage.spec.js checks these lists, together with the payload cases and
// EXTERNAL_COVERAGE, against tools\e2e\data\event-callbacks.json (the rendered bindings of every wrapper).
//
// Callbacks under `proven` are asserted by the case itself: a bound model display or a regular demo page counter.

const FORMS = '/testing/events-forms';
const OVERLAYS = '/testing/events-overlays';
const CONTENT = '/testing/events-content';
const PRO = '/testing/events-pro';

/** @typedef {import('@playwright/test').Page} Page */
/** @typedef {import('@playwright/test').Locator} Locator */

/**
 * @typedef {object} EventCase
 * @property {string} name test title suffix
 * @property {string} route page to open
 * @property {string[]} tags custom elements to wait for before interacting
 * @property {string[]} callbacks "Wrapper.Callback" ids the case proves
 * @property {string[]} [proven] further ids the run function proves itself, from a model display or a demo page
 *   counter rather than the event log (two-way binding callbacks, the demo pages' counters)
 * @property {boolean} [clipboard] grant clipboard permissions first
 * @property {boolean} [pro] the tags are Pro components: the case skips visibly unless they upgrade
 * @property {(page: Page) => Promise<void>} run the interaction (and any case-specific assertions)
 */

/** Keyboard callbacks every WaInputBase control inherits (see FormEventsHarness.Common). */
const KEYBOARD = ['OnKeyDown', 'OnKeyPress', 'OnKeyUp'];

/**
 * Focus callbacks, proven only where the host element itself takes focus. Where focus lands on an element in
 * the shadow root (the text controls, toggles, pickers, WaButton), Blazor never delivers focus/blur: it
 * dispatches a non-bubbling event only to composedPath()[0], the inner element. Those callbacks are exempted
 * in tools\e2e\data\event-coverage-exemptions.json as a known defect.
 */
const FOCUS = ['OnFocus', 'OnBlur'];

/**
 * @param {string} wrapper
 * @param {string[]} names
 */
const ids = (wrapper, names) => names.map(n => `${wrapper}.${n}`);

/**
 * Focuses a control, types a printable key and a value-changing key, then tabs away: the keyboard and focus
 * callbacks of every text-like and toggle control, plus input where the key edits the value.
 *
 * @param {Locator} target element inside the control that takes focus
 * @param {string[]} keys keys to press while focused (the first printable one produces keypress)
 */
async function focusTypeAndLeave(target, keys) {
  await target.focus();
  for (const key of keys) await target.press(key);
  await target.page().keyboard.press('Tab');
}

/**
 * The centre of an element's bounding box, after scrolling it into view (page.mouse does not scroll).
 *
 * @param {Locator} locator
 */
async function centreOf(locator) {
  await locator.scrollIntoViewIfNeeded();
  const box = await locator.boundingBox();
  if (!box) throw new Error('element has no bounding box');
  return { x: box.x + box.width / 2, y: box.y + box.height / 2 };
}

/** @type {EventCase[]} */
const EVENT_CASES = [
  // --- 3.12-rewired callbacks, read from the counters of the regular demo pages ---
  {
    name: 'WaCheckbox.OnCheckedChange counter on the Checkbox page follows a click',
    route: '/components/checkbox', tags: ['wa-checkbox'], callbacks: [],
    proven: ['WaCheckbox.OnCheckedChange'],
    run: async page => {
      const checkbox = page.locator('wa-checkbox', { hasText: 'Accept terms' });
      const calls = page.locator('p', { hasText: 'OnCheckedChange calls:' });
      await expect(calls).toHaveText('OnCheckedChange calls: 0');
      await checkbox.locator('[part~="control"]').click();
      await expect(calls).toHaveText('OnCheckedChange calls: 1');
      await expect(page.locator('p', { hasText: 'Agreed:' })).toHaveText('Agreed: True');
      await checkbox.locator('[part~="control"]').click();
      await expect(calls).toHaveText('OnCheckedChange calls: 2');
      await expect(page.locator('p', { hasText: 'Agreed:' })).toHaveText('Agreed: False');
    },
  },
  {
    name: 'WaRadioGroup.OnValueChange counter on the Radio Group page follows a click',
    route: '/components/radio-group', tags: ['wa-radio-group', 'wa-radio'], callbacks: [],
    proven: ['WaRadioGroup.OnValueChange'],
    run: async page => {
      const calls = page.locator('p', { hasText: 'OnValueChange calls:' });
      await expect(calls).toHaveText('OnValueChange calls: 0');
      // the "Two-way Binding" example is the last group on the page
      const group = page.locator('wa-radio-group').last();
      await group.locator('wa-radio', { hasText: 'Option 2' }).click();
      await expect(calls).toHaveText('OnValueChange calls: 1');
      await expect(page.locator('p', { hasText: 'Selected:' })).toHaveText('Selected: 2');
      await group.locator('wa-radio', { hasText: 'Option 3' }).click();
      await expect(calls).toHaveText('OnValueChange calls: 2');
    },
  },
  {
    name: 'WaSlider.OnValueChange counter on the Slider page follows the keyboard',
    route: '/components/slider', tags: ['wa-slider'], callbacks: [],
    proven: ['WaSlider.OnValueChange'],
    run: async page => {
      const slider = page.locator('wa-slider[label="Volume"]');
      const calls = page.locator('p', { hasText: 'OnValueChange calls:' });
      await expect(calls).toHaveText('OnValueChange calls: 0');
      await slider.locator('[role="slider"]').press('ArrowRight');
      await expect(calls).toHaveText('OnValueChange calls: 1');
      await expect(page.locator('p', { hasText: 'Volume:' })).toHaveText('Volume: 51');
    },
  },
  {
    name: 'WaZoomableFrame.OnLoad counter on the Zoomable Frame page counts the frame load',
    route: '/components/zoomable-frame', tags: ['wa-zoomable-frame'], callbacks: [],
    proven: ['WaZoomableFrame.OnLoad'],
    run: async page => {
      await expect(page.locator('p', { hasText: 'Frame loaded' })).toHaveText('Frame loaded 1 time(s)');
    },
  },

  // --- form controls (FormEventsHarness) ---
  {
    name: 'WaInput keyboard, input, clear',
    route: FORMS, tags: ['wa-input'],
    callbacks: [...ids('WaInput', [...KEYBOARD, 'OnInput', 'OnClear'])],
    run: async page => {
      const input = page.getByTestId('ev-input');
      await focusTypeAndLeave(input.locator('input'), ['x']);
      await expect(page.getByTestId('ev-input-model')).toHaveText('Hellox');
      await input.locator('[part~="clear-button"]').click();
      await expect(page.getByTestId('ev-input-model')).toHaveText('');
    },
  },
  {
    name: 'WaTextArea keyboard, input',
    route: FORMS, tags: ['wa-textarea'],
    callbacks: ids('WaTextArea', [...KEYBOARD, 'OnInput']),
    run: async page => {
      await focusTypeAndLeave(page.getByTestId('ev-textarea').locator('textarea'), ['x']);
      await expect(page.getByTestId('ev-textarea-model')).toHaveText('Some notesx');
    },
  },
  {
    name: 'WaNumberInput keyboard, input, stepper beforeinput',
    route: FORMS, tags: ['wa-number-input'],
    callbacks: ids('WaNumberInput', [...KEYBOARD, 'OnInput', 'OnBeforeInput']),
    run: async page => {
      const number = page.getByTestId('ev-number-input');
      await focusTypeAndLeave(number.locator('input'), ['7']);
      await expect(page.getByTestId('ev-number-input-model')).toHaveText('57');
      await number.locator('[part~="stepper-increment"]').click();
      await expect(page.getByTestId('ev-number-input-model')).toHaveText('58');
    },
  },
  {
    name: 'WaCheckbox keyboard, input, checked change',
    route: FORMS, tags: ['wa-checkbox'],
    callbacks: ids('WaCheckbox', [...KEYBOARD, 'OnInput', 'OnCheckedChange']),
    run: async page => {
      await focusTypeAndLeave(page.getByTestId('ev-checkbox').locator('input'), [' ']);
      await expect(page.getByTestId('ev-checkbox-model')).toHaveText('True');
      await expectFiredTimes(page, 'WaCheckbox.OnCheckedChange', 1);
      expect(await payloadOf(page, 'WaCheckbox.OnCheckedChange')).toBe(true);
    },
  },
  {
    name: 'WaSwitch keyboard, input, checked change',
    route: FORMS, tags: ['wa-switch'],
    callbacks: ids('WaSwitch', [...KEYBOARD, 'OnInput', 'OnCheckedChange']),
    run: async page => {
      await focusTypeAndLeave(page.getByTestId('ev-switch').locator('input'), [' ']);
      await expect(page.getByTestId('ev-switch-model')).toHaveText('True');
      await expectFiredTimes(page, 'WaSwitch.OnCheckedChange', 1);
      expect(await payloadOf(page, 'WaSwitch.OnCheckedChange')).toBe(true);
    },
  },
  {
    name: 'WaRadioGroup keyboard, input, value change and WaRadio focus, blur',
    route: FORMS, tags: ['wa-radio-group', 'wa-radio'],
    callbacks: [...ids('WaRadioGroup', [...KEYBOARD, 'OnInput', 'OnValueChange']), 'WaRadio.OnFocus', 'WaRadio.OnBlur'],
    run: async page => {
      // clicking Alpha focuses it (WaRadio.OnFocus) and selects it; ArrowRight moves on to Beta (blur)
      await page.getByTestId('ev-radio-alpha').click();
      await expect(page.getByTestId('ev-radio-group-model')).toHaveText('alpha');
      await page.keyboard.press('a');
      await page.keyboard.press('ArrowRight');
      await expect(page.getByTestId('ev-radio-group-model')).toHaveText('beta');
      await page.keyboard.press('Tab');
      await expectFired(page, 'WaRadioGroup.OnValueChange', 2);
      expect(await payloadOf(page, 'WaRadioGroup.OnValueChange')).toBe('beta');
    },
  },
  {
    name: 'WaSlider focus, keyboard, numeric input, value change',
    route: FORMS, tags: ['wa-slider'],
    callbacks: ids('WaSlider', [...KEYBOARD, ...FOCUS, 'OnInput', 'OnValueChange']),
    run: async page => {
      await focusTypeAndLeave(page.getByTestId('ev-slider').locator('[role="slider"]'), ['a', 'ArrowRight']);
      await expect(page.getByTestId('ev-slider-model')).toHaveText('51');
      // OnInput receives the value through the numericinput alias as a string, OnValueChange as a number
      expect(await payloadOf(page, 'WaSlider.OnInput')).toEqual({ value: '51' });
      expect(await payloadOf(page, 'WaSlider.OnValueChange')).toBe(51);
    },
  },
  {
    name: 'WaRange (range mode) focus, keyboard, numeric input, min and max value change',
    route: FORMS, tags: ['wa-slider'],
    callbacks: ids('WaRange', [...KEYBOARD, ...FOCUS, 'OnInput', 'OnMinValueChange', 'OnMaxValueChange']),
    run: async page => {
      const range = page.getByTestId('ev-range');
      await focusTypeAndLeave(range.locator('#thumb-min'), ['a', 'ArrowRight']);
      await expectFired(page, 'WaRange.OnMinValueChange');
      expect(await payloadOf(page, 'WaRange.OnMinValueChange')).toBe(21);
      expect(await payloadOf(page, 'WaRange.OnInput')).toEqual({ value: '21,80' });
      await range.locator('#thumb-max').press('ArrowLeft');
      await expectFired(page, 'WaRange.OnMaxValueChange');
      expect(await payloadOf(page, 'WaRange.OnMaxValueChange')).toBe(79);
    },
  },
  {
    name: 'WaRating focus, keyboard, hover',
    route: FORMS, tags: ['wa-rating'],
    callbacks: ids('WaRating', [...KEYBOARD, ...FOCUS, 'OnHover']),
    run: async page => {
      const rating = page.getByTestId('ev-rating');
      // wa-rating takes focus on its host
      await focusTypeAndLeave(rating, ['a', 'ArrowRight']);
      await expect(page.getByTestId('ev-rating-model')).toHaveText('3');
      const star = await centreOf(rating.locator('.symbol').nth(4));
      await page.mouse.move(star.x - 30, star.y);
      await page.mouse.move(star.x, star.y, { steps: 5 });
      await expectFired(page, 'WaRating.OnHover');
      expect((await payloadOf(page, 'WaRating.OnHover')).value).toBe(5);
    },
  },
  {
    name: 'WaColorPicker keyboard, input from its text field',
    route: FORMS, tags: ['wa-color-picker'],
    callbacks: ids('WaColorPicker', [...KEYBOARD, 'OnInput']),
    run: async page => {
      const picker = page.getByTestId('ev-color-picker');
      const trigger = picker.locator('[part~="trigger"]');
      await trigger.focus();
      await trigger.press('a');
      // Enter opens the popup; typing a colour into its text field and committing it edits the value
      await trigger.press('Enter');
      const field = picker.locator('[part~="input"] input, wa-input input').first();
      await field.fill('#ff0000');
      await field.press('Enter');
      await expect(page.getByTestId('ev-color-picker-model')).toHaveText('#ff0000');
      await page.keyboard.press('Escape');
      await page.keyboard.press('Tab');
    },
  },
  {
    name: 'WaKnownDate keyboard, input',
    route: FORMS, tags: ['wa-known-date'],
    callbacks: ids('WaKnownDate', [...KEYBOARD, 'OnInput']),
    run: async page => {
      const day = page.getByTestId('ev-known-date').locator('input[data-field="day"]');
      await day.focus();
      await day.press('Backspace');
      await day.press('Backspace');
      await day.press('2');
      await day.press('0');
      await page.keyboard.press('Tab');
      await expect(page.getByTestId('ev-known-date-model')).toHaveText('2000-01-20');
    },
  },
  {
    name: 'WaOtpInput keyboard, input, complete, clear',
    route: FORMS, tags: ['wa-otp-input'],
    callbacks: ids('WaOtpInput', [...KEYBOARD, 'OnInput', 'OnComplete', 'OnClear']),
    run: async page => {
      const otp = page.getByTestId('ev-otp-input');
      await otp.locator('.segment').first().click();
      await page.keyboard.type('4321');
      await expectFired(page, 'WaOtpInput.OnComplete');
      // the value is committed (change) on blur
      await page.keyboard.press('Tab');
      await expect(page.getByTestId('ev-otp-input-model')).toHaveText('4321');
      await page.getByTestId('ev-otp-input-clear').click();
    },
  },
  {
    name: 'WaTimeInput keyboard, input, popup show/hide, clear',
    route: FORMS, tags: ['wa-time-input'],
    callbacks: ids('WaTimeInput', [...KEYBOARD, 'OnInput', 'OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide', 'OnClear']),
    run: async page => {
      const time = page.getByTestId('ev-time-input');
      await focusTypeAndLeave(time.locator('[data-segment="minute"]'), ['a', 'ArrowUp']);
      await expect(page.getByTestId('ev-time-input-model')).toHaveText('09:31');
      await time.locator('[part~="expand-button"]').click();
      await expectFired(page, 'WaTimeInput.OnAfterShow');
      await page.keyboard.press('Escape');
      await expectFired(page, 'WaTimeInput.OnAfterHide');
      await time.locator('[part~="clear-button"]').click();
      await expect(page.getByTestId('ev-time-input-model')).toHaveText('');
    },
  },
  {
    name: 'WaSelect keyboard, input, popup show/hide, clear',
    route: FORMS, tags: ['wa-select', 'wa-option'],
    // no OnKeyPress: wa-select cancels every printable keydown for type-to-select, so no keypress follows
    callbacks: ids('WaSelect', ['OnKeyDown', 'OnKeyUp', 'OnInput', 'OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide', 'OnClear']),
    run: async page => {
      const select = page.getByTestId('ev-select');
      // opening the listbox and picking another option commits a new value and closes it
      await select.locator('[part~="combobox"]').click();
      await expectFired(page, 'WaSelect.OnAfterShow');
      await select.locator('wa-option', { hasText: 'Banana' }).click();
      await expect(page.getByTestId('ev-select-model')).toHaveText('banana');
      await expectFired(page, 'WaSelect.OnAfterHide');
      // a printable key on the focused select gives the keyboard callbacks
      await select.locator('[part~="display-input"]').press('c');
      await page.keyboard.press('Escape');
      await select.locator('[part~="clear-button"]').click();
      await expect(page.getByTestId('ev-select-model')).toHaveText('');
    },
  },
  {
    name: 'WaSelect (multiple) SelectedValuesChanged follows option clicks',
    route: FORMS, tags: ['wa-select', 'wa-option'],
    callbacks: [],
    proven: ['WaSelect.SelectedValuesChanged'],
    run: async page => {
      const select = page.getByTestId('ev-select-multiple');
      await select.locator('[part~="combobox"]').click();
      await select.locator('wa-option', { hasText: 'Cheese' }).click();
      await select.locator('wa-option', { hasText: 'Olives' }).click();
      await expect(page.getByTestId('ev-select-multiple-model')).toHaveText('cheese,olives');
    },
  },
  {
    name: 'WaButton click submitting the form',
    route: FORMS, tags: ['wa-button'],
    callbacks: ['WaButton.OnClick'],
    run: async page => {
      await page.getByTestId('ev-submit').click();
      await expectFiredTimes(page, 'form.submit', 1);
      await page.keyboard.press('Tab');
    },
  },
  {
    name: 'OnInvalid of every form control when an invalid form is submitted',
    route: FORMS, tags: ['wa-input', 'wa-button', 'wa-slider', 'wa-select', 'wa-color-picker'],
    callbacks: ids('WaInput', ['OnInvalid']).concat(
      ['WaTextArea', 'WaNumberInput', 'WaCheckbox', 'WaSwitch', 'WaRadioGroup', 'WaSlider', 'WaRange', 'WaRating',
        'WaColorPicker', 'WaKnownDate', 'WaOtpInput', 'WaTimeInput', 'WaSelect', 'WaButton'].map(w => `${w}.OnInvalid`)),
    run: async page => {
      await page.getByTestId('ev-mark-invalid').click();
      await expect(page.getByTestId('ev-invalid-marked')).toHaveText('True');
      await page.getByTestId('ev-submit').click();
      await expectFired(page, 'WaInput.OnInvalid');
      // the browser blocks the submit of an invalid form
      await expectFiredTimes(page, 'form.submit', 0);
    },
  },

  // --- Pro form controls (ProFormEventsHarness), skipped visibly on the free CDN ---
  {
    // no OnKeyDown: wa-combobox stops the propagation of every keydown in its input, so it never reaches Blazor
    name: 'WaCombobox keyboard, input, popup show/hide, create, clear',
    route: PRO, tags: ['wa-combobox', 'wa-option'], pro: true,
    callbacks: ids('WaCombobox', ['OnKeyPress', 'OnKeyUp', 'OnInput', 'OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide', 'OnCreate', 'OnClear']),
    proven: ['WaCombobox.ValueChanged'],
    run: async page => {
      const combobox = page.getByTestId('pro-combobox');
      const input = combobox.locator('[part~="combobox-input"]');
      await input.click();
      await expectFired(page, 'WaCombobox.OnAfterShow');
      // clearing the preselected Apple commits an empty value
      await combobox.locator('[part~="clear-button"]').click();
      await expect(page.getByTestId('pro-combobox-model')).toHaveText('');
      await input.pressSequentially('Mango');
      // with AllowCreate, Enter on text that matches no option asks the page to create it
      await input.press('Enter');
      await expectFired(page, 'WaCombobox.OnCreate');
      expect((await payloadOf(page, 'WaCombobox.OnCreate')).inputValue).toBe('Mango');
      await page.keyboard.press('Escape');
      await expectFired(page, 'WaCombobox.OnAfterHide');
    },
  },
  {
    // no OnKeyPress: the date segments cancel their keydown (they edit the segment themselves)
    name: 'WaDateInput keyboard, input, popup show/hide, clear',
    route: PRO, tags: ['wa-date-input'], pro: true,
    callbacks: ids('WaDateInput', ['OnKeyDown', 'OnKeyUp', 'OnInput', 'OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide', 'OnClear']),
    run: async page => {
      const date = page.getByTestId('pro-date-input');
      await date.locator('[data-segment="day"]').press('ArrowUp');
      await page.keyboard.press('Tab');
      await expect(page.getByTestId('pro-date-input-model')).toHaveText('2024-01-16');
      await date.locator('[part~="expand-button"]').click();
      await expectFired(page, 'WaDateInput.OnAfterShow');
      await page.keyboard.press('Escape');
      await expectFired(page, 'WaDateInput.OnAfterHide');
      await date.locator('[part~="clear-button"]').click();
      await expect(page.getByTestId('pro-date-input-model')).toHaveText('');
    },
  },
  {
    name: 'WaCombobox (multiple) SelectedValuesChanged follows option clicks',
    route: PRO, tags: ['wa-combobox', 'wa-option'], pro: true,
    callbacks: [],
    proven: ['WaCombobox.SelectedValuesChanged'],
    run: async page => {
      const combobox = page.getByTestId('pro-combobox-multiple');
      await combobox.locator('[part~="combobox-input"]').click();
      await combobox.locator('wa-option', { hasText: 'Cheese' }).click();
      await combobox.locator('wa-option', { hasText: 'Ham' }).click();
      await expect(page.getByTestId('pro-combobox-multiple-model')).toHaveText('cheese,ham');
    },
  },
  {
    name: 'WaFileInput change and input from a chosen file',
    route: PRO, tags: ['wa-file-input'], pro: true,
    callbacks: ids('WaFileInput', ['OnChange', 'OnInput']),
    run: async page => {
      await page.getByTestId('pro-file-input').locator('input[type="file"]')
        .setInputFiles({ name: 'notes.txt', mimeType: 'text/plain', buffer: Buffer.from('harness') });
    },
  },
  {
    name: 'WaDatePicker input and bound value from a day click',
    route: PRO, tags: ['wa-date-picker'], pro: true,
    callbacks: ['WaDatePicker.OnInput'],
    proven: ['WaDatePicker.ValueChanged'],
    run: async page => {
      await page.getByTestId('pro-date-picker').getByRole('button', { name: 'Wednesday, March 20, 2024' }).click();
      await expect(page.getByTestId('pro-date-picker-model')).toHaveText('2024-03-20');
      expect(await payloadOf(page, 'WaDatePicker.OnInput')).toEqual({ value: '2024-03-20' });
    },
  },
  {
    // the harness plays a generated half-second silent clip, so playback runs to its end quickly and offline
    name: 'WaVideo metadata, play, time update, pause, end, volume from the keyboard',
    route: PRO, tags: ['wa-video'], pro: true,
    callbacks: ids('WaVideo', ['OnLoadedMetadata', 'OnPlay', 'OnTimeUpdate', 'OnPause', 'OnEnded', 'OnVolumeChange']),
    run: async page => {
      const video = page.getByTestId('pro-video');
      await expectFired(page, 'WaVideo.OnLoadedMetadata');
      await video.focus();
      // k toggles playback; the clip ends by itself, which pauses it and ends it
      await page.keyboard.press('k');
      await expectFired(page, 'WaVideo.OnEnded');
      await page.keyboard.press('ArrowDown');
    },
  },
  {
    name: 'OnInvalid of the Pro form controls when an invalid form is submitted',
    route: PRO, tags: ['wa-combobox', 'wa-date-input', 'wa-file-input'], pro: true,
    callbacks: ['WaCombobox.OnInvalid', 'WaDateInput.OnInvalid', 'WaFileInput.OnInvalid'],
    run: async page => {
      await page.getByTestId('pro-mark-invalid').click();
      await expect(page.getByTestId('pro-invalid-marked')).toHaveText('True');
      await page.getByTestId('pro-submit').click();
      await expectFired(page, 'WaCombobox.OnInvalid');
      await expectFiredTimes(page, 'form.submit', 0);
    },
  },

  // --- overlays (OverlayEventsHarness) ---
  {
    name: 'WaDialog show/hide from a button and Escape',
    route: OVERLAYS, tags: ['wa-dialog', 'wa-button'],
    callbacks: ids('WaDialog', ['OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide']),
    run: async page => {
      await page.getByTestId('ov-dialog-open').click();
      await expectFired(page, 'WaDialog.OnAfterShow');
      await page.keyboard.press('Escape');
      await expectFired(page, 'WaDialog.OnAfterHide');
      await expectFiredTimes(page, 'WaDialog.OnShow', 1);
    },
  },
  {
    name: 'WaDrawer show/hide from a button and its close button',
    route: OVERLAYS, tags: ['wa-drawer', 'wa-button'],
    callbacks: ids('WaDrawer', ['OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide']),
    run: async page => {
      await page.getByTestId('ov-drawer-open').click();
      await expectFired(page, 'WaDrawer.OnAfterShow');
      await page.getByTestId('ov-drawer').locator('[part~="close-button"]').click();
      await expectFired(page, 'WaDrawer.OnAfterHide');
      await expectFiredTimes(page, 'WaDrawer.OnHide', 1);
    },
  },
  {
    name: 'WaPopover show/hide from its anchor',
    route: OVERLAYS, tags: ['wa-popover', 'wa-button'],
    callbacks: ids('WaPopover', ['OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide']),
    run: async page => {
      await page.getByTestId('ov-popover-anchor').click();
      await expectFired(page, 'WaPopover.OnAfterShow');
      await page.keyboard.press('Escape');
      await expectFired(page, 'WaPopover.OnAfterHide');
    },
  },
  {
    name: 'WaTooltip show/hide on hover',
    route: OVERLAYS, tags: ['wa-tooltip', 'wa-button'],
    callbacks: ids('WaTooltip', ['OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide']),
    run: async page => {
      await page.getByTestId('ov-tooltip-anchor').hover();
      await expectFired(page, 'WaTooltip.OnAfterShow');
      await page.mouse.move(0, 0);
      await expectFired(page, 'WaTooltip.OnAfterHide');
    },
  },
  {
    name: 'WaDropdown show/select/hide and WaDropdownItem focus/blur with the keyboard',
    route: OVERLAYS, tags: ['wa-dropdown', 'wa-dropdown-item', 'wa-button'],
    callbacks: [...ids('WaDropdown', ['OnShow', 'OnAfterShow', 'OnSelect', 'OnHide', 'OnAfterHide']), 'WaDropdownItem.OnFocus', 'WaDropdownItem.OnBlur'],
    run: async page => {
      await page.getByTestId('ov-dropdown-trigger').click();
      await expectFired(page, 'WaDropdown.OnAfterShow');
      // the menu focuses its first item (Copy); ArrowDown moves focus to Paste, Enter selects it
      await page.keyboard.press('ArrowDown');
      await expectFired(page, 'WaDropdownItem.OnFocus');
      await page.keyboard.press('ArrowDown');
      await expectFired(page, 'WaDropdownItem.OnBlur');
      await page.keyboard.press('Enter');
      await expectFired(page, 'WaDropdown.OnSelect');
      await expectFired(page, 'WaDropdown.OnAfterHide');
    },
  },
  {
    name: 'WaToastItem show on insertion and hide from its close button',
    route: OVERLAYS, tags: ['wa-toast', 'wa-button'],
    callbacks: ids('WaToastItem', ['OnShow', 'OnAfterShow', 'OnHide', 'OnAfterHide']),
    run: async page => {
      await page.getByTestId('ov-toast-show').click();
      await expectFired(page, 'WaToastItem.OnAfterShow');
      await page.getByTestId('ov-toast-item').locator('[part~="close-button"]').click();
      await expectFired(page, 'WaToastItem.OnAfterHide');
    },
  },

  // --- content (ContentEventsHarness) ---
  {
    name: 'WaTabGroup tab show/hide and WaTab click, focus, blur',
    route: CONTENT, tags: ['wa-tab-group', 'wa-tab'],
    callbacks: [...ids('WaTabGroup', ['OnTabShow', 'OnTabHide']), ...ids('WaTab', ['OnClick', 'OnFocus', 'OnBlur'])],
    run: async page => {
      await page.getByTestId('ct-tab-second').click();
      await expectFired(page, 'WaTabGroup.OnTabShow');
      expect((await payloadOf(page, 'WaTabGroup.OnTabShow')).name).toBe('second');
      await page.getByTestId('ct-tab-first').click();
      await expectFired(page, 'WaTabGroup.OnTabHide');
      await expectFired(page, 'WaTab.OnFocus');
      // moving the focus to the next tab blurs the first one
      await page.keyboard.press('ArrowRight');
    },
  },
  {
    name: 'WaAccordion expand and collapse from clicks',
    route: CONTENT, tags: ['wa-accordion', 'wa-accordion-item'],
    callbacks: ids('WaAccordion', ['OnExpand', 'OnAfterExpand', 'OnCollapse', 'OnAfterCollapse']),
    run: async page => {
      const header = page.getByTestId('ct-accordion-item').locator('[part~="button"]');
      await header.click();
      await expectFired(page, 'WaAccordion.OnAfterExpand');
      await header.click();
      await expectFired(page, 'WaAccordion.OnAfterCollapse');
    },
  },
  {
    name: 'WaTree selection and WaTreeItem expand/collapse/lazy load',
    route: CONTENT, tags: ['wa-tree', 'wa-tree-item'],
    callbacks: ['WaTree.OnSelectionChange', ...ids('WaTreeItem', ['OnExpand', 'OnAfterExpand', 'OnCollapse', 'OnAfterCollapse', 'OnLazyLoad', 'OnLazyChange'])],
    run: async page => {
      const parent = page.getByTestId('ct-tree-parent');
      await parent.locator('[part~="expand-button"]').first().click();
      await expectFired(page, 'WaTreeItem.OnAfterExpand');
      await parent.locator('[part~="expand-button"]').first().click();
      await expectFired(page, 'WaTreeItem.OnAfterCollapse');
      await parent.locator('[part~="item"]').first().click({ position: { x: 60, y: 10 } });
      await expectFired(page, 'WaTree.OnSelectionChange');
      await page.getByTestId('ct-tree-lazy').locator('[part~="expand-button"]').first().click();
      await expectFired(page, 'WaTreeItem.OnLazyLoad');
      await expectFired(page, 'WaTreeItem.OnLazyChange');
    },
  },
  {
    name: 'WaTag remove, WaCarousel slide change, WaPagination page change, WaBreadcrumbItem click',
    route: CONTENT, tags: ['wa-tag', 'wa-carousel', 'wa-pagination', 'wa-breadcrumb-item'],
    callbacks: ['WaTag.OnRemove', 'WaCarousel.OnSlideChange', 'WaPagination.OnBeforePageChange', 'WaPagination.OnPageChange', 'WaBreadcrumbItem.OnClick'],
    run: async page => {
      await page.getByTestId('ct-tag').locator('[part~="remove-button"]').click();
      await expect(page.getByTestId('ct-tag')).toHaveCount(0);
      await page.getByTestId('ct-carousel').locator('[part~="navigation-button-next"]').click();
      await expectFired(page, 'WaCarousel.OnSlideChange');
      await page.getByTestId('ct-pagination').getByRole('button', { name: /next/i }).click();
      await expectFired(page, 'WaPagination.OnPageChange');
      await page.getByTestId('ct-breadcrumb-item').click();
    },
  },
  {
    name: 'WaCopyButton copy and error',
    route: CONTENT, tags: ['wa-copy-button'], clipboard: true,
    callbacks: ['WaCopyButton.OnCopy', 'WaCopyButton.OnError'],
    run: async page => {
      await page.getByTestId('ct-copy').click();
      await expectFired(page, 'WaCopyButton.OnCopy');
      await page.getByTestId('ct-copy-error').click();
      await expectFired(page, 'WaCopyButton.OnError');
    },
  },
  {
    name: 'WaAnimatedImage, WaAvatar, WaIcon, WaInclude load and error',
    route: CONTENT, tags: ['wa-animated-image', 'wa-avatar', 'wa-icon', 'wa-include'],
    callbacks: ['WaAnimatedImage.OnLoad', 'WaAnimatedImage.OnError', 'WaAvatar.OnError', 'WaIcon.OnLoad', 'WaIcon.OnError', 'WaInclude.OnLoad', 'WaInclude.OnIncludeError'],
    run: async page => {
      // these fire as the page loads its sources; the payload of the failed include carries the fetch status
      await expectFired(page, 'WaInclude.OnIncludeError');
      expect((await payloadOf(page, 'WaInclude.OnIncludeError')).status).toBe(-1);
    },
  },
  {
    name: 'WaComparison change from the keyboard',
    route: CONTENT, tags: ['wa-comparison'],
    callbacks: ['WaComparison.OnChange'],
    run: async page => {
      await page.getByTestId('ct-comparison').locator('[part~="handle"]').press('ArrowRight');
    },
  },
  {
    name: 'WaAnimation start, finish, cancel',
    route: CONTENT, tags: ['wa-animation', 'wa-button'],
    callbacks: ['WaAnimation.OnStart', 'WaAnimation.OnFinish', 'WaAnimation.OnCancel'],
    run: async page => {
      await page.getByTestId('ct-animation-play').click();
      await expectFiredTimes(page, 'WaAnimation.OnStart', 1);
      await page.getByTestId('ct-animation-finish').click();
      await expectFired(page, 'WaAnimation.OnFinish');
      await page.getByTestId('ct-animation-play').click();
      await expectFiredTimes(page, 'WaAnimation.OnStart', 2);
      await page.getByTestId('ct-animation-cancel').click();
      await expectFired(page, 'WaAnimation.OnCancel');
    },
  },
  {
    name: 'WaPopup reposition on activation',
    route: CONTENT, tags: ['wa-popup', 'wa-button'],
    callbacks: ['WaPopup.OnReposition'],
    run: async page => {
      await page.getByTestId('ct-popup-activate').click();
    },
  },
  {
    // browsers do not fire error on an iframe whose document fails, so the inner iframe's error event is
    // raised by hand; what is proven is the part the library owns: Web Awesome's relay to the host (a
    // non-bubbling, composed event) and the wrapper's "onerror" binding receiving it
    name: 'WaZoomableFrame error relayed from its inner iframe',
    route: CONTENT, tags: ['wa-zoomable-frame'],
    callbacks: ['WaZoomableFrame.OnError'],
    run: async page => {
      await page.getByTestId('ct-zoomable-frame').evaluate(el => el.shadowRoot?.querySelector('iframe')?.dispatchEvent(new Event('error')));
    },
  },
];

/**
 * @typedef {object} KnownDefectCase
 * @property {string} name test title suffix
 * @property {string} exemption the entry of tools\e2e\data\event-coverage-exemptions.json that records the defect
 * @property {string} route page to open
 * @property {string[]} tags custom elements to wait for
 * @property {boolean} [pro] the tags are Pro components: the case skips visibly unless they upgrade
 * @property {(page: Page) => Promise<void>} run asserts the correct behaviour, which the defect breaks
 */

// Known defects, run as expected failures (test.fail): each asserts the behaviour a consumer would expect, so it
// fails today, and passes - failing the suite - once the defect is fixed, at which point the case moves to
// EVENT_CASES and the exemption is removed. The callbacks involved are exempted from coverage, not covered.
/** @type {KnownDefectCase[]} */
const KNOWN_DEFECT_CASES = [
  {
    name: 'OnFocus/OnBlur of a control focused inside its shadow root (WaInput) reach .NET',
    exemption: 'focus-in-shadow-root',
    route: FORMS, tags: ['wa-input'],
    run: async page => {
      const input = page.getByTestId('ev-input').locator('input');
      await input.focus();
      await page.keyboard.press('Tab');
      await expectFired(page, 'WaInput.OnFocus');
      await expectFired(page, 'WaInput.OnBlur');
    },
  },
  {
    name: 'WaColorPicker popup OnShow/OnAfterShow reach .NET',
    exemption: 'color-picker-non-bubbling-popup-events',
    route: FORMS, tags: ['wa-color-picker'],
    run: async page => {
      await page.getByTestId('ev-color-picker').locator('[part~="trigger"]').click();
      await expectFired(page, 'WaColorPicker.OnShow');
      await expectFired(page, 'WaColorPicker.OnAfterShow');
    },
  },
  {
    name: 'WaCombobox.OnKeyDown reaches .NET',
    exemption: 'combobox-keydown-stopped',
    route: PRO, tags: ['wa-combobox'], pro: true,
    run: async page => {
      await page.getByTestId('pro-combobox').locator('[part~="combobox-input"]').press('ArrowDown');
      await expectFired(page, 'WaCombobox.OnKeyDown');
    },
  },
  {
    name: 'WaIntersectionObserver.OnIntersect reaches .NET',
    exemption: 'intersect-non-bubbling',
    route: '/testing/event-payloads', tags: ['wa-intersection-observer'],
    run: async page => {
      await page.getByTestId('pl-intersection-observer').locator('div').first().scrollIntoViewIfNeeded();
      await expectFired(page, 'WaIntersectionObserver.OnIntersect');
    },
  },
];

module.exports = { EVENT_CASES, KNOWN_DEFECT_CASES, focusTypeAndLeave, centreOf };
