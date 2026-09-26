// @ts-check
const fs = require('fs');
const path = require('path');

// Coverage bookkeeping for event-coverage.spec.js: which EventCallbacks the browser suite proves, and where.
// EVENT_CASES (event-dispatch.spec.js) and PAYLOAD_CASES (event-payload.spec.js) prove their callbacks through
// the harness event log; EXTERNAL_COVERAGE lists callbacks proven by other specs, each with a literal piece of
// the spec's source that names the proof, so a renamed or deleted proof fails the coverage check.

const DATA_DIR = path.resolve(__dirname, '..', '..', 'data');
const TESTS_DIR = path.resolve(__dirname, '..');

/**
 * @typedef {object} ExternalCoverage
 * @property {string} callback "Wrapper.Callback"
 * @property {string} spec spec file under tools\e2e\tests
 * @property {string} evidence literal text of that spec that drives and asserts the callback
 */

/** @param {string} wrapper */
const valueSync = wrapper => ({ callback: `${wrapper}.ValueChanged`, spec: 'value-sync-binding.spec.js', evidence: `wrapper: '${wrapper}'` });

/** @type {ExternalCoverage[]} */
const EXTERNAL_COVERAGE = [
  { callback: 'WaTabGroup.OnTabChange', spec: 'custom-event-payload.spec.js', evidence: "toHaveText('Last active panel: custom')" },
  { callback: 'WaDetails.OnToggle', spec: 'custom-event-payload.spec.js', evidence: "toHaveText('OnToggle calls: 2 (last: closed)')" },
  { callback: 'WaDetails.OnAfterShow', spec: 'custom-event-payload.spec.js', evidence: "toHaveText('OnAfterShow calls: 1')" },
  { callback: 'WaDetails.OnAfterHide', spec: 'custom-event-payload.spec.js', evidence: "toHaveText('OnAfterHide calls: 1')" },
  // @bind-Value user edits reaching the model (step (a) of each value-sync row)
  ...['WaInput', 'WaTextArea', 'WaCheckbox', 'WaNumberInput', 'WaColorPicker', 'WaDateInput', 'WaKnownDate', 'WaOtpInput',
    'WaRadioGroup', 'WaSlider', 'WaTimeInput', 'WaSwitch', 'WaSelect'].map(valueSync),
  // the range wrappers' value binding: user edits in value-sync-binding.spec.js, typed picks in date-typing.spec.js
  ...['WaDateRangeInput', 'WaDateRangePicker'].map(valueSync),
  { callback: 'WaRange.ValueChanged', spec: 'number-value-binding.spec.js', evidence: 'WaRange user edit (ArrowRight) reaches the bound model' },
  { callback: 'WaRating.ValueChanged', spec: 'number-value-binding.spec.js', evidence: 'WaRating user edit (star click) reaches the bound model' },
  { callback: 'WaSlider.MinValueChanged', spec: 'number-value-binding.spec.js', evidence: 'user edit (min thumb) reaches the bound MinValue' },
  { callback: 'WaSlider.MaxValueChanged', spec: 'number-value-binding.spec.js', evidence: 'user edit (max thumb) reaches the bound MaxValue' },
];

/**
 * Reads a JSON file of tools\e2e\data.
 *
 * @param {string} name
 * @returns {any}
 */
function readData(name) {
  return JSON.parse(fs.readFileSync(path.join(DATA_DIR, name), 'utf8').replace(/^﻿/, ''));
}

/**
 * Reads the source of a spec file of tools\e2e\tests, or null when it does not exist.
 *
 * @param {string} name
 */
function readSpec(name) {
  const file = path.join(TESTS_DIR, name);
  return fs.existsSync(file) ? fs.readFileSync(file, 'utf8') : null;
}

module.exports = { EXTERNAL_COVERAGE, readData, readSpec };
