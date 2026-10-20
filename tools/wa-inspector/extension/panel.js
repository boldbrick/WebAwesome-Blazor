// Panel of the WA Inspector. One page for three hosts:
// - mode=devtools: the "Web Awesome" DevTools panel (devtools.js), inspecting chrome.devtools.inspectedWindow;
// - mode=side: the browser side panel, inspecting the active tab of its window;
// - tabId=<n>: a normal tab (or window) that inspects tab <n>.
//
// The review session of the page's origin lives in chrome.storage.local under "wai:<origin>". This panel writes it;
// the content script in the page applies every change at once. Requests to the page go through background.js.

const params = new URLSearchParams(location.search);
const mode = params.get('tabId') ? 'tab' : (params.get('mode') || 'devtools');

// the colors of the change-list items, in order; the names go into the export
const ITEM_COLORS = [
  ['#e6194b', 'red'], ['#3cb44b', 'green'], ['#4363d8', 'blue'], ['#f58231', 'orange'], ['#911eb4', 'purple'],
  ['#0f9fb4', 'cyan'], ['#f032e6', 'magenta'], ['#808000', 'olive'], ['#9a6324', 'brown'], ['#000075', 'navy'],
  ['#469990', 'teal'], ['#800000', 'maroon'],
];

const SCOPES = [
  ['placement', 'This placement', 'all instances that the same source position renders'],
  ['site', 'Same source site', 'the same markup line wherever its component is used (Blazor metadata only)'],
  ['tag', 'All of this tag', 'every element with this tag on every page'],
  ['instance', 'This instance', 'only this element (matched again by text and position after a reload)'],
];

// the null value of an attribute in observed values and remaps
const ABSENT = '∅';

// a metadata default that is an expression, not a literal (the generator drops null and undefined)
const COMPUTED_DEFAULT_PATTERN = /[()]/;

const REQUEST_TIMEOUT_MS = 15000;
const RECONNECT_DELAY_MS = 500;
const TOAST_MS = 3000;
const EXPORT_FOLDER = 'wa-review';

const state = {
  tabId: null,
  port: null,
  requests: new Map(),
  nextRequestId: 1,
  metadata: null,
  info: null,
  session: null,
  tree: null,
  collapsed: new Set(),
  selectedId: null,
  details: null,
  scope: 'placement',
  status: {},
  tab: 'elements',
  treeFilter: '',
  themeFilter: '',
  error: null,
};

// -----------------------------------------------------------------------------------------------------------------
// small DOM helper

function h(tag, props, ...children) {
  const element = document.createElement(tag);
  for (const [name, value] of Object.entries(props || {})) {
    if (value === null || value === undefined || value === false) continue;
    if (name === 'class') element.className = value;
    else if (name === 'style' && typeof value === 'object') {
      for (const [property, propertyValue] of Object.entries(value)) {
        if (property.startsWith('--')) element.style.setProperty(property, propertyValue);
        else element.style[property] = propertyValue;
      }
    }
    else if (name.startsWith('on')) element.addEventListener(name.slice(2), value);
    else if (name === 'dataset') Object.assign(element.dataset, value);
    else if (name in element && typeof value !== 'string') element[name] = value;
    else element.setAttribute(name, value === true ? '' : value);
  }
  for (const child of children.flat(Infinity)) {
    if (child === null || child === undefined || child === false) continue;
    element.append(child instanceof Node ? child : document.createTextNode(String(child)));
  }
  return element;
}

const $ = id => document.getElementById(id);

function toast(text) {
  const node = h('div', { class: 'toast' }, text);
  document.body.append(node);
  setTimeout(() => node.remove(), TOAST_MS);
}

// -----------------------------------------------------------------------------------------------------------------
// connection to the page

function connect() {
  state.port = chrome.runtime.connect({ name: `panel:${state.tabId}` });
  state.port.onMessage.addListener(onPortMessage);
  state.port.onDisconnect.addListener(() => {
    state.port = null;
    for (const { reject } of state.requests.values()) reject(new Error('disconnected'));
    state.requests.clear();
    // the service worker restarted: connect again
    setTimeout(() => { if (!state.port && state.tabId !== null) connect(); }, RECONNECT_DELAY_MS);
  });
}

function onPortMessage(message) {
  if (message.event) {
    onPageEvent(message.event, message.data || {});
    return;
  }
  const request = state.requests.get(message.id);
  if (!request) return;
  state.requests.delete(message.id);
  clearTimeout(request.timer);
  if ('error' in message) request.reject(new Error(message.error));
  else request.resolve(message.result);
}

function request(method, requestParams) {
  if (!state.port) connect();
  const id = state.nextRequestId++;
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => {
      state.requests.delete(id);
      reject(new Error(`timeout: ${method}`));
    }, REQUEST_TIMEOUT_MS);
    state.requests.set(id, { resolve, reject, timer });
    state.port.postMessage({ id, method, params: requestParams || {} });
  });
}

function onPageEvent(event, data) {
  switch (event) {
    case 'ready':
      // the page loaded or reloaded
      setTimeout(() => refreshAll(), 200);
      break;
    case 'treeChanged':
      refreshTree();
      break;
    case 'sessionApplied':
      refreshTree();
      refreshStatus();
      refreshDetails();
      break;
    case 'selected':
      if (data.fromDevTools && !$('follow').checked) return;
      selectElement(data.id);
      if (state.tab !== 'elements') showTab('elements');
      break;
    case 'pickEnded':
      $('pick').classList.remove('active');
      break;
  }
}

// -----------------------------------------------------------------------------------------------------------------
// session storage

function storageKey() {
  return state.info ? state.info.storageKey : null;
}

async function loadSession() {
  const key = storageKey();
  if (!key) return;
  const stored = await chrome.storage.local.get(key);
  state.session = Object.assign({ version: 1, items: [], theme: {}, markersVisible: true, nextNumber: 1 }, stored[key] || {});
}

