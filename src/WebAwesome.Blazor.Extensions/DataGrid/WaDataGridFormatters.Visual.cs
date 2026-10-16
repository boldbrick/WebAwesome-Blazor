using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

// more visual presets (design section 6.3): elements that show a value
public static partial class WaDataGridFormatters
{
    /// <summary>
    /// The icon of a positive value of <see cref="Trend"/> when no icon is given.
    /// </summary>
    public const string DefaultTrendUpIconName = "arrow-trend-up";

    /// <summary>
    /// The icon of a negative value of <see cref="Trend"/> when no icon is given.
    /// </summary>
    public const string DefaultTrendDownIconName = "arrow-trend-down";

    /// <summary>
    /// A <c>wa-progress-bar</c> for a share, for example the progress of a task. Its accessible name is the localized
    /// percentage. Output: an element.
    /// </summary>
    /// <param name="scale">The scale of the values: a fraction (0.25 is 25%, the default) or a percentage (25 is
    /// 25%)</param>
    /// <param name="showLabel">Whether the bar shows the percentage as its text</param>
    /// <param name="thresholds">Color steps by value, for example green from 0.8; the default color below the first
    /// step</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits of the percentage (0 to 100)</param>
    /// <param name="nullText">The text of a null or empty value (no bar); empty by default</param>
    /// <param name="culture">The culture of the percentage; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a threshold is null or the digit count is out of range</exception>
    public static WaJsFunction ProgressBar(WaDataGridPercentScale? scale = null, bool showLabel = false,
        IReadOnlyList<WaDataGridThreshold>? thresholds = null, int? maxFractionDigits = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        return ProgressArgs(scale, showLabel, thresholds, maxFractionDigits, nullText, culture).ToFunction(PresetNames.ProgressBar, ProgressBarTag);
    }

    /// <summary>
    /// A <c>wa-progress-ring</c> for a share, a compact form of <see cref="ProgressBar"/>. Output: an element.
    /// </summary>
    /// <param name="scale">The scale of the values: a fraction (the default) or a percentage</param>
    /// <param name="showLabel">Whether the ring shows the percentage in its center</param>
    /// <param name="thresholds">Color steps by value; the default color below the first step</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits of the percentage (0 to 100)</param>
    /// <param name="nullText">The text of a null or empty value (no ring); empty by default</param>
    /// <param name="culture">The culture of the percentage; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a threshold is null or the digit count is out of range</exception>
    public static WaJsFunction ProgressRing(WaDataGridPercentScale? scale = null, bool showLabel = false,
        IReadOnlyList<WaDataGridThreshold>? thresholds = null, int? maxFractionDigits = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        return ProgressArgs(scale, showLabel, thresholds, maxFractionDigits, nullText, culture).ToFunction(PresetNames.ProgressRing, ProgressRingTag);
    }

    /// <summary>
    /// A read-only <c>wa-rating</c> for a number, for example a review score. Its accessible name is the value and the
    /// maximum ("4 / 5"). Output: an element.
    /// </summary>
    /// <param name="max">The highest rating; the default of <c>wa-rating</c> (5) when null</param>
    /// <param name="precision">The step of the shown value, for example 0.5 for half symbols; the default of
    /// <c>wa-rating</c> when null</param>
    /// <param name="size">The size of the symbols</param>
    /// <param name="nullText">The text of a null or empty value (no rating); empty by default</param>
    /// <param name="culture">The culture of the accessible name; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when max or precision is not positive</exception>
    public static WaJsFunction Rating(int? max = null, double? precision = null, WaSize? size = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        if (max is <= 0) throw new ArgumentException("The highest rating must be positive.", nameof(max));
        if (precision is not null && (!double.IsFinite(precision.Value) || precision <= 0))
            throw new ArgumentException("The precision must be a positive number.", nameof(precision));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(MaxKey, max)
            .Set(PrecisionKey, precision)
            .Set(SizeKey, size?.ToHtmlValue())
            .ToFunction(PresetNames.Rating, RatingTag);
    }

