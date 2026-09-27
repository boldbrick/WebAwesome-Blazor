// @ts-check
const { test, expect, blazorErrorUiProblems } = require('./helpers/test');
const { getAllRoutes, HARNESS_ROUTES } = require('./helpers/routes');
const { WA_READY_TIMEOUT_MS } = require('./helpers/wa-ready');
const { PRO_TAGS, collectProblems, waitForAllWaReady } = require('./helpers/page-health');
const {
  CONTENT_SELECTOR, SWEEP_ATTRIBUTE, INITIAL_ATTRIBUTE, CANDIDATE_SELECTOR, OVERLAY_SELECTOR, REVEALER_KINDS, TYPED_VALUES,
  nextCandidate, drive, settle, closeOverlays,
} = require('./helpers/interactions');

// Drives every demo page the way a curious user would: each enabled interactive element in the page's content area
// once (buttons, switches, checkboxes, radios, tabs, disclosures, selects and comboboxes with an option picked,
// inputs with a value typed, tag inputs with a tag entered, sliders, ratings, pickers, dropdowns with an item picked, tree items, removable tags,
// pagination, carousels, split panels, comparisons), with every dialog, drawer, popover or menu it opens closed
// again. After each interaction the page clock jumps past the pending timers (toasts, delays, autoplay), so what an
// interaction starts also finishes, and the page must still be healthy: no Blazor error UI, no page error, no
// console error or warning beyond the sweep's noise filter (helpers\page-health.js). sweep.spec.js covers the
// passive render of the same routes; the showcase flows (showcase-*.spec.js) assert what the interactions render.
//
// The harness pages (/testing/...) are not swept: their dedicated specs drive every control with exact assertions,
// and some of their controls deliberately leave a page in an unusual state (a toast item the model keeps).

const ROUTES = getAllRoutes().filter(route => !HARNESS_ROUTES.includes(route));

// a real click is a user gesture, which lets a page write to the clipboard (the data grid's copy, the copy buttons);
// a headless browser denies it without the permission
test.use({ permissions: ['clipboard-read', 'clipboard-write'] });

// elements the sweep does not drive on any page, with the reason
const COMMON_EXCLUSIONS = [
  // the demo's "Source" disclosure under each example: a native details element without Blazor state, repeated
  // on every component page
  { selector: 'details.example-code', reason: 'demo chrome (the example source disclosure)' },
];

// elements the sweep does not drive on one page, with the reason: an interaction that legitimately leaves the page
// (a navigation, a download, a new window) or one the sweep cannot drive generically
/** @type {Record<string, { selector: string, reason: string }[]>} */
const ROUTE_EXCLUSIONS = {
};

// the longest the interactions of one page may take; a page with more is cut short, visibly (an annotation)
const PAGE_BUDGET_MS = 30_000;

// the most interactions of one page, a guard against a page that renders a new element for every interaction
const MAX_INTERACTIONS = 250;

// set to list every driven and passed-over element of a page in the test's annotations
const DETAIL_VARIABLE = 'E2E_SWEEP_DETAIL';

// the longest one sweep test may take: the boot (up to WA_READY_TIMEOUT_MS under load) plus the page budget
const TEST_TIMEOUT_MS = 120_000;

/**
 * The first line of an error, for the report of an element that could not be driven.
 *
 * @param {unknown} error
 */
function firstLine(error) {
  return String(error instanceof Error ? error.message : error).split('\n')[0];
}

