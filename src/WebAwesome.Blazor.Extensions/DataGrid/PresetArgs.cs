using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Collects the options of one cell preset and turns them into the <see cref="WaJsFunction"/> reference of the
/// registered preset function. Only the options that are set are written, so the preset applies its own default for the
/// others. Map keys are written verbatim: the serializer of <see cref="WaJsFunction.WithArgs"/> has no dictionary key
/// policy.
/// </summary>
internal sealed class PresetArgs
{
    /// <summary>
    /// The locale that stands for the invariant culture, whose number and date formats follow American English. The
    /// invariant culture has no BCP 47 name that the browser could read.
    /// </summary>
    public const string InvariantCultureLocale = "en-US";

    /// <summary>
    /// Sets an option when the value is not null.
    /// </summary>
    /// <param name="key">The option name that the preset function reads</param>
    /// <param name="value">The value</param>
    /// <returns>This instance</returns>
    public PresetArgs Set(string key, object? value)
    {
        if (value is not null) values[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the <c>culture</c> option: the name of the given culture, or of the current culture when none is given.
    /// </summary>
    /// <param name="culture">The culture, or null for <see cref="CultureInfo.CurrentCulture"/></param>
    /// <returns>This instance</returns>
    public PresetArgs SetCulture(CultureInfo? culture)
    {
        return Set(CultureKey, LocaleOf(culture ?? CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Sets the <c>nullText</c> option, the text of a null, undefined or empty value.
    /// </summary>
    /// <param name="nullText">The text, or null for an empty cell</param>
    /// <returns>This instance</returns>
    public PresetArgs SetNullText(string? nullText) => Set(NullTextKey, nullText);

    /// <summary>
    /// Creates the reference to the registered preset function with these options.
    /// </summary>
    /// <param name="presetName">The registered name of the preset function</param>
    /// <param name="tags">The Web Awesome elements the preset renders</param>
    /// <returns>The function reference</returns>
    public WaJsFunction ToFunction(string presetName, params string[] tags)
    {
        var function = WaJsFunction.Registered(presetName).WithArgs(values);
        return tags.Length == 0 ? function : function.Uses(tags);
    }

    /// <summary>
    /// The BCP 47 name the browser reads for a culture.
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>The culture name, or <see cref="InvariantCultureLocale"/> for the invariant culture</returns>
    public static string LocaleOf(CultureInfo culture)
    {
        return string.IsNullOrEmpty(culture.Name) ? InvariantCultureLocale : culture.Name;
    }

    /// <summary>
    /// Copies a map of display texts and checks it.
    /// </summary>
    /// <param name="map">The map, value key to text</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    /// <exception cref="ArgumentException">Thrown when a text is null</exception>
    public static Dictionary<string, string>? CopyTexts(IReadOnlyDictionary<string, string>? map, string parameterName)
    {
        if (map is null) return null;
        if (map.Any(entry => entry.Value is null))
            throw new ArgumentException("A text of the map must not be null.", parameterName);

        return map.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// Copies a map of variants as the strings that the elements read.
    /// </summary>
    /// <param name="map">The map, value key to variant</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    public static Dictionary<string, string>? CopyVariants(IReadOnlyDictionary<string, WaVariant>? map)
    {
        return map?.ToDictionary(entry => entry.Key, entry => entry.Value.ToHtmlValue(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Copies a map of icons as the objects that the preset functions read.
    /// </summary>
    /// <param name="map">The map, value key to icon</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    /// <exception cref="ArgumentException">Thrown when an icon is null</exception>
    public static Dictionary<string, object>? CopyIcons(IReadOnlyDictionary<string, WaDataGridIcon>? map, string parameterName)
    {
        if (map is null) return null;
        if (map.Any(entry => entry.Value is null))
            throw new ArgumentException("An icon of the map must not be null.", parameterName);

        return map.ToDictionary(entry => entry.Key, entry => (object)IconOf(entry.Value), StringComparer.Ordinal);
    }

    /// <summary>
    /// The object that the preset functions read for an icon: the name, and the family, variant and library when set.
    /// </summary>
    /// <param name="icon">The icon</param>
    /// <returns>The icon object, or null when the icon is null</returns>
    public static object? IconOrNull(WaDataGridIcon? icon) => icon is null ? null : IconOf(icon);

    #region ------ Internals ------

    private const string CultureKey = "culture";
    private const string NullTextKey = "nullText";

    // the icon object of design section 6.3: name, family, variant, library
    private const string IconNameKey = "name";
    private const string IconFamilyKey = "family";
    private const string IconVariantKey = "variant";
    private const string IconLibraryKey = "library";

    private static Dictionary<string, object> IconOf(WaDataGridIcon icon)
    {
        var result = new Dictionary<string, object> { [IconNameKey] = icon.Name };
        if (icon.Family is not null) result[IconFamilyKey] = icon.Family;
        if (icon.Variant is not null) result[IconVariantKey] = icon.Variant;
        if (icon.Library is not null) result[IconLibraryKey] = icon.Library;
        return result;
    }

    // insertion order is the JSON order, so the descriptor of the same options is always the same text
    private readonly Dictionary<string, object> values = new(StringComparer.Ordinal);

    #endregion
}