    /// <summary>
    /// A <c>wa-avatar</c> for a person or an item: an image, initials or the default icon, with the name as its
    /// accessible name. Output: an element.
    /// </summary>
    /// <param name="imageField">The path of a row field with the image URL (relative, http or https)</param>
    /// <param name="initialsField">The path of a row field with the initials</param>
    /// <param name="labelField">The path of a row field with the name; the value of the cell when null</param>
    /// <param name="shape">The shape of the avatar</param>
    /// <param name="showLabel">Whether the name shows next to the avatar</param>
    /// <param name="size">The size of the avatar, a CSS length such as <c>2rem</c>; the default of <c>wa-avatar</c>
    /// when null</param>
    /// <param name="nullText">The text of a row with no name, image or initials; empty by default</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a field path or the size is not valid</exception>
    public static WaJsFunction Avatar(string? imageField = null, string? initialsField = null, string? labelField = null,
        WaAvatarShape? shape = null, bool showLabel = false, string? size = null, string? nullText = null)
    {
        ValidateOptionalField(imageField, nameof(imageField));
        ValidateOptionalField(initialsField, nameof(initialsField));
        ValidateOptionalField(labelField, nameof(labelField));
        ValidateCssLength(size, nameof(size));

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(ImageFieldKey, imageField)
            .Set(InitialsFieldKey, initialsField)
            .Set(LabelFieldKey, labelField)
            .Set(ShapeKey, shape?.ToHtmlValue())
            .Set(ShowLabelKey, showLabel ? true : null)
            .Set(SizeKey, size)
            .ToFunction(PresetNames.Avatar, AvatarTag);
    }

    /// <summary>
    /// An image (an <c>img</c> element, loaded lazily). Only relative URLs and the schemes http and https are allowed:
    /// a cell whose URL has another scheme shows no image. Output: an element.
    /// </summary>
    /// <param name="srcTemplate">The URL template, with <c>{field}</c> placeholders whose values are URL-encoded; when
    /// null, the value of the cell is the URL</param>
    /// <param name="altTemplate">The alternative text, a template as in <see cref="Template"/>; when null, the alternative
    /// text is empty and the image is decoration</param>
    /// <param name="width">The width in pixels</param>
    /// <param name="height">The height in pixels</param>
    /// <param name="nullText">The text of a cell without an image; empty by default</param>
    /// <param name="culture">The culture of the formats of the alternative text template;
    /// <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a template is not valid or names another scheme, or a size is not
    /// positive</exception>
    public static WaJsFunction Image(string? srcTemplate = null, string? altTemplate = null, int? width = null, int? height = null,
        string? nullText = null, CultureInfo? culture = null)
    {
        if (srcTemplate is not null) CellTemplate.ValidateImageUrl(srcTemplate, nameof(srcTemplate));
        if (altTemplate is not null) CellTemplate.ValidateText(altTemplate, nameof(altTemplate));
        if (width is <= 0) throw new ArgumentException("The width must be positive.", nameof(width));
        if (height is <= 0) throw new ArgumentException("The height must be positive.", nameof(height));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(SrcKey, srcTemplate)
            .Set(AltKey, altTemplate)
            .Set(WidthKey, width)
            .Set(HeightKey, height)
            .ToFunction(PresetNames.Image);
    }

    /// <summary>
    /// A color swatch for a CSS color value, for example <c>#ff8800</c>. A value that is not a CSS color shows as text.
    /// Output: an element.
    /// </summary>
    /// <param name="showText">Whether the color value shows next to the swatch; else it is the accessible name</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction ColorSwatch(bool showText = false, string? nullText = null)
    {
        return new PresetArgs()
            .SetNullText(nullText)
            .Set(ShowTextKey, showText ? true : null)
            .ToFunction(PresetNames.ColorSwatch);
    }

    /// <summary>
    /// A <c>wa-sparkline</c> for an array of numbers, for example the sales of the last weeks. Output: an element.
    /// </summary>
    /// <param name="appearance">The fill of the sparkline</param>
    /// <param name="curve">The curve between the points</param>
    /// <param name="trend">The trend that sets the color</param>
    /// <param name="trendField">The path of a row field with the trend of the row (<c>positive</c>, <c>negative</c> or
    /// <c>neutral</c>); it wins over <paramref name="trend"/></param>
    /// <param name="labelTemplate">The accessible name, a template as in <see cref="Template"/>; the localized values
    /// when null</param>
    /// <param name="nullText">The text of a value without numbers; empty by default</param>
    /// <param name="culture">The culture of the label; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the field path or the label template is not valid</exception>
    public static WaJsFunction Sparkline(WaSparklineAppearance? appearance = null, WaSparklineCurve? curve = null,
        WaSparklineTrend? trend = null, string? trendField = null, string? labelTemplate = null, string? nullText = null,
        CultureInfo? culture = null)
    {
        ValidateOptionalField(trendField, nameof(trendField));
        if (labelTemplate is not null) CellTemplate.ValidateText(labelTemplate, nameof(labelTemplate));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(AppearanceKey, appearance?.ToHtmlValue())
            .Set(CurveKey, curve?.ToHtmlValue())
            .Set(TrendKey, trend?.ToHtmlValue())
            .Set(TrendFieldKey, trendField)
            .Set(LabelKey, labelTemplate)
            .ToFunction(PresetNames.Sparkline, SparklineTag);
    }

