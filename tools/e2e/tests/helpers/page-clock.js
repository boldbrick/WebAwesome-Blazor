// @ts-check

// The page clock of the specs that fast-forward timers (interaction-sweep.spec.js, the showcase flows): Playwright's
// fake clock, plus a guard for the server-hosted demo.
//
// On Demo.Server the Blazor circuit runs over SignalR, and its client starts a timer of serverTimeoutInMilliseconds
// (30 s) after every message from the server; the timer closes the connection with "Server timeout elapsed without
// receiving a message from the server". The fake clock fakes that timer too, and a fast-forward fires it: the sweep
// jumps the clock 12-24 s after each interaction, so two interactions that make the server send nothing (a button
// without a handler) end the circuit, although the server is alive and pings every 15 s of real time. The guard gives
// that one timer a delay no test reaches, so the fake clock no longer ends the circuit; a real server failure still
// fails the test (the page stops rendering, and the test asserts what it renders). It changes nothing on the
// WebAssembly host, which has no circuit.

// the SignalR client's server timeout callback, `() => this.serverTimeout()`, also in the minified blazor.web.js
const SERVER_TIMEOUT_CALLBACK = 'serverTimeout()';

// the delay the guard gives the server timeout timer: one day of page time
const HELD_TIMEOUT_MS = 24 * 60 * 60 * 1000;

/**
 * Installs the fake page clock with the SignalR server timeout guard. Call it before the page's first navigation.
 *
 * @param {import('@playwright/test').Page} page
 */
async function installPageClock(page) {
  // the guard wraps whatever setTimeout the window holds, also the fake one the clock assigns later (the clock replaces
  // the timers by assignment, which the accessor catches), so the order of the two init scripts does not matter
  await page.addInitScript(({ marker, heldMs }) => {
    const wrap = (/** @type {typeof setTimeout} */ original) => /** @type {typeof setTimeout} */ (/** @type {unknown} */ (
      function (/** @type {any} */ handler, /** @type {number | undefined} */ timeout, /** @type {any[]} */ ...args) {
        const held = typeof handler === 'function' && String(handler).includes(marker);
        return original.call(window, handler, held ? heldMs : timeout, ...args);
      }));
    let current = wrap(window.setTimeout);
    Object.defineProperty(window, 'setTimeout', {
      configurable: true,
      get: () => current,
      set: value => { current = wrap(value); },
    });
  }, { marker: SERVER_TIMEOUT_CALLBACK, heldMs: HELD_TIMEOUT_MS });
  await page.clock.install();
}

module.exports = { installPageClock };
