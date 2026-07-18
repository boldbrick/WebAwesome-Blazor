// @ts-check
const { test, expect } = require('@playwright/test');
const { getAllRoutes } = require('./helpers/routes');

// Console/page errors that indicate a genuine app crash, as opposed to benign resource-load
// noise (e.g. Font Awesome Pro icon 403s when no kit code is configured in this dev environment,
// which is expected per the project's no-hardcoded-credentials rule).
function isRealAppError(text) {
  return (
    text.includes('Unhandled exception') ||
    text.startsWith('crit:') ||
    text.includes('Uncaught') ||
    text.includes('InvalidOperationException') ||
    text.includes('JSException')
  );
}

for (const route of getAllRoutes()) {
  test(`no unhandled errors on ${route}`, async ({ page }) => {
    const problems = [];
    page.on('pageerror', err => problems.push(`[pageerror] ${err.message}`));
    page.on('console', msg => {
      if (msg.type() === 'error' && isRealAppError(msg.text())) {
        problems.push(`[console.error] ${msg.text()}`);
      }
    });

    // 'load' rather than 'networkidle': some pages (e.g. comparison, carousel autoplay) have
    // continuous background network/animation activity that never goes idle.
    await page.goto(route, { waitUntil: 'load' });
    await page.waitForSelector('.demo-shell', { timeout: 15_000 });
    // let Blazor finish first render and any Web Awesome custom-element upgrade/JS-interop calls
    await page.waitForTimeout(1000);

    expect(problems, `Unhandled error(s) on ${route}:\n${problems.join('\n')}`).toEqual([]);
  });
}
