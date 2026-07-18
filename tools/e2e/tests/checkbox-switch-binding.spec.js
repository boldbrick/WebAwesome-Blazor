// @ts-check
const { test, expect } = require('@playwright/test');

// Regression coverage for a real bug: WaCheckbox/WaSwitch's @bind-Value never reflected the
// real checked state (Blazor's built-in change-event value extraction only reads .checked for
// tagName === "INPUT"; these are custom elements). See docs\CHANGELOG.md for the root cause
// and the JS-interop-based workaround applied in WaCheckbox.cs/WaSwitch.cs.
test('checkbox two-way binding reflects clicks back into Blazor state', async ({ page }) => {
  await page.goto('/components/checkbox');
  await page.waitForSelector('.demo-shell');

  const agreeCheckbox = page.locator('wa-checkbox', { hasText: 'Accept terms' }).first();
  const paragraph = page.locator('p', { hasText: 'Agreed:' });

  await expect(paragraph).toHaveText('Agreed: False');

  // click near the control itself (not the label text) to ensure a real toggle
  await agreeCheckbox.click({ position: { x: 10, y: 20 } });
  await page.waitForTimeout(300);

  await expect(paragraph).toHaveText('Agreed: True');
});

test('switch two-way binding reflects clicks back into Blazor state', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('.demo-shell');

  const darkSwitch = page.locator('wa-switch', { hasText: 'Dark mode' }).first();
  await expect(darkSwitch).not.toHaveJSProperty('checked', true);

  await darkSwitch.click();
  await page.waitForTimeout(300);
  await expect(darkSwitch).toHaveJSProperty('checked', true);

  await darkSwitch.click();
  await page.waitForTimeout(300);
  await expect(darkSwitch).toHaveJSProperty('checked', false);
});
