using System;

namespace WebAwesome.Blazor.Components;

#region ------ Core Visual Enums ------

/// <summary>
/// Visual style variant for Web Awesome components
/// </summary>
public enum WaVariant
{
    Neutral,
    Brand,
    Success,
    Warning,
    Danger
}

/// <summary>
/// Size variant for Web Awesome components
/// </summary>
public enum WaSize
{
    Small,
    Medium,
    Large
}

public enum WaInputAppearance
{
    Outlined,
    Filled,
}
/// <summary>
/// Visual appearance style for Web Awesome components
/// </summary>
public enum WaAppearance
{
    Filled,
    Outlined,
    OutlinedFilled,
    Text,
    Plain,
    Accent
}

public static class WaAppearanceExt
{
    public static string ToStringForHtml(this WaAppearance appearance)
        => appearance == WaAppearance.OutlinedFilled ? "outlined filled" : appearance.ToString().ToLowerInvariant();
}

/// <summary>
/// Attention effect on a badge.
/// </summary>
public enum WaAttention
{
    Pulse,
    Bounce,
}

/// <summary>
/// Orientation for Web Awesome components
/// </summary>
public enum WaOrientation
{
    Horizontal,
    Vertical
}

#endregion

#region ------ Placement & Positioning ------

/// <summary>
/// Placement positions for Web Awesome components
/// </summary>
public enum WaPlacement
{
    Top,
    Bottom,
    Left,
    Right,
    Start,
    End,
    TopStart,
    TopEnd,
    BottomStart,
    BottomEnd,
    LeftStart,
    LeftEnd,
    RightStart,
    RightEnd
}

/// <summary>
/// Arrow placement for Web Awesome components
/// </summary>
public enum WaArrowPlacement
{
    Anchor,
    Start,
    End,
    Center
}

#endregion

#region ------ Form & Input Enums ------

/// <summary>
/// Button type for Web Awesome buttons
/// </summary>
public enum WaButtonType
{
    Button,
    Submit,
    Reset
}

/// <summary>
/// Input types for Web Awesome input components
/// </summary>
public enum WaInputType
{
    Text,
    Email,
    Number,
    Password,
    Search,
    Tel,
    Url,
    Date
}

#endregion

#region ------ Animation Enums ------

/// <summary>
/// Animation direction for Web Awesome animation component
/// </summary>
public enum WaAnimationDirection
{
    Normal,
    Reverse,
    Alternate,
    AlternateReverse
}

/// <summary>
/// Animation easing functions
/// </summary>
public enum WaAnimationEasing
{
    Linear,
    Ease,
    EaseIn,
    EaseOut,
    EaseInOut
}

/// <summary>
/// Animation fill mode
/// </summary>
public enum WaAnimationFill
{
    None,
    Forwards,
    Backwards,
    Both
}

#endregion

#region ------ Component-Specific Enums ------

/// <summary>
/// Avatar shape variants
/// </summary>
public enum WaAvatarShape
{
    Circle,
    Square,
    Rounded
}

/// <summary>
/// Dropdown trigger types
/// </summary>
[Flags]
public enum WaDropdownTrigger
{
    None = 0,
    Click = 1,
    Hover = 2,
    Focus = 4,
    Manual = 8,
    HoverFocus = Hover | Focus
}

/// <summary>
/// Format number types
/// </summary>
public enum WaFormatNumberType
{
    Decimal,
    Currency,
    Percent
}

/// <summary>
/// Currency display modes
/// </summary>
public enum WaCurrencyDisplay
{
    Symbol,
    Code,
    Name
}

/// <summary>
/// Number notation types
/// </summary>
public enum WaNotation
{
    Standard,
    Scientific,
    Engineering,
    Compact
}

/// <summary>
/// Compact display modes
/// </summary>
public enum WaCompactDisplay
{
    Short,
    Long
}

