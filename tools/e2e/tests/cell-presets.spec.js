// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// The data grid cell presets of WebAwesome.Blazor.Extensions (WaDataGridFormatters, WaDataGridComparators) and the cell
// action bridge (@onwablazor-cell-action, docs\technical.md "JavaScript function references" and the Extensions
// PACKAGE.md). Pages: the harness /testing/cell-presets (every preset with fixed cultures and time zones, OnCellClick
// and @onwablazor-cell-action bound on the visual grid) and the "Extensions: Cell Presets" example of
// /components/data-grid. The registry test runs on the free CDN too; the grid tests need the Pro wa-data-grid and skip
// on the free CDN.

const HARNESS = '/testing/cell-presets';
const DATA_GRID_PAGE = '/components/data-grid';

// the names the Extensions initializer registers (PresetNames.cs)
const PRESET_NAMES = [
  'number', 'currency', 'percent', 'date', 'dateTime', 'time', 'relativeTime', 'bytes', 'duration', 'lookup', 'boolean',
  'template', 'list', 'badge', 'tag', 'tags', 'icon', 'link', 'actionLink', 'email', 'phone', 'ordinal',
].map(name => `wab.dataGrid.${name}`);

// Intl puts no-break spaces into some formats (cs-CZ groups, the space before AM/PM); the tests compare plain spaces
const normalize = (/** @type {string} */ text) => text.replace(/[  ]/g, ' ').replace(/\s+/g, ' ').trim();

/**
 * The texts of the data cells of a grid, by column id, in display order.
 *
 * @param {import('@playwright/test').Locator} grid
 * @returns {Promise<Record<string, string[]>>}
 */
async function cellTexts(grid) {
  const raw = await grid.evaluate(el => {
    /** @type {Record<string, string[]>} */
    const columns = {};
    const cells = [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-col-id]') ?? [])]
      .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex));
    for (const cell of cells) {
      const id = /** @type {HTMLElement} */ (cell).dataset.colId ?? '';
      (columns[id] ??= []).push(cell.textContent ?? '');
    }
    return columns;
  });
  return Object.fromEntries(Object.entries(raw).map(([id, texts]) => [id, texts.map(normalize)]));
}

/**
 * Opens a page with a Pro data grid and returns the grid once its columns have arrived.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} route
 * @param {string} selector
 * @param {number} columnCount
 */
async function openGrid(page, route, selector, columnCount) {
  await page.goto(route);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, ['wa-data-grid']);

  const grid = page.locator(selector);
  await expect.poll(() => grid.evaluate(el => /** @type {any} */ (el).columns?.length ?? 0), { message: 'the columns arrived', timeout: WA_READY_TIMEOUT_MS }).toBe(columnCount);
  return grid;
}

/**
 * The elements of one column's cells, described as "tag attr=value ... | text".
 *
 * @param {import('@playwright/test').Locator} grid
 * @param {string} columnId
 * @param {string} selector
 * @param {string[]} attributes
 */
function describeElements(grid, columnId, selector, attributes) {
  return grid.evaluate((el, { id, css, names }) => [...(el.shadowRoot?.querySelectorAll(`[part~="cell"][data-col-id="${id}"] ${css}`) ?? [])]
    .map(node => [node.localName, ...names.filter(name => node.hasAttribute(name)).map(name => `${name}=${node.getAttribute(name)}`), `upgraded=${!!node.shadowRoot || !node.localName.startsWith('wa-')}`, `| ${(node.textContent ?? '').trim()}`].join(' ')),
  { id: columnId, css: selector, names: attributes });
}

