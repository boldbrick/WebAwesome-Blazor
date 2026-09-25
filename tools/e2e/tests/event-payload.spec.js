// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');
const { PAYLOAD_CASES, PAYLOADS } = require('./helpers/payload-cases');

// Payload shape of the events the JS initializer projects by hand (review X6); see helpers\payload-cases.js.

for (const c of PAYLOAD_CASES) {
  test(`payload: ${c.name}`, async ({ page }) => {
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    await page.goto(PAYLOADS);
    await page.waitForSelector('.demo-shell');
    if (c.pro) await skipUnlessProUpgrades(page, c.tags);
    await waitForWaReady(page, c.tags);

    await c.run(page);

    expect(pageErrors, 'no page errors').toEqual([]);
  });
}
