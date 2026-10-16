/**
 * WebAwesome.Blazor.Extensions - the ctx helpers of data grid functions
 *
 * A context decorator (registerContextDecorator of WebAwesome.Blazor/webawesome-functions.js) that adds helpers to the
 * ctx object of every function bound to a wa-data-grid member: column members (formatter, aggregatedFormatter, footer,
 * cellClass, ...) and grid members (rowDetail, rowClass, ...). Design: tasks/WA-3.14/WAB-49/datagrid-cell-rendering-design.md,
 * sections 8.4 and 9.3.
 *
 *   ctx.emit(action, payload?, source?)  - raises a cell action (@onwablazor-cell-action) for the row of the call
 *   ctx.h(tag, props?, ...children)      - a small DOM builder; text is always set as text, never parsed as HTML
 *   ctx.preset(name, options?)           - calls a cell preset ('badge', 'currency', ...) for the value of the call
 *   ctx.format.number(value, options?)   - the text presets as plain functions (number, currency, date, ...)
 *   ctx.cache(key, create)               - keeps the node of a cell while the key and the row object stay the same
 *
 * The helpers live on the prototype of ctx, which the resolver builds once per resolved function, so a function that
 * does not use them pays nothing per call. They read the call through this[callArguments].
 */

import { callArguments } from '../WebAwesome.Blazor/webawesome-functions.js';
import {
    presets, presetPrefix, textPresetNames, raiseCellAction, ensureCellActionBridge, cachedNode, reportOnce, isAllowedUrl,
} from './data-grid-presets.js';

// the element whose members get the helpers
const gridTag = 'wa-data-grid';

// the members whose first argument is the value of the cell (the others have no value: a footer, a detail panel)
const valueMembers = new Set(['formatter', 'aggregatedFormatter', 'cellClass']);

// props of ctx.h that would parse HTML (a name with 'HTML', srcdoc); they are refused
const htmlPropPattern = /HTML/;
const srcdocProp = 'srcdoc';

// attributes that hold a URL: only relative URLs and http, https, mailto and tel are set
const urlProps = new Set(['href', 'src', 'formaction', 'xlink:href', 'poster', 'cite', 'background']);

// the props of ctx.h that raise a cell action on click
const actionProp = 'action';
const payloadProp = 'payload';

// the most option objects ctx.preset and ctx.format keep per resolved function, so that options built from row data
// cannot grow the cache without limit
const maxNormalizedOptions = 256;

/**
 * Adds the ctx helpers to the prototype of a function bound to a wa-data-grid member. Registered by the Extensions JS
 * initializer with registerContextDecorator.
 * @param {object} proto - The prototype of the ctx objects of one resolved function
 * @param {object} binding - { element, member, label, reference, args, rowArgument }
 */
export function decorateContext(proto, binding) {
    const grid = binding.element;
    if (grid?.localName !== gridTag) return;

    // the declarative actions of the function's output need the bridge before the first click
    ensureCellActionBridge(grid);

    const rowArgument = binding.rowArgument;
    const valueArgument = valueMembers.has(binding.member) ? 0 : undefined;
    const culture = typeof binding.args?.culture === 'string' ? binding.args.culture : undefined;
    const normalize = optionNormalizer(culture);

    // the cache owner of this resolved function: ctx.cache keys its nodes by the row object and this token
    const cacheToken = {};

    const rowOf = call => (rowArgument === undefined ? null : call[rowArgument] ?? null);
    const valueOf = call => (valueArgument === undefined ? null : call[valueArgument]);

    const format = Object.freeze(Object.fromEntries(textPresetNames.map(name => [
        name,
        (value, options) => presets[presetPrefix + name](value, null, { args: normalize(options) }),
    ])));

    Object.defineProperties(proto, {
        emit: {
            value: function emit(action, payload, source) {
                if (typeof action !== 'string' || action === '') {
                    throw new TypeError(`WebAwesome.Blazor.Extensions: ctx.emit needs an action id (the ${binding.label})`);
                }

                const call = this[callArguments];
                const row = rowOf(call);
                raiseCellAction(grid, {
                    column: this.column ?? '',
                    action,
                    rowKey: rowArgument === undefined ? null : this.rowKey ?? null,
                    row,
                    value: valueOf(call) ?? null,
                    payload: payload ?? null,
                }, source);
            },
        },
        h: {
            value: function h(tag, props, ...children) {
                return createElement(this, tag, props, children);
            },
        },
        preset: {
            value: function preset(name, options) {
                const fullName = String(name).startsWith(presetPrefix) ? String(name) : presetPrefix + name;
                const target = presets[fullName];
                if (!target) throw new TypeError(`WebAwesome.Blazor.Extensions: ctx.preset knows no preset '${name}'`);

                const call = this[callArguments];
                const value = options && Object.prototype.hasOwnProperty.call(options, 'value') ? options.value : valueOf(call);
                return target(value, rowOf(call), {
                    args: normalize(options),
                    grid,
                    column: this.column ?? '',
                    rowKey: rowArgument === undefined ? null : this.rowKey ?? null,
                });
            },
        },
        format: { value: format },
        cache: {
            value: function cache(key, create) {
                if (typeof create !== 'function') throw new TypeError('WebAwesome.Blazor.Extensions: ctx.cache needs a create function');

                return cachedNode(rowOf(this[callArguments]) ?? cacheToken, cacheToken, [key], create);
            },
        },
    });
}

