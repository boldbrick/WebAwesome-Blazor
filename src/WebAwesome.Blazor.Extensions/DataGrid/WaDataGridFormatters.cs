using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Cell presets for <see cref="WaDataGridColumn.Formatter"/>: built-in renderers that a column configures in C# only.
/// Each factory returns a <see cref="WaJsFunction"/> reference to a preset function that the JS initializer of the
/// Extensions package registers, with the options as its arguments.
/// </summary>
/// <remarks>
/// <para>
/// Text presets (<see cref="Number"/>, <see cref="Currency"/>, <see cref="Date"/>, <see cref="Lookup"/>, ...) return a
/// string, so the CSV export and the copy of rows contain the formatted text. Visual presets (<see cref="Badge"/>,
/// <see cref="Tag"/>, <see cref="Icon"/>, ...) and links return an element; the export then contains the raw value. A
/// visual preset keeps its element for the same row object and value, so the element keeps its state when the grid
/// renders again.
/// </para>
/// <para>
/// A formatter changes only what the cell shows: the grid sorts, filters and searches the raw value. Use
/// <see cref="WaDataGridComparators.Ordinal(string[])"/> to sort codes in a given order.
/// </para>
/// <para>
/// <c>nullText</c> is the text of a null, missing or empty value; by default the cell is empty. <c>culture</c> sets the
/// locale of the formats; by default it is <see cref="CultureInfo.CurrentCulture"/> when the factory runs, not the
/// locale of the browser, so the cells match the other texts of the app. Keep the column list in a field: the grid pushes
/// the columns again only when their JSON changes.
/// </para>
/// <para>
/// Dates: a <see cref="DateOnly"/> value (JSON <c>yyyy-MM-dd</c>) shows its calendar day in every time zone, and a
/// <see cref="TimeOnly"/> value its time of day. A <see cref="System.DateTime"/> of the kind
/// <see cref="DateTimeKind.Unspecified"/> serializes without a zone, and the browser reads it as its local time: use UTC
/// or <see cref="DateTimeOffset"/> in the row, and set <c>timeZone</c> for the display.
/// </para>
/// </remarks>
public static partial class WaDataGridFormatters
{
    /// <summary>
    /// The icon of true values of <see cref="BooleanIcon"/> when no icon is given.
    /// </summary>
    public const string DefaultTrueIconName = "check";

    /// <summary>
    /// The icon of false values of <see cref="BooleanIcon"/> when no icon is given.
    /// </summary>
    public const string DefaultFalseIconName = "xmark";

    /// <summary>
    /// The separator of <see cref="List"/> when no separator is given.
    /// </summary>
    public const string DefaultListSeparator = ", ";

    // text presets: a string, so the CSV export holds the formatted text

    /// <summary>
    /// A localized number (<c>Intl.NumberFormat</c>), with an optional unit. Output: text.
    /// </summary>
    /// <param name="minFractionDigits">The minimum number of fraction digits (0 to 100)</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits (0 to 100)</param>
    /// <param name="useGrouping">Whether to show grouping separators; the culture decides when null</param>
    /// <param name="notation">The notation, for example compact ("1.2M")</param>
    /// <param name="signDisplay">When the sign shows</param>
    /// <param name="unit">A unit identifier of <c>Intl.NumberFormat</c>, for example <c>kilometer</c> or
    /// <c>kilometer-per-hour</c>; null for a plain number</param>
    /// <param name="unitDisplay">The length of the unit text</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a digit count is out of range, or the unit is not a unit
    /// identifier</exception>
    public static WaJsFunction Number(int? minFractionDigits = null, int? maxFractionDigits = null, bool? useGrouping = null,
        WaDataGridNumberNotation? notation = null, WaDataGridSignDisplay? signDisplay = null, string? unit = null,
        WaDisplay? unitDisplay = null, string? nullText = null, CultureInfo? culture = null)
    {
        ValidateFractionDigits(minFractionDigits, maxFractionDigits);
        if (unit is not null && !UnitPattern.IsMatch(unit))
            throw new ArgumentException($"'{unit}' is not a unit identifier such as 'kilometer' or 'kilometer-per-hour'.", nameof(unit));

        return NumberArgs(minFractionDigits, maxFractionDigits, signDisplay, nullText, culture)
            .Set(UseGroupingKey, useGrouping)
            .Set(NotationKey, notation?.ToJsValue())
            .Set(UnitKey, unit)
            .Set(UnitDisplayKey, unitDisplay?.ToHtmlValue())
            .ToFunction(PresetNames.Number);
    }

