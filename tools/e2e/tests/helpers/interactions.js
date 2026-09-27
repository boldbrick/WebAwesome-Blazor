// @ts-check

// The generic interactions of the interaction sweep (interaction-sweep.spec.js): which elements of a demo page are
// interacted with, in which order, how each kind is driven, and how the page is settled and tidied afterwards.
//
// Elements are chosen in the page itself, one at a time (nextCandidate), inside the page's content area only (not
// the demo shell's sidebar). Each chosen element is marked with SWEEP_ATTRIBUTE, so it is driven once; an element
// Blazor renders later (a result panel, a new list item) is found on a later round. Elements that reveal others
// (tabs, disclosures) come after every other visible element, so the content of each tab panel and each disclosure
// gets its turn when it shows.

// the content area of every demo page (MainLayout.razor); the sidebar's theme and dark-mode controls are
// theme-and-dark-mode.spec.js's
const CONTENT_SELECTOR = 'main.demo-content';

// the attribute that marks an element the sweep has chosen (its sequence number) or passed over (a reason)
const SWEEP_ATTRIBUTE = 'data-e2e-sweep';

// the attribute that marks the elements the page had when the sweep started
const INITIAL_ATTRIBUTE = 'data-e2e-initial';

// the elements the sweep considers; nextCandidate classifies each into a kind or passes it over
const CANDIDATE_SELECTOR = [
  'wa-button', 'button', 'wa-copy-button',
  'wa-switch', 'wa-checkbox', 'input[type="checkbox"]',
  'wa-radio', 'input[type="radio"]',
  'wa-tab',
  'wa-details', 'details', 'wa-accordion-item',
  'wa-select', 'wa-combobox',
  'wa-input', 'wa-textarea', 'wa-number-input', 'input', 'textarea', 'wa-tag-input',
  'wa-otp-input', 'wa-known-date', 'wa-date-input', 'wa-time-input', 'wa-date-picker',
  'wa-slider', 'wa-rating', 'wa-color-picker',
  'wa-dropdown',
  'wa-tree-item',
  'wa-tag[with-remove]',
  'wa-pagination', 'wa-carousel', 'wa-split-panel', 'wa-comparison',
].join(', ');

// containers whose content only shows while they are open; the sweep opens and closes the container (through its
// trigger) but does not drive its content, which the showcase flows and the harness specs cover
const OVERLAY_SELECTOR = 'wa-dialog, wa-drawer, wa-popover, wa-dropdown, wa-toast, wa-popup, wa-tooltip';

// the open overlays the sweep closes after every interaction
const OPEN_OVERLAY_SELECTOR = [
  'wa-dialog[open]', 'wa-drawer[open]', 'wa-popover[open]', 'wa-dropdown[open]',
  'wa-select[open]', 'wa-combobox[open]', 'wa-color-picker[open]', 'wa-date-input[open]', 'wa-time-input[open]',
].join(', ');

// the kinds that reveal other elements when driven, in the order they get their turn after every other visible element
const REVEALER_KINDS = ['disclosure', 'tab'];

// wa-input types a value is typed into, with the value (other types, e.g. date or color, are passed over)
const TYPED_VALUES = {
  '': 'e2e sweep', text: 'e2e sweep', search: 'e2e sweep', password: 'e2e sweep',
  email: 'e2e@example.com', url: 'https://example.com/', tel: '5550100', number: '7',
};

// the tag entered into a tag input (a single word, so no delimiter splits it)
const TYPED_TAG = 'e2e';

// the digits typed into a one-time-code or known-date field
const TYPED_DIGITS = '123456';

// the longest a single step of an interaction (a click, a keypress, an option to show) may wait for its element
const ACTION_TIMEOUT_MS = 3_000;

// how far the page clock jumps after each interaction: past every timer the demo pages start (a toast's 5 s
// duration, the dashboard's 1.2 s refresh, autoplay intervals), so they fire without real waiting
const TIMER_JUMP_MS = 6_000;

// the longest the sweep waits for the finite animations an interaction started (an overlay's show or hide)
const SETTLE_TIMEOUT_MS = 3_000;

// how many times settle jumps the clock and waits for the animations
const SETTLE_ROUNDS = 2;

// how often the page is polled for running animations while settling
const SETTLE_POLL_MS = 50;

// how often the sweep presses Escape to close the overlays an interaction opened
const ESCAPE_ATTEMPTS = 3;

/**
 * Chooses the next element to drive on the page (runs in the page). Marks passed-over elements with the reason and
 * the chosen one with its sequence number.
 *
 * @param {{ content: string, candidates: string, overlays: string, attribute: string, initialAttribute: string, revealers: string[],
 *   exclusions: { selector: string, reason: string }[], typedTypes: string[], proTags: string[], sequence: number }} options
 * @returns {{ id: string, kind: string, tag: string, label: string, type: string } | { skipped: { label: string, reason: string }[] } | null}
 */
