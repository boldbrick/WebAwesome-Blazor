// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Regression coverage for a real bug: WaCheckbox/WaSwitch's @bind-Value never reflected the
// real checked state (Blazor's built-in change-event value extraction only reads .checked for
// tagName === "INPUT"; these are custom elements). See docs\CHANGELOG.md for the root cause
// and the JS-interop-based workaround applied in WaCheckbox.cs/WaSwitch.cs. Each test reads the
// Blazor-rendered model and OnCheckedChange counter of the demo page's "Two-way Binding" example:
// Web Awesome toggles the element's own checked property by itself, so only the rendered echo
// proves that the state reached .NET.

test('checkbox two-way binding reflects clicks back into Blazor state', async ({ page }) => {
  await page.goto('/components/checkbox');
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-checkbox']);

  const control = page.locator('wa-checkbox', { hasText: 'Accept terms' }).locator('[part~="control"]');
  const model = page.locator('p', { hasText: 'Agreed:' });
  const calls = page.locator('p', { hasText: 'OnCheckedChange calls:' });

  await expect(model).toHaveText('Agreed: False');

  // click the control itself (not the label text) to ensure a real toggle
  await control.click();
  await expect(model).toHaveText('Agreed: True');
  await expect(calls).toHaveText('OnCheckedChange calls: 1');

  await control.click();
  await expect(model).toHaveText('Agreed: False');
  await expect(calls).toHaveText('OnCheckedChange calls: 2');
});

test('switch two-way binding reflects clicks back into Blazor state', async ({ page }) => {
  await page.goto('/components/switch');
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-switch']);

  const control = page.locator('wa-switch', { hasText: 'Notifications' }).locator('[part~="control"]');
  const model = page.locator('p', { hasText: 'Notifications:' });
  const calls = page.locator('p', { hasText: 'OnCheckedChange calls:' });

  await expect(model).toHaveText('Notifications: False');

  await control.click();
  await expect(model).toHaveText('Notifications: True');
  await expect(calls).toHaveText('OnCheckedChange calls: 1');

  await control.click();
  await expect(model).toHaveText('Notifications: False');
  await expect(calls).toHaveText('OnCheckedChange calls: 2');
});
