// @ts-check
const { test, expect } = require('@playwright/test');

// WaButton.Form renders the form="id" content attribute. The attribute left the Web Awesome
// 3.1.0 CEM (form association moved to the native ElementInternals mechanism, upstream PR 1815)
// but remains fully functional — this spec proves it end-to-end in a real browser: a submit
// wa-button placed OUTSIDE an EditForm drives that form's submit event, and Blazor's EditForm
// intercepts it (OnSubmit handler runs, no full-page post). Because the attribute is now
// CEM-invisible, the parity suite cannot flag upstream regressions here — this spec is the guard.
test('external submit wa-button drives an EditForm via form="id" association', async ({ page }) => {
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));

  await page.goto('/components/button');
  await page.waitForSelector('.demo-shell');

  const button = page.locator('wa-button[data-testid="external-submit"]');
  await expect(button).toHaveAttribute('form', 'external-submit-form');

  // type into the form's input, then submit from the associated button outside the form
  const input = page.locator('#external-submit-form wa-input input');
  await input.fill('Ada');
  await button.click();

  const result = page.locator('wa-callout[data-testid="external-submit-result"]');
  await expect(result).toContainText('Submitted 1 time(s)');
  await expect(result).toContainText('last value: Ada');

  // second submit still goes through Blazor's handler, proving no full-page form post occurred
  await button.click();
  await expect(result).toContainText('Submitted 2 time(s)');

  expect(errors).toEqual([]);
});
