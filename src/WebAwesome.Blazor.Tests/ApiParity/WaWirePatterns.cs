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
}