async function updateSession(change) {
  const key = storageKey();
  if (!key) return;
  const session = structuredClone(state.session);
  change(session);
  state.session = session;
  await chrome.storage.local.set({ [key]: session });
}

chrome.storage.onChanged.addListener((changes, area) => {
  const key = storageKey();
  if (area !== 'local' || !key || !changes[key]) return;
  state.session = Object.assign({ version: 1, items: [], theme: {}, markersVisible: true, nextNumber: 1 }, changes[key].newValue || {});
  renderChangeCount();
  if (state.tab === 'changes') renderChanges();
  if (state.tab === 'theme') renderTheme();
  // the content script applies the change first; then read the element again
  setTimeout(() => refreshDetails(), 60);
});

// -----------------------------------------------------------------------------------------------------------------
// refresh

async function refreshAll() {
  try {
    state.info = await request('page.info');
    state.error = null;
  } catch (error) {
    state.info = null;
    state.error = error.message === 'no-content-script'
      ? 'The inspector is not active on this page. Reload the page (the extension loads with the page).'
      : error.message;
    renderStatus();
    $('tree').replaceChildren(h('div', { class: 'empty' }, state.error));
    $('details').replaceChildren();
    return;
  }
  await loadSession();
  $('markers').checked = state.session.markersVisible !== false;
  renderStatus();
  renderChangeCount();
  await refreshTree();
  await refreshStatus();
  if (state.selectedId) await refreshDetails();
  else renderDetails();
  if (state.tab === 'changes') renderChanges();
  if (state.tab === 'theme') renderTheme();
}

let treeRefreshPending = false;
let treeRefreshAgain = false;

async function refreshTree() {
  if (treeRefreshPending) {
    // the page changed while the tree was read: read it once more when that request ends
    treeRefreshAgain = true;
    return;
  }
  treeRefreshPending = true;
  try {
    do {
      treeRefreshAgain = false;
      try {
        state.tree = await request('page.tree');
        renderTree();
      } catch {
        // the page navigated; the "ready" event refreshes again
      }
    } while (treeRefreshAgain);
  } finally {
    treeRefreshPending = false;
  }
}

async function refreshDetails() {
  if (!state.selectedId) return;
  try {
    state.details = await request('page.details', { id: state.selectedId });
  } catch {
    state.details = null;
    state.selectedId = null;
  }
  renderDetails();
}

async function refreshStatus() {
  try {
    state.status = await request('page.itemStatus');
  } catch {
    state.status = {};
  }
  if (state.tab === 'changes') renderChanges();
}

async function selectElement(id) {
  state.selectedId = id;
  await refreshDetails();
  renderTree();
  const row = document.querySelector(`.node-row[data-id="${id}"]`);
  if (row) row.scrollIntoView({ block: 'nearest' });
}

// -----------------------------------------------------------------------------------------------------------------
// status line

function renderStatus() {
  const status = $('status');
  if (!state.info) {
    status.replaceChildren(h('span', { class: 'error' }, state.error || 'Not connected'));
    return;
  }
  const parts = [state.info.title || state.info.url];
  const metadataVersion = state.metadata ? state.metadata.version : '?';
  if (state.info.waVersion && state.info.waVersion !== metadataVersion) {
    parts.push(h('span', { class: 'warn' }, ` · Web Awesome ${state.info.waVersion} (forms from ${metadataVersion})`));
  } else {
    parts.push(` · Web Awesome ${state.info.waVersion || metadataVersion}`);
  }
  parts.push(state.info.blazorMetadata
    ? ' · Blazor placement metadata: on'
    : h('span', { class: 'muted' }, ' · no Blazor placement metadata (structural matching)'));
  status.replaceChildren(...parts);
}

function renderChangeCount() {
  $('change-count').textContent = state.session && state.session.items.length ? String(state.session.items.length) : '';
}

// -----------------------------------------------------------------------------------------------------------------
// tree

function itemById(id) {
  return state.session ? state.session.items.find(item => item.id === id) : null;
}

function nodeMatches(node, filter) {
  const text = [node.tag, node.component, node.label, ...(node.classes || []), ...Object.values(node.attributes || {})]
    .filter(Boolean).join(' ').toLowerCase();
  return text.includes(filter);
}

function renderTree() {
  const container = $('tree');
  if (!state.tree) {
    container.replaceChildren(h('div', { class: 'empty' }, 'Reading the page…'));
    return;
  }
  const filter = state.treeFilter.trim().toLowerCase();
  const rows = [];

  // a node is shown when it or a descendant matches the filter
  const visible = new Map();
  const mark = node => {
    let shown = !filter || nodeMatches(node, filter);
    for (const child of node.children) shown = mark(child) || shown;
    visible.set(node, shown);
    return shown;
  };
  state.tree.root.children.forEach(mark);

  const walk = (node, depth) => {
    if (!visible.get(node)) return;
    const hasChildren = node.children.length > 0;
    const collapsed = !filter && state.collapsed.has(node.id);
    const attributes = Object.entries(node.attributes || {}).map(([name, value]) =>
      h('span', { class: 'node-attr' }, ` ${name}=`, h('b', null, `"${value}"`)));
    const badges = (node.items || []).map(itemId => {
      const item = itemById(itemId);
      return item ? h('span', { class: 'item-badge', style: { background: item.color } }, item.number) : null;
    });

    rows.push(h('div', {
      class: `node-row ${node.isWa ? 'node-wa' : 'node-plain'} ${node.id === state.selectedId ? 'selected' : ''}`,
      dataset: { id: node.id },
      style: { paddingLeft: `${4 + depth * 14}px` },
      onmouseenter: () => request('page.highlight', { id: node.id }).catch(() => {}),
      onclick: () => selectElement(node.id),
    },
    h('span', {
      class: 'twisty',
      onclick: event => {
        event.stopPropagation();
        if (state.collapsed.has(node.id)) state.collapsed.delete(node.id);
        else state.collapsed.add(node.id);
        renderTree();
      },
    }, hasChildren ? (collapsed ? '▸' : '▾') : ''),
    h('span', { class: 'node-tag' }, `<${node.tag}`),
    node.classes.length ? h('span', { class: 'node-class' }, ` .${node.classes.join('.')}`) : null,
    attributes,
    h('span', { class: 'node-tag' }, '>'),
    node.label ? h('span', { class: 'node-label' }, node.label) : null,
    node.count > 1 ? h('span', { class: 'node-count', title: 'instances of this placement' }, `×${node.count}`) : null,
    badges));

    if (!collapsed) node.children.forEach(child => walk(child, depth + 1));
  };
  state.tree.root.children.forEach(node => walk(node, 0));

  if (!rows.length) rows.push(h('div', { class: 'empty' }, filter ? 'No element matches the filter.' : 'No wa- elements and no utility classes on this page.'));
  if (state.tree.truncated) rows.push(h('div', { class: 'empty' }, `The tree shows the first ${state.tree.count} nodes only.`));
  container.replaceChildren(...rows);
}

