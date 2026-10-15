// @ts-check
const { test, expect } = require('./helpers/test');

// The optional package WebAwesome.Blazor.Extensions brings its JavaScript as a Blazor JS initializer
// (wwwroot\WebAwesome.Blazor.Extensions.lib.module.js), which the runtime imports only when the static web asset
// reaches the app through the whole chain: the package's static web asset with the JSModule trait, the demo's
// project reference, and the boot configuration of the host. A break anywhere fails silently, so the later features
// of the package would never start. The file name may carry the fingerprint of the static web asset.
const EXTENSIONS_INITIALIZER =
  /\/_content\/WebAwesome\.Blazor\.Extensions\/WebAwesome\.Blazor\.Extensions(\.[0-9a-z]+)?\.lib\.module\.js$/;

test('the demo loads the JS initializer of WebAwesome.Blazor.Extensions', async ({ page }) => {
  /** @type {{ url: string, status: number }[]} */
  const loads = [];
  page.on('response', response => {
    const url = new URL(response.url());
    if (EXTENSIONS_INITIALIZER.test(url.pathname)) loads.push({ url: url.pathname, status: response.status() });
  });

  await page.goto('/');
  await page.waitForSelector('.demo-shell');

  await expect.poll(() => loads.length, { message: 'the Extensions JS initializer was requested' }).toBeGreaterThan(0);
  expect(loads.filter(load => load.status !== 200), 'initializer requests that did not answer 200').toEqual([]);
});
