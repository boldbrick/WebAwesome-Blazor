/**
 * WebAwesome.Blazor.Extensions - data grid cell presets and the cell action bridge
 *
 * The C# factories WaDataGridFormatters and WaDataGridComparators (namespace WebAwesome.Blazor.Extensions.DataGrid)
 * return WaJsFunction.Registered references to the functions of `presets`, with their options as ctx.args. The JS
 * initializer of the package registers them in the shared function registry of WebAwesome.Blazor
 * (webawesome-functions.js) under the same names, so the grid's column resolver finds them. The resolver calls each one
 * like the grid member it binds, with one trailing ctx: a formatter as (value, row, ctx), a comparator as
 * (a, b, rowA, rowB, ctx). ctx.args is one object per resolved column function, so it keys the caches of a column.
 *
 * The table also holds the presets of the other function members (WaDataGridClassRules, WaDataGridFooters,
 * WaDataGridSearch, WaDataGridRows): cellClass and rowClass rules, footer aggregates, the formatted search, the detail
 * fields and the selectable field. The ctx helpers of custom functions (ctx.emit, ctx.h, ...) are in data-grid-context.js.
 *
 * Rules (design: tasks/WA-3.14/WAB-49/datagrid-cell-rendering-design.md, sections 6, 7, 9 and 11):
 * - A text preset returns a string, so the CSV export and the copy of rows hold the formatted text. A visual preset
 *   returns an element and keeps it per row object and column while the value stays the same, so the element keeps its
 *   state when the grid renders again; new data from .NET brings new row objects, which empties the cache.
 * - Intl formatter objects are created once per column (per ctx.args), never per cell.
 * - The DOM is built with createElement, setAttribute and textContent only. No HTML text is parsed, so row data cannot
 *   become markup. Each icon-only element has an accessible name.
 * - A link URL comes from a C# template; each placeholder value is URL-encoded, and a URL whose scheme is not http,
 *   https, mailto or tel (or relative) renders as plain text.
 * - Content that raises a cell action dispatches `wablazor-cell-action` on the grid host (bubbles, composed). A click
 *   that raises an action, hits a declarative action (data-wablazor-action) or hits a control of a preset does not
 *   reach Blazor as wa-cell-click (decision D4): a wa-cell-click listener on the grid host stops its propagation, so
 *   listeners on the grid element still see it, but Blazor, which listens on the document, does not.
 */

const logPrefix = 'WebAwesome.Blazor.Extensions:';

/**
 * The name of the cell action event, dispatched on the grid element. EventHandlers.CellActionEventName in C#.
 */
export const cellActionEventName = 'wablazor-cell-action';

// the index of a row that is not known (WaDataGridCellActionEventArgs.UnknownRowIndex)
const unknownRowIndex = -1;

// the depth of the row and payload copies in the event arguments: a row with nested objects (design 9.2)
const rowDepth = 4;

// the options of a preset function that got no ctx.args (a reference made without the C# factory)
const noOptions = Object.freeze({});

// the variants of Web Awesome, the only values a color option may name
const variants = ['brand', 'neutral', 'success', 'warning', 'danger'];

// the URL schemes a link may have; a relative URL resolves to the scheme of the page
const linkProtocols = ['http:', 'https:', 'mailto:', 'tel:'];

// the default separator of the list preset (WaDataGridFormatters.DefaultListSeparator)
const defaultListSeparator = ', ';

// the inline style of links, and of the button of an action link (the grid's shadow root has no page styles)
const linkStyle = 'color: var(--wa-color-text-link);';
const actionLinkStyle = 'color: var(--wa-color-text-link); text-decoration: underline; background: none; border: none; '
    + 'padding: 0; margin: 0; font: inherit; cursor: pointer;';

// the target that opens a new browsing context, which gets rel="noopener noreferrer"
const newContextTarget = '_blank';
const newContextRel = 'noopener noreferrer';

// a DateOnly value (yyyy-MM-dd) and a TimeOnly value (HH:mm[:ss[.fffffff]]) as System.Text.Json writes them
const calendarDatePattern = /^(\d{4})-(\d{2})-(\d{2})$/;
const timeOfDayPattern = /^(\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{1,7}))?)?$/;

// a TimeSpan as System.Text.Json writes it ([-][d.]hh:mm:ss[.fffffff]) and an ISO 8601 duration (PnYnMnWnDTnHnMnS)
const timeSpanPattern = /^(-)?(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.(\d{1,7}))?$/;
const isoDurationPattern = /^(-)?P(?!$)(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)W)?(?:(\d+)D)?(?:T(?=\d)(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d{1,3})?)S)?)?$/;

// the units of the relative time, the longest first, with their length in seconds
const relativeTimeUnits = [
    ['year', 365 * 24 * 3600],
    ['month', 30 * 24 * 3600],
    ['week', 7 * 24 * 3600],
    ['day', 24 * 3600],
    ['hour', 3600],
    ['minute', 60],
    ['second', 1],
];

// the decimal prefixes of the bytes preset, as wa-format-bytes uses them
const bytePrefixes = ['', 'kilo', 'mega', 'giga', 'tera', 'peta'];
const bitPrefixes = ['', 'kilo', 'mega', 'giga', 'tera'];
const bytesPrefixBase = 1000;
const bytesPrecision = 3;

// the fields of a duration, the longest first, and the units Intl.NumberFormat names them with
const durationFields = ['years', 'months', 'weeks', 'days', 'hours', 'minutes', 'seconds', 'milliseconds'];
const durationUnits = {
    years: 'year', months: 'month', weeks: 'week', days: 'day',
    hours: 'hour', minutes: 'minute', seconds: 'second', milliseconds: 'millisecond',
};
const durationDefaultStyle = 'short';
const durationDigitalStyle = 'digital';

const millisecondsPerSecond = 1000;
const secondsPerMinute = 60;
const secondsPerHour = 3600;
const secondsPerDay = 86400;
const fractionDigitsOfTicks = 7;
const fractionDigitsOfMilliseconds = 3;

/* ------------------------------------------------------------------------------------------------------------------
 * Shared helpers
 * ------------------------------------------------------------------------------------------------------------------ */

function isEmpty(value) {
    return value === null || value === undefined || value === '';
}

function nullTextOf(args) {
    return args.nullText ?? '';
}

// the entry of a map from C# by its own key only, so that a value such as "constructor" finds nothing
function lookup(map, key) {
    return map && Object.prototype.hasOwnProperty.call(map, key) ? map[key] : undefined;
}

// a field of the row on a dotted path, by own keys only
function readPath(row, path) {
    let current = row;
    for (const key of path) {
        if (current === null || typeof current !== 'object' || !Object.prototype.hasOwnProperty.call(current, key)) return undefined;
        current = current[key];
    }

    return current;
}

// each message once per page, so a broken option does not log once per cell
const reportedMessages = new Set();

function reportOnce(message) {
    if (reportedMessages.has(message)) return;
    reportedMessages.add(message);
    console.error(`${logPrefix} ${message}`);
}

// column caches: ctx.args -> Map(name -> value); ctx.args is one object per resolved column function
const columnCaches = new WeakMap();

function columnCached(args, name, create) {
    let cache = columnCaches.get(args);
    if (!cache) {
        cache = new Map();
        columnCaches.set(args, cache);
    }

    let value = cache.get(name);
    if (value === undefined) {
        value = create();
        cache.set(name, value);
    }

    return value;
}

// Creates an Intl formatter with the culture of the column. An invalid locale or time zone (a RangeError) is reported
// once, and the formatter of the browser default applies.
function createFormat(Format, args, options, label) {
    try {
        return new Format(args.culture, options);
    } catch (error) {
        if (!(error instanceof RangeError)) throw error;

        reportOnce(`the ${label} options do not work in this browser (${error.message}); the browser default applies.`);
        const { timeZone, ...withoutTimeZone } = options;
        try {
            return new Format(undefined, withoutTimeZone);
        } catch {
            return new Format(undefined, {});
        }
    }
}

// The node cache of the visual presets (design 6.1): row object -> (ctx.args -> { keys, node, epoch }). A node that the
// current render already returned is not returned again in the same render: the grouped cell of a group row shows the
// value of its first data row, with the same row object, and a node cannot be in two cells. A render of the grid is one
// synchronous task, so a microtask ends it.
const nodeCaches = new WeakMap();
let renderEpoch = 0;
let renderEpochEnding = false;

