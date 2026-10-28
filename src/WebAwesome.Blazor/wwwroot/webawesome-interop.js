/**
 * Web Awesome Blazor JavaScript Interop Module
 * Provides JavaScript functionality for Web Awesome Blazor components
 */

import { findGlobalFunction, isFunctionDescriptor, resolveFunction } from './webawesome-functions.js';

/**
 * Sets a custom validation message on a Web Awesome form control element
 * @param {HTMLElement} element - The Web Awesome form control element
 * @param {string} message - The validation message to display, or empty string to clear
 */
export function setCustomValidity(element, message) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    // Check if element implements WebAwesomeFormControl interface by testing for setCustomValidity method
    if (typeof element.setCustomValidity !== 'function') {
        throw new Error(`Element ${tagName} does not implement WebAwesomeFormControl interface. Ensure Web Awesome library is properly loaded.`);
    }

    try {
        element.setCustomValidity(message || '');
    } catch (error) {
        throw new Error(`Failed to set custom validity on ${tagName}: ${error.message}`);
    }
}

/**
 * Invokes a method on a Web Awesome element once its custom element is defined, and does nothing before: an element
 * whose module is still loading has none of its methods yet, and its upgrade renders it from its current attributes
 * anyway. For refreshes a wrapper requests on every parameter change (wa-relative-time's update), which can run while
 * the autoloader is still importing the element's module.
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} methodName - The name of the method to invoke
 * @returns {boolean} Whether the method was invoked
 */
export function invokeMethodIfDefined(element, methodName) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!customElements.get(element.localName)) return false;

    callElementMethod(element, methodName, []);
    return true;
}

/**
 * Invokes a method on a Web Awesome element, once its custom element is defined. The autoloader imports the module of
 * an element only after it finds the element in the DOM, so a call soon after the first render (in OnAfterRenderAsync,
 * for example) can arrive while the element has none of its methods yet. Such a call waits for the definition, at most
 * elementDefinitionTimeoutMs, and then fails if the element is still not defined (a Pro element on the free CDN).
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} methodName - The name of the method to invoke
 * @param {Array} args - Arguments to pass to the method
 * @returns {Promise<any>} The result of the method call
 */
export async function invokeMethod(element, methodName, args) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!methodName) {
        throw new Error('Method name cannot be null or empty');
    }

    await whenElementDefined(element);
    return callElementMethod(element, methodName, args);
}

// the longest wait of a method call for the definition of its element; the autoloader imports the module of the
// element in this time, unless the module does not load at all
const elementDefinitionTimeoutMs = 10000;

// waits until the custom element of an element is defined, at most elementDefinitionTimeoutMs; an element that is not
// a custom element, or is defined already, does not wait (customElements.define upgrades the connected elements before
// whenDefined resolves)
async function whenElementDefined(element) {
    const tagName = element.localName;
    if (!tagName?.includes('-') || customElements.get(tagName)) return;

    let timer;
    try {
        await Promise.race([
            customElements.whenDefined(tagName),
            new Promise(resolve => { timer = setTimeout(resolve, elementDefinitionTimeoutMs); }),
        ]);
    } finally {
        clearTimeout(timer);
    }
}

function callElementMethod(element, methodName, args) {
    const tagName = element.tagName?.toLowerCase() || 'unknown';

    // Check if the method exists on the element
    if (typeof element[methodName] !== 'function') {
        if (tagName.includes('-') && !customElements.get(tagName)) {
            throw new Error(`Method '${methodName}' cannot be invoked: element ${tagName} is not defined after ${elementDefinitionTimeoutMs} ms. Ensure Web Awesome library is properly loaded and the component is available (Pro components need the Pro assets).`);
        }

        throw new Error(`Method '${methodName}' does not exist on element ${tagName} or is not a function. Ensure Web Awesome library is properly loaded.`);
    }

    try {
        // Apply the method with the provided arguments
        const result = element[methodName].apply(element, args || []);
        return result;
    } catch (error) {
        throw new Error(`Failed to invoke method '${methodName}' on ${tagName}: ${error.message}`);
    }
}

