/**
 * Web Awesome Blazor JavaScript function references
 *
 * A Web Awesome member that takes a function (a data grid column formatter, and so on) gets it from C# as a WaJsFunction
 * reference. The reference crosses the JS interop boundary as a JSON descriptor (WaJsFunction.WriteDescriptor):
 *
 *   { "$wab": "fn", "ref": { "kind": "registered", "name": "myapp.state" }, "args": {...}, "uses": ["wa-badge"] }
 *   { "$wab": "fn", "ref": { "kind": "module", "url": "./Pages/Orders.razor.js", "export": "stateCell" } }
 *   { "$wab": "fn", "ref": { "kind": "global", "path": "myApp.grid.stateCell" } }
 *
 * This module is the public module of the function references. An app, or the WebAwesome.Blazor.Extensions package,
 * imports it from _content/WebAwesome.Blazor/webawesome-functions.js:
 *
 *   registerFunction(name, fn)          - makes fn available to WaJsFunction.Registered(name)
 *   registerContextDecorator(decorator) - adds members to the ctx object of every resolved function
 *
 * The interop module (webawesome-interop.js) resolves descriptors with resolveFunction. The resolved function calls the
 * referenced function with the element's arguments in their order and one trailing ctx object. Nothing here evaluates
 * text as code: a reference names a function, it never carries one.
 *
 * The registry is shared state on globalThis[Symbol.for('WebAwesome.Blazor.functions')], so every copy of this module
 * (for example one imported through another base path) reads and writes the same registry. Its shape is a contract
 * between copies of different versions: { functions: Map<name, fn>, decorators: fn[] (replaced on each change, never
 * changed in place), listeners: Set<() => void> (called after each change of functions) }.
 */

const registryKey = Symbol.for('WebAwesome.Blazor.functions');

/**
 * The key of the element's arguments on a ctx object. A context decorator reads the arguments of the current call
 * through it (this[callArguments] in a member it defines), for example the row of a formatter call.
 */
export const callArguments = Symbol.for('WebAwesome.Blazor.callArguments');

// how long a Registered reference waits for its name after one animation frame, for a registration that comes late
// (for example from a JS initializer that runs after the first render)
const lateRegistrationTimeoutMs = 2000;

// the longest wait for one animation frame; a hidden page runs no frames
const frameTimeoutMs = 100;

// the export a module reference without an export name uses (WaJsFunction.DefaultExportName)
const defaultExportName = 'default';

// the URL schemes a module may have (WaJsFunction.Module accepts only these)
const moduleProtocols = ['http:', 'https:'];

const logPrefix = 'WebAwesome.Blazor:';

// absolute module URL -> import promise, so each module loads once
const moduleImports = new Map();

/**
 * Registers a function under a name, for WaJsFunction.Registered(name). A registration under an existing name replaces
 * it. Register before the element renders, for example in the app's JS initializer; a reference waits a short time for
 * a late registration.
 * @param {string} name - The name, for example 'myapp.orderState'
 * @param {Function} fn - The function
 * @returns {() => void} A function that removes this registration
 */
export function registerFunction(name, fn) {
    if (typeof name !== 'string' || name.trim() === '' || name.trim() !== name) {
        throw new TypeError(`${logPrefix} a function name must be a non-empty string without leading or trailing whitespace`);
    }

    if (typeof fn !== 'function') {
        throw new TypeError(`${logPrefix} the value registered under '${name}' is not a function`);
    }

    const registry = sharedRegistry();
    registry.functions.set(name, fn);
    notifyListeners(registry);

    return () => {
        if (registry.functions.get(name) === fn) registry.functions.delete(name);
    };
}

/**
 * Registers a context decorator. A decorator adds members to the ctx object of resolved functions: it is called once per
 * resolved function, before its first call (and again when the decorator list changes), as decorator(proto, binding).
 * proto is the prototype of every ctx of that function: define methods or getters on it, which read the call through
 * this (this.args, this.element, this[callArguments], ...). binding describes the function: { element, member, label,
 * reference, args, rowArgument }; rowArgument is the index of the call argument that holds the row (undefined for a
 * member without a row, for example a column footer). A decorator must not throw; an error is logged and the decorator
 * is skipped.
 * @param {(proto: object, binding: object) => void} decorator - The decorator
 * @returns {() => void} A function that removes the decorator
 */
