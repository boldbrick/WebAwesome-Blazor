// @ts-check
const { test } = require('./helpers/test');
const { runMethodCase, runKnownDefectCase } = require('./helpers/method-harness');
const group = require('./helpers/method-cases/navigation');

// The wrapper method suite, group "navigation": WaAccordion, WaAccordionItem, WaDetails, WaTab, WaTabGroup, WaStepper,
// WaCarousel, WaTreeItem and WaSplitPanel on /testing/methods-navigation. Each case calls wrapper methods from .NET
// through the wrapper's @ref and asserts their effect; the known defects run as expected failures. The cases are in
// helpers\method-cases\navigation.js, where method-coverage.spec.js reads which methods they prove. Every group spec has
// this same shape.

test.describe('methods navigation', () => {
  for (const c of group.cases) {
    test(`method: ${c.name}`, async ({ page, context }) => runMethodCase(group, c, page, context));
  }
  for (const c of group.knownDefects ?? []) {
    test(`method known defect: ${c.name}`, async ({ page, context }) => runKnownDefectCase(group, c, page, context));
  }
});