/**
 * Sets a property value on a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} propertyName - The name of the property to set
 * @param {any} value - The value to set
 */
export function setProperty(element, propertyName, value) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!propertyName) {
        throw new Error('Property name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    try {
        element[propertyName] = value;
    } catch (error) {
        throw new Error(`Failed to set property '${propertyName}' on ${tagName}: ${error.message}`);
    }
}

/**
 * Synchronizes a live property of a Web Awesome element with a value from .NET, assigning it only
 * when it differs from the current one. Used to push model changes into form controls whose value
 * attribute only maps to the default value (an unconditional assignment could move the caret or
 * trigger a needless update while the user is typing).
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} propertyName - The name of the live property to synchronize
 * @param {any} value - The value to assign
 */
export function syncProperty(element, propertyName, value) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!propertyName) {
        throw new Error('Property name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    try {
        if (element[propertyName] !== value) {
            element[propertyName] = value;
        }
    } catch (error) {
        throw new Error(`Failed to sync property '${propertyName}' on ${tagName}: ${error.message}`);
    }
}

/**
 * Gets a property value from a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} propertyName - The name of the property to get
 * @returns {any} The property value
 */
export function getProperty(element, propertyName) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!propertyName) {
        throw new Error('Property name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    try {
        return element[propertyName];
    } catch (error) {
        throw new Error(`Failed to get property '${propertyName}' from ${tagName}: ${error.message}`);
    }
}

/**
 * Fires the slotchange event of an element's default slot, so the element re-reads its light-DOM children in its own
 * slotchange handler. A text node always goes to the default slot, so appending one and removing it at once changes
 * the slot's assigned nodes twice (the event fires once, after this task), and leaves the DOM as Blazor rendered it.
 * wa-date-input forwards its day-YYYY-MM-DD children to its popup calendar only on its first update and on this
 * slotchange, which a day-slotted child, assigned to no default slot, never causes when it is added or removed later.
 * @param {HTMLElement} element - The Web Awesome element
 */
export function signalDefaultSlotChange(element) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const probe = document.createTextNode('');
    element.appendChild(probe);
    probe.remove();
}

/*
 * Icon library registry
 *
 * Web Awesome keeps its icon library registry, default icon family and kit code as module state inside its
 * dist chunks; the entry point (webawesome.loader.js) re-exports registerIconLibrary and friends. A separately
 * loaded module reaches that state only by importing the entry point from the same base URL the page loaded:
 * another copy or version is a second Web Awesome instance with its own registry, which the page's wa-icon
 * elements never read. The entry point is therefore taken from the page's own Web Awesome script tag, and
 * only when there is none from the loader URL configured in .NET.
 */

const webAwesomeEntryPointFileNames = ['webawesome.loader.js', 'webawesome.js', 'webawesome.ssr-loader.js'];
const iconResolverPlaceholder = /\{(name|family|variant)\}/g;
let webAwesomeModule = null;

// the icon library members that take a function (upstream IconLibrary): the number of arguments wa-icon passes before
// ctx. The resolver gets (name, family, variant, autoWidth) and returns a URL or a Promise of one; the mutator gets
// (svg, hostElement).
// NOTE checked against WA 3.14.0: the arities match components/icon/library.d.ts; recheck them on each upgrade (watch list)
const iconLibraryFunctions = {
    resolver: { arity: 4 },
    mutator: { arity: 2 },
};

/**
 * Registers an icon library with Web Awesome
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET, used when the page has no Web Awesome script tag
 * @param {string} name - Library name, referenced by wa-icon's library attribute
 * @param {{resolver?: string|null, resolverFunction?: object|null, mutator?: string|null, mutatorFunction?: object|null, spriteSheet?: boolean}} options -
 *        the resolver as a URL template with {name}, {family} and {variant} placeholders or as a WaJsFunction descriptor,
 *        the mutator as a WaJsFunction descriptor (or the obsolete global function path, e.g. "myApp.icons.mutate"),
 *        and the sprite sheet flag
 */
