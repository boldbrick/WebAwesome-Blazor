using System;
using System.Globalization;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Demo.Services;

/// <summary>
/// Culture-free display of the typed date and time models on the demo and harness pages, so an echo reads the same
/// in every browser locale (the e2e specs compare it literally).
/// </summary>
public static class DemoFormat
{
    /// <summary>
    /// Formats a date as ISO <c>yyyy-MM-dd</c>.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns>The ISO date, or an empty string for null</returns>
    public static string Iso(DateOnly? date) => date?.ToString(DatePattern, CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>
    /// Formats a time as 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when it has seconds (like the time input's wire value).
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The time, or an empty string for null</returns>
    public static string Iso(TimeOnly? time)
        => time?.ToString(time.Value.Second != 0 ? TimeWithSecondsPattern : TimePattern, CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>
    /// Formats a date range as <c>from … to</c> in ISO dates, with <c>?</c> for a missing end.
    /// </summary>
    /// <param name="range">The range</param>
    /// <returns>The range, or an empty string for null</returns>
    public static string Iso(WaDateRange? range)
        => range is { } value ? $"{EndOrMissing(value.From)} to {EndOrMissing(value.To)}" : string.Empty;

    #region ------ Internals ------

    private const string DatePattern = "yyyy-MM-dd";
    private const string TimePattern = "HH':'mm";
    private const string TimeWithSecondsPattern = "HH':'mm':'ss";
    private const string MissingEnd = "?";

    private static string EndOrMissing(DateOnly? date) => date.HasValue ? Iso(date) : MissingEnd;

    #endregion
}