// Returns a function that turns the options of ctx.preset and ctx.format into the args object of a preset: one object
// per option text, without 'value', with the culture of the column unless the options name one. The preset caches
// (Intl formatters, nodes) key on that object, so they work for a new object literal per call too.
function optionNormalizer(culture) {
    const byObject = new WeakMap();
    const byText = new Map();
    return options => {
        if (options !== null && typeof options === 'object') {
            const known = byObject.get(options);
            if (known) return known;
        }

        const { value: _value, ...rest } = options ?? {};
        if (rest.culture === undefined && culture !== undefined) rest.culture = culture;
        const text = JSON.stringify(rest);
        let args = byText.get(text);
        if (!args) {
            if (byText.size >= maxNormalizedOptions) byText.clear();
            args = Object.freeze(JSON.parse(text));
            byText.set(text, args);
        }

        if (options !== null && typeof options === 'object') byObject.set(options, args);
        return args;
    };
}

// ctx.h: an element with props and children
function createElement(ctx, tag, props, children) {
    const element = document.createElement(String(tag));
    if (props !== null && typeof props === 'object') {
        for (const [key, value] of Object.entries(props)) applyProp(element, key, value);

        const action = props[actionProp];
        if (typeof action === 'string' && action !== '') {
            const payload = props[payloadProp];
            element.addEventListener('click', () => ctx.emit(action, payload, element));
        }
    }

    appendChildren(element, children);
    return element;
}

function applyProp(element, key, value) {
    if (key === actionProp || key === payloadProp) return;

    if (htmlPropPattern.test(key) || key === srcdocProp) {
        reportOnce(`ctx.h does not set '${key}': it would parse HTML. Build the content with ctx.h.`);
        return;
    }

    if (key.startsWith('on')) {
        // a listener: onClick -> click, 'onwa-select' -> wa-select; a text value would be an inline handler
        if (typeof value === 'function') {
            const name = key.slice(2);
            element.addEventListener(name.includes('-') ? name : name.toLowerCase(), value);
        } else if (value !== null && value !== undefined) {
            reportOnce(`ctx.h sets '${key}' only from a function, never from text.`);
        }

        return;
    }

    if (value === null || value === undefined || value === false) return;

    if (key === 'textContent') {
        element.textContent = String(value);
        return;
    }

    const name = key === 'className' ? 'class' : key;
    if (value === true) {
        element.setAttribute(name, '');
        return;
    }

    if (typeof value === 'object' || typeof value === 'function') {
        // an object, an array or a function is a property, for example the data of an element
        element[key] = value;
        return;
    }

    const text = String(value);
    if (urlProps.has(name) && !isAllowedUrl(text)) {
        reportOnce(`ctx.h leaves out the ${name} '${text.slice(0, 40)}': only relative URLs and http, https, mailto and tel are set.`);
        return;
    }

    element.setAttribute(name, text);
}

function appendChildren(element, children) {
    for (const child of children) {
        if (Array.isArray(child)) {
            appendChildren(element, child);
        } else if (child instanceof Node) {
            element.append(child);
        } else if (child !== null && child !== undefined && child !== false && child !== true) {
            element.append(document.createTextNode(String(child)));
        }
    }
}