// -----------------------------------------------------------------------------------------------------------------
// details

// the identity of a target within a scope: two edits of the same element in the same scope go to one item, also when
// the page has not applied the first edit yet
function targetKey(target, scope) {
  const placement = target.placement || `s:${target.structuralKey}`;
  switch (scope) {
    case 'site': return target.site || placement;
    case 'tag': return target.tag;
    case 'instance': return `${placement}#${target.ordinal}`;
    default: return placement;
  }
}

// the item of the current scope for the selected element, if any
function currentItem(session) {
  const details = state.details;
  const source = session || state.session;
  if (!details || !source) return null;
  const key = targetKey(details.locator, state.scope);
  const byKey = source.items.filter(item => item.scope === state.scope && item.target && targetKey(item.target, state.scope) === key);
  if (byKey.length) return byKey[byKey.length - 1];
  const matching = details.items.map(id => source.items.find(item => item.id === id)).filter(item => item && item.scope === state.scope);
  return matching.length ? matching[matching.length - 1] : null;
}

function newItem(session) {
  const details = state.details;
  const number = session.nextNumber || session.items.length + 1;
  const [color, colorName] = ITEM_COLORS[(number - 1) % ITEM_COLORS.length];
  const item = {
    id: `i${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`,
    number,
    color,
    colorName,
    scope: state.scope,
    target: details.locator,
    matchCount: scopeCount(state.scope),
    attributes: {},
    cssProperties: {},
    partStyles: {},
    classes: { add: [], remove: [] },
    inlineStyle: '',
    comment: '',
    createdAt: new Date().toISOString(),
  };
  session.nextNumber = number + 1;
  session.items.push(item);
  return item;
}

// changes the current item, creating it on the first change
async function editItem(change) {
  await updateSession(session => {
    let item = currentItem(session);
    if (!item) item = newItem(session);
    change(item);
    item.updatedAt = new Date().toISOString();
  });
}

function scopeCount(scope) {
  const details = state.details;
  if (!details) return 0;
  switch (scope) {
    case 'placement': return details.counts.placement;
    case 'site': return details.counts.site || details.counts.placement;
    case 'tag': return details.counts.tag || 0;
    default: return 1;
  }
}

function scopeAvailable(scope) {
  const details = state.details;
  if (scope === 'site') return !!(details.meta && details.meta.site);
  if (scope === 'tag') return details.isWa;
  return true;
}

function renderDetails() {
  const container = $('details');
  const details = state.details;
  if (!details) {
    container.replaceChildren(h('div', { class: 'empty' },
      'Select an element in the tree, or use Pick and click an element on the page. Any element can take a comment; wa- elements also get attribute and style forms.'));
    return;
  }
  if (!scopeAvailable(state.scope)) state.scope = 'placement';

  const item = currentItem();
  const elementMetadata = state.metadata && state.metadata.elements[details.tag];
  const parts = [];

  parts.push(h('h2', null, `<${details.tag}>`, elementMetadata && elementMetadata.pro ? h('span', { class: 'driven' }, 'Pro') : null));
  if (details.label) parts.push(h('div', { class: 'muted' }, details.label));
  if (elementMetadata && elementMetadata.summary) parts.push(h('div', { class: 'muted' }, elementMetadata.summary));

  if (details.ancestors.length) {
    parts.push(h('div', { class: 'breadcrumbs' }, details.ancestors.map(ancestor =>
      h('span', { class: 'crumb', title: ancestor.label || '', onclick: () => selectElement(ancestor.id) }, ancestor.tag))));
  }

  if (details.meta) {
    parts.push(h('div', { class: 'meta-line mono' }, `Blazor: ${details.meta.component}`));
    parts.push(h('div', { class: 'meta-line mono muted' }, details.meta.chain));
  } else if (details.owner) {
    parts.push(h('div', { class: 'meta-line mono' }, 'Inside Blazor ',
      h('span', { class: 'crumb', onclick: () => selectElement(details.owner.id) }, details.owner.component)));
    parts.push(h('div', { class: 'meta-line mono muted' }, details.owner.chain));
  }

  parts.push(h('div', { class: 'inline' },
    h('button', { class: 'tool', onclick: () => request('page.reveal', { id: details.id }) }, 'Reveal'),
    mode === 'devtools' ? h('button', { class: 'tool', onclick: () => inspectInElements(details.id) }, 'Inspect in Elements') : null,
    h('button', { class: 'tool', onclick: () => editItem(() => {}), title: 'Add a numbered marker without a change' }, item ? `Item #${item.number}` : 'Add marker')));

  // scope
  parts.push(h('h3', null, 'Apply to'));
  parts.push(h('div', { class: 'scopes' }, SCOPES.map(([value, label, title]) => {
    const available = scopeAvailable(value);
    return h('label', { class: available ? '' : 'disabled', title },
      h('input', {
        type: 'radio', name: 'scope', value, checked: state.scope === value, disabled: !available,
        onchange: () => { state.scope = value; renderDetails(); },
      }),
      `${label}${available && value !== 'instance' ? ` (${scopeCount(value)})` : ''}`);
  })));
  if (item) {
    parts.push(h('div', { class: 'editing' },
      h('span', { class: 'item-badge', style: { background: item.color } }, item.number),
      `Changes go to item #${item.number} (${item.scope}).`,
      h('button', { class: 'tool danger', onclick: () => deleteItem(item.id) }, 'Delete item')));
  }

  if (details.isWa && elementMetadata) {
    parts.push(renderAttributes(details, elementMetadata, item));
    if (elementMetadata.cssProperties.length) parts.push(renderCssProperties(elementMetadata, item));
    if (elementMetadata.cssParts.length) parts.push(renderParts(elementMetadata, item));
    if (elementMetadata.slots.length) {
      parts.push(h('h3', null, 'Slots'), h('div', { class: 'muted mono' }, elementMetadata.slots.map(slot => slot.name || '(default)').join(', ')));
    }
  } else if (details.isWa) {
    parts.push(h('div', { class: 'muted' }, `No metadata for <${details.tag}> in Web Awesome ${state.metadata ? state.metadata.version : '?'}.`));
  }

  parts.push(renderClasses(details, item));
  parts.push(renderInlineStyle(item));
  parts.push(renderComment(item));

  parts.push(h('h3', null, 'Locator'));
  parts.push(h('div', { class: 'meta-line mono muted' }, details.locator.path));

  container.replaceChildren(...parts);
}