export async function registerIconLibrary(configuredEntryPointUrl, name, options) {
    if (!name) {
        throw new Error('Icon library name cannot be null or empty');
    }

    const resolverDescriptor = isFunctionDescriptor(options?.resolverFunction) ? options.resolverFunction : null;
    if (!options?.resolver && !resolverDescriptor) {
        throw new Error(`Icon library '${name}' requires a resolver URL template or a resolver function`);
    }

    const [webAwesome, resolver, mutator] = await Promise.all([
        importWebAwesome(configuredEntryPointUrl),
        resolverDescriptor ? resolveIconLibraryFunction(name, 'resolver', resolverDescriptor) : createIconResolver(options.resolver),
        resolveIconMutator(name, options),
    ]);

    // a library without a resolver draws no icon, so the registration fails (resolveFunction logged the reason)
    if (!resolver) {
        throw new Error(`Icon library '${name}': the resolver function does not resolve (see the console error)`);
    }

    webAwesome.registerIconLibrary(name, {
        resolver,
        mutator,
        spriteSheet: options.spriteSheet === true
    });
}

/**
 * Removes an icon library from Web Awesome's registry
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET
 * @param {string} name - Library name
 */
export async function unregisterIconLibrary(configuredEntryPointUrl, name) {
    const webAwesome = await importWebAwesome(configuredEntryPointUrl);
    webAwesome.unregisterIconLibrary(name);
}

/**
 * Sets the icon family used by wa-icon elements without a family attribute; rendered icons re-resolve
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET
 * @param {string} family - Icon family name, e.g. "classic" or "sharp"
 */
export async function setDefaultIconFamily(configuredEntryPointUrl, family) {
    const webAwesome = await importWebAwesome(configuredEntryPointUrl);
    webAwesome.setDefaultIconFamily(family);
}

/**
 * Gets the icon family used by wa-icon elements without a family attribute
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET
 * @returns {Promise<string>} The default icon family name
 */
export async function getDefaultIconFamily(configuredEntryPointUrl) {
    const webAwesome = await importWebAwesome(configuredEntryPointUrl);
    return webAwesome.getDefaultIconFamily();
}

/**
 * Sets the Font Awesome kit code that unlocks the Pro icons of the default library
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET
 * @param {string} kitCode - Font Awesome kit code
 */
export async function setKitCode(configuredEntryPointUrl, kitCode) {
    const webAwesome = await importWebAwesome(configuredEntryPointUrl);
    webAwesome.setKitCode(kitCode);

    // setKitCode alone does not refresh rendered icons; re-applying the default family re-resolves them
    webAwesome.setDefaultIconFamily(webAwesome.getDefaultIconFamily());
}

async function importWebAwesome(configuredEntryPointUrl) {
    if (!webAwesomeModule) {
        const url = findWebAwesomeEntryPoint(configuredEntryPointUrl);
        webAwesomeModule = import(url).catch(error => {
            webAwesomeModule = null;
            throw new Error(`Failed to import Web Awesome from '${url}': ${error.message}`);
        });
    }

    return webAwesomeModule;
}

function findWebAwesomeEntryPoint(configuredEntryPointUrl) {
    // the script element's src property is the resolved URL, i.e. the key of the page's module instance
    const script = Array.from(document.querySelectorAll('script[type="module"][src]'))
        .find(s => webAwesomeEntryPointFileNames.some(fileName => new URL(s.src).pathname.endsWith(`/${fileName}`)));
    if (script) {
        return script.src;
    }

    if (configuredEntryPointUrl) {
        return new URL(configuredEntryPointUrl, document.baseURI).href;
    }

    throw new Error('Web Awesome is not loaded: no webawesome.loader.js script tag found and no loader URL configured');
}

function createIconResolver(template) {
    return (name, family, variant) => {
        const values = { name, family, variant };
        return template.replace(iconResolverPlaceholder, (placeholder, key) => values[key] ?? '');
    };
}

// a function member of an icon library: ctx holds args and the library name; there is no element of the member
function resolveIconLibraryFunction(name, member, descriptor) {
    return resolveFunction(descriptor, {
        element: undefined,
        member,
        label: `icon library "${name}" ${member}`,
        arity: iconLibraryFunctions[member].arity,
        context: { library: name },
    });
}

