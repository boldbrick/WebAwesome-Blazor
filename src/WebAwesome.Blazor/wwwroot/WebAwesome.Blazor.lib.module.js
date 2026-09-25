// Blazor JS initializer for WebAwesome.Blazor (auto-loaded by the Blazor runtime from
// _content/WebAwesome.Blazor/WebAwesome.Blazor.lib.module.js).
//
// Registers every wa-* custom event the wrappers bind so that Blazor (1) listens for the
// event at all and (2) passes a JSON-serializable payload to the .NET EventCallback<T>.
// Without registerCustomEventType, custom events bound in the render tree never reach the
// .NET handlers. The createEventArgs result is deserialized case-insensitively into the
// wrapper's typed event args (extra properties are ignored), so payload shapes here must
// stay in sync with Components\EventArgs.cs. It also relays the events Blazor cannot receive
// where Web Awesome dispatches them (relayedEvents below).

// events whose detail (when present) is JSON-safe and maps 1:1 onto the typed args
const eventNames = [
  'wa-after-collapse',
  'wa-after-expand',
  'wa-after-hide',
  'wa-after-show',
  'wa-before-page-change',
  'wa-cancel',
  'wa-cell-click',
  'wa-cell-contextmenu',
  'wa-clear',
  'wa-collapse',
  'wa-column-move',
  'wa-column-pin',
  'wa-column-resize',
  'wa-column-visibility-change',
  'wa-complete',
  'wa-content-change',
  'wa-copy',
  'wa-create',
  'wa-data-error',
  'wa-data-request',
  'wa-error',
  'wa-expand',
  'wa-filter-change',
  'wa-finish',
  'wa-focus-day',
  'wa-hide',
  'wa-hover',
  'wa-include-error',
  'wa-invalid',
  'wa-lazy-change',
  'wa-lazy-load',
  'wa-load',
  'wa-mutation',
  'wa-page-change',
  'wa-remove',
  'wa-reposition',
  'wa-resize',
  'wa-row-collapse',
  'wa-row-expand',
  'wa-row-select',
  'wa-select',
  'wa-selection-change',
  'wa-show',
  'wa-slide-change',
  'wa-sort-change',
  'wa-start',
  'wa-tab-hide',
  'wa-tab-show',
  'wa-video-change',
  'wa-view-change',
];

// native-named events that Web Awesome re-dispatches as custom events (not Blazor built-ins,
// so they need registerCustomEventType to reach .NET); empty detail -> default detailArgs
const nativeCustomEventNames = [
  'beforeinput',
];

