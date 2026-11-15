using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// The condition of a <see cref="WaDataGridClassRule"/>: a test of a cell value or a row field in the browser.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EqualTo(string)"/> and <see cref="In(string[])"/> compare the value as text, as the lookup presets do
/// (<c>String(value)</c> in the browser): a number key is its JSON form (<c>"2"</c>, <c>"0.5"</c>), and an enum key the
/// JSON form of the enum value. <see cref="LessThan(double)"/>, <see cref="GreaterThan(double)"/> and
/// <see cref="Between(double, double)"/> (inclusive) compare numbers, or dates with a <see cref="DateOnly"/> or
/// <see cref="DateTimeOffset"/> bound; a value that is not a number or a date never matches.
/// </para>
/// <para>
/// The name of the equality condition is <c>EqualTo</c>, because a static <c>Equals</c> would hide
/// <see cref="object.Equals(object)"/>.
/// </para>
/// </remarks>
public sealed class WaDataGridCondition
{
    /// <summary>
    /// Matches a null or missing value.
    /// </summary>
    public static WaDataGridCondition IsNull { get; } = new(IsNullKind);

    /// <summary>
    /// Matches the boolean value true.
    /// </summary>
    public static WaDataGridCondition IsTrue { get; } = new(IsTrueKind);

    /// <summary>
    /// Matches the boolean value false.
    /// </summary>
    public static WaDataGridCondition IsFalse { get; } = new(IsFalseKind);