for (const route of ROUTES) {
  test(`interactions on ${route}`, async ({ page }) => {
    test.setTimeout(TEST_TIMEOUT_MS);
    const problems = collectProblems(page);
    const failures = [];
    const notDriven = [];
    const exclusions = [...COMMON_EXCLUSIONS, ...(ROUTE_EXCLUSIONS[route] ?? [])];

    // a native dialog or a new window would block or leave the page: dismiss it, and fail on a new window
    page.on('dialog', dialog => dialog.dismiss().catch(() => undefined));
    page.on('popup', popup => {
      failures.push(`an interaction opened a new window (${popup.url()})`);
      popup.close().catch(() => undefined);
    });

    // the fake clock must be installed before the app starts, so the timers the page sets up can be fast-forwarded;
    // it runs on real time until the sweep jumps it
    await page.clock.install();
    await page.goto(route, { waitUntil: 'load' });
    await page.waitForSelector('.demo-shell', { timeout: WA_READY_TIMEOUT_MS });
    const notReady = await waitForAllWaReady(page);
    if (notReady.length > 0) problems.push(`[not ready] ${notReady.join(', ')} not defined after ${WA_READY_TIMEOUT_MS} ms`);
    await settle(page);
    expect([...problems, ...blazorErrorUiProblems(page)], `the page is healthy before any interaction:\n${problems.join('\n')}`).toEqual([]);

    const started = Date.now();
    const driven = [];
    const passedOver = [];
    let sequence = 0;
    for (;;) {
      if (Date.now() - started > PAGE_BUDGET_MS || sequence >= MAX_INTERACTIONS) {
        test.info().annotations.push({ type: 'interaction budget', description: `stopped after ${sequence} interactions (${Date.now() - started} ms)` });
        break;
      }

      const next = await page.evaluate(nextCandidate, {
        content: CONTENT_SELECTOR, candidates: CANDIDATE_SELECTOR, overlays: OVERLAY_SELECTOR, attribute: SWEEP_ATTRIBUTE,
        initialAttribute: INITIAL_ATTRIBUTE,
        revealers: REVEALER_KINDS, exclusions, typedTypes: Object.keys(TYPED_VALUES), proTags: [...PRO_TAGS], sequence,
      });
      if (next?.skipped) passedOver.push(...next.skipped);
      if (!next || !('id' in next)) break;
      sequence++;

      const description = `${next.kind} ${next.label}`;
      const seen = problems.length;
      const crashesSeen = blazorErrorUiProblems(page).length;
      const interactionStarted = Date.now();
      let drivable = true;
      try {
        await drive(page, page.locator(`[${SWEEP_ATTRIBUTE}="${next.id}"]`), next);
      } catch (error) {
        drivable = false;
        notDriven.push(`${description}: ${firstLine(error)}`);
      }
      // the pointer moves on, as a user's does: a resting pointer would hold what hovering pauses (a toast's timer)
      await page.mouse.move(0, 0);
      await settle(page);
      const stillOpen = await closeOverlays(page);
      if (stillOpen.length > 0) failures.push(`after ${description}: ${stillOpen.join(', ')} did not close`);
      if (drivable) driven.push(`${description} (${Date.now() - interactionStarted} ms)`);

      const newProblems = [...problems.slice(seen), ...blazorErrorUiProblems(page).slice(crashesSeen)];
      if (newProblems.length > 0) failures.push(`after ${description}:\n  ${newProblems.join('\n  ')}`);

      // a crashed page renders nothing more and a page left behind has nothing more to drive
      if (blazorErrorUiProblems(page).length > 0) break;
      const path = new URL(page.url()).pathname;
      if (path !== route) {
        failures.push(`after ${description}: the page navigated to ${path} (list the element in ROUTE_EXCLUSIONS with the reason)`);
        break;
      }
    }

    const elapsed = Date.now() - started;
    test.info().annotations.push({ type: 'interactions', description: `${driven.length} driven, ${notDriven.length} not drivable, ${passedOver.length} passed over, ${elapsed} ms` });
    if (notDriven.length > 0) test.info().annotations.push({ type: 'not drivable', description: notDriven.join(' | ') });
    if (process.env[DETAIL_VARIABLE]) {
      test.info().annotations.push({ type: 'driven', description: driven.join(' | ') });
      test.info().annotations.push({ type: 'passed over', description: passedOver.map(p => `${p.label}: ${p.reason}`).join(' | ') });
    }

    expect(failures, `Interactions on ${route} failed:\n${failures.join('\n')}`).toEqual([]);
  });
}
