// @ts-check
const fs = require('fs');
const path = require('path');
const { test, expect, ERROR_UI_ID } = require('./helpers/test');

// The common test base (helpers\test.js): its page fixture fails a test whose page showed the Blazor error UI, even
// when the test itself passes, and every spec must use it (no browser for that check). A spec importing
// @playwright/test directly would pass with the error UI showing.

// the import of the common test base, as every spec writes it
const COMMON_BASE_IMPORT = "require('./helpers/test')";

// a direct import of Playwright's test runner
const DIRECT_IMPORT = /require\(['"]@playwright\/test['"]\)/;

test('every spec uses the common test base with the Blazor crash guard', () => {
  const specs = fs.readdirSync(__dirname).filter(name => name.endsWith('.spec.js'));
  expect(specs.length, 'specs found').toBeGreaterThan(1);

  const misses = specs.filter(name => {
    const source = fs.readFileSync(path.join(__dirname, name), 'utf8');
    return !source.includes(COMMON_BASE_IMPORT) || DIRECT_IMPORT.test(source);
  });
  expect(misses, `specs not on the common test base (take test and expect from ${COMMON_BASE_IMPORT})`).toEqual([]);
});

// expected to fail: the page shows the error UI as Blazor does on an unhandled exception (style display: block), the
// test body passes, and the fixture must fail the test when it ends
test('the crash guard fails a test whose page showed the Blazor error UI', async ({ page }) => {
  test.fail(true, 'the page fixture must fail this test');
  await page.goto('/');
  await page.waitForSelector('.demo-shell');
  await page.evaluate(id => { /** @type {HTMLElement} */ (document.getElementById(id)).style.display = 'block'; }, ERROR_UI_ID);
  await page.evaluate(id => { /** @type {HTMLElement} */ (document.getElementById(id)).style.display = 'none'; }, ERROR_UI_ID);
});