using System;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// A predefined length of a date or time text, as <c>Intl.DateTimeFormat</c> defines it (<c>dateStyle</c>,
/// <c>timeStyle</c>). The culture decides the exact form.
/// </summary>
public enum WaDataGridDateStyle
{
    /// <summary>
    /// The longest form, for example "Thursday, October 15, 2026".
    /// </summary>
    Full,

    /// <summary>
    /// A long form, for example "October 15, 2026".
    /// </summary>
    Long,

    /// <summary>
    /// A medium form, for example "Oct 15, 2026".
    /// </summary>
    Medium,

    /// <summary>
    /// The shortest form, for example "10/15/26".
    /// </summary>
    Short
}

/// <summary>
/// The notation of a formatted number (<c>Intl.NumberFormat</c> <c>notation</c>).
/// </summary>
public enum WaDataGridNumberNotation
{
    /// <summary>
    /// Plain number formatting, for example "1,234,567".
    /// </summary>
    Standard,

    /// <summary>
    /// Scientific notation, for example "1.235E6".
    /// </summary>
    Scientific,

    /// <summary>
    /// Engineering notation (an exponent that is a multiple of three), for example "1.235E6".
    /// </summary>
    Engineering,

    /// <summary>
    /// Compact notation of the culture, for example "1.2M".
    /// </summary>
    Compact
}

/// <summary>
/// When a formatted number shows its sign (<c>Intl.NumberFormat</c> <c>signDisplay</c>).
/// </summary>
public enum WaDataGridSignDisplay
{
    /// <summary>
    /// The sign of negative numbers only, the default of the browser.
    /// </summary>
    Auto,

    /// <summary>
    /// Always a sign, also "+0".
    /// </summary>
    Always,

    /// <summary>
    /// A sign for positive and negative numbers, none for zero.
    /// </summary>
    ExceptZero,

    /// <summary>
    /// A sign for negative numbers only, none for negative zero.
    /// </summary>
    Negative,

    /// <summary>
    /// Never a sign.
    /// </summary>
    Never
}

/// <summary>
/// The scale of the values of a percent column.
/// </summary>
public enum WaDataGridPercentScale
{
    /// <summary>
    /// The value is a fraction: 0.25 shows as "25%".
    /// </summary>
    Fraction,

    /// <summary>
    /// The value is already a percentage: 25 shows as "25%".
    /// </summary>
    Percent
}

/// <summary>
/// The unit of a numeric duration value.
/// </summary>
public enum WaDataGridDurationUnit
{
    /// <summary>
    /// The number is a count of seconds.
    /// </summary>
    Seconds,

    /// <summary>
    /// The number is a count of milliseconds.
    /// </summary>
    Milliseconds
}

/// <summary>
/// The form of a formatted duration (<c>Intl.DurationFormat</c> <c>style</c>).
/// </summary>
public enum WaDataGridDurationStyle
{
    /// <summary>
    /// Full unit names, for example "1 hour, 30 minutes".
    /// </summary>
    Long,

    /// <summary>
    /// Short unit names, for example "1 hr, 30 min".
    /// </summary>
    Short,

    /// <summary>
    /// The shortest unit names, for example "1h 30m".
    /// </summary>
    Narrow,

    /// <summary>
    /// A clock form, for example "1:30:00".
    /// </summary>
    Digital
}

/// <summary>
/// Which differences between two texts count in a sort (<c>Intl.Collator</c> <c>sensitivity</c>).
/// </summary>
public enum WaDataGridCollationSensitivity
{
    /// <summary>
    /// Only different base letters count: "a" equals "á" and "A".
    /// </summary>
    Base,

    /// <summary>
    /// Base letters and accents count: "a" differs from "á", but equals "A".
    /// </summary>
    Accent,

    /// <summary>
    /// Base letters and case count: "a" differs from "A", but equals "á".
    /// </summary>
    Case,

    /// <summary>
    /// Base letters, accents, case and other differences count.
    /// </summary>
    Variant
}
/// <summary>
/// The JavaScript option values of the data grid enums of the Extensions package.
/// </summary>
internal static class WaDataGridEnumExtensions
{
    /// <summary>
    /// The <c>Intl.DateTimeFormat</c> value of a date or time style.
    /// </summary>
    /// <param name="style">The style</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridDateStyle style)
    {
        return style switch
        {
            WaDataGridDateStyle.Full => "full",
            WaDataGridDateStyle.Long => "long",
            WaDataGridDateStyle.Medium => "medium",
            WaDataGridDateStyle.Short => "short",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// The <c>Intl.NumberFormat</c> value of a notation.
    /// </summary>
    /// <param name="notation">The notation</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridNumberNotation notation)
    {
        return notation switch
        {
            WaDataGridNumberNotation.Standard => "standard",
            WaDataGridNumberNotation.Scientific => "scientific",
            WaDataGridNumberNotation.Engineering => "engineering",
            WaDataGridNumberNotation.Compact => "compact",
            _ => throw new ArgumentOutOfRangeException(nameof(notation), notation, null)
        };
    }

    /// <summary>
    /// The <c>Intl.NumberFormat</c> value of a sign display.
    /// </summary>
    /// <param name="signDisplay">The sign display</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridSignDisplay signDisplay)
    {
        return signDisplay switch
        {
            WaDataGridSignDisplay.Auto => "auto",
            WaDataGridSignDisplay.Always => "always",
            WaDataGridSignDisplay.ExceptZero => "exceptZero",
            WaDataGridSignDisplay.Negative => "negative",
            WaDataGridSignDisplay.Never => "never",
            _ => throw new ArgumentOutOfRangeException(nameof(signDisplay), signDisplay, null)
        };
    }

    /// <summary>
    /// The preset option value of a percent scale.
    /// </summary>
    /// <param name="scale">The scale</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridPercentScale scale)
    {
        return scale switch
        {
            WaDataGridPercentScale.Fraction => "fraction",
            WaDataGridPercentScale.Percent => "percent",
            _ => throw new ArgumentOutOfRangeException(nameof(scale), scale, null)
        };
    }

    /// <summary>
    /// The preset option value of a duration unit.
    /// </summary>
    /// <param name="unit">The unit</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridDurationUnit unit)
    {
        return unit switch
        {
            WaDataGridDurationUnit.Seconds => "seconds",
            WaDataGridDurationUnit.Milliseconds => "milliseconds",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }

    /// <summary>
    /// The <c>Intl.DurationFormat</c> value of a duration style.
    /// </summary>
    /// <param name="style">The style</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridDurationStyle style)
    {
        return style switch
        {
            WaDataGridDurationStyle.Long => "long",
            WaDataGridDurationStyle.Short => "short",
            WaDataGridDurationStyle.Narrow => "narrow",
            WaDataGridDurationStyle.Digital => "digital",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// The <c>Intl.Collator</c> value of a sensitivity.
    /// </summary>
    /// <param name="sensitivity">The sensitivity</param>
    /// <returns>The option value</returns>
    public static string ToJsValue(this WaDataGridCollationSensitivity sensitivity)
    {
        return sensitivity switch
        {
            WaDataGridCollationSensitivity.Base => "base",
            WaDataGridCollationSensitivity.Accent => "accent",
            WaDataGridCollationSensitivity.Case => "case",
            WaDataGridCollationSensitivity.Variant => "variant",
            _ => throw new ArgumentOutOfRangeException(nameof(sensitivity), sensitivity, null)
        };
    }
}
