// @ts-check
const fs = require('fs');
const { test, expect, openHarness, openPanel, selectByTestId, sessionOf, tabIdOf } = require('./fixtures');

// The review side of the panel on the harness page: a comment on a plain element picked on the page, the numbered
// outlines of the change-list items, and the export (JSON and Markdown, the files in Downloads\wa-review).

// the first bytes of every PNG file
const PNG_SIGNATURE = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);

/**
 * Calls a test hook of the panel (window.__waInspectorPanel) with JSON arguments.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} method
 * @param {any[]} [args]
 */
async function callPanel(panel, method, args = []) {
  return panel.evaluate(({ name, values }) => /** @type {any} */ (window).__waInspectorPanel[name](...values), { name: method, values: args });
}

/**
 * The item numbers of the numbered outlines the page draws (content.js test hook page.drawnMarkers), sorted.
 *
 * @param {import('@playwright/test').Page} panel
 */
async function drawnMarkers(panel) {
  const numbers = await callPanel(panel, 'request', ['page.drawnMarkers']);
  return [...numbers].sort((a, b) => a - b);
}

test('a comment on a plain element picked on the page is stored', async ({ page, context, serviceWorker, extensionId }) => {
  await openHarness(page);
  const panel = await openPanel({ context, serviceWorker, extensionId }, page);

  // pick mode: the next click on the page selects the element under the pointer instead of acting on it
  await panel.locator('#pick').click();
  await expect(panel.locator('#pick')).toHaveClass(/active/);
  await page.getByTestId('ih-intro').click();
  await expect(panel.locator('#pick')).not.toHaveClass(/active/);
  await expect(panel.locator('#details h2')).toHaveText('<p>');

  const comment = panel.locator('#details textarea[placeholder^="What should change here"]');
  await comment.fill('Shorten this sentence.');
  await comment.blur();

  await expect.poll(async () => (await sessionOf(panel)).items.length).toBe(1);
  const [item] = (await sessionOf(panel)).items;
  expect(item).toMatchObject({ number: 1, scope: 'placement', comment: 'Shorten this sentence.' });
  expect(item.target).toMatchObject({ tag: 'p', placement: null, component: null });
  expect(item.target.text).toContain('end-to-end test harness');
  expect(item.target.ownerComponent, 'a plain element in a page has no wrapper as its owner').toBeFalsy();

  const statusOf = async () => (await callPanel(panel, 'request', ['page.itemStatus']))[item.id];

  // the comment survives a reload and still finds its element
  await page.reload();
  await openHarness(page);
  await expect.poll(statusOf).toEqual({ matches: 1, status: 'unchecked' });

  // a color scheme switch (classes on <html>) keeps the match of a plain element
  await page.evaluate(() => /** @type {any} */ (window).demoTheme.setDark(true));
  await callPanel(panel, 'request', ['page.apply']);
  expect(await statusOf()).toEqual({ matches: 1, status: 'unchecked' });
});

test('the markers draw one numbered outline per matched element', async ({ page, context, serviceWorker, extensionId }) => {
  await openHarness(page);
  const panel = await openPanel({ context, serviceWorker, extensionId }, page);

  // item 1: the three loop badges; item 2: the card
  await selectByTestId(panel, 'ih-badge');
  await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(() => {}));
  await selectByTestId(panel, 'ih-card');
  await panel.locator('#details button', { hasText: 'Add marker' }).click();

  await expect.poll(async () => (await sessionOf(panel)).items.map(item => item.number)).toEqual([1, 2]);
  await expect.poll(() => drawnMarkers(panel)).toEqual([1, 1, 1, 2]);

  // the tree shows the item numbers on the matched rows
  await expect(panel.locator('.node-row .item-badge', { hasText: /^1$/ })).toHaveCount(3);
  await expect(panel.locator('.node-row .item-badge', { hasText: /^2$/ })).toHaveCount(1);

  // the Markers switch hides and shows the outlines
  await panel.locator('#markers').uncheck();
  await expect.poll(() => drawnMarkers(panel)).toEqual([]);
  expect((await sessionOf(panel)).markersVisible).toBe(false);
  await panel.locator('#markers').check();
  await expect.poll(() => drawnMarkers(panel)).toEqual([1, 1, 1, 2]);

  // a deleted item loses its outlines; the next item keeps counting
  const [first] = (await sessionOf(panel)).items;
  await panel.locator('.tab[data-tab="changes"]').click();
  await panel.locator('.change-item').filter({ has: panel.locator('.item-badge', { hasText: /^1$/ }) }).getByRole('button', { name: 'Delete' }).click();
  await expect.poll(() => drawnMarkers(panel)).toEqual([2]);
  expect((await sessionOf(panel)).items.find(item => item.id === first.id)).toBeUndefined();
  await panel.locator('.tab[data-tab="elements"]').click();
  await selectByTestId(panel, 'ih-toggle');
  await panel.locator('#details button', { hasText: 'Add marker' }).click();
  await expect.poll(() => drawnMarkers(panel)).toEqual([2, 3]);
});

