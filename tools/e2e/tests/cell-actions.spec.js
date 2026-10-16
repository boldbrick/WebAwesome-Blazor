// @ts-check
const { test, expect } = require('./helpers/test');
const { skipUnlessProUpgrades, WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');

// The phase 2 extras of WebAwesome.Blazor.Extensions (docs\technical.md, "Cell presets and cell actions (Extensions)"):
// the ctx helpers (ctx.emit, ctx.h, ctx.preset, ctx.format, ctx.cache), the declarative actions (data-wablazor-action),
// the interactive presets (Button, Actions, Menu, Switch, Checkbox), decision D4 for every action, the risks R2 (a Lit
// template of the app's own Lit), R4 (the keyboard in an action cell) and R6 (a menu in a pinned column and in the last
// row), and the rule and member presets (class rules, footers, collation, formatted search, filter options, detail
// fields, selectable field). Pages: the harnesses /testing/cell-actions and /testing/lit-cells, and the
// "Extensions: Actions & Rules" example of /components/data-grid. The helper test runs on the free CDN too; the grid
// tests need the Pro wa-data-grid and skip on the free CDN.

const HARNESS = '/testing/cell-actions';
const LIT_HARNESS = '/testing/lit-cells';
const DATA_GRID_PAGE = '/components/data-grid';
const CONTEXT_MODULE = '/_content/WebAwesome.Blazor.Extensions/data-grid-context.js';

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
 * The texts of one column's data cells, in display order.
 *
 * @param {import('@playwright/test').Locator} grid
 * @param {string} columnId
 * @returns {Promise<string[]>}
 */
function columnTexts(grid, columnId) {
  return grid.evaluate((el, id) => [...(el.shadowRoot?.querySelectorAll(`[part~="cell"][data-col-id="${id}"]`) ?? [])]
    .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex))
    .map(cell => (cell.textContent ?? '').replace(/\s+/g, ' ').trim()), columnId);
}

/**
 * The classes of one column's data cells (without the grid's own "cell" class), in display order.
 *
 * @param {import('@playwright/test').Locator} grid
 * @param {string} columnId
 * @returns {Promise<string[]>}
 */
function columnClasses(grid, columnId) {
  return grid.evaluate((el, id) => [...(el.shadowRoot?.querySelectorAll(`[part~="cell"][data-col-id="${id}"]`) ?? [])]
    .sort((a, b) => Number(/** @type {HTMLElement} */ (a).dataset.rowIndex) - Number(/** @type {HTMLElement} */ (b).dataset.rowIndex))
    .map(cell => [...cell.classList].filter(name => name !== 'cell').join(' ')), columnId);
}

/**
 * The texts of the footer cells, by column id.
 *
 * @param {import('@playwright/test').Locator} grid
 * @returns {Promise<Record<string, string>>}
 */
function footerTexts(grid) {
  return grid.evaluate(el => Object.fromEntries([...(el.shadowRoot?.querySelectorAll('[part~="footer-cell"]') ?? [])]
    .map(cell => [/** @type {HTMLElement} */ (cell).dataset.colId ?? '', (cell.textContent ?? '').replace(/\s+/g, ' ').trim()])));
}

/**
 * What the tickets grid of the harness received: the last action, the last cell click and both counts.
 *
 * @param {import('@playwright/test').Page} page
 */
async function received(page) {
  return {
    action: await page.getByTestId('ca-action').textContent(),
    click: await page.getByTestId('ca-click').textContent(),
    actions: Number(await page.getByTestId('ca-action-count').textContent()),
    clicks: Number(await page.getByTestId('ca-click-count').textContent()),
  };
}

/**
 * Opens the tickets grid of the harness once its interactive cells have rendered.
 *
 * @param {import('@playwright/test').Page} page
 */
async function openTickets(page) {
  const grid = await openGrid(page, HARNESS, '[data-testid="ca-tickets"]', 8);
  await expect.poll(() => grid.evaluate(el => el.shadowRoot?.querySelectorAll('[data-col-id="menu"] wa-dropdown').length ?? 0)).toBe(5);
  await expect(grid.locator('wa-button', { hasText: 'Edit #1' })).toBeVisible();
  return grid;
}