function nextCandidate(options) {
  const { content, candidates, overlays, attribute, initialAttribute, revealers, exclusions, typedTypes, proTags, sequence } = options;
  const root = document.querySelector(content);
  if (!root) return null;

  // the elements of the page as it loaded; one rendered later with the kind and label of an element already driven
  // (the same list rendered again after a reset) is passed over, so a page cannot cycle through its budget
  if (sequence === 0) root.querySelectorAll(candidates).forEach(el => el.setAttribute(initialAttribute, ''));
  const win = /** @type {any} */ (window);
  win.__e2eSweepDriven ??= new Set();
  const driven = /** @type {Set<string>} */ (win.__e2eSweepDriven);

  const skipped = [];
  const labelOf = (/** @type {Element} */ el) => {
    const text = (el.getAttribute('label') || el.getAttribute('summary') || el.getAttribute('aria-label') || el.textContent || '')
      .replace(/\s+/g, ' ').trim().slice(0, 60);
    return `<${el.localName}>${text ? ` "${text}"` : ''}`;
  };
  const passOver = (/** @type {Element} */ el, /** @type {string} */ reason) => {
    el.setAttribute(attribute, reason);
    skipped.push({ label: labelOf(el), reason });
  };
  const isDisabled = (/** @type {any} */ el) => el.disabled === true || el.hasAttribute('disabled')
    || el.getAttribute('aria-disabled') === 'true' || el.hasAttribute('loading') || !!el.parentElement?.closest('[disabled]');
  const isVisible = (/** @type {Element} */ el) => {
    // an element without a box of its own (a dropdown is display: contents) shows through its children
    if (getComputedStyle(el).display === 'contents') return [...el.children].some(isVisible);
    if (!el.checkVisibility({ checkOpacity: true, checkVisibilityCSS: true })) return false;
    const rect = el.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
  };

  /** @returns {string | null} the kind of the element, or null to pass it over for good (with a reason set) */
  const classify = (/** @type {any} */ el) => {
    const tag = el.localName;
    if (tag.includes('-') && !customElements.get(tag)) return passOver(el, 'not upgraded (a Pro component on the free CDN)'), null;
    if (el.parentElement?.closest(overlays)) return passOver(el, 'overlay content'), null;
    const excluded = exclusions.find(e => el.matches(e.selector));
    if (excluded) return passOver(el, `excluded: ${excluded.reason}`), null;
    if (el.closest('a[href]') || (el.hasAttribute('href') && tag !== 'wa-copy-button')) return passOver(el, 'link'), null;

    switch (tag) {
      case 'wa-button':
      case 'button':
        // a dropdown's trigger is driven through its dropdown
        if (el.closest('[slot="trigger"]') && el.closest('wa-dropdown')) return passOver(el, 'dropdown trigger'), null;
        // the example's button calls a method of its Pro component, which does not exist while the component has
        // not upgraded (the free CDN); the Pro pass drives it
        if (proTags.some(pro => el.closest('.example-live')?.querySelector(`${pro}:not(:defined)`))) {
          return passOver(el, 'next to a Pro component that did not upgrade'), null;
        }
        return 'button';
      case 'wa-copy-button': return 'button';
      case 'wa-switch':
      case 'wa-checkbox': return 'toggle';
      case 'wa-radio': return 'radio';
      case 'wa-tab': return 'tab';
      case 'wa-details':
      case 'wa-accordion-item': return 'disclosure';
      case 'details': return 'native-disclosure';
      case 'wa-select': return 'select';
      case 'wa-combobox': return 'combobox';
      case 'wa-input':
        if (!typedTypes.includes(el.getAttribute('type') ?? '')) return passOver(el, `input type ${el.getAttribute('type')}`), null;
        return el.hasAttribute('readonly') ? (passOver(el, 'readonly'), null) : 'text';
      case 'wa-textarea':
      case 'wa-number-input':
      case 'textarea': return el.hasAttribute('readonly') ? (passOver(el, 'readonly'), null) : 'text';
      case 'wa-tag-input': return el.hasAttribute('readonly') ? (passOver(el, 'readonly'), null) : 'tags';
      case 'input': {
        const type = el.getAttribute('type') ?? '';
        if (type === 'checkbox') return 'toggle';
        if (type === 'radio') return 'radio';
        if (!typedTypes.includes(type)) return passOver(el, `input type ${type}`), null;
        return el.hasAttribute('readonly') ? (passOver(el, 'readonly'), null) : 'text';
      }
      case 'wa-otp-input':
      case 'wa-known-date': return 'digits';
      case 'wa-date-input':
      case 'wa-time-input': return 'picker';
      case 'wa-date-picker': return 'date-picker';
      case 'wa-slider': return 'slider';
      case 'wa-rating': return el.hasAttribute('readonly') ? (passOver(el, 'readonly'), null) : 'rating';
      case 'wa-color-picker': return 'color-picker';
      case 'wa-dropdown': return 'dropdown';
      case 'wa-tree-item': return 'tree-item';
      case 'wa-tag': return 'tag-remove';
      case 'wa-pagination': return 'pagination';
      case 'wa-carousel': return 'carousel';
      case 'wa-split-panel': return 'split-panel';
      case 'wa-comparison': return 'comparison';
      default: return passOver(el, 'not interactive'), null;
    }
  };

  // the first visible revealer of each kind, in the order of revealers (a disclosure inside the shown tab panel
  // before the next tab)
  const firstRevealers = new Map();
  for (const el of root.querySelectorAll(candidates)) {
    if (el.hasAttribute(attribute)) continue;
    const kind = classify(el);
    if (!kind) continue;
    // a checked radio, an active tab and a disabled element may change later; they stay candidates
    if ((kind === 'radio' && /** @type {any} */ (el).checked) || (kind === 'tab' && el.hasAttribute('active'))) continue;
    if (isDisabled(el) || !isVisible(el)) continue;
    if (!el.hasAttribute(initialAttribute) && driven.has(`${kind} ${labelOf(el)}`)) {
      passOver(el, 'rendered again after an interaction, like one already driven');
      continue;
    }

    if (!revealers.includes(kind)) return choose(el, kind);
    if (!firstRevealers.has(kind)) firstRevealers.set(kind, el);
  }
  const revealerKind = revealers.find(kind => firstRevealers.has(kind));
  if (revealerKind) return choose(firstRevealers.get(revealerKind), revealerKind);
  return skipped.length > 0 ? { skipped } : null;

  function choose(/** @type {Element} */ el, /** @type {string} */ kind) {
    el.setAttribute(attribute, String(sequence));
    driven.add(`${kind} ${labelOf(el)}`);
    return { id: String(sequence), kind, tag: el.localName, label: labelOf(el), type: el.getAttribute('type') ?? '', skipped };
  }
}