function inspectInElements(id) {
  request('page.markForInspect', { id }).then(() => {
    chrome.devtools.inspectedWindow.eval("inspect(document.querySelector('[data-wai-inspect]'))");
  }).catch(() => {});
}

function attributeLabel(name, changed, description) {
  return h('span', { class: `name ${changed ? 'changed' : ''}`, title: description || '' }, name);
}

function distinctValues(observed) {
  return observed ? Object.keys(observed) : [];
}

// the element default of an attribute in its attribute form, or null when the metadata has no default or the default
// is computed when the element is set up (for example "new Date()")
function literalDefault(attribute) {
  if (typeof attribute.default !== 'string' || COMPUTED_DEFAULT_PATTERN.test(attribute.default)) return null;
  return attribute.default;
}

function renderAttributes(details, elementMetadata, item) {
  const rows = [];
  for (const attribute of elementMetadata.attributes) {
    const name = attribute.name;
    const operation = item && item.attributes[name];
    const current = name in details.attributes ? details.attributes[name] : null;
    const source = name in details.sourceAttributes ? details.sourceAttributes[name] : current;
    const values = distinctValues(details.observed[name]);
    const dataDriven = values.length > 1;

    // the field kind and the element default go with the operation: the content script writes the default in place
    // of a removal of a non-boolean attribute (Lit sets the property to null when the attribute goes)
    const set = op => editItem(target => {
      target.attributes[name] = Object.assign(op, {
        from: source,
        dataDriven,
        observed: details.observed[name] || null,
        kind: attribute.kind,
        elementDefault: literalDefault(attribute),
      });
    });
    const reset = h('button', {
      class: 'reset', title: 'Remove this change', hidden: !operation,
      onclick: () => editItem(target => { delete target.attributes[name]; }),
    }, '×');

    let input;
    if (dataDriven && attribute.kind !== 'boolean') {
      input = renderRemap(attribute, values, operation, set, details.observed[name]);
    } else if (attribute.kind === 'boolean') {
      input = h('input', {
        type: 'checkbox', checked: current !== null,
        onchange: event => set(event.target.checked ? { value: '' } : { remove: true }),
      });
    } else if (attribute.kind === 'enum' && !attribute.custom) {
      input = h('select', { onchange: event => set(event.target.value === ABSENT ? { remove: true } : { value: event.target.value }) },
        h('option', { value: ABSENT, selected: current === null }, attribute.default ? `(not set: ${attribute.default})` : '(not set)'),
        attribute.options.map(option => h('option', { value: option, selected: current === option }, option)),
        current !== null && !attribute.options.includes(current) ? h('option', { value: current, selected: true }, current) : null);
    } else {
      const listId = attribute.kind === 'enum' ? `list-${name}` : null;
      input = h('span', { class: 'inline' },
        h('input', {
          type: attribute.kind === 'number' ? 'number' : 'text',
          value: current === null ? '' : current,
          placeholder: attribute.default !== undefined ? String(attribute.default) : '',
          list: listId,
          onchange: event => set(event.target.value === '' ? { remove: true } : { value: event.target.value }),
        }),
        listId ? h('datalist', { id: listId }, attribute.options.map(option => h('option', { value: option }))) : null);
    }

    rows.push(attributeLabel(name, !!operation, attribute.description), input, reset);
    const hints = [];
    if (operation && source !== (current === null ? null : current)) hints.push(`page value: ${source === null ? '(not set)' : source}`);
    if (dataDriven) hints.push(`data-driven: ${values.map(value => `${value}×${details.observed[name][value]}`).join(', ')}`);
    if (hints.length) rows.push(h('div', { class: 'hint' }, hints.join(' · ')));
  }
  return h('div', null, h('h3', null, 'Attributes'), h('div', { class: 'form' }, rows));
}

// a data-driven attribute: map each value the page uses to a new one, instead of one fixed value
function renderRemap(attribute, values, operation, set, observed) {
  const remap = (operation && operation.remap) || {};
  const options = attribute.kind === 'enum' ? attribute.options : null;
  const rows = values.map(value => {
    const change = event => {
      const next = Object.assign({}, remap);
      if (event.target.value === '' || event.target.value === value) delete next[value];
      else next[value] = event.target.value;
      set({ remap: next });
    };
    const mapped = value in remap ? remap[value] : '';
    const input = options
      ? h('select', { onchange: change },
        h('option', { value: '', selected: mapped === '' }, '(keep)'),
        h('option', { value: ABSENT, selected: mapped === ABSENT }, '(not set)'),
        options.map(option => h('option', { value: option, selected: mapped === option }, option)))
      : h('input', { type: attribute.kind === 'number' ? 'number' : 'text', value: mapped, placeholder: '(keep)', onchange: change });
    return [h('span', { class: 'mono' }, value), h('span', { class: 'muted' }, `×${observed[value]} →`), input];
  });
  return h('div', null, h('span', { class: 'driven', title: 'The page sets different values here; map each value' }, 'remap'), h('div', { class: 'remap' }, rows));
}