export function registerContextDecorator(decorator) {
    if (typeof decorator !== 'function') {
        throw new TypeError(`${logPrefix} a context decorator must be a function`);
    }

    const registry = sharedRegistry();
    if (!registry.decorators.includes(decorator)) {
        registry.decorators = [...registry.decorators, decorator];
    }

    return () => {
        if (registry.decorators.includes(decorator)) {
            registry.decorators = registry.decorators.filter(d => d !== decorator);
        }
    };
}

/**
 * Whether a value is a WaJsFunction descriptor.
 * @param {any} value - The value
 * @returns {boolean} true for an object with the descriptor marker
 */
export function isFunctionDescriptor(value) {
    return value !== null && typeof value === 'object' && value.$wab === 'fn';
}

/**
 * Resolves a WaJsFunction descriptor into a function that the element calls with its own arguments. The returned
 * function calls the referenced function with those arguments and a trailing ctx object. Used by webawesome-interop.js
 * for every function-valued member; the binding describes the member.
 * @param {object} descriptor - The descriptor ({ $wab: 'fn', ref, args?, uses? })
 * @param {object} binding - The member that gets the function:
 *        element - the element of the member (ctx.element);
 *        member - the member name, for example 'formatter' (given to the decorators);
 *        label - the member for messages, for example 'data grid column "state" formatter';
 *        arity - the number of arguments the element passes; ctx always follows them (optional: after the actual ones);
 *        context - more ctx members, for example { column, grid } (optional);
 *        rowKey - (callArgs) => the key of the row of a call, read lazily as ctx.rowKey (optional);
 *        rowArgument - the index of the argument that holds the row of a call, given to the decorators (optional)
 * @returns {Promise<Function|undefined>} The function, or undefined when the reference does not resolve: one
 *          console.error names the member and the reference, and the caller leaves the member out
 */
export async function resolveFunction(descriptor, binding) {
    let target;
    try {
        target = await findTarget(descriptor?.ref);
    } catch (error) {
        console.error(`${logPrefix} the ${binding.label} ${describeReference(descriptor?.ref)} does not resolve: ${error.message}. The member is left out.`);
        return undefined;
    }

    return bindFunction(target, descriptor, binding);
}

/**
 * Finds a function on a global dotted path, for example 'myApp.grid.stateCell'.
 * @param {string} path - The dotted path from the global object
 * @returns {Function|undefined} The function, bound to its owner object, or undefined when the path names no function
 */
export function findGlobalFunction(path) {
    const members = String(path).split('.');
    const owner = members.slice(0, -1).reduce((scope, member) => scope?.[member], globalThis);
    const target = owner?.[members[members.length - 1]];
    if (typeof target !== 'function') return undefined;

    // keep the owner as "this", as a call through the dotted path would
    return target.bind(owner);
}

function sharedRegistry() {
    let registry = globalThis[registryKey];
    if (!registry) {
        registry = { functions: new Map(), decorators: [], listeners: new Set() };
        Object.defineProperty(globalThis, registryKey, { value: registry });
    }

    return registry;
}

function notifyListeners(registry) {
    // a listener removes itself when it is done, so iterate over a copy
    for (const listener of [...registry.listeners]) listener();
}

async function findTarget(ref) {
    switch (ref?.kind) {
        case 'registered':
            return findRegisteredFunction(ref.name);
        case 'module':
            return findModuleExport(ref);
        case 'global': {
            const target = findGlobalFunction(ref.path);
            if (!target) throw new Error('the path names no function');
            return target;
        }
        default:
            throw new Error(`the reference kind '${ref?.kind}' is unknown`);
    }
}

async function findRegisteredFunction(name) {
    const registry = sharedRegistry();
    let target = registry.functions.get(name);
    if (!target) {
        await nextFrame();
        target = registry.functions.get(name) ?? await registration(registry, name);
    }

    if (!target) {
        throw new Error(`no function is registered under this name within ${lateRegistrationTimeoutMs} ms; register it with registerFunction of _content/WebAwesome.Blazor/webawesome-functions.js before the element renders`);
    }

    return target;
}

