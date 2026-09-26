// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');

// Browser acceptance for the strongly typed date and time wrappers (3.12.0): a real user picks a date, a range and a
// time, and the typed .NET model (DateOnly, WaDateRange, TimeOnly), echoed through its own members on the harness
// /testing/date-typing, follows; a half-filled range (one date picked) binds as From only; DisabledDates and
// DisabledDaysOfWeek (IReadOnlySet<DateOnly>, IReadOnlySet<DayOfWeek>) really disable the calendar cells; and a
// DateTimeOffset reaches wa-relative-time as the right instant in a browser time zone far from UTC. The model -> UI
// direction after a user edit is covered by the range rows of value-sync-binding.spec.js. The date inputs and
// pickers are Pro components and skip visibly on the free CDN.

const ROUTE = '/testing/date-typing';

/**
 * Opens the harness and waits until Blazor is interactive and the given elements have upgraded.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string[]} freeTags
 * @param {string[]} proTags
 */
async function open(page, freeTags, proTags = []) {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, freeTags);
  if (proTags.length > 0) {
    await skipUnlessProUpgrades(page, proTags);
    await waitForWaReady(page, proTags);
  }
}

test('date typing: picking a day in the WaDateInput popup binds a DateOnly', async ({ page }) => {
  await open(page, [], ['wa-date-input']);

  const input = page.getByTestId('dt-date-input');
  await input.locator('[part~="expand-button"]').click();
  await input.locator('button[data-date="2024-03-18"]').click();

  await expect(page.getByTestId('dt-date-input-model'), 'the DateOnly echo').toHaveText('2024-03-18');
  await expect(page.getByTestId('dt-date-input-weekday'), 'DateOnly.DayOfWeek of the bound value').toHaveText('Monday');
  await expect(input, 'the element holds the ISO value').toHaveJSProperty('value', '2024-03-18');
});

test('date typing: picking two days in the WaDateRangeInput popup binds a complete WaDateRange', async ({ page }) => {
  await open(page, [], ['wa-date-input']);

  const input = page.getByTestId('dt-range-input');
  await input.locator('[part~="expand-button"]').click();
  await input.locator('button[data-date="2024-03-22"]').click();
  await input.locator('button[data-date="2024-03-18"]').click();

  // picked end first: the element orders the range, and so does the model
  await expect(page.getByTestId('dt-range-input-from')).toHaveText('2024-03-18');
  await expect(page.getByTestId('dt-range-input-to')).toHaveText('2024-03-22');
  await expect(page.getByTestId('dt-range-input-complete'), 'WaDateRange.IsComplete').toHaveText('True');
  await expect(input).toHaveJSProperty('value', '2024-03-18/2024-03-22');
});

test('date typing: typing only the start date of a WaDateRangeInput binds a half-filled range', async ({ page }) => {
  await open(page, [], ['wa-date-input']);

  const input = page.getByTestId('dt-range-input');
  await input.locator('[data-segment="month"][data-group="from"]').click();
  await page.keyboard.type('03182024');
  await page.keyboard.press('Tab');
  await page.locator('h1').click();

  // Web Awesome reports a half-filled range as its one date, which binds as From only
  await expect(input, 'the element reports the one date').toHaveJSProperty('value', '2024-03-18');
  await expect(page.getByTestId('dt-range-input-from')).toHaveText('2024-03-18');
  await expect(page.getByTestId('dt-range-input-to')).toHaveText('');
  await expect(page.getByTestId('dt-range-input-complete')).toHaveText('False');
});

test('date typing: clicking a day in WaDatePicker binds a DateOnly', async ({ page }) => {
  await open(page, [], ['wa-date-picker']);

  await page.getByTestId('dt-date-picker').locator('button[data-date="2024-03-19"]').click();

  await expect(page.getByTestId('dt-date-picker-model')).toHaveText('2024-03-19');
  await expect(page.getByTestId('dt-date-picker-weekday')).toHaveText('Tuesday');
});