    /// <summary>
    /// A localized amount of money (<c>Intl.NumberFormat</c> with the currency style). The currency is fixed, or comes
    /// from a field of the row (for example a column of amounts in several currencies). Output: text.
    /// </summary>
    /// <param name="currency">The ISO 4217 currency code, for example <c>EUR</c>; with <paramref name="currencyField"/>
    /// it is the currency of rows without a code</param>
    /// <param name="currencyField">The path of a row field that holds the currency code of the row</param>
    /// <param name="minFractionDigits">The minimum number of fraction digits (0 to 100); the currency decides when
    /// null</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits (0 to 100); the currency decides when
    /// null</param>
    /// <param name="currencyDisplay">How the currency shows: symbol, narrow symbol, code or name</param>
    /// <param name="signDisplay">When the sign shows</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when neither a currency nor a currency field is given, when the
    /// currency is not a three-letter code, the field is not a field path, or a digit count is out of range</exception>
    public static WaJsFunction Currency(string? currency = null, string? currencyField = null, int? minFractionDigits = null,
        int? maxFractionDigits = null, WaCurrencyDisplay? currencyDisplay = null, WaDataGridSignDisplay? signDisplay = null,
        string? nullText = null, CultureInfo? culture = null)
    {
        if (currency is null && currencyField is null)
            throw new ArgumentException("Give a currency code, a currency field, or both.", nameof(currency));
        if (currency is not null && !CurrencyPattern.IsMatch(currency))
            throw new ArgumentException($"'{currency}' is not an ISO 4217 currency code such as 'EUR'.", nameof(currency));
        if (currencyField is not null) CellTemplate.ValidateFieldPath(currencyField, nameof(currencyField));
        ValidateFractionDigits(minFractionDigits, maxFractionDigits);

        return NumberArgs(minFractionDigits, maxFractionDigits, signDisplay, nullText, culture)
            .Set(CurrencyKey, currency?.ToUpperInvariant())
            .Set(CurrencyFieldKey, currencyField)
            .Set(CurrencyDisplayKey, currencyDisplay?.ToHtmlValue())
            .ToFunction(PresetNames.Currency);
    }

    /// <summary>
    /// A localized percentage (<c>Intl.NumberFormat</c> with the percent style). Output: text.
    /// </summary>
    /// <param name="scale">The scale of the values: a fraction (0.25 is 25%, the default) or a percentage (25 is
    /// 25%)</param>
    /// <param name="minFractionDigits">The minimum number of fraction digits (0 to 100)</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits (0 to 100)</param>
    /// <param name="signDisplay">When the sign shows</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a digit count is out of range</exception>
    public static WaJsFunction Percent(WaDataGridPercentScale? scale = null, int? minFractionDigits = null,
        int? maxFractionDigits = null, WaDataGridSignDisplay? signDisplay = null, string? nullText = null, CultureInfo? culture = null)
    {
        ValidateFractionDigits(minFractionDigits, maxFractionDigits);

        return NumberArgs(minFractionDigits, maxFractionDigits, signDisplay, nullText, culture)
            .Set(ScaleKey, scale?.ToJsValue())
            .ToFunction(PresetNames.Percent);
    }

    /// <summary>
    /// A localized date (<c>Intl.DateTimeFormat</c>). A <see cref="DateOnly"/> value shows its calendar day; an instant
    /// shows its day in <paramref name="timeZone"/>. Output: text.
    /// </summary>
    /// <param name="dateStyle">The length of the date; the numeric date of the culture when null</param>
    /// <param name="timeZone">The IANA time zone of instants, for example <c>UTC</c> or <c>Europe/Prague</c>; the time
    /// zone of the browser when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the time zone is empty or contains whitespace</exception>
    public static WaJsFunction Date(WaDataGridDateStyle? dateStyle = null, string? timeZone = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        return DateArgs(dateStyle, null, timeZone, nullText, culture).ToFunction(PresetNames.Date);
    }

