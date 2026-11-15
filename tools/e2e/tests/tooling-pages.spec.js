// @ts-check
const { test, expect } = require('./helpers/test');
const { TOOLING_ROUTES } = require('./helpers/routes');

// The screenshots of the Tooling pages (src\WebAwesome.Blazor.Demo\wwwroot\images\tooling) load: every image answers
// 200 and decodes. The route sweep cannot see a missing image, because its console filter drops network failures
// (helpers\page-health.js). The image URLs must be relative, so that they resolve against <base href>: GitHub Pages
// serves the WebAssembly demo under the repository path, and the deploy rewrites only the base href.

for (const route of TOOLING_ROUTES) {
  test(`the images of ${route} load from relative URLs`, async ({ page }) => {
    /** @type {{ url: string, status: number }[]} */
    const failures = [];
    page.on('response', response => {
      if (response.request().resourceType() === 'image' && response.status() >= 400) {
        failures.push({ url: response.url(), status: response.status() });
      }
    });

    await page.goto(route);
    await page.waitForSelector('.demo-shell');
    const images = page.locator('main.demo-content img');
    await expect(images.first()).toBeAttached();

    const sources = await images.evaluateAll(elements => elements.map(element => element.getAttribute('src') || ''));
    expect(sources.filter(source => /^(\/|[a-z]+:)/i.test(source)), 'image URLs that ignore the base href').toEqual([]);

    // every image decoded, with an alt text for the reader who cannot see it
    await expect.poll(() => images.evaluateAll(elements => elements
      .filter(element => !(/** @type {HTMLImageElement} */ (element).complete && /** @type {HTMLImageElement} */ (element).naturalWidth > 0))
      .map(element => element.getAttribute('src'))), { message: 'images that did not load' }).toEqual([]);
    const withoutAlt = await images.evaluateAll(elements => elements
      .filter(element => !(element.getAttribute('alt') || '').trim())
      .map(element => element.getAttribute('src')));
    expect(withoutAlt, 'images without an alt text').toEqual([]);
    expect(failures, 'image requests that failed').toEqual([]);
  });
}
