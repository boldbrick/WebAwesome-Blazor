// @ts-check
const { test, expect } = require('./helpers/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// WebAwesomeOptions.IconPath on the harness /testing/icon-path. The page renders its own WebAwesomeAssets with the
// host's asset settings plus the relative icon path "fragments/icon-path" (the demo's test-only folder
// wwwroot\fragments\icon-path, our own trivial SVGs), so the component emits the module that calls Web Awesome's
// setIconPath before any icon renders. Every default-library icon then loads from the path, also the icons a component
// renders itself (the zoom buttons of wa-zoomable-frame), and no request reaches the Font Awesome CDN. Free CDN; the
// other demo pages keep their icons.

const HARNESS = '/testing/icon-path';

// the relative icon path of the harness, which WebAwesomeAssets resolves against the app base
const ICON_PATH = 'fragments/icon-path';

// the hosts of the Font Awesome CDN (ka-f.fontawesome.com, ka-p.fontawesome.com) and any other Font Awesome host
const FONT_AWESOME_HOST = /(^|\.)fontawesome\.com$/i;

/**
 * Reads the marker of the drawn svg in the shadow root of a wa-icon (the data-harness-icon attribute of the harness
 * artwork, "<folder>/<name>"), or null while none is drawn.
 *
 * @param {import('@playwright/test').Locator} icon
 * @returns {Promise<string|null>}
 */
function drawnIcon(icon) {
  return icon.evaluate(el => el.shadowRoot?.querySelector('svg')?.getAttribute('data-harness-icon') ?? null);
}

test('the default icon library loads from WebAwesomeOptions.IconPath and never from the Font Awesome CDN', async ({ page }) => {
  // every Font Awesome request is recorded and aborted from the first document on
  /** @type {string[]} */
  const fontAwesomeRequests = [];
  await page.route(url => FONT_AWESOME_HOST.test(url.hostname), route => {
    fontAwesomeRequests.push(route.request().url());
    return route.abort();
  });

  /** @type {string[]} */
  const svgRequests = [];
  page.on('request', request => {
    const url = new URL(request.url());
    if (url.pathname.endsWith('.svg')) svgRequests.push(url.href);
  });

  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-icon', 'wa-zoomable-frame']);

  // the component rendered one module after the loader: it imports setIconPath from the loader URL itself and passes
  // the path resolved against the app base
  const head = await page.evaluate(iconPath => {
    const loaders = [...document.head.querySelectorAll('script[type="module"][src]')]
      .filter(s => new URL(s.src).pathname.endsWith('/webawesome.loader.js'));
    const modules = [...document.head.querySelectorAll('script[type="module"]:not([src])')]
      .map(s => s.textContent ?? '')
      .filter(text => text.includes('setIconPath'));
    return {
      loaderCount: loaders.length,
      loaderSrc: loaders[0]?.getAttribute('src') ?? null,
      modules,
      expectedPath: new URL(iconPath, document.baseURI).href,
    };
  }, ICON_PATH);
  expect(head.loaderCount).toBe(1);
  expect(head.modules).toHaveLength(1);
  expect(head.modules[0]).toContain(`from '${head.loaderSrc}'`);
  expect(head.modules[0]).toContain(`setIconPath('${head.expectedPath}')`);

  // the default library resolves the family and variant to the folder beneath the path (classic solid -> solid)
  await expect.poll(() => drawnIcon(page.getByTestId('ip-icon-solid'))).toBe('solid/harness-square');
  await expect.poll(() => drawnIcon(page.getByTestId('ip-icon-regular'))).toBe('regular/harness-square');

  // the icons wa-zoomable-frame renders in its shadow root load from the path too
  const frame = page.getByTestId('ip-zoomable-frame');
  await expect.poll(() => drawnIcon(frame.locator('wa-icon[name="plus"]'))).toBe('solid/plus');
  await expect.poll(() => drawnIcon(frame.locator('wa-icon[name="minus"]'))).toBe('solid/minus');

  expect(svgRequests).toEqual(expect.arrayContaining([
    `${head.expectedPath}/solid/harness-square.svg`,
    `${head.expectedPath}/regular/harness-square.svg`,
    `${head.expectedPath}/solid/plus.svg`,
    `${head.expectedPath}/solid/minus.svg`,
  ]));
  expect(fontAwesomeRequests).toEqual([]);
});
