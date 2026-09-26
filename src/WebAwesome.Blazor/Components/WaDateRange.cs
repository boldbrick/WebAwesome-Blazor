using System;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A range of calendar dates, the value of <c>WaDateRangeInput</c> and <c>WaDateRangePicker</c>. Either
/// end may be missing: while the user is picking a range, Web Awesome reports the half-filled range as its one date.
/// </summary>
/// <remarks>
/// <para>
/// The wire form Web Awesome reads and writes is <c>from/to</c> in ISO dates (<c>2026-05-01/2026-05-07</c>), a single
/// date for a half-filled range, and an empty string for no range; <see cref="ToString"/> and <see cref="TryParse"/>
/// convert it culture-free. A single date reads back as <see cref="From"/>, so a range with only <see cref="To"/> set
/// reads back with that date as <see cref="From"/> once the element reports it; a range whose <see cref="From"/> is
/// after its <see cref="To"/> reads back with the ends swapped, as the element orders them.
/// </para>
/// <para>
/// Equality is structural (both ends), as a record struct's; <c>default</c> is the empty range.
/// </para>
/// </remarks>
/// <param name="From">The first day of the range, or null when not picked yet</param>
/// <param name="To">The last day of the range, or null when not picked yet</param>
public readonly record struct WaDateRange(DateOnly? From, DateOnly? To)
{
    /// <summary>
    /// Whether neither end is set.
    /// </summary>
    public bool IsEmpty => From is null && To is null;

    /// <summary>
    /// Whether both ends are set.
    /// </summary>
    public bool IsComplete => From is not null && To is not null;

    /// <summary>
    /// Returns the range in Web Awesome's wire form: <c>yyyy-MM-dd/yyyy-MM-dd</c> for a complete range, the one
    /// date (<c>yyyy-MM-dd</c>) for a half-filled one, and an empty string for the empty range. Culture-free.
    /// </summary>
    /// <returns>The wire form</returns>
    public override string ToString() => WaWireFormat.FormatDateRange(this);

    /// <summary>
    /// Parses Web Awesome's wire form: <c>yyyy-MM-dd/yyyy-MM-dd</c> (the earlier date becomes <see cref="From"/>),
    /// or a single <c>yyyy-MM-dd</c> date, which becomes <see cref="From"/>. Culture-free and strict: each date must
    /// be exactly <c>yyyy-MM-dd</c> and a real calendar day, surrounding whitespace aside.
    /// </summary>
    /// <param name="value">The text to parse</param>
    /// <param name="result">The parsed range, or the empty range when the text is invalid</param>
    /// <returns>true when the text is a valid date or date range</returns>
    public static bool TryParse([NotNullWhen(true)] string? value, out WaDateRange result)
        => WaWireFormat.TryParseDateRange(value, out result);

    /// <summary>
    /// Parses Web Awesome's wire form; see <see cref="TryParse"/>.
    /// </summary>
    /// <param name="value">The text to parse</param>
    /// <returns>The parsed range</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="FormatException">Thrown when value is not a valid date or date range</exception>
    public static WaDateRange Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return TryParse(value, out var result)
            ? result
            : throw new FormatException($"'{value}' is not an ISO date (yyyy-MM-dd) or date range (yyyy-MM-dd/yyyy-MM-dd).");
    }
}
