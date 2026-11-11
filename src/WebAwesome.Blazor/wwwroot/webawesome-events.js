/**
 * Web Awesome Blazor custom event types
 *
 * Blazor delivers a custom element event to .NET only when the event name is registered with
 * Blazor.registerCustomEventType, and the registration's createEventArgs builds the JSON payload that .NET reads into
 * the typed event args (case-insensitive, extra properties ignored). This module is the public module of those
 * registrations. The JS initializer of WebAwesome.Blazor registers its wa-* events through it, and a package that
 * wraps custom elements of its own (a sister wrapper set) imports it from its own JS initializer
 * (wwwroot/<PackageId>.lib.module.js):
 *
 *   import { registerEventTypes, detailArgs } from '../WebAwesome.Blazor/webawesome-events.js';
 *
 *   export function afterWebStarted(blazor) { register(blazor); }   // Blazor Web (blazor.web.js)
 *   export function afterStarted(blazor) { register(blazor); }      // blazor.webassembly.js, blazor.server.js
 *
 *   function register(blazor) {
 *     registerEventTypes(blazor, {
 *       'xyz-remove': null,                                     // the sanitized event detail
 *       'xyz-select': event => ({ key: event.detail.key }),     // an own payload
 *       'xyz-press': { browserEventName: 'click', createEventArgs: detailArgs },
 *     });
 *   }
 *
 * Event names are global for the page: Blazor rejects a second registration of a name, so a package registers only its
 * own prefix. The names registered through this module are kept on globalThis[Symbol.for('WebAwesome.Blazor.eventTypes')]
 * (a Set of names), so every copy of the module (another base path, another version) sees the same registrations.
 */

const registryKey = Symbol.for('WebAwesome.Blazor.eventTypes');

const logPrefix = 'WebAwesome.Blazor:';

// the depth to which detailArgs copies an event detail
const detailDepth = 3;

// the names registered through any copy of this module
function registeredNames() {
  let names = globalThis[registryKey];
  if (!(names instanceof Set)) {
    names = new Set();
    globalThis[registryKey] = names;
  }
  return names;
}

/**
 * Recursively copies the JSON-safe values of a value, for example an event detail: strings, numbers, booleans, arrays
 * and plain objects. It drops DOM nodes, windows, functions and everything deeper than depth, because an event detail
 * can hold live elements, which cannot be marshalled to .NET.
 * @param {*} value - The value to copy
 * @param {number} depth - How many levels of arrays and objects to copy
 * @returns {*} The copy; null for null or undefined, undefined for a value that is dropped
 */
export function sanitize(value, depth) {
  if (value === null || value === undefined) return null;
  const type = typeof value;
  if (type === 'string' || type === 'number' || type === 'boolean') return value;
  if (type === 'function') return undefined;
  if (typeof Node !== 'undefined' && value instanceof Node) return undefined;
  if (typeof Window !== 'undefined' && value instanceof Window) return undefined;
  if (depth <= 0) return undefined;
  if (Array.isArray(value)) {
    return value.map(item => sanitize(item, depth - 1)).filter(item => item !== undefined);
  }
  if (type === 'object') {
    const result = {};
    for (const key of Object.keys(value)) {
      const sanitized = sanitize(value[key], depth - 1);
      if (sanitized !== undefined) result[key] = sanitized;
    }
    return result;
  }
  return undefined;
}

/**
 * The default payload of an event: its sanitized detail (three levels deep), or an empty object when the detail is
 * missing or is no plain object.
 * @param {Event} event - The event
 * @returns {object} The payload
 */
export function detailArgs(event) {
  const detail = sanitize(event.detail, detailDepth);
  return detail && typeof detail === 'object' && !Array.isArray(detail) ? detail : {};
}

/**
 * Formats a JS Date (or a value that new Date() parses) as an ISO date, YYYY-MM-DD, in the browser's time zone, the
 * form a .NET DateOnly reads.
 * @param {*} value - A Date, or a date string or number
 * @returns {string | null} The ISO date, or null when the value is no valid date
 */
export function isoDate(value) {
  const date = value instanceof Date ? value : (value != null ? new Date(value) : null);
  if (!date || isNaN(date.getTime())) return null;
  const year = String(date.getFullYear()).padStart(4, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Registers custom event types with Blazor, once per name. Call it from a JS initializer's afterWebStarted or
 * afterStarted with the Blazor object it receives. A name that is already registered through this module (by any copy
 * of it) is skipped with one console.warn; a duplicate never throws.
 * @param {object} blazor - The Blazor object that the start callback receives
 * @param {Object<string, null | undefined | ((event: Event) => object) | { createEventArgs?: (event: Event) => object, browserEventName?: string }>} definitions -
 *   Maps each event name (without the "on" prefix, for example 'xyz-remove') to its payload: null or undefined for
 *   detailArgs, a createEventArgs function, or an object with createEventArgs (detailArgs when missing) and
 *   browserEventName (the browser event that the name stands for, when it differs)
 */
export function registerEventTypes(blazor, definitions) {
  if (!blazor || typeof blazor.registerCustomEventType !== 'function' || !definitions) return;

  const names = registeredNames();
  for (const [name, definition] of Object.entries(definitions)) {
    if (names.has(name)) {
      console.warn(`${logPrefix} the custom event type '${name}' is already registered; the second registration is skipped.`);
      continue;
    }

    const options = typeof definition === 'function'
      ? { createEventArgs: definition }
      : definition && typeof definition === 'object'
        ? { ...definition, createEventArgs: definition.createEventArgs || detailArgs }
        : { createEventArgs: detailArgs };

    blazor.registerCustomEventType(name, options);
    names.add(name);
  }
}