function currentRenderEpoch() {
    if (!renderEpochEnding) {
        renderEpochEnding = true;
        queueMicrotask(() => {
            renderEpoch++;
            renderEpochEnding = false;
        });
    }

    return renderEpoch;
}

function cachedNode(row, args, keys, create) {
    if (row === null || typeof row !== 'object') return create();

    let byColumn = nodeCaches.get(row);
    if (!byColumn) {
        byColumn = new WeakMap();
        nodeCaches.set(row, byColumn);
    }

    const epoch = currentRenderEpoch();
    const entry = byColumn.get(args);
    if (entry && entry.keys.length === keys.length && entry.keys.every((key, i) => Object.is(key, keys[i]))) {
        if (entry.epoch === epoch) return create();

        entry.epoch = epoch;
        return entry.node;
    }

    const node = create();
    byColumn.set(args, { keys, node, epoch });
    return node;
}

/* ------------------------------------------------------------------------------------------------------------------
 * Numbers
 * ------------------------------------------------------------------------------------------------------------------ */

function toNumber(value) {
    if (typeof value === 'number') return value;
    if (typeof value === 'string' && value.trim() !== '') return Number(value);
    return NaN;
}

function numberOptions(args, style) {
    const options = { style };
    if (args.minFractionDigits !== undefined) options.minimumFractionDigits = args.minFractionDigits;
    if (args.maxFractionDigits !== undefined) options.maximumFractionDigits = args.maxFractionDigits;
    if (args.useGrouping !== undefined) options.useGrouping = args.useGrouping;
    if (args.notation !== undefined) options.notation = args.notation;
    if (args.signDisplay !== undefined) options.signDisplay = args.signDisplay;
    return options;
}

function plainNumberFormat(args) {
    return columnCached(args, 'number', () => {
        const options = numberOptions(args, args.unit ? 'unit' : 'decimal');
        if (args.unit) options.unit = args.unit;
        if (args.unitDisplay) options.unitDisplay = args.unitDisplay;
        return createFormat(Intl.NumberFormat, args, options, 'number');
    });
}

function currencyFormat(args, currency) {
    return columnCached(args, `currency:${currency}`, () => {
        const options = numberOptions(args, 'currency');
        options.currency = currency;
        if (args.currencyDisplay) options.currencyDisplay = args.currencyDisplay;
        return createFormat(Intl.NumberFormat, args, options, `currency ${currency}`);
    });
}

function percentFormat(args) {
    return columnCached(args, 'percent', () => createFormat(Intl.NumberFormat, args, numberOptions(args, 'percent'), 'percent'));
}

function number(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    return Number.isFinite(n) ? plainNumberFormat(args).format(n) : String(value);
}

function currency(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    // the code of the row, else the fixed code; a row without any code shows a plain number
    const rowCurrency = args.currencyField ? readPath(row, args.currencyField.split('.')) : undefined;
    const code = typeof rowCurrency === 'string' && rowCurrency !== '' ? rowCurrency.toUpperCase() : args.currency;
    return code ? currencyFormat(args, code).format(n) : plainNumberFormat(args).format(n);
}

function percent(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    // Intl multiplies by 100; a value that is already a percentage is divided first
    return percentFormat(args).format(args.scale === 'percent' ? n / 100 : n);
}

/* ------------------------------------------------------------------------------------------------------------------
 * Dates and times
 * ------------------------------------------------------------------------------------------------------------------ */

// a Date of a year, month and day in UTC; Date.UTC reads the years 0-99 as 1900-1999, setUTCFullYear does not
function utcDate(year, month, day, hours = 0, minutes = 0, seconds = 0, milliseconds = 0) {
    const date = new Date(0);
    date.setUTCFullYear(year, month - 1, day);
    date.setUTCHours(hours, minutes, seconds, milliseconds);
    return date;
}

function fractionMilliseconds(fraction) {
    return fraction ? Math.floor(Number(fraction.padEnd(fractionDigitsOfTicks, '0')) / 10 ** (fractionDigitsOfTicks - fractionDigitsOfMilliseconds)) : 0;
}

// Reads a date value. A calendar date (DateOnly) or a time of day (TimeOnly) has no time zone: it comes back as a UTC
// date with floating = true, and the caller formats it in UTC, so that it shows the same day and time everywhere.
// Other strings and numbers are instants.
function readDate(value) {
    if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : { date: value, floating: false };

    if (typeof value === 'string') {
        let match = calendarDatePattern.exec(value);
        if (match) return { date: utcDate(Number(match[1]), Number(match[2]), Number(match[3])), floating: true };

        match = timeOfDayPattern.exec(value);
        if (match) {
            return {
                date: utcDate(1970, 1, 1, Number(match[1]), Number(match[2]), Number(match[3] ?? 0), fractionMilliseconds(match[4])),
                floating: true,
            };
        }
    }

    if (typeof value === 'string' || typeof value === 'number') {
        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? null : { date, floating: false };
    }

    return null;
}

function dateTimeOptions(mode, args) {
    const { dateStyle, timeStyle } = args;
    switch (mode) {
        case 'date':
            return dateStyle ? { dateStyle } : { year: 'numeric', month: 'numeric', day: 'numeric' };
        case 'time':
            return timeStyle ? { timeStyle } : { hour: 'numeric', minute: '2-digit' };
        default:
            return dateStyle || timeStyle
                ? { dateStyle: dateStyle ?? 'short', timeStyle: timeStyle ?? 'short' }
                : { year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: '2-digit' };
    }
}

// the formatter of a mode: in the time zone of the column for instants, in UTC for floating values
function dateTimeFormat(mode, args, floating) {
    return columnCached(args, `${mode}:${floating ? 'floating' : 'instant'}`, () => {
        const options = dateTimeOptions(mode, args);
        if (floating) options.timeZone = 'UTC';
        else if (args.timeZone) options.timeZone = args.timeZone;
        return createFormat(Intl.DateTimeFormat, args, options, mode);
    });
}

function formatDateValue(mode, value, args) {
    const read = readDate(value);
    return read ? dateTimeFormat(mode, args, read.floating).format(read.date) : String(value);
}

function datePreset(mode) {
    return (value, row, ctx) => {
        const args = ctx.args ?? noOptions;
        return isEmpty(value) ? nullTextOf(args) : formatDateValue(mode, value, args);
    };
}

function relativeTime(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const read = readDate(value);
    if (!read) return String(value);

    // a calendar date is relative to the local midnight of that day
    const date = read.floating
        ? new Date(read.date.getUTCFullYear(), read.date.getUTCMonth(), read.date.getUTCDate(), read.date.getUTCHours(), read.date.getUTCMinutes(), read.date.getUTCSeconds())
        : read.date;

    const seconds = (date.getTime() - Date.now()) / millisecondsPerSecond;
    const [unit, length] = relativeTimeUnits.find(([, size]) => Math.abs(seconds) >= size) ?? relativeTimeUnits[relativeTimeUnits.length - 1];
    const format = columnCached(args, 'relativeTime', () => {
        const options = {};
        if (args.numeric) options.numeric = args.numeric;
        if (args.style) options.style = args.style;
        return createFormat(Intl.RelativeTimeFormat, args, options, 'relative time');
    });

    return format.format(Math.round(seconds / length), unit);
}

/* ------------------------------------------------------------------------------------------------------------------
 * Bytes and durations
 * ------------------------------------------------------------------------------------------------------------------ */

function bytes(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    // the largest decimal prefix that keeps the number at 1 or more, as wa-format-bytes does
    const unit = args.unit === 'bit' ? 'bit' : 'byte';
    const prefixes = unit === 'bit' ? bitPrefixes : bytePrefixes;
    const index = n === 0 ? 0 : Math.max(0, Math.min(Math.floor(Math.log10(Math.abs(n)) / 3), prefixes.length - 1));
    const scaled = Number((n / bytesPrefixBase ** index).toPrecision(bytesPrecision));
    const prefixed = prefixes[index] + unit;

    const format = columnCached(args, `bytes:${prefixed}`, () => {
        const options = { style: 'unit', unit: prefixed };
        if (args.display) options.unitDisplay = args.display;
        return createFormat(Intl.NumberFormat, args, options, 'bytes');
    });

    return format.format(scaled);
}

// splits a count of milliseconds into days, hours, minutes, seconds and milliseconds
function durationOfMilliseconds(total) {
    const sign = total < 0 ? -1 : 1;
    let rest = Math.round(Math.abs(total));
    const milliseconds = rest % millisecondsPerSecond;
    rest = Math.floor(rest / millisecondsPerSecond);

    return withSign({
        days: Math.floor(rest / secondsPerDay),
        hours: Math.floor((rest % secondsPerDay) / secondsPerHour),
        minutes: Math.floor((rest % secondsPerHour) / secondsPerMinute),
        seconds: rest % secondsPerMinute,
        milliseconds,
    }, sign);
}

