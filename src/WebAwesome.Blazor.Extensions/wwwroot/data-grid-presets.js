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
 * Rules (design: tasks/WA-3.14/WAB-49/datagrid-cell-rendering-design.md, sections 6, 7.3, 9 and 11):
 * - A text preset returns a string, so the CSV export and the copy of rows hold the formatted text. A visual preset
 *   returns an element and keeps it per row object and column while the value stays the same, so the element keeps its
 *   state when the grid renders again; new data from .NET brings new row objects, which empties the cache.
 * - Intl formatter objects are created once per column (per ctx.args), never per cell.
 * - The DOM is built with createElement, setAttribute and textContent only. No HTML text is parsed, so row data cannot
 *   become markup. Each icon-only element has an accessible name.
 * - A link URL comes from a C# template; each placeholder value is URL-encoded, and a URL whose scheme is not http,
 *   https, mailto or tel (or relative) renders as plain text.
 * - Content that raises a cell action dispatches `wablazor-cell-action` on the grid host (bubbles, composed). A click
 *   that raises an action does not reach Blazor as wa-cell-click (decision D4): a wa-cell-click listener on the grid
 *   host stops its propagation, so listeners on the grid element still see it, but Blazor, which listens on the
 *   document, does not.
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

// grid -> the token of the click that raised an action; the wa-cell-click of that click is stopped (decision D4)
const pendingActionClicks = new WeakMap();
const guardedGrids = new WeakSet();
let actionClickToken = 0;

// Installs the listeners on the grid host once: wa-cell-click of a click that raised an action stops at the host, so
// Blazor (a listener on the document) does not see it, and the mark ends when the click reaches the host.
function guardCellClicks(grid) {
    if (guardedGrids.has(grid)) return;
    guardedGrids.add(grid);

    grid.addEventListener('wa-cell-click', event => {
        if (!pendingActionClicks.has(grid)) return;
        pendingActionClicks.delete(grid);
        event.stopPropagation();
    });
    grid.addEventListener('click', () => pendingActionClicks.delete(grid));
}

/**
 * Raises a cell action: dispatches `wablazor-cell-action` on the grid element, and stops the wa-cell-click of the same
 * click before it reaches Blazor.
 * @param {HTMLElement} grid - The wa-data-grid element
 * @param {object} detail - { column, action, rowKey, row, value, payload }
 * @param {Element} source - The element in the cell that raised the action; its cell gives the row index
 */
export function raiseCellAction(grid, detail, source) {
    guardCellClicks(grid);

    // the click goes on to the cell, which dispatches wa-cell-click, and then to the host; a task later at the latest,
    // the mark ends, so that a later wa-cell-click of another interaction reaches Blazor
    const token = ++actionClickToken;
    pendingActionClicks.set(grid, token);
    setTimeout(() => {
        if (pendingActionClicks.get(grid) === token) pendingActionClicks.delete(grid);
    });

    const cell = source.closest('[data-row-index]');
    const rowIndex = cell ? Number(cell.getAttribute('data-row-index')) : unknownRowIndex;
    grid.dispatchEvent(new CustomEvent(cellActionEventName, {
        bubbles: true,
        composed: true,
        detail: { ...detail, rowIndex: Number.isInteger(rowIndex) ? rowIndex : unknownRowIndex },
    }));
}

function actionLink(value, row, ctx) {
    const args = ctx.args ?? noOptions;
    const text = linkText(value, row, args);
    if (text === null || text === '') return nullTextOf(args);

    const { grid, column, rowKey } = ctx;
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
    'wab.dataGrid.ordinal': ordinal,
});