    /// <summary>
    /// A number with an icon and a color for its direction, for example a change against the last period. Output: an
    /// element.
    /// </summary>
    /// <param name="format">The text format of the number, a text preset such as <see cref="Percent"/>; a localized
    /// number with its sign when null</param>
    /// <param name="positiveVariant">The color of a positive value; <see cref="WaVariant.Success"/> when null</param>
    /// <param name="negativeVariant">The color of a negative value; <see cref="WaVariant.Danger"/> when null</param>
    /// <param name="upIcon">The icon of a positive value; <see cref="DefaultTrendUpIconName"/> when null</param>
    /// <param name="downIcon">The icon of a negative value; <see cref="DefaultTrendDownIconName"/> when null</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when the format is not a text preset</exception>
    public static WaJsFunction Trend(WaJsFunction? format = null, WaVariant? positiveVariant = null, WaVariant? negativeVariant = null,
        WaDataGridIcon? upIcon = null, WaDataGridIcon? downIcon = null, string? nullText = null, CultureInfo? culture = null)
    {
        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(FormatKey, PresetArgs.TextFormatOrNull(format, nameof(format)))
            .Set(PositiveVariantKey, (positiveVariant ?? WaVariant.Success).ToHtmlValue())
            .Set(NegativeVariantKey, (negativeVariant ?? WaVariant.Danger).ToHtmlValue())
            .Set(UpIconKey, PresetArgs.IconOrNull(upIcon ?? DefaultTrendUpIconName))
            .Set(DownIconKey, PresetArgs.IconOrNull(downIcon ?? DefaultTrendDownIconName))
            .ToFunction(PresetNames.Trend, IconTag);
    }

    /// <summary>
    /// A horizontal bar whose length shows the value between a minimum and a maximum, with the formatted value as its
    /// accessible name. Output: an element.
    /// </summary>
    /// <param name="min">The value of an empty bar; when null, 0 or the lowest negative value of the loaded rows</param>
    /// <param name="max">The value of a full bar; when null, the highest value of the loaded rows</param>
    /// <param name="variant">The color of the bar; <see cref="WaVariant.Brand"/> when null</param>
    /// <param name="showValue">Whether the formatted value shows next to the bar</param>
    /// <param name="format">The text format of the value, a text preset such as <see cref="Currency"/>; a localized
    /// number when null</param>
    /// <param name="nullText">The text of a null or empty value (no bar); empty by default</param>
    /// <param name="culture">The culture of the default format; <see cref="CultureInfo.CurrentCulture"/> when null</param>
    /// <returns>The formatter reference</returns>
    /// <exception cref="ArgumentException">Thrown when a limit is not finite, min is not lower than max, or the format is
    /// not a text preset</exception>
    /// <remarks>The computed limits come from the rows the grid holds. In server mode, that is the loaded page only, so
    /// the bars of two pages do not compare: set <paramref name="min"/> and <paramref name="max"/> there.</remarks>
    public static WaJsFunction DataBar(double? min = null, double? max = null, WaVariant? variant = null, bool showValue = false,
        WaJsFunction? format = null, string? nullText = null, CultureInfo? culture = null)
    {
        if (min is not null && !double.IsFinite(min.Value)) throw new ArgumentException("The minimum must be a finite number.", nameof(min));
        if (max is not null && !double.IsFinite(max.Value)) throw new ArgumentException("The maximum must be a finite number.", nameof(max));
        if (min >= max) throw new ArgumentException("The minimum must be lower than the maximum.", nameof(min));

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(MinKey, min)
            .Set(MaxKey, max)
            .Set(VariantKey, variant?.ToHtmlValue())
            .Set(ShowValueKey, showValue ? true : null)
            .Set(FormatKey, PresetArgs.TextFormatOrNull(format, nameof(format)))
            .ToFunction(PresetNames.DataBar);
    }

