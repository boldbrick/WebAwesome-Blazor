// @ts-check
const { test, expect } = require('./helpers/test');
const { loadMethodCaseModules, methodGroups, readMethodData } = require('./helpers/method-harness');
const { readSpec } = require('./helpers/event-coverage');

// Checks the wrapper method suite's coverage against the wrappers' actual interop methods (WAB-65), as
// event-coverage.spec.js does for the EventCallbacks. tools\e2e\data\wrapper-methods.json is exported by the bUnit test
// WrapperMethodManifestTests, which fails when the wrappers and the manifest disagree; this spec then requires every
// method in it to be proven by a browser case (the "cases" and "external" proofs of the case modules under
// tests\helpers\method-cases) or exempted, with a reason, in tools\e2e\data\method-coverage-exemptions.json - and no
// list may name a method that no longer exists. A new method therefore fails the build until it is covered or
// exempted. Every wrapper of the manifest must be in exactly one group of tools\e2e\data\method-groups.json, and a
// case module proves only methods of its own group. Known-defect exemptions ("knownDefect": true) need an
// expected-failure case (knownDefects of a module); a "pendingGroup" exemption stands in for a group whose case module
// does not exist yet and is printed as "pending group <name>". Runs without a browser page.

/**
 * @typedef {object} Exemption
 * @property {string} key identifier, referenced by a known-defect case
 * @property {string} reason why the methods have no browser case
 * @property {string[]} methods "Wrapper.Method" ids
 * @property {boolean} [knownDefect] the exemption records a known defect (its reason starts with KNOWN DEFECT)
 * @property {string} [defect] the ticket and finding of a known defect, for example "WAB-64 F1"
 * @property {string} [pendingGroup] a group whose cases are not written yet: exempts its unproven methods
 */

const KNOWN_DEFECT_MARKER = 'KNOWN DEFECT';
const PENDING_MARKER = 'PENDING';

// a ticket key and a finding, for example "WAB-64 F8"
const DEFECT_PATTERN = /^[A-Z][A-Z0-9]*-\d+ \S+$/;