function renderCssProperties(elementMetadata, item) {
  const rows = [];
  for (const property of elementMetadata.cssProperties) {
    const value = item && item.cssProperties[property.name];
    rows.push(attributeLabel(property.name, !!value, property.description),
      h('input', {
        type: 'text', value: value || '', placeholder: property.default || '',
        onchange: event => editItem(target => {
          if (event.target.value === '') delete target.cssProperties[property.name];
          else target.cssProperties[property.name] = event.target.value;
        }),
      }),
      h('button', {
        class: 'reset', hidden: !value, title: 'Remove this change',
        onclick: () => editItem(target => { delete target.cssProperties[property.name]; }),
      }, '×'));
  }
  return h('div', null, h('h3', null, 'CSS custom properties'), h('div', { class: 'form' }, rows));
}

function renderParts(elementMetadata, item) {
  const parts = (item && item.partStyles) || {};
  const select = h('select', null, elementMetadata.cssParts.map(part => h('option', { value: part.name, title: part.description || '' }, part.name)));
  const area = h('textarea', { placeholder: 'CSS declarations for the part, for example: padding: 0; gap: 4px' });
  const sync = () => { area.value = parts[select.value] || ''; };
  select.addEventListener('change', sync);
  const firstStyled = Object.keys(parts).find(name => parts[name]);
  if (firstStyled) select.value = firstStyled;
  sync();
  area.addEventListener('change', () => editItem(target => {
    if (area.value.trim() === '') delete target.partStyles[select.value];
    else target.partStyles[select.value] = area.value;
  }));
  const styled = Object.keys(parts).filter(name => parts[name]);
  return h('div', null,
    h('h3', null, 'Part styles', styled.length ? h('span', { class: 'muted' }, ` (${styled.join(', ')})`) : null),
    h('div', { class: 'inline' }, select), area);
}

function renderClasses(details, item) {
  const add = new Set((item && item.classes.add) || []);
  const remove = new Set((item && item.classes.remove) || []);
  const chips = [];
  for (const name of details.sourceClasses) {
    const removed = remove.has(name);
    chips.push(h('span', { class: `chip ${name.startsWith('wa-') ? 'utility' : ''} ${removed ? 'removed' : ''}` }, name,
      h('button', {
        title: removed ? 'Keep this class' : 'Remove this class',
        onclick: () => editItem(target => {
          const list = new Set(target.classes.remove);
          if (removed) list.delete(name);
          else list.add(name);
          target.classes.remove = [...list];
        }),
      }, removed ? '↺' : '×')));
  }
  for (const name of add) {
    chips.push(h('span', { class: 'chip added utility' }, name,
      h('button', {
        title: 'Do not add this class',
        onclick: () => editItem(target => { target.classes.add = target.classes.add.filter(other => other !== name); }),
      }, '×')));
  }

  const utilities = state.metadata ? Object.values(state.metadata.utilityClasses).flat() : [];
  const input = h('input', { type: 'text', list: 'utility-classes', placeholder: 'Add a class, for example wa-gap-l' });
  const addClass = () => {
    const names = input.value.split(/\s+/).filter(Boolean);
    if (!names.length) return;
    editItem(target => {
      const list = new Set(target.classes.add);
      names.forEach(name => list.add(name));
      target.classes.add = [...list];
      target.classes.remove = target.classes.remove.filter(other => !list.has(other));
    });
  };
  input.addEventListener('keydown', event => { if (event.key === 'Enter') addClass(); });

  return h('div', null,
    h('h3', null, 'Classes'),
    h('div', { class: 'chips' }, chips.length ? chips : h('span', { class: 'muted' }, 'No classes.')),
    h('div', { class: 'inline' }, input, h('button', { class: 'tool', onclick: addClass }, 'Add')),
    h('datalist', { id: 'utility-classes' }, utilities.map(name => h('option', { value: name }))));
}

function renderInlineStyle(item) {
  const area = h('textarea', { placeholder: 'CSS declarations for the element, for example: margin-top: 0; max-width: 40rem' });
  area.value = (item && item.inlineStyle) || '';
  area.addEventListener('change', () => editItem(target => { target.inlineStyle = area.value; }));
  return h('div', null, h('h3', null, 'Style'), area);
}

function renderComment(item) {
  const area = h('textarea', { placeholder: 'What should change here, and why? The coding agent reads this as the intent.' });
  area.value = (item && item.comment) || '';
  area.addEventListener('change', () => editItem(target => { target.comment = area.value; }));
  return h('div', null, h('h3', null, 'Comment'), area);
}

async function deleteItem(itemId) {
  await updateSession(session => { session.items = session.items.filter(item => item.id !== itemId); });
}

// -----------------------------------------------------------------------------------------------------------------
// change list

function describeTarget(item) {
  const target = item.target || {};
  const name = target.component ? `${target.component} <${target.tag}>` : `<${target.tag}>`;
  return `${name}${target.text ? ` “${target.text}”` : ''}`;
}

function changeLines(item) {
  const lines = [];
  for (const [name, operation] of Object.entries(item.attributes || {})) {
    if (operation.remap) {
      lines.push(`${name}: remap ${Object.entries(operation.remap).map(([from, to]) => `${from} → ${to}`).join(', ')}`);
    } else if (operation.remove) {
      lines.push(`${name}: remove (was ${operation.from === null ? 'not set' : `"${operation.from}"`})`);
    } else {
      lines.push(`${name}: ${operation.from === null || operation.from === undefined ? '(not set)' : `"${operation.from}"`} → "${operation.value}"${operation.dataDriven ? ' (data-driven)' : ''}`);
    }
  }
  for (const [name, value] of Object.entries(item.cssProperties || {})) lines.push(`${name}: ${value}`);
  for (const [part, css] of Object.entries(item.partStyles || {})) lines.push(`::part(${part}) { ${css} }`);
  const classes = item.classes || {};
  if (classes.add && classes.add.length) lines.push(`add classes: ${classes.add.join(' ')}`);
  if (classes.remove && classes.remove.length) lines.push(`remove classes: ${classes.remove.join(' ')}`);
  if (item.inlineStyle) lines.push(`style: ${item.inlineStyle}`);
  return lines;
}

