using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The patterns Web Awesome parses its date and time attributes with, copied from the 3.12.0 dist sources
/// (temp\wa-src\3.12.0\dist\chunks), so the render checks validate what the wrappers emit against the element's own
/// parser rather than against the wrapper's formatting code. Compiled with ECMAScript semantics, so \d is an ASCII
/// digit as in JavaScript. Re-verify the cited lines on every Web Awesome upgrade.
/// </summary>
internal static class WaWirePatterns
{
    /// <summary>
    /// A date: ISO_DATE of date-picker/internal/iso.ts (chunk.4RAXYMTU.js:4), which parseIsoDate applies to every
    /// date attribute (value, min, max, today, focused-date) and each item of disabled-dates.
    /// </summary>
    public static readonly Regex IsoDate = new(@"^(\d{4})-(\d{2})-(\d{2})$", RegexOptions.ECMAScript);

    /// <summary>
    /// A time: the pattern of wireToTimeSegments in time-input/internal/segments.ts (chunk.PR6VR6I7.js:198).
    /// </summary>
    public static readonly Regex WireTime = new(@"^(\d{1,2}):(\d{2})(?::(\d{2}(?:\.\d+)?))?$", RegexOptions.ECMAScript);

    /// <summary>
    /// The separator parseDisabledDates and parseDaysOfWeek split their lists on (matchers.ts,
    /// chunk.T2PA53U2.js:28 and :51).
    /// </summary>
    public static readonly Regex ListSeparator = new(@"\s+", RegexOptions.ECMAScript);

    /// <summary>
    /// The separator of the two dates of a range, which parseRange splits on (iso.ts, chunk.4RAXYMTU.js:36).
    /// </summary>
    public const char RangeSeparator = '/';

    /// <summary>
    /// The weekday tokens parseDaysOfWeek reads, the keys of WEEKDAY_NAMES (matchers.ts, chunk.T2PA53U2.js:40-48),
    /// indexed by their getDay() value (Sunday = 0, as <see cref="System.DayOfWeek"/>).
    /// </summary>
    public static readonly string[] WeekdayTokens = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"];

    /// <summary>
    /// An instant with its offset: the ECMAScript date-time string format (ECMA-262, Date Time String Format) with
    /// milliseconds and an explicit offset, the form <c>new Date(this.date)</c> reads as one instant in every time zone
    /// (wa-relative-time chunk.P2JUPGTG.js:46, wa-format-date chunk.6WKRMMNM.js:26); without the offset a date-time is
    /// read as browser-local time.
    /// </summary>
    public static readonly Regex EcmaScriptInstant = new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}(Z|[+-]\d{2}:\d{2})$", RegexOptions.ECMAScript);

    /// <summary>
    /// A local date and time, the HTML "valid normalized local date and time string" a native
    /// <c>&lt;input type="datetime-local"&gt;</c> reads its min and max in (HTML Living Standard, dates and times): no offset,
    /// the seconds and a fraction optional. wa-input passes min/max to its native input unchanged (chunk.3UR7XKQK.js:232-233).
    /// </summary>
    public static readonly Regex LocalDateTime = new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(?::\d{2}(?:\.\d{1,3})?)?$", RegexOptions.ECMAScript);

    /// <summary>
    /// The separator each list-valued attribute is split on by its element, keyed "tag:attribute", so the render check
    /// reads a rendered list the way the element reads it (3.12.0 dist chunks):
    /// - threshold: parseThreshold through parseSpaceDelimitedTokens, <c>input.split(" ")</c> (chunk.PVXOAR6W.js:58, chunk.TW3VXPTP.js:2)
    /// - zoom-levels: parseZoomLevels through parseSpaceDelimitedTokens (chunk.PJARYDTD.js:87)
    /// - attr: <c>this.attr.split(" ")</c> (chunk.KJH3JDJP.js:48)
    /// - data: <c>this.data.trim().split(/\s+/)</c> (chunk.7K4I5LYJ.js:26)
    /// - flip-fallback-placements: the converter's <c>value.split(" ")</c> (chunk.2YFBUTFX.js:396)
    /// - group-by: <c>this.groupBy.split(/[\s,]+/)</c> (chunk.545TV6Q6.js:330)
    /// - swatches: <c>this.swatches.split(";")</c> (chunk.6O6PWE4O.js:785)
    /// - accept: <c>this.accept.split(",")</c> (chunk.D5HSK7BD.js:230)
    /// A list parameter mapped to an attribute without an entry fails the check, so a new one needs its source cited here.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, Regex> ListSeparators = new Dictionary<string, Regex>(StringComparer.Ordinal)
    {
        ["wa-intersection-observer:threshold"] = SingleSpace,
        ["wa-zoomable-frame:zoom-levels"] = SingleSpace,
        ["wa-mutation-observer:attr"] = SingleSpace,
        ["wa-sparkline:data"] = ListSeparator,
        ["wa-popup:flip-fallback-placements"] = SingleSpace,
        ["wa-data-grid:group-by"] = new Regex(@"[\s,]+", RegexOptions.ECMAScript),
        ["wa-color-picker:swatches"] = new Regex(";", RegexOptions.ECMAScript),
        ["wa-file-input:accept"] = new Regex(",", RegexOptions.ECMAScript),
    };

    #region ------ Internals ------

    // a single space, the separator of split(" ")
    private static Regex SingleSpace => new(" ", RegexOptions.ECMAScript);

    #endregion
}
