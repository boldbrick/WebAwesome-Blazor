// @ts-check
const { test, expect } = require('./helpers/test');
const { openShowcase, expectHealthy, readableText } = require('./helpers/showcase');

// The Content & Observers showcase (/showcases/content) as a user works it: the document tree reports the selected
// document, the tip rotator reports its pick (OnContentChange), the split panel reports its position and the
// resize observer the measured pane width, adding reviewers is reported by the mutation observer until the pool is
// used up, and scrolling the changelog brings the sign-off card into view for the intersection observer. Every
// outcome asserted is text Blazor renders from the event payloads.

const ROUTE = '/showcases/content';
const TAGS = ['wa-tree', 'wa-tree-item', 'wa-random-content', 'wa-split-panel', 'wa-resize-observer', 'wa-mutation-observer',
  'wa-intersection-observer', 'wa-button', 'wa-tag'];

// past the tip rotator's 4 s autoplay interval
const AUTOPLAY_JUMP_MS = 4_500;

// the tips the rotator shows, one of which it reports
const TIPS = [
  'Press Ctrl+K to open the command palette.',
  'Drag a panel to the edge to dock it.',
  'Right-click a layer to duplicate it in place.',
  'Hold Alt while resizing to scale from the center.',
];

/**
 * Escapes a text for a regular expression.
 *
 * @param {string} text
 */
function escapeRegExp(text) {
  return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

test('content showcase: the document tree reports the selected document', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const selected = page.getByText(/Selected:/);
  const tree = page.locator('wa-tree');

  // the documents are the leaf items (a parent item's text includes its children's)
  const documents = tree.locator('wa-tree-item:not(:has(wa-tree-item))');

  await expect(selected).toHaveText('Selected: Release notes');
  await documents.filter({ hasText: 'Upgrade guide' }).first().locator('[part~="label"]').click();
  await expect(selected).toHaveText('Selected: Upgrade guide');
  await documents.filter({ hasText: 'Known issues' }).locator('[part~="label"]').click();
  await expect(selected).toHaveText('Selected: Known issues');

  // a parent item reports its own label, not its subtree's text
  const release31 = tree.locator('wa-tree-item:has(wa-tree-item)').filter({ hasText: '3.1 release' });
  const ownLabel = await release31.evaluateHandle(item => /** @type {any} */ (item).shadowRoot.querySelector('[part~="label"]'));
  await /** @type {import('@playwright/test').ElementHandle} */ (ownLabel.asElement()).click();
  await expect(selected).toHaveText('Selected: 3.1 release');

  await expectHealthy(page, problems);
});

test('content showcase: the tip rotator reports the tip it shows', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS, { clock: true });
  const tip = page.getByText(/Waiting for the first tip|Now showing:/);

  await page.clock.fastForward(AUTOPLAY_JUMP_MS);
  const shown = new RegExp(`^Now showing: (${TIPS.map(escapeRegExp).join('|')})$`);
  await expect.poll(() => readableText(tip), 'the first tip is reported').toMatch(shown);
  const first = await readableText(tip);

  // the unique mode shows another tip on the next interval
  await page.clock.fastForward(AUTOPLAY_JUMP_MS);
  await expect.poll(() => readableText(tip), 'another tip is reported').not.toBe(first);
  await expect.poll(() => readableText(tip)).toMatch(shown);

  await expectHealthy(page, problems);
});

test('content showcase: the split panel and the resize observer report the new layout', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const position = page.getByText(/Split position:/);
  const width = page.getByText(/Drag the divider to measure this pane\.|This pane is \d+ px wide\./);

  await expect(position).toHaveText('Split position: 55%');
  const divider = page.locator('wa-split-panel [part~="divider"]');
  await divider.focus();
  for (let step = 0; step < 5; step++) await page.keyboard.press('ArrowLeft');

  await expect(position).toHaveText('Split position: 50%');
  // the reported width is the observed pane's content box (ResizeObserverEntry.contentRect) at the new position
  const contentWidth = () => page.locator('wa-resize-observer').evaluate(el => {
    const pane = /** @type {HTMLElement} */ (el.firstElementChild);
    const style = getComputedStyle(pane);
    return Math.round(pane.getBoundingClientRect().width - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight));
  });
  await expect.poll(async () => readableText(width), 'the resize observer measured the pane').toMatch(/^This pane is \d+ px wide\.$/);
  await expect.poll(async () => Math.abs(Number((await readableText(width)).match(/\d+/)?.[0]) - await contentWidth()),
    'the reported width is the pane content width').toBeLessThanOrEqual(1);

  await expectHealthy(page, problems);
});

test('content showcase: adding reviewers is reported by the mutation observer until the pool is used up', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const reviewers = page.locator('wa-mutation-observer wa-tag');
  const addReviewer = page.locator('wa-button', { hasText: 'Add reviewer' });
  const message = page.getByText(/No mutations observed yet|Observed \d+ mutation record/);

  await expect(reviewers).toHaveText(['Mara Lindqvist', 'Priya Anand']);
  await expect(message).toHaveText('No mutations observed yet');

  await addReviewer.click();
  await expect(reviewers).toHaveText(['Mara Lindqvist', 'Priya Anand', 'Jonas Petrov']);
  await expect.poll(() => readableText(message), 'the mutation observer reported').toMatch(/^Observed [1-9]\d* mutation record\(s\) at /);

  await addReviewer.click();
  await addReviewer.click();
  await expect(reviewers).toHaveText(['Mara Lindqvist', 'Priya Anand', 'Jonas Petrov', 'Tomás Ruiz', 'Elena Fischer']);
  await expect(addReviewer, 'the pool is used up').toHaveAttribute('disabled', '');

  await expectHealthy(page, problems);
});

test('content showcase: scrolling the changelog brings the sign-off card into view', async ({ page }) => {
  const problems = await openShowcase(page, ROUTE, TAGS);
  const signOff = page.getByText(/Sign-off card:/);
  const observer = page.locator('wa-intersection-observer');

  await expect.poll(() => readableText(signOff), 'the card starts out of view').toMatch(/^Sign-off card: out of view \(intersection ratio 0\.\d\d\)$/);
  // the observer's root is the viewport, so the card must be in the page's view and scrolled into the changelog's
  await observer.locator('wa-card').scrollIntoViewIfNeeded();
  await expect.poll(() => readableText(signOff), 'the card came into view')
    .toMatch(/^Sign-off card: visible \(intersection ratio (0\.[5-9]\d|1\.00)\)$/);

  await observer.evaluate(el => { /** @type {HTMLElement} */ (el.parentElement).scrollTop = 0; });
  await expect.poll(() => readableText(signOff), 'the card left the view').toMatch(/^Sign-off card: out of view /);

  await expectHealthy(page, problems);
});