/// <summary>
/// Auto-size behavior for popups
/// </summary>
[Flags]
public enum WaAutoSize
{
    None = 0,
    Width = 1,
    Height = 2,
    Both = Width | Height
}

/// <summary>
/// Sync width/height behavior
/// </summary>
[Flags]
public enum WaSync
{
    None = 0,
    Width = 1,
    Height = 2,
    Both = Width | Height
}

/// <summary>
/// QR Code error correction levels
/// </summary>
public enum WaErrorCorrection
{
    L,
    M,
    Q,
    H
}

/// <summary>
/// Skeleton animation effects
/// </summary>
public enum WaEffect
{
    Sheen,
    Pulse,
    None
}

/// <summary>
/// Textarea resize behavior
/// </summary>
public enum WaResize
{
    None,
    Vertical,
    Horizontal,
    Both,
    Auto,
}

/// <summary>
/// Tab group activation modes
/// </summary>
public enum WaActivation
{
    Auto,
    Manual
}

/// <summary>
/// Image fit modes
/// </summary>
public enum WaFit
{
    Fill,
    Contain,
    Cover,
    ScaleDown,
    None
}

/// <summary>
/// Image loading behavior
/// </summary>
public enum WaLoading
{
    Eager,
    Lazy
}

/// <summary>
/// Relative time format styles
/// </summary>
public enum WaFormat
{
    Auto,
    Relative,
    Numeric
}

/// <summary>
/// Format bytes display modes
/// </summary>
public enum WaDisplay
{
    Short,
    Long
}

/// <summary>
/// Include request modes
/// </summary>
public enum WaMode
{
    Cors,
    NoCors,
    SameOrigin
}

/// <summary>
/// Split panel primary side
/// </summary>
public enum WaPrimary
{
    Start,
    End
}

/// <summary>
/// Tooltip trigger types
/// </summary>
[Flags]
public enum WaTriggerType
{
    None = 0,
    Hover = 1,
    Focus = 2,
    Manual = 4,
    HoverFocus = Hover | Focus
}

/// <summary>
/// Menu item types
/// </summary>
public enum WaMenuItemType
{
    Normal,
    Checkbox,
    Radio
}

/// <summary>
/// Dropdown item types
/// </summary>
public enum WaDropdownItemType
{
    Normal,
    Checkbox,
    Radio
}

/// <summary>
/// Radio appearance styles
/// </summary>
public enum WaRadioAppearance
{
    /// <summary>Default radio circle appearance</summary>
    Normal,
    /// <summary>Button-style radio appearance</summary>
    Button
}

/// <summary>
/// Tooltip and popover trigger types
/// </summary>
public enum WaTrigger
{
    Hover,
    Click,
    Manual
}

/// <summary>
/// Dialog and drawer placement options
/// </summary>
public enum WaDrawerPlacement
{
    Start,
    End,
    Top,
    Bottom
}

/// <summary>
/// Byte unit for format-bytes component
/// </summary>
public enum WaByteUnit
{
    Byte,
    Bit
}

/// <summary>
/// Date/time style options for format-date component
/// </summary>
public enum WaDateTimeStyle
{
    Long,
    Short,
    Narrow,
    Numeric,
    TwoDigit
}

/// <summary>
/// Hour format for format-date component
/// </summary>
public enum WaHourFormat
{
    Twelve,
    TwentyFour
}

/// <summary>
/// Color format for color picker component
/// </summary>
public enum WaColorFormat
{
    Hex,
    Rgb,
    Hsl,
    Hsv
}

/// <summary>
/// Tab placement for tab group component
/// </summary>
public enum WaTabPlacement
{
    Top,
    Bottom,
    Start,
    End
}

/// <summary>
/// Icon position for details component
/// </summary>
public enum WaIconPosition
{
    Start,
    End
}

#endregion

#region ------ Extension Methods ------