    /// <summary>
    /// Matches a value whose text equals the key.
    /// </summary>
    /// <param name="value">The key, for example a state code</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static WaDataGridCondition EqualTo(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new WaDataGridCondition(EqualsKind).With(ValueKey, value);
    }

    /// <summary>
    /// Matches a value equal to a number.
    /// </summary>
    /// <param name="value">The number</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when the number is not finite</exception>
    public static WaDataGridCondition EqualTo(double value)
    {
        return EqualTo(NumberKey(value, nameof(value)));
    }

    /// <summary>
    /// Matches a boolean value.
    /// </summary>
    /// <param name="value">The boolean</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition EqualTo(bool value)
    {
        return value ? IsTrue : IsFalse;
    }

    /// <summary>
    /// Matches a value of an enum: the key is the JSON form of the value (its number, or its name with a JSON converter
    /// attribute such as <c>JsonStringEnumConverter</c>).
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the values</typeparam>
    /// <param name="value">The enum value</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition EqualTo<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return EqualTo(EnumKeys.Of(value));
    }

    /// <summary>
    /// Matches a value whose text is one of the keys.
    /// </summary>
    /// <param name="values">The keys</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no key, or a key is null</exception>
    public static WaDataGridCondition In(params string[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length == 0) throw new ArgumentException("Give at least one value.", nameof(values));
        if (values.Any(value => value is null)) throw new ArgumentException("A value must not be null.", nameof(values));

        return new WaDataGridCondition(InKind).With(ValuesKey, values.Distinct(StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// Matches one of the given values of an enum (keys as in <see cref="EqualTo{TEnum}(TEnum)"/>).
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the values</typeparam>
    /// <param name="values">The enum values</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no value</exception>
    public static WaDataGridCondition In<TEnum>(params TEnum[] values) where TEnum : struct, Enum
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        return In(values.Select(EnumKeys.Of).ToArray());
    }

    /// <summary>
    /// Matches a number lower than the bound.
    /// </summary>
    /// <param name="value">The bound</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when the bound is not finite</exception>
    public static WaDataGridCondition LessThan(double value) => Compare(LessThanKind, NumberBound(value, nameof(value)), NumberType);

    /// <summary>
    /// Matches a date before the day. A <see cref="DateOnly"/> value of the row is that calendar day; an instant compares
    /// with the start of the day in UTC.
    /// </summary>
    /// <param name="value">The day</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition LessThan(DateOnly value) => Compare(LessThanKind, DateBound(value), DateType);

    /// <summary>
    /// Matches a date or an instant before the instant.
    /// </summary>
    /// <param name="value">The instant</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition LessThan(DateTimeOffset value) => Compare(LessThanKind, InstantBound(value), DateType);

    /// <summary>
    /// Matches a number greater than the bound.
    /// </summary>
    /// <param name="value">The bound</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when the bound is not finite</exception>
    public static WaDataGridCondition GreaterThan(double value) => Compare(GreaterThanKind, NumberBound(value, nameof(value)), NumberType);

    /// <summary>
    /// Matches a date after the day (see <see cref="LessThan(DateOnly)"/>).
    /// </summary>
    /// <param name="value">The day</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition GreaterThan(DateOnly value) => Compare(GreaterThanKind, DateBound(value), DateType);

    /// <summary>
    /// Matches a date or an instant after the instant.
    /// </summary>
    /// <param name="value">The instant</param>
    /// <returns>The condition</returns>
    public static WaDataGridCondition GreaterThan(DateTimeOffset value) => Compare(GreaterThanKind, InstantBound(value), DateType);

    /// <summary>
    /// Matches a number from the minimum to the maximum, both included.
    /// </summary>
    /// <param name="min">The minimum</param>
    /// <param name="max">The maximum</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when a bound is not finite, or min is greater than max</exception>
    public static WaDataGridCondition Between(double min, double max)
    {
        var low = NumberBound(min, nameof(min));
        var high = NumberBound(max, nameof(max));
        if (min > max) throw new ArgumentException("The minimum must not be greater than the maximum.", nameof(min));

        return Range(low, high, NumberType);
    }

    /// <summary>
    /// Matches a date from the first day to the last day, both included.
    /// </summary>
    /// <param name="min">The first day</param>
    /// <param name="max">The last day</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when min is after max</exception>
    public static WaDataGridCondition Between(DateOnly min, DateOnly max)
    {
        if (min > max) throw new ArgumentException("The first day must not be after the last day.", nameof(min));
        return Range(DateBound(min), DateBound(max), DateType);
    }

    /// <summary>
    /// Matches a date or an instant from the first instant to the last instant, both included.
    /// </summary>
    /// <param name="min">The first instant</param>
    /// <param name="max">The last instant</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when min is after max</exception>
    public static WaDataGridCondition Between(DateTimeOffset min, DateTimeOffset max)
    {
        if (min > max) throw new ArgumentException("The first instant must not be after the last instant.", nameof(min));
        return Range(InstantBound(min), InstantBound(max), DateType);
    }

    /// <summary>
    /// Matches a value whose text matches a JavaScript regular expression, for example <c>^ERR-</c>. The browser checks
    /// the pattern: an invalid pattern writes one console error, and the rule never matches.
    /// </summary>
    /// <param name="pattern">The pattern, in the JavaScript syntax, without slashes and flags</param>
    /// <param name="ignoreCase">Whether the match ignores case (the <c>i</c> flag)</param>
    /// <returns>The condition</returns>
    /// <exception cref="ArgumentException">Thrown when the pattern is empty</exception>
    public static WaDataGridCondition Matches(string pattern, bool ignoreCase = false)
    {
        if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A pattern must not be empty.", nameof(pattern));

        var condition = new WaDataGridCondition(MatchesKind).With(PatternKey, pattern);
        return ignoreCase ? condition.With(IgnoreCaseKey, true) : condition;
    }

    /// <summary>
    /// The JSON text of the condition, as the rule presets read it.
    /// </summary>
    /// <returns>The JSON text</returns>
    public override string ToString() => JsonSerializer.Serialize(values, JsonSerializerOptions.Web);

    #region ------ Constructors ------

    private WaDataGridCondition(string kind)
    {
        values[WhenKey] = kind;
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Copies the members of the condition into a rule.
    /// </summary>
    /// <param name="rule">The rule object</param>
    internal void WriteTo(Dictionary<string, object> rule)
    {
        foreach (var (key, value) in values) rule[key] = value;
    }

    // the option names of a condition, and the condition kinds (data-grid-presets.js, compileRule)
    private const string WhenKey = "when";
    private const string ValueKey = "value";
    private const string ValuesKey = "values";
    private const string MinKey = "min";
    private const string MaxKey = "max";
    private const string TypeKey = "type";
    private const string PatternKey = "pattern";
    private const string IgnoreCaseKey = "ignoreCase";
    private const string EqualsKind = "equals";
    private const string InKind = "in";
    private const string LessThanKind = "lessThan";
    private const string GreaterThanKind = "greaterThan";
    private const string BetweenKind = "between";
    private const string IsNullKind = "isNull";
    private const string IsTrueKind = "isTrue";
    private const string IsFalseKind = "isFalse";
    private const string MatchesKind = "matches";
    private const string NumberType = "number";
    private const string DateType = "date";

    // the wire forms of the date bounds: an ISO day, and an instant with its offset
    private const string DateFormat = "yyyy-MM-dd";
    private const string InstantFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

    // insertion order is the JSON order, so the same condition always gives the same descriptor text
    private readonly Dictionary<string, object> values = new(StringComparer.Ordinal);

    private WaDataGridCondition With(string key, object value)
    {
        values[key] = value;
        return this;
    }

    private static WaDataGridCondition Compare(string kind, object bound, string type)
    {
        return new WaDataGridCondition(kind).With(ValueKey, bound).With(TypeKey, type);
    }

    private static WaDataGridCondition Range(object min, object max, string type)
    {
        return new WaDataGridCondition(BetweenKind).With(MinKey, min).With(MaxKey, max).With(TypeKey, type);
    }

    private static double NumberBound(double value, string parameterName)
    {
        if (!double.IsFinite(value)) throw new ArgumentException("A bound must be a finite number.", parameterName);
        return value;
    }

    // the text of a number as String(value) shows it in the browser: its JSON form
    private static string NumberKey(double value, string parameterName)
    {
        return JsonSerializer.Serialize(NumberBound(value, parameterName));
    }

    private static string DateBound(DateOnly value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string InstantBound(DateTimeOffset value) => value.ToString(InstantFormat, CultureInfo.InvariantCulture);

    #endregion
}