test('ctx helpers: h, preset, format, cache and emit work without a grid, and h never parses HTML', async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');

  const result = await page.evaluate(async url => {
    const { decorateContext } = await import(url);
    const callArguments = Symbol.for('WebAwesome.Blazor.callArguments');

    // a grid element (not upgraded on the free CDN) and a ctx of a formatter call, as the resolver builds it
    const grid = document.createElement('wa-data-grid');
    /** @type {any} */ (grid).rowKey = 'id';
    document.body.append(grid);
    const proto = Object.create({ args: { culture: 'en-US' }, element: grid, grid, column: 'state', rowKey: 7 });
    decorateContext(proto, { element: grid, member: 'formatter', label: 'test', args: { culture: 'en-US' }, rowArgument: 1 });
    const row = { id: 7, state: 'active', amount: 12.5 };
    const ctx = Object.create(proto);
    ctx[callArguments] = ['active', row];

    /** @type {any[]} */
    const events = [];
    grid.addEventListener('wablazor-cell-action', event => events.push(/** @type {CustomEvent} */ (event).detail));

    const listened = [];
    const element = ctx.h('div', { class: 'box', title: 'T', hidden: false, 'data-x': 3, onClick: () => listened.push('click') },
      'a', ['b', null, false], ctx.h('span', {}, '<b>c</b>'));
    element.click();
    const blocked = ctx.h('a', { href: 'javascript:alert(1)', innerHTML: '<b>x</b>', onclick: 'alert(1)' }, 'link');
    const allowed = ctx.h('a', { href: '/orders/7' }, 'link');
    const action = ctx.h('button', { action: 'approve', payload: { level: 2 } }, 'Approve');
    action.click();
    ctx.emit('custom', { via: 'emit' });

    const badge = ctx.preset('badge', { variants: { active: 'success' } });

    // the node cache: one render (a task) never gets one node twice; a later render gets the kept node
    const first = ctx.cache('k', () => document.createElement('i'));
    const sameRender = ctx.cache('k', () => document.createElement('i'));
    await new Promise(resolve => setTimeout(resolve));
    const laterRender = ctx.cache('k', () => document.createElement('i'));
    grid.remove();
    return {
      element: element.outerHTML,
      listened,
      blocked: blocked.outerHTML,
      allowed: allowed.getAttribute('href'),
      events,
      badge: `${badge.localName} ${badge.getAttribute('variant')} ${badge.textContent}`,
      currency: ctx.format.currency(1234.5, { currency: 'EUR' }),
      number: ctx.format.number(0.5, { maxFractionDigits: 2, culture: 'de-DE' }),
      date: ctx.format.date('2026-10-15', { dateStyle: 'long' }),
      cached: `${first === sameRender ? 'same' : 'new'} in one render, ${first === laterRender ? 'kept' : 'new'} later`,
    };
  }, CONTEXT_MODULE);

  expect(result.element, 'props are attributes, text children are text nodes').toBe('<div class="box" title="T" data-x="3">ab<span>&lt;b&gt;c&lt;/b&gt;</span></div>');
  expect(result.listened, 'an on* function is a listener').toEqual(['click']);
  expect(result.blocked, 'an unsafe URL, an HTML prop and a text handler are left out').toBe('<a>link</a>');
  expect(result.allowed).toBe('/orders/7');
  expect(result.events).toEqual([
    { column: 'state', action: 'approve', rowKey: 7, row: { id: 7, state: 'active', amount: 12.5 }, value: 'active', payload: { level: 2 }, rowIndex: -1 },
    { column: 'state', action: 'custom', rowKey: 7, row: { id: 7, state: 'active', amount: 12.5 }, value: 'active', payload: { via: 'emit' }, rowIndex: -1 },
  ]);
  expect(result.badge, 'ctx.preset formats the value of the call').toBe('wa-badge success active');
  expect(result.currency, 'ctx.format takes the culture of ctx.args').toBe('€1,234.50');
  expect(result.number, 'an option culture wins').toBe('0,5');
  expect(result.date, 'a DateOnly is its calendar day').toBe('October 15, 2026');
  expect(result.cached).toBe('new in one render, kept later');
});