    /// <summary>
    /// A localized date and time (<c>Intl.DateTimeFormat</c>). Output: text.
    /// </summary>
    /// <param name="dateStyle">The length of the date</param>
    /// <param name="timeStyle">The length of the time</param>
    /// <param name="timeZone">The IANA time zone, for example <c>UTC</c> or <c>Europe/Prague</c>; the time zone of the
    /// browser when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <remarks>Without styles, the cell shows the numeric date with hours and minutes. When only one style is set, the
    /// other part uses <see cref="WaDataGridDateStyle.Short"/>.</remarks>
    /// <exception cref="ArgumentException">Thrown when the time zone is empty or contains whitespace</exception>
    public static WaJsFunction DateTime(WaDataGridDateStyle? dateStyle = null, WaDataGridDateStyle? timeStyle = null,
        string? timeZone = null, string? nullText = null, CultureInfo? culture = null)
    {
        return DateArgs(dateStyle, timeStyle, timeZone, nullText, culture).ToFunction(PresetNames.DateTime);
    }

    /// <summary>
    /// A localized time of day (<c>Intl.DateTimeFormat</c>). A <see cref="TimeOnly"/> value shows as it is; an instant
    /// shows its time in <paramref name="timeZone"/>. Output: text.
    /// </summary>
    /// <param name="timeStyle">The length of the time; hours and minutes when null</param>
    /// <param name="timeZone">The IANA time zone of instants, for example <c>UTC</c> or <c>Europe/Prague</c>; the time
    /// zone of the browser when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the time zone is empty or contains whitespace</exception>
    public static WaJsFunction Time(WaDataGridDateStyle? timeStyle = null, string? timeZone = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        return DateArgs(null, timeStyle, timeZone, nullText, culture).ToFunction(PresetNames.Time);
    }

