// @ts-check
const { test, expect, openHarness, openPanel, selectByTestId, sessionOf, HARNESS_ROUTE } = require('./fixtures');

// The live changes of the panel on the harness page: attribute overrides on a placement, value remaps of a
// data-driven attribute, CSS custom properties, utility classes and theme tokens. The session is the source of truth:
// the content script applies it again after every page change (a render of the page, a navigation away and back, a
// reload).

// the route the navigation tests leave the harness for (another harness page)
const OTHER_ROUTE = '/testing/sticky-attributes';

/**
 * The value of one attribute on every element with the test id.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} testId
 * @param {string} name
 */
async function attributeValues(page, testId, name) {
  return page.evaluate(({ id, attribute }) =>
    [...document.querySelectorAll(`[data-testid="${id}"]`)].map(element => element.getAttribute(attribute)), { id: testId, attribute: name });
}

/**
 * The input control of one row of a form in the details pane (attributes, CSS custom properties): the element after
 * the row's name.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} name
 */
function formControl(panel, name) {
  return panel.locator('#details .form > span.name').filter({ hasText: new RegExp(`^${name}$`) }).locator('xpath=following-sibling::*[1]');
}

/**
 * Leaves the harness by a Blazor navigation and comes back by the history.
 *
 * @param {import('@playwright/test').Page} page
 */
async function navigateAwayAndBack(page) {
  await page.evaluate(route => /** @type {any} */ (window).Blazor.navigateTo(route), OTHER_ROUTE);
  await page.waitForURL(`**${OTHER_ROUTE}`);
  await page.getByTestId('sa-slider').waitFor();
  await expect(page.getByTestId('ih-card')).toHaveCount(0);
  await page.goBack();
  await page.waitForURL(`**${HARNESS_ROUTE}`);
  await page.getByTestId('ih-card').waitFor();
}