test('cell presets: the Extensions initializer registers every preset, and the presets build their DOM without a grid', async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(names => {
    const registry = /** @type {any} */ (globalThis)[Symbol.for('WebAwesome.Blazor.functions')];
    const missing = names.filter(name => typeof registry?.functions.get(name) !== 'function');
    const call = (/** @type {string} */ name, /** @type {any} */ args, /** @type {any} */ value, row = {}) =>
      registry.functions.get(`wab.dataGrid.${name}`)(value, row, { args, column: 'c', grid: document.body });

    const badge = call('badge', { variants: { active: 'success' }, labels: { active: 'Active' }, icons: { active: { name: 'check' } } }, 'active');
    const link = call('link', { href: '/orders/{id}?q={q}' }, 'Order', { id: 7, q: 'a&b c' });
    const blocked = call('link', { href: '{id}' }, 'x', { id: 'javascript:alert(1)' });
    const email = call('email', {}, 'a?cc=x@example.com');
    return {
      missing,
      text: call('currency', { culture: 'en-US', currency: 'USD' }, 12.5),
      badge: `${badge.localName} ${badge.getAttribute('variant')} ${badge.firstElementChild?.localName}/${badge.firstElementChild?.getAttribute('slot')} ${badge.textContent}`,
      sameNode: badge === call('badge', { variants: {} }, 'active'),
      link: `${link.getAttribute('href')} ${link.textContent}`,
      blocked: `${blocked.localName ?? 'text'} ${blocked.getAttribute?.('href')}`,
      email: email.getAttribute('href'),
      markup: call('lookup', { labels: {} }, '<img src=x onerror=alert(1)>'),
    };
  }, PRESET_NAMES);

  expect(result.missing, 'every preset is registered under its name').toEqual([]);
  expect(result.text).toBe('$12.50');
  expect(result.badge).toBe('wa-badge success wa-icon/start Active');
  expect(result.sameNode, 'a call without a row object makes a new node').toBe(false);
  expect(result.link, 'each placeholder value is URL-encoded').toBe('/orders/7?q=a%26b%20c Order');
  expect(result.blocked, 'an encoded value cannot choose the scheme: the URL is relative').toBe('a javascript%3Aalert(1)');
  expect(result.email, 'the parts of the address are encoded, so the value adds no header').toBe('mailto:a%3Fcc%3Dx@example.com');
  expect(result.markup, 'a text preset returns text, not markup').toBe('<img src=x onerror=alert(1)>');
});

test.describe('in a time zone west of UTC', () => {
  test.use({ timezoneId: 'America/Los_Angeles' });

  test('cell presets: text presets format the cells and the CSV export, and a DateOnly shows its calendar day', async ({ page }) => {
    const numbers = await openGrid(page, HARNESS, '[data-testid="cp-numbers"]', 8);

    const expectedNumbers = {
      amount: ['1,234.57', '1,234.57'],
      price: ['€1,499.50', '€1,499.50'],
      'local-price': ['1 499,50 Kč', '1 499,5'],
      share: ['25.6%', '25.6%'],
      due: ['10/15/2026', '10/15/2026'],
      created: ['10/15/2026, 10:30 AM', '10/15/2026, 10:30 AM'],
      'created-prague': ['12:30 PM', '12:30 PM'],
      opens: ['2:30 PM', '2:30 PM'],
    };
    await expect.poll(() => cellTexts(numbers)).toEqual(expectedNumbers);

    // the browser's own time zone would show the DateOnly 2026-10-15 (UTC midnight) as October 14
    expect(await page.evaluate(() => new Date('2026-10-15').getDate()), 'the test runs west of UTC').toBe(14);

    const texts = page.locator('[data-testid="cp-texts"]');
    const expectedTexts = {
      seen: ['3 hours ago', '3 hours ago'],
      size: ['1.54 MB', '1.54 MB'],
      spent: ['1 hr, 30 min', '1 hr, 30 min'],
      region: ['Europe', 'none'],
      active: ['Yes', 'unknown'],
      summary: ['Ada (3)', 'Grace (12)'],
      labels: ['a, b', 'none'],
    };
    await expect.poll(() => cellTexts(texts)).toEqual(expectedTexts);

    // string output reaches the CSV export: each formatted text is in the export of its grid
    for (const [grid, expected] of [[numbers, expectedNumbers], [texts, expectedTexts]]) {
      const csv = normalize(await /** @type {import('@playwright/test').Locator} */ (grid).evaluate(el => /** @type {any} */ (el).getDataAsCsv()));
      for (const text of Object.values(expected).flat()) expect(csv).toContain(text);
    }
  });
});