// the mutator function, the function of the obsolete global path, or undefined; a mutator function that does not
// resolve leaves the library without a mutator (resolveFunction logged the reason)
async function resolveIconMutator(name, options) {
    if (isFunctionDescriptor(options.mutatorFunction)) {
        return resolveIconLibraryFunction(name, 'mutator', options.mutatorFunction);
    }

    // TODO WA 3.15.0 train: remove the obsolete global path (IconLibraryOptions.Mutator)
    return options.mutator ? resolveGlobalFunction(options.mutator) : undefined;
}

function resolveGlobalFunction(path) {
    const target = findGlobalFunction(path);
    if (!target) {
        throw new Error(`Icon library mutator '${path}' is not a global function`);
    }

    return target;
}

/*
 * Data grid columns
 *
 * A column member that takes a function arrives as a WaJsFunction descriptor (see webawesome-functions.js). Before the
 * columns are assigned, every descriptor is resolved into its function, and the Web Awesome elements the functions
 * declare (WaJsFunction.Uses) are registered, because the autoloader does not see the cells in the grid's shadow root.
 * After the grid has rendered, discover(grid.shadowRoot) registers the elements no function declared; an element
 * defined later upgrades in place. A reference that does not resolve leaves its member out, so the column shows raw
 * values (or the built-in sort) and the grid keeps working.
 */

// the column members that take a function (upstream DataGridColumn): the number of arguments the grid passes before
// ctx, whether the function renders content in the shadow root, and the argument that holds the row of the call. A
// member with a static form (footer, aggregation, cellClass) keeps a string under the same key: only a descriptor is
// resolved.
// NOTE checked against WA 3.14.0: the arities match data-grid.d.ts; recheck them on each upgrade (watch list)
const dataGridColumnFunctions = {
    formatter: { arity: 2, renders: true, rowArgument: 1 },
    aggregatedFormatter: { arity: 2, renders: true },
    comparator: { arity: 4, renders: false },
    value: { arity: 1, renders: false, rowArgument: 0 },
    filterFn: { arity: 3, renders: false, rowArgument: 2 },
    footer: { arity: 1, renders: true },
    aggregation: { arity: 2, renders: false },
    cellClass: { arity: 2, renders: false, rowArgument: 1 },
};

// grid element -> its last push: the column JSON and, once resolved, the columns (a newer push replaces the entry)
const dataGridColumns = new WeakMap();

// the longest wait for the registration of the declared elements before the columns are assigned; an element that is
// defined later upgrades in place
const preloadTimeoutMs = 3000;

/**
 * Assigns the columns of a wa-data-grid from their JSON, with the function references resolved. The same JSON again for
 * the same grid assigns nothing, so the grid does not render every cell again.
 * @param {HTMLElement} element - The wa-data-grid element
 * @param {string} columnsJson - The JSON array of the column definitions
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET, used when the page has no Web Awesome script tag
 */
export async function setDataGridColumns(element, columnsJson, configuredEntryPointUrl) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (dataGridColumns.get(element)?.json === columnsJson) return;

    const push = { json: columnsJson, columns: JSON.parse(columnsJson) };
    dataGridColumns.set(element, push);

    const tags = new Set();
    const resolutions = [];
    let rendersContent = false;
    push.columns.forEach((column, index) => {
        for (const [member, signature] of Object.entries(dataGridColumnFunctions)) {
            const descriptor = column[member];
            if (!isFunctionDescriptor(descriptor)) continue;

            // the member is assigned once it resolves, and stays out when it does not
            delete column[member];
            descriptor.uses?.forEach(tag => tags.add(tag));
            rendersContent ||= signature.renders;

            // the column id as the grid computes it (its columnId: id, else field, else col-<index>)
            const columnId = column.id ?? column.field ?? `col-${index}`;
            resolutions.push(resolveFunction(descriptor, {
                element,
                member,
                label: `data grid column "${columnId}" ${member}`,
                arity: signature.arity,
                context: { column: columnId, grid: element },
                rowArgument: signature.rowArgument,
                rowKey: signature.rowArgument === undefined ? undefined : callArgs => dataGridRowKey(element, callArgs[signature.rowArgument]),
            }).then(target => {
                if (target) column[member] = target;
            }));
        }
    });

    if (resolutions.length > 0) {
        await Promise.all([preloadTags([...tags], configuredEntryPointUrl), ...resolutions]);

        // a newer push replaced this one while the references resolved
        if (dataGridColumns.get(element) !== push) return;
    }

    element.columns = push.columns;
    if (rendersContent) discoverShadowContent(element, configuredEntryPointUrl);
}

