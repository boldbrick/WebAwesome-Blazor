// @ts-check
const { test, expect } = require('./helpers/test');
const { waitForWaReady } = require('./helpers/wa-ready');

// The function references of an icon library (IconLibraryOptions.ResolverFunction and MutatorFunction, docs\technical\technical.md,
// "JavaScript function references") on the harness /testing/icon-library. Each button registers one library through
// WaIconLibraryService; the resolver is a WaJsFunction that wa-icon calls as (name, family, variant, autoWidth, ctx),
// the mutator one it calls as (svg, icon, ctx). The icons are data URLs and a local sprite sheet, so the spec needs no
// network, and icon library registration is free: everything runs on the free CDN.

const HARNESS = '/testing/icon-library';

/**
 * Reads an attribute of the drawn svg in the shadow root of a wa-icon, or null while none is drawn.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} testId
 * @param {string} name
 * @returns {Promise<string|null>}
 */
function svgAttribute(page, testId, name) {
  return page.getByTestId(testId).evaluate((el, attribute) => {
    const svg = el.shadowRoot?.querySelector('svg');
    return svg && svg.childElementCount > 0 ? svg.getAttribute(attribute) : null;
  }, name);
}

/**
 * Registers the library of a harness button and waits for its status.
 *
 * @param {import('@playwright/test').Page} page
 * @param {string} key
 * @param {string} status
 */
async function register(page, key, status) {
  await page.getByTestId(`il-register-${key}`).click();
  await expect(page.getByTestId(`il-status-${key}`)).toHaveText(status);
}

/**
 * Collects the console errors of a page.
 *
 * @param {import('@playwright/test').Page} page
 * @returns {string[]}
 */
function collectConsoleErrors(page) {
  const errors = [];
  page.on('console', message => {
    if (message.type() === 'error') errors.push(message.text());
  });
  return errors;
}

test.beforeEach(async ({ page }) => {
  await page.goto(HARNESS);
  await page.waitForSelector('.demo-shell');
  await waitForWaReady(page, ['wa-button', 'wa-icon']);
});

test('a module resolver draws icons and gets the icon arguments and ctx; a module mutator changes the svg', async ({ page }) => {
  // the library does not exist yet, so nothing is drawn
  expect(await svgAttribute(page, 'il-icon-module', 'data-name')).toBeNull();

  await register(page, 'module', 'registered');

  // registration re-resolves the icons already on the page: (name, family, variant, autoWidth) and then ctx
  await expect.poll(() => svgAttribute(page, 'il-icon-module', 'data-name')).toBe('dot');
  expect(await svgAttribute(page, 'il-icon-module', 'data-family')).toBe('sample');
  expect(await svgAttribute(page, 'il-icon-module', 'data-variant')).toBe('solid');
  expect(await svgAttribute(page, 'il-icon-module', 'data-auto-width')).toBe('false');
  expect(await svgAttribute(page, 'il-icon-module', 'fill')).toBe('rgb(255, 0, 0)');
  expect(await svgAttribute(page, 'il-icon-module', 'data-library')).toBe('harness-module');
  expect(await svgAttribute(page, 'il-icon-module', 'data-element')).toBe('undefined');
  await expect.poll(() => svgAttribute(page, 'il-icon-module-auto', 'data-auto-width')).toBe('true');

  // the mutator got the svg, the wa-icon and ctx (args and the library name)
  expect(await svgAttribute(page, 'il-icon-module', 'data-mutated')).toBe('module|wa-icon|harness-module');
});

test('a registered resolver that returns a Promise draws icons; a global mutator changes the svg', async ({ page }) => {
  await register(page, 'registered', 'registered');

  await expect.poll(() => svgAttribute(page, 'il-icon-registered', 'data-name')).toBe('dot');
  expect(await svgAttribute(page, 'il-icon-registered', 'fill')).toBe('rgb(0, 0, 255)');
  expect(await svgAttribute(page, 'il-icon-registered', 'data-library')).toBe('harness-registered');

  // demoIcons.thinStroke, referred to with WaJsFunction.Global
  expect(await svgAttribute(page, 'il-icon-registered', 'stroke-width')).toBe('1.25');
});

test('a resolver of a sprite sheet library returns a sprite reference that the icon renders', async ({ page }) => {
  await register(page, 'sprite', 'registered');

  const icon = page.getByTestId('il-icon-sprite');
  await expect.poll(() => icon.evaluate(el => el.shadowRoot?.querySelector('use')?.getAttribute('href') ?? null))
    .toBe('fragments/icon-sprite.svg#star');

  // the referenced symbol renders, and the mutator ran on the sprite svg too
  await expect.poll(() => icon.evaluate(el => el.shadowRoot?.querySelector('use')?.getBBox().width ?? 0)).toBeGreaterThan(0);
  expect(await icon.evaluate(el => el.shadowRoot?.querySelector('svg')?.getAttribute('data-mutated') ?? null))
    .toBe('sprite|wa-icon|harness-sprite');
});

test('the obsolete global mutator path still mutates until its removal', async ({ page }) => {
  await register(page, 'obsolete', 'registered');

  await expect.poll(() => svgAttribute(page, 'il-icon-obsolete', 'data-name')).toBe('dot');
  expect(await svgAttribute(page, 'il-icon-obsolete', 'stroke-width')).toBe('1.25');
});

test('a resolver that does not resolve fails the registration; a mutator that does not resolve is left out', async ({ page }) => {
  const consoleErrors = collectConsoleErrors(page);

  // no library: the .NET call throws, and the icon draws nothing
  await register(page, 'bad-resolver', 'InvalidOperationException');
  expect(await svgAttribute(page, 'il-icon-bad-resolver', 'data-name')).toBeNull();
  expect(consoleErrors.some(text => text.includes('icon library "harness-bad-resolver" resolver')
    && text.includes("export 'missingResolver'"))).toBe(true);

  // the library without a mutator: the icon draws unchanged
  await register(page, 'bad-mutator', 'registered');
  await expect.poll(() => svgAttribute(page, 'il-icon-bad-mutator', 'data-name')).toBe('dot');
  expect(await svgAttribute(page, 'il-icon-bad-mutator', 'data-mutated')).toBeNull();
  expect(await svgAttribute(page, 'il-icon-bad-mutator', 'stroke-width')).toBeNull();
  expect(consoleErrors.some(text => text.includes('icon library "harness-bad-mutator" mutator')
    && text.includes("global function 'demoIcons.missingMutator'"))).toBe(true);
});