function withSign(duration, sign) {
    if (sign < 0) {
        for (const field of Object.keys(duration)) duration[field] = -duration[field];
    }

    return duration;
}

// Reads a duration: a number in the unit of the column, a TimeSpan text or an ISO 8601 duration. Returns the fields of
// Intl.DurationFormat, or null.
function readDuration(value, unit) {
    if (typeof value === 'number') {
        return Number.isFinite(value) ? durationOfMilliseconds(unit === 'milliseconds' ? value : value * millisecondsPerSecond) : null;
    }

    if (typeof value !== 'string') return null;

    let match = timeSpanPattern.exec(value);
    if (match) {
        const [, negative, days, hours, minutes, seconds, fraction] = match;
        return withSign({
            days: Number(days ?? 0),
            hours: Number(hours),
            minutes: Number(minutes),
            seconds: Number(seconds),
            milliseconds: fractionMilliseconds(fraction),
        }, negative ? -1 : 1);
    }

    match = isoDurationPattern.exec(value);
    if (match) {
        const [, negative, years, months, weeks, days, hours, minutes, seconds] = match;
        const wholeSeconds = Math.floor(Number(seconds ?? 0));
        return withSign({
            years: Number(years ?? 0),
            months: Number(months ?? 0),
            weeks: Number(weeks ?? 0),
            days: Number(days ?? 0),
            hours: Number(hours ?? 0),
            minutes: Number(minutes ?? 0),
            seconds: wholeSeconds,
            milliseconds: Math.round((Number(seconds ?? 0) - wholeSeconds) * millisecondsPerSecond),
        }, negative ? -1 : 1);
    }

    return null;
}

// the fields that are not zero; a zero duration keeps its seconds, so that it shows as "0 sec"
function significantFields(duration) {
    const fields = {};
    for (const field of durationFields) {
        if (duration[field]) fields[field] = duration[field];
    }

    return Object.keys(fields).length > 0 ? fields : { seconds: 0 };
}

// a clock text, for browsers without Intl.DurationFormat: hours (with the days), minutes, seconds
function digitalDuration(fields, args) {
    const sign = durationFields.some(field => fields[field] < 0) ? '-' : '';
    const hours = Math.abs((fields.days ?? 0) * 24 + (fields.hours ?? 0));
    const minutes = String(Math.abs(fields.minutes ?? 0)).padStart(2, '0');
    const seconds = String(Math.abs(fields.seconds ?? 0)).padStart(2, '0');
    return `${sign}${plainNumberFormat(args).format(hours)}:${minutes}:${seconds}`;
}

function duration(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const read = readDuration(value, args.unit);
    if (!read) return String(value);

    const fields = significantFields(read);
    const style = args.style ?? durationDefaultStyle;
    if (typeof Intl.DurationFormat === 'function') {
        // a zero duration has no unit to show, unless the seconds always show
        const zero = Object.values(fields).every(amount => amount === 0);
        return columnCached(args, zero ? 'duration:zero' : 'duration', () =>
            createFormat(Intl.DurationFormat, args, zero ? { style, secondsDisplay: 'always' } : { style }, 'duration')).format(fields);
    }

    if (style === durationDigitalStyle) return digitalDuration(fields, args);

    // fallback: each unit with Intl.NumberFormat, joined as a list of units
    const parts = Object.entries(fields).map(([field, amount]) => columnCached(args, `duration:${field}`, () =>
        createFormat(Intl.NumberFormat, args, { style: 'unit', unit: durationUnits[field], unitDisplay: style }, 'duration')).format(amount));
    const list = columnCached(args, 'duration:list', () => typeof Intl.ListFormat === 'function'
        ? createFormat(Intl.ListFormat, args, { type: 'unit', style }, 'duration')
        : { format: items => items.join(' ') });
    return list.format(parts);
}

/* ------------------------------------------------------------------------------------------------------------------
 * Lookups, booleans, templates and lists
 * ------------------------------------------------------------------------------------------------------------------ */

function lookupPreset(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const key = String(value);
    return lookup(args.labels, key) ?? args.fallback ?? key;
}

function booleanPreset(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (value === true || value === 'true') return args.trueText ?? String(value);
    if (value === false || value === 'false') return args.falseText ?? String(value);
    return isEmpty(value) ? nullTextOf(args) : String(value);
}

// Parses a template into text parts and placeholders ({ path, format }). The C# factories check the grammar
// (CellTemplate.cs); an unmatched brace here stays text.
function parseTemplate(template) {
    const parts = [];
    let text = '';
    for (let i = 0; i < template.length; i++) {
        const c = template[i];
        if ((c === '{' || c === '}') && template[i + 1] === c) {
            text += c;
            i++;
            continue;
        }

        const end = c === '{' ? template.indexOf('}', i + 1) : -1;
        if (end < 0) {
            text += c;
            continue;
        }

        if (text) parts.push(text);
        text = '';
        const body = template.slice(i + 1, end);
        const separator = body.indexOf(':');
        parts.push({
            path: (separator < 0 ? body : body.slice(0, separator)).split('.'),
            format: separator < 0 ? null : body.slice(separator + 1),
        });
        i = end;
    }

    if (text) parts.push(text);
    return parts;
}

// the text of a placeholder value with a named format (CellTemplate.Formats)
function formatPlaceholder(value, format, args) {
    switch (format) {
        case 'number': {
            const n = toNumber(value);
            return Number.isFinite(n) ? columnCached(args, 'template:number', () => createFormat(Intl.NumberFormat, args, {}, 'number')).format(n) : String(value);
        }
        case 'percent': {
            const n = toNumber(value);
            return Number.isFinite(n) ? columnCached(args, 'template:percent', () => createFormat(Intl.NumberFormat, args, { style: 'percent' }, 'percent')).format(n) : String(value);
        }
        case 'date':
            return formatDateValue('date', value, noStyles(args));
        case 'datetime':
            return formatDateValue('dateTime', value, noStyles(args));
        case 'time':
            return formatDateValue('time', value, noStyles(args));
        default:
            return String(value);
    }
}

// the options of the template formats: the culture of the column, the default styles, the browser time zone
function noStyles(args) {
    return columnCached(args, 'template:dateOptions', () => ({ culture: args.culture }));
}

// Renders a template for a row. Returns null when the template has placeholders and all of them are empty.
function renderTemplate(template, row, args) {
    const parts = columnCached(args, `template:${template}`, () => parseTemplate(template));
    let text = '';
    let placeholders = 0;
    let filled = 0;
    for (const part of parts) {
        if (typeof part === 'string') {
            text += part;
            continue;
        }

        placeholders++;
        const value = readPath(row, part.path);
        if (isEmpty(value)) continue;

        filled++;
        text += part.format ? formatPlaceholder(value, part.format, args) : String(value);
    }

    return placeholders > 0 && filled === 0 ? null : text;
}

function templatePreset(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    return args.template ? renderTemplate(args.template, row, args) ?? nullTextOf(args) : nullTextOf(args);
}

function list(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (!Array.isArray(value)) return isEmpty(value) ? nullTextOf(args) : String(value);

    const items = value.filter(item => !isEmpty(item)).map(String);
    return items.length > 0 ? items.join(args.separator ?? defaultListSeparator) : nullTextOf(args);
}

/* ------------------------------------------------------------------------------------------------------------------
 * Badges, tags and icons
 * ------------------------------------------------------------------------------------------------------------------ */

function createIcon(spec) {
    const icon = document.createElement('wa-icon');
    const { name, family, variant, library } = typeof spec === 'string' ? { name: spec } : spec;
    icon.setAttribute('name', name);
    if (family) icon.setAttribute('family', family);
    if (variant) icon.setAttribute('variant', variant);
    if (library) icon.setAttribute('library', library);
    return icon;
}

// sets the variant, appearance, pill and size options of a wa-badge or wa-tag
function applyMarkerOptions(element, key, args) {
    const variant = lookup(args.variants, key) ?? args.fallback;
    if (variant) element.setAttribute('variant', variant);
    if (args.appearance) element.setAttribute('appearance', args.appearance);
    if (args.pill) element.setAttribute('pill', '');
    if (args.size) element.setAttribute('size', args.size);
}