function renderChanges() {
  const container = $('changes');
  const items = state.session ? state.session.items : [];
  if (!items.length) {
    container.replaceChildren(h('div', { class: 'empty' }, 'No changes yet. Select an element and change a value, add a class, or write a comment.'));
    return;
  }
  container.replaceChildren(...items.map(item => {
    const status = state.status[item.id] || { status: 'unchecked', matches: 0 };
    return h('div', { class: 'change-item', style: { '--item-color': item.color } },
      h('div', { class: 'change-head' },
        h('span', { class: 'item-badge', style: { background: item.color } }, item.number),
        h('span', { class: 'title' }, describeTarget(item)),
        h('span', { class: 'muted' }, item.scope),
        h('span', { class: `state ${status.status}`, title: 'pending: the page does not have the value yet; applied: the page has it on its own' },
          status.status === 'missing' ? 'not on this page' : `${status.status} · ${status.matches} match${status.matches === 1 ? '' : 'es'}`),
        h('span', { class: 'spacer' }),
        h('button', { class: 'tool', onclick: () => revealItem(item) }, 'Show'),
        h('button', { class: 'tool danger', onclick: () => deleteItem(item.id) }, 'Delete')),
      item.target && (item.target.chain || item.target.ownerChain)
        ? h('div', { class: 'mono muted' }, item.target.chain || `inside ${item.target.ownerChain}`) : null,
      h('ul', { class: 'change-list' }, changeLines(item).map(line => h('li', { class: 'mono' }, line))),
      item.comment ? h('div', null, '“', item.comment, '”') : null);
  }));
}

async function revealItem(item) {
  const id = await request('page.reveal', { itemId: item.id }).catch(() => null);
  if (!id) {
    toast('The item matches nothing on this page.');
    return;
  }
  state.scope = item.scope;
  showTab('elements');
  await selectElement(id);
}

// -----------------------------------------------------------------------------------------------------------------
// theme

function renderTheme() {
  const container = $('theme');
  if (!state.metadata || !state.session) {
    container.replaceChildren();
    return;
  }
  const filter = state.themeFilter.trim().toLowerCase();
  const groups = new Map();
  for (const token of state.metadata.themeTokens) {
    if (filter && !token.name.includes(filter) && !token.value.toLowerCase().includes(filter)) continue;
    if (!groups.has(token.group)) groups.set(token.group, []);
    groups.get(token.group).push(token);
  }
  const theme = state.session.theme || {};
  const setToken = (name, value) => updateSession(session => {
    session.theme = session.theme || {};
    if (value === '') delete session.theme[name];
    else session.theme[name] = value;
  });

  container.replaceChildren(...[...groups].map(([group, tokens]) => h('div', { class: 'token-group' },
    h('h3', null, group),
    tokens.map(token => {
      const value = theme[token.name] || '';
      const isColor = token.name.startsWith('--wa-color-');
      return h('div', { class: 'token-row' },
        h('span', { class: `name mono ${value ? 'changed' : ''}` }, token.name),
        h('span', { class: 'inline' },
          h('input', { type: 'text', value, placeholder: token.value, onchange: event => setToken(token.name, event.target.value.trim()) }),
          h('span', { class: 'default', title: token.value }, token.value)),
        isColor ? h('input', { type: 'color', value: /^#[0-9a-f]{6}$/i.test(value) ? value : '#000000', onchange: event => setToken(token.name, event.target.value) }) : h('span'),
        h('button', { class: 'reset', hidden: !value, onclick: () => setToken(token.name, '') }, '×'));
    }))));
  if (!groups.size) container.replaceChildren(h('div', { class: 'empty' }, 'No token matches the filter.'));
}

// -----------------------------------------------------------------------------------------------------------------
// export

function pad(number) {
  return String(number).padStart(2, '0');
}

function timestamp(date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}_${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
}

function tokenDefault(name) {
  const token = state.metadata && state.metadata.themeTokens.find(candidate => candidate.name === name);
  return token ? token.value : null;
}

/**
 * Builds the machine-readable change list.
 *
 * @param {object} info the page info
 * @param {object} status the item status by item id
 * @param {object} screenshots the screenshot file names: page, and by item id
 */
function buildExport(info, status, screenshots) {
  return {
    schema: 'wa-inspector/change-list@1',
    exportedAt: new Date().toISOString(),
    page: {
      url: info.url,
      title: info.title,
      origin: info.origin,
      waVersion: info.waVersion,
      formsVersion: state.metadata ? state.metadata.version : null,
      blazorMetadata: info.blazorMetadata,
      themeClasses: info.themeClasses,
      viewport: info.viewport,
      devicePixelRatio: info.dpr,
    },
    screenshot: screenshots.page || null,
    theme: Object.entries(state.session.theme || {}).map(([token, value]) => ({ token, from: tokenDefault(token), to: value })),
    items: state.session.items.map(item => ({
      number: item.number,
      color: item.colorName || item.color,
      scope: item.scope,
      matchCount: (status[item.id] && status[item.id].matches) || 0,
      matchCountAtCreation: item.matchCount,
      status: (status[item.id] && status[item.id].status) || 'unchecked',
      target: item.target,
      changes: [
        ...Object.entries(item.attributes || {}).map(([name, operation]) => ({
          kind: 'attribute',
          name,
          operation: operation.remap ? 'remap' : operation.remove ? 'remove' : 'set',
          value: operation.remap ? undefined : operation.remove ? null : operation.value,
          remap: operation.remap,
          from: operation.from,
          dataDriven: !!operation.dataDriven,
          observed: operation.observed || undefined,
        })),
        ...Object.entries(item.cssProperties || {}).map(([name, value]) => ({ kind: 'cssProperty', name, value })),
        ...Object.entries(item.partStyles || {}).map(([part, css]) => ({ kind: 'partStyle', part, css })),
        ...(item.classes && (item.classes.add.length || item.classes.remove.length)
          ? [{ kind: 'classes', add: item.classes.add, remove: item.classes.remove }] : []),
        ...(item.inlineStyle ? [{ kind: 'style', css: item.inlineStyle }] : []),
      ],
      comment: item.comment || null,
      screenshot: screenshots[item.id] || null,
      createdAt: item.createdAt,
    })),
  };
}

function inlineCode(text) {
  return `\`${String(text).replace(/`/g, "'")}\``;
}