test('interactive presets: buttons, actions, a menu, a switch and a checkbox raise their actions, not OnCellClick', async ({ page }) => {
  const grid = await openTickets(page);

  // a page listener on the grid element still sees wa-cell-click (D4 stops it only for Blazor)
  await grid.evaluate(el => {
    /** @type {any} */ (window).__cellClicks = 0;
    el.addEventListener('wa-cell-click', () => { /** @type {any} */ (window).__cellClicks++; });
  });

  await grid.locator('wa-button', { hasText: 'Edit #1' }).click();
  await expect.poll(() => received(page)).toEqual({ action: 'edit|edit|1|0|1|Login fails|', click: 'none', actions: 1, clicks: 0 });
  expect(await page.evaluate(() => /** @type {any} */ (window).__cellClicks), 'the grid dispatched wa-cell-click').toBe(1);

  // the locked row: a disabled edit button, no delete button, a disabled menu item
  expect(await grid.evaluate(el => /** @type {any} */ ([...(el.shadowRoot?.querySelectorAll('[data-col-id="edit"] wa-button') ?? [])][1])?.disabled)).toBe(true);
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="cell"][data-col-id="actions"]') ?? [])]
    .map(cell => [...cell.querySelectorAll(':scope wa-button:not([slot])')].map(button => button.querySelector('wa-icon')?.getAttribute('label')).join('+'))))
    .toEqual(['View+Delete', 'View', 'View+Delete', 'View+Delete', 'View+Delete']);

  // an icon action with a tooltip, then an item of the actions menu
  await grid.locator('[data-col-id="actions"] wa-button:not([slot])').first().click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'view|actions|1|0||Login fails|', actions: 2, clicks: 0 });
  expect(await grid.evaluate(el => el.shadowRoot?.querySelector('[data-col-id="actions"] wa-tooltip')?.textContent)).toBe('View the ticket');
  await grid.locator('[data-col-id="actions"] wa-dropdown wa-button').nth(2).click();
  await grid.locator('[data-col-id="actions"] wa-dropdown-item[value="export"]').nth(2).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'export|actions|3|2||Export to CSV|', actions: 3, clicks: 0 });

  // a switch and a checkbox send the new state as the payload
  await grid.locator('[data-col-id="active"] wa-switch').first().click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'toggle|active|1|0|True|Login fails|{"checked":false}', actions: 4, clicks: 0 });
  await grid.locator('[data-col-id="done"] wa-checkbox').nth(2).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'complete|done|3|2|False|Export to CSV|{"checked":true}', actions: 5, clicks: 0 });

  // a plain cell still raises OnCellClick, so none of the clicks above did
  await grid.locator('[part~="cell"][data-col-id="title"]').first().click();
  await expect.poll(() => received(page)).toMatchObject({ click: 'title|0', actions: 5, clicks: 1 });
});

test('keyboard (R4): Tab reaches the buttons of an action cell, and Enter and Space click a focused button', async ({ page }) => {
  const grid = await openTickets(page);

  // the active cell is the title of the third row; Tab moves into the buttons of the next cell, not to the next cell
  await grid.locator('[part~="cell"][data-col-id="title"]').nth(2).click();
  await expect.poll(() => received(page)).toMatchObject({ clicks: 1 });
  /** @type {string[]} */
  const focused = [];
  for (let i = 0; i < 3; i++) {
    await page.keyboard.press('Tab');
    focused.push(await grid.evaluate(el => {
      const active = el.shadowRoot?.activeElement;
      const cell = active?.closest('[data-col-id]');
      return `${/** @type {HTMLElement | undefined} */ (cell)?.dataset.colId}/${/** @type {HTMLElement | undefined} */ (cell)?.dataset.rowIndex}:${active?.localName}:${active?.getAttribute('slot') ?? active?.querySelector('wa-icon')?.getAttribute('label') ?? ''}`;
    }));
  }
  expect(focused).toEqual(['actions/2:wa-button:View', 'actions/2:wa-button:Delete', 'actions/2:wa-button:trigger']);

  await page.keyboard.press('Shift+Tab');
  await page.keyboard.press('Enter');
  await expect.poll(() => received(page)).toMatchObject({ action: 'delete|actions|3|2||Export to CSV|', actions: 1, clicks: 1 });

  await grid.locator('wa-button', { hasText: 'Edit #4' }).focus();
  await page.keyboard.press(' ');
  await expect.poll(() => received(page)).toMatchObject({ action: 'edit|edit|4|3|4|Slow search|', actions: 2, clicks: 1 });
});