// the key of a row as the grid reads it (its getRowId: the row's own rowKey field)
function dataGridRowKey(grid, row) {
    const key = grid.rowKey;
    return key && row != null ? row[key] ?? undefined : undefined;
}

// registers the given wa-* elements before they render: discover() reads the tags of a data-wa-preload attribute, also
// on a detached element
async function preloadTags(tags, configuredEntryPointUrl) {
    if (tags.length === 0) return;

    try {
        const webAwesome = await importWebAwesome(configuredEntryPointUrl);
        const carrier = document.createElement('div');
        carrier.setAttribute('data-wa-preload', tags.join(' '));

        // discover() ends with an animation frame, which a hidden page never runs
        await Promise.race([webAwesome.discover(carrier), new Promise(resolve => setTimeout(resolve, preloadTimeoutMs))]);
    } catch (error) {
        console.error(`WebAwesome.Blazor: the elements ${tags.join(', ')} could not be registered: ${error.message}`);
    }
}

// registers the undefined wa-* elements in the shadow roots of an element once it has rendered
function discoverShadowContent(element, configuredEntryPointUrl) {
    renderedShadowRoots(element)
        .then(async roots => {
            if (roots.length === 0) return;

            const webAwesome = await importWebAwesome(configuredEntryPointUrl);
            for (const root of roots) await webAwesome.discover(root);
        })
        .catch(error => console.error(`WebAwesome.Blazor: the elements in the shadow root of ${element.localName} could not be registered: ${error.message}`));
}

/*
 * Shadow roots
 *
 * A function member renders its output in the shadow root of its element, or of an element in that shadow root:
 * wa-date-input renders its day cells in the shadow root of its own wa-date-picker. The rendered output there gets the
 * autoload discovery and the shadow styles of the wrapper.
 */

// tag -> the element in its shadow root whose shadow root also renders the output of the function members
const nestedShadowHosts = {
    'wa-date-input': 'wa-date-picker',
};

// the shadow roots of an element and of its nested host, once the element is defined and has rendered
async function renderedShadowRoots(element) {
    await customElements.whenDefined(element.localName);
    await element.updateComplete;

    const roots = element.shadowRoot ? [element.shadowRoot] : [];
    const nestedTag = nestedShadowHosts[element.localName];
    const nested = nestedTag ? element.shadowRoot?.querySelector(nestedTag) : null;
    if (nested) {
        await customElements.whenDefined(nestedTag);
        await nested.updateComplete;
        if (nested.shadowRoot) roots.push(nested.shadowRoot);
    }

    return roots;
}

/*
 * Shadow styles
 *
 * The CSS text of a wrapper's ShadowStyles goes into one constructed stylesheet per element, which is adopted by the
 * element's shadow roots (above). A new text replaces the content of the same sheet; an empty text removes the sheet.
 * The styles are for the output of the app's own functions; Web Awesome's own parts are styled with ::part and the
 * CSS custom properties.
 */

// element -> { css, sheet, roots }: the last text, its sheet (null while there is no text) and the roots holding it
const shadowStyles = new WeakMap();

/**
 * Adopts CSS text into the shadow roots of an element as one constructed stylesheet, once the element is defined and
 * has rendered. The same text again changes nothing; a new text replaces the content of the sheet; null or an empty
 * text removes the sheet.
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string|null} cssText - The CSS text, or null to remove the styles
 */
