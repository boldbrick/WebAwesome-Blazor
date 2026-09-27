// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, isOpen, pickOption, typeInto } = require('./helpers/showcase');

// The Overlays showcase (/showcases/overlays) as a user works it: the invite dialog's form feeds the feedback
// callout, the drawer and the popover open and close (by their own buttons and by Escape, after which the trigger
// opens them again), the popup toggles with its button's text, the dropdown's items feed the callout, and both
// toasts show and close: the programmatic one (WaToast.CreateAsync) and the declarative deployment items, whose model
// entry OnAfterHide removes. Every outcome asserted is one Blazor renders from the model.

const ROUTE = '/showcases/overlays';
const TAGS = ['wa-button', 'wa-dialog', 'wa-drawer', 'wa-input', 'wa-select', 'wa-option', 'wa-popover', 'wa-popup',
  'wa-dropdown', 'wa-dropdown-item', 'wa-toast'];

// past the 5 s duration of the showcase's toasts
const TOAST_DURATION_JUMP_MS = 6_000;

/**
 * The feedback callout Blazor renders above the cards.
 *
 * @param {import('@playwright/test').Page} page
 */
function feedback(page) {
  return page.locator('main wa-callout[variant="success"]');
}

test('overlays showcase: the invite dialog sends the typed e-mail and the picked role', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const dialog = page.locator('wa-dialog[label="Invite a teammate"]');
  const invite = page.locator('wa-button', { hasText: 'Invite teammate' });

  await invite.click();
  await expect.poll(() => isOpen(dialog), 'the dialog opened').toBe(true);
  await typeInto(dialog.locator('wa-input[label="E-mail address"]'), 'jonas.petrov@meridianlabs.example');
  await pickOption(dialog.locator('wa-select[label="Role"]'), 'Workspace admin');
  await dialog.locator('wa-button', { hasText: 'Send invitation' }).click();

  await expect.poll(() => isOpen(dialog), 'Send closed the dialog').toBe(false);
  await expect(feedback(page)).toHaveText('Invitation sent to jonas.petrov@meridianlabs.example as admin.');

  // the model cleared the e-mail and kept the role; reopening shows both
  await invite.click();
  await expect.poll(() => isOpen(dialog), 'the dialog opened again').toBe(true);
  await expect.poll(() => dialog.locator('wa-input[label="E-mail address"]').evaluate(el => /** @type {any} */ (el).value ?? ''),
    'the e-mail was cleared').toBe('');
  await expect.poll(() => dialog.locator('wa-select[label="Role"]').evaluate(el => /** @type {any} */ (el).value),
    'the role was kept').toBe('admin');

  // Send without an e-mail drafts the invitation
  await dialog.locator('wa-button', { hasText: 'Send invitation' }).click();
  await expect(feedback(page)).toHaveText('Invitation drafted — add an e-mail address to send it.');

  // Escape closes the dialog through OnHide, so the trigger opens it once more; Cancel closes it too
  await invite.click();
  await expect.poll(() => isOpen(dialog), 'the dialog opened a third time').toBe(true);
  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(dialog), 'Escape closed the dialog').toBe(false);
  await invite.click();
  await expect.poll(() => isOpen(dialog), 'the dialog opened after Escape').toBe(true);
  await dialog.locator('wa-button', { hasText: 'Cancel' }).click();
  await expect.poll(() => isOpen(dialog), 'Cancel closed the dialog').toBe(false);
  await expect(feedback(page), 'Cancel sent nothing').toHaveText('Invitation drafted — add an e-mail address to send it.');

  await expectHealthy(page, problems);
});

