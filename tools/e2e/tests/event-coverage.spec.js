// @ts-check
const { test, expect } = require('@playwright/test');
const { EVENT_CASES, KNOWN_DEFECT_CASES } = require('./helpers/event-cases');
const { PAYLOAD_CASES } = require('./helpers/payload-cases');
const { EXTERNAL_COVERAGE, readData, readSpec } = require('./helpers/event-coverage');

// Checks the browser suite's event coverage against the wrappers' actual EventCallback parameters
// (review X1). tools\e2e\data\event-callbacks.json is exported from the rendered bindings by the bUnit test
// EventCallbackManifestTests, which fails when the wrappers and the manifest disagree; this spec then requires
// every callback in it to be proven by a browser test (EVENT_CASES, PAYLOAD_CASES, EXTERNAL_COVERAGE) or
// exempted, with a reason, in tools\e2e\data\event-coverage-exemptions.json - and neither list may name a
// callback that no longer exists. A new callback therefore fails the build until it is covered or exempted.
// Runs without a browser page.

/**
 * @typedef {object} Exemption
 * @property {string} key identifier, referenced by KNOWN_DEFECT_CASES
 * @property {string} reason why the callback cannot, or does not yet, have a browser dispatch test
 * @property {string[]} callbacks "Wrapper.Callback" ids
 */

test('every wrapper EventCallback is proven by a browser test or exempted with a reason', () => {
  const manifest = readData('event-callbacks.json');
  /** @type {{ exemptions: Exemption[] }} */
  const { exemptions } = readData('event-coverage-exemptions.json');
  const known = new Set(manifest.callbacks.map((/** @type {{ id: string }} */ c) => c.id));

  /** @type {Map<string, string[]>} callback id -> where it is proven */
  const proofs = new Map();
  const prove = (/** @type {string} */ id, /** @type {string} */ where) => proofs.set(id, [...(proofs.get(id) ?? []), where]);
  for (const c of EVENT_CASES) for (const id of [...c.callbacks, ...(c.proven ?? [])]) prove(id, `event-dispatch.spec.js > dispatch: ${c.name}`);
  for (const c of PAYLOAD_CASES) for (const id of c.callbacks) prove(id, `event-payload.spec.js > payload: ${c.name}`);

  const problems = [];
  for (const e of EXTERNAL_COVERAGE) {
    const source = readSpec(e.spec);
    if (source === null) problems.push(`EXTERNAL_COVERAGE ${e.callback}: spec ${e.spec} does not exist`);
    else if (!source.includes(e.evidence)) problems.push(`EXTERNAL_COVERAGE ${e.callback}: ${e.spec} no longer contains its evidence "${e.evidence}"`);
    else prove(e.callback, e.spec);
  }

  /** @type {Map<string, string>} callback id -> exemption key */
  const exempted = new Map();
  const keys = new Set();
  for (const x of exemptions) {
    if (!x.key || keys.has(x.key)) problems.push(`exemption key '${x.key}' is missing or duplicated`);
    keys.add(x.key);
    if (!x.reason || !x.reason.trim()) problems.push(`exemption '${x.key}' has no reason`);
    for (const id of x.callbacks) {
      if (exempted.has(id)) problems.push(`${id} is exempted twice ('${exempted.get(id)}', '${x.key}')`);
      exempted.set(id, x.key);
    }
  }
  for (const c of KNOWN_DEFECT_CASES) {
    if (!keys.has(c.exemption)) problems.push(`KNOWN_DEFECT_CASES '${c.name}' references a missing exemption '${c.exemption}'`);
  }

  for (const id of known) {
    if (!proofs.has(id) && !exempted.has(id)) problems.push(`${id}: no browser test proves it and it is not exempted`);
    if (proofs.has(id) && exempted.has(id)) problems.push(`${id}: proven by ${proofs.get(id)?.[0]}, so exemption '${exempted.get(id)}' is stale`);
  }
  for (const id of proofs.keys()) {
    if (!known.has(id)) problems.push(`${id}: listed as proven, but no wrapper has this EventCallback (see event-callbacks.json)`);
  }
  for (const [id, key] of exempted) {
    if (!known.has(id)) problems.push(`${id}: exempted in '${key}', but no wrapper has this EventCallback`);
  }

  expect(problems, `event coverage problems:\n${problems.join('\n')}`).toEqual([]);

  test.info().annotations.push({
    type: 'coverage',
    description: `${[...known].filter(id => proofs.has(id)).length} of ${known.size} EventCallbacks proven in the browser, ${exempted.size} exempted`,
  });
});