/**
 * Drives one element the way a user would, by its kind.
 *
 * @param {import('@playwright/test').Page} page
 * @param {import('@playwright/test').Locator} el
 * @param {{ kind: string, type: string }} candidate
 */
async function drive(page, el, candidate) {
  const timeout = ACTION_TIMEOUT_MS;
  switch (candidate.kind) {
    case 'button':
    case 'toggle':
    case 'radio':
    case 'tab':
    case 'rating':
      await el.click({ timeout });
      break;
    case 'disclosure':
      await clickPart(el, '[part~="header"], [part~="button"]');
      break;
    case 'native-disclosure':
      await el.locator(':scope > summary').click({ timeout });
      break;
    case 'select':
    case 'combobox':
      await el.locator(candidate.kind === 'select' ? '[part~="combobox"]' : '[part~="combobox-input"]').click({ timeout });
      await pickOption(el);
      break;
    case 'text': {
      const value = TYPED_VALUES[candidate.type] ?? TYPED_VALUES[''];
      const typed = candidate.tag === 'wa-number-input' ? '7' : value;
      await el.focus({ timeout });
      await page.keyboard.press('ControlOrMeta+A');
      await page.keyboard.type(typed);
      await page.keyboard.press('Tab');
      break;
    }
    case 'tags':
      // the text box inside the shadow root; Enter turns the typed text into a tag
      await el.locator('[part~="input"]').focus({ timeout });
      await page.keyboard.type(TYPED_TAG);
      await page.keyboard.press('Enter');
      await page.keyboard.press('Tab');
      break;
    case 'digits':
      await el.focus({ timeout });
      await page.keyboard.type(TYPED_DIGITS);
      await page.keyboard.press('Tab');
      break;
    case 'picker':
      await el.locator('[part~="expand-button"]').click({ timeout });
      break;
    case 'date-picker':
      await el.locator('[part~="day"]:not([part~="day-disabled"]):not([part~="day-outside"])').nth(9).click({ timeout });
      break;
    case 'slider':
      await el.locator('[part~="thumb"], [part~="thumb-min"]').first().focus({ timeout });
      await page.keyboard.press('ArrowRight');
      break;
    case 'color-picker':
      await el.locator('[part~="trigger"]').click({ timeout });
      if (await el.locator('[part~="swatch"]').count() > 0) await el.locator('[part~="swatch"]').first().click({ timeout });
      break;
    case 'dropdown':
      // an item that is a link would leave the page; a menu of links only is opened and closed
      await el.locator('[slot="trigger"]').first().click({ timeout });
      if (await el.locator('wa-dropdown-item:not([disabled]):not([href])').count() > 0) {
        await el.locator('wa-dropdown-item:not([disabled]):not([href])').first().click({ timeout });
      }
      break;
    case 'tree-item':
      // select the item by its label, then toggle a parent item by its expand button
      await clickPart(el, '[part~="label"]');
      if (await el.evaluate(item => item.querySelector(':scope > wa-tree-item') !== null)) await clickPart(el, '[part~="expand-button"]');
      break;
    case 'tag-remove':
      await el.locator('[part~="remove-button"]').click({ timeout });
      break;
    case 'pagination':
      await el.locator('[part~="next-button"]').click({ timeout });
      break;
    case 'carousel': {
      // the next button where the carousel has navigation, else the keyboard on its scroll container
      const next = el.locator('[part~="navigation-button-next"]');
      if (await next.count() > 0 && await next.isVisible() && await next.isEnabled()) {
        await next.click({ timeout });
      } else {
        await el.locator('[part~="scroll-container"]').focus({ timeout });
        await page.keyboard.press('ArrowRight');
      }
      break;
    }
    case 'split-panel':
      await el.locator('[part~="divider"]').focus({ timeout });
      await page.keyboard.press('ArrowLeft');
      break;
    case 'comparison':
      await el.locator('[part~="handle"]').focus({ timeout });
      await page.keyboard.press('ArrowRight');
      break;
    default:
      throw new Error(`no driver for kind ${candidate.kind}`);
  }
}