    /// <summary>
    /// A localized time relative to now, for example "3 hours ago" (<c>Intl.RelativeTimeFormat</c>). The text is
    /// computed each time the grid renders the cell. Output: text.
    /// </summary>
    /// <param name="numeric">Whether to always use numbers ("1 day ago") or words where the culture has them
    /// ("yesterday")</param>
    /// <param name="style">The length of the unit names</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction RelativeTime(WaRelativeTimeNumeric? numeric = null, WaRelativeTimeFormat? style = null,
        string? nullText = null, CultureInfo? culture = null)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(NumericKey, numeric?.ToHtmlValue())
            .Set(StyleKey, style?.ToHtmlValue())
            .ToFunction(PresetNames.RelativeTime);
    }

    /// <summary>
    /// A localized size in bytes or bits with a decimal prefix, for example "1.5 MB", as <c>wa-format-bytes</c> shows
    /// it. Output: text.
    /// </summary>
    /// <param name="unit">Bytes (the default) or bits</param>
    /// <param name="display">The length of the unit text</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction Bytes(WaByteUnit? unit = null, WaDisplay? display = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(UnitKey, unit?.ToHtmlValue())
            .Set(DisplayKey, display?.ToHtmlValue())
            .ToFunction(PresetNames.Bytes);
    }

    /// <summary>
    /// A localized duration, for example "1 hr, 30 min" (<c>Intl.DurationFormat</c>, with a simple fallback where the
    /// browser does not have it). The value is a number in <paramref name="unit"/>, an ISO 8601 duration
    /// (<c>PT1H30M</c>) or the JSON form of a <see cref="TimeSpan"/> (<c>01:30:00</c>). Output: text.
    /// </summary>
    /// <param name="unit">The unit of a numeric value: seconds (the default) or milliseconds</param>
    /// <param name="style">The form of the text; short when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction Duration(WaDataGridDurationUnit? unit = null, WaDataGridDurationStyle? style = null,
        string? nullText = null, CultureInfo? culture = null)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(UnitKey, unit?.ToJsValue())
            .Set(StyleKey, style?.ToJsValue())
            .ToFunction(PresetNames.Duration);
    }

    /// <summary>
    /// A display text for each value, for example the localized name of a state code. The cell shows the text; the
    /// value stays the code for sort, filter and search. Output: text.
    /// </summary>
    /// <param name="labels">The texts, keyed by the value as text (<c>String(value)</c> in the browser: <c>"2"</c> for
    /// the number 2, <c>"true"</c> for true)</param>
    /// <param name="fallback">The text of a value that has no label; the value itself when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when labels is null</exception>
    /// <exception cref="ArgumentException">Thrown when a label is null</exception>
    public static WaJsFunction Lookup(IReadOnlyDictionary<string, string> labels, string? fallback = null, string? nullText = null)
    {
        if (labels is null) throw new ArgumentNullException(nameof(labels));

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(LabelsKey, PresetArgs.CopyTexts(labels, nameof(labels)))
            .Set(FallbackKey, fallback)
            .ToFunction(PresetNames.Lookup);
    }

    /// <summary>
    /// A display text for each value of an enum. The keys are the JSON form of each value: its number, or its name when
    /// the enum has a JSON converter attribute such as <c>[JsonConverter(typeof(JsonStringEnumConverter))]</c>. A
    /// converter that only the serializer options of the app configure is not visible here; give such an enum the
    /// attribute. Output: text.
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the column values</typeparam>
    /// <param name="label">The text of a value, for example from the resources of the app; null leaves the value out</param>
    /// <param name="fallback">The text of a value that has no label; the value itself when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when label is null</exception>
    public static WaJsFunction Lookup<TEnum>(Func<TEnum, string?> label, string? fallback = null, string? nullText = null)
        where TEnum : struct, Enum
    {
        if (label is null) throw new ArgumentNullException(nameof(label));

        return Lookup(EnumKeys.Map(label), fallback, nullText);
    }

    /// <summary>
    /// A text for true, false and null values. Output: text.
    /// </summary>
    /// <param name="trueText">The text of true</param>
    /// <param name="falseText">The text of false</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when trueText or falseText is null</exception>
    public static WaJsFunction Boolean(string trueText, string falseText, string? nullText = null)
    {
        if (trueText is null) throw new ArgumentNullException(nameof(trueText));
        if (falseText is null) throw new ArgumentNullException(nameof(falseText));

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(TrueTextKey, trueText)
            .Set(FalseTextKey, falseText)
            .ToFunction(PresetNames.Boolean);
    }

    /// <summary>
    /// A text built from the fields of the row, for example <c>"{firstName} {lastName}"</c>. A placeholder can name a
    /// format: <c>{amount:number}</c>, <c>{share:percent}</c>, <c>{due:date}</c>, <c>{created:datetime}</c>,
    /// <c>{start:time}</c>. Write <c>{{</c> and <c>}}</c> for literal braces. A missing or null field gives an empty
    /// text. Output: text.
    /// </summary>
    /// <param name="template">The template</param>
    /// <param name="nullText">The text of a cell whose placeholders are all empty; empty by default</param>
    /// <param name="culture">The culture of the formats; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the template is empty, has an unmatched brace, a placeholder
    /// that is not a field path, or an unknown format</exception>
    public static WaJsFunction Template(string template, string? nullText = null, CultureInfo? culture = null)
    {
        CellTemplate.ValidateText(template, nameof(template));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(TemplateKey, template)
            .ToFunction(PresetNames.Template);
    }

    /// <summary>
    /// The items of an array value, joined with a separator. A value that is not an array shows as it is. Output: text.
    /// </summary>
    /// <param name="separator">The separator; <see cref="DefaultListSeparator"/> when null</param>
    /// <param name="nullText">The text of a null value or an empty array; empty by default</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction List(string? separator = null, string? nullText = null)
    {
        return new PresetArgs()
            .SetNullText(nullText)
            .Set(SeparatorKey, separator)
            .ToFunction(PresetNames.List);
    }

    // visual presets: an element, kept per row object and value

    /// <summary>
    /// A <c>wa-badge</c> whose variant depends on the value, for example a state column. The badge always shows a text
    /// (the label or the value), so the color is never the only signal. Output: an element.
    /// </summary>
    /// <param name="variants">The variant of each value, keyed by the value as text</param>
    /// <param name="fallback">The variant of a value that is not in the map; the default of <c>wa-badge</c> when
    /// null</param>
    /// <param name="appearance">The appearance of the badges</param>
    /// <param name="pill">Whether the badges have rounded edges</param>
    /// <param name="labels">The text of each value; the value itself when it has no label</param>
    /// <param name="icons">An icon in front of the text of each value</param>
    /// <param name="nullText">The text of a null or empty value (no badge); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when variants is null</exception>
    /// <exception cref="ArgumentException">Thrown when a label or an icon is null</exception>
    public static WaJsFunction Badge(IReadOnlyDictionary<string, WaVariant> variants, WaVariant? fallback = null,
        WaBadgeAppearance? appearance = null, bool pill = false, IReadOnlyDictionary<string, string>? labels = null,
        IReadOnlyDictionary<string, WaDataGridIcon>? icons = null, string? nullText = null)
    {
        if (variants is null) throw new ArgumentNullException(nameof(variants));

        var args = MarkerArgs(variants, fallback, appearance, pill, labels, nullText)
            .Set(IconsKey, PresetArgs.CopyIcons(icons, nameof(icons)));

        return icons is { Count: > 0 }
            ? args.ToFunction(PresetNames.Badge, BadgeTag, IconTag)
            : args.ToFunction(PresetNames.Badge, BadgeTag);
    }

    /// <summary>
    /// A <c>wa-badge</c> for each value of an enum, for example a state column. The keys are the JSON form of each
    /// value (see <see cref="Lookup{TEnum}"/>). Output: an element.
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the column values</typeparam>
    /// <param name="variant">The variant of a value</param>
    /// <param name="label">The text of a value, for example from the resources of the app; the value as JSON shows when
    /// null</param>
    /// <param name="icon">An icon in front of the text of a value; null for no icon</param>
    /// <param name="fallback">The variant of a value that the enum does not declare</param>
    /// <param name="appearance">The appearance of the badges</param>
    /// <param name="pill">Whether the badges have rounded edges</param>
    /// <param name="nullText">The text of a null or empty value (no badge); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when variant is null</exception>
    public static WaJsFunction Badge<TEnum>(Func<TEnum, WaVariant> variant, Func<TEnum, string?>? label = null,
        Func<TEnum, WaDataGridIcon?>? icon = null, WaVariant? fallback = null, WaBadgeAppearance? appearance = null,
        bool pill = false, string? nullText = null)
        where TEnum : struct, Enum
    {
        if (variant is null) throw new ArgumentNullException(nameof(variant));

        return Badge(EnumKeys.Map(variant), fallback, appearance, pill,
            label is null ? null : EnumKeys.Map(label),
            icon is null ? null : EnumKeys.Map(icon),
            nullText);
    }

    /// <summary>
    /// A <c>wa-tag</c> whose variant depends on the value. Output: an element.
    /// </summary>
    /// <param name="variants">The variant of each value, keyed by the value as text; null for one variant</param>
    /// <param name="fallback">The variant of a value that is not in the map</param>
    /// <param name="appearance">The appearance of the tags</param>
    /// <param name="pill">Whether the tags have rounded edges</param>
    /// <param name="size">The size of the tags</param>
    /// <param name="labels">The text of each value; the value itself when it has no label</param>
    /// <param name="nullText">The text of a null or empty value (no tag); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a label is null</exception>
    public static WaJsFunction Tag(IReadOnlyDictionary<string, WaVariant>? variants = null, WaVariant? fallback = null,
        WaBadgeAppearance? appearance = null, bool pill = false, WaSize? size = null,
        IReadOnlyDictionary<string, string>? labels = null, string? nullText = null)
    {
        return MarkerArgs(variants, fallback, appearance, pill, labels, nullText)
            .Set(SizeKey, size?.ToHtmlValue())
            .ToFunction(PresetNames.Tag, TagTag);
    }

    /// <summary>
    /// A <c>wa-tag</c> for each item of an array value, for example the labels of an issue. Output: an element.
    /// </summary>
    /// <param name="variants">The variant of each item, keyed by the item as text; null for one variant</param>
    /// <param name="fallback">The variant of an item that is not in the map</param>
    /// <param name="appearance">The appearance of the tags</param>
    /// <param name="pill">Whether the tags have rounded edges</param>
    /// <param name="size">The size of the tags</param>
    /// <param name="labels">The text of each item; the item itself when it has no label</param>
    /// <param name="nullText">The text of a null value or an empty array (no tag); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a label is null</exception>
    public static WaJsFunction Tags(IReadOnlyDictionary<string, WaVariant>? variants = null, WaVariant? fallback = null,
        WaBadgeAppearance? appearance = null, bool pill = false, WaSize? size = null,
        IReadOnlyDictionary<string, string>? labels = null, string? nullText = null)
    {
        return MarkerArgs(variants, fallback, appearance, pill, labels, nullText)
            .Set(SizeKey, size?.ToHtmlValue())
            .ToFunction(PresetNames.Tags, TagTag);
    }

    /// <summary>
    /// A <c>wa-icon</c> for each value, with an accessible name. Output: an element.
    /// </summary>
    /// <param name="icons">The icon of each value, keyed by the value as text; a string converts to an icon name</param>
    /// <param name="labels">The accessible name of each value; the value itself when it has no label</param>
    /// <param name="colors">The color of each value, as the variant color of Web Awesome</param>
    /// <param name="fallback">The icon of a value that is not in the map; when null, such a value shows its label or
    /// the value as text</param>
    /// <param name="nullText">The text of a null or empty value (no icon); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when icons is null</exception>
    /// <exception cref="ArgumentException">Thrown when an icon or a label is null</exception>
    public static WaJsFunction Icon(IReadOnlyDictionary<string, WaDataGridIcon> icons,
        IReadOnlyDictionary<string, string>? labels = null, IReadOnlyDictionary<string, WaVariant>? colors = null,
        WaDataGridIcon? fallback = null, string? nullText = null)
    {
        if (icons is null) throw new ArgumentNullException(nameof(icons));

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(IconsKey, PresetArgs.CopyIcons(icons, nameof(icons)))
            .Set(LabelsKey, PresetArgs.CopyTexts(labels, nameof(labels)))
            .Set(ColorsKey, PresetArgs.CopyVariants(colors))
            .Set(FallbackKey, PresetArgs.IconOrNull(fallback))
            .ToFunction(PresetNames.Icon, IconTag);
    }

    /// <summary>
    /// A check icon for true and a cross icon for false, each with an accessible name. Output: an element.
    /// </summary>
    /// <param name="trueLabel">The accessible name of true, for example "Active"</param>
    /// <param name="falseLabel">The accessible name of false, for example "Inactive"</param>
    /// <param name="trueIcon">The icon of true; <see cref="DefaultTrueIconName"/> when null</param>
    /// <param name="falseIcon">The icon of false; <see cref="DefaultFalseIconName"/> when null</param>
    /// <param name="trueColor">The color of the true icon</param>
    /// <param name="falseColor">The color of the false icon</param>
    /// <param name="nullText">The text of a null value (no icon); empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentNullException">Thrown when trueLabel or falseLabel is null</exception>
    public static WaJsFunction BooleanIcon(string trueLabel, string falseLabel, WaDataGridIcon? trueIcon = null,
        WaDataGridIcon? falseIcon = null, WaVariant? trueColor = null, WaVariant? falseColor = null, string? nullText = null)
    {
        if (trueLabel is null) throw new ArgumentNullException(nameof(trueLabel));
        if (falseLabel is null) throw new ArgumentNullException(nameof(falseLabel));

        var colors = new Dictionary<string, WaVariant>(StringComparer.Ordinal);
        if (trueColor is not null) colors[TrueKey] = trueColor.Value;
        if (falseColor is not null) colors[FalseKey] = falseColor.Value;

        return Icon(
            new Dictionary<string, WaDataGridIcon>(StringComparer.Ordinal)
            {
                [TrueKey] = trueIcon ?? DefaultTrueIconName,
                [FalseKey] = falseIcon ?? DefaultFalseIconName,
            },
            new Dictionary<string, string>(StringComparer.Ordinal) { [TrueKey] = trueLabel, [FalseKey] = falseLabel },
            colors.Count > 0 ? colors : null,
            nullText: nullText);
    }

    // links

    /// <summary>
    /// A link (an <c>a</c> element) whose URL comes from the fields of the row, for example <c>/orders/{id}</c>. Each
    /// placeholder value is URL-encoded. Only relative URLs and the schemes http, https, mailto and tel are allowed: a
    /// cell whose URL has another scheme shows its text without a link. Output: an element.
    /// </summary>
    /// <param name="hrefTemplate">The URL template, with <c>{field}</c> placeholders (no formats)</param>
    /// <param name="textTemplate">The link text, a template as in <see cref="Template"/>; the value of the cell when
    /// null</param>
    /// <param name="target">Where the link opens; a new tab or window also gets <c>rel="noopener noreferrer"</c></param>
    /// <param name="nullText">The text of a cell without link text; empty by default</param>
    /// <param name="culture">The culture of the formats of the text template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a template is not valid, contains whitespace or a control
    /// character, or names another scheme</exception>
    /// <remarks>The keyboard navigation of the grid uses Enter on a cell for itself, so Enter on a focused link
    /// activates the cell, not the link. Use <see cref="ActionLink"/> for an action that the keyboard must reach.</remarks>
    public static WaJsFunction Link(string hrefTemplate, string? textTemplate = null, WaLinkTarget? target = null,
        string? nullText = null, CultureInfo? culture = null)
    {
        CellTemplate.ValidateUrl(hrefTemplate, nameof(hrefTemplate));

        return TextArgs(textTemplate, nullText, culture)
            .Set(HrefKey, hrefTemplate)
            .Set(TargetKey, target?.ToHtmlValue())
            .ToFunction(PresetNames.Link);
    }

    /// <summary>
    /// A button that looks like a link and raises a cell action (the <c>Action</c> form of the link preset). Handle the
    /// action with <c>@onwablazor-cell-action</c> on the <see cref="WaDataGrid"/> (see
    /// <see cref="WaDataGridCellActionEventArgs"/>). A click on the button does not raise <c>OnCellClick</c>. Output: an
    /// element.
    /// </summary>
    /// <param name="action">The action id that the event reports, for example <c>open</c></param>
    /// <param name="textTemplate">The button text, a template as in <see cref="Template"/>; the value of the cell when
    /// null</param>
    /// <param name="nullText">The text of a cell without button text (no button); empty by default</param>
    /// <param name="culture">The culture of the formats of the text template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the action is empty, has leading or trailing whitespace or a
    /// control character, or the text template is not valid</exception>
    public static WaJsFunction ActionLink(string action, string? textTemplate = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        PresetArgs.ValidateActionId(action, nameof(action));

        return TextArgs(textTemplate, nullText, culture)
            .Set(ActionKey, action)
            .ToFunction(PresetNames.ActionLink);
    }

    /// <summary>
    /// A <c>mailto:</c> link to the e-mail address in the cell. Output: an element.
    /// </summary>
    /// <param name="textTemplate">The link text, a template as in <see cref="Template"/>; the address when null</param>
    /// <param name="nullText">The text of a null or empty value (no link); empty by default</param>
    /// <param name="culture">The culture of the formats of the text template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the text template is not valid</exception>
    public static WaJsFunction Email(string? textTemplate = null, string? nullText = null, CultureInfo? culture = null)
    {
        return TextArgs(textTemplate, nullText, culture).ToFunction(PresetNames.Email);
    }

    /// <summary>
    /// A <c>tel:</c> link to the phone number in the cell. The URL keeps only the digits and <c>+ * #</c> of the
    /// number. Output: an element.
    /// </summary>
    /// <param name="textTemplate">The link text, a template as in <see cref="Template"/>; the number as written when
    /// null</param>
    /// <param name="nullText">The text of a null or empty value (no link); empty by default</param>
    /// <param name="culture">The culture of the formats of the text template; <see cref="CultureInfo.CurrentCulture"/>
    /// when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the text template is not valid</exception>
    public static WaJsFunction Phone(string? textTemplate = null, string? nullText = null, CultureInfo? culture = null)
    {
        return TextArgs(textTemplate, nullText, culture).ToFunction(PresetNames.Phone);
    }

    #region ------ Internals ------

    // the option names that the preset functions in data-grid-presets.js read
    private const string MinFractionDigitsKey = "minFractionDigits";
    private const string MaxFractionDigitsKey = "maxFractionDigits";
    private const string UseGroupingKey = "useGrouping";
    private const string NotationKey = "notation";
    private const string SignDisplayKey = "signDisplay";
    private const string UnitKey = "unit";
    private const string UnitDisplayKey = "unitDisplay";
    private const string CurrencyKey = "currency";
    private const string CurrencyFieldKey = "currencyField";
    private const string CurrencyDisplayKey = "currencyDisplay";
    private const string ScaleKey = "scale";
    private const string DateStyleKey = "dateStyle";
    private const string TimeStyleKey = "timeStyle";
    private const string TimeZoneKey = "timeZone";
    private const string NumericKey = "numeric";
    private const string StyleKey = "style";
    private const string DisplayKey = "display";
    private const string LabelsKey = "labels";
    private const string FallbackKey = "fallback";
    private const string TrueTextKey = "trueText";
    private const string FalseTextKey = "falseText";
    private const string TemplateKey = "template";
    private const string SeparatorKey = "separator";
    private const string VariantsKey = "variants";
    private const string AppearanceKey = "appearance";
    private const string PillKey = "pill";
    private const string SizeKey = "size";
    private const string IconsKey = "icons";
    private const string ColorsKey = "colors";
    private const string HrefKey = "href";
    private const string TextKey = "text";
    private const string TargetKey = "target";
    private const string ActionKey = "action";

    // the map keys of boolean values: String(true) and String(false) in the browser
    private const string TrueKey = "true";
    private const string FalseKey = "false";

    // the elements the visual presets render, registered before the first render (WaJsFunction.Uses)
    private const string BadgeTag = "wa-badge";
    private const string TagTag = "wa-tag";
    private const string IconTag = "wa-icon";

    // the range of fraction digits that Intl.NumberFormat accepts
    private const int MaxFractionDigitsLimit = 100;

    // a unit identifier of Intl.NumberFormat: lowercase words joined with "-" ("kilometer-per-hour")
    private static readonly Regex UnitPattern = new("^[a-z]+(?:-[a-z]+)*$", RegexOptions.CultureInvariant);

    // an ISO 4217 currency code
    private static readonly Regex CurrencyPattern = new("^[A-Za-z]{3}$", RegexOptions.CultureInvariant);

    private static void ValidateFractionDigits(int? minFractionDigits, int? maxFractionDigits)
    {
        if (minFractionDigits is < 0 or > MaxFractionDigitsLimit)
            throw new ArgumentException($"The minimum fraction digits must be between 0 and {MaxFractionDigitsLimit}.", nameof(minFractionDigits));
        if (maxFractionDigits is < 0 or > MaxFractionDigitsLimit)
            throw new ArgumentException($"The maximum fraction digits must be between 0 and {MaxFractionDigitsLimit}.", nameof(maxFractionDigits));
        if (minFractionDigits > maxFractionDigits)
            throw new ArgumentException("The minimum fraction digits must not be greater than the maximum.", nameof(minFractionDigits));
    }

    private static void ValidateTimeZone(string? timeZone)
    {
        if (timeZone is not null && (timeZone.Length == 0 || timeZone.Any(c => char.IsWhiteSpace(c) || char.IsControl(c))))
            throw new ArgumentException($"'{timeZone}' is not an IANA time zone such as 'Europe/Prague'.", nameof(timeZone));
    }

    private static PresetArgs NumberArgs(int? minFractionDigits, int? maxFractionDigits, WaDataGridSignDisplay? signDisplay,
        string? nullText, CultureInfo? culture)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(MinFractionDigitsKey, minFractionDigits)
            .Set(MaxFractionDigitsKey, maxFractionDigits)
            .Set(SignDisplayKey, signDisplay?.ToJsValue());
    }

    private static PresetArgs DateArgs(WaDataGridDateStyle? dateStyle, WaDataGridDateStyle? timeStyle, string? timeZone,
        string? nullText, CultureInfo? culture)
    {
        ValidateTimeZone(timeZone);

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(DateStyleKey, dateStyle?.ToJsValue())
            .Set(TimeStyleKey, timeStyle?.ToJsValue())
            .Set(TimeZoneKey, timeZone);
    }

    // the options of the badge and tag presets
    private static PresetArgs MarkerArgs(IReadOnlyDictionary<string, WaVariant>? variants, WaVariant? fallback,
        WaBadgeAppearance? appearance, bool pill, IReadOnlyDictionary<string, string>? labels, string? nullText)
    {
        return new PresetArgs()
            .SetNullText(nullText)
            .Set(VariantsKey, PresetArgs.CopyVariants(variants))
            .Set(FallbackKey, fallback?.ToHtmlValue())
            .Set(AppearanceKey, appearance?.ToHtmlValue())
            .Set(PillKey, pill ? true : null)
            .Set(LabelsKey, PresetArgs.CopyTexts(labels, nameof(labels)));
    }

    // the options of the link presets: the text template and its culture
    private static PresetArgs TextArgs(string? textTemplate, string? nullText, CultureInfo? culture)
    {
        if (textTemplate is not null) CellTemplate.ValidateText(textTemplate, nameof(textTemplate));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(TextKey, textTemplate);
    }

    #endregion
}
