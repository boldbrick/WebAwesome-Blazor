// @ts-check
const { test, expect, openHarness, openPanel, selectByTestId } = require('./fixtures');

// The layout of the Elements tab: the tree half and the details half scroll on their own inside the panel, and the
// divider between them resizes them (drag, arrow keys, double-click), stacked in a narrow panel and side by side in a
// wide one. The panel stores the size per orientation (panel.js, SPLIT_* constants).

// a panel narrower than the stack width of panel.js (SPLIT_STACK_BELOW_PX), and one wider
const NARROW_VIEWPORT = { width: 600, height: 600 };
const WIDE_VIEWPORT = { width: 1280, height: 720 };

// panel.js: the default share of the tree half, the minimum size of a half, the arrow-key step, the storage keys
const DEFAULT_FRACTION = 0.4;
const MIN_PANE_PX = 80;
const KEY_STEP_PX = 16;
const STORAGE_KEY_ROWS = 'wai-panel:split:rows';
const STORAGE_KEY_COLUMNS = 'wai-panel:split:columns';

// the distance of a drag of the divider, and the tolerance of a size compare (sub-pixel layout)
const DRAG_PX = 100;
const TOLERANCE_PX = 2;

/**
 * The boxes and scroll positions of the Elements tab, and the stored divider sizes.
 *
 * @param {import('@playwright/test').Page} panel
 */
async function layoutOf(panel) {
  return panel.evaluate(({ rowsKey, columnsKey }) => {
    const box = id => {
      const rect = document.getElementById(id).getBoundingClientRect();
      return { top: rect.top, bottom: rect.bottom, left: rect.left, right: rect.right, width: rect.width, height: rect.height };
    };
    const scroll = id => {
      const element = document.getElementById(id);
      return { scrollTop: element.scrollTop, scrollHeight: element.scrollHeight, clientHeight: element.clientHeight };
    };
    const tabPage = document.getElementById('tab-elements');
    return {
      viewport: { width: window.innerWidth, height: window.innerHeight },
      documentHeight: document.documentElement.scrollHeight,
      tabPageOverflow: tabPage.scrollHeight - tabPage.clientHeight,
      orientation: document.getElementById('split').classList.contains('split-rows') ? 'rows' : 'columns',
      treePane: box('tree-pane'),
      splitter: box('splitter'),
      tree: { ...box('tree'), ...scroll('tree') },
      details: { ...box('details'), ...scroll('details') },
      stored: { rows: localStorage.getItem(rowsKey), columns: localStorage.getItem(columnsKey) },
    };
  }, { rowsKey: STORAGE_KEY_ROWS, columnsKey: STORAGE_KEY_COLUMNS });
}

/**
 * Drags the divider by a distance along the orientation, with the mouse.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {number} dx
 * @param {number} dy
 */
async function dragSplitter(panel, dx, dy) {
  const box = await panel.locator('#splitter').boundingBox();
  if (!box) throw new Error('the divider is not visible');
  const x = box.x + box.width / 2;
  const y = box.y + box.height / 2;
  await panel.mouse.move(x, y);
  await panel.mouse.down();
  await panel.mouse.move(x + dx, y + dy, { steps: 5 });
  await panel.mouse.up();
}

/**
 * Opens the panel for the harness at a viewport size, waits for the orientation, and selects a wa-button so that the
 * details half holds its long attribute form.
 *
 * @param {any} fixtures
 * @param {{ width: number, height: number }} viewport
 * @param {'rows' | 'columns'} orientation
 */
async function openPanelAt({ page, context, serviceWorker, extensionId }, viewport, orientation) {
  await openHarness(page);
  const panel = await openPanel({ context, serviceWorker, extensionId }, page);
  await panel.setViewportSize(viewport);
  await expect(panel.locator('#split')).toHaveClass(new RegExp(`split-${orientation}`));
  await selectByTestId(panel, 'ih-toggle');
  await expect(panel.locator('#details h2')).toHaveText('<wa-button>');
  return panel;
}

