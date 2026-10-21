// @ts-check
const { test, expect, openHarness, openPanel, treeNodes } = require('./fixtures');

// The element tree of the panel and the Blazor placement metadata on the harness page
// (src\WebAwesome.Blazor.Demo\Pages\Testing\InspectorHarness.razor, served by Demo.Server with
// WebAwesome__EmitInspectorMetadata=true).

test('the tree lists the wa- elements and the utility-class element', async ({ page, context, serviceWorker, extensionId }) => {
  await openHarness(page);
  const panel = await openPanel({ context, serviceWorker, extensionId }, page);

  const nodes = await treeNodes(panel);
  const tags = new Set(nodes.map(node => node.tag));
  for (const tag of ['wa-button', 'wa-badge', 'wa-card']) expect(tags, `the tree holds <${tag}>`).toContain(tag);

  // the task badges (the demo navigation has badges of its own)
  const badges = nodes.filter(node => node.tag === 'wa-badge' && /^(Open|Done)$/.test(node.label));
  expect(badges.length, 'one badge per task row').toBe(3);
  expect(badges.every(node => node.component === 'WaBadge'), 'the badges are WaBadge wrappers').toBe(true);
  expect(badges.map(node => node.count), 'each badge counts the instances of its placement').toEqual([3, 3, 3]);
  expect(new Set(badges.map(node => node.attributes.variant)), 'the variants the data sets').toEqual(new Set(['neutral', 'success']));

  const note = nodes.find(node => node.tag === 'div' && node.classes.includes('wa-gap-m'));
  expect(note, 'the plain div with the wa-gap-m utility class is in the tree').toBeTruthy();
  expect(note.isWa).toBe(false);
  expect(nodes.some(node => node.tag === 'div' && node.classes.includes('wa-stack')), 'the WaStack div is in the tree').toBe(true);
  expect(nodes.some(node => node.tag === 'div' && node.classes.includes('wa-cluster')), 'the WaCluster div is in the tree').toBe(true);

  // the panel renders the rows
  await expect(panel.locator('.node-row', { hasText: 'wa-gap-m' })).toHaveCount(1);
  await expect(panel.locator('#status')).toContainText('Blazor placement metadata: on');
});

test('the harness elements carry the Blazor placement metadata', async ({ page }) => {
  await openHarness(page);

  const meta = await page.evaluate(() => {
    const read = element => ({
      component: element.getAttribute('data-wablazor-component'),
      chain: element.getAttribute('data-wablazor-chain'),
      placement: element.getAttribute('data-wablazor-placement'),
      site: element.getAttribute('data-wablazor-site'),
    });
    const all = testId => [...document.querySelectorAll(`[data-testid="${testId}"]`)].map(read);
    return {
      badges: all('ih-badge'),
      cardAction: all('ih-action-card')[0],
      noteAction: all('ih-action-note')[0],
      toggle: all('ih-toggle')[0],
      reopen: all('ih-reopen')[0],
    };
  });

  expect(meta.badges.length).toBe(3);
  for (const badge of meta.badges) {
    expect(badge.component).toBe('WaBadge');
    expect(badge.chain, 'the chain names the page').toContain('InspectorHarness');
    expect(badge.placement, 'a placement hash').toMatch(/^[0-9a-f]{12}$/);
    expect(badge.site, 'a site hash').toMatch(/^[0-9a-f]{12}$/);
  }
  expect(new Set(meta.badges.map(badge => badge.placement)).size, 'the loop badges share one placement').toBe(1);

  // the reusable action: one source site in InspectorHarnessAction, two placements on the page
  expect(meta.cardAction.chain).toContain('InspectorHarnessAction');
  expect(meta.cardAction.site, 'the same site').toBe(meta.noteAction.site);
  expect(meta.cardAction.placement, 'another placement').not.toBe(meta.noteAction.placement);

  // two buttons of one toolbar: two source positions
  expect(meta.toggle.placement).not.toBe(meta.reopen.placement);
  expect(meta.toggle.site).not.toBe(meta.reopen.site);
});
