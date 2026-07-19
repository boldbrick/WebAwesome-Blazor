// @ts-check
const { test, expect } = require('@playwright/test');

// Regression coverage for a real bug: the dark-mode switch and theme selector visibly did
// nothing, because (a) WaSwitch/WaCheckbox's two-way @bind-Value never received the real
// checked state (see docs\CHANGELOG.md "Fixed" for the root cause), and (b) the theme
// stylesheet swap alone has no effect without also applying Web Awesome's "wa-theme-{name}"
// class to <html> (its theme CSS is scoped under that class, not :root).
test.describe('theme switching', () => {
  test('dark mode switch actually toggles the color scheme', async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('.demo-shell');

    const bgBefore = await page.evaluate(() => getComputedStyle(document.body).backgroundColor);

    const darkSwitch = page.locator('wa-switch', { hasText: 'Dark mode' }).first();
    await darkSwitch.click();
    await page.waitForTimeout(400);

    await expect(page.locator('html')).toHaveClass(/wa-dark/);
    const bgAfter = await page.evaluate(() => getComputedStyle(document.body).backgroundColor);
    expect(bgAfter).not.toBe(bgBefore);

    // toggle back off
    await darkSwitch.click();
    await page.waitForTimeout(400);
    await expect(page.locator('html')).toHaveClass(/wa-light/);
  });

  test('theme selector applies both the stylesheet and the wa-theme-{name} class', async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('.demo-shell');

    await page.locator('wa-select[label="Theme"]').click();
    await page.locator('wa-option', { hasText: 'Awesome' }).click();
    await page.waitForTimeout(400);

    await expect(page.locator('html')).toHaveClass(/wa-theme-awesome/);
    const href = await page.evaluate(() => /** @type {HTMLLinkElement | null} */ (document.getElementById('wa-theme'))?.href);
    expect(href).toContain('themes/awesome.css');
  });
});
