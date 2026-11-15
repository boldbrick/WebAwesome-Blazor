// @ts-check
const fs = require('fs');
const path = require('path');
const { test, expect, openPanel, clearSessions } = require('./fixtures');
const { waitForAllWaReady } = require('../tests/helpers/page-health');

// Captures the screenshots of the demo's WA Inspector page (src\WebAwesome.Blazor.Demo\Pages\Tooling\InspectorPage.razor)
// with the extension in Chromium, on the Settings showcase of Demo.Server with the inspector metadata on. Not a test:
// its own config (playwright.screenshots.config.js) runs it, and the standard and inspector suites do not. Run it again
// after a visible change of the panel or of the showcase:
//
//   $env:WebAwesome__EmitInspectorMetadata = 'true'
//   dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
//   cd tools\e2e; npm run capture:inspector-screenshots
//
// It overwrites the PNG files in wwwroot\images\tooling and logs the size of each: the page gives each image its width
// and height, so update them there when a size changes.

// where the demo serves the images from
const OUTPUT_DIR = path.resolve(__dirname, '..', '..', '..', 'src', 'WebAwesome.Blazor.Demo', 'wwwroot', 'images', 'tooling');

// the reviewed page: demo content without Pro components, so it renders fully on the free CDN
const ROUTE = '/showcases/settings';

// the size of every screenshot, the inspected page and the panel tab alike
const VIEWPORT = { width: 1280, height: 760 };

// the review the screenshots show
const INPUT_LABEL = 'Workspace name';
const INPUT_APPEARANCE = 'filled';
const DENSITY_GROUP_LABEL = 'Interface density';
const DENSITY_COMMENT = 'Make the selected density easier to see: give the active button the brand variant.';
const SWITCH_HINT = 'Applies immediately to all open editor tabs';
const SWITCH_SIZE = 'l';
const SWITCH_COMMENT = 'Auto-save is the main setting of this tab: make the switch larger.';
const THEME_TOKEN = '--wa-color-brand-fill-loud';
const THEME_VALUE = '#7c3aed';
const THEME_FILTER = 'color-brand';

// the room below the auto-save switch when the settings box scrolls it into view
const BOX_MARGIN_PX = 16;

// the room below the content of a panel tab in its screenshot
const CONTENT_MARGIN_PX = 16;

// the time the page and the panel get to paint after a change (outlines, form updates)
const PAINT_MS = 500;

/**
 * Selects the first element of the tag whose attribute has the value, in the panel, and returns its details.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} tag
 * @param {string} attribute
 * @param {string} value
 */
async function selectWhere(panel, tag, attribute, value) {
  const details = await panel.evaluate(async ({ tagName, name, expected }) => {
    const hooks = /** @type {any} */ (window).__waInspectorPanel;
    const tree = await hooks.request('page.tree');
    const ids = [];
    const walk = node => {
      for (const child of node.children) {
        if (child.tag === tagName) ids.push(child.id);
        walk(child);
      }
    };
    walk(tree.root);
    for (const id of ids) {
      const details = await hooks.request('page.details', { id });
      if (details.attributes[name] === expected) {
        await hooks.selectElement(id);
        return hooks.state.details;
      }
    }
    throw new Error(`no <${tagName}> with ${name}="${expected}"`);
  }, { tagName: tag, name: attribute, expected: value });
  // the forms of the element, not of the one selected before
  await expect(panel.locator('#details h2')).toContainText(`<${tag}>`);
  return details;
}

/**
 * Sets an enum attribute of the selected element through the select of its attribute form, as a reviewer does.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} name
 * @param {string} value
 */
async function selectAttributeValue(panel, name, value) {
  const select = panel.locator('#details .form span.name', { hasText: new RegExp(`^${name}$`) }).locator('xpath=following-sibling::*[1]');
  await select.selectOption(value);
}

/**
 * Writes the comment of the selected element's item, as a reviewer does.
 *
 * @param {import('@playwright/test').Page} panel
 * @param {string} text
 */
async function writeComment(panel, text) {
  const comment = panel.locator('#details textarea[placeholder^="What should change here"]');
  await comment.fill(text);
  await comment.blur();
}

/**
 * The review session the panel holds.
 *
 * @param {import('@playwright/test').Page} panel
 */
async function sessionOf(panel) {
  return panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.state.session);
}

/**
 * Saves a screenshot of the page in the output folder, and logs its size for the width and height of the image on the
 * demo page.
 *
 * @param {import('@playwright/test').Page} target
 * @param {string} name
 * @param {string} [contentSelector] the content of a panel tab: the screenshot ends below it, without the empty rest
 */
