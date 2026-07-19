// @ts-check
const { test, expect } = require('@playwright/test');

// Regression coverage for a real bug: every wa-* custom event bound in the wrappers was
// bound under a bare "wa-..." attribute name and no event type was registered with
// Blazor.registerCustomEventType, so no wa-* EventCallback ever fired — and when fixed,
// payload-carrying events additionally require the typed args to flow from event.detail.
// The wrappers now bind "onwa-..." and the RCL JS initializer
// (WebAwesome.Blazor.lib.module.js) registers every bound event with a createEventArgs
// mapping. See docs\CHANGELOG.md for the full root cause.
//
// The tab group demo displays the payload (e.Name) of OnTabChange, which is served by the
// real wa-tab-show browser event — this proves both delivery and detail deserialization.
test('wa-* custom event delivers its typed payload into the Blazor handler', async ({ page }) => {
  await page.goto('/components/tab-group');
  await page.waitForSelector('.demo-shell');

  // the "Reacting to Tab Changes" example is the group with exactly two tabs
  const group = page.locator('wa-tab-group', { has: page.locator('wa-tab[panel="custom"]') }).last();
  const display = page.locator('p', { hasText: 'Last active panel:' });

  await group.locator('wa-tab[panel="custom"]').click();
  await expect(display).toHaveText('Last active panel: custom');

  await group.locator('wa-tab[panel="general"]').click();
  await expect(display).toHaveText('Last active panel: general');
});

// WaDetails derives OnToggle's IsOpen from which event fired (wa-show/wa-hide) via the
// initializer's createEventArgs — verify a real open/close round-trip produces no errors.
test('wa-show/wa-hide reach Blazor without event dispatch errors', async ({ page }) => {
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));

  await page.goto('/components/details');
  await page.waitForSelector('.demo-shell');

  const details = page.locator('wa-details').first();
  await details.click();
  await page.waitForTimeout(400);
  await details.click();
  await page.waitForTimeout(400);

  expect(errors).toEqual([]);
});
