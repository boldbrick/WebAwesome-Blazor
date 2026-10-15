// @ts-check
const { test, expect } = require('./helpers/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');

// The function-valued members of the element wrappers and ShadowStyles (docs\technical.md, "JavaScript function
// references"), on the harness /testing/element-functions: each WaJsFunction parameter reaches its element as a
// function with a trailing ctx (args, element), ShadowStyles styles the output of a function in the shadow root
// (changed in place), a function set back to null gives the element its own renderer again, and the obsolete string
// methods no longer break wa-select and wa-rating. The select, rating, slider, range and pagination run on the free CDN;
// the combobox, the date controls and the data grid are Pro components and skip visibly there.

const HARNESS = '/testing/element-functions';

/**
 * Opens the harness and waits until the given elements have upgraded; Pro elements skip the test on the free CDN.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} tags
 * @param {string[]} [proTags]
 */
async function open(page, tags, proTags = []) {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');
  if (proTags.length > 0) await skipUnlessProUpgrades(page, proTags);
  await waitForWaReady(page, [...tags, ...proTags]);
}

/**
 * Collects the uncaught page errors (and unhandled rejections) of a page.
 *
 * @param {import('@playwright/test').Page} page
 * @returns {string[]}
 */
function collectPageErrors(page) {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  return errors;
}

/**
 * Asks an element to render again and returns the error of a failed update, or null.
 *
 * @param {import('@playwright/test').Locator} element
 * @returns {Promise<string|null>}
 */
function updateError(element) {
  return element.evaluate(async el => {
    try {
      /** @type {any} */ (el).requestUpdate();
      await /** @type {any} */ (el).updateComplete;
      return null;
    } catch (error) {
      return String(error);
    }
  });
}

/**
 * The texts of the elements matching a selector in the shadow root of an element.
 *
 * @param {import('@playwright/test').Locator} element
 * @param {string} selector
 * @returns {Promise<string[]>}
 */
function shadowTexts(element, selector) {
  return element.evaluate((el, s) => [...(el.shadowRoot?.querySelectorAll(s) ?? [])].map(node => (node.textContent ?? '').trim()), selector);
}

/**
 * The computed text color of the first element matching a selector in the shadow root of an element.
 *
 * @param {import('@playwright/test').Locator} element
 * @param {string} selector
 * @returns {Promise<string|null>}
 */
function shadowColor(element, selector) {
  return element.evaluate((el, s) => {
    const node = el.shadowRoot?.querySelector(s);
    return node ? getComputedStyle(node).color : null;
  }, selector);
}

test('obsolete string methods: wa-select and wa-rating keep rendering after the call', async ({ page }) => {
  const errors = collectPageErrors(page);
  await open(page, ['wa-select', 'wa-rating', 'wa-button']);

  await page.getByTestId('ef-call-obsolete').click();
  await expect(page.getByTestId('ef-obsolete-rating-result')).not.toBeEmpty();

  const select = page.getByTestId('ef-obsolete-select');
  const rating = page.getByTestId('ef-obsolete-rating');
  expect(await updateError(select), 'wa-select renders its tags').toBeNull();
  expect(await updateError(rating), 'wa-rating renders its symbols').toBeNull();
  expect(errors, 'no page error').toEqual([]);

  // the tags and the symbols are the element's own
  await expect.poll(() => select.evaluate(el => el.shadowRoot?.querySelectorAll('wa-tag').length ?? 0)).toBe(2);
  await expect.poll(() => rating.evaluate(el => el.shadowRoot?.querySelectorAll('wa-icon').length ?? 0)).toBeGreaterThan(0);

  // the methods tell the caller what to use instead
  await expect(page.getByTestId('ef-obsolete-select-result')).toHaveText('NotSupportedException');
  await expect(page.getByTestId('ef-obsolete-rating-result')).toHaveText('NotSupportedException');
});

test('select: GetTagFunction renders the tags, ShadowStyles colors them, and null restores the element tags', async ({ page }) => {
  await open(page, ['wa-select', 'wa-button']);
  const select = page.getByTestId('ef-select');

  await expect.poll(() => shadowTexts(select, '.ef-tag')).toEqual(['* Alpha#0|wa-select', '* Beta#1|wa-select']);
  await expect.poll(() => shadowColor(select, '.ef-tag'), { message: 'the shadow styles color the tags' }).toBe('rgb(255, 0, 0)');

  // new styles replace the sheet's content
  await page.getByTestId('ef-change-styles').click();
  await expect.poll(() => shadowColor(select, '.ef-tag')).toBe('rgb(0, 0, 255)');
  expect(await select.evaluate(el => el.shadowRoot?.adoptedStyleSheets.filter(sheet => [...sheet.cssRules].some(rule => rule.cssText.includes('.ef-tag'))).length),
    'one sheet, replaced in place').toBe(1);

  // without the function the element renders its own tags again
  await page.getByTestId('ef-remove-tag-function').click();
  await expect.poll(() => shadowTexts(select, '.ef-tag')).toEqual([]);
  await expect.poll(() => select.evaluate(el => el.shadowRoot?.querySelectorAll('wa-tag').length ?? 0)).toBe(2);
});

