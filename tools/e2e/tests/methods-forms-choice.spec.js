// @ts-check
const { test } = require('./helpers/test');
const { runMethodCase, runKnownDefectCase } = require('./helpers/method-harness');
const group = require('./helpers/method-cases/forms-choice');

// The wrapper method suite, group "forms-choice": WaButton, WaCheckbox, WaSwitch, WaRadio, WaRadioGroup, WaRating,
// WaSlider and WaRange on /testing/methods-forms-choice. Each case calls wrapper methods from .NET through the wrapper's
// @ref and asserts their effect; the known defects run as expected failures. The cases are in
// helpers\method-cases\forms-choice.js, where method-coverage.spec.js reads which methods they prove. Every group spec
// has this same shape.

test.describe('methods forms-choice', () => {
  for (const c of group.cases) {
    test(`method: ${c.name}`, async ({ page, context }) => runMethodCase(group, c, page, context));
  }
  for (const c of group.knownDefects ?? []) {
    test(`method known defect: ${c.name}`, async ({ page, context }) => runKnownDefectCase(group, c, page, context));
  }
});