test('menu (R6): a menu in a pinned column and in the last row opens fully, and its item reaches the bridge', async ({ page }) => {
  const grid = await openTickets(page);

  const pinned = await grid.evaluate(el => getComputedStyle(/** @type {Element} */ (el.shadowRoot?.querySelector('[part~="cell"][data-col-id="menu"]'))).position);
  expect(pinned, 'the menu column is pinned').toBe('sticky');

  await grid.locator('[data-col-id="menu"] wa-dropdown wa-button').nth(4).click();
  const layout = await grid.evaluate(el => {
    const dropdown = /** @type {any} */ ([...(el.shadowRoot?.querySelectorAll('[data-col-id="menu"] wa-dropdown') ?? [])][4]);
    return [...dropdown.querySelectorAll('wa-dropdown-item')].map(item => {
      const box = item.getBoundingClientRect();
      const x = box.left + box.width / 2;
      const y = box.top + box.height / 2;

      // the element at the center of the item, through the shadow roots: the item itself when nothing clips or covers it
      let hit = document.elementFromPoint(x, y);
      while (hit?.shadowRoot) {
        const inner = hit.shadowRoot.elementFromPoint(x, y);
        if (!inner || inner === hit) break;
        hit = inner;
      }

      const inView = box.top >= 0 && box.left >= 0 && box.bottom <= innerHeight && box.right <= innerWidth && box.height > 0;
      return `${item.getAttribute('value')}:${dropdown.open}:${inView}:${hit === item || item.contains(hit) || item.shadowRoot?.contains(hit)}`;
    });
  });
  expect(layout).toEqual(['open:true:true:true', 'duplicate:true:true:true', 'remove:true:true:true']);

  await grid.locator('[data-col-id="menu"] wa-dropdown-item[value="duplicate"]').nth(4).click();
  await expect.poll(() => received(page)).toEqual({ action: 'duplicate|menu|5|4||Typo in footer|', click: 'none', actions: 1, clicks: 0 });
});

test('ctx helpers and declarative actions raise actions from a cell, a detail panel and a footer', async ({ page }) => {
  const grid = await openTickets(page);
  /** @type {string[]} */
  const warnings = [];
  page.on('console', message => { if (message.type() === 'warning') warnings.push(message.text()); });

  // the custom cell: a badge from ctx.preset (upgraded through Uses), the amount from ctx.format, three action buttons
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="custom"] .ca-custom') ?? [])]
    .map(cell => `${cell.querySelector('wa-badge')?.textContent}:${!!cell.querySelector('wa-badge')?.shadowRoot}:${cell.querySelector('.ca-amount')?.textContent}`)))
    .toEqual(['Open:true:€120.50', 'Closed:true:€80.00', 'Open:true:€42.00', 'Open:true:€310.00', 'Closed:true:€9.99']);

  await grid.locator('[data-col-id="custom"] button', { hasText: 'Approve' }).first().click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'approve|custom|1|0|open|Login fails|{"level":2}', actions: 1, clicks: 0 });
  await grid.locator('[data-col-id="custom"] button', { hasText: 'Reject' }).nth(1).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'reject|custom|2|1|closed|Dark mode|{"reason":"test"}', actions: 2, clicks: 0 });

  // keyboard: Enter on a focused declarative button pairs like a click
  await grid.locator('[data-col-id="custom"] button', { hasText: 'Reject' }).nth(3).focus();
  await page.keyboard.press('Enter');
  await expect.poll(() => received(page)).toMatchObject({ action: 'reject|custom|4|3|open|Slow search|{"reason":"test"}', actions: 3, clicks: 0 });
  await grid.locator('[data-col-id="custom"] button', { hasText: 'Emit' }).nth(2).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'custom|custom|3|2|open|Export to CSV|{"via":"emit"}', actions: 4, clicks: 0 });

  // the footer: the sum from ctx.format, and an action without a row
  expect((await footerTexts(grid)).amount).toBe('562.49 Recalculate');
  await grid.locator('[part~="footer-cell"] button', { hasText: 'Recalculate' }).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'recalculate|amount||-1|||{"rows":5}', actions: 5 });

  // the detail panel: ctx.emit knows the row; a declarative action there has no wa-cell-click and is dropped with a warning
  await grid.locator('[part~="expand-button"]').first().click();
  await grid.locator('[part~="row-detail"] button', { hasText: 'Open from detail' }).click();
  await expect.poll(() => received(page)).toMatchObject({ action: 'detail-open||1|-1||Login fails|', actions: 6, clicks: 0 });
  await grid.locator('[part~="row-detail"] button', { hasText: 'Declarative in detail' }).click();
  await expect.poll(() => warnings.filter(text => text.includes("'detail-declarative' was dropped"))).toHaveLength(1);
  expect((await received(page)).actions).toBe(6);

  // the class rule of the amount column (more than 100)
  expect(await columnClasses(grid, 'amount')).toEqual(['ca-high', '', '', 'ca-high', '']);
});