test.describe('panel changes on the harness', () => {
  test('an attribute override on a placement applies to all instances and stays applied', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);

    // the badges' own appearance (Web Awesome reflects the element default)
    const own = await attributeValues(page, 'ih-badge', 'appearance');
    const details = await selectByTestId(panel, 'ih-badge');
    expect(details.meta.component).toBe('WaBadge');
    expect(details.counts.placement, 'the placement holds the three loop badges').toBe(3);

    await formControl(panel, 'appearance').selectOption('outlined');
    await expect.poll(() => attributeValues(page, 'ih-badge', 'appearance')).toEqual(['outlined', 'outlined', 'outlined']);

    const session = await sessionOf(panel);
    expect(session.items).toHaveLength(1);
    expect(session.items[0]).toMatchObject({ number: 1, scope: 'placement', matchCount: 3, attributes: { appearance: { value: 'outlined' } } });
    expect(session.items[0].target.placement).toBe(details.meta.placement);

    // a render of the page (the data changes one badge): the override stays on every badge
    await page.getByTestId('ih-toggle').click();
    await expect.poll(() => attributeValues(page, 'ih-badge', 'variant')).toEqual(['success', 'success', 'neutral']);
    expect(await attributeValues(page, 'ih-badge', 'appearance')).toEqual(['outlined', 'outlined', 'outlined']);

    // a Blazor navigation away and back renders new elements at the same placement
    await navigateAwayAndBack(page);
    await expect.poll(() => attributeValues(page, 'ih-badge', 'appearance')).toEqual(['outlined', 'outlined', 'outlined']);

    // a full reload starts a new content script, which reads the stored session
    await page.reload();
    await openHarness(page);
    await expect.poll(() => attributeValues(page, 'ih-badge', 'appearance')).toEqual(['outlined', 'outlined', 'outlined']);

    // the other wa-badge elements of the page (the navigation) are not part of the placement
    const others = await page.evaluate(() => [...document.querySelectorAll('wa-badge:not([data-testid="ih-badge"])')]
      .filter(badge => badge.getAttribute('appearance') === 'outlined').length);
    expect(others, 'badges outside the placement keep their appearance').toBe(0);

    // removing the change gives the elements their own value back
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { delete item.attributes.appearance; }));
    await expect.poll(() => attributeValues(page, 'ih-badge', 'appearance')).toEqual(own);
  });

  test('a data-driven badge variant is detected and a remap follows the data', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);

    const details = await selectByTestId(panel, 'ih-badge');
    expect(details.observed.variant, 'the variants across the placement').toEqual({ neutral: 2, success: 1 });
    await expect(panel.locator('#details .driven', { hasText: 'remap' })).toBeVisible();
    await expect(panel.locator('#details .hint', { hasText: 'data-driven: neutral×2, success×1' })).toBeVisible();

    // map "neutral" to "warning"; "success" stays
    const neutralRow = panel.locator('#details .remap > span.mono').filter({ hasText: /^neutral$/ });
    await neutralRow.locator('xpath=following-sibling::select[1]').selectOption('warning');
    await expect.poll(() => attributeValues(page, 'ih-badge', 'variant')).toEqual(['warning', 'success', 'warning']);

    const session = await sessionOf(panel);
    expect(session.items[0].attributes.variant).toMatchObject({ remap: { neutral: 'warning' }, dataDriven: true });

    // the page changes the value of the first badge: the remap follows the page's own value
    await page.getByTestId('ih-toggle').click();
    await expect.poll(() => attributeValues(page, 'ih-badge', 'variant')).toEqual(['success', 'success', 'warning']);
    await page.getByTestId('ih-toggle').click();
    await expect.poll(() => attributeValues(page, 'ih-badge', 'variant')).toEqual(['warning', 'success', 'warning']);
    await expect(page.getByTestId('ih-open-count')).toHaveText('2');
  });

  test('a change on the source site applies to the reusable action at both placements', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);

    const details = await selectByTestId(panel, 'ih-action-card');
    expect(details.counts).toMatchObject({ placement: 1, site: 2 });
    await panel.locator('#details .scopes label', { hasText: 'Same source site (2)' }).locator('input').check();
    await formControl(panel, 'variant').selectOption('danger');

    await expect.poll(() => attributeValues(page, 'ih-action-card', 'variant')).toEqual(['danger']);
    expect(await attributeValues(page, 'ih-action-note', 'variant')).toEqual(['danger']);
    expect(await attributeValues(page, 'ih-toggle', 'variant'), 'another site keeps its value').toEqual(['brand']);

    const [item] = (await sessionOf(panel)).items;
    expect(item).toMatchObject({ scope: 'site', matchCount: 2, target: { site: details.meta.site } });
  });

  test('a CSS custom property and a utility class apply', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);

    await selectByTestId(panel, 'ih-card');
    const spacing = formControl(panel, '--spacing');
    await spacing.fill('3rem');
    await spacing.press('Enter');
    await expect.poll(() => page.getByTestId('ih-card').evaluate(card => getComputedStyle(card).getPropertyValue('--spacing').trim())).toBe('3rem');

    // the plain div with a utility class: add wa-justify-content-end
    const note = page.getByTestId('ih-note');
    const justify = () => note.evaluate(element => getComputedStyle(element).justifyContent);
    expect(await justify()).not.toMatch(/end$/);
    const noteDetails = await selectByTestId(panel, 'ih-note');
    expect(noteDetails.isWa).toBe(false);
    const classInput = panel.locator('#details input[list="utility-classes"]');
    await classInput.fill('wa-justify-content-end');
    await classInput.press('Enter');
    await expect.poll(justify).toMatch(/end$/);
    expect((await note.getAttribute('class') || '').split(' ').sort(), 'the page classes stay').toEqual(['ih-note', 'wa-gap-m', 'wa-justify-content-end']);

    const session = await sessionOf(panel);
    expect(session.items.map(item => item.number)).toEqual([1, 2]);
    expect(session.items[0].cssProperties).toEqual({ '--spacing': '3rem' });
    expect(session.items[1].classes).toEqual({ add: ['wa-justify-content-end'], remove: [] });
    expect(session.items[1].target.placement, 'a plain element has no Blazor placement').toBeFalsy();
    expect(session.items[1].target.structuralKey).toMatch(/^[0-9a-f]{8}$/);
    expect(session.items[1].target.ownerComponent, 'the plain div names no wrapper of its own').toBeFalsy();

    // both stay after a navigation away and back
    await navigateAwayAndBack(page);
    await expect.poll(justify).toMatch(/end$/);
    await expect.poll(() => page.getByTestId('ih-card').evaluate(card => getComputedStyle(card).getPropertyValue('--spacing').trim())).toBe('3rem');
  });

  test('a theme token applies on :root', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);

    const token = '--wa-color-brand-fill-loud';
    const value = 'rgb(200, 0, 100)';
    const rootValue = () => page.evaluate(name => getComputedStyle(document.documentElement).getPropertyValue(name).trim(), token);
    expect(await rootValue()).not.toBe(value);

    await panel.locator('.tab[data-tab="theme"]').click();
    await panel.locator('#theme-filter').fill('brand-fill-loud');
    const row = panel.locator('.token-row').filter({ has: panel.locator('span.name', { hasText: new RegExp(`^${token}$`) }) });
    const input = row.locator('input[type="text"]');
    await input.fill(value);
    await input.press('Enter');

    await expect.poll(rootValue).toBe(value);
    // the brand button takes the token
    await expect.poll(() => page.getByTestId('ih-toggle').evaluate(button => {
      const base = button.shadowRoot && button.shadowRoot.querySelector('[part~="base"]');
      return base ? getComputedStyle(base).backgroundColor : null;
    })).toBe(value);

    const session = await sessionOf(panel);
    expect(session.theme).toEqual({ [token]: value });
    expect(session.items, 'a theme token is no change-list item').toHaveLength(0);
  });

  test('a removed non-boolean attribute gets the element default, not null', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);
    const property = (testId, name) => page.getByTestId(testId).evaluate((element, key) => /** @type {any} */ (element)[key], name);

    // the page does not set the type of the second toolbar button, and wa-button does not reflect it
    expect(await page.getByTestId('ih-reopen').getAttribute('type')).toBeNull();
    expect(await property('ih-reopen', 'type')).toBe('button');
    await selectByTestId(panel, 'ih-reopen');
    await formControl(panel, 'type').selectOption('submit');
    await expect.poll(() => property('ih-reopen', 'type')).toBe('submit');
    const [typeItem] = (await sessionOf(panel)).items;
    expect(typeItem.attributes.type, 'the operation carries the field kind and the element default')
      .toMatchObject({ value: 'submit', from: null, kind: 'enum', elementDefault: 'button' });

    // delete the override: the page value "not set" comes back as the element default
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { delete item.attributes.type; }));
    await expect.poll(() => property('ih-reopen', 'type')).toBe('button');
    expect(await page.getByTestId('ih-reopen').getAttribute('type')).toBe('button');
    const restored = await panel.evaluate(async () => {
      const panelHooks = /** @type {any} */ (window).__waInspectorPanel;
      return panelHooks.request('page.details', { id: panelHooks.state.selectedId });
    });
    expect(restored.attributes.type, 'the panel shows the element default as not set').toBeUndefined();
    expect(restored.observed.type, 'the element default is no page value').toEqual({ '∅': 1 });

    // an override "(not set)" on an enum attribute that the page sets
    await selectByTestId(panel, 'ih-toggle');
    await formControl(panel, 'variant').selectOption({ label: '(not set: neutral)' });
    await expect.poll(() => property('ih-toggle', 'variant')).toBe('neutral');
    expect(await page.getByTestId('ih-toggle').getAttribute('variant')).toBe('neutral');
    const variantItem = (await sessionOf(panel)).items.find(item => item.attributes.variant);
    expect(variantItem.attributes.variant).toMatchObject({ remove: true, from: 'brand', kind: 'enum', elementDefault: 'neutral' });
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { delete item.attributes.variant; }));
    await expect.poll(() => property('ih-toggle', 'variant'), 'the page value comes back').toBe('brand');

    // a boolean attribute keeps present and absent
    await selectByTestId(panel, 'ih-reopen');
    await formControl(panel, 'pill').check();
    await expect.poll(() => page.getByTestId('ih-reopen').getAttribute('pill')).toBe('');
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { delete item.attributes.pill; }));
    await expect.poll(() => page.getByTestId('ih-reopen').getAttribute('pill')).toBeNull();
    expect(await property('ih-reopen', 'pill')).toBe(false);

    // an operation of an older session (no field kind, no default) removes the attribute as before
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { item.attributes.type = { value: 'reset', from: null }; }));
    await expect.poll(() => page.getByTestId('ih-reopen').getAttribute('type')).toBe('reset');
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => { delete item.attributes.type; }));
    await expect.poll(() => page.getByTestId('ih-reopen').getAttribute('type')).toBeNull();
  });

  test('an item turns "applied" when the page value equals the override', async ({ page, context, serviceWorker, extensionId }) => {
    await openHarness(page);
    const panel = await openPanel({ context, serviceWorker, extensionId }, page);
    const hooks = () => panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.request('page.itemStatus'));

    // all badges "neutral": the page has it on two badges of three
    await selectByTestId(panel, 'ih-badge');
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => {
      item.attributes.variant = { value: 'neutral', from: 'neutral' };
    }));
    await expect.poll(() => attributeValues(page, 'ih-badge', 'variant')).toEqual(['neutral', 'neutral', 'neutral']);
    const [badgeItem] = (await sessionOf(panel)).items;
    await expect.poll(async () => (await hooks())[badgeItem.id]).toEqual({ matches: 3, status: 'pending' });

    // the page sets "neutral" on the last badge too (the value the override shows already)
    await page.getByTestId('ih-reopen').click();
    await expect(page.getByTestId('ih-open-count')).toHaveText('3');
    await expect.poll(async () => (await hooks())[badgeItem.id]).toEqual({ matches: 3, status: 'applied' });

    // the toolbar button is "brand" already: applied at once
    await selectByTestId(panel, 'ih-toggle');
    await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.editItem(item => {
      item.attributes.variant = { value: 'brand', from: 'brand' };
    }));
    const buttonItem = (await sessionOf(panel)).items[1];
    await expect.poll(async () => (await hooks())[buttonItem.id]).toEqual({ matches: 1, status: 'applied' });

    // the Changes tab shows the status
    await panel.locator('.tab[data-tab="changes"]').click();
    await expect(panel.locator('.change-item .state.applied')).toHaveCount(2);
    await expect(panel.locator('#change-count')).toHaveText('2');
  });
});
