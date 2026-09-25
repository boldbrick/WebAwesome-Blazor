// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// how long the scroller's content may take to lay out once the element has rendered
const SCROLL_LAYOUT_TIMEOUT_MS = 15_000;

// Regression coverage for the WaScroller/WaTree/WaTreeItem wrappers added after confirming (via
// api-surface.json, generated from the exact bound 3.0.0-beta.6 CEM) that all three components
// were already part of the bound release with no Blazor wrapper yet — a real gap, not a future
// feature. See docs\CHANGELOG.md. The attributes Blazor emits are covered by bUnit; these tests check
// what the upgraded elements do with them.
test('tree items upgrade into a single-selection tree that expands and selects on click', async ({ page }) => {
  await page.goto('/components/tree');
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-tree', 'wa-tree-item']);

  const tree = page.locator('wa-tree').first();
  const documents = tree.locator('wa-tree-item', { hasText: 'Documents' }).first();
  const notes = tree.locator('wa-tree-item', { hasText: 'Notes' }).first();

  // the element applied the default single selection and treats Documents as a collapsed branch
  await expect(tree).toHaveJSProperty('selection', 'single');
  await expect(documents).toHaveAttribute('aria-expanded', 'false');

  await documents.locator('[part~="expand-button"]').first().click();
  await expect(documents).toHaveJSProperty('expanded', true);
  await expect(documents.locator('wa-tree-item', { hasText: 'Invoices' })).toBeVisible();

  // single selection: selecting Notes after Documents leaves only Notes selected
  await documents.locator('[part~="item"]').first().click({ position: { x: 60, y: 10 } });
  await expect(documents).toHaveJSProperty('selected', true);
  await notes.click();
  await expect(notes).toHaveJSProperty('selected', true);
  await expect(documents).toHaveJSProperty('selected', false);
});

test('tree item state parameters drive the upgraded items', async ({ page }) => {
  await page.goto('/components/tree-item');
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-tree', 'wa-tree-item']);

  const tree = page.locator('wa-tree').first();
  const documents = tree.locator('wa-tree-item', { hasText: 'Documents' }).first();
  const receipts = documents.locator('wa-tree-item', { hasText: 'Receipts' });
  const archived = tree.locator('wa-tree-item', { hasText: 'Archived' }).first();

  // Expanded shows the children, Selected marks the item for assistive technology, Disabled blocks selection
  await expect(documents).toHaveJSProperty('expanded', true);
  await expect(receipts).toBeVisible();
  await expect(receipts).toHaveAttribute('aria-selected', 'true');
  await expect(archived).toHaveAttribute('aria-disabled', 'true');
  await archived.click({ force: true });
  await expect(archived).toHaveJSProperty('selected', false);
  await expect(receipts).toHaveJSProperty('selected', true);
});
test('scroller content overflows and becomes scrollable', async ({ page }) => {
  await page.goto('/components/scroller');
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-scroller']);
  const scroller = page.locator('wa-scroller').first();
  await expect(scroller).toBeVisible();

  // the actual scrolling happens on an element inside wa-scroller's shadow root, not the host; the element is
  // upgraded and rendered now, but its content still has to be laid out (fonts, images of the demo content),
  // which under a full worker load can take longer than the default expect timeout
  await expect(async () => {
    const overflows = await scroller.evaluate(el => {
      const content = el.shadowRoot?.querySelector('[part~="content"]');
      return !!content && content.scrollWidth > content.clientWidth;
    });
    expect(overflows).toBe(true);
  }).toPass({ timeout: SCROLL_LAYOUT_TIMEOUT_MS });
});
