// @ts-check
const { test, expect } = require('@playwright/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// Browser acceptance for WaIconLibraryService. Web Awesome keeps its icon library registry and default
// icon family as module state of the page's own Web Awesome entry point, so the interop module must reach
// that exact module instance; the bUnit/Moq tests mock the interop layer and cannot prove it. Until the
// 3.12.0 fix the service invoked JS functions that webawesome-interop.js never exported, and every call
// threw at runtime while its tests passed. The driving controls live on the Icon demo page.

const ROUTE = '/components/icon';

/**
 * Returns whether the wa-icon has drawn a real image: the initial placeholder svg and a failed resolve
 * both leave the shadow root without any svg content.
 *
 * @param {import('@playwright/test').Locator} icon
 */
function hasDrawnSvg(icon) {
  return icon.evaluate(el => (el.shadowRoot?.querySelector('svg')?.childElementCount ?? 0) > 0);
}

test.beforeEach(async ({ page }) => {
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-button', 'wa-icon']);
});

test('registered icon libraries resolve through the page Web Awesome instance', async ({ page }) => {
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));

  const lucide = page.locator('wa-icon[data-testid="icon-lucide"]');
  const heroicons = page.locator('wa-icon[data-testid="icon-heroicons"]');
  const tabler = page.locator('wa-icon[data-testid="icon-tabler"]');

  // the libraries don't exist yet, so nothing is drawn
  expect(await hasDrawnSvg(lucide)).toBe(false);
  expect(await hasDrawnSvg(tabler)).toBe(false);

  await page.locator('wa-button[data-testid="icon-libraries-register"]').click();
  await expect(page.getByTestId('icon-libraries-status')).toHaveText('Registered lucide, heroicons, tabler');

  // registration re-resolves the icons already on the page (URL templates from .NET)
  await expect.poll(() => hasDrawnSvg(lucide)).toBe(true);
  await expect.poll(() => hasDrawnSvg(heroicons)).toBe(true);
  await expect.poll(() => hasDrawnSvg(tabler)).toBe(true);

  // the mutator named from .NET ("demoIcons.thinStroke") ran on the Tabler svg
  await expect.poll(() => tabler.evaluate(el => el.shadowRoot?.querySelector('svg')?.getAttribute('stroke-width')))
    .toBe('1.25');

  // after unregistering, a new Tabler icon stays empty while a Lucide control icon still draws
  await page.locator('wa-button[data-testid="icon-libraries-unregister"]').click();
  await expect(page.getByTestId('icon-libraries-status')).toHaveText('Unregistered tabler');
  await page.evaluate(async () => {
    for (const [library, name] of [['tabler', 'brand-github'], ['lucide', 'feather']]) {
      const icon = document.createElement('wa-icon');
      icon.setAttribute('library', library);
      icon.setAttribute('name', name);
      icon.dataset.testid = `late-${library}`;
      document.body.appendChild(icon);
    }
  });
  await expect.poll(() => hasDrawnSvg(page.locator('wa-icon[data-testid="late-lucide"]'))).toBe(true);
  expect(await hasDrawnSvg(page.locator('wa-icon[data-testid="late-tabler"]'))).toBe(false);

  expect(errors).toEqual([]);
});

test('default icon family set from .NET is the family of the page Web Awesome instance', async ({ page }) => {
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));

  // reads the family straight from the entry point module the page itself loaded
  const pageDefaultFamily = () => page.evaluate(async () => {
    const script = /** @type {HTMLScriptElement[]} */ (Array.from(document.querySelectorAll('script[type="module"][src]')))
      .find(s => new URL(s.src).pathname.endsWith('/webawesome.loader.js'));
    if (!script) throw new Error('no Web Awesome loader script tag on the page');
    const webAwesome = await import(script.src);
    return webAwesome.getDefaultIconFamily();
  });

  expect(await pageDefaultFamily()).toBe('classic');

  await page.locator('wa-button[data-testid="icon-family-sharp"]').click();
  await expect(page.getByTestId('icon-family-current')).toHaveText('sharp');
  expect(await pageDefaultFamily()).toBe('sharp');

  await page.locator('wa-button[data-testid="icon-family-classic"]').click();
  await expect(page.getByTestId('icon-family-current')).toHaveText('classic');
  expect(await pageDefaultFamily()).toBe('classic');

  expect(errors).toEqual([]);
});