function createTag(item, args) {
    const key = String(item);
    const tag = document.createElement('wa-tag');
    applyMarkerOptions(tag, key, args);
    tag.textContent = lookup(args.labels, key) ?? key;
    return tag;
}

function badge(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    return cachedNode(row, args, [value], () => {
        const key = String(value);
        const element = document.createElement('wa-badge');
        applyMarkerOptions(element, key, args);

        // the badge always shows a text, so its color is never the only signal; the icon is decoration
        element.textContent = lookup(args.labels, key) ?? key;
        const icon = lookup(args.icons, key);
        if (icon) {
            const iconElement = createIcon(icon);
            iconElement.setAttribute('slot', 'start');
            element.prepend(iconElement);
        }

        return element;
    });
}

function tag(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    return cachedNode(row, args, [value], () => createTag(value, args));
}

function tags(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const items = Array.isArray(value) ? value.filter(item => !isEmpty(item)) : (isEmpty(value) ? [] : [value]);
    if (items.length === 0) return nullTextOf(args);

    return cachedNode(row, args, [JSON.stringify(items)], () => {
        const container = document.createElement('span');
        container.setAttribute('style', 'display: inline-flex; flex-wrap: wrap; gap: var(--wa-space-2xs);');
        for (const item of items) container.appendChild(createTag(item, args));
        return container;
    });
}

function icon(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const key = String(value);
    const label = lookup(args.labels, key) ?? key;
    const spec = lookup(args.icons, key) ?? args.fallback;
    if (!spec) return label;

    return cachedNode(row, args, [value], () => {
        const element = createIcon(spec);

        // an icon alone in a cell needs an accessible name
        element.setAttribute('label', label);
        const color = lookup(args.colors, key);
        if (variants.includes(color)) element.setAttribute('style', `color: var(--wa-color-${color}-on-quiet);`);
        return element;
    });
}

/* ------------------------------------------------------------------------------------------------------------------
 * Links and cell actions
 * ------------------------------------------------------------------------------------------------------------------ */

// the text of a link: the text template, else the value; null when there is none
function linkText(value, row, args) {
    if (args.text) return renderTemplate(args.text, row, args);
    return isEmpty(value) ? null : String(value);
}

// the URL of a link template, with each placeholder value URL-encoded; null when a placeholder is empty, because the URL
// would then name another resource ("/orders/" instead of "/orders/7")
function buildHref(template, row, args) {
    const parts = columnCached(args, `href:${template}`, () => parseTemplate(template));
    let href = '';
    for (const part of parts) {
        if (typeof part === 'string') {
            href += part;
            continue;
        }

        const value = readPath(row, part.path);
        if (isEmpty(value)) return null;
        href += encodeURIComponent(String(value));
    }

    return href;
}

function isAllowedUrl(href) {
    try {
        return linkProtocols.includes(new URL(href, document.baseURI).protocol);
    } catch {
        return false;
    }
}

function createLink(href, text, target) {
    const anchor = document.createElement('a');
    anchor.setAttribute('href', href);
    anchor.setAttribute('style', linkStyle);
    if (target) {
        anchor.setAttribute('target', target);
        if (target === newContextTarget) anchor.setAttribute('rel', newContextRel);
    }

    anchor.textContent = text;
    return anchor;
}

// a link node for a URL, or the text alone when the URL has a scheme that is not allowed
function linkOrText(row, args, href, text, target) {
    if (!isAllowedUrl(href)) {
        reportOnce(`a link URL with a scheme other than http, https, mailto or tel is shown as text (${href.slice(0, 40)}).`);
        return text;
    }

    return cachedNode(row, args, [href, text], () => createLink(href, text, target));
}

function link(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const text = linkText(value, row, args);
    if (text === null || text === '') return nullTextOf(args);

    const href = buildHref(args.href ?? '', row, args);
    return href === null ? text : linkOrText(row, args, href, text, args.target);
}

function email(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const address = String(value).trim();
    const text = linkText(value, row, args) ?? address;
    const at = address.lastIndexOf('@');
    if (at <= 0 || at === address.length - 1) return text;

    // each part encoded, so that "?" or "&" in the value cannot add headers to the mail
    const href = `mailto:${encodeURIComponent(address.slice(0, at))}@${encodeURIComponent(address.slice(at + 1))}`;
    return linkOrText(row, args, href, text);
}

