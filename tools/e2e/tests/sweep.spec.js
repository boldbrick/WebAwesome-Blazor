// @ts-check
const { test, expect } = require('./helpers/test');
const { getAllRoutes } = require('./helpers/routes');
const { WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');
const { collectProblems, waitForAllWaReady } = require('./helpers/page-health');

// Visits every demo route and fails on any page error, on any console error or warning that is not known noise,
// and (through the common test base, helpers\test.js) on the Blazor error UI. The demo pages set non-default enum,
// boolean and number values, so an invalid value a wrapper emits surfaces here: Web Awesome throws (a RangeError
// from Intl for a bad wa-relative-time format reaches 'pageerror') or warns (a deprecated size, an icon button
// without a label, a popover target that does not exist). Interactions are the interaction sweep's
// (interaction-sweep.spec.js).

// the time to let late work settle after the elements rendered (timers, Intl formatting in updated(), relays)
const SETTLE_MS = 1000;

for (const route of getAllRoutes()) {
  test(`no unhandled errors on ${route}`, async ({ page }) => {
    const problems = collectProblems(page);

    // 'load' rather than 'networkidle': some pages (e.g. comparison, carousel autoplay) have
    // continuous background network/animation activity that never goes idle.
    await page.goto(route, { waitUntil: 'load' });
    await page.waitForSelector('.demo-shell', { timeout: WA_READY_TIMEOUT_MS });

    const notReady = await waitForAllWaReady(page);
    if (notReady.length > 0) problems.push(`[not ready] ${notReady.join(', ')} not defined after ${WA_READY_TIMEOUT_MS} ms`);
    await page.waitForTimeout(SETTLE_MS);

    expect(problems, `Unhandled error(s) on ${route}:\n${problems.join('\n')}`).toEqual([]);
  });
}