    /// <summary>
    /// A <c>wa-copy-button</c> that copies the value of the cell, for example an id or a key. A click on the button does
    /// not raise <c>OnCellClick</c>. Output: an element.
    /// </summary>
    /// <param name="showValue">Whether the value shows in front of the button</param>
    /// <param name="copyLabel">The tooltip before the copy; the localized default of <c>wa-copy-button</c> when
    /// null</param>
    /// <param name="successLabel">The tooltip after the copy; the localized default of <c>wa-copy-button</c> when
    /// null</param>
    /// <param name="nullText">The text of a null or empty value (no button); empty by default</param>
    /// <returns>The formatter reference</returns>
    public static WaJsFunction CopyButton(bool showValue = false, string? copyLabel = null, string? successLabel = null,
        string? nullText = null)
    {
        return new PresetArgs()
            .SetNullText(nullText)
            .Set(ShowValueKey, showValue ? true : null)
            .Set(CopyLabelKey, copyLabel)
            .Set(SuccessLabelKey, successLabel)
            .ToFunction(PresetNames.CopyButton, CopyButtonTag);
    }

    #region ------ Internals ------

    // the option names of the visual presets
    private const string ThresholdsKey = "thresholds";
    private const string ThresholdFromKey = "from";
    private const string ThresholdVariantKey = "variant";
    private const string MaxKey = "max";
    private const string MinKey = "min";
    private const string PrecisionKey = "precision";
    private const string ImageFieldKey = "imageField";
    private const string InitialsFieldKey = "initialsField";
    private const string LabelFieldKey = "labelField";
    private const string ShapeKey = "shape";
    private const string SrcKey = "src";
    private const string AltKey = "alt";
    private const string WidthKey = "width";
    private const string HeightKey = "height";
    private const string ShowTextKey = "showText";
    private const string CurveKey = "curve";
    private const string TrendKey = "trend";
    private const string TrendFieldKey = "trendField";
    private const string FormatKey = "format";
    private const string PositiveVariantKey = "positiveVariant";
    private const string NegativeVariantKey = "negativeVariant";
    private const string UpIconKey = "upIcon";
    private const string DownIconKey = "downIcon";
    private const string ShowValueKey = "showValue";
    private const string CopyLabelKey = "copyLabel";
    private const string SuccessLabelKey = "successLabel";

    // the elements the visual presets render
    private const string ProgressBarTag = "wa-progress-bar";
    private const string ProgressRingTag = "wa-progress-ring";
    private const string RatingTag = "wa-rating";
    private const string AvatarTag = "wa-avatar";
    private const string SparklineTag = "wa-sparkline";
    private const string CopyButtonTag = "wa-copy-button";

    // characters that would end a CSS declaration or open a block
    private static readonly char[] CssDelimiters = [';', '{', '}'];

    private static void ValidateCssLength(string? length, string parameterName)
    {
        if (length is not null && (string.IsNullOrWhiteSpace(length) || length.IndexOfAny(CssDelimiters) >= 0 || length.Any(char.IsControl)))
            throw new ArgumentException($"'{length}' is not a CSS length such as '2rem'.", parameterName);
    }

    private static PresetArgs ProgressArgs(WaDataGridPercentScale? scale, bool showLabel, IReadOnlyList<WaDataGridThreshold>? thresholds,
        int? maxFractionDigits, string? nullText, CultureInfo? culture)
    {
        ValidateFractionDigits(null, maxFractionDigits);
        if (thresholds is not null && thresholds.Any(threshold => threshold is null))
            throw new ArgumentException("A threshold must not be null.", nameof(thresholds));

        // ascending, so that the last step a value reaches gives its color
        var steps = thresholds?
            .OrderBy(threshold => threshold.From)
            .Select(threshold => new Dictionary<string, object>
            {
                [ThresholdFromKey] = threshold.From,
                [ThresholdVariantKey] = threshold.Variant.ToHtmlValue(),
            })
            .ToList();

        return new PresetArgs()
            .SetCulture(culture)
            .SetNullText(nullText)
            .Set(ScaleKey, scale?.ToJsValue())
            .Set(ShowLabelKey, showLabel ? true : null)
            .Set(ThresholdsKey, steps is { Count: > 0 } ? steps : null)
            .Set(MaxFractionDigitsKey, maxFractionDigits);
    }

    #endregion
}