test('overlays showcase: the drawer, the popover and the popup open and close', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);

  // drawer: its Close button, then Escape, and the trigger opens it after each
  const drawer = page.locator('wa-drawer[label="Recent activity"]');
  const showActivity = page.locator('wa-button', { hasText: 'Show activity' });
  await showActivity.click();
  await expect.poll(() => isOpen(drawer), 'the drawer opened').toBe(true);
  await expect(drawer.getByText('Priya Anand')).toBeVisible();
  await drawer.locator('wa-button', { hasText: 'Close' }).click();
  await expect.poll(() => isOpen(drawer), 'Close closed the drawer').toBe(false);
  await showActivity.click();
  await expect.poll(() => isOpen(drawer), 'the drawer opened again').toBe(true);
  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(drawer), 'Escape closed the drawer').toBe(false);
  await showActivity.click();
  await expect.poll(() => isOpen(drawer), 'the drawer opened after Escape').toBe(true);
  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(drawer)).toBe(false);

  // popover: anchored to its trigger by For
  const popover = page.locator('wa-popover[for="deploy-status-trigger"]');
  await page.locator('#deploy-status-trigger').click();
  await expect.poll(() => isOpen(popover), 'the popover opened').toBe(true);
  await expect(popover.getByText('Staging is healthy.')).toBeVisible();
  await page.keyboard.press('Escape');
  await expect.poll(() => isOpen(popover), 'Escape closed the popover').toBe(false);

  // popup: the button's text and the popup's Active both follow the model
  const popupButton = page.locator('#popup-anchor');
  const popup = page.locator('wa-popup[anchor="popup-anchor"]');
  await expect(popupButton).toHaveText('Show keyboard hint');
  await popupButton.click();
  await expect(popupButton).toHaveText('Hide keyboard hint');
  await expect.poll(() => isOpen(popup), 'the popup is active').toBe(true);
  await popupButton.click();
  await expect(popupButton).toHaveText('Show keyboard hint');
  await expect.poll(() => isOpen(popup), 'the popup is inactive').toBe(false);

  await expectHealthy(page, problems);
});

test('overlays showcase: the dropdown items feed the feedback callout', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const dropdown = page.locator('wa-dropdown', { has: page.locator('wa-button', { hasText: 'Document actions' }) });

  for (const [item, message] of [
    ['Duplicate', 'Duplicated document.'],
    ['Rename', "Renamed document to 'Q2 launch plan'."],
    ['Delete', 'Document moved to trash.'],
  ]) {
    await dropdown.locator('wa-button', { hasText: 'Document actions' }).click();
    await expect.poll(() => isOpen(dropdown), `the menu opened for ${item}`).toBe(true);
    await dropdown.locator('wa-dropdown-item', { hasText: item }).click();
    await expect(feedback(page)).toHaveText(message);
    await expect.poll(() => isOpen(dropdown), `picking ${item} closed the menu`).toBe(false);
  }

  await expectHealthy(page, problems);
});

test('overlays showcase: the programmatic and the declarative toasts show, close and show again', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS, { clock: true });
  const stack = page.locator('wa-toast');
  const items = stack.locator('wa-toast-item');
  const deploy = page.locator('wa-button', { hasText: 'Deploy to staging' });

  // Save draft: a toast item WaToast.CreateAsync makes (not Blazor's), gone after its duration
  await page.locator('wa-button', { hasText: 'Save draft' }).click();
  await expect(items.filter({ hasText: 'Draft saved to the workspace.' }), 'the draft toast shows').toBeVisible();
  await page.mouse.move(0, 0);
  await page.clock.fastForward(TOAST_DURATION_JUMP_MS);
  await expect(items, 'the draft toast is gone').toHaveCount(0);

  // Deploy to staging: a declarative item from the model, removed from the model in OnAfterHide
  await deploy.click();
  await expect(items, 'one deployment toast').toHaveCount(1);
  await expect(items.first()).toHaveText('Build 3.2.0-rc.1 deployed to staging.');
  await expect(items.first()).toBeVisible();
  await page.mouse.move(0, 0);
  await page.clock.fastForward(TOAST_DURATION_JUMP_MS);
  await expect(items, 'the first deployment toast is gone').toHaveCount(0);
  await expectHealthy(page, problems);

  // a second deployment renders the next build from the model's counter, and closes as well
  await deploy.click();
  await expect(items, 'a second deployment toast').toHaveCount(1);
  await expect(items.first()).toHaveText('Build 3.2.0-rc.2 deployed to staging.');
  await expect(items.first()).toBeVisible();
  await page.mouse.move(0, 0);
  await page.clock.fastForward(TOAST_DURATION_JUMP_MS);
  await expect(items, 'the second deployment toast is gone').toHaveCount(0);

  // two at once: both show, and both close
  await deploy.click();
  await deploy.click();
  await expect(items, 'two deployment toasts').toHaveCount(2);
  await expect(items.nth(1)).toHaveText('Build 3.2.0-rc.4 deployed to staging.');
  await page.mouse.move(0, 0);
  await page.clock.fastForward(TOAST_DURATION_JUMP_MS);
  await expect(items, 'both deployment toasts are gone').toHaveCount(0);

  await expectHealthy(page, problems);
});
