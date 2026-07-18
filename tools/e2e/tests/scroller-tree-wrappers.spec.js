// @ts-check
const { test, expect } = require('@playwright/test');

// Regression coverage for the WaScroller/WaTree/WaTreeItem wrappers added after confirming (via
// api-surface.json, generated from the exact bound 3.0.0-beta.6 CEM) that all three components
// were already part of the bound release with no Blazor wrapper yet — a real gap, not a future
// feature. See docs\CHANGELOG.md.
test('tree renders nested items with expected state', async ({ page }) => {
  await page.goto('/components/tree');
  await expect(page.locator('wa-tree-item')).toHaveCount(7);
  await expect(page.locator('wa-tree')).toHaveAttribute('selection', 'single');
});

test('tree item state attributes render correctly', async ({ page }) => {
  await page.goto('/components/tree-item');
  await expect(page.locator('wa-tree-item[expanded]')).toHaveCount(1);
  await expect(page.locator('wa-tree-item[selected]')).toHaveCount(1);
  await expect(page.locator('wa-tree-item[disabled]')).toHaveCount(1);
});

test('scroller content overflows and becomes scrollable', async ({ page }) => {
  await page.goto('/components/scroller');
  const scroller = page.locator('wa-scroller').first();
  await expect(scroller).toBeVisible();

  // the actual scrolling happens on an element inside wa-scroller's shadow root, not the host;
  // give the custom element a moment to upgrade and render its shadow DOM before checking
  await expect(async () => {
    const overflows = await scroller.evaluate(el => {
      const content = el.shadowRoot?.querySelector('[part~="content"]');
      return !!content && content.scrollWidth > content.clientWidth;
    });
    expect(overflows).toBe(true);
  }).toPass({ timeout: 5_000 });
});