async function capture(target, name, contentSelector) {
  await target.bringToFront();
  await target.waitForTimeout(PAINT_MS);
  let height = VIEWPORT.height;
  if (contentSelector) {
    const bottom = await target.locator(contentSelector).evaluateAll(elements =>
      Math.max(...elements.map(element => element.getBoundingClientRect().bottom)));
    height = Math.min(height, Math.ceil(bottom) + CONTENT_MARGIN_PX);
  }
  await target.screenshot({ path: path.join(OUTPUT_DIR, name), clip: { x: 0, y: 0, width: VIEWPORT.width, height } });
  console.log(`${name}: ${VIEWPORT.width} x ${height}`);
}

test('capture the screenshots of the WA Inspector demo page', async ({ page, context, serviceWorker, extensionId }) => {
  test.setTimeout(120_000);
  fs.mkdirSync(OUTPUT_DIR, { recursive: true });
  await clearSessions(serviceWorker);

  await page.setViewportSize(VIEWPORT);
  await page.goto(ROUTE);
  await page.waitForSelector('.demo-shell');
  expect(await waitForAllWaReady(page), 'elements that did not upgrade').toEqual([]);

  const panel = await openPanel({ context, serviceWorker, extensionId }, page);
  await panel.setViewportSize(VIEWPORT);
  await expect(panel.locator('#status'), 'the page has the Blazor placement metadata (EmitInspectorMetadata)').not.toContainText('no Blazor placement metadata');

  // item 1: the filled appearance of the workspace name
  await selectWhere(panel, 'wa-input', 'label', INPUT_LABEL);
  await selectAttributeValue(panel, 'appearance', INPUT_APPEARANCE);
  await expect(page.locator(`wa-input[label="${INPUT_LABEL}"]`)).toHaveAttribute('appearance', INPUT_APPEARANCE);

  // item 2: a comment on the density buttons
  await selectWhere(panel, 'wa-button-group', 'label', DENSITY_GROUP_LABEL);
  await writeComment(panel, DENSITY_COMMENT);

  // item 3: a larger auto-save switch, with the reason
  await selectWhere(panel, 'wa-switch', 'hint', SWITCH_HINT);
  await selectAttributeValue(panel, 'size', SWITCH_SIZE);
  await expect(page.locator(`wa-switch[hint="${SWITCH_HINT}"]`)).toHaveAttribute('size', SWITCH_SIZE);
  await writeComment(panel, SWITCH_COMMENT);
  await expect.poll(async () => (await sessionOf(panel)).items.map(item => item.number)).toEqual([1, 2, 3]);

  // the theme: another brand color for the whole page
  await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.showTab('theme'));
  await panel.locator('#theme-filter').fill(THEME_FILTER);
  const token = panel.locator('#theme .token-row')
    .filter({ has: panel.locator('span.name', { hasText: new RegExp(`^${THEME_TOKEN}$`) }) })
    .locator('input[type="text"]');
  await token.fill(THEME_VALUE);
  await token.press('Enter');
  await expect.poll(async () => (await sessionOf(panel)).theme).toEqual({ [THEME_TOKEN]: THEME_VALUE });
  await token.blur();
  await capture(panel, 'inspector-theme.png', '#theme > *');

  // the page with the numbered outlines of the three items: the settings box scrolls on its own, so it scrolls until
  // the auto-save switch is in view; the focus outline of the heading (FocusOnNavigate) goes
  await page.bringToFront();
  await page.evaluate(({ hint, margin }) => {
    window.scrollTo(0, 0);
    if (document.activeElement instanceof HTMLElement) document.activeElement.blur();
    const toggle = /** @type {HTMLElement} */ (document.querySelector(`wa-switch[hint="${hint}"]`));
    const box = /** @type {HTMLElement} */ (toggle.closest('wa-page')?.parentElement);
    box.scrollTop += Math.max(0, toggle.getBoundingClientRect().bottom - box.getBoundingClientRect().bottom + margin);
  }, { hint: SWITCH_HINT, margin: BOX_MARGIN_PX });
  await expect.poll(async () => new Set(await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.request('page.drawnMarkers'))).size)
    .toBe(3);
  await capture(page, 'inspector-page-markers.png');

  await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.showTab('changes'));
  await capture(panel, 'inspector-changes.png', '#changes > *');

  // the Elements tab with the workspace name input selected: the row in the middle of the tree, the details from the top
  await panel.evaluate(() => /** @type {any} */ (window).__waInspectorPanel.showTab('elements'));
  await selectWhere(panel, 'wa-input', 'label', INPUT_LABEL);
  await panel.evaluate(() => {
    document.querySelector('#tree .node-row.selected')?.scrollIntoView({ block: 'center' });
    /** @type {HTMLElement} */ (document.getElementById('details')).scrollTop = 0;
  });
  await capture(panel, 'inspector-elements.png');

  await clearSessions(serviceWorker);
});