function nextFrame() {
    return new Promise(resolve => {
        requestAnimationFrame(() => resolve());
        setTimeout(resolve, frameTimeoutMs);
    });
}

// the function registered under the name within lateRegistrationTimeoutMs, or undefined
function registration(registry, name) {
    return new Promise(resolve => {
        const finish = target => {
            clearTimeout(timer);
            registry.listeners.delete(listener);
            resolve(target);
        };
        const listener = () => {
            const target = registry.functions.get(name);
            if (target) finish(target);
        };
        const timer = setTimeout(() => finish(undefined), lateRegistrationTimeoutMs);
        registry.listeners.add(listener);
    });
}

async function findModuleExport(ref) {
    const url = new URL(ref.url, document.baseURI);
    if (!moduleProtocols.includes(url.protocol)) {
        throw new Error(`the scheme '${url.protocol}' is not allowed for a module`);
    }

    if (url.origin !== location.origin && ref.allowCrossOrigin !== true) {
        throw new Error(`the module is not on the origin of the page (${location.origin}); allow another origin explicitly with allowCrossOrigin`);
    }

    const module = await importModule(url.href);
    const exportName = ref.export ?? defaultExportName;
    const target = module[exportName];
    if (typeof target !== 'function') {
        throw new Error(`the module has no function export '${exportName}'`);
    }

    return target;
}

function importModule(href) {
    let module = moduleImports.get(href);
    if (!module) {
        module = import(href);
        moduleImports.set(href, module);
    }

    return module;
}

function describeReference(ref) {
    switch (ref?.kind) {
        case 'registered':
            return `function '${ref.name}' (registered)`;
        case 'module':
            return `export '${ref.export ?? defaultExportName}' of module '${ref.url}'`;
        case 'global':
            return `global function '${ref.path}'`;
        default:
            return 'function reference';
    }
}

// The ctx object exists once per call, so that a decorator member (for example emit, WAB-59) knows the row of the call.
// Its cost stays small: the members that do not change per call (args, element, column, grid, the decorator members)
// are on a shared prototype, built once per resolved function, and the object itself holds only the call arguments.
// rowKey is a getter, so a function that does not read it pays nothing for it.
function bindFunction(target, descriptor, binding) {
    const registry = sharedRegistry();

    const base = { args: descriptor.args, element: binding.element, ...binding.context };
    if (binding.rowKey) {
        const rowKeyOf = binding.rowKey;
        Object.defineProperty(base, 'rowKey', {
            enumerable: true,
            get() { return rowKeyOf(this[callArguments]); },
        });
    }

    const decoration = Object.freeze({
        element: binding.element,
        member: binding.member,
        label: binding.label,
        reference: descriptor.ref,
        args: descriptor.args,
        rowArgument: binding.rowArgument,
    });

    function Context(callArgs) {
        this[callArguments] = callArgs;
    }

    // the decorator list the prototype was built with; the registry replaces the list on each change
    let decorators = null;
    const createContext = callArgs => {
        if (decorators !== registry.decorators) {
            decorators = registry.decorators;
            Context.prototype = decorate(Object.create(base), decorators, decoration);
        }

        return new Context(callArgs);
    };

    // a wrapper with fixed parameters is several times faster than one with rest parameters and a spread call
    switch (binding.arity) {
        case 0:
            return function () { return target(createContext([])); };
        case 1:
            return function (a) { return target(a, createContext([a])); };
        case 2:
            return function (a, b) { return target(a, b, createContext([a, b])); };
        case 3:
            return function (a, b, c) { return target(a, b, c, createContext([a, b, c])); };
        case 4:
            return function (a, b, c, d) { return target(a, b, c, d, createContext([a, b, c, d])); };
        default:
            return function (...callArgs) {
                if (binding.arity !== undefined) callArgs.length = binding.arity;
                return target(...callArgs, createContext(callArgs));
            };
    }
}

function decorate(proto, decorators, decoration) {
    for (const decorator of decorators) {
        try {
            decorator(proto, decoration);
        } catch (error) {
            console.error(`${logPrefix} a context decorator failed for the ${decoration.label}: ${error.message}`);
        }
    }

    return proto;
}