test('rating: GetSymbolFunction renders the symbols', async ({ page }) => {
  await open(page, ['wa-rating']);
  const rating = page.getByTestId('ef-rating');

  await expect.poll(() => rating.evaluate(el => el.shadowRoot?.querySelectorAll('.ef-symbol').length ?? 0)).toBeGreaterThan(0);
  const symbols = await shadowTexts(rating, '.ef-symbol');
  expect(symbols, 'selected and unselected symbols from ctx.args').toEqual(expect.arrayContaining(['#', 'o']));
  expect(await rating.evaluate(el => el.shadowRoot?.querySelectorAll('wa-icon').length ?? 0), 'no default stars').toBe(0);
});

test('slider and range: ValueFormatterFunction formats the value text', async ({ page }) => {
  await open(page, ['wa-slider']);

  for (const [testId, expected] of [['ef-slider', '40 units'], ['ef-range', '25 units']]) {
    const slider = page.getByTestId(testId);
    await expect.poll(() => slider.evaluate(el => el.shadowRoot?.querySelector('[aria-valuetext]')?.getAttribute('aria-valuetext') ?? null),
      { message: `${testId} announces the formatted value` }).toBe(expected);
  }
});

test('pagination: HrefTemplateFunction builds the page links', async ({ page }) => {
  await open(page, ['wa-pagination']);
  const pagination = page.getByTestId('ef-pagination');

  await expect.poll(() => pagination.evaluate(el => [...(el.shadowRoot?.querySelectorAll('a[href]') ?? [])].map(a => a.getAttribute('href'))))
    .toEqual(expect.arrayContaining(['/testing/element-functions?p=2', '/testing/element-functions?p=5']));
});

test('combobox: GetTagFunction renders the tags and FilterFunction filters the options', async ({ page }) => {
  await open(page, [], ['wa-combobox']);
  const combobox = page.getByTestId('ef-combobox');

  await expect.poll(() => shadowTexts(combobox, '.ef-tag')).toEqual(['+ Alpha#0|wa-combobox', '+ Beta#1|wa-combobox']);
  await expect.poll(() => shadowColor(combobox, '.ef-tag')).toBe('rgb(255, 0, 0)');

  // the element's own filter would keep every option (each label contains "a"); the function keeps "Alpha" only
  const input = combobox.locator('input[part~="combobox-input"]');
  await input.click();
  await input.pressSequentially('a');
  await expect.poll(() => combobox.evaluate(el => [...el.querySelectorAll('wa-option')].filter(option => !option.hidden).map(option => (option.textContent ?? '').trim())))
    .toEqual(['Alpha']);
});

test('date picker: IsDateDisabledFunction, DayContentFunction and ShadowStyles', async ({ page }) => {
  await open(page, [], ['wa-date-picker']);
  const picker = page.getByTestId('ef-date-picker');

  await expect(picker.locator('button[data-date="2024-03-16"]'), 'a Saturday is disabled').toHaveAttribute('data-disabled', '');
  await expect(picker.locator('button[data-date="2024-03-12"]'), 'a Tuesday is not').not.toHaveAttribute('data-disabled');
  await expect(picker.locator('button[data-date="2024-03-15"] .ef-day')).toHaveText('15!');
  await expect.poll(() => picker.locator('button[data-date="2024-03-15"] .ef-day').evaluate(node => getComputedStyle(node).color)).toBe('rgb(0, 128, 0)');
});

test('date input and date range input: the functions and ShadowStyles reach the popup calendar', async ({ page }) => {
  await open(page, [], ['wa-date-input']);

  for (const testId of ['ef-date-input', 'ef-date-range-input']) {
    const input = page.getByTestId(testId);
    await expect(input.locator('button[data-date="2024-03-16"]'), `${testId}: a Saturday is disabled`).toHaveAttribute('data-disabled', '');
    await expect(input.locator('button[data-date="2024-03-12"]'), `${testId}: a Tuesday is not`).not.toHaveAttribute('data-disabled');
    await expect(input.locator('button[data-date="2024-03-15"] .ef-day'), `${testId}: the day content`).toHaveText('15!');
  }

  // the calendar of wa-date-input is in its own shadow root, which gets the sheet too
  const content = page.getByTestId('ef-date-input').locator('button[data-date="2024-03-15"] .ef-day');
  await expect.poll(() => content.evaluate(node => getComputedStyle(node).color)).toBe('rgb(0, 128, 0)');
});

test('data grid: ShadowStyles colors a class that a formatter sets', async ({ page }) => {
  await open(page, [], ['wa-data-grid']);
  const grid = page.getByTestId('ef-grid');

  await expect.poll(() => shadowTexts(grid, '.ef-cell')).toEqual(['Ada', 'Grace']);
  await expect.poll(() => shadowColor(grid, '.ef-cell')).toBe('rgb(255, 0, 0)');
});