function buildMarkdown(data) {
  const lines = [];
  lines.push(`# WA review: ${data.page.title || data.page.url}`, '');
  lines.push(`- URL: ${data.page.url}`);
  lines.push(`- Exported: ${data.exportedAt}`);
  lines.push(`- Web Awesome: ${data.page.waVersion || 'unknown'} (forms from ${data.page.formsVersion})`);
  lines.push(`- Blazor placement metadata: ${data.page.blazorMetadata ? 'yes' : 'no'}`);
  lines.push(`- Viewport: ${data.page.viewport.width}×${data.page.viewport.height} at ${data.page.devicePixelRatio}x`);
  if (data.screenshot) lines.push(`- Screenshot: ${data.screenshot} (the numbered outlines are the item numbers below)`);
  lines.push('- Machine-readable: changes.json', '');

  if (data.theme.length) {
    lines.push('## Theme tokens', '', '| Token | Default | New value |', '|---|---|---|');
    for (const token of data.theme) lines.push(`| ${inlineCode(token.token)} | ${token.from ? inlineCode(token.from) : ''} | ${inlineCode(token.to)} |`);
    lines.push('');
  }

  for (const item of data.items) {
    const target = item.target || {};
    lines.push(`## Item ${item.number} (${item.color}): <${target.tag}>${target.text ? ` “${target.text}”` : ''}`, '');
    lines.push(`- Scope: ${item.scope} (${item.matchCount} match${item.matchCount === 1 ? '' : 'es'} now, ${item.matchCountAtCreation} when created); status: ${item.status}`);
    if (target.component) lines.push(`- Blazor component: ${inlineCode(target.component)}, chain ${inlineCode(target.chain)}`);
    if (!target.component && target.ownerComponent) lines.push(`- Inside Blazor component: ${inlineCode(target.ownerComponent)}, chain ${inlineCode(target.ownerChain)}`);
    if (target.placement) lines.push(`- Placement ${inlineCode(target.placement)}, site ${inlineCode(target.site)}`);
    lines.push(`- Path: ${inlineCode(target.path)}`);
    if (target.classes && target.classes.length) lines.push(`- Classes: ${inlineCode(target.classes.join(' '))}`);
    if (target.scopeAttributes && target.scopeAttributes.length) lines.push(`- CSS isolation scope: ${inlineCode(target.scopeAttributes.join(' '))}`);
    if (target.url && target.url !== data.page.url) lines.push(`- Created on: ${target.url}`);
    if (item.screenshot) lines.push(`- Screenshot: ${item.screenshot}`);
    if (item.changes.length) {
      lines.push('', 'Changes:');
      for (const change of item.changes) {
        switch (change.kind) {
          case 'attribute':
            if (change.operation === 'remap') {
              lines.push(`- attribute ${inlineCode(change.name)}: data-driven, remap ${Object.entries(change.remap).map(([from, to]) => `${inlineCode(from)} → ${inlineCode(to)}`).join(', ')}`);
            } else if (change.operation === 'remove') {
              lines.push(`- attribute ${inlineCode(change.name)}: remove (page value ${change.from === null ? 'not set' : inlineCode(change.from)})`);
            } else {
              lines.push(`- attribute ${inlineCode(change.name)}: ${change.from === null || change.from === undefined ? '(not set)' : inlineCode(change.from)} → ${inlineCode(change.value)}${change.dataDriven ? ' (data-driven: the page sets several values here)' : ''}`);
            }
            if (change.observed && Object.keys(change.observed).length > 1) {
              lines.push(`  - values seen: ${Object.entries(change.observed).map(([value, count]) => `${inlineCode(value)}×${count}`).join(', ')}`);
            }
            break;
          case 'cssProperty':
            lines.push(`- CSS custom property ${inlineCode(change.name)}: ${inlineCode(change.value)}`);
            break;
          case 'partStyle':
            lines.push(`- part ${inlineCode(change.part)}: ${inlineCode(change.css)}`);
            break;
          case 'classes':
            if (change.add.length) lines.push(`- add classes: ${inlineCode(change.add.join(' '))}`);
            if (change.remove.length) lines.push(`- remove classes: ${inlineCode(change.remove.join(' '))}`);
            break;
          case 'style':
            lines.push(`- style: ${inlineCode(change.css)}`);
            break;
        }
      }
    }
    if (item.comment) {
      lines.push('', 'Comment:', '');
      for (const line of item.comment.split('\n')) lines.push(`> ${line}`);
    }
    lines.push('');
  }
  return lines.join('\n');
}

