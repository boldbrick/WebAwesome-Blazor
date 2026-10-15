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

    invokeMethod(element, methodName, []);
    return true;
}
/**
 * Invokes a method on a Web Awesome element
 * @param {HTMLElement} element - The Web Awesome element
 * @param {string} methodName - The name of the method to invoke
 * @param {Array} args - Arguments to pass to the method
 * @returns {any} The result of the method call
 */
export function invokeMethod(element, methodName, args) {
    if (!element) {
        throw new Error('Element reference is null or undefined');
    }

    if (!methodName) {
        throw new Error('Method name cannot be null or empty');
    }

    const tagName = element.tagName?.toLowerCase() || 'unknown';

    // Check if the method exists on the element
    if (typeof element[methodName] !== 'function') {
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

/**
 * Registers an icon library with Web Awesome
 * @param {string|null} configuredEntryPointUrl - Web Awesome entry point configured in .NET, used when the page has no Web Awesome script tag
 * @param {string} name - Library name, referenced by wa-icon's library attribute
 * @param {{resolver: string, mutator?: string|null, spriteSheet?: boolean}} options - URL template with {name}, {family} and {variant}
 *        placeholders, optional global mutator function path (e.g. "myApp.icons.mutate") and the sprite sheet flag
 */
export async function registerIconLibrary(configuredEntryPointUrl, name, options) {
    if (!name) {
        throw new Error('Icon library name cannot be null or empty');
    }

    if (!options?.resolver) {
        throw new Error(`Icon library '${name}' requires a resolver URL template`);
    }

    const webAwesome = await importWebAwesome(configuredEntryPointUrl);
    webAwesome.registerIconLibrary(name, {
        resolver: createIconResolver(options.resolver),
        mutator: options.mutator ? resolveGlobalFunction(options.mutator) : undefined,
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
// ctx, whether the function renders cell content, and the argument that holds the row of the cell
const dataGridColumnFunctions = {
    formatter: { arity: 2, renders: true, rowArgument: 1 },
    aggregatedFormatter: { arity: 2, renders: true },
    comparator: { arity: 4, renders: false },
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

// registers the undefined wa-* elements in the shadow root once the element has rendered
function discoverShadowContent(element, configuredEntryPointUrl) {
    customElements.whenDefined(element.localName)
        .then(() => element.updateComplete)
        .then(async () => {
            if (!element.shadowRoot) return;

            const webAwesome = await importWebAwesome(configuredEntryPointUrl);
            await webAwesome.discover(element.shadowRoot);
        })
        .catch(error => console.error(`WebAwesome.Blazor: the elements in the cells of ${element.localName} could not be registered: ${error.message}`));
}