test('cell presets: visual presets render upgraded elements with accessible names, and links are safe', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cp-visual"]', 11);

  // the badges and tags in the cells are upgraded; nothing in the light DOM renders wa-tag (the sidebar renders badges),
  // so the tag preset's Uses declaration registered it
  expect(await page.evaluate(() => document.querySelectorAll('wa-tag').length)).toBe(0);
  await expect.poll(() => describeElements(grid, 'state', 'wa-badge', ['variant', 'pill'])).toEqual([
    'wa-badge variant=danger pill= upgraded=true | Blocked',
    'wa-badge variant=success pill= upgraded=true | Done',
    'wa-badge variant=neutral pill= upgraded=true | Open',
    'wa-badge variant=brand pill= upgraded=true | In progress',
  ]);
  expect(await describeElements(grid, 'state', 'wa-icon', ['name', 'slot'])).toEqual(['wa-icon name=ban slot=start upgraded=true | ']);

  expect(await describeElements(grid, 'kind', 'wa-tag', ['variant', 'size'])).toEqual([
    'wa-tag variant=danger size=s upgraded=true | bug',
    'wa-tag variant=brand size=s upgraded=true | feature',
    'wa-tag variant=brand size=s upgraded=true | feature',
    'wa-tag variant=danger size=s upgraded=true | bug',
  ]);
  expect((await cellTexts(grid)).labels).toEqual(['authurgent', 'ui', 'none', 'search']);
  expect(await describeElements(grid, 'labels', 'wa-tag', ['pill'])).toHaveLength(4);

  // icon-only output has an accessible name; a value without an icon shows its text
  expect(await describeElements(grid, 'health', 'wa-icon', ['name', 'label'])).toEqual([
    'wa-icon name=triangle-exclamation label=Degraded upgraded=true | ',
    'wa-icon name=circle-check label=Healthy upgraded=true | ',
    'wa-icon name=circle-check label=Healthy upgraded=true | ',
  ]);
  expect((await cellTexts(grid)).health[3]).toBe('unknown');
  expect(await describeElements(grid, 'paid', 'wa-icon', ['name', 'label'])).toEqual([
    'wa-icon name=xmark label=Not paid upgraded=true | ',
    'wa-icon name=check label=Paid upgraded=true | ',
    'wa-icon name=check label=Paid upgraded=true | ',
    'wa-icon name=xmark label=Not paid upgraded=true | ',
  ]);

  // links: URL-encoded placeholders, rel on a new tab, mailto and tel
  expect((await describeElements(grid, 'slug', 'a', ['href', 'target', 'rel']))[0])
    .toBe('a href=https://example.com/tickets/login-fails?from=Login%20fails target=_blank rel=noopener noreferrer upgraded=true | Login fails');
  expect(await describeElements(grid, 'email', 'a', ['href'])).toContain('a href=mailto:ada@example.com upgraded=true | ada@example.com');
  expect(await describeElements(grid, 'phone', 'a', ['href'])).toEqual([
    'a href=tel:+15550100001 upgraded=true | +1 (555) 010-0001',
    'a href=tel:+15550100002 upgraded=true | +1 555 010 0002',
    'a href=tel:5550103 upgraded=true | 555-0103',
    'a href=tel:5550104 upgraded=true | 555 0104',
  ]);
});