async function exportChangeList() {
  const button = $('export');
  button.disabled = true;
  try {
    const info = await request('page.info');
    const status = await request('page.itemStatus');
    const folder = `${EXPORT_FOLDER}/${timestamp(new Date())}`;
    const screenshots = {};
    const images = [];
    let hiddenBy = null;
    try {
      hiddenBy = await showInspectedTab();
      await request('page.prepareCapture', { markers: true });
      images.push(['page.png', await request('capture', {})]);
      screenshots.page = 'page.png';
      for (const item of state.session.items) {
        if (!status[item.id] || status[item.id].matches === 0) continue;
        const prepared = await request('page.prepareCapture', { itemId: item.id, markers: true });
        if (!prepared.rect) continue;
        const name = `item-${pad(item.number)}.png`;
        images.push([name, await request('capture', { crop: Object.assign({ dpr: prepared.dpr }, prepared.rect) })]);
        screenshots[item.id] = name;
      }
    } catch (error) {
      toast(`Screenshots failed: ${error.message}`);
    } finally {
      await request('page.endCapture').catch(() => {});
      if (hiddenBy) await chrome.tabs.update(hiddenBy, { active: true }).catch(() => {});
    }

    const data = buildExport(info, status, screenshots);
    await request('download', { filename: `${folder}/changes.json`, text: JSON.stringify(data, null, 2), mime: 'application/json' });
    await request('download', { filename: `${folder}/changes.md`, text: buildMarkdown(data), mime: 'text/markdown' });
    for (const [name, dataUrl] of images) await request('download', { filename: `${folder}/${name}`, dataUrl });
    toast(`Saved to Downloads/${folder}`);
    return { folder, data };
  } catch (error) {
    toast(`Export failed: ${error.message}`);
    return null;
  } finally {
    button.disabled = false;
  }
}

// a screenshot shows the visible tab of a window: a panel in a tab of the inspected tab's window hides the page, so
// show the inspected tab while the screenshots are taken; returns the id of the panel's tab to show again, or null
async function showInspectedTab() {
  if (mode !== 'tab') return null;
  const own = await chrome.tabs.getCurrent();
  const inspected = await chrome.tabs.get(state.tabId);
  if (!own || own.windowId !== inspected.windowId || inspected.active) return null;
  await chrome.tabs.update(state.tabId, { active: true });
  return own.id;
}

async function copyMarkdown() {
  const info = await request('page.info');
  const status = await request('page.itemStatus');
  const markdown = buildMarkdown(buildExport(info, status, {}));
  try {
    await navigator.clipboard.writeText(markdown);
  } catch {
    // DevTools panels may refuse the async clipboard; fall back to a selection copy
    const area = h('textarea', { style: { position: 'fixed', opacity: '0' } });
    area.value = markdown;
    document.body.append(area);
    area.select();
    document.execCommand('copy');
    area.remove();
  }
  toast('Change list copied as Markdown.');
}

// -----------------------------------------------------------------------------------------------------------------
// tabs and toolbar

function showTab(tab) {
  state.tab = tab;
  for (const button of document.querySelectorAll('.tab')) button.classList.toggle('selected', button.dataset.tab === tab);
  for (const page of document.querySelectorAll('.tab-page')) page.hidden = page.id !== `tab-${tab}`;
  if (tab === 'changes') refreshStatus().then(renderChanges);
  if (tab === 'theme') renderTheme();
}

function wireToolbar() {
  for (const button of document.querySelectorAll('.tab')) button.addEventListener('click', () => showTab(button.dataset.tab));
  $('pick').addEventListener('click', async () => {
    const enabled = !$('pick').classList.contains('active');
    $('pick').classList.toggle('active', enabled);
    await request('page.pick', { enabled }).catch(() => $('pick').classList.remove('active'));
  });
  $('refresh').addEventListener('click', () => refreshAll());
  $('markers').addEventListener('change', event => updateSession(session => { session.markersVisible = event.target.checked; }));
  $('export').addEventListener('click', () => exportChangeList());
  $('copy').addEventListener('click', () => copyMarkdown().catch(error => toast(`Copy failed: ${error.message}`)));
  $('clear-applied').addEventListener('click', () => updateSession(session => {
    session.items = session.items.filter(item => !state.status[item.id] || state.status[item.id].status !== 'applied');
  }));
  $('clear-all').addEventListener('click', () => {
    if (!confirm('Remove all items and theme changes of this site?')) return;
    updateSession(session => {
      session.items = [];
      session.theme = {};
      session.nextNumber = 1;
    });
  });
  $('tree-filter').addEventListener('input', event => { state.treeFilter = event.target.value; renderTree(); });
  $('theme-filter').addEventListener('input', event => { state.themeFilter = event.target.value; renderTheme(); });
  $('tree').addEventListener('mouseleave', () => request('page.highlight', { id: null }).catch(() => {}));
}

// -----------------------------------------------------------------------------------------------------------------
// start

async function resolveTabId() {
  if (mode === 'tab') return Number(params.get('tabId'));
  if (mode === 'devtools') return chrome.devtools.inspectedWindow.tabId;
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  return tab ? tab.id : null;
}

async function bindTab(tabId) {
  if (state.tabId === tabId) return;
  if (state.port) {
    const old = state.port;
    state.port = null;
    old.disconnect();
  }
  state.tabId = tabId;
  state.selectedId = null;
  state.details = null;
  state.tree = null;
  if (tabId !== null) connect();
  await refreshAll();
}

async function start() {
  document.body.classList.add(`mode-${mode}`);
  if (mode === 'devtools' && chrome.devtools.panels.themeName === 'dark') document.documentElement.classList.add('theme-dark');
  wireToolbar();
  showTab('elements');

  state.metadata = await (await fetch('metadata/wa-metadata.json')).json();

  if (mode === 'devtools') {
    chrome.devtools.panels.elements.onSelectionChanged.addListener(() => {
      if (!$('follow').checked) return;
      chrome.devtools.inspectedWindow.eval("$0 && $0.dispatchEvent(new CustomEvent('wai-inspect'))");
    });
  }
  if (mode === 'side') {
    // the side panel follows the active tab of its window
    chrome.tabs.onActivated.addListener(info => bindTab(info.tabId));
  }

  await bindTab(await resolveTabId());
}

// the test hooks of the browser tests (tools\e2e\inspector)
window.__waInspectorPanel = { state, request, refreshAll, selectElement, editItem, buildExport, buildMarkdown, exportChangeList, showTab };

start();