export function setShadowStyles(element, cssText) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const css = cssText || null;
    const state = shadowStyles.get(element) ?? { css: null, sheet: null, roots: [] };
    if (state.css === css) return;

    state.css = css;
    shadowStyles.set(element, state);

    if (css === null) {
        removeSheet(state);
        return;
    }

    if (state.sheet) {
        state.sheet.replaceSync(css);
        return;
    }

    const sheet = new CSSStyleSheet();
    sheet.replaceSync(css);
    state.sheet = sheet;

    // not awaited: an element whose module never loads (a Pro element on the free CDN) must not hold the interop call
    adoptSheet(element, state, sheet)
        .catch(error => console.error(`WebAwesome.Blazor: the shadow styles of ${element.localName} could not be adopted: ${error.message}`));
}

async function adoptSheet(element, state, sheet) {
    const roots = await renderedShadowRoots(element);

    // a newer push replaced or removed this sheet while the element rendered
    if (state.sheet !== sheet) return;

    for (const root of roots) root.adoptedStyleSheets = [...root.adoptedStyleSheets, sheet];
    state.roots = roots;
}

function removeSheet(state) {
    const sheet = state.sheet;
    state.sheet = null;
    for (const root of state.roots) root.adoptedStyleSheets = root.adoptedStyleSheets.filter(s => s !== sheet);
    state.roots = [];
}

/*
 * Element function members
 *
 * A wrapper parameter of type WaJsFunction (WaSelect.GetTagFunction, and so on) arrives as a descriptor (see
 * webawesome-functions.js) and is assigned to the element property once it resolves. The wrapper sends a member only
 * when its descriptor changed. Before the first assignment the element's own value of the member is kept, so a member
 * set back to null gets it again: the element's default renderer, or the empty hrefTemplate.
 */

// tag -> member -> the number of arguments the element passes (ctx follows them), whether the function renders its
// output in the shadow root (then the declared elements are registered before the assignment, and the rendered output
// is discovered after it), and the argument that holds the row of the call (for ctx.rowKey, see elementFunctionContexts).
// NOTE checked against WA 3.14.0: the members and arities match the components' .d.ts files; recheck them on each
// upgrade (watch list)
const elementFunctions = {
    'wa-select': {
        getTag: { arity: 2, renders: true },
    },
    'wa-combobox': {
        getTag: { arity: 2, renders: true },
        filter: { arity: 2, renders: false },
    },
    'wa-rating': {
        getSymbol: { arity: 2, renders: true },
    },
    'wa-slider': {
        valueFormatter: { arity: 1, renders: false },
    },
    'wa-date-picker': {
        isDateDisabled: { arity: 1, renders: false },
        dayContent: { arity: 1, renders: true },
    },
    'wa-date-input': {
        isDateDisabled: { arity: 1, renders: false },
        dayContent: { arity: 1, renders: true },
    },
    'wa-pagination': {
        hrefTemplate: { arity: 1, renders: false },
    },
    // NOTE checked against WA 3.14.0: the grid reads childRows only when it receives data (its row model is keyed on
    // the data). The first data push waits for the pending function members (setDataGridData); a childRows change after
    // the data applies from the next data push
    'wa-data-grid': {
        childRows: { arity: 1, renders: false, rowArgument: 0 },
        rowClass: { arity: 1, renders: false, rowArgument: 0 },
        rowDetail: { arity: 1, renders: true, rowArgument: 0 },
        searchFn: { arity: 3, renders: false, rowArgument: 2 },
        selectableRows: { arity: 1, renders: false, rowArgument: 0 },
    },
};

// tag -> the ctx members its function members get beyond args and element: context(element) adds fixed members, and
// rowKey(element, row) reads the key of the row argument of a member that declares one
const elementFunctionContexts = {
    'wa-data-grid': { context: element => ({ grid: element }), rowKey: dataGridRowKey },
};

// element -> member -> { json, original, kept, assigned }: the last descriptor JSON, the element's own value before the
// first assignment (kept once it is read), and the function assigned (undefined while none is)
const elementFunctionState = new WeakMap();

