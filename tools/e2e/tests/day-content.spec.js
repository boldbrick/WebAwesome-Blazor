// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');

// Browser acceptance for WaDayContent (3.12.0): per-day content rendered into the day-YYYY-MM-DD slots of
// wa-date-picker, and of wa-date-input, which forwards them to its popup calendar. On the harness /testing/day-content
// every host shows the same content: it must appear in its own day cell only, follow additions and removals made by a
// render of the page and by a component re-rendering on its own (no render of the host), show every content given for
// one date, and show content of another month after navigating there. wa-date-input forwards day slots only on its
// first update and on its default slot's slotchange, which a day-slotted child never causes, so the added, removed
// and toggled rows prove the wrapper's forwarding nudge (WaDateInputBase, signalDefaultSlotChange). The date inputs
// and pickers are Pro components and skip visibly on the free CDN.

const ROUTE = '/testing/day-content';

// the four hosts: test id, element tag, and whether the calendar sits in a popup (wa-date-input)
const HOSTS = [
  { name: 'WaDatePicker', testId: 'dc-picker', tag: 'wa-date-picker', popup: false },
  { name: 'WaDateRangePicker', testId: 'dc-range-picker', tag: 'wa-date-picker', popup: false },
  { name: 'WaDateInput', testId: 'dc-input', tag: 'wa-date-input', popup: true },
  { name: 'WaDateRangeInput', testId: 'dc-range-input', tag: 'wa-date-input', popup: true },
];

/**
 * Opens the harness, waits until Blazor is interactive and the host has upgraded, and opens its popup calendar.
 *
 * @param {import('@playwright/test').Page} page
 * @param {typeof HOSTS[number]} host
 */
async function open(page, host) {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await skipUnlessProUpgrades(page, [host.tag]);
  await waitForWaReady(page, [host.tag]);

  const element = page.getByTestId(host.testId);
  if (host.popup) {
    await element.locator('[part~="expand-button"]').click();
    await expect(element.locator('button[data-date="2024-03-15"]'), 'the popup calendar is open').toBeVisible();
  }
  return element;
}

/**
 * What a day cell shows: the text of the nodes its day slot renders, flattened through wa-date-input's forwarding
 * slot - the slotted day content, or the day number (the slot's fallback) when no content is assigned. A forwarded
 * slot left without content renders nothing, so its cell reads empty.
 *
 * @param {import('@playwright/test').Locator} host
 * @param {string} iso
 */
function dayCell(host, iso) {
  return host.locator(`button[data-date="${iso}"]`).evaluate(button => {
    const slot = button.querySelector('slot');
    if (!slot) return null;
    return slot.assignedNodes({ flatten: true }).map(node => node.textContent ?? '').join('').trim();
  });
}

/**
 * Expects a day cell to show the given text, waiting for the element to catch up.
 *
 * @param {import('@playwright/test').Locator} host
 * @param {string} iso
 * @param {string} expected
 * @param {string} message
 */
async function expectDay(host, iso, expected, message) {
  await expect.poll(() => dayCell(host, iso), { message: `${iso}: ${message}` }).toBe(expected);
}

for (const host of HOSTS) {
  test(`day content: ${host.name} shows the initial day content in its own cell, and every content given for one date`, async ({ page }) => {
    const element = await open(page, host);

    await expectDay(element, '2024-03-17', '🍀17', 'the initial holiday');
    await expectDay(element, '2024-03-16', '16', 'the day before keeps its number');
    await expectDay(element, '2024-03-18', '18', 'the day after keeps its number');
    await expectDay(element, '2024-03-28', '🎂🎉', 'two contents for one date, both shown in document order');
    await expect(element.locator('button[data-date="2024-03-17"]'), 'the cell keeps its accessible name').toHaveAttribute('aria-label', /March 17, 2024/);
  });

  test(`day content: ${host.name} shows day content added and drops day content removed by a render of the page`, async ({ page }) => {
    const element = await open(page, host);
    await expectDay(element, '2024-03-20', '20', 'no content yet');

    await page.getByTestId('dc-add').click();
    await expectDay(element, '2024-03-20', '🌸20', 'the added holiday');

    await page.getByTestId('dc-remove').click();
    await expectDay(element, '2024-03-17', '17', 'the removed holiday gives the day its number back');
    await expectDay(element, '2024-03-20', '🌸20', 'the other holiday stays');
  });

  test(`day content: ${host.name} follows day content toggled inside a component re-rendering on its own`, async ({ page }) => {
    const element = await open(page, host);
    await expectDay(element, '2024-03-22', '22', 'no content yet');

    // the toggle re-renders only the SelfRenderingDayContent components, never the host
    await page.getByTestId('dc-toggle').click();
    await expectDay(element, '2024-03-22', '⭐22', 'the toggled-on content');

    await page.getByTestId('dc-toggle').click();
    await expectDay(element, '2024-03-22', '22', 'the toggled-off content gives the day its number back');
  });

  test(`day content: ${host.name} shows day content of another month after navigating there`, async ({ page }) => {
    const element = await open(page, host);

    await element.locator('[part~="next"]').click();
    await expectDay(element, '2024-04-01', '🐟1', 'the holiday of the next month');
    await expectDay(element, '2024-04-02', '2', 'the day after keeps its number');

    await element.locator('[part~="previous"]').click();
    await expectDay(element, '2024-03-17', '🍀17', 'back in March');
  });
}
