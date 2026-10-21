// Content script of the WA Inspector: the engine that runs in the inspected page.
//
// - It builds the element tree the panel shows: wa- elements and plain elements with Web Awesome utility classes.
// - It applies the review session of the page's origin (chrome.storage.local, key "wai:<origin>"): attribute
//   overrides, value remaps, class changes, CSS custom properties, part styles and theme tokens. The session is the
//   source of truth; the panel writes it, this script follows every change. A MutationObserver keeps the overrides on
//   elements that the page renders again or changes, so they stay applied while the reviewer navigates.
// - It draws the numbered outlines of the change-list items, the hover highlight and the pick mode in an overlay.
// - It answers the panel's requests ("page.*", routed by background.js) and reports events back.
//
// The page's own attribute values are kept as "source" values: an override never loses them, the panel shows them,
// and a value the page changes on its own counts as evidence that the attribute is data-driven.
(() => {
  if (window.__waInspectorContent) return;
  window.__waInspectorContent = true;

  // the session of this origin in chrome.storage.local
  const STORAGE_KEY = `wai:${location.origin}`;

  // the placement metadata the WebAwesome.Blazor wrappers write (WebAwesomeOptions.EmitInspectorMetadata)
  const META_COMPONENT = 'data-wablazor-component';
  const META_CHAIN = 'data-wablazor-chain';
  const META_PLACEMENT = 'data-wablazor-placement';
  const META_SITE = 'data-wablazor-site';

  // the marker attribute that ties an element to the items that match it (the stylesheet selects on it)
  const MATCH_ATTRIBUTE = 'data-wai-match';
  // the marker of the element the DevTools "inspect" call selects
  const INSPECT_ATTRIBUTE = 'data-wai-inspect';
  // the event the DevTools panel dispatches on $0 to select it here
  const INSPECT_EVENT = 'wai-inspect';

  // attributes that are no review settings
  const IGNORED_ATTRIBUTES = new Set(['class', 'style', 'id', MATCH_ATTRIBUTE, INSPECT_ATTRIBUTE]);
  // the attributes the tree shows next to the tag
  const TREE_ATTRIBUTES = ['variant', 'appearance', 'size', 'orientation', 'placement'];
  // a Blazor CSS isolation scope attribute, for example b-3xx5ti8xyz
  const SCOPE_ATTRIBUTE_PATTERN = /^b-[a-z0-9]{10}$/;
  // an id that looks generated (numbers, GUID parts) is no stable part of a structural key
  const GENERATED_ID_PATTERN = /\d{2,}|[-_:][0-9a-f]{6,}/i;

  const TREE_NODE_LIMIT = 5000;
  const TREE_EVENT_DELAY_MS = 300;
  const TEXT_SNIPPET_LENGTH = 60;
  const MARKER_LIMIT_PER_ITEM = 60;
  const FLASH_MS = 1200;
  const CAPTURE_PADDING = 8;
  // how often the echo guard checks whether the elements it waits for have upgraded, and how long it waits at most
  const ECHO_SWEEP_MS = 50;
  const ECHO_MAX_MS = 15000;

  let session = emptySession();

  // ids of the elements for the messages to the panel
  const idsByElement = new WeakMap();
  const elementsById = new Map();
  let nextElementId = 1;

  // per element: the page's own values of the attributes and classes this script overrides, and what it applied
  const sourceAttributes = new WeakMap();
  const appliedAttributes = new WeakMap();
  const sourceClassNames = new WeakMap();
  const appliedClassNames = new WeakMap();
  // per element: the field kind and the element default of each overridden attribute, from the item operation (the
  // panel stores them from the element metadata); kept while the override applies, so that the restore can use them
  const attributeFields = new WeakMap();
  // per element: the element defaults this script wrote in place of a removal (the sticky rule, see writeAttribute),
  // by attribute name; such a value is no page value
  const standIns = new WeakMap();
  // the elements this script changed (attributes, classes, match marker)
  const touched = new Set();
  // the echo guard: a custom element can write the value of this script's attribute write back to the attribute when
  // it upgrades or updates next (Lit reflects a property that it set from the attribute), which is no page change;
  // by element: the attribute values this script wrote, kept until the element has upgraded and updated
  const echoes = new WeakMap();
  const echoElements = new Map();
  let echoTimer = 0;

  // the element an instance-scoped item matched last, by item id
  const instanceElements = new Map();
  // values the page set on its own, by placement key, attribute and value
  const observedHistory = new Map();
  // the elements each item matched in the last apply pass
  let lastMatches = new Map();

  let overlay = null;
  let styleElement = null;
  let highlightElement = null;
  let flashElement = null;
  let flashTimer = 0;
  let pickMode = false;
  let pickCandidate = null;
  let captureMarkers = null;
  let treeEventTimer = 0;
  let applying = false;

  // ---------------------------------------------------------------------------------------------------------------
  // session

  function emptySession() {
    return { version: 1, items: [], theme: {}, markersVisible: true, nextNumber: 1 };
  }

  function loadSession() {
    chrome.storage.local.get(STORAGE_KEY).then(stored => {
      session = Object.assign(emptySession(), stored[STORAGE_KEY] || {});
      apply();
    }).catch(() => {});
  }

  chrome.storage.onChanged.addListener((changes, area) => {
    if (area !== 'local' || !changes[STORAGE_KEY]) return;
    session = Object.assign(emptySession(), changes[STORAGE_KEY].newValue || {});
    apply();
    sendEvent('sessionApplied', {});
  });

  // ---------------------------------------------------------------------------------------------------------------
  // element ids and descriptions

  function idOf(element) {
    let id = idsByElement.get(element);
    if (!id) {
      id = nextElementId++;
      idsByElement.set(element, id);
      elementsById.set(id, new WeakRef(element));
    }
    return id;
  }

  function elementOf(id) {
    const reference = elementsById.get(id);
    const element = reference && reference.deref();
    if (!element || !element.isConnected) {
      elementsById.delete(id);
      return null;
    }
    return element;
  }

  function isWaElement(element) {
    return element.localName.startsWith('wa-');
  }

  function sourceClassList(element) {
    const className = sourceClassNames.has(element) ? sourceClassNames.get(element) : element.getAttribute('class') || '';
    return className.split(/\s+/).filter(Boolean);
  }

  function hasUtilityClass(element) {
    if (element === document.documentElement || element === document.body) return false;
    return sourceClassList(element).some(name => name.startsWith('wa-'));
  }

  function isOwnNode(element) {
    return element === overlay || element === styleElement;
  }

  function textSnippet(element) {
    const text = (element.textContent || '').replace(/\s+/g, ' ').trim();
    return text.length > TEXT_SNIPPET_LENGTH ? `${text.slice(0, TEXT_SNIPPET_LENGTH - 1)}…` : text;
  }

  function sourceAttribute(element, name) {
    const sources = sourceAttributes.get(element);
    return sources && sources.has(name) ? sources.get(name) : pageAttribute(element, name);
  }

  // the current value of an attribute as the page set it: null when the value is an element default that this script
  // wrote in place of a removal
  function pageAttribute(element, name) {
    const value = element.getAttribute(name);
    return isStandIn(element, name, value) ? null : value;
  }

  function isStandIn(element, name, value) {
    const written = standIns.get(element);
    return !!written && written.has(name) && written.get(name) === value;
  }

  function forgetStandIn(element, name) {
    const written = standIns.get(element);
    if (written) written.delete(name);
  }

  // one ancestor level of a structural key: tag, stable id, classes, slot and CSS isolation scope; no positions, so
  // all instances of a loop share the key. The classes of <html> and <body> are page state (theme, color scheme,
  // scroll lock), not structure: a theme switch must not lose the items
  function signature(element) {
    let text = element.localName;
    if (element === document.documentElement || element === document.body) return text;
    if (element.id && !GENERATED_ID_PATTERN.test(element.id)) text += `#${element.id}`;
    const classes = sourceClassList(element).sort();
    if (classes.length) text += `.${classes.join('.')}`;
    const slot = element.getAttribute('slot');
    if (slot) text += `[slot=${slot}]`;
    for (const attribute of element.attributes) {
      if (SCOPE_ATTRIBUTE_PATTERN.test(attribute.name)) text += `[${attribute.name}]`;
    }
    return text;
  }

  function structuralKey(element) {
    const parts = [];
    for (let current = element; current && current.nodeType === Node.ELEMENT_NODE; current = parentElementOf(current)) {
      parts.push(signature(current));
    }
    return hash(parts.reverse().join('>'));
  }

  // the parent element, across the boundary of an open shadow root of a non-wa host
  function parentElementOf(element) {
    if (element.parentElement) return element.parentElement;
    const root = element.parentNode;
    return root && root.host ? root.host : null;
  }

  function placementKey(element) {
    return element.getAttribute(META_PLACEMENT) || `s:${structuralKey(element)}`;
  }

  // FNV-1a, 32 bits, as 8 hex digits
  function hash(text) {
    let value = 0x811c9dc5;
    for (let index = 0; index < text.length; index++) {
      value ^= text.charCodeAt(index);
      value = Math.imul(value, 0x01000193) >>> 0;
    }
    return value.toString(16).padStart(8, '0');
  }

  // a readable CSS path for the change list (positions included, so it names one instance)
  function cssPath(element) {
    const parts = [];
    for (let current = element; current && current !== document.documentElement; current = parentElementOf(current)) {
      let part = current.localName;
      if (current.id && !GENERATED_ID_PATTERN.test(current.id)) {
        parts.push(`${part}#${current.id}`);
        break;
      }
      const classes = sourceClassList(current).filter(name => !SCOPE_ATTRIBUTE_PATTERN.test(name)).slice(0, 3);
      if (classes.length) part += `.${classes.join('.')}`;
      const parent = current.parentElement;
      if (parent) {
        const siblings = [...parent.children].filter(child => child.localName === current.localName);
        if (siblings.length > 1) part += `:nth-of-type(${siblings.indexOf(current) + 1})`;
      }
      parts.push(part);
    }
    return parts.reverse().join(' > ');
  }

  // the nearest ancestor (or the element itself) with Blazor placement metadata
  function metadataOwner(element) {
    for (let current = element; current; current = parentElementOf(current)) {
      if (current.hasAttribute(META_COMPONENT)) return current;
    }
    return null;
  }

  function metadataOf(element) {
    if (!element.hasAttribute(META_COMPONENT)) return null;
    return {
      component: element.getAttribute(META_COMPONENT),
      chain: element.getAttribute(META_CHAIN),
      placement: element.getAttribute(META_PLACEMENT),
      site: element.getAttribute(META_SITE),
    };
  }

  function scopeAttributes(element) {
    return [...element.attributes].map(attribute => attribute.name).filter(name => SCOPE_ATTRIBUTE_PATTERN.test(name));
  }

  function groupMembers(element) {
    const placement = element.getAttribute(META_PLACEMENT);
    if (placement) return [...document.querySelectorAll(`[${META_PLACEMENT}="${CSS.escape(placement)}"]`)];
    const key = structuralKey(element);
    return [...document.querySelectorAll(CSS.escape(element.localName))].filter(candidate => structuralKey(candidate) === key);
  }

  function locatorOf(element) {
    const meta = metadataOf(element);
    const owner = meta ? null : metadataOwner(element);
    const members = groupMembers(element);
    return {
      tag: element.localName,
      component: meta && meta.component,
      chain: meta && meta.chain,
      placement: meta && meta.placement,
      site: meta && meta.site,
      ownerComponent: owner && owner.getAttribute(META_COMPONENT),
      ownerChain: owner && owner.getAttribute(META_CHAIN),
      structuralKey: structuralKey(element),
      path: cssPath(element),
      text: textSnippet(element),
      classes: sourceClassList(element),
      id: element.id || null,
      slot: element.getAttribute('slot'),
      scopeAttributes: scopeAttributes(element),
      ordinal: Math.max(0, members.indexOf(element)),
      url: location.href,
      title: document.title,
    };
  }

  // ---------------------------------------------------------------------------------------------------------------
  // tree

  function buildTree() {
    const root = { id: 0, tag: '#document', children: [] };
    const placementCounts = new Map();
    let count = 0;
    let truncated = false;

    const walk = (parent, node) => {
      for (const child of parent.children) {
        if (isOwnNode(child)) continue;
        if (count >= TREE_NODE_LIMIT) {
          truncated = true;
          return;
        }
        const included = isWaElement(child) || hasUtilityClass(child);
        let target = node;
        if (included) {
          count++;
          target = describeNode(child, placementCounts);
          node.children.push(target);
        }
        walk(child, target);
        // a wa- element's shadow root is its internal markup; another element's open shadow root is app content
        if (child.shadowRoot && !isWaElement(child)) walk(child.shadowRoot, target);
      }
    };
    if (document.body) walk(document.body, root);

    // the instance count of each node's placement, once the whole tree is known
    const fill = node => {
      if (node.groupKey) node.count = placementCounts.get(node.groupKey) || 1;
      delete node.groupKey;
      node.children.forEach(fill);
    };
    fill(root);
    return { root, count, truncated };
  }

  function describeNode(element, placementCounts) {
    const isWa = isWaElement(element);
    const groupKey = placementKey(element);
    placementCounts.set(groupKey, (placementCounts.get(groupKey) || 0) + 1);

    const attributes = {};
    if (isWa) {
      for (const name of TREE_ATTRIBUTES) {
        const value = element.getAttribute(name);
        if (value !== null) attributes[name] = value;
      }
    }
    return {
      id: idOf(element),
      tag: element.localName,
      isWa,
      classes: sourceClassList(element).filter(name => name.startsWith('wa-')),
      attributes,
      component: element.getAttribute(META_COMPONENT),
      label: isWa ? textSnippet(element) : '',
      items: itemsMatching(element),
      groupKey,
      children: [],
    };
  }

  function itemsMatching(element) {
    const ids = [];
    for (const [itemId, elements] of lastMatches) {
      if (elements.includes(element)) ids.push(itemId);
    }
    return ids;
  }

  function scheduleTreeEvent() {
    clearTimeout(treeEventTimer);
    treeEventTimer = setTimeout(() => sendEvent('treeChanged', {}), TREE_EVENT_DELAY_MS);
  }

  // ---------------------------------------------------------------------------------------------------------------
  // details of one element

  function detailsOf(element) {
    const isWa = isWaElement(element);
    const attributes = {};
    const sources = {};
    for (const attribute of element.attributes) {
      if (IGNORED_ATTRIBUTES.has(attribute.name) || attribute.name.startsWith('data-wablazor-')) continue;
      if (SCOPE_ATTRIBUTE_PATTERN.test(attribute.name) || attribute.name.startsWith('_bl_')) continue;
      // an element default in place of a removal shows as "not set"
      if (isStandIn(element, attribute.name, attribute.value)) continue;
      attributes[attribute.name] = attribute.value;
    }
    const applied = appliedAttributes.get(element);
    if (applied) {
      for (const name of applied.keys()) sources[name] = sourceAttribute(element, name);
    }

    const members = groupMembers(element);
    const site = element.getAttribute(META_SITE);
    const owner = metadataOwner(element);

    const ancestors = [];
    for (let current = parentElementOf(element); current; current = parentElementOf(current)) {
      if (isWaElement(current) || hasUtilityClass(current) || current.hasAttribute(META_COMPONENT)) {
        ancestors.unshift({ id: idOf(current), tag: current.localName, label: isWaElement(current) ? textSnippet(current) : '' });
      }
    }

    return {
      id: idOf(element),
      tag: element.localName,
      isWa,
      label: textSnippet(element),
      attributes,
      sourceAttributes: sources,
      classes: (element.getAttribute('class') || '').split(/\s+/).filter(Boolean),
      sourceClasses: sourceClassList(element),
      meta: metadataOf(element),
      owner: owner && owner !== element
        ? { id: idOf(owner), tag: owner.localName, component: owner.getAttribute(META_COMPONENT), chain: owner.getAttribute(META_CHAIN) }
        : null,
      locator: locatorOf(element),
      counts: {
        placement: members.length,
        site: site ? document.querySelectorAll(`[${META_SITE}="${CSS.escape(site)}"]`).length : null,
        tag: isWa ? document.querySelectorAll(CSS.escape(element.localName)).length : null,
      },
      observed: isWa ? observedValues(element, members) : {},
      ancestors,
      items: itemsMatching(element),
      inlineStyle: element.getAttribute('style'),
    };
  }

  // the values of each attribute across the instances of the element's placement, plus the values the page set
  // over time; more than one value means the attribute is data-driven
  function observedValues(element, members) {
    const result = {};
    const add = (name, value, count) => {
      const key = value === null ? '∅' : value;
      result[name] = result[name] || {};
      result[name][key] = (result[name][key] || 0) + count;
    };
    const names = new Set();
    for (const member of members) {
      for (const attribute of member.attributes) names.add(attribute.name);
      const applied = appliedAttributes.get(member);
      if (applied) applied.forEach((_, name) => names.add(name));
    }
    for (const name of names) {
      if (IGNORED_ATTRIBUTES.has(name) || name.startsWith('data-') || SCOPE_ATTRIBUTE_PATTERN.test(name) || name.startsWith('_bl_')) continue;
      for (const member of members) add(name, sourceAttribute(member, name), 1);
    }
    const history = observedHistory.get(placementKey(element));
    if (history) {
      for (const [name, values] of history) {
        for (const value of values.keys()) {
          if (!result[name] || !(value in result[name])) add(name, value === '∅' ? null : value, 0);
        }
      }
    }
    return result;
  }

  function recordObserved(element, name, value) {
    if (IGNORED_ATTRIBUTES.has(name) || name.startsWith('data-')) return;
    const key = placementKey(element);
    if (!observedHistory.has(key)) observedHistory.set(key, new Map());
    const byName = observedHistory.get(key);
    if (!byName.has(name)) byName.set(name, new Map());
    const values = byName.get(name);
    const valueKey = value === null ? '∅' : value;
    values.set(valueKey, (values.get(valueKey) || 0) + 1);
  }

  // ---------------------------------------------------------------------------------------------------------------
  // matching items to elements

  function matchItem(item) {
    const target = item.target || {};
    const byAttribute = (name, value) => [...document.querySelectorAll(`[${name}="${CSS.escape(value)}"]`)];
    const structural = () => [...document.querySelectorAll(CSS.escape(target.tag))]
      .filter(candidate => !isOwnNode(candidate) && structuralKey(candidate) === target.structuralKey);
    const placementGroup = () => target.placement ? byAttribute(META_PLACEMENT, target.placement) : structural();

    switch (item.scope) {
      case 'tag':
        return target.tag && target.tag.startsWith('wa-') ? [...document.querySelectorAll(CSS.escape(target.tag))] : [];
      case 'site':
        return target.site ? byAttribute(META_SITE, target.site) : placementGroup();
      case 'instance': {
        const known = instanceElements.get(item.id);
        const element = known && known.deref();
        if (element && element.isConnected) return [element];
        const candidates = placementGroup();
        // after a reload: the same text first, else the same position in the group
        const found = candidates.find(candidate => target.text && textSnippet(candidate) === target.text)
          || candidates[target.ordinal || 0];
        if (!found) return [];
        instanceElements.set(item.id, new WeakRef(found));
        return [found];
      }
      default:
        return placementGroup();
    }
  }

  // ---------------------------------------------------------------------------------------------------------------
  // applying the session

  // writes an attribute value, or removes the attribute for null; returns the value the attribute has now. The sticky
  // rule of the WebAwesome.Blazor wrappers (WaAttributeMemory): when Lit sees a non-boolean attribute removed, it sets
  // the property to null, not to its default. So for a rendered non-boolean attribute of a wa- element with a literal
  // element default, this writes the default instead of a removal. Boolean attributes keep present and absent. An
  // operation of an older session has no field kind and no default: the attribute is removed
  function writeAttribute(element, name, value, field) {
    if (value === null || value === undefined) {
      const standIn = standInValue(element, name, field);
      if (standIn === null) {
        forgetStandIn(element, name);
        if (element.hasAttribute(name)) element.removeAttribute(name);
        return null;
      }
      if (!standIns.has(element)) standIns.set(element, new Map());
      standIns.get(element).set(name, standIn);
      value = standIn;
    } else {
      forgetStandIn(element, name);
    }
    if (element.getAttribute(name) !== value) {
      element.setAttribute(name, value);
      expectEcho(element, name, value);
    }
    return value;
  }

  // the element default to write in place of a removal, or null to remove the attribute; "rendered" means the
  // attribute is on the element now (the page, the element's own reflection or this script set it)
  function standInValue(element, name, field) {
    if (!field || typeof field.kind !== 'string' || field.kind === 'boolean') return null;
    if (typeof field.elementDefault !== 'string') return null;
    if (!isWaElement(element) || !element.hasAttribute(name)) return null;
    return field.elementDefault;
  }

  function fieldOf(operation) {
    return { kind: operation.kind, elementDefault: operation.elementDefault };
  }

  function expectEcho(element, name, value) {
    if (!element.localName.includes('-')) return;
    if (!echoes.has(element)) echoes.set(element, new Map());
    echoes.get(element).set(name, value);
    echoElements.set(element, Date.now());
    if (!echoTimer) echoTimer = setTimeout(sweepEchoes, ECHO_SWEEP_MS);
  }

  // whether a record is the element's echo of this script's write; an echo comes at most once
  function isEcho(element, name, value) {
    const written = echoes.get(element);
    if (!written || written.get(name) !== value) return false;
    written.delete(name);
    return true;
  }

  // an upgraded element echoes in the task of the write (Lit updates in a microtask); an element whose definition has
  // not loaded yet echoes when it upgrades
  function sweepEchoes() {
    echoTimer = 0;
    const now = Date.now();
    for (const [element, writtenAt] of echoElements) {
      if (element.isConnected && !element.matches(':defined') && now - writtenAt < ECHO_MAX_MS) continue;
      echoes.delete(element);
      echoElements.delete(element);
    }
    if (echoElements.size) echoTimer = setTimeout(sweepEchoes, ECHO_SWEEP_MS);
  }

  function desiredValue(operation, source) {
    if (operation.remap) {
      const key = source === null ? '∅' : source;
      if (!(key in operation.remap)) return source;
      const mapped = operation.remap[key];
      return mapped === '∅' ? null : mapped;
    }
    if (operation.remove) return null;
    return operation.value === undefined ? source : String(operation.value);
  }

  function apply() {
    if (applying) return;
    applying = true;
    try {
      // the page's changes that the observer has not delivered yet come first: they can change the source values
      if (readMutations(observer.takeRecords()).treeChanged) scheduleTreeEvent();
      applyItems();
      applyStyles();
      updateOverlay();
    } finally {
      // the records of this script's own writes: they are no page change, so they give no source value
      observer.takeRecords();
      applying = false;
    }
  }

  function applyItems() {
    const attributeOperations = new Map();
    const classOperations = new Map();
    const matchIds = new Map();
    lastMatches = new Map();

    for (const item of session.items) {
      let elements = [];
      try {
        elements = matchItem(item);
      } catch {
        elements = [];
      }
      lastMatches.set(item.id, elements);

      for (const element of elements) {
        if (!matchIds.has(element)) matchIds.set(element, []);
        matchIds.get(element).push(item.id);

        for (const [name, operation] of Object.entries(item.attributes || {})) {
          if (!attributeOperations.has(element)) attributeOperations.set(element, new Map());
          attributeOperations.get(element).set(name, operation);
        }
        const classes = item.classes || {};
        if ((classes.add && classes.add.length) || (classes.remove && classes.remove.length)) {
          if (!classOperations.has(element)) classOperations.set(element, { add: new Set(), remove: new Set() });
          const operation = classOperations.get(element);
          (classes.remove || []).forEach(name => { operation.remove.add(name); operation.add.delete(name); });
          (classes.add || []).forEach(name => { operation.add.add(name); operation.remove.delete(name); });
        }
      }
    }

    // attributes
    for (const [element, operations] of attributeOperations) {
      const sources = sourceAttributes.get(element) || new Map();
      const applied = appliedAttributes.get(element) || new Map();
      const fields = attributeFields.get(element) || new Map();
      for (const [name, operation] of operations) {
        if (!sources.has(name)) sources.set(name, pageAttribute(element, name));
        fields.set(name, fieldOf(operation));
        // the value the attribute has after the write: the echo guard and the page-change check compare with it
        applied.set(name, writeAttribute(element, name, desiredValue(operation, sources.get(name)), fields.get(name)));
      }
      for (const name of [...applied.keys()]) {
        if (operations.has(name)) continue;
        writeAttribute(element, name, sources.get(name), fields.get(name));
        applied.delete(name);
        sources.delete(name);
        fields.delete(name);
      }
      sourceAttributes.set(element, sources);
      appliedAttributes.set(element, applied);
      attributeFields.set(element, fields);
      touched.add(element);
    }

    // classes
    for (const [element, operation] of classOperations) {
      if (!sourceClassNames.has(element)) sourceClassNames.set(element, element.getAttribute('class') || '');
      const classes = sourceClassNames.get(element).split(/\s+/).filter(name => name && !operation.remove.has(name));
      for (const name of operation.add) {
        if (!classes.includes(name)) classes.push(name);
      }
      const className = classes.join(' ');
      appliedClassNames.set(element, className);
      if ((element.getAttribute('class') || '') !== className) element.setAttribute('class', className);
      touched.add(element);
    }

    // the match marker the stylesheet selects on
    for (const [element, ids] of matchIds) {
      const value = ids.join(' ');
      if (element.getAttribute(MATCH_ATTRIBUTE) !== value) element.setAttribute(MATCH_ATTRIBUTE, value);
      touched.add(element);
    }

    // restore what is no longer wanted
    for (const element of [...touched]) {
      if (!element.isConnected) {
        touched.delete(element);
        continue;
      }
      if (!attributeOperations.has(element) && appliedAttributes.has(element)) {
        const sources = sourceAttributes.get(element);
        const fields = attributeFields.get(element) || new Map();
        for (const name of appliedAttributes.get(element).keys()) writeAttribute(element, name, sources.get(name), fields.get(name));
        appliedAttributes.delete(element);
        sourceAttributes.delete(element);
        attributeFields.delete(element);
      }
      if (!classOperations.has(element) && sourceClassNames.has(element)) {
        const className = sourceClassNames.get(element);
        if ((element.getAttribute('class') || '') !== className) {
          if (className) element.setAttribute('class', className);
          else element.removeAttribute('class');
        }
        sourceClassNames.delete(element);
        appliedClassNames.delete(element);
      }
      if (!matchIds.has(element) && element.hasAttribute(MATCH_ATTRIBUTE)) element.removeAttribute(MATCH_ATTRIBUTE);
      if (!appliedAttributes.has(element) && !sourceClassNames.has(element) && !matchIds.has(element)) touched.delete(element);
    }
  }

  function important(declarations) {
    return declarations.split(';').map(part => part.trim()).filter(Boolean)
      .map(part => /!important\s*$/i.test(part) ? part : `${part} !important`).join('; ');
  }

  function applyStyles() {
    const rules = [];
    const theme = Object.entries(session.theme || {}).filter(([, value]) => value !== '' && value !== null);
    if (theme.length) rules.push(`:root { ${theme.map(([name, value]) => `${name}: ${value} !important`).join('; ')} }`);

    for (const item of session.items) {
      const selector = `[${MATCH_ATTRIBUTE}~="${CSS.escape(item.id)}"]`;
      const properties = Object.entries(item.cssProperties || {}).filter(([, value]) => value !== '' && value !== null);
      if (properties.length) rules.push(`${selector} { ${properties.map(([name, value]) => `${name}: ${value} !important`).join('; ')} }`);
      if (item.inlineStyle) rules.push(`${selector} { ${important(item.inlineStyle)} }`);
      for (const [part, declarations] of Object.entries(item.partStyles || {})) {
        if (declarations && declarations.trim()) rules.push(`${selector}::part(${part}) { ${important(declarations)} }`);
      }
    }

    const css = rules.join('\n');
    if (!css) {
      if (styleElement) styleElement.textContent = '';
      return;
    }
    ensureStyleElement();
    if (styleElement.textContent !== css) styleElement.textContent = css;
  }

  function ensureStyleElement() {
    if (!styleElement) {
      styleElement = document.createElement('style');
      styleElement.id = 'wa-inspector-styles';
    }
    // outside body and head, which the page's framework may manage; re-added when a navigation removes it
    if (!styleElement.isConnected) document.documentElement.appendChild(styleElement);
  }

  // ---------------------------------------------------------------------------------------------------------------
  // overlay: numbered outlines, hover highlight, pick mode

  let overlayRoot = null;
  let overlayFrame = 0;
  // the item numbers of the outlines drawn last, one per outline (the "page.drawnMarkers" test hook)
  let drawnMarkers = [];

  function ensureOverlay() {
    if (!overlay) {
      overlay = document.createElement('div');
      overlay.id = 'wa-inspector-overlay';
      overlay.setAttribute('style', 'position: fixed; inset: 0; pointer-events: none; z-index: 2147483647;');
      overlayRoot = overlay.attachShadow({ mode: 'closed' });
      overlayRoot.innerHTML = `<style>
        .box { position: fixed; box-sizing: border-box; border: 2px solid var(--color); border-radius: 3px; }
        .box.highlight { border-style: dashed; background: color-mix(in srgb, var(--color) 12%, transparent); }
        .box.flash { border-width: 3px; background: color-mix(in srgb, var(--color) 25%, transparent); }
        .badge { position: absolute; top: -10px; left: -10px; min-width: 20px; height: 20px; padding: 0 4px;
          box-sizing: border-box; border-radius: 10px; background: var(--color); color: white;
          font: 600 11px/20px system-ui, sans-serif; text-align: center; box-shadow: 0 0 0 2px white; }
        .tag { position: absolute; bottom: 100%; left: 0; margin-bottom: 2px; padding: 1px 4px; border-radius: 3px;
          background: var(--color); color: white; font: 11px/16px ui-monospace, monospace; white-space: nowrap; }
      </style><div class="layer"></div>`;
    }
    if (!overlay.isConnected) document.documentElement.appendChild(overlay);
  }

  function markersVisible() {
    return captureMarkers === null ? session.markersVisible !== false : captureMarkers;
  }

  function updateOverlay() {
    const needed = (markersVisible() && session.items.length > 0) || highlightElement || flashElement || (pickMode && pickCandidate);
    if (!needed) {
      if (overlayRoot) overlayRoot.querySelector('.layer').replaceChildren();
      cancelAnimationFrame(overlayFrame);
      overlayFrame = 0;
      drawnMarkers = [];
      return;
    }
    ensureOverlay();
    if (!overlayFrame) overlayFrame = requestAnimationFrame(drawOverlay);
  }

  function drawOverlay() {
    overlayFrame = 0;
    if (!overlayRoot) return;
    const layer = overlayRoot.querySelector('.layer');
    const boxes = [];
    const markers = [];
    const box = (element, color, className, badge, tag) => {
      if (!element || !element.isConnected) return false;
      const rect = element.getBoundingClientRect();
      if (rect.width === 0 && rect.height === 0) return false;
      if (rect.bottom < 0 || rect.top > innerHeight || rect.right < 0 || rect.left > innerWidth) return false;
      const node = document.createElement('div');
      node.className = `box ${className}`;
      node.style.cssText = `--color: ${color}; left: ${rect.left}px; top: ${rect.top}px; width: ${rect.width}px; height: ${rect.height}px;`;
      if (badge) {
        const label = document.createElement('div');
        label.className = 'badge';
        label.textContent = badge;
        node.appendChild(label);
      }
      if (tag) {
        const label = document.createElement('div');
        label.className = 'tag';
        label.textContent = tag;
        node.appendChild(label);
      }
      boxes.push(node);
      return true;
    };

    if (markersVisible()) {
      for (const item of session.items) {
        const elements = (lastMatches.get(item.id) || []).slice(0, MARKER_LIMIT_PER_ITEM);
        for (const element of elements) {
          if (box(element, item.color || '#e6194b', 'marker', String(item.number), null)) markers.push(item.number);
        }
      }
    }
    if (highlightElement) box(highlightElement, '#1a73e8', 'highlight', null, highlightElement.localName);
    if (pickMode && pickCandidate) box(pickCandidate, '#1a73e8', 'highlight', null, pickCandidate.localName);
    if (flashElement) box(flashElement, '#f58231', 'flash', null, null);

    layer.replaceChildren(...boxes);
    drawnMarkers = markers;
    // keep the outlines on their elements while the page scrolls, resizes or animates
    if (boxes.length) overlayFrame = requestAnimationFrame(drawOverlay);
  }

  function flash(element) {
    flashElement = element;
    clearTimeout(flashTimer);
    flashTimer = setTimeout(() => {
      flashElement = null;
      updateOverlay();
    }, FLASH_MS);
    updateOverlay();
  }

  // ---------------------------------------------------------------------------------------------------------------
  // pick mode

  function setPickMode(enabled) {
    if (pickMode === enabled) return;
    pickMode = enabled;
    const options = { capture: true };
    if (enabled) {
      addEventListener('mousemove', onPickMove, options);
      addEventListener('click', onPickClick, options);
      addEventListener('mousedown', swallow, options);
      addEventListener('mouseup', swallow, options);
      addEventListener('pointerdown', swallow, options);
      addEventListener('pointerup', swallow, options);
      addEventListener('keydown', onPickKey, options);
    } else {
      removeEventListener('mousemove', onPickMove, options);
      removeEventListener('click', onPickClick, options);
      removeEventListener('mousedown', swallow, options);
      removeEventListener('mouseup', swallow, options);
      removeEventListener('pointerdown', swallow, options);
      removeEventListener('pointerup', swallow, options);
      removeEventListener('keydown', onPickKey, options);
      pickCandidate = null;
    }
    updateOverlay();
  }

  function onPickMove(event) {
    const element = document.elementFromPoint(event.clientX, event.clientY);
    if (element && !isOwnNode(element) && element !== pickCandidate) {
      pickCandidate = element;
      updateOverlay();
    }
  }

  function onPickClick(event) {
    event.preventDefault();
    event.stopImmediatePropagation();
    const element = pickCandidate || document.elementFromPoint(event.clientX, event.clientY);
    setPickMode(false);
    if (element) sendEvent('selected', { id: idOf(element) });
    sendEvent('pickEnded', {});
  }

  function onPickKey(event) {
    if (event.key !== 'Escape') return;
    event.preventDefault();
    event.stopImmediatePropagation();
    setPickMode(false);
    sendEvent('pickEnded', {});
  }

  function swallow(event) {
    event.preventDefault();
    event.stopImmediatePropagation();
  }

  // the DevTools panel selects $0 through this event
  document.addEventListener(INSPECT_EVENT, event => {
    const element = event.target;
    if (element && element.nodeType === Node.ELEMENT_NODE) sendEvent('selected', { id: idOf(element), fromDevTools: true });
  }, true);

  // ---------------------------------------------------------------------------------------------------------------
  // observing the page

  // reads the page's own changes from mutation records: apply() takes the records of this script's own writes before
  // they get here, so a record of an overridden attribute or class is a page write, also one that sets the value the
  // override shows already (a page that catches up with the review), unless it is a custom element's echo of this
  // script's write (the echo guard); returns whether the session must be applied again and whether the tree changed
  function readMutations(records) {
    let reapply = false;
    let treeChanged = false;
    for (const record of records) {
      const target = record.target;
      if (target === overlay || target === styleElement || (overlay && overlay.contains(target))) continue;

      if (record.type === 'childList') {
        if ([...record.removedNodes].some(node => node === overlay || node === styleElement)) reapply = true;
        if ([...record.addedNodes, ...record.removedNodes].some(node => node.nodeType === Node.ELEMENT_NODE && !isOwnNode(node))) {
          reapply = true;
          treeChanged = true;
        }
        continue;
      }
      if (record.type !== 'attributes') continue;

      const name = record.attributeName;
      if (name === MATCH_ATTRIBUTE) {
        // a framework that syncs the DOM (enhanced navigation) removed the marker
        if (session.items.length) reapply = true;
        continue;
      }
      if (name === INSPECT_ATTRIBUTE) continue;

      if (name === 'class') {
        if (appliedClassNames.has(target)) {
          // the page set its own classes: they are the new source, the overrides go on top
          sourceClassNames.set(target, target.getAttribute('class') || '');
          reapply = true;
        }
        if (hasUtilityClass(target) || (record.oldValue || '').includes('wa-')) treeChanged = true;
        continue;
      }

      const applied = appliedAttributes.get(target);
      const current = target.getAttribute(name);
      if (applied && applied.has(name)) {
        if (current === applied.get(name) && isEcho(target, name, current)) continue;
        // a page write: an element default this script wrote in place of a removal is gone
        forgetStandIn(target, name);
        sourceAttributes.get(target).set(name, current);
        recordObserved(target, name, current);
        reapply = true;
      } else if (target.localName && target.localName.startsWith('wa-')) {
        if (standIns.has(target) && standIns.get(target).has(name)) {
          if (isStandIn(target, name, current) && isEcho(target, name, current)) continue;
          forgetStandIn(target, name);
        }
        if (record.oldValue !== current) {
          recordObserved(target, name, current);
          if (TREE_ATTRIBUTES.includes(name)) treeChanged = true;
        }
      }
    }
    return { reapply, treeChanged };
  }

  const observer = new MutationObserver(records => {
    const { reapply, treeChanged } = readMutations(records);
    if (reapply) apply();
    if (treeChanged) scheduleTreeEvent();
  });

  function startObserving() {
    observer.observe(document.documentElement, {
      subtree: true,
      childList: true,
      attributes: true,
      attributeOldValue: true,
    });
  }

  addEventListener('resize', () => updateOverlay());

  // ---------------------------------------------------------------------------------------------------------------
  // requests of the panel

  function pageInfo() {
    let waVersion = null;
    for (const node of document.querySelectorAll('script[src], link[href]')) {
      const url = node.getAttribute('src') || node.getAttribute('href') || '';
      const match = /webawesome[@/-](\d+\.\d+\.\d+)/i.exec(url);
      if (match) {
        waVersion = match[1];
        break;
      }
    }
    return {
      url: location.href,
      origin: location.origin,
      title: document.title,
      waVersion,
      blazorMetadata: !!document.querySelector(`[${META_COMPONENT}]`),
      themeClasses: (document.documentElement.getAttribute('class') || '').split(/\s+/).filter(name => name.startsWith('wa-')),
      viewport: { width: innerWidth, height: innerHeight },
      dpr: devicePixelRatio,
      storageKey: STORAGE_KEY,
    };
  }

  // whether each item is still pending, already done in the page's own values, or matches nothing
  function itemStatus() {
    const result = {};
    for (const item of session.items) {
      const elements = lastMatches.get(item.id) || [];
      if (!elements.length) {
        result[item.id] = { matches: 0, status: 'missing' };
        continue;
      }
      let checks = 0;
      let done = 0;
      for (const element of elements) {
        for (const [name, operation] of Object.entries(item.attributes || {})) {
          if (operation.remap) continue;
          checks++;
          const source = sourceAttribute(element, name);
          const wanted = desiredValue(operation, source);
          if (source === wanted) done++;
        }
        const classes = item.classes || {};
        const own = sourceClassList(element);
        for (const name of classes.add || []) {
          checks++;
          if (own.includes(name)) done++;
        }
        for (const name of classes.remove || []) {
          checks++;
          if (!own.includes(name)) done++;
        }
      }
      result[item.id] = {
        matches: elements.length,
        status: checks === 0 ? 'unchecked' : done === checks ? 'applied' : 'pending',
      };
    }
    return result;
  }

  // the scroll position before the first capture of an export, restored by page.endCapture
  let captureScroll = null;

  async function prepareCapture(params) {
    if (!captureScroll) captureScroll = { x: scrollX, y: scrollY };
    captureMarkers = params.markers !== false;
    highlightElement = null;
    let rect = null;
    if (params.itemId) {
      const elements = lastMatches.get(params.itemId) || [];
      const element = elements[0];
      if (element) {
        element.scrollIntoView({ block: 'center', inline: 'nearest' });
        rect = element.getBoundingClientRect();
      }
    }
    apply();
    // let the page and the overlay paint
    await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    if (rect && params.itemId) {
      const element = (lastMatches.get(params.itemId) || [])[0];
      rect = element.getBoundingClientRect();
    }
    return {
      rect: rect && {
        x: Math.max(0, rect.left - CAPTURE_PADDING - 12),
        y: Math.max(0, rect.top - CAPTURE_PADDING - 12),
        width: Math.min(innerWidth, rect.width + 2 * CAPTURE_PADDING + 24),
        height: Math.min(innerHeight, rect.height + 2 * CAPTURE_PADDING + 24),
      },
      dpr: devicePixelRatio,
      viewport: { width: innerWidth, height: innerHeight },
    };
  }

  function requireElement(params) {
    const element = elementOf(params.id);
    if (!element) throw new Error('element-gone');
    return element;
  }

  const handlers = {
    'page.info': () => pageInfo(),
    'page.tree': () => buildTree(),
    'page.details': params => detailsOf(requireElement(params)),
    'page.highlight': params => {
      highlightElement = params.id ? elementOf(params.id) : null;
      updateOverlay();
      return true;
    },
    'page.reveal': params => {
      let element = params.id ? elementOf(params.id) : null;
      if (!element && params.itemId) element = (lastMatches.get(params.itemId) || [])[0] || null;
      if (!element) return null;
      element.scrollIntoView({ block: 'center', inline: 'nearest', behavior: 'smooth' });
      flash(element);
      return idOf(element);
    },
    'page.pick': params => {
      setPickMode(!!params.enabled);
      return pickMode;
    },
    'page.markForInspect': params => {
      for (const marked of document.querySelectorAll(`[${INSPECT_ATTRIBUTE}]`)) marked.removeAttribute(INSPECT_ATTRIBUTE);
      requireElement(params).setAttribute(INSPECT_ATTRIBUTE, '');
      return true;
    },
    'page.itemStatus': () => itemStatus(),
    'page.itemMatches': params => (lastMatches.get(params.itemId) || []).map(idOf),
    // test hook of the browser tests (tools\e2e\inspector): the item number of each numbered outline drawn last; the
    // overlay's shadow root is closed, so a test cannot count the outlines itself
    'page.drawnMarkers': () => drawnMarkers,
    'page.prepareCapture': params => prepareCapture(params),
    'page.endCapture': () => {
      if (captureScroll) scrollTo(captureScroll.x, captureScroll.y);
      captureScroll = null;
      captureMarkers = null;
      updateOverlay();
      return true;
    },
    'page.apply': () => {
      apply();
      return true;
    },
    'page.panelClosed': () => {
      setPickMode(false);
      highlightElement = null;
      updateOverlay();
      return true;
    },
  };

  chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
    if (!message || !message.method) return false;
    const handler = handlers[message.method];
    if (!handler) {
      sendResponse({ error: `unknown method ${message.method}` });
      return false;
    }
    Promise.resolve()
      .then(() => handler(message.params || {}))
      .then(result => sendResponse({ result }), error => sendResponse({ error: String(error && error.message || error) }));
    return true;
  });

  function sendEvent(event, data) {
    try {
      chrome.runtime.sendMessage({ event, data }).catch(() => {});
    } catch {
      // the extension was reloaded; this content script is orphaned
    }
  }

  // ---------------------------------------------------------------------------------------------------------------
  // start

  startObserving();
  loadSession();
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => apply());
  // an open panel reloads its view when the page loads or reloads
  sendEvent('ready', { url: location.href });
})();