// recursively copies JSON-safe values from an event detail, dropping DOM nodes, functions,
// and anything too deep to marshal - custom event details may carry live Element references
function sanitize(value, depth) {
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

// default payload: the sanitized event detail (or an empty object when there is none)
function detailArgs(event) {
  const detail = sanitize(event.detail, 3);
  return detail && typeof detail === 'object' && !Array.isArray(detail) ? detail : {};
}

// formats a JS Date (or ISO-parseable string) as a YYYY-MM-DD string; null when not a valid date
function isoDate(value) {
  const date = value instanceof Date ? value : (value != null ? new Date(value) : null);
  if (!date || isNaN(date.getTime())) return null;
  const year = String(date.getFullYear()).padStart(4, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

// events needing a hand-rolled payload because their detail is not JSON-safe, is empty, or
// the typed args derive values from the event itself
const specialArgs = {
  // detail is { date: Date } - project the day to an ISO string (WaDatePickerFocusDayEventArgs)
  'wa-focus-day': event => ({ date: isoDate(event.detail && event.detail.date) }),

  // detail is { view, date } where date is a Date - project the date to an ISO string
  // (WaDatePickerViewChangeEventArgs)
  'wa-view-change': event => {
    const detail = event.detail || {};
    return {
      view: typeof detail.view === 'string' ? detail.view : null,
      date: isoDate(detail.date),
    };
  },

  // WaDetailsToggleEventArgs.IsOpen is derived from which of the two events fired; the
  // extra property is ignored by every other wa-show/wa-hide consumer
  'wa-show': event => ({ ...detailArgs(event), isOpen: true }),
  'wa-hide': event => ({ ...detailArgs(event), isOpen: false }),

  // detail is { entry: IntersectionObserverEntry } - flatten the two marshalable fields; wa-intersect does not
  // bubble, so it is registered only as its relay (relayedEvents), which uses this payload
  'wa-intersect': event => {
    const entry = event.detail && event.detail.entry;
    return {
      isIntersecting: !!(entry && entry.isIntersecting),
      intersectionRatio: entry && typeof entry.intersectionRatio === 'number' ? entry.intersectionRatio : 0,
    };
  },

  // detail is { mutationList: MutationRecord[] } - records hold live DOM nodes
  'wa-mutation': event => ({
    mutationRecords: ((event.detail && event.detail.mutationList) || []).map(record => ({
      type: record.type,
      attributeName: record.attributeName,
      oldValue: record.oldValue,
    })),
  }),

  // detail is { entries: ResizeObserverEntry[] } - entries hold live DOM nodes
  'wa-resize': event => ({
    resizeObserverEntries: ((event.detail && event.detail.entries) || []).map(entry => ({
      contentRect: entry.contentRect && entry.contentRect.toJSON ? entry.contentRect.toJSON() : null,
    })),
  }),

  // detail is { items: Element[] } - live child elements now shown; project to identifying data
  // and expose the count (WaContentChangeEventArgs)
  'wa-content-change': event => {
    const items = (event.detail && event.detail.items) || [];
    return {
      count: items.length,
      items: items.map(item => ({
        id: item.id || null,
        textContent: (item.textContent || '').trim(),
      })),
    };
  },

  // detail is { selection: WaTreeItem[] } - project the live elements to identifying data
  'wa-selection-change': event => ({
    selection: ((event.detail && event.detail.selection) || []).map(item => ({
      id: item.id || null,
      textContent: (item.textContent || '').trim(),
    })),
  }),

  // detail is { previousIndex, currentIndex, video } - video is a live wa-video element;
  // project its title and drop the node (WaVideoChangeEventArgs)
  'wa-video-change': event => {
    const detail = event.detail || {};
    const video = detail.video;
    return {
      previousIndex: typeof detail.previousIndex === 'number' ? detail.previousIndex : 0,
      currentIndex: typeof detail.currentIndex === 'number' ? detail.currentIndex : 0,
      videoTitle: video && typeof video.title === 'string' ? video.title : null,
    };
  },

  // wa-reposition carries no detail; WaSplitPanelRepositionEventArgs reads the position
  // from the element itself
  'wa-reposition': event => {
    const target = event.target;
    return {
      position: target && typeof target.position === 'number' ? target.position : 0,
      positionInPixels: target && typeof target.positionInPixels === 'number' ? Math.round(target.positionInPixels) : 0,
    };
  },

  // detail also carries originalEvent (a PointerEvent/KeyboardEvent) - not marshalable
  // (WaDataGridCellContextMenuEventArgs)
  'wa-cell-contextmenu': event => {
    const detail = event.detail || {};
    return {
      column: typeof detail.column === 'string' ? detail.column : null,
      value: detail.value,
      row: detail.row || null,
      rowIndex: typeof detail.rowIndex === 'number' ? detail.rowIndex : 0,
    };
  },

  // detail.side is 'left' | 'right' | false - normalize false to null (WaDataGridColumnPinEventArgs.Side)
  'wa-column-pin': event => {
    const detail = event.detail || {};
    return {
      column: typeof detail.column === 'string' ? detail.column : null,
      side: typeof detail.side === 'string' ? detail.side : null,
    };
  },

  // detail.error is an Error instance (own enumerable props are empty) - project its message
  // (WaDataGridDataErrorEventArgs)
  'wa-data-error': event => {
    const detail = event.detail || {};
    const error = detail.error;
    return {
      error: error && typeof error.message === 'string' ? error.message : (error != null ? String(error) : null),
      request: detail.request || null,
    };
  },

  // detail.signal is an AbortSignal - not marshalable (WaDataGridDataRequestEventArgs)
  'wa-data-request': event => {
    const detail = event.detail || {};
    return {
      sort: detail.sort || [],
      filters: detail.filters || [],
      search: typeof detail.search === 'string' ? detail.search : '',
      page: typeof detail.page === 'number' ? detail.page : 0,
      pageSize: typeof detail.pageSize === 'number' ? detail.pageSize : 0,
    };
  },
};

// wa-slider and wa-rating keep their live value as a JS number and dispatch plain change/input
// events. Blazor's built-in change/input reader forwards target.value as is, and the server
// rejects a number ("Unsupported ChangeEventArgs value") before any handler runs, so a binder on
// "onchange"/"oninput" never fires for these elements. These aliases listen to the same browser
// events under non-wa names (the wrappers bind "onnumericchange"/"onnumericinput") and hand .NET
// the value as a string: "<minValue>,<maxValue>" for a range-mode wa-slider (its value is an
// unused default there), otherwise String(value), empty for null/undefined.
function numericValueArgs(event) {
  const target = event.target;
  if (!target) return { value: '' };
  if (target.range) return { value: `${target.minValue},${target.maxValue}` };
  const value = target.value;
  return { value: value === null || value === undefined ? '' : String(value) };
}

// Blazor event name -> aliased browser event name
const numericValueEventAliases = {
  'numericchange': 'change',
  'numericinput': 'input',
};

// Blazor receives an event only where it listens: on the document, in the bubbling phase for a custom event
// (and for keydown), and for a built-in non-bubbling event only at composedPath()[0]. Some events never get
// there: wa-color-picker dispatches its popup events as plain non-bubbling CustomEvents, WaIntersectEvent is
// constructed with bubbles: false, and wa-select, wa-combobox (always) and wa-color-picker (Escape while open)
// stop the propagation of the keydown in their shadow root. A capture-phase listener on the document still
// sees each of them first, so it re-dispatches every one, once, as a bubbling, composed event under a private
// name on the host element; the wrappers bind the private name (Constants.Relayed*EventAttribute) instead of
// the original, together with a Blazor-side stopPropagation, so the relayed event reaches no other wrapper.
// The original event is left untouched and page listeners never see a second wa-* event.
//
// Blazor event name -> { event: the element event it relays, hosts: the elements it is relayed for,
// source: 'host' when only the host's own dispatch counts (a nested component's event retargeted to the host
// is not relayed), 'subtree' when the event may originate anywhere inside the host (native input events) }
const relayedEvents = {
  'wablazor-show': { event: 'wa-show', hosts: ['wa-color-picker'], source: 'host' },
  'wablazor-after-show': { event: 'wa-after-show', hosts: ['wa-color-picker'], source: 'host' },
  'wablazor-hide': { event: 'wa-hide', hosts: ['wa-color-picker'], source: 'host' },
  'wablazor-after-hide': { event: 'wa-after-hide', hosts: ['wa-color-picker'], source: 'host' },
  'wablazor-intersect': { event: 'wa-intersect', hosts: ['wa-intersection-observer'], source: 'host' },
  'wablazor-keydown': { event: 'keydown', hosts: ['wa-color-picker', 'wa-combobox', 'wa-select'], source: 'subtree' },
};

// the payload Blazor's built-in keyboard reader builds (KeyboardEventArgs), for relayed keyboard events
function keyboardArgs(event) {
  return {
    key: event.key,
    code: event.code,
    location: event.location,
    repeat: event.repeat,
    ctrlKey: event.ctrlKey,
    shiftKey: event.shiftKey,
    altKey: event.altKey,
    metaKey: event.metaKey,
    type: event.type,
    isComposing: event.isComposing,
  };
}

// payload builders of the relayed native events; relayed wa-* events use their own payload (specialArgs or
// the sanitized detail)
const relayedNativeArgs = {
  'keydown': keyboardArgs,
};

// relayed event -> the original event, read by createEventArgs while the relayed event is being dispatched
const relayOrigins = new WeakMap();

// the element a relayed event is dispatched on, or null when the event is not relayed there
function relayHost(event, relay) {
  const path = event.composedPath();
  if (relay.source === 'host') {
    const origin = path[0];
    return origin instanceof Element && relay.hosts.includes(origin.localName) ? origin : null;
  }

  // the innermost listed host the event passes through
  for (const node of path) {
    if (node instanceof Element && relay.hosts.includes(node.localName)) return node;
  }
  return null;
}

function relayArgs(relay) {
  const argsOf = relayedNativeArgs[relay.event] || specialArgs[relay.event] || detailArgs;
  return event => argsOf(relayOrigins.get(event) || event);
}

// symbol marking the document once the relay listeners are installed, shared by every copy of this module
const relayInstalledKey = Symbol.for('WebAwesome.Blazor.eventRelay');

function installEventRelay() {
  if (typeof document === 'undefined' || document[relayInstalledKey]) return;
  document[relayInstalledKey] = true;

  for (const [name, relay] of Object.entries(relayedEvents)) {
    document.addEventListener(relay.event, event => {
      const host = relayHost(event, relay);
      if (!host) return;

      const relayed = new CustomEvent(name, { bubbles: true, composed: true, detail: event.detail });
      relayOrigins.set(relayed, event);
      host.dispatchEvent(relayed);
    }, true);
  }
}

let eventTypesRegistered = false;

function registerEventTypes(blazor) {
  if (eventTypesRegistered || !blazor || typeof blazor.registerCustomEventType !== 'function') return;
  eventTypesRegistered = true;

  for (const [name, relay] of Object.entries(relayedEvents)) {
    blazor.registerCustomEventType(name, {
      createEventArgs: relayArgs(relay),
    });
  }

  installEventRelay();

  for (const name of eventNames) {
    blazor.registerCustomEventType(name, {
      createEventArgs: specialArgs[name] || detailArgs,
    });
  }

  for (const name of nativeCustomEventNames) {
    blazor.registerCustomEventType(name, {
      createEventArgs: specialArgs[name] || detailArgs,
    });
  }

  for (const [name, browserEventName] of Object.entries(numericValueEventAliases)) {
    blazor.registerCustomEventType(name, {
      browserEventName,
      createEventArgs: numericValueArgs,
    });
  }
}

// Blazor Web (blazor.web.js, .NET 8+)
export function afterWebStarted(blazor) {
  registerEventTypes(blazor);
}

// classic hosts (blazor.webassembly.js / blazor.server.js)
export function afterStarted(blazor) {
  registerEventTypes(blazor);
}
