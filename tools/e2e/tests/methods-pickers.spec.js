// @ts-check
const { test } = require('./helpers/test');
const { runMethodCase, runKnownDefectCase } = require('./helpers/method-harness');
const group = require('./helpers/method-cases/pickers');

// The wrapper method suite, group "pickers": WaColorPicker, WaSelect and WaTimeInput (free), WaDateInput,
// WaDateRangeInput, WaDatePicker and WaDateRangePicker (Pro) on /testing/methods-pickers. Each case calls wrapper
// methods from .NET through the wrapper's @ref and asserts their effect; the known defects run as expected failures.
// The cases are in helpers\method-cases\pickers.js, where method-coverage.spec.js reads which methods they prove. Every
// group spec has this same shape.

test.describe('methods pickers', () => {
  for (const c of group.cases) {
    test(`method: ${c.name}`, async ({ page, context }) => runMethodCase(group, c, page, context));
  }
  for (const c of group.knownDefects ?? []) {
    test(`method known defect: ${c.name}`, async ({ page, context }) => runKnownDefectCase(group, c, page, context));
  }
});