test('rule presets: class rules, footers, collation, formatted search, filter options, detail fields and selectable rows', async ({ page }) => {
  const grid = await openGrid(page, HARNESS, '[data-testid="ca-cities"]', 9);
  await expect.poll(() => columnTexts(grid, 'region')).toEqual(['Pardubice Region', 'Pardubice Region', 'Liberec Region', 'Ústí Region', 'Zlín Region', 'Prague']);

  // cell rules: the first matching rule gives the class; numbers, dates, null, booleans, In, Matches and EqualTo
  expect(await columnClasses(grid, 'population')).toEqual(['pop-small', '', 'pop-small', 'pop-mid', 'pop-mid', 'pop-big']);
  expect(await columnClasses(grid, 'growth')).toEqual(['neg', '', 'unknown', 'neg', '', '']);
  expect(await columnClasses(grid, 'founded'), 'a DateOnly bound').toEqual(['', 'old', '', 'old', '', 'old']);
  expect(await columnClasses(grid, 'code')).toEqual(['code-hl', 'code-c', 'code-c', '', 'code-in', 'code-in']);
  expect(await columnClasses(grid, 'capital')).toEqual(['not-capital', 'not-capital', 'not-capital', 'capital', 'capital', 'capital']);
  expect(await grid.evaluate(el => getComputedStyle(/** @type {Element} */ ([...(el.shadowRoot?.querySelectorAll('[data-col-id="population"]') ?? [])].find(cell => cell.classList.contains('pop-big')))).color),
    'ShadowStyles styles a rule class').toBe('rgb(0, 0, 160)');

  // row rules
  expect(await grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row"]') ?? [])].filter(row => row.querySelector('[data-col-id="name"]'))
    .map(row => [...row.classList].filter(name => name.startsWith('row-')).join(' '))))
    .toEqual(['row-small', '', 'row-small', 'row-capital', 'row-capital', 'row-capital']);

  // footers
  expect(await footerTexts(grid)).toMatchObject({ population: 'Total 1,582,800', growth: 'Avg 0.2', code: 'Cities 6', biggest: '1,380,000', smallest: 'Min 4,300' });

  // collation: the Czech order puts "ch" after "h" and "Ú" after "U"
  await grid.locator('[part~="header-cell"][data-col-id="name"]').click();
  await expect.poll(() => columnTexts(grid, 'name')).toEqual(['Cvikov', 'Hlinsko', 'Chrudim', 'Praha', 'Ústí nad Labem', 'Zlín']);

  // the formatted search finds a lookup label, and the footers follow the filtered rows
  await grid.locator('[part="search"] input').fill('pardubice');
  await expect.poll(() => columnTexts(grid, 'name')).toEqual(['Hlinsko', 'Chrudim']);
  await expect.poll(async () => (await footerTexts(grid)).code).toBe('Cities 2');
  await grid.locator('[part="search"] input').fill('zl');
  await expect.poll(() => columnTexts(grid, 'name'), { message: 'the raw value still matches' }).toEqual(['Zlín']);
  await grid.locator('[part="search"] input').fill('');

  // the filter options of the region column come from the lookup labels
  expect(await grid.evaluate(el => /** @type {any} */ (el).columns.find((/** @type {any} */ column) => column.field === 'region').filterOptions))
    .toEqual([
      { value: 'PA', label: 'Pardubice Region' }, { value: 'LI', label: 'Liberec Region' }, { value: 'UL', label: 'Ústí Region' },
      { value: 'ZL', label: 'Zlín Region' }, { value: 'PR', label: 'Prague' },
    ]);

  // selectable rows: the row without canSelect has a disabled checkbox
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row"]') ?? [])].filter(row => row.querySelector('[data-col-id="name"]'))
    .map(row => `${row.querySelector('[data-col-id="name"]')?.textContent?.trim()}:${/** @type {any} */ (row.querySelector('wa-checkbox'))?.disabled}`)))
    .toContain('Cvikov:true');

  // the detail fields of the first row (sorted: Cvikov)
  await grid.locator('[part~="expand-button"]').first().click();
  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row-detail"] dl > *') ?? [])].map(node => node.textContent)))
    .toEqual(['Name', 'Cvikov', 'Founded', 'January 1, 1352', 'Growth', 'none', 'Email', 'info@cvikov.example']);
});