test('every wrapper method is proven by a browser case or exempted with a reason', () => {
  const manifest = readMethodData('wrapper-methods.json');
  /** @type {{ exemptions: Exemption[] }} */
  const { exemptions } = readMethodData('method-coverage-exemptions.json');
  const groups = methodGroups();
  const modules = loadMethodCaseModules();
  const problems = [];

  /** @type {Map<string, { component: string }>} */
  const methods = new Map(manifest.methods.map((/** @type {{ id: string, component: string }} */ m) => [m.id, m]));
  const components = new Set([...methods.values()].map(m => m.component));

  // every wrapper with methods in exactly one group, and no group naming a wrapper without methods
  /** @type {Map<string, string>} component -> group */
  const groupOf = new Map();
  for (const g of groups) {
    for (const component of g.components) {
      if (groupOf.has(component)) problems.push(`${component} is in two groups ('${groupOf.get(component)}', '${g.name}')`);
      groupOf.set(component, g.name);
      if (!components.has(component)) problems.push(`group '${g.name}' lists ${component}, which has no method in wrapper-methods.json`);
    }
  }
  for (const component of components) {
    if (!groupOf.has(component)) problems.push(`${component} has methods but is in no group of method-groups.json`);
  }
  const groupNames = new Set(groups.map(g => g.name));
  const groupOfMethod = (/** @type {string} */ id) => groupOf.get(methods.get(id)?.component ?? '');

  /** @type {Map<string, string[]>} method id -> where it is proven */
  const proofs = new Map();
  const prove = (/** @type {string} */ id, /** @type {string} */ where) => proofs.set(id, [...(proofs.get(id) ?? []), where]);
  /** @type {{ name: string, defect: string, methods: string[], exemption?: string }[]} */
  const defectCases = [];

  for (const [file, module] of modules) {
    if (!groupNames.has(file)) { problems.push(`case module ${file}.js names no group of method-groups.json`); continue; }
    if (module.group !== file) problems.push(`case module ${file}.js declares the group '${module.group}'`);
    const owns = (/** @type {string} */ id, /** @type {string} */ where) => {
      if (!methods.has(id)) { problems.push(`${where}: ${id} is not a method of wrapper-methods.json`); return false; }
      if (groupOfMethod(id) !== file) { problems.push(`${where}: ${id} belongs to the group '${groupOfMethod(id)}', not '${file}'`); return false; }
      return true;
    };

    for (const c of module.cases ?? []) {
      if (!c.methods?.length) problems.push(`${file}.js case '${c.name}' proves no method`);
      for (const id of c.methods ?? []) if (owns(id, `${file}.js case '${c.name}'`)) prove(id, `methods-${file}.spec.js > method: ${c.name}`);
    }
    for (const e of module.external ?? []) {
      if (!owns(e.method, `${file}.js external proof`)) continue;
      const source = readSpec(e.spec);
      if (source === null) problems.push(`${file}.js external proof of ${e.method}: spec ${e.spec} does not exist`);
      else if (!source.includes(e.evidence)) problems.push(`${file}.js external proof of ${e.method}: ${e.spec} no longer contains its evidence "${e.evidence}"`);
      else prove(e.method, e.spec);
    }
    for (const c of module.knownDefects ?? []) {
      if (!c.defect || !DEFECT_PATTERN.test(c.defect)) problems.push(`${file}.js known defect '${c.name}' must name its defect as "<ticket> <finding>", e.g. "WAB-64 F8"`);
      if (!c.methods?.length) problems.push(`${file}.js known defect '${c.name}' names no method`);
      for (const id of c.methods ?? []) owns(id, `${file}.js known defect '${c.name}'`);
      defectCases.push(c);
    }
  }

  // the exemptions
  /** @type {Map<string, string>} method id -> exemption key */
  const exempted = new Map();
  const keys = new Set();
  /** @type {Map<string, string[]>} pending group -> the methods it exempts */
  const pending = new Map();
  for (const x of exemptions) {
    if (!x.key || keys.has(x.key)) problems.push(`exemption key '${x.key}' is missing or duplicated`);
    keys.add(x.key);
    if (!x.reason || !x.reason.trim()) problems.push(`exemption '${x.key}' has no reason`);
    const marked = typeof x.reason === 'string' && x.reason.trimStart().startsWith(KNOWN_DEFECT_MARKER);
    if (marked !== (x.knownDefect === true)) problems.push(`exemption '${x.key}': "knownDefect": true and a reason starting with ${KNOWN_DEFECT_MARKER} must go together`);
    if (x.knownDefect === true && (!x.defect || !DEFECT_PATTERN.test(x.defect))) problems.push(`exemption '${x.key}': a known defect names its defect as "<ticket> <finding>", e.g. "WAB-64 F1"`);
    if (x.knownDefect === true && !defectCases.some(c => c.exemption === x.key)) problems.push(`known defect '${x.key}' has no expected-failure case (knownDefects of a case module)`);

    if (x.pendingGroup !== undefined) {
      if (!groupNames.has(x.pendingGroup)) problems.push(`exemption '${x.key}': pendingGroup '${x.pendingGroup}' is not a group of method-groups.json`);
      if (modules.has(x.pendingGroup)) problems.push(`exemption '${x.key}': the group '${x.pendingGroup}' has its case module now - remove the pending exemption`);
      if (!x.reason?.startsWith(PENDING_MARKER)) problems.push(`exemption '${x.key}': a pending group's reason starts with ${PENDING_MARKER}`);
      if (x.methods?.length) problems.push(`exemption '${x.key}': a pending group lists no methods (it stands for the whole group)`);
      if (pending.has(x.pendingGroup)) problems.push(`the group '${x.pendingGroup}' has two pending exemptions`);
      pending.set(x.pendingGroup, []);
      continue;
    }

    for (const id of x.methods ?? []) {
      if (!methods.has(id)) problems.push(`${id}: exempted in '${x.key}', but no wrapper has this method (see wrapper-methods.json)`);
      if (exempted.has(id)) problems.push(`${id} is exempted twice ('${exempted.get(id)}', '${x.key}')`);
      exempted.set(id, x.key);
    }
  }
  for (const c of defectCases) {
    if (c.exemption === undefined) continue;
    const exemption = exemptions.find(x => x.key === c.exemption);
    if (!exemption) problems.push(`known defect '${c.name}' references a missing exemption '${c.exemption}'`);
    else if (exemption.knownDefect !== true) problems.push(`known defect '${c.name}' references '${c.exemption}', which is not flagged "knownDefect": true`);
  }

  for (const id of methods.keys()) {
    const group = groupOfMethod(id) ?? '';
    if (proofs.has(id) && exempted.has(id)) problems.push(`${id}: proven by ${proofs.get(id)?.[0]}, so exemption '${exempted.get(id)}' is stale`);
    if (proofs.has(id) || exempted.has(id)) continue;
    if (pending.has(group)) pending.get(group)?.push(id);
    else problems.push(`${id}: no browser case proves it and it is not exempted`);
  }

  expect(problems, `method coverage problems:\n${problems.join('\n')}`).toEqual([]);

  const knownDefects = exemptions.filter(x => x.knownDefect === true);
  const pendingCount = [...pending.values()].reduce((n, ids) => n + ids.length, 0);
  const lines = [
    `${proofs.size} of ${methods.size} wrapper methods proven in the browser, ${exempted.size} exempted (of which ` +
      `${knownDefects.reduce((n, x) => n + x.methods.length, 0)} by ${knownDefects.length} known defects), ${pendingCount} pending`,
    `${defectCases.length} known-defect cases: ${defectCases.map(c => `${c.defect} (${c.methods.join(', ')})`).join('; ') || 'none'}`,
    ...[...pending].map(([group, ids]) => `pending group ${group}: ${ids.length} methods`),
  ];
  for (const line of lines) test.info().annotations.push({ type: 'coverage', description: line });
  console.log(`method coverage:\n  ${lines.join('\n  ')}`);
});