/**
 * Assigns a function-valued member of a Web Awesome element from its WaJsFunction descriptor, once the element is
 * defined. The same JSON again for the same member changes nothing. Null restores the element's own value of the member,
 * if the element still holds the assigned function. A reference that does not resolve writes one console.error and
 * leaves the element's own value.
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} member - The member, for example 'getTag'
 * @param {string|null} descriptorJson - The JSON of the descriptor, or null to remove the function
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET, used when the page has no Web Awesome script tag
 */
export function setElementFunction(element, member, descriptorJson, configuredEntryPointUrl) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const signature = elementFunctions[element.localName]?.[member];
    if (!signature) {
        throw new Error(`${element.localName} has no function member '${member}'`);
    }

    let members = elementFunctionState.get(element);
    if (!members) {
        members = new Map();
        elementFunctionState.set(element, members);
    }

    const state = members.get(member) ?? { json: null, original: undefined, assigned: undefined, kept: false };
    if (state.json === descriptorJson) return;

    state.json = descriptorJson;
    members.set(member, state);

    // not awaited: an element whose module never loads (a Pro element on the free CDN) must not hold the interop call;
    // a data push of the element waits for it (setDataGridData)
    const assignment = assignElementFunction(element, member, signature, state, descriptorJson, configuredEntryPointUrl)
        .catch(error => console.error(`WebAwesome.Blazor: the ${element.localName} ${member} could not be assigned: ${error.message}`));
    trackPendingFunction(element, assignment);
}

// element -> the assignments of its function members that have not finished (a reference that does not resolve
// finishes too, without a function)
const pendingElementFunctions = new WeakMap();

function trackPendingFunction(element, assignment) {
    let pending = pendingElementFunctions.get(element);
    if (!pending) {
        pending = new Set();
        pendingElementFunctions.set(element, pending);
    }

    pending.add(assignment);
    assignment.finally(() => pending.delete(assignment));
}

// grid element -> its data push that waits for pending function members (a newer push replaces it)
const deferredDataGridData = new WeakMap();

/**
 * Assigns the data of a wa-data-grid. While function members of the grid are still resolving (setElementFunction), the
 * assignment waits for them, because the grid reads childRows only when it receives data. A newer push replaces a
 * waiting one. Returns at once.
 * @param {HTMLElement} element - The wa-data-grid element
 * @param {Array} data - The row objects
 */
export function setDataGridData(element, data) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    const pending = pendingElementFunctions.get(element);
    if (!pending || pending.size === 0) {
        deferredDataGridData.delete(element);
        element.data = data;
        return;
    }

    const push = { data };
    deferredDataGridData.set(element, push);
    Promise.allSettled([...pending]).then(() => {
        if (deferredDataGridData.get(element) !== push) return;

        deferredDataGridData.delete(element);
        element.data = push.data;
    });
}
async function assignElementFunction(element, member, signature, state, descriptorJson, configuredEntryPointUrl) {
    await customElements.whenDefined(element.localName);

    let target;
    if (descriptorJson !== null) {
        const descriptor = JSON.parse(descriptorJson);
        const contexts = elementFunctionContexts[element.localName];
        const [resolved] = await Promise.all([
            resolveFunction(descriptor, {
                element,
                member,
                label: `${element.localName} ${member}`,
                arity: signature.arity,
                context: contexts?.context(element),
                rowArgument: signature.rowArgument,
                rowKey: contexts && signature.rowArgument !== undefined ? callArgs => contexts.rowKey(element, callArgs[signature.rowArgument]) : undefined,
            }),
            signature.renders ? preloadTags(descriptor.uses ?? [], configuredEntryPointUrl) : undefined,
        ]);
        target = resolved;
    }

    // a newer push replaced this one while the reference resolved
    if (state.json !== descriptorJson) return;

    if (target) {
        if (!state.kept) {
            state.original = element[member];
            state.kept = true;
        }

        element[member] = target;
        state.assigned = target;
        if (signature.renders) discoverShadowContent(element, configuredEntryPointUrl);
        return;
    }

    // no function (removed, or a reference that does not resolve): the element's own value again, unless something
    // else assigned the member meanwhile (an attribute of the same property, for example)
    if (state.assigned !== undefined && element[member] === state.assigned) element[member] = state.original;
    state.assigned = undefined;
}