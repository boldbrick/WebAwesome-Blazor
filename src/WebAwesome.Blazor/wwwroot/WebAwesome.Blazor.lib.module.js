// Blazor JS initializer for WebAwesome.Blazor (auto-loaded by the Blazor runtime from
// _content/WebAwesome.Blazor/WebAwesome.Blazor.lib.module.js).
//
// Registers every wa-* custom event the wrappers bind so that Blazor (1) listens for the
// event at all and (2) passes a JSON-serializable payload to the .NET EventCallback<T>.
// Without registerCustomEventType, custom events bound in the render tree never reach the
// .NET handlers. The createEventArgs result is deserialized case-insensitively into the
// wrapper's typed event args (extra properties are ignored), so payload shapes here must
// stay in sync with Components\EventArgs.cs.

// events whose detail (when present) is JSON-safe and maps 1:1 onto the typed args
const eventNames = [
  'wa-after-collapse',
  'wa-after-expand',
  'wa-after-hide',
  'wa-after-show',
  'wa-cancel',
  'wa-change',
  'wa-clear',
  'wa-collapse',
  'wa-copy',
  'wa-create',
  'wa-error',
  'wa-expand',
  'wa-finish',
  'wa-hide',
  'wa-hover',
  'wa-include-error',
  'wa-initial-focus',
  'wa-intersect',
  'wa-invalid',
  'wa-lazy-change',
  'wa-lazy-load',
  'wa-load',
  'wa-mutation',
  'wa-password-toggle',
  'wa-password-visibility-change',
  'wa-remove',
  'wa-reposition',
  'wa-resize',
  'wa-select',
  'wa-selection-change',
  'wa-show',
  'wa-slide-change',
  'wa-start',
  'wa-success',
  'wa-tab-change',
  'wa-tab-close',
  'wa-tab-hide',
  'wa-tab-show',
  'wa-video-change',
  'wa-zoom-change',
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

// events needing a hand-rolled payload because their detail is not JSON-safe, is empty, or
// the typed args derive values from the event itself
const specialArgs = {
  // WaDetailsToggleEventArgs.IsOpen is derived from which of the two events fired; the
  // extra property is ignored by every other wa-show/wa-hide consumer
  'wa-show': event => ({ ...detailArgs(event), isOpen: true }),
  'wa-hide': event => ({ ...detailArgs(event), isOpen: false }),

  // detail is { entry: IntersectionObserverEntry } - flatten the two marshalable fields
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
};

let eventTypesRegistered = false;

function registerEventTypes(blazor) {
  if (eventTypesRegistered || !blazor || typeof blazor.registerCustomEventType !== 'function') return;
  eventTypesRegistered = true;

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
}

// Blazor Web (blazor.web.js, .NET 8+)
export function afterWebStarted(blazor) {
  registerEventTypes(blazor);
}

// classic hosts (blazor.webassembly.js / blazor.server.js)
export function afterStarted(blazor) {
  registerEventTypes(blazor);
}