test('date typing: WaDateRangePicker binds the range on the second click, after an input with the first date', async ({ page }) => {
  await open(page, [], ['wa-date-picker']);

  const picker = page.getByTestId('dt-range-picker');
  await picker.locator('button[data-date="2024-03-11"]').click();

  // the first click is an input with the one date; the committed value (change) comes with the second
  await expect(page.getByTestId('dt-range-picker-input'), 'OnInput carries the wire string').toHaveText('2024-03-11');
  await expect(page.getByTestId('dt-range-picker-complete'), 'no committed range yet').toHaveText('');

  await picker.locator('button[data-date="2024-03-14"]').click();

  await expect(page.getByTestId('dt-range-picker-from')).toHaveText('2024-03-11');
  await expect(page.getByTestId('dt-range-picker-to')).toHaveText('2024-03-14');
  await expect(page.getByTestId('dt-range-picker-complete')).toHaveText('True');
});

test('date typing: DisabledDates and DisabledDaysOfWeek disable exactly those calendar cells', async ({ page }) => {
  await open(page, [], ['wa-date-picker']);

  const picker = page.getByTestId('dt-disabled-picker');
  const cell = (/** @type {string} */ iso) => picker.locator(`button[data-date="${iso}"]`);

  // Holidays = 2024-03-20, 2024-03-21; Weekend = Saturday, Sunday (2024-03-16/17, 23/24)
  for (const iso of ['2024-03-16', '2024-03-17', '2024-03-20', '2024-03-21', '2024-03-23', '2024-03-24']) {
    await expect(cell(iso), `${iso} is disabled`).toHaveAttribute('aria-disabled', 'true');
  }
  for (const iso of ['2024-03-15', '2024-03-18', '2024-03-19', '2024-03-22']) {
    await expect(cell(iso), `${iso} stays enabled`).toHaveAttribute('aria-disabled', 'false');
  }

  // a disabled cell cannot be picked; an enabled one can
  await cell('2024-03-20').click({ force: true });
  await expect(page.getByTestId('dt-disabled-picker-model'), 'a click on a disabled day binds nothing').toHaveText('');
  await cell('2024-03-22').click();
  await expect(page.getByTestId('dt-disabled-picker-model')).toHaveText('2024-03-22');
});

test('date typing: picking hour and minute in the WaTimeInput popup binds a TimeOnly', async ({ page }) => {
  await open(page, ['wa-time-input']);

  const input = page.getByTestId('dt-time-input');
  await input.locator('[part~="expand-button"]').click();
  await input.locator('[part~="column-hour"] [data-value="14"]').click();
  await input.locator('[part~="column-minute"] [data-value="30"]').click();
  await page.keyboard.press('Escape');

  await expect(page.getByTestId('dt-time-input-model')).toHaveText('14:30');
  await expect(page.getByTestId('dt-time-input-parts'), 'TimeOnly.Hour and Minute').toHaveText('14h 30m');
  await expect(input).toHaveJSProperty('value', '14:30');
});

test.describe('in a browser time zone far from UTC', () => {
  // a date-time without its offset would be read as Tokyo wall-clock time and be nine hours off
  test.use({ timezoneId: 'Asia/Tokyo' });

  test('date typing: WaRelativeTime reads a UTC DateTimeOffset, and the same instant at another offset, as that instant', async ({ page }) => {
    await open(page, ['wa-relative-time']);

    const expected = await page.evaluate(() => new Intl.RelativeTimeFormat('en-US').format(-2, 'hour'));
    for (const id of ['rt-utc', 'rt-offset']) {
      const element = page.getByTestId(id).locator('wa-relative-time');
      await expect(element, `${id} carries its offset`).toHaveAttribute('date', /T\d\d:\d\d:\d\d\.\d{3}[+-]\d\d:\d\d$/);
      await expect
        .poll(() => element.evaluate(el => (el.shadowRoot?.textContent ?? '').trim()), { message: `rendered text of ${id}` })
        .toBe(expected);
    }
  });
});
