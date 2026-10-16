// Blazor JS initializer for WebAwesome.Blazor.Extensions (auto-loaded by the Blazor runtime from
// _content/WebAwesome.Blazor.Extensions/WebAwesome.Blazor.Extensions.lib.module.js).
//
// The Extensions package holds features that add behavior upstream Web Awesome does not have; the main package
// WebAwesome.Blazor stays pure bindings. This initializer connects the package to the bindings through their public
// JavaScript only:
// - it registers the data grid preset functions (data-grid-presets.js) in the shared function registry of
//   WebAwesome.Blazor (webawesome-functions.js), under the names that the C# factories WaDataGridFormatters and
//   WaDataGridComparators refer to (WaJsFunction.Registered), before Blazor starts, so they exist before the first grid;
// - it registers the ctx helpers of data grid functions (data-grid-context.js: ctx.emit, ctx.invoke, ctx.h, ctx.preset,
//   ctx.format, ctx.cache) as a context decorator, in the same step;
// - it registers the cell action event (`wablazor-cell-action`) with Blazor.registerCustomEventType, so that
//   `@onwablazor-cell-action` on a WaDataGrid (declared by EventHandlers, passed to the element through the
//   wrapper's additional attributes) reaches .NET as WaDataGridCellActionEventArgs.

import { registerFunction, registerContextDecorator } from '../WebAwesome.Blazor/webawesome-functions.js';
import { presets, cellActionEventName, cellActionArgs } from './data-grid-presets.js';
import { decorateContext } from './data-grid-context.js';

let presetsRegistered = false;

function registerPresets() {
  if (presetsRegistered) return;
  presetsRegistered = true;

  for (const [name, preset] of Object.entries(presets)) {
    registerFunction(name, preset);
  }

  registerContextDecorator(decorateContext);
}

let eventTypesRegistered = false;

function registerEventTypes(blazor) {
  if (eventTypesRegistered || !blazor || typeof blazor.registerCustomEventType !== 'function') return;
  eventTypesRegistered = true;

  blazor.registerCustomEventType(cellActionEventName, {
    createEventArgs: cellActionArgs,
  });
}

// Blazor Web (blazor.web.js, .NET 8+)
export function beforeWebStart() {
  registerPresets();
}

export function afterWebStarted(blazor) {
  registerPresets();
  registerEventTypes(blazor);
}

// classic hosts (blazor.webassembly.js / blazor.server.js)
export function beforeStart() {
  registerPresets();
}

export function afterStarted(blazor) {
  registerPresets();
  registerEventTypes(blazor);
}