function phone(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const number = String(value);
    const text = linkText(value, row, args) ?? number;
    const dialable = number.replace(/[^0-9+*#]/g, '');
    if (!/\d/.test(dialable)) return text;

    // "#" starts the fragment of a URL
    return linkOrText(row, args, `tel:${dialable.replaceAll('#', '%23')}`, text);
}

/*
 * The cell action bridge (design 9.2-9.4). Per grid, a capture listener on the shadow root keeps the click in flight. A
 * click that raises an action, that hits a declarative action (data-wablazor-action), or that hits a control a preset
 * renders (a menu trigger, a switch) does not reach Blazor as wa-cell-click (decision D4): a wa-cell-click listener on
 * the grid host stops the propagation of that event, so Blazor, which listens on the document, does not see it, while
 * listeners on the grid element still do. The click ends when it reaches the host, or one task later.
 */

// the attributes of a declarative action in the output of a custom function (design 9.3, decision D5)
const actionAttribute = 'data-wablazor-action';
const actionPayloadAttribute = 'data-wablazor-action-payload';

// grid -> { click }: the click in flight in the grid's shadow root ({ action, raised, consumed }), or null
const bridges = new WeakMap();

/**
 * Installs the cell action bridge of a grid once: the listeners that apply decision D4 and pair a declarative action
 * with the wa-cell-click of its click. Call it when content that can raise an action is created, before the first
 * click; the presets and the ctx helpers do.
 * @param {HTMLElement} grid - The wa-data-grid element
 * @returns {object|undefined} The bridge state, or undefined without a grid
 */
export function ensureCellActionBridge(grid) {
    if (!grid || typeof grid.addEventListener !== 'function') return undefined;

    let bridge = bridges.get(grid);
    if (bridge) return bridge;

    bridge = { click: null };
    bridges.set(grid, bridge);
    grid.addEventListener('wa-cell-click', event => pairCellClick(grid, bridge, event));
    grid.addEventListener('click', () => endClick(bridge));
    watchShadowRoot(grid, bridge);
    return bridge;
}

// the capture listener on the grid's shadow root, once the grid has one
function watchShadowRoot(grid, bridge) {
    const listen = root => root.addEventListener('click', event => beginClick(root, bridge, event), { capture: true });
    if (grid.shadowRoot) {
        listen(grid.shadowRoot);
        return;
    }

    const tag = grid.localName;
    if (!tag?.includes('-')) return;
    customElements.whenDefined(tag)
        .then(() => grid.updateComplete)
        .then(() => {
            if (grid.shadowRoot) listen(grid.shadowRoot);
        })
        .catch(() => { /* an element that never upgrades raises no actions */ });
}

function beginClick(root, bridge, event) {
    const click = { action: declarativeAction(root, event), raised: false, consumed: false };
    bridge.click = click;

    // a click whose propagation something stopped never reaches the host: end it one task later at the latest
    setTimeout(() => {
        if (bridge.click === click) bridge.click = null;
    });
}

// the nearest element with a declarative action on the path of a click inside the shadow root, as { action, payload }
function declarativeAction(root, event) {
    for (const target of event.composedPath()) {
        if (target === root) return null;
        if (!(target instanceof Element) || !target.hasAttribute(actionAttribute)) continue;

        const action = target.getAttribute(actionAttribute);
        if (!action) return null;

        return { action, payload: readActionPayload(target), source: target };
    }

    return null;
}

function readActionPayload(element) {
    const text = element.getAttribute(actionPayloadAttribute);
    if (text === null) return null;

    try {
        return JSON.parse(text);
    } catch {
        reportOnce(`the ${actionPayloadAttribute} of the action '${element.getAttribute(actionAttribute)}' is not JSON; the action has no payload.`);
        return null;
    }
}

// the wa-cell-click of the click in flight: a declarative action takes its column, row, value and row index; a click
// that raised an action stops here (D4)
function pairCellClick(grid, bridge, event) {
    const click = bridge.click;
    if (!click) return;

    if (click.action && !click.consumed) {
        click.consumed = true;
        click.raised = true;
        const detail = event.detail ?? {};
        dispatchCellAction(grid, {
            column: detail.column,
            action: click.action.action,
            rowKey: rowKeyOf(grid, detail.row),
            row: detail.row ?? null,
            value: detail.value,
            payload: click.action.payload,
        }, detail.rowIndex);
    }

    if (click.raised) event.stopPropagation();
}

function endClick(bridge) {
    const click = bridge.click;
    bridge.click = null;
    if (click?.action && !click.consumed) {
        console.warn(`${logPrefix} the declarative action '${click.action.action}' was dropped: its click raised no wa-cell-click (a detail panel, a footer or a group row). Use ctx.emit there.`);
    }
}

// the key of a row as the grid reads it (its rowKey field)
function rowKeyOf(grid, row) {
    const key = grid?.rowKey;
    return key && row !== null && typeof row === 'object' ? row[key] ?? null : null;
}

function rowIndexOf(source) {
    const element = source instanceof Event ? source.composedPath().find(target => target instanceof Element) : source;
    const cell = element instanceof Element ? element.closest('[data-row-index]') : null;
    const rowIndex = cell ? Number(cell.getAttribute('data-row-index')) : unknownRowIndex;
    return Number.isInteger(rowIndex) ? rowIndex : unknownRowIndex;
}

function dispatchCellAction(grid, detail, rowIndex) {
    grid.dispatchEvent(new CustomEvent(cellActionEventName, {
        bubbles: true,
        composed: true,
        detail: { ...detail, rowIndex: Number.isInteger(rowIndex) ? rowIndex : unknownRowIndex },
    }));
}

/**
 * Marks the click in flight of a grid as a click on a control (a menu trigger, a switch), so that its wa-cell-click does
 * not reach Blazor (D4). Call it from a click listener of the control.
 * @param {HTMLElement} grid - The wa-data-grid element
 */
export function markControlClick(grid) {
    const bridge = bridges.get(grid);
    if (bridge?.click) bridge.click.raised = true;
}

/**
 * Raises a cell action: dispatches `wablazor-cell-action` on the grid element. When a click in the grid raised it, the
 * wa-cell-click of that click does not reach Blazor (D4).
 * @param {HTMLElement} grid - The wa-data-grid element
 * @param {object} detail - { column, action, rowKey, row, value, payload }
 * @param {Element|Event} [source] - The element in the cell that raised the action, or its event; its cell gives the
 *        row index (-1 without it)
 */
export function raiseCellAction(grid, detail, source) {
    if (!grid) return;

    const bridge = ensureCellActionBridge(grid);
    if (bridge?.click) bridge.click.raised = true;
    dispatchCellAction(grid, detail, source ? rowIndexOf(source) : unknownRowIndex);
}

function actionLink(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const text = linkText(value, row, args);
    if (text === null || text === '') return nullTextOf(args);

    const { grid, column, rowKey } = ctx;
    ensureCellActionBridge(grid);
    return cachedNode(row, args, [value, text], () => {
        // a button: the element performs an action, it does not navigate
        const button = document.createElement('button');
        button.setAttribute('type', 'button');
        button.setAttribute('style', actionLinkStyle);
        button.textContent = text;
        button.addEventListener('click', () => raiseCellAction(grid, {
            column,
            action: args.action,
            rowKey,
            row,
            value,
            payload: null,
        }, button));
        return button;
    });
}

/* ------------------------------------------------------------------------------------------------------------------
 * Comparators
 * ------------------------------------------------------------------------------------------------------------------ */

function ordinal(a, b, rowA, rowB, ctx) {
    const args = ctx.args ?? noOptions;
    const positions = columnCached(args, 'ordinal', () => new Map((args.values ?? []).map((value, index) => [value, index])));
    const rank = value => (value === null || value === undefined ? undefined : positions.get(String(value))) ?? positions.size;

    const rankA = rank(a);
    const rankB = rank(b);
    if (rankA !== rankB) return rankA - rankB;
    if (rankA < positions.size) return 0;

    // values that are not in the list sort among themselves by their text
    const textA = a === null || a === undefined ? '' : String(a);
    const textB = b === null || b === undefined ? '' : String(b);
    return textA < textB ? -1 : textA > textB ? 1 : 0;
}

function textOf(value) {
    return value === null || value === undefined ? '' : String(value);
}

function collation(a, b, rowA, rowB, ctx) {
    const args = ctx.args ?? noOptions;
    const collator = columnCached(args, 'collator', () => {
        const options = {};
        if (args.sensitivity) options.sensitivity = args.sensitivity;
        if (args.numeric) options.numeric = true;
        if (args.ignorePunctuation) options.ignorePunctuation = true;
        return createFormat(Intl.Collator, args, options, 'collation');
    });

    return collator.compare(textOf(a), textOf(b));
}

/* ------------------------------------------------------------------------------------------------------------------
 * Interactive presets: buttons, actions, menus, switches and checkboxes (design 6.4)
 * ------------------------------------------------------------------------------------------------------------------ */

// the icon of the menu trigger, and the default action of a switch or a checkbox (WaDataGridFormatters in C#)
const menuIconName = 'ellipsis-vertical';
const defaultToggleAction = 'toggle';

// the style of a group of controls in one cell, and of a text that only assistive technology reads
const controlGroupStyle = 'display: inline-flex; align-items: center; gap: var(--wa-space-2xs);';
const visuallyHiddenStyle = 'position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; '
    + 'clip: rect(0 0 0 0); white-space: nowrap; border: 0;';

// a counter for the ids that connect a wa-tooltip with its button in the grid's shadow root
let tooltipIdCounter = 0;

// whether a row field on a dotted path holds a true value (DisabledField, HiddenField)
function rowFlag(row, path) {
    return !!path && !!readPath(row, path.split('.'));
}

// the element of a group of controls, or the single control
function groupOf(nodes) {
    if (nodes.length === 1) return nodes[0];

    const group = document.createElement('span');
    group.setAttribute('style', controlGroupStyle);
    group.append(...nodes);
    return group;
}

// Creates a wa-button that runs onClick unless it is disabled. With iconOnly, the label is the accessible name of the
// icon; else the icon is in the start slot and the label is the text.
function createButton(spec, onClick) {
    const button = document.createElement('wa-button');
    if (spec.variant) button.setAttribute('variant', spec.variant);
    if (spec.appearance) button.setAttribute('appearance', spec.appearance);
    if (spec.size) button.setAttribute('size', spec.size);
    if (spec.disabled) button.setAttribute('disabled', '');

    if (spec.icon) {
        const icon = createIcon(spec.icon);
        if (spec.iconOnly) icon.setAttribute('label', spec.label);
        else icon.setAttribute('slot', 'start');
        button.append(icon);
    }

    if (!spec.icon || !spec.iconOnly) button.append(document.createTextNode(spec.label));
    button.addEventListener('click', () => {
        if (!button.disabled && !button.hasAttribute('disabled')) onClick(button);
    });
    return button;
}

// a wa-tooltip for a button; both go into the same container, so the id resolves in the same shadow root
function createTooltip(button, text) {
    const id = `wablazor-action-${++tooltipIdCounter}`;
    button.setAttribute('id', id);
    const tooltip = document.createElement('wa-tooltip');
    tooltip.setAttribute('for', id);
    tooltip.textContent = text;
    return tooltip;
}

// the detail of an action of a cell: its column, row and value
function actionDetail(ctx, row, value, action, payload) {
    return { column: ctx.column, action, rowKey: ctx.rowKey ?? null, row, value, payload: payload ?? null };
}

function button(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (rowFlag(row, args.hiddenField)) return nullTextOf(args);

    const label = args.label ? renderTemplate(args.label, row, args) : (isEmpty(value) ? null : String(value));
    if (!label) return nullTextOf(args);

    const disabled = rowFlag(row, args.disabledField);
    const { grid } = ctx;
    ensureCellActionBridge(grid);
    return cachedNode(row, args, [value, label, disabled], () => createButton(
        { label, icon: args.icon, iconOnly: args.iconOnly, variant: args.variant, appearance: args.appearance, size: args.size, disabled },
        source => raiseCellAction(grid, actionDetail(ctx, row, value, args.action), source)));
}

// the actions of a row: the items that are not hidden, each with its disabled state
function visibleActions(items, row) {
    return (items ?? [])
        .filter(item => !rowFlag(row, item.hiddenField))
        .map(item => ({ ...item, disabled: rowFlag(row, item.disabledField) }));
}

// the key of the node of a list of actions: the visible items and their disabled states
function actionsKey(items) {
    return items.map(item => `${item.id}${item.disabled ? ':disabled' : ''}`).join('|');
}

// Creates a wa-dropdown with a trigger button and one wa-dropdown-item per action. The selection raises the action of
// the item; a click on the trigger or in the menu does not raise OnCellClick (D4).
function createMenu(grid, items, args, onSelect) {
    const dropdown = document.createElement('wa-dropdown');
    const trigger = document.createElement('wa-button');
    trigger.setAttribute('slot', 'trigger');
    if (args.appearance) trigger.setAttribute('appearance', args.appearance);
    if (args.size) trigger.setAttribute('size', args.size);
    const icon = createIcon(menuIconName);
    icon.setAttribute('label', args.menuLabel ?? '');
    trigger.append(icon);
    dropdown.append(trigger);

    for (const item of items) {
        const element = document.createElement('wa-dropdown-item');
        element.setAttribute('value', item.id);
        if (item.variant === 'danger') element.setAttribute('variant', 'danger');
        if (item.disabled) element.setAttribute('disabled', '');
        if (item.icon) {
            const itemIcon = createIcon(item.icon);
            itemIcon.setAttribute('slot', 'icon');
            element.append(itemIcon);
        }

        element.append(document.createTextNode(item.label));
        dropdown.append(element);
    }

    dropdown.addEventListener('click', () => markControlClick(grid));
    dropdown.addEventListener('wa-select', event => {
        const item = event.detail?.item;
        const id = item?.getAttribute?.('value');
        if (id && !item.disabled) onSelect(id, item);
    });
    return dropdown;
}

function actions(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const items = visibleActions(args.items, row);
    if (items.length === 0) return nullTextOf(args);

    const { grid } = ctx;
    ensureCellActionBridge(grid);
    return cachedNode(row, args, [actionsKey(items)], () => {
        const raise = (id, source) => raiseCellAction(grid, actionDetail(ctx, row, value, id), source);
        const nodes = [];
        for (const item of items.filter(item => !item.inMenu)) {
            const control = createButton(
                { label: item.label, icon: item.icon, iconOnly: !!item.icon, variant: item.variant, appearance: args.appearance, size: args.size, disabled: item.disabled },
                source => raise(item.id, source));
            nodes.push(control);
            if (item.tooltip) nodes.push(createTooltip(control, item.tooltip));
        }

        const menuItems = items.filter(item => item.inMenu);
        if (menuItems.length > 0) nodes.push(createMenu(grid, menuItems, args, raise));
        return groupOf(nodes);
    });
}

function menu(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const items = visibleActions(args.items, row);
    if (items.length === 0) return nullTextOf(args);

    const { grid } = ctx;
    ensureCellActionBridge(grid);
    return cachedNode(row, args, [actionsKey(items)], () =>
        createMenu(grid, items, args, (id, source) => raiseCellAction(grid, actionDetail(ctx, row, value, id), source)));
}

// A wa-switch or wa-checkbox for a boolean value. The change raises the action with { checked }; the click before it
// does not raise OnCellClick. The element shows the new state at once: the app updates its row and pushes the data
// (design 9.5), and data with the old state shows the old state again.
function togglePreset(tagName) {
    return (value, row, ctx) => {
        const args = ctx.args ?? noOptions;
        const checked = value === true || value === 'true';
        const disabled = rowFlag(row, args.disabledField);
        const label = args.label ? renderTemplate(args.label, row, args) ?? '' : '';
        const { grid } = ctx;
        ensureCellActionBridge(grid);
        return cachedNode(row, args, [checked, disabled, label], () => {
            const element = document.createElement(tagName);
            if (checked) element.setAttribute('checked', '');
            if (disabled) element.setAttribute('disabled', '');
            if (args.size) element.setAttribute('size', args.size);
            if (label) {
                const text = document.createElement('span');
                if (!args.showLabel) text.setAttribute('style', visuallyHiddenStyle);
                text.textContent = label;
                element.append(text);
            }

            element.addEventListener('click', () => markControlClick(grid));
            element.addEventListener('change', () => raiseCellAction(grid,
                actionDetail(ctx, row, value, args.action ?? defaultToggleAction, { checked: !!element.checked }), element));
            return element;
        });
    };
}

/* ------------------------------------------------------------------------------------------------------------------
 * More visual presets (design 6.3)
 * ------------------------------------------------------------------------------------------------------------------ */

// the URL schemes an image may have; a relative URL resolves to the scheme of the page
const imageProtocols = ['http:', 'https:'];

function isAllowedImageUrl(src) {
    try {
        return imageProtocols.includes(new URL(src, document.baseURI).protocol);
    } catch {
        return false;
    }
}

// the variant of a value by thresholds ({ from, variant }, ascending): the last threshold the value reaches
function thresholdVariant(thresholds, n) {
    let variant;
    for (const threshold of thresholds ?? []) {
        if (n >= threshold.from) variant = threshold.variant;
    }

    return variants.includes(variant) ? variant : undefined;
}

// a progress element (wa-progress-bar or wa-progress-ring) for a value in the scale of the column
function progressPreset(tagName) {
    return (value, row, ctx) => {
        const args = ctx.args ?? noOptions;
        if (isEmpty(value)) return nullTextOf(args);

        const n = toNumber(value);
        if (!Number.isFinite(n)) return String(value);

        const fraction = args.scale === 'percent' ? n / 100 : n;
        const text = percentFormat(args).format(fraction);
        return cachedNode(row, args, [n], () => {
            const element = document.createElement(tagName);
            element.setAttribute('value', String(Math.min(100, Math.max(0, fraction * 100))));
            element.setAttribute('label', text);
            if (args.showLabel) element.textContent = text;
            const variant = thresholdVariant(args.thresholds, n);
            if (variant) element.style.setProperty('--indicator-color', `var(--wa-color-${variant}-fill-loud)`);
            return element;
        });
    };
}

function rating(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    return cachedNode(row, args, [n], () => {
        const element = document.createElement('wa-rating');
        element.setAttribute('readonly', '');
        element.setAttribute('value', String(n));
        if (args.max !== undefined) element.setAttribute('max', String(args.max));
        if (args.precision !== undefined) element.setAttribute('precision', String(args.precision));
        if (args.size) element.setAttribute('size', args.size);
        const max = args.max ?? 5;
        element.setAttribute('label', `${plainNumberFormat(args).format(n)} / ${plainNumberFormat(args).format(max)}`);
        return element;
    });
}

function avatar(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const labelValue = args.labelField ? readPath(row, args.labelField.split('.')) : value;
    const imageValue = args.imageField ? readPath(row, args.imageField.split('.')) : undefined;
    const initialsValue = args.initialsField ? readPath(row, args.initialsField.split('.')) : undefined;
    if (isEmpty(labelValue) && isEmpty(imageValue) && isEmpty(initialsValue)) return nullTextOf(args);

    const label = textOf(labelValue);
    let image = textOf(imageValue);
    if (image && !isAllowedImageUrl(image)) {
        reportOnce(`an avatar image with a scheme other than http or https is left out (${image.slice(0, 40)}).`);
        image = '';
    }

    const initials = textOf(initialsValue);
    return cachedNode(row, args, [label, image, initials], () => {
        const element = document.createElement('wa-avatar');
        if (label) element.setAttribute('label', label);
        if (image) element.setAttribute('image', image);
        if (initials) element.setAttribute('initials', initials);
        if (args.shape) element.setAttribute('shape', args.shape);
        if (args.size) element.style.setProperty('--size', args.size);
        if (!args.showLabel || !label) return element;

        const text = document.createElement('span');
        text.textContent = label;
        return groupOf([element, text]);
    });
}

function image(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const src = args.src ? buildHref(args.src, row, args) : (isEmpty(value) ? null : String(value));
    if (!src) return nullTextOf(args);

    if (!isAllowedImageUrl(src)) {
        reportOnce(`an image URL with a scheme other than http or https is left out (${src.slice(0, 40)}).`);
        return nullTextOf(args);
    }

    const alt = args.alt ? renderTemplate(args.alt, row, args) ?? '' : '';
    return cachedNode(row, args, [src, alt], () => {
        const element = document.createElement('img');
        element.setAttribute('src', src);
        element.setAttribute('alt', alt);
        element.setAttribute('loading', 'lazy');
        if (args.width !== undefined) element.setAttribute('width', String(args.width));
        if (args.height !== undefined) element.setAttribute('height', String(args.height));
        element.setAttribute('style', 'vertical-align: middle; object-fit: cover;');
        return element;
    });
}

function colorSwatch(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const color = String(value);
    if (typeof CSS === 'undefined' || !CSS.supports('color', color)) return color;

    return cachedNode(row, args, [color], () => {
        const swatch = document.createElement('span');
        swatch.setAttribute('style', 'display: inline-block; inline-size: 1em; block-size: 1em; vertical-align: middle; '
            + 'border-radius: var(--wa-border-radius-s); border: 1px solid var(--wa-color-neutral-border-normal);');
        swatch.style.setProperty('background-color', color);
        if (!args.showText) {
            swatch.setAttribute('role', 'img');
            swatch.setAttribute('aria-label', color);
            return swatch;
        }

        const text = document.createElement('span');
        text.textContent = color;
        return groupOf([swatch, text]);
    });
}

// the trends of wa-sparkline
const sparklineTrends = ['positive', 'negative', 'neutral'];

function sparkline(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const values = (Array.isArray(value) ? value : []).map(toNumber).filter(Number.isFinite);
    if (values.length === 0) return nullTextOf(args);

    const data = values.join(' ');
    const rowTrend = args.trendField ? readPath(row, args.trendField.split('.')) : undefined;
    const trend = sparklineTrends.includes(rowTrend) ? rowTrend : args.trend;
    const label = args.label ? renderTemplate(args.label, row, args) ?? data : values.map(n => plainNumberFormat(args).format(n)).join(', ');
    return cachedNode(row, args, [data, trend, label], () => {
        const element = document.createElement('wa-sparkline');
        element.setAttribute('data', data);
        element.setAttribute('label', label);
        if (trend) element.setAttribute('trend', trend);
        if (args.appearance) element.setAttribute('appearance', args.appearance);
        if (args.curve) element.setAttribute('curve', args.curve);
        return element;
    });
}

// the text of a value with a text preset of the C# factories ({ name, args }), else with the plain number format
function formatWith(format, value, row, args) {
    const preset = format ? presets[format.name] : undefined;
    if (preset) return preset(value, row, { args: format.args ?? noOptions });

    const n = toNumber(value);
    return Number.isFinite(n) ? plainNumberFormat(args).format(n) : textOf(value);
}

function trend(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    const text = args.format
        ? formatWith(args.format, n, row, args)
        : columnCached(args, 'trend', () => createFormat(Intl.NumberFormat, args, { signDisplay: 'exceptZero' }, 'trend')).format(n);
    const direction = Math.sign(n);
    return cachedNode(row, args, [n, text], () => {
        const element = document.createElement('span');
        element.setAttribute('style', controlGroupStyle);
        const variant = direction > 0 ? args.positiveVariant : direction < 0 ? args.negativeVariant : undefined;
        if (variants.includes(variant)) element.style.setProperty('color', `var(--wa-color-${variant}-on-quiet)`);

        // the text carries the value and its sign, so the icon is decoration
        const iconSpec = direction > 0 ? args.upIcon : direction < 0 ? args.downIcon : undefined;
        if (iconSpec) element.append(createIcon(iconSpec));
        element.append(document.createTextNode(text));
        return element;
    });
}

// the column of a grid by the id the grid computes (id, else field, else col-<index>)
function findColumn(grid, columnId) {
    const columns = Array.isArray(grid?.columns) ? grid.columns : [];
    return columns.find((column, index) => (column.id ?? column.field ?? `col-${index}`) === columnId);
}

// the value of a column for a row: its value function, else its field
function columnValue(column, row) {
    if (typeof column?.value === 'function') return column.value(row);
    return column?.field ? readPath(row, String(column.field).split('.')) : undefined;
}

// the finite numbers of a column over rows
function columnNumbers(column, rows) {
    const numbers = [];
    for (const row of rows ?? []) {
        const n = toNumber(columnValue(column, row));
        if (Number.isFinite(n)) numbers.push(n);
    }

    return numbers;
}

// the limits of a data bar: fixed, or the lowest and highest value of the loaded rows (grid.data), kept per data array
function dataBarLimits(args, ctx) {
    if (args.min !== undefined && args.max !== undefined) return { min: args.min, max: args.max };

    const grid = ctx.grid;
    const data = grid?.data;
    const cached = columnCached(args, 'dataBar:limits', () => ({ data: undefined, min: 0, max: 0 }));
    if (cached.data !== data) {
        const numbers = columnNumbers(findColumn(grid, ctx.column), data);
        cached.data = data;
        cached.min = numbers.length > 0 ? Math.min(0, ...numbers) : 0;
        cached.max = numbers.length > 0 ? Math.max(...numbers) : 0;
    }

    return { min: args.min ?? cached.min, max: args.max ?? cached.max };
}

function dataBar(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const n = toNumber(value);
    if (!Number.isFinite(n)) return String(value);

    const { min, max } = dataBarLimits(args, ctx);
    const share = max > min ? Math.min(1, Math.max(0, (n - min) / (max - min))) : 0;
    const text = formatWith(args.format, n, row, args);
    return cachedNode(row, args, [n, min, max, text], () => {
        const container = document.createElement('div');
        container.setAttribute('style', 'display: flex; align-items: center; gap: var(--wa-space-xs); inline-size: 100%;');
        container.setAttribute('role', 'img');
        container.setAttribute('aria-label', text);

        const track = document.createElement('div');
        track.setAttribute('style', 'flex: 1 1 auto; block-size: 0.75em; background: var(--wa-color-neutral-fill-quiet); '
            + 'border-radius: var(--wa-border-radius-s); overflow: hidden;');
        const bar = document.createElement('div');
        bar.setAttribute('style', 'block-size: 100%;');
        bar.style.setProperty('inline-size', `${(share * 100).toFixed(2)}%`);
        bar.style.setProperty('background', `var(--wa-color-${variants.includes(args.variant) ? args.variant : 'brand'}-fill-loud)`);
        track.append(bar);
        container.append(track);

        if (args.showValue) {
            const label = document.createElement('span');
            label.setAttribute('aria-hidden', 'true');
            label.textContent = text;
            container.append(label);
        }

        return container;
    });
}

function copyButton(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    if (isEmpty(value)) return nullTextOf(args);

    const text = String(value);
    const { grid } = ctx;
    ensureCellActionBridge(grid);
    return cachedNode(row, args, [text], () => {
        const element = document.createElement('wa-copy-button');
        element.setAttribute('value', text);
        if (args.copyLabel) element.setAttribute('copy-label', args.copyLabel);
        if (args.successLabel) element.setAttribute('success-label', args.successLabel);
        element.addEventListener('click', () => markControlClick(grid));
        if (!args.showValue) return element;

        const label = document.createElement('span');
        label.textContent = text;
        return groupOf([label, element]);
    });
}

/* ------------------------------------------------------------------------------------------------------------------
 * Rules, footers, search, detail panels and row selection (design 7)
 * ------------------------------------------------------------------------------------------------------------------ */

// a value that can be compared with a bound of a rule: a number, or the time of a date (a DateOnly as UTC midnight)
function comparable(value, type) {
    if (type === 'date') return readDate(value)?.date.getTime() ?? NaN;
    return toNumber(value);
}

// Compiles one rule of a class rule list into { className, path, test }. The C# factories write the rules
// (WaDataGridCondition): { class, when, value?, values?, min?, max?, type?, pattern?, ignoreCase?, field? }.
function compileRule(rule) {
    const path = rule.field ? String(rule.field).split('.') : null;
    let test;
    switch (rule.when) {
        case 'equals':
            test = value => value !== null && value !== undefined && String(value) === rule.value;
            break;
        case 'in': {
            const keys = new Set(rule.values ?? []);
            test = value => value !== null && value !== undefined && keys.has(String(value));
            break;
        }
        case 'lessThan': {
            const bound = comparable(rule.value, rule.type);
            test = value => comparable(value, rule.type) < bound;
            break;
        }
        case 'greaterThan': {
            const bound = comparable(rule.value, rule.type);
            test = value => comparable(value, rule.type) > bound;
            break;
        }
        case 'between': {
            const min = comparable(rule.min, rule.type);
            const max = comparable(rule.max, rule.type);
            test = value => {
                const n = comparable(value, rule.type);
                return n >= min && n <= max;
            };
            break;
        }
        case 'isNull':
            test = value => value === null || value === undefined;
            break;
        case 'isTrue':
            test = value => value === true;
            break;
        case 'isFalse':
            test = value => value === false;
            break;
        case 'matches': {
            let pattern = null;
            try {
                pattern = new RegExp(rule.pattern, rule.ignoreCase ? 'i' : '');
            } catch (error) {
                reportOnce(`the class rule pattern '${rule.pattern}' is not a valid regular expression (${error.message}); the rule never matches.`);
            }

            test = value => pattern !== null && value !== null && value !== undefined && pattern.test(String(value));
            break;
        }
        default:
            reportOnce(`the class rule condition '${rule.when}' is unknown; the rule never matches.`);
            test = () => false;
    }

    return { className: String(rule.class ?? ''), path, test };
}

function compiledRules(args) {
    return columnCached(args, 'rules', () => (args.rules ?? []).map(compileRule));
}

function cellClassRules(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    for (const rule of compiledRules(args)) {
        if (rule.test(rule.path ? readPath(row, rule.path) : value)) return rule.className;
    }

    return '';
}

function rowClassRules(row, ctx) {
    const args = ctx.args ?? noOptions;
    for (const rule of compiledRules(args)) {
        if (rule.path && rule.test(readPath(row, rule.path))) return rule.className;
    }

    return null;
}

// the aggregates of the footer preset over the numbers of a column
const aggregates = {
    sum: numbers => numbers.reduce((sum, n) => sum + n, 0),
    average: numbers => numbers.length > 0 ? numbers.reduce((sum, n) => sum + n, 0) / numbers.length : NaN,
    min: numbers => numbers.length > 0 ? Math.min(...numbers) : NaN,
    max: numbers => numbers.length > 0 ? Math.max(...numbers) : NaN,
};

function footerAggregate(rows, ctx) {
    const args = ctx.args ?? noOptions;
    const list = Array.isArray(rows) ? rows : [];
    const result = args.aggregate === 'count'
        ? list.length
        : (aggregates[args.aggregate] ?? aggregates.sum)(columnNumbers(findColumn(ctx.grid, ctx.column), list));
    if (!Number.isFinite(result)) return nullTextOf(args);

    const text = formatWith(args.format, result, null, args);
    return args.label ? `${args.label} ${text}` : text;
}

// Whether the label of a value of one of the columns of the search preset contains the term. An array value (tags)
// matches when one of its items does.
function labelsContain(columns, row, needle) {
    for (const column of columns) {
        const value = readPath(row, column.path);
        for (const item of Array.isArray(value) ? value : [value]) {
            if (isEmpty(item)) continue;
            const label = lookup(column.labels, String(item)) ?? column.fallback;
            if (typeof label === 'string' && label.toLowerCase().includes(needle)) return true;
        }
    }

    return false;
}

function searchFormatted(value, searchTerm, row, ctx) {
    const args = ctx.args ?? noOptions;
    const needle = String(searchTerm ?? '').toLowerCase();
    if (needle === '') return true;

    // the default search of the grid: the raw value contains the term, without case
    if (!isEmpty(value) && String(value).toLowerCase().includes(needle)) return true;
    if (row === null || typeof row !== 'object') return false;

    // the grid calls the function once per searchable column of a row: the labels of the row are compared once per term
    const columns = columnCached(args, 'search:columns', () => (args.columns ?? []).map(column => ({ ...column, path: String(column.field).split('.') })));
    const results = columnCached(args, 'search:results', () => new WeakMap());
    const cached = results.get(row);
    if (cached?.needle === needle) return cached.result;

    const result = labelsContain(columns, row, needle);
    results.set(row, { needle, result });
    return result;
}

function detailFields(row, ctx) {
    const args = ctx.args ?? noOptions;
    return cachedNode(row, args, [], () => {
        const list = document.createElement('dl');
        list.setAttribute('style', 'display: grid; grid-template-columns: max-content 1fr; gap: var(--wa-space-2xs) var(--wa-space-m); '
            + 'margin: 0; padding: var(--wa-space-s) var(--wa-space-m);');
        for (const field of args.fields ?? []) {
            const term = document.createElement('dt');
            term.setAttribute('style', 'font-weight: var(--wa-font-weight-semibold);');
            term.textContent = field.label;

            const value = readPath(row, String(field.field).split('.'));
            const description = document.createElement('dd');
            description.setAttribute('style', 'margin: 0;');
            description.textContent = isEmpty(value) ? nullTextOf(args) : field.format ? textOf(formatWith(field.format, value, row, args)) : textOf(value);
            list.append(term, description);
        }

        return list;
    });
}

function selectableField(row, ctx) {
    const args = ctx.args ?? noOptions;
    return rowFlag(row, args.field);
}

/* ------------------------------------------------------------------------------------------------------------------
 * Event arguments and the preset table
 * ------------------------------------------------------------------------------------------------------------------ */

// recursively copies JSON-safe values, dropping DOM nodes, functions and anything too deep (as the main initializer)
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

/**
 * The .NET payload of a cell action event (WaDataGridCellActionEventArgs), built from the event detail.
 * @param {CustomEvent} event - The `wablazor-cell-action` event
 * @returns {object} { column, action, rowKey, row, value, payload, rowIndex }
 */
export function cellActionArgs(event) {
    const detail = event.detail ?? {};
    const row = detail.row !== null && typeof detail.row === 'object' && !Array.isArray(detail.row) ? sanitize(detail.row, rowDepth) : null;
    const rowKey = typeof detail.rowKey === 'string' || typeof detail.rowKey === 'number' ? detail.rowKey : null;
    return {
        column: detail.column == null ? '' : String(detail.column),
        action: detail.action == null ? '' : String(detail.action),
        rowKey,
        row,
        value: sanitize(detail.value, rowDepth) ?? null,
        payload: sanitize(detail.payload, rowDepth) ?? null,
        rowIndex: Number.isInteger(detail.rowIndex) ? detail.rowIndex : unknownRowIndex,
    };
}

/**
 * The preset functions by their registered names (PresetNames.cs in C#).
 */
export const presets = Object.freeze({
    'wab.dataGrid.number': number,
    'wab.dataGrid.currency': currency,
    'wab.dataGrid.percent': percent,
    'wab.dataGrid.date': datePreset('date'),
    'wab.dataGrid.dateTime': datePreset('dateTime'),
    'wab.dataGrid.time': datePreset('time'),
    'wab.dataGrid.relativeTime': relativeTime,
    'wab.dataGrid.bytes': bytes,
    'wab.dataGrid.duration': duration,
    'wab.dataGrid.lookup': lookupPreset,
    'wab.dataGrid.boolean': booleanPreset,
    'wab.dataGrid.template': templatePreset,
    'wab.dataGrid.list': list,
    'wab.dataGrid.badge': badge,
    'wab.dataGrid.tag': tag,
    'wab.dataGrid.tags': tags,
    'wab.dataGrid.icon': icon,
    'wab.dataGrid.link': link,
    'wab.dataGrid.actionLink': actionLink,
    'wab.dataGrid.email': email,
    'wab.dataGrid.phone': phone,
    'wab.dataGrid.button': button,
    'wab.dataGrid.actions': actions,
    'wab.dataGrid.menu': menu,
    'wab.dataGrid.switch': togglePreset('wa-switch'),
    'wab.dataGrid.checkbox': togglePreset('wa-checkbox'),
    'wab.dataGrid.progressBar': progressPreset('wa-progress-bar'),
    'wab.dataGrid.progressRing': progressPreset('wa-progress-ring'),
    'wab.dataGrid.rating': rating,
    'wab.dataGrid.avatar': avatar,
    'wab.dataGrid.image': image,
    'wab.dataGrid.colorSwatch': colorSwatch,
    'wab.dataGrid.sparkline': sparkline,
    'wab.dataGrid.trend': trend,
    'wab.dataGrid.dataBar': dataBar,
    'wab.dataGrid.copyButton': copyButton,
    'wab.dataGrid.ordinal': ordinal,
    'wab.dataGrid.collation': collation,
    'wab.dataGrid.cellClassRules': cellClassRules,
    'wab.dataGrid.rowClassRules': rowClassRules,
    'wab.dataGrid.footerAggregate': footerAggregate,
    'wab.dataGrid.searchFormatted': searchFormatted,
    'wab.dataGrid.detailFields': detailFields,
    'wab.dataGrid.selectableField': selectableField,
});

/**
 * The prefix of the registered preset names.
 */
export const presetPrefix = 'wab.dataGrid.';

/**
 * The text presets that format a value without its row: ctx.format offers them as plain functions (short names).
 */
export const textPresetNames = Object.freeze([
    'number', 'currency', 'percent', 'date', 'dateTime', 'time', 'relativeTime', 'bytes', 'duration', 'lookup', 'boolean',
    'list',
]);

export { cachedNode, reportOnce, isAllowedUrl };