test('Lit (R2): a template of the app\'s own Lit renders in a cell, and its @click and declarative action reach the bridge', async ({ page }) => {
  const grid = await openGrid(page, LIT_HARNESS, '[data-testid="lit-grid"]', 2);

  await expect.poll(() => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[data-col-id="lit"] .lit-cell') ?? [])]
    .map(cell => `${cell.querySelector('strong')?.textContent}:${cell.querySelectorAll('button').length}`))).toEqual(['Ada:2', 'Grace:2']);

  await grid.locator('button', { hasText: 'Lit click' }).nth(1).click();
  await expect(page.getByTestId('lit-action')).toHaveText('lit-click|lit|2|{"id":2}');
  await grid.locator('button', { hasText: 'Declarative' }).first().click();
  await expect(page.getByTestId('lit-action')).toHaveText('lit-declarative|lit|1|');
  await expect(page.getByTestId('lit-click-count')).toHaveText('0');
});

test('data grid page: the "Extensions: Actions & Rules" example toggles, marks, sums and receives the menu action', async ({ page }) => {
  const grid = await openGrid(page, DATA_GRID_PAGE, 'wa-data-grid[label="Subscriptions"]', 5);
  const rows = () => grid.evaluate(el => [...(el.shadowRoot?.querySelectorAll('[part~="row"]') ?? [])].filter(row => row.querySelector('[data-col-id="customer"]'))
    .map(row => `${row.querySelector('[data-col-id="customer"]')?.textContent?.trim()}:${row.classList.contains('paused')}:${row.querySelector('[data-col-id="seats"]')?.classList.contains('large-team')}:${/** @type {any} */ (row.querySelector('wa-switch'))?.checked}`));

  await expect.poll(rows).toEqual(['Northwind:false:true:true', 'Contoso:false:false:true', 'Fabrikam:true:false:false', 'Tailspin:false:true:true']);
  expect((await footerTexts(grid)).seats, 'the footer sum').toBe('217');

  // the switch: the handler updates the row and pushes the data, so the row rule follows
  await grid.locator('wa-switch').nth(1).click();
  await expect(page.getByTestId('subscription-action')).toHaveText('Contoso is paused now.');
  await expect.poll(rows).toEqual(['Northwind:false:true:true', 'Contoso:true:false:false', 'Fabrikam:true:false:false', 'Tailspin:false:true:true']);

  await grid.locator('[data-col-id="actions"] wa-dropdown wa-button').first().click();
  await grid.locator('[data-col-id="actions"] wa-dropdown-item[value="invoice"]').first().click();
  await expect(page.getByTestId('subscription-action')).toHaveText('Action "invoice" for Northwind.');
});