test.describe('the Elements tab layout', () => {
  test('stacked: both halves scroll on their own and the divider resizes them', async ({ page, context, serviceWorker, extensionId }) => {
    const panel = await openPanelAt({ page, context, serviceWorker, extensionId }, NARROW_VIEWPORT, 'rows');

    // both halves fit into the panel and overflow on their own; the page and the tab do not scroll
    const start = await layoutOf(panel);
    expect(start.documentHeight, 'the panel page does not grow beyond the viewport').toBeLessThanOrEqual(start.viewport.height);
    expect(start.tabPageOverflow, 'the Elements tab itself does not scroll').toBeLessThanOrEqual(0);
    expect(start.treePane.bottom, 'the tree half is above the divider').toBeLessThanOrEqual(start.splitter.top + TOLERANCE_PX);
    expect(start.details.top, 'the details half is below the divider').toBeGreaterThanOrEqual(start.splitter.bottom - TOLERANCE_PX);
    expect(start.details.bottom, 'the details half ends inside the panel').toBeLessThanOrEqual(start.viewport.height + TOLERANCE_PX);
    expect(start.tree.scrollHeight, 'the tree overflows its half').toBeGreaterThan(start.tree.clientHeight);
    expect(start.details.scrollHeight, 'the details overflow their half').toBeGreaterThan(start.details.clientHeight);
    expect(start.treePane.height / (start.treePane.height + start.details.height), 'the default share of the tree half')
      .toBeCloseTo(DEFAULT_FRACTION, 1);

    // the mouse wheel scrolls only the half under the pointer
    await panel.locator('#tree').hover();
    await panel.mouse.wheel(0, 300);
    await expect.poll(async () => (await layoutOf(panel)).tree.scrollTop, 'the tree scrolls').toBeGreaterThan(0);
    expect((await layoutOf(panel)).details.scrollTop, 'the details stay').toBe(0);
    const treeScroll = (await layoutOf(panel)).tree.scrollTop;
    await panel.locator('#details').hover();
    await panel.mouse.wheel(0, 300);
    await expect.poll(async () => (await layoutOf(panel)).details.scrollTop, 'the details scroll').toBeGreaterThan(0);
    expect((await layoutOf(panel)).tree.scrollTop, 'the tree stays').toBe(treeScroll);

    // a drag down makes the tree half larger and the details half smaller, and stores the size
    await dragSplitter(panel, 0, DRAG_PX);
    const dragged = await layoutOf(panel);
    expect(dragged.treePane.height - start.treePane.height).toBeCloseTo(DRAG_PX, -1);
    expect(start.details.height - dragged.details.height).toBeCloseTo(DRAG_PX, -1);
    expect(Number(dragged.stored.rows), 'the stored share of the stacked layout').toBeCloseTo(
      dragged.treePane.height / (dragged.treePane.height + dragged.details.height), 2);
    expect(dragged.stored.columns, 'the side-by-side size stays unset').toBeNull();

    // an arrow key on the focused divider moves it by one step
    await panel.locator('#splitter').focus();
    await panel.keyboard.press('ArrowUp');
    const keyed = await layoutOf(panel);
    expect(dragged.treePane.height - keyed.treePane.height).toBeCloseTo(KEY_STEP_PX, 0);

    // a reload keeps the size
    await panel.reload();
    await expect(panel.locator('#split')).toHaveClass(/split-rows/);
    await expect.poll(async () => (await layoutOf(panel)).treePane.height, 'the stored size after a reload')
      .toBeGreaterThan(keyed.treePane.height - TOLERANCE_PX);
    expect((await layoutOf(panel)).treePane.height).toBeLessThan(keyed.treePane.height + TOLERANCE_PX);

    // a double-click resets the default size and removes the stored one
    await panel.locator('#splitter').dblclick();
    const reset = await layoutOf(panel);
    expect(reset.treePane.height / (reset.treePane.height + reset.details.height)).toBeCloseTo(DEFAULT_FRACTION, 2);
    expect(reset.stored.rows, 'the stored size is gone').toBeNull();
  });

  test('side by side: the divider resizes the halves and keeps a minimum size', async ({ page, context, serviceWorker, extensionId }) => {
    const panel = await openPanelAt({ page, context, serviceWorker, extensionId }, WIDE_VIEWPORT, 'columns');

    const start = await layoutOf(panel);
    expect(start.details.left, 'the details half is right of the divider').toBeGreaterThanOrEqual(start.splitter.right - TOLERANCE_PX);
    expect(start.details.bottom, 'the details half ends inside the panel').toBeLessThanOrEqual(start.viewport.height + TOLERANCE_PX);
    expect(start.details.scrollHeight, 'the details overflow their half').toBeGreaterThan(start.details.clientHeight);

    // a drag to the left makes the tree half narrower and the details half wider
    await dragSplitter(panel, -DRAG_PX, 0);
    const dragged = await layoutOf(panel);
    expect(start.treePane.width - dragged.treePane.width).toBeCloseTo(DRAG_PX, -1);
    expect(dragged.details.width - start.details.width).toBeCloseTo(DRAG_PX, -1);
    expect(dragged.stored.columns, 'the side-by-side size is stored').not.toBeNull();
    expect(dragged.stored.rows, 'the stacked size stays unset').toBeNull();

    // an arrow key on the focused divider moves it by one step
    await panel.locator('#splitter').focus();
    await panel.keyboard.press('ArrowRight');
    expect((await layoutOf(panel)).treePane.width - dragged.treePane.width).toBeCloseTo(KEY_STEP_PX, 0);

    // a drag far past the edge stops at the minimum size of each half
    await dragSplitter(panel, -start.viewport.width, 0);
    expect((await layoutOf(panel)).treePane.width).toBeCloseTo(MIN_PANE_PX, 0);
    await dragSplitter(panel, start.viewport.width, 0);
    expect((await layoutOf(panel)).details.width).toBeCloseTo(MIN_PANE_PX, 0);

    // a narrow panel stacks the halves with their own stored size (none yet: the default)
    await panel.setViewportSize(NARROW_VIEWPORT);
    await expect(panel.locator('#split')).toHaveClass(/split-rows/);
    await expect.poll(async () => {
      const narrow = await layoutOf(panel);
      return narrow.treePane.height / (narrow.treePane.height + narrow.details.height);
    }, 'the stacked layout starts at the default share').toBeCloseTo(DEFAULT_FRACTION, 2);
  });
});