/**
 * Clicks a part of the element's own shadow root (not one of a nested element of the same kind, as a nested tree
 * item or details).
 *
 * @param {import('@playwright/test').Locator} el
 * @param {string} selector
 */
async function clickPart(el, selector) {
  const part = (await el.evaluateHandle((host, s) => host.shadowRoot?.querySelector(s) ?? null, selector)).asElement();
  if (!part) throw new Error(`<${await el.evaluate(host => host.localName)}> has no ${selector}`);
  await part.click({ timeout: ACTION_TIMEOUT_MS });
}

/**
 * Picks the first option of an open select or combobox that is not selected yet.
 *
 * @param {import('@playwright/test').Locator} el
 */
async function pickOption(el) {
  const option = el.locator('wa-option:not([disabled]):not([selected])').first();
  if (await option.count() === 0) return;
  await option.click({ timeout: ACTION_TIMEOUT_MS });
}

/**
 * Lets the page finish what an interaction started, in SETTLE_ROUNDS rounds: each jumps the page clock past the
 * pending timers, waits (up to SETTLE_TIMEOUT_MS) until no finite animation runs, and gives Blazor one task to render.
 * The second round fires the timers that start only once an animation has ended (a toast item counts its duration
 * down after its show animation, then hides with another).
 *
 * @param {import('@playwright/test').Page} page
 */
async function settle(page) {
  for (let round = 0; round < SETTLE_ROUNDS; round++) {
    await page.clock.fastForward(TIMER_JUMP_MS);
    // an animation that runs for good or for longer than the settle timeout (a spinner, a 1000-iteration pulse) is
    // decoration, not the end of what the interaction started
    await page.waitForFunction(limit => document.getAnimations().every(a => {
      if (a.playState !== 'running') return true;
      const remaining = Number(a.effect?.getComputedTiming().endTime) - Number(a.currentTime ?? 0);
      return !Number.isFinite(remaining) || remaining > limit;
    }), SETTLE_TIMEOUT_MS, { timeout: SETTLE_TIMEOUT_MS, polling: SETTLE_POLL_MS }).catch(() => undefined);
    await page.evaluate(() => new Promise(resolve => setTimeout(resolve, 0)));
  }
}

/**
 * Closes the overlays an interaction left open: Escape first (as a user would), then the element's own hide().
 *
 * @param {import('@playwright/test').Page} page
 * @returns {Promise<string[]>} the overlays that were still open afterwards
 */
async function closeOverlays(page) {
  const openOverlays = () => page.evaluate(selector => [...document.querySelectorAll(selector)].map(el => el.localName), OPEN_OVERLAY_SELECTOR);
  for (let attempt = 0; attempt < ESCAPE_ATTEMPTS && (await openOverlays()).length > 0; attempt++) {
    await page.keyboard.press('Escape');
    await settle(page);
  }
  if ((await openOverlays()).length === 0) return [];

  await page.evaluate(selector => document.querySelectorAll(selector).forEach(el => /** @type {any} */ (el).hide?.()), OPEN_OVERLAY_SELECTOR);
  await settle(page);
  return openOverlays();
}

module.exports = {
  CONTENT_SELECTOR, SWEEP_ATTRIBUTE, INITIAL_ATTRIBUTE, CANDIDATE_SELECTOR, OVERLAY_SELECTOR, REVEALER_KINDS, TYPED_VALUES,
  nextCandidate, drive, settle, closeOverlays,
};
