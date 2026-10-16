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
  'template', 'list', 'badge', 'tag', 'tags', 'icon', 'link', 'actionLink', 'email', 'phone', 'ordinal', 'button', 'actions',
  'menu', 'switch', 'checkbox', 'progressBar', 'progressRing', 'rating', 'avatar', 'image', 'colorSwatch', 'sparkline', 'trend',
  'dataBar', 'copyButton', 'collation', 'cellClassRules', 'rowClassRules', 'footerAggregate', 'searchFormatted', 'detailFields',
  'selectableField',
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

test('cell presets: the phase 2 presets build their DOM, rules, footers, search and sort without a grid', async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(() => {
    const registry = /** @type {any} */ (globalThis)[Symbol.for('WebAwesome.Blazor.functions')];
    const preset = (/** @type {string} */ name) => registry.functions.get(`wab.dataGrid.${name}`);
    const grid = document.createElement('div');
    const ctx = (/** @type {any} */ args, extra = {}) => ({ args, column: 'c', grid, ...extra });
    const describe = (/** @type {any} */ node) => typeof node === 'string' ? `text:${node}`
      : `${node.localName}[${[...node.attributes].filter(a => a.name !== 'style').map(a => `${a.name}=${a.value}`).join(' ')}]${node.textContent ? `"${node.textContent}"` : ''}`;
    const row = { id: 1, locked: true, name: 'Ada', photo: 'javascript:alert(1)', trend: 'negative', canSelect: false };

    const rules = { rules: [
      { class: 'null', when: 'isNull' },
      { class: 'eq', when: 'equals', value: '2' },
      { class: 'in', when: 'in', values: ['a', 'b'] },
      { class: 'lt', when: 'lessThan', value: 0, type: 'number' },
      { class: 'between', when: 'between', min: 10, max: 20, type: 'number' },
      { class: 'gt-date', when: 'greaterThan', value: '2026-01-01', type: 'date' },
      { class: 'true', when: 'isTrue' },
      { class: 'false', when: 'isFalse' },
      { class: 'match', when: 'matches', pattern: '^x', ignoreCase: true },
    ] };
    const cellClass = (/** @type {any} */ value) => preset('cellClassRules')(value, {}, ctx(rules));
    const footerGrid = { columns: [{ field: 'amount' }, { id: 'double', value: (/** @type {any} */ r) => r.amount * 2 }] };
    const rows = [{ amount: 1 }, { amount: 2.5 }, { amount: 'n/a' }, { amount: 4 }];
    const footer = (/** @type {string} */ aggregate, column = 'amount', extra = {}) =>
      preset('footerAggregate')(rows, { args: { aggregate, culture: 'en-US', ...extra }, column, grid: footerGrid });
    const search = (/** @type {any} */ value, /** @type {string} */ term, /** @type {any} */ searchRow) => preset('searchFormatted')(value, term, searchRow,
      ctx({ columns: [{ field: 'region', labels: { PA: 'Pardubice' } }, { field: 'tags', labels: { u: 'Urgent' } }] }));
    const collation = ctx({ culture: 'cs-CZ' });
    const names = ['Zlín', 'Chrudim', 'Hlinsko', null, 'Cvikov'].sort((a, b) => preset('collation')(a, b, {}, {}, collation));

    return {
      button: describe(preset('button')(5, { id: 5 }, ctx({ action: 'edit', label: 'Edit #{id}', icon: { name: 'pen' }, size: 's', culture: 'en-US' }))),
      disabled: describe(preset('button')(5, row, ctx({ action: 'edit', disabledField: 'locked' }))),
      hidden: preset('button')(5, row, ctx({ action: 'edit', hiddenField: 'locked', nullText: '-' })),
      actions: [...preset('actions')(1, row, ctx({ items: [{ id: 'view', label: 'View', icon: { name: 'eye' } }, { id: 'delete', label: 'Delete', hiddenField: 'locked' }, { id: 'more', label: 'More', inMenu: true }], menuLabel: 'Menu' })).children].map(describe),
      menu: [...preset('menu')(1, row, ctx({ items: [{ id: 'open', label: 'Open' }, { id: 'remove', label: 'Remove', variant: 'danger', disabledField: 'locked' }], menuLabel: 'Menu' })).children].map(describe),
      toggle: describe(preset('switch')(true, row, ctx({ label: 'Active: {name}', disabledField: 'locked', culture: 'en-US' }))),
      progress: describe(preset('progressBar')(0.85, {}, ctx({ culture: 'en-US', thresholds: [{ from: 0.8, variant: 'success' }] }))),
      rating: describe(preset('rating')(3.5, {}, ctx({ max: 5, precision: 0.5, culture: 'en-US' }))),
      avatar: describe(preset('avatar')('Ada', row, ctx({ imageField: 'photo' }))),
      image: preset('image')('javascript:alert(1)', row, ctx({ nullText: '-' })),
      imageTemplate: describe(preset('image')(null, { id: 'a b' }, ctx({ src: '/photos/{id}.png', alt: 'Photo {id}' }))),
      swatch: describe(preset('colorSwatch')('red', {}, ctx({}))),
      noColor: preset('colorSwatch')('url(javascript:x)', {}, ctx({})),
      sparkline: describe(preset('sparkline')([1, '2', 'x', 3], row, ctx({ trendField: 'trend', culture: 'en-US' }))),
      trend: describe(preset('trend')(-0.5, {}, ctx({ culture: 'en-US', negativeVariant: 'danger', downIcon: { name: 'arrow-trend-down' } }))),
      dataBar: describe(preset('dataBar')(25, {}, ctx({ min: 0, max: 100, culture: 'en-US' }))),
      copy: describe(preset('copyButton')('KEY-1', {}, ctx({}))),
      classes: [null, 2, 'b', -1, 15, '2026-06-01', true, false, 'Xyz', 'other'].map(cellClass),
      rowClass: preset('rowClassRules')({ state: 'b' }, ctx({ rules: [{ class: 'r', when: 'in', values: ['b'], field: 'state' }] })),
      footers: [footer('sum'), footer('average'), footer('min'), footer('max'), footer('count'), footer('sum', 'double', { label: 'Total' }),
        footer('sum', 'amount', { format: { name: 'wab.dataGrid.currency', args: { culture: 'en-US', currency: 'EUR' } } })],
      search: [search('x', 'pard', { region: 'PA' }), search('Ada', 'ad', {}), search('x', 'urg', { tags: ['u'] }), search('x', 'zzz', { region: 'PA' })],
      names,
      detail: [...preset('detailFields')({ name: 'Ada', due: '2026-10-15' }, ctx({ nullText: '-', fields: [{ label: 'Name', field: 'name' }, { label: 'Due', field: 'due', format: { name: 'wab.dataGrid.date', args: { culture: 'en-US' } } }, { label: 'Note', field: 'note' }] })).children].map(node => node.textContent),
      selectable: [preset('selectableField')(row, ctx({ field: 'canSelect' })), preset('selectableField')({ canSelect: true }, ctx({ field: 'canSelect' }))],
    };
  });

  expect(result.button).toBe('wa-button[size=s]"Edit #5"');
  expect(result.disabled).toBe('wa-button[disabled=]"5"');
  expect(result.hidden).toBe('-');
  expect(result.actions, 'an icon action shows its icon with the label as the name; a hidden action is left out').toEqual([
    'wa-button[]', 'wa-dropdown[]"More"',
  ]);
  expect(result.menu).toEqual(['wa-button[slot=trigger]', 'wa-dropdown-item[value=open]"Open"', 'wa-dropdown-item[value=remove variant=danger disabled=]"Remove"']);
  expect(result.toggle, 'the label is the accessible name').toBe('wa-switch[checked= disabled=]"Active: Ada"');
  expect(result.progress).toBe('wa-progress-bar[value=85 label=85%]');
  expect(result.rating).toBe('wa-rating[readonly= value=3.5 max=5 precision=0.5 label=3.5 / 5]');
  expect(result.avatar, 'an image URL with another scheme is left out').toBe('wa-avatar[label=Ada]');
  expect(result.image).toBe('-');
  expect(result.imageTemplate, 'the placeholder value is URL-encoded').toBe('img[src=/photos/a%20b.png alt=Photo a b loading=lazy]');
  expect(result.swatch).toBe('span[role=img aria-label=red]');
  expect(result.noColor, 'a value that is not a CSS color shows as text').toBe('url(javascript:x)');
  expect(result.sparkline).toBe('wa-sparkline[data=1 2 3 label=1, 2, 3 trend=negative]');
  expect(result.trend).toBe('span[]"-0.5"');
  expect(result.dataBar).toBe('div[role=img aria-label=25]');
  expect(result.copy).toBe('wa-copy-button[value=KEY-1]');
  expect(result.classes, 'the first rule that matches gives the class').toEqual(['null', 'eq', 'in', 'lt', 'between', 'gt-date', 'true', 'false', 'match', '']);
  expect(result.rowClass).toBe('r');
  expect(result.footers, 'values that are not numbers are left out; a value function counts').toEqual(['7.5', '2.5', '1', '4', '4', 'Total 15', '€7.50']);
  expect(result.search).toEqual([true, true, true, false]);
  expect(result.names, 'the Czech order: "ch" after "h"').toEqual([null, 'Cvikov', 'Hlinsko', 'Chrudim', 'Zlín']);
  expect(result.detail).toEqual(['Name', 'Ada', 'Due', '10/15/2026', 'Note', '-']);
  expect(result.selectable).toEqual([false, true]);
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

test('cell presets: progress, rating, avatar, image, color, sparkline, trend, data bar and copy presets render upgraded elements', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="cp-more"]', 11);

  await expect.poll(() => describeElements(grid, 'progress', 'wa-progress-bar', ['value', 'label'])).toEqual([
    'wa-progress-bar value=25 label=25% upgraded=true | 25%',
    'wa-progress-bar value=90 label=90% upgraded=true | 90%',
    'wa-progress-bar value=50 label=50% upgraded=true | 50%',
  ]);
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="progress"] wa-progress-bar') ?? [])]
    .map(bar => /** @type {HTMLElement} */ (bar).style.getPropertyValue('--indicator-color'))), 'the threshold colors')
    .toEqual(['var(--wa-color-warning-fill-loud)', 'var(--wa-color-success-fill-loud)', 'var(--wa-color-warning-fill-loud)']);
  expect(await describeElements(grid, 'ring', 'wa-progress-ring', ['value', 'label'])).toHaveLength(3);
  expect(await describeElements(grid, 'score', 'wa-rating', ['value', 'readonly', 'label'])).toEqual([
    'wa-rating value=3 readonly= label=3 / 5 upgraded=true | ', 'wa-rating value=5 readonly= label=5 / 5 upgraded=true | ',
    'wa-rating value=4 readonly= label=4 / 5 upgraded=true | ',
  ]);
  expect(await describeElements(grid, 'owner', 'wa-avatar', ['label', 'image', 'initials', 'shape'])).toEqual([
    'wa-avatar label=Ada Lovelace image=fragments/icon-sprite.svg initials=AL shape=rounded upgraded=true | ',
    'wa-avatar label=Grace Hopper initials=GH shape=rounded upgraded=true | ',
    'wa-avatar label=Alan Turing shape=rounded upgraded=true | ',
  ]);
  expect((await cellTexts(grid)).owner, 'the name shows next to the avatar').toEqual(['Ada Lovelace', 'Grace Hopper', 'Alan Turing']);
  expect(await describeElements(grid, 'image', 'img', ['src', 'alt', 'width'])).toEqual(['img src=fragments/icon-sprite.svg alt=Photo of Ada Lovelace width=24 upgraded=true | ']);
  expect(await describeElements(grid, 'color', 'span > span:first-child', [])).toHaveLength(2);
  expect((await cellTexts(grid)).color).toEqual(['#ff8800', 'rebeccapurple', 'not a color']);
  expect(await describeElements(grid, 'sales', 'wa-sparkline', ['data', 'trend', 'label'])).toEqual([
    'wa-sparkline data=3 5 4 8 trend=positive label=3, 5, 4, 8 upgraded=true | ',
    'wa-sparkline data=8 6 5 2 trend=negative label=8, 6, 5, 2 upgraded=true | ',
  ]);
  expect((await cellTexts(grid)).change, 'the trend text with its format').toEqual(['+12.5%', '-5%', '0%']);
  expect(await describeElements(grid, 'change', 'wa-icon', ['name'])).toEqual(['wa-icon name=arrow-trend-up upgraded=true | ', 'wa-icon name=arrow-trend-down upgraded=true | ']);
  expect(await describeElements(grid, 'budget', 'div[role="img"]', ['aria-label'])).toEqual([
    'div aria-label=€1,200.00 upgraded=true | €1,200.00', 'div aria-label=€300.00 upgraded=true | €300.00', 'div aria-label=€600.00 upgraded=true | €600.00',
  ]);
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="budget"] div[role="img"] > div > div') ?? [])]
    .map(bar => /** @type {HTMLElement} */ (bar).style.inlineSize)), 'the limits come from the loaded rows').toEqual(['100%', '25%', '50%']);
  expect(await describeElements(grid, 'key', 'wa-copy-button', ['value'])).toEqual([
    'wa-copy-button value=KEY-ATLAS upgraded=true | ', 'wa-copy-button value=KEY-BOREALIS upgraded=true | ', 'wa-copy-button value=KEY-CIRRUS upgraded=true | ',
  ]);

  // a click on the copy button does not raise OnCellClick
  const clicks = await page.getByTestId('cp-click-count').textContent();
  await grid.locator('wa-copy-button').first().click();
  await grid.locator('[part~="cell"][data-col-id="name"]').first().click();
  await expect(page.getByTestId('cp-click')).toHaveText('name|0');
  await expect(page.getByTestId('cp-click-count')).toHaveText(String(Number(clicks) + 1));
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