/// <summary>
/// Extension methods for converting Web Awesome enums to HTML attribute values
/// </summary>
public static class WaEnumExtensions
{
    /// <summary>
    /// Converts WaSize to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaSize size)
    {
        return size switch
        {
            WaSize.Small => "small",
            WaSize.Medium => "medium",
            WaSize.Large => "large",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }

    /// <summary>
    /// Converts WaVariant to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaVariant variant)
    {
        return variant switch
        {
            WaVariant.Neutral => "neutral",
            WaVariant.Brand => "brand",
            WaVariant.Success => "success",
            WaVariant.Warning => "warning",
            WaVariant.Danger => "danger",
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
        };
    }

    /// <summary>
    /// Converts WaAppearance to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAppearance appearance)
    {
        return appearance switch
        {
            WaAppearance.Filled => "filled",
            WaAppearance.Outlined => "outlined",
            WaAppearance.OutlinedFilled => "outlined filled",
            WaAppearance.Text => "text",
            WaAppearance.Plain => "plain",
            WaAppearance.Accent => "accent",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts WaInputAppearance to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaInputAppearance appearance)
    {
        return appearance switch
        {
            WaInputAppearance.Outlined => "outlined",
            WaInputAppearance.Filled => "filled",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts WaInputType to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaInputType type)
    {
        return type switch
        {
            WaInputType.Text => "text",
            WaInputType.Email => "email",
            WaInputType.Number => "number",
            WaInputType.Password => "password",
            WaInputType.Search => "search",
            WaInputType.Tel => "tel",
            WaInputType.Url => "url",
            WaInputType.Date => "date",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts WaButtonType to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaButtonType type)
    {
        return type switch
        {
            WaButtonType.Button => "button",
            WaButtonType.Submit => "submit",
            WaButtonType.Reset => "reset",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts WaResize to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaResize resize)
    {
        return resize switch
        {
            WaResize.None => "none",
            WaResize.Vertical => "vertical",
            WaResize.Horizontal => "horizontal",
            WaResize.Both => "both",
            WaResize.Auto => "auto",
            _ => throw new ArgumentOutOfRangeException(nameof(resize), resize, null)
        };
    }

    /// <summary>
    /// Converts WaPlacement to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaPlacement placement)
    {
        return placement switch
        {
            WaPlacement.Top => "top",
            WaPlacement.Bottom => "bottom",
            WaPlacement.Left => "left",
            WaPlacement.Right => "right",
            WaPlacement.Start => "start",
            WaPlacement.End => "end",
            WaPlacement.TopStart => "top-start",
            WaPlacement.TopEnd => "top-end",
            WaPlacement.BottomStart => "bottom-start",
            WaPlacement.BottomEnd => "bottom-end",
            WaPlacement.LeftStart => "left-start",
            WaPlacement.LeftEnd => "left-end",
            WaPlacement.RightStart => "right-start",
            WaPlacement.RightEnd => "right-end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts WaAttention to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAttention attention)
    {
        return attention switch
        {
            WaAttention.Pulse => "pulse",
            WaAttention.Bounce => "bounce",
            _ => throw new ArgumentOutOfRangeException(nameof(attention), attention, null)
        };
    }

    /// <summary>
    /// Converts WaOrientation to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaOrientation orientation)
    {
        return orientation switch
        {
            WaOrientation.Horizontal => "horizontal",
            WaOrientation.Vertical => "vertical",
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    /// <summary>
    /// Converts WaRadioAppearance to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaRadioAppearance appearance)
    {
        return appearance switch
        {
            WaRadioAppearance.Normal => "normal",
            WaRadioAppearance.Button => "button",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts WaAvatarShape to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAvatarShape shape)
    {
        return shape switch
        {
            WaAvatarShape.Circle => "circle",
            WaAvatarShape.Square => "square",
            WaAvatarShape.Rounded => "rounded",
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };
    }

    /// <summary>
    /// Converts WaEffect to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaEffect effect)
    {
        return effect switch
        {
            WaEffect.None => "none",
            WaEffect.Sheen => "sheen",
            WaEffect.Pulse => "pulse",
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, null)
        };
    }

    /// <summary>
    /// Converts WaLoading to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaLoading loading)
    {
        return loading switch
        {
            WaLoading.Eager => "eager",
            WaLoading.Lazy => "lazy",
            _ => throw new ArgumentOutOfRangeException(nameof(loading), loading, null)
        };
    }

    /// <summary>
    /// Converts WaTrigger to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaTrigger trigger)
    {
        return trigger switch
        {
            WaTrigger.Hover => "hover",
            WaTrigger.Click => "click",
            WaTrigger.Manual => "manual",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
    }

    /// <summary>
    /// Converts WaDrawerPlacement to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaDrawerPlacement placement)
    {
        return placement switch
        {
            WaDrawerPlacement.Start => "start",
            WaDrawerPlacement.End => "end",
            WaDrawerPlacement.Top => "top",
            WaDrawerPlacement.Bottom => "bottom",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts WaDropdownItemType to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaDropdownItemType type)
    {
        return type switch
        {
            WaDropdownItemType.Normal => "normal",
            WaDropdownItemType.Checkbox => "checkbox",
            WaDropdownItemType.Radio => "radio",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts WaByteUnit to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaByteUnit unit)
    {
        return unit switch
        {
            WaByteUnit.Byte => "byte",
            WaByteUnit.Bit => "bit",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }

    /// <summary>
    /// Converts WaDateTimeStyle to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaDateTimeStyle style)
    {
        return style switch
        {
            WaDateTimeStyle.Long => "long",
            WaDateTimeStyle.Short => "short",
            WaDateTimeStyle.Narrow => "narrow",
            WaDateTimeStyle.Numeric => "numeric",
            WaDateTimeStyle.TwoDigit => "2-digit",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// Converts WaHourFormat to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaHourFormat format)
    {
        return format switch
        {
            WaHourFormat.Twelve => "12",
            WaHourFormat.TwentyFour => "24",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts WaFormatNumberType to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaFormatNumberType type)
    {
        return type switch
        {
            WaFormatNumberType.Decimal => "decimal",
            WaFormatNumberType.Currency => "currency",
            WaFormatNumberType.Percent => "percent",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts WaCurrencyDisplay to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaCurrencyDisplay display)
    {
        return display switch
        {
            WaCurrencyDisplay.Symbol => "symbol",
            WaCurrencyDisplay.Code => "code",
            WaCurrencyDisplay.Name => "name",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }

    /// <summary>
    /// Converts WaNotation to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaNotation notation)
    {
        return notation switch
        {
            WaNotation.Standard => "standard",
            WaNotation.Scientific => "scientific",
            WaNotation.Engineering => "engineering",
            WaNotation.Compact => "compact",
            _ => throw new ArgumentOutOfRangeException(nameof(notation), notation, null)
        };
    }

    /// <summary>
    /// Converts WaCompactDisplay to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaCompactDisplay display)
    {
        return display switch
        {
            WaCompactDisplay.Short => "short",
            WaCompactDisplay.Long => "long",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }

    /// <summary>
    /// Converts WaColorFormat to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaColorFormat format)
    {
        return format switch
        {
            WaColorFormat.Hex => "hex",
            WaColorFormat.Rgb => "rgb",
            WaColorFormat.Hsl => "hsl",
            WaColorFormat.Hsv => "hsv",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts WaTabPlacement to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaTabPlacement placement)
    {
        return placement switch
        {
            WaTabPlacement.Top => "top",
            WaTabPlacement.Bottom => "bottom",
            WaTabPlacement.Start => "start",
            WaTabPlacement.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts WaActivation to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaActivation activation)
    {
        return activation switch
        {
            WaActivation.Auto => "auto",
            WaActivation.Manual => "manual",
            _ => throw new ArgumentOutOfRangeException(nameof(activation), activation, null)
        };
    }

    /// <summary>
    /// Converts WaMenuItemType to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaMenuItemType type)
    {
        return type switch
        {
            WaMenuItemType.Normal => "normal",
            WaMenuItemType.Checkbox => "checkbox",
            WaMenuItemType.Radio => "radio",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts WaIconPosition to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaIconPosition position)
    {
        return position switch
        {
            WaIconPosition.Start => "start",
            WaIconPosition.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }

    /// <summary>
    /// Converts WaPrimary to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaPrimary primary)
    {
        return primary switch
        {
            WaPrimary.Start => "start",
            WaPrimary.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(primary), primary, null)
        };
    }

    /// <summary>
    /// Converts WaAnimationDirection to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAnimationDirection direction)
    {
        return direction switch
        {
            WaAnimationDirection.Normal => "normal",
            WaAnimationDirection.Reverse => "reverse",
            WaAnimationDirection.Alternate => "alternate",
            WaAnimationDirection.AlternateReverse => "alternate-reverse",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    /// <summary>
    /// Converts WaAnimationEasing to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAnimationEasing easing)
    {
        return easing switch
        {
            WaAnimationEasing.Linear => "linear",
            WaAnimationEasing.Ease => "ease",
            WaAnimationEasing.EaseIn => "ease-in",
            WaAnimationEasing.EaseOut => "ease-out",
            WaAnimationEasing.EaseInOut => "ease-in-out",
            _ => throw new ArgumentOutOfRangeException(nameof(easing), easing, null)
        };
    }

    /// <summary>
    /// Converts WaAnimationFill to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAnimationFill fill)
    {
        return fill switch
        {
            WaAnimationFill.None => "none",
            WaAnimationFill.Forwards => "forwards",
            WaAnimationFill.Backwards => "backwards",
            WaAnimationFill.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(fill), fill, null)
        };
    }

    /// <summary>
    /// Converts WaErrorCorrection to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaErrorCorrection errorCorrection)
    {
        return errorCorrection switch
        {
            WaErrorCorrection.L => "L",
            WaErrorCorrection.M => "M",
            WaErrorCorrection.Q => "Q",
            WaErrorCorrection.H => "H",
            _ => throw new ArgumentOutOfRangeException(nameof(errorCorrection), errorCorrection, null)
        };
    }

    /// <summary>
    /// Converts WaArrowPlacement to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaArrowPlacement placement)
    {
        return placement switch
        {
            WaArrowPlacement.Anchor => "anchor",
            WaArrowPlacement.Start => "start",
            WaArrowPlacement.End => "end",
            WaArrowPlacement.Center => "center",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts WaAutoSize to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaAutoSize autoSize)
    {
        return autoSize switch
        {
            WaAutoSize.None => "",
            WaAutoSize.Width => "width",
            WaAutoSize.Height => "height",
            WaAutoSize.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(autoSize), autoSize, null)
        };
    }

    /// <summary>
    /// Converts WaSync to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaSync sync)
    {
        return sync switch
        {
            WaSync.None => "",
            WaSync.Width => "width",
            WaSync.Height => "height",
            WaSync.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(sync), sync, null)
        };
    }

    /// <summary>
    /// Converts WaMode to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaMode mode)
    {
        return mode switch
        {
            WaMode.Cors => "cors",
            WaMode.NoCors => "no-cors",
            WaMode.SameOrigin => "same-origin",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    /// <summary>
    /// Converts WaFormat to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaFormat format)
    {
        return format switch
        {
            WaFormat.Auto => "auto",
            WaFormat.Relative => "relative",
            WaFormat.Numeric => "numeric",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts WaDisplay to HTML attribute value
    /// </summary>
    public static string ToHtmlValue(this WaDisplay display)
    {
        return display switch
        {
            WaDisplay.Short => "short",
            WaDisplay.Long => "long",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }
}

#endregion