test('the export builds the change list as JSON and Markdown and saves the files', async ({ page, context, serviceWorker, extensionId }) => {
  await openHarness(page);
  const panel = await openPanel({ context, serviceWorker, extensionId }, page);

  // item 1: a remap on the loop badges, item 2: a CSS custom property and a comment on the card, a theme token
  const badge = await selectByTestId(panel, 'ih-badge');
  await panel.evaluate(observed => /** @type {any} */ (window).__waInspectorPanel.editItem(item => {
    item.attributes.variant = { remap: { neutral: 'warning' }, from: 'neutral', dataDriven: true, observed };
  }), badge.observed.variant);
  await selectByTestId(panel, 'ih-card');
  await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => {
    item.cssProperties['--spacing'] = '2rem';
    item.comment = 'More room inside the card.';
  }));
  await panel.evaluate(async () => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    const key = hooks.state.info.storageKey;
    const session = structuredClone(hooks.state.session);
    session.theme = { '--wa-color-brand-fill-loud': '#123456' };
    await chrome.storage.local.set({ [key]: session });
  });
  await expect.poll(async () => (await sessionOf(panel)).theme).toEqual({ '--wa-color-brand-fill-loud': '#123456' });

  const { data, markdown } = await panel.evaluate(async () => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    const info = await hooks.request('page.info');
    const status = await hooks.request('page.itemStatus');
    const built = hooks.buildExport(info, status, { page: 'page.png' });
    return { data: built, markdown: hooks.buildMarkdown(built) };
  });

  expect(data.schema).toBe('wa-inspector/change-list@1');
  expect(data.page).toMatchObject({ formsVersion: '3.14.0', blazorMetadata: true });
  expect(data.page.url).toMatch(/\/testing\/inspector$/);
  expect(data.theme).toEqual([{ token: '--wa-color-brand-fill-loud', from: 'var(--wa-color-brand-50)', to: '#123456' }]);
  expect(data.items).toHaveLength(2);
  expect(data.items[0]).toMatchObject({
    number: 1, color: 'red', scope: 'placement', matchCount: 3,
    target: { tag: 'wa-badge', component: 'WaBadge' },
    changes: [{ kind: 'attribute', name: 'variant', operation: 'remap', remap: { neutral: 'warning' }, dataDriven: true }],
  });
  expect(data.items[0].target.chain).toContain('InspectorHarness');
  expect(data.items[1]).toMatchObject({
    number: 2, color: 'green', matchCount: 1, comment: 'More room inside the card.',
    target: { tag: 'wa-card', component: 'WaCard' },
    changes: [{ kind: 'cssProperty', name: '--spacing', value: '2rem' }],
  });

  expect(markdown).toContain('# WA review: ');
  expect(markdown).toContain('- Blazor placement metadata: yes');
  expect(markdown).toContain('- Screenshot: page.png');
  expect(markdown).toContain('| `--wa-color-brand-fill-loud` | `var(--wa-color-brand-50)` | `#123456` |');
  expect(markdown).toContain('## Item 1 (red): <wa-badge>');
  expect(markdown).toContain('- attribute `variant`: data-driven, remap `neutral` → `warning`');
  expect(markdown).toContain('  - values seen: `neutral`×2, `success`×1');
  expect(markdown).toContain('## Item 2 (green): <wa-card>');
  expect(markdown).toContain('- CSS custom property `--spacing`: `2rem`');
  expect(markdown).toContain('> More room inside the card.');

  // the export saves the change list and the screenshots under Downloads\wa-review\<timestamp>. Playwright saves the
  // downloads of its browser under names of its own, so the service worker records the file names the extension asks
  // for, and the test reads the saved files by their download ids
  // for the screenshots, the service worker records the active tab of the window at each capture
  await serviceWorker.evaluate(() => {
    const scope = /** @type {any} */ (self);
    if (scope.__waiRequestedDownloads) return;
    scope.__waiRequestedDownloads = [];
    scope.__waiCapturedTabs = [];
    const download = chrome.downloads.download.bind(chrome.downloads);
    chrome.downloads.download = async options => {
      const id = await download(options);
      scope.__waiRequestedDownloads.push({ id, filename: options.filename });
      return id;
    };
    const captureVisibleTab = chrome.tabs.captureVisibleTab.bind(chrome.tabs);
    chrome.tabs.captureVisibleTab = async (windowId, options) => {
      const [active] = await chrome.tabs.query({ active: true, windowId });
      scope.__waiCapturedTabs.push(active ? active.id : null);
      return captureVisibleTab(windowId, options);
    };
  });

  const result = await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.exportChangeList());
  expect(result, 'the export finished').toBeTruthy();

  // a panel in a tab of the page's window: the export shows the page for the screenshots, then the panel again
  const tabId = await tabIdOf(serviceWorker, page);
  const panelTabId = await tabIdOf(serviceWorker, panel);
  expect(await serviceWorker.evaluate(() => /** @type {any} */ (self).__waiCapturedTabs), 'every screenshot shows the page').toEqual([tabId, tabId, tabId]);
  expect(await serviceWorker.evaluate(id => chrome.tabs.get(id).then(tab => tab.active), panelTabId), 'the panel tab is active again').toBe(true);
  expect(result.folder).toMatch(/^wa-review\/\d{4}-\d{2}-\d{2}_\d{6}$/);
  expect(result.data.screenshot).toBe('page.png');
  expect(result.data.items.map(item => item.screenshot)).toEqual(['item-01.png', 'item-02.png']);

  const downloads = await serviceWorker.evaluate(() => /** @type {any} */ (self).__waiRequestedDownloads);
  expect(downloads.map(item => item.filename)).toEqual(['changes.json', 'changes.md', 'page.png', 'item-01.png', 'item-02.png']
    .map(name => `${result.folder}/${name}`));

  /** @type {Record<string, { path: string, state: string }>} */
  let saved = {};
  await expect.poll(async () => {
    saved = await serviceWorker.evaluate(async requested => {
      const files = {};
      for (const { id, filename } of requested) {
        const [item] = await chrome.downloads.search({ id });
        files[filename.split('/').pop()] = { path: item ? item.filename : '', state: item ? item.state : 'missing' };
      }
      return files;
    }, downloads);
    return Object.values(saved).map(file => file.state);
  }, { timeout: 15_000 }).toEqual(['complete', 'complete', 'complete', 'complete', 'complete']);

  const savedJson = JSON.parse(fs.readFileSync(saved['changes.json'].path, 'utf8'));
  expect(savedJson.schema).toBe('wa-inspector/change-list@1');
  expect(savedJson.items.map(item => item.number)).toEqual([1, 2]);
  expect(savedJson.items[1].comment).toBe('More room inside the card.');
  const savedMarkdown = fs.readFileSync(saved['changes.md'].path, 'utf8');
  expect(savedMarkdown).toContain('- Screenshot: page.png');
  expect(savedMarkdown).toContain('- attribute `variant`: data-driven, remap `neutral` → `warning`');
  for (const name of ['page.png', 'item-01.png', 'item-02.png']) {
    const bytes = fs.readFileSync(saved[name].path);
    expect(bytes.subarray(0, PNG_SIGNATURE.length).equals(PNG_SIGNATURE), `${name} is a PNG`).toBe(true);
    await test.info().attach(name, { path: saved[name].path, contentType: 'image/png' });  }
});
