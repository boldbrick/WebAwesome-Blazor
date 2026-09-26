using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// The culture-free wire formats Web Awesome parses its date and time attributes and values in, formatted and
/// parsed exactly as the element does (3.12.0 sources: dates in date-picker/internal/iso.ts, disabled dates and
/// weekdays in date-picker/internal/matchers.ts, times in time-input/internal/segments.ts). Every conversion uses
/// the invariant culture and an explicit pattern, never the current culture or ToString().
/// </summary>
/// <remarks>
/// Web Awesome builds a date as a local JS Date, which maps the years 0-99 to 1900-1999 and then rejects the date
/// as invalid; the years a date attribute can carry are therefore 100-9999. An earlier year renders, and the
/// element treats it as invalid.
/// </remarks>
internal static class WaWireFormat
{
    /// <summary>
    /// Formats a date as ISO <c>yyyy-MM-dd</c> (the year zero-padded to four digits, as formatIsoDate pads it).
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns>The wire string</returns>
    public static string FormatDate(DateOnly date) => date.ToString(DatePattern, CultureInfo.InvariantCulture);

    /// <summary>
    /// Parses a date the way parseIsoDate does: the trimmed text must be exactly four, two and two ASCII digits
    /// separated by '-' and name a real calendar day.
    /// </summary>
    /// <param name="value">The wire string</param>
    /// <param name="date">The parsed date</param>
    /// <returns>true when the text is a valid ISO date</returns>
    public static bool TryParseDate(string? value, out DateOnly date)
    {
        date = default;
        if (value is null) return false;

        var text = value.Trim();
        if (text.Length != DatePattern.Length) return false;

        for (var i = 0; i < text.Length; i++)
        {
            var isSeparator = i == FirstDateSeparatorIndex || i == SecondDateSeparatorIndex;
            if (isSeparator ? text[i] != DateSeparator : !char.IsAsciiDigit(text[i])) return false;
        }

        return DateOnly.TryParseExact(text, DatePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>
    /// Formats a date range as formatRange does: <c>from/to</c> when both ends are set, the one date when only one
    /// is, and an empty string when neither is. The ends are written as given, not reordered.
    /// </summary>
    /// <param name="range">The range</param>
    /// <returns>The wire string</returns>
    public static string FormatDateRange(WaDateRange range)
    {
        return (range.From, range.To) switch
        {
            ({ } from, { } to) => FormatDate(from) + RangeSeparator + FormatDate(to),
            ({ } from, null) => FormatDate(from),
            (null, { } to) => FormatDate(to),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Parses a date range as parseRange reads what the element emits: a single date is the start of a half-filled
    /// range, two dates separated by '/' are ordered so that the earlier one is the start. Stricter than
    /// parseRange, which also reads a malformed half: every part must be a valid date, and there are at most two.
    /// </summary>
    /// <param name="value">The wire string</param>
    /// <param name="range">The parsed range</param>
    /// <returns>true when the text is a valid ISO date or date range</returns>
    public static bool TryParseDateRange(string? value, out WaDateRange range)
    {
        range = default;
        if (value is null) return false;

        var parts = value.Split(RangeSeparator);
        if (parts.Length > 2 || !TryParseDate(parts[0], out var from)) return false;

        if (parts.Length == 1)
        {
            range = new WaDateRange(from, null);
            return true;
        }

        if (!TryParseDate(parts[1], out var to)) return false;

        range = from <= to ? new WaDateRange(from, to) : new WaDateRange(to, from);
        return true;
    }

    /// <summary>
    /// Formats a time as the element emits its value: 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when the seconds are
    /// shown; fractions of a second are dropped, as the element drops them.
    /// </summary>
    /// <param name="time">The time</param>
    /// <param name="withSeconds">Whether to write the seconds</param>
    /// <returns>The wire string</returns>
    public static string FormatTime(TimeOnly time, bool withSeconds)
        => time.ToString(withSeconds ? TimeWithSecondsPattern : TimePattern, CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a time bound (the min or max a native time input reads): <c>HH:mm</c>, or <c>HH:mm:ss</c> when the
    /// bound has seconds.
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The wire string</returns>
    public static string FormatTimeBound(TimeOnly time) => FormatTime(time, withSeconds: time.Second != 0);

    /// <summary>
    /// Parses a time the way wireToTimeSegments does: <c>H:mm</c>, optionally followed by <c>:ss</c> and a fraction
    /// of a second, in the 24-hour range. Unlike the element, which truncates the fraction, the fraction is kept
    /// (to the tick).
    /// </summary>
    /// <param name="value">The wire string</param>
    /// <param name="time">The parsed time</param>
    /// <returns>true when the text is a valid wire time</returns>
    public static bool TryParseTime(string? value, out TimeOnly time)
    {
        time = default;
        if (string.IsNullOrEmpty(value)) return false;

        var parts = value.Split(TimeSeparator);
        if (parts.Length is < 2 or > 3) return false;
        if (!TryParseDigits(parts[0], 1, 2, out var hour) || hour > MaxHour) return false;
        if (!TryParseDigits(parts[1], 2, 2, out var minute) || minute > MaxMinuteOrSecond) return false;

        var second = 0;
        long fractionTicks = 0;
        if (parts.Length == 3)
        {
            var secondText = parts[2];
            var point = secondText.IndexOf(FractionSeparator);
            var wholeText = point < 0 ? secondText : secondText[..point];
            if (!TryParseDigits(wholeText, 2, 2, out second) || second > MaxMinuteOrSecond) return false;

            if (point >= 0 && !TryParseFractionTicks(secondText[(point + 1)..], out fractionTicks)) return false;
        }

        time = new TimeOnly(hour, minute, second).Add(TimeSpan.FromTicks(fractionTicks));
        return true;
    }

    /// <summary>
    /// Whether wa-time-input shows (and emits) seconds for a step attribute value, as withSecondsForStep reads the
    /// step stepFromAttribute converts: "any", or a positive step under a minute or not a whole number of minutes.
    /// A missing or invalid step is the default, 60 seconds, which hides them.
    /// </summary>
    /// <param name="step">The step attribute value</param>
    /// <returns>true when the element shows seconds</returns>
    public static bool TimeStepShowsSeconds(string? step)
    {
        if (step is null) return false;
        if (step == AnyStep) return true;

        if (!double.TryParse(step.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            || !double.IsFinite(seconds) || seconds <= 0)
        {
            return false;
        }

        return seconds < SecondsPerMinute || seconds % SecondsPerMinute != 0;
    }

    /// <summary>
    /// Formats an instant in the ECMAScript date-time string format <c>new Date(text)</c> reads unambiguously:
    /// <c>yyyy-MM-ddTHH:mm:ss.fff</c> with the offset (<c>+01:00</c>, <c>+00:00</c> for UTC).
    /// </summary>
    /// <param name="instant">The instant</param>
    /// <returns>The wire string</returns>
    public static string FormatInstant(DateTimeOffset instant) => instant.ToString(InstantPattern, CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a set of dates as the whitespace-separated list parseDisabledDates splits, ascending, so an equal
    /// set always renders the same text.
    /// </summary>
    /// <param name="dates">The dates</param>
    /// <returns>The wire string, empty for no dates</returns>
    public static string FormatDates(IEnumerable<DateOnly> dates)
        => string.Join(ListSeparator, dates.Distinct().Order().Select(FormatDate));

    /// <summary>
    /// Formats a set of weekdays as the lower-case three-letter tokens parseDaysOfWeek reads (sun, mon, …),
    /// ordered Sunday first like <see cref="DayOfWeek"/> and JS getDay().
    /// </summary>
    /// <param name="days">The weekdays</param>
    /// <returns>The wire string, empty for no weekdays</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a value that is not a <see cref="DayOfWeek"/> member</exception>
    public static string FormatDaysOfWeek(IEnumerable<DayOfWeek> days)
        => string.Join(ListSeparator, days.Distinct().Order().Select(DayOfWeekToken));

    #region ------ Internals ------

    private const string DatePattern = "yyyy-MM-dd";
    private const char DateSeparator = '-';
    private const int FirstDateSeparatorIndex = 4;
    private const int SecondDateSeparatorIndex = 7;
    private const char RangeSeparator = '/';
    private const string TimePattern = "HH':'mm";
    private const string TimeWithSecondsPattern = "HH':'mm':'ss";
    private const char TimeSeparator = ':';
    private const char FractionSeparator = '.';
    private const int MaxHour = 23;
    private const int MaxMinuteOrSecond = 59;
    private const int TickDigits = 7;
    private const string AnyStep = "any";
    private const double SecondsPerMinute = 60;
    private const string InstantPattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz";
    private const string ListSeparator = " ";

    // the tokens of parseDaysOfWeek's WEEKDAY_NAMES, indexed by DayOfWeek (Sunday = 0, like JS getDay())
    private static readonly string[] DayOfWeekTokens = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"];

    private static string DayOfWeekToken(DayOfWeek day)
    {
        if (day < DayOfWeek.Sunday || day > DayOfWeek.Saturday)
            throw new ArgumentOutOfRangeException(nameof(day), day, "Not a day of the week.");

        return DayOfWeekTokens[(int)day];
    }

    // parses minLength..maxLength ASCII digits
    private static bool TryParseDigits(string text, int minLength, int maxLength, out int value)
    {
        value = 0;
        if (text.Length < minLength || text.Length > maxLength) return false;

        foreach (var c in text)
        {
            if (!char.IsAsciiDigit(c)) return false;
            value = value * 10 + (c - '0');
        }

        return true;
    }

    // parses the digits after the decimal point into ticks, dropping digits beyond the tick resolution
    private static bool TryParseFractionTicks(string digits, out long ticks)
    {
        ticks = 0;
        if (digits.Length == 0) return false;

        for (var i = 0; i < digits.Length; i++)
        {
            if (!char.IsAsciiDigit(digits[i])) return false;
            if (i < TickDigits) ticks = ticks * 10 + (digits[i] - '0');
        }

        for (var i = digits.Length; i < TickDigits; i++)
            ticks *= 10;

        return true;
    }

    #endregion
}
