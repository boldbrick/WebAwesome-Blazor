// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady, skipUnlessProUpgrades } = require('./helpers/wa-ready');
const { expectFired } = require('./helpers/event-log');
const { EVENT_CASES, KNOWN_DEFECT_CASES } = require('./helpers/event-cases');

// Real-interaction dispatch proof for the wrappers' EventCallbacks (review X1). bUnit can only trigger the
// handler a wrapper rendered; it cannot show that Web Awesome dispatches the event on the host, whether it
// bubbles or composes far enough for Blazor, or that the JS initializer registers it - the 3.11 dead
// callbacks (bound to events that never fire) passed every bUnit test. Each case drives a component with the
// mouse or keyboard, and then every callback it lists must have reached .NET, as rendered by the harness
// pages' event log (src\WebAwesome.Blazor.Demo\Pages\Testing) or by a demo page counter.
// event-coverage.spec.js checks the case lists against every callback the wrappers bind.

for (const c of EVENT_CASES) {
  test(`dispatch: ${c.name}`, async ({ page, context }) => {
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));
    if (c.clipboard) await context.grantPermissions(['clipboard-read', 'clipboard-write']);

    await page.goto(c.route);
    await page.waitForSelector('.demo-shell');
    if (c.pro) await skipUnlessProUpgrades(page, c.tags);
    await waitForWaReady(page, c.tags);

    await c.run(page);

    for (const id of c.callbacks) await expectFired(page, id);
    expect(pageErrors, 'no page errors').toEqual([]);
  });
}

// expected failures: see KNOWN_DEFECT_CASES; a pass means the defect is gone and the case must be promoted
for (const c of KNOWN_DEFECT_CASES) {
  test(`known defect: ${c.name}`, async ({ page }) => {
    test.fail(true, `known defect, exempted as '${c.exemption}' in tools\\e2e\\data\\event-coverage-exemptions.json`);

    await page.goto(c.route);
    await page.waitForSelector('.demo-shell');
    if (c.pro) await skipUnlessProUpgrades(page, c.tags);
    await waitForWaReady(page, c.tags);

    await c.run(page);
  });
}
