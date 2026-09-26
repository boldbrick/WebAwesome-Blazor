// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades } = require('./helpers/wa-ready');
const { openShowcase, expectHealthy, pickOption, typeInto } = require('./helpers/showcase');

// The Registration Form showcase (/showcases/registration-form) as a user works it: submitting the empty form lists
// every required field's DataAnnotations message, a wrong value shows its own message, and each correction removes
// its message as the field changes (the wrappers' bound values reach the EditContext). The arrival date is a
// WaDateInput, a Pro component, so on the free CDN the form cannot be completed: the first test ends with that
// message as the only one left, and the second (Pro) completes the registration and checks the summary Blazor
// renders from the model, then starts over.

const ROUTE = '/showcases/registration-form';
const TAGS = ['wa-input', 'wa-select', 'wa-option', 'wa-radio-group', 'wa-radio', 'wa-otp-input', 'wa-known-date',
  'wa-checkbox', 'wa-switch', 'wa-button', 'wa-textarea', 'wa-number-input'];

// the message of each required field when the form is submitted empty, in the model's order
const REQUIRED_MESSAGES = [
  'Please enter your full name.',
  'Please enter your e-mail address.',
  'Please choose a conference track.',
  'Please pick a ticket type.',
  'Please choose your arrival date.',
  'Please enter the verification code we texted you.',
  'Please provide your date of birth.',
  'You must accept the code of conduct to attend.',
];

const INVALID_EMAIL_MESSAGE = "That doesn't look like a valid e-mail address.";
const SHORT_CODE_MESSAGE = 'The verification code is six digits long.';
const ARRIVAL_MESSAGE = 'Please choose your arrival date.';

/**
 * The validation summary's messages, as Blazor renders them.
 *
 * @param {import('@playwright/test').Page} page
 */
function messages(page) {
  return page.locator('.validation-errors .validation-message');
}

/**
 * Submits the form with its primary button.
 *
 * @param {import('@playwright/test').Page} page
 */
function submit(page) {
  return page.locator('wa-button', { hasText: 'Complete registration' }).click();
}

/**
 * Types a code into the one-time-code field: its segments are painted over one hidden input, focused by a click.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} code
 */
async function typeCode(page, code) {
  const otp = page.locator('wa-otp-input');
  await otp.locator('.segment').first().click();
  await page.keyboard.press('ControlOrMeta+A');
  await page.keyboard.type(code);
  await page.keyboard.press('Tab');
}

/**
 * Fills every field the free CDN can drive: all required ones except the arrival date (a Pro WaDateInput).
 *
 * @param {import('@playwright/test').Page} page
 */
async function fillFreeFields(page) {
  await typeInto(page.locator('wa-input[label="Full name"]'), 'Ada Lovelace');
  await typeInto(page.locator('wa-input[label="Work e-mail"]'), 'ada@example.com');
  await pickOption(page.locator('wa-select[label="Conference track"]'), 'Applied AI');
  await page.locator('wa-radio', { hasText: 'Pro (incl. workshops)' }).click();
  await typeCode(page, '123456');

  // the date of birth: one field per part, committed on leaving it
  for (const [field, value] of [['month', '5'], ['day', '14'], ['year', '1990']]) {
    const input = page.locator(`wa-known-date input[data-field="${field}"]`);
    await input.fill(value);
    await input.press('Tab');
  }

  await page.locator('wa-checkbox', { hasText: 'I accept the' }).locator('[part~="control"]').click();
}

test('registration form showcase: validation lists each missing field and follows the corrections', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);

  await submit(page);
  await expect(messages(page), 'every required field').toHaveText(REQUIRED_MESSAGES);

  // a wrong e-mail and a short code show their own messages; the full name clears its message
  await typeInto(page.locator('wa-input[label="Full name"]'), 'Ada Lovelace');
  await typeInto(page.locator('wa-input[label="Work e-mail"]'), 'ada-at-example');
  await typeCode(page, '123');
  await expect(messages(page)).not.toContainText(['Please enter your full name.']);
  await expect(messages(page).filter({ hasText: INVALID_EMAIL_MESSAGE })).toHaveCount(1);
  await expect(messages(page).filter({ hasText: SHORT_CODE_MESSAGE })).toHaveCount(1);

  // correcting every field the free CDN can drive leaves the Pro arrival date as the only message
  await fillFreeFields(page);
  await expect(messages(page), 'only the arrival date is missing').toHaveText([ARRIVAL_MESSAGE]);
  await submit(page);
  await expect(messages(page), 'the submit validates the same').toHaveText([ARRIVAL_MESSAGE]);
  await expect(page.getByText("You're in,"), 'nothing was submitted').toHaveCount(0);

  // Clear form starts over with a new model: no messages, empty fields
  await page.locator('wa-button', { hasText: 'Clear form' }).click();
  await expect(messages(page)).toHaveCount(0);
  await expect.poll(() => page.locator('wa-input[label="Full name"]').evaluate(el => /** @type {any} */ (el).value ?? ''),
    'the name was cleared').toBe('');

  await expectHealthy(page, problems);
});

test('registration form showcase: a complete registration shows the summary and starts over', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  await skipUnlessProUpgrades(page, ['wa-date-input']);

  await fillFreeFields(page);

  // the arrival date: a day of the conference (March 12-14, 2026), typed into the segments
  const arrival = page.locator('wa-date-input[label="Arrival date"]');
  await arrival.locator('[data-segment="month"]').click();
  await page.keyboard.type('03132026');
  await page.keyboard.press('Tab');

  await submit(page);
  const summary = page.locator('wa-callout[variant="success"]');
  await expect(summary).toContainText("You're in, Ada Lovelace!");
  await expect(summary).toContainText('Your pro ticket for the ai track is reserved. A confirmation was sent to ada@example.com.');
  await expect(summary).toContainText('Your badge will use the default Nova Summit artwork.');
  await expect(page.locator('wa-input[label="Full name"]'), 'the form was replaced').toHaveCount(0);

  await page.locator('wa-button', { hasText: 'Register another attendee' }).click();
  await expect(summary).toHaveCount(0);
  await expect(page.locator('wa-input[label="Full name"]')).toHaveCount(1);
  await expect.poll(() => page.locator('wa-input[label="Full name"]').evaluate(el => /** @type {any} */ (el).value ?? ''),
    'a new, empty model').toBe('');

  await expectHealthy(page, problems);
});