test('cell presets: an action link raises OnCellAction, not OnCellClick, by click and by Enter', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cp-visual"]', 11);
  await expect.poll(() => describeElements(grid, 'open', 'button', ['type'])).toHaveLength(4);

  // a page listener on the grid element itself still sees wa-cell-click (decision D4 stops it only for Blazor)
  await grid.evaluate(el => {
    /** @type {any} */ (window).__cellClicks = 0;
    el.addEventListener('wa-cell-click', () => { /** @type {any} */ (window).__cellClicks++; });
  });

  await grid.locator('button', { hasText: 'Open #101' }).click();
  await expect(page.getByTestId('cp-action')).toHaveText('open|open|101|0|101|Login fails|Blocked');
  await expect(page.getByTestId('cp-action-count')).toHaveText('1');
  expect(await page.evaluate(() => /** @type {any} */ (window).__cellClicks), 'the grid dispatched wa-cell-click').toBe(1);

  // a plain cell still raises OnCellClick, so the action click above did not
  await grid.locator('[part~="cell"][data-col-id="title"]').first().click();
  await expect(page.getByTestId('cp-click')).toHaveText('title|0');
  await expect(page.getByTestId('cp-click-count')).toHaveText('1');
  await expect(page.getByTestId('cp-action-count')).toHaveText('1');

  // keyboard: Enter on the focused button activates it; the grid lets Enter through to a button
  await grid.locator('button', { hasText: 'Open #102' }).focus();
  await page.keyboard.press('Enter');
  await expect(page.getByTestId('cp-action')).toHaveText('open|open|102|1|102|Dark mode|Done');
  await expect(page.getByTestId('cp-action-count')).toHaveText('2');
  await expect(page.getByTestId('cp-click-count')).toHaveText('1');
});

test('cell presets: an ordinal comparator sorts in its order, and a badge keeps its element when the grid renders again', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cp-visual"]', 11);
  await expect.poll(async () => (await cellTexts(grid)).state).toEqual(['Blocked', 'Done', 'Open', 'In progress']);

  // mark the badge of the "Done" row, then let the grid render all rows again by sorting
  await grid.evaluate(el => {
    const done = [...(el.shadowRoot?.querySelectorAll('[data-col-id="state"] wa-badge') ?? [])].find(badge => badge.textContent === 'Done');
    /** @type {any} */ (done).__marker = 'kept';
  });

  await grid.locator('[part~="header-cell"][data-col-id="state"]').click();
  await expect.poll(async () => (await cellTexts(grid)).state, { message: 'the workflow order, not the alphabetic one' })
    .toEqual(['Open', 'In progress', 'Blocked', 'Done']);
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="state"] wa-badge') ?? [])]
    .map(badge => /** @type {any} */ (badge).__marker ?? '-')), 'the same row and value give the same element').toEqual(['-', '-', '-', 'kept']);
});

test('cell presets: a relative link follows the router without a page load', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cp-visual"]', 11);
  await expect.poll(() => describeElements(grid, 'same-page', 'a', ['href'])).toContain('a href=testing/cell-presets?ticket=103 upgraded=true | Ticket 103');
  await page.evaluate(() => { /** @type {any} */ (window).__sameDocument = true; });

  await grid.locator('a', { hasText: 'Ticket 103' }).click();
  await expect(page).toHaveURL(/\/testing\/cell-presets\?ticket=103$/);
  expect(await page.evaluate(() => /** @type {any} */ (window).__sameDocument), 'the link did not load the page again').toBe(true);
});

test('cell presets: the demo example shows state badges and receives the order action', async ({ page }) => {
  const grid = await openGrid(page, DATA_GRID_PAGE, 'wa-data-grid[label="Orders"]:not([server])', 8);

  await expect.poll(() => describeElements(grid, 'state', 'wa-badge', ['variant'])).toEqual([
    'wa-badge variant=success upgraded=true | Shipped',
    'wa-badge variant=brand upgraded=true | Processing',
    'wa-badge variant=neutral upgraded=true | New',
    'wa-badge variant=danger upgraded=true | Cancelled',
    'wa-badge variant=brand upgraded=true | Processing',
  ]);
  expect(await describeElements(grid, 'paid', 'wa-icon', ['label'])).toEqual([
    'wa-icon label=Paid upgraded=true | ', 'wa-icon label=Not paid upgraded=true | ', 'wa-icon label=Not paid upgraded=true | ',
    'wa-icon label=Paid upgraded=true | ', 'wa-icon label=Paid upgraded=true | ',
  ]);

  await grid.locator('button', { hasText: '#1002' }).click();
  await expect(page.getByTestId('preset-cell-action')).toHaveText('Action "open" for order 1002 (display row 1): Grace Hopper, Processing.');
  await expect(page.getByTestId('preset-cell-click')).toHaveText('Click any other cell to raise OnCellClick.');
});
