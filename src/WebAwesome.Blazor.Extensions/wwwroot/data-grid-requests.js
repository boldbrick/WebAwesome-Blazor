/**
 * WebAwesome.Blazor.Extensions - requests from data grid content to .NET (ctx.invoke)
 *
 * ctx.invoke(action, payload?, source?) of a grid function (data-grid-context.js) sends a request to the .NET handler of
 * the grid and returns a Promise of its answer. Design: tasks/WA-3.14/WAB-49/datagrid-cell-rendering-design.md,
 * section 9.6.
 *
 * The .NET side is the component WaDataGridRequestHandler. It renders one element (display: contents) around its
 * grids and registers a DotNetObjectReference for that element here (registerRequestHandler). A request finds the
 * handler through the ancestors of the grid element at the time of the call, so a grid element that Blazor replaces
 * finds the same handler. The handlers live on globalThis[Symbol.for('WebAwesome.Blazor.Extensions.cellRequests')], so
 * two copies of this module (another base path) share them.
 *
 * Rules:
 * - The registration can arrive after the first cells render (the module import and the registration are
 *   asynchronous, two round trips on Blazor Server). A request without a handler waits for a registration up to
 *   handlerWaitTimeoutMs, then rejects.
 * - .NET answers { value, error }: the Promise resolves with value, or rejects with an Error whose message is error (a
 *   WaDataGridRequestException of the handler). A call that fails in the interop (another exception of the handler, a
 *   disposed reference, a Blazor Server disconnect) rejects with an Error that names the action, with the cause.
 * - Each call is a separate invokeMethodAsync, so concurrent calls get their own results.
 */

const logPrefix = 'WebAwesome.Blazor.Extensions:';

/**
 * The name of the invokable .NET method (CellRequestReceiver.InvokeMethodName in C#).
 */
export const invokeMethodName = 'InvokeCellRequestAsync';

// the longest wait for the registration of a handler before a request rejects
const handlerWaitTimeoutMs = 2000;

const stateKey = Symbol.for('WebAwesome.Blazor.Extensions.cellRequests');

// { handlers: WeakMap(element -> DotNetObjectReference), listeners: Set(function) }; keep the shape compatible
function sharedState() {
    let state = globalThis[stateKey];
    if (!state) {
        state = { handlers: new WeakMap(), listeners: new Set() };
        Object.defineProperty(globalThis, stateKey, { value: state });
    }

    return state;
}

/**
 * Registers the .NET handler of the grids inside an element (WaDataGridRequestHandler calls it after its first render).
 * @param {Element} element - The element of the WaDataGridRequestHandler
 * @param {object} handler - The DotNetObjectReference of its request receiver
 */
export function registerRequestHandler(element, handler) {
    if (!element || !handler) throw new Error(`${logPrefix} registerRequestHandler needs an element and a handler`);

    const state = sharedState();
    state.handlers.set(element, handler);
    for (const listener of [...state.listeners]) listener();
}

/**
 * Removes the handler of an element, when it is still the given one.
 * @param {Element} element - The element of the WaDataGridRequestHandler
 * @param {object} handler - The DotNetObjectReference that was registered
 */
export function unregisterRequestHandler(element, handler) {
    const state = sharedState();
    if (element && state.handlers.get(element) === handler) state.handlers.delete(element);
}

// the handler of the nearest ancestor (or the element itself) that has one; a shadow root continues at its host
function findHandler(element) {
    const { handlers } = sharedState();
    let node = element;
    while (node) {
        const handler = handlers.get(node);
        if (handler) return handler;
        node = node.parentNode instanceof ShadowRoot ? node.parentNode.host : node.parentElement;
    }

    return undefined;
}

function waitForHandler(element) {
    const found = findHandler(element);
    if (found) return Promise.resolve(found);

    const state = sharedState();
    return new Promise(resolve => {
        const finish = handler => {
            clearTimeout(timer);
            state.listeners.delete(listener);
            resolve(handler);
        };
        const listener = () => {
            const handler = findHandler(element);
            if (handler) finish(handler);
        };
        const timer = setTimeout(() => finish(undefined), handlerWaitTimeoutMs);
        state.listeners.add(listener);
    });
}

/**
 * Sends a request to the .NET handler of a grid and returns its answer.
 * @param {Element} element - The wa-data-grid element (or any element inside a WaDataGridRequestHandler)
 * @param {object} request - The request as .NET reads it (WaDataGridCellActionEventArgs): { column, action, rowKey,
 *        row, value, payload, rowIndex }, JSON-safe
 * @returns {Promise<any>} The value of the handler (null for none)
 */
export async function invokeCellRequest(element, request) {
    const action = request?.action;
    const handler = await waitForHandler(element);
    if (!handler) {
        throw new Error(`${logPrefix} ctx.invoke('${action}') found no request handler. Put the WaDataGrid inside a WaDataGridRequestHandler.`);
    }

    let response;
    try {
        response = await handler.invokeMethodAsync(invokeMethodName, request);
    } catch (error) {
        throw new Error(`${logPrefix} the request '${action}' did not complete in .NET: ${error?.message ?? error}`, { cause: error });
    }

    if (typeof response?.error === 'string') throw new Error(response.error);
    return response?.value ?? null;
}
