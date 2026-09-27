// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, isOpen, pickOption, typeInto } = require('./helpers/showcase');

// The Settings showcase (/showcases/settings) as a user works it: every control's bound value reaches the model,
// which the footer's "Saved settings" line renders; the tab group reports the section being edited (OnTabChange);
// the density buttons render the selected one from the model; and the notification details form an exclusive
// accordion (Web Awesome's name grouping).

const ROUTE = '/showcases/settings';
const TAGS = ['wa-tab-group', 'wa-tab', 'wa-tab-panel', 'wa-input', 'wa-select', 'wa-option', 'wa-switch', 'wa-button',
  'wa-details', 'wa-copy-button'];

/**
 * The footer line Blazor renders from the settings model.
 *
 * @param {import('@playwright/test').Page} page
 */
function savedSettings(page) {
  return page.getByText(/Saved settings:/);
}

/**
 * The footer line naming the section being edited.
 *
 * @param {import('@playwright/test').Page} page
 */
function editingSection(page) {
  return page.getByText(/Editing section:/);
}

/**
 * Clicks a switch by its label and waits until the element reports the new state.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} label
 * @param {boolean} checked the state after the click
 */
async function toggle(page, label, checked) {
  const control = page.locator('wa-switch', { hasText: label });
  await control.locator('[part~="control"]').click();
  await expect.poll(() => control.evaluate(el => /** @type {any} */ (el).checked), `${label} switched`).toBe(checked);
}

test('settings showcase: the general section updates the saved settings', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);

  await expect(savedSettings(page)).toHaveText(
    'Saved settings: Nova Studio · English · week starts on monday · comfortable density · auto-save on · ' +
    'notifications: mentions, review results · API key expires after 90 days');
  await expect(editingSection(page)).toHaveText('Editing section: general — changes are saved automatically');

  await typeInto(page.locator('wa-input[label="Workspace name"]'), 'Borealis Lab');
  await pickOption(page.locator('wa-select[label="Default language"]'), 'Deutsch');
  await pickOption(page.locator('wa-select[label="First day of the week"]'), 'Sunday');
  await page.locator('wa-button', { hasText: 'Spacious' }).click();
  await toggle(page, 'Save documents automatically', false);

  await expect(savedSettings(page)).toHaveText(
    'Saved settings: Borealis Lab · Deutsch · week starts on sunday · spacious density · auto-save off · ' +
    'notifications: mentions, review results · API key expires after 90 days');

  // the density buttons render the model's choice as their appearance
  await expect(page.locator('wa-button', { hasText: 'Spacious' })).toHaveAttribute('appearance', 'filled-outlined');
  await expect(page.locator('wa-button', { hasText: 'Comfortable' })).toHaveAttribute('appearance', 'outlined');
  await page.locator('wa-button', { hasText: 'Compact' }).click();
  await expect(savedSettings(page)).toContainText('compact density');
  await expect(page.locator('wa-button', { hasText: 'Compact' })).toHaveAttribute('appearance', 'filled-outlined');
  await expect(page.locator('wa-button', { hasText: 'Spacious' })).toHaveAttribute('appearance', 'outlined');

  await expectHealthy(page, problems);
});

test('settings showcase: the tabs report the section and the other sections update the saved settings', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);

  // notifications: the switches feed the model, the details open one at a time
  await page.locator('wa-tab', { hasText: 'Notifications' }).click();
  await expect(editingSection(page)).toHaveText('Editing section: notifications — changes are saved automatically');
  await toggle(page, 'Mentions', false);
  await toggle(page, 'Weekly digest', true);
  await expect(savedSettings(page)).toContainText('notifications: review results, weekly digest ·');
  await toggle(page, 'Review results', false);
  await toggle(page, 'Weekly digest', false);
  await expect(savedSettings(page)).toContainText('notifications: none ·');

  const quietHours = page.locator('wa-details[summary="Quiet hours"]');
  const channels = page.locator('wa-details[summary="Delivery channels"]');
  await quietHours.locator('[part~="header"]').click();
  await expect.poll(() => isOpen(quietHours), 'Quiet hours opened').toBe(true);
  await channels.locator('[part~="header"]').click();
  await expect.poll(() => isOpen(channels), 'Delivery channels opened').toBe(true);
  await expect.poll(() => isOpen(quietHours), 'Quiet hours closed (exclusive accordion)').toBe(false);

  // API access: the key expiration and the IP restriction feed the model
  await page.locator('wa-tab', { hasText: 'API access' }).click();
  await expect(editingSection(page)).toHaveText('Editing section: api — changes are saved automatically');
  await expect.poll(() => page.locator('wa-input[label="Workspace API key"]').evaluate(el => /** @type {any} */ (el).value),
    'the placeholder key').toBe('nova_live_your_api_key_here');
  await pickOption(page.locator('wa-select[label="Key expiration"]'), '1 year');
  await toggle(page, 'Restrict key to allowed IP ranges', true);
  await expect(savedSettings(page)).toContainText('API key expires after 365 days, allowed IP ranges only');

  // back to General: its controls kept their model values
  await page.locator('wa-tab', { hasText: 'General' }).click();
  await expect(editingSection(page)).toHaveText('Editing section: general — changes are saved automatically');
  await expect(savedSettings(page)).toContainText('Saved settings: Nova Studio · English ·');

  await expectHealthy(page, problems);
});
