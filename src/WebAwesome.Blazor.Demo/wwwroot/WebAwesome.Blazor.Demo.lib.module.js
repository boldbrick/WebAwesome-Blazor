// Blazor JS initializer of the demo app (auto-loaded by the Blazor runtime, in both demo hosts). It registers the
// JavaScript functions that the demo refers to by name (WaJsFunction.Registered) or by global path
// (WaJsFunction.Global) before Blazor starts, so they exist before the first grid needs them.

import { registerFunction } from './_content/WebAwesome.Blazor/webawesome-functions.js';

function registerDemoFunctions() {
  // the JavaScript functions harness (/testing/js-functions): a formatter that shows what ctx holds
  registerFunction('demo.describeCell', (value, row, ctx) =>
    `${ctx.args.prefix}${value} | key ${ctx.rowKey} | column ${ctx.column} | ${ctx.grid === ctx.element ? ctx.grid.localName : 'another element'}`);

  // the same harness: a function on a global path
  globalThis.demoGrid = {
    shout(value) {
      return `${String(value).toUpperCase()}!`;
    },
  };
}

// Blazor Web (blazor.web.js, the server host)
export function beforeWebStart() {
  registerDemoFunctions();
}

// classic hosts (blazor.webassembly.js, the WebAssembly host)
export function beforeStart() {
  registerDemoFunctions();
}
