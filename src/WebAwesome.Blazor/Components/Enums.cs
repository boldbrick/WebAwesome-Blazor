using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

#region ------ Core Visual Enums ------

/// <summary>
/// Visual style variant for Web Awesome components
/// </summary>
public enum WaVariant
{
    /// <summary>Neutral, unemphasized variant.</summary>
    Neutral,
    /// <summary>Brand-colored variant.</summary>
    Brand,
    /// <summary>Success (positive) variant.</summary>
    Success,
    /// <summary>Warning (cautionary) variant.</summary>
    Warning,
    /// <summary>Danger (destructive/error) variant.</summary>
    Danger
}

/// <summary>
/// Size variant for Web Awesome components
/// </summary>
public enum WaSize
{
    /// <summary>Small size.</summary>
    Small,
    /// <summary>Medium size.</summary>
    Medium,
    /// <summary>Large size.</summary>
    Large,
    /// <summary>Extra-small size.</summary>
    ExtraSmall,
    /// <summary>Extra-large size.</summary>
    ExtraLarge
}

/// <summary>
/// Visual appearance for form input components.
/// </summary>
public enum WaInputAppearance
{
    /// <summary>Draws the input with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws the input with a filled background.</summary>
    Filled,
    /// <summary>Draws the input with both an outline and a filled background.</summary>
    FilledOutlined,
}
/// <summary>
/// Visual appearance style for the button, callout and card components, whose appearance attribute takes
/// 'accent' | 'filled' | 'outlined' | 'filled-outlined' | 'plain'.
/// </summary>
public enum WaAppearance
{
    /// <summary>Draws the component with a filled background.</summary>
    Filled,
    /// <summary>Draws the component with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws the component with both an outline and a filled background.</summary>
    OutlinedFilled,
    /// <summary>Draws the component with minimal, unstyled chrome.</summary>
    Plain,
    /// <summary>Draws the component using the accent theme styling.</summary>
    Accent
}

/// <summary>
/// Visual appearance for the details and accordion components, whose appearance attribute takes
/// 'filled' | 'outlined' | 'filled-outlined' | 'plain'.
/// </summary>
public enum WaDetailsAppearance
{
    /// <summary>Draws the component with a filled background.</summary>
    Filled,
    /// <summary>Draws the component with an outline and no fill (Web Awesome default).</summary>
    Outlined,
    /// <summary>Draws the component with both an outline and a filled background.</summary>
    FilledOutlined,
    /// <summary>Draws the component with minimal, unstyled chrome.</summary>
    Plain
}

/// <summary>
/// Visual appearance for the badge and tag components, whose appearance attribute takes
/// 'accent' | 'filled' | 'outlined' | 'filled-outlined'.
/// </summary>
public enum WaBadgeAppearance
{
    /// <summary>Draws the component using the accent theme styling (the badge default).</summary>
    Accent,
    /// <summary>Draws the component with a filled background.</summary>
    Filled,
    /// <summary>Draws the component with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws the component with both an outline and a filled background (the tag default).</summary>
    FilledOutlined
}

/// <summary>
/// Extension methods for converting <see cref="WaAppearance"/> to its raw string representation.
/// </summary>
public static class WaAppearanceExt
{
    /// <summary>
    /// Converts the value to the string used by the underlying HTML/CSS appearance token.
    /// </summary>
    /// <param name="appearance">The appearance value to convert</param>
    /// <returns>The lowercase string representation, with <see cref="WaAppearance.OutlinedFilled"/> rendered as "filled-outlined"</returns>
    public static string ToStringForHtml(this WaAppearance appearance)
        => appearance == WaAppearance.OutlinedFilled ? "filled-outlined" : appearance.ToString().ToLowerInvariant();
}

/// <summary>
/// Attention effect on a badge.
/// </summary>
public enum WaAttention
{
    /// <summary>Pulses to draw attention.</summary>
    Pulse,
    /// <summary>Bounces to draw attention.</summary>
    Bounce,
    /// <summary>No attention animation (the Web Awesome default).</summary>
    None,
}

/// <summary>
/// Orientation for Web Awesome components
/// </summary>
public enum WaOrientation
{
    /// <summary>Lays out the component horizontally.</summary>
    Horizontal,
    /// <summary>Lays out the component vertically.</summary>
    Vertical
}

/// <summary>
/// The layout direction of a wa-stepper.
/// </summary>
public enum WaStepperOrientation
{
    /// <summary>Lays the steps out in a row (the Web Awesome default).</summary>
    Horizontal,
    /// <summary>Stacks the steps in a column.</summary>
    Vertical,
    /// <summary>Lays the steps out in a row and stacks them once the stepper is too narrow for every step's label.</summary>
    Auto,
}

/// <summary>
/// The placement of a wa-divider's label along the divider line.
/// </summary>
public enum WaDividerLabelPlacement
{
    /// <summary>Places the label at the start of the divider.</summary>
    Start,
    /// <summary>Places the label at the center of the divider (the Web Awesome default).</summary>
    Center,
    /// <summary>Places the label at the end of the divider.</summary>
    End,
}

#endregion

#region ------ Placement & Positioning ------

/// <summary>
/// Placement positions for floating elements, whose placement attribute takes top | bottom | left | right, each
/// optionally suffixed with -start or -end (12 values). Used by <see cref="WaTooltip.Placement"/>,
/// <see cref="WaPopover.Placement"/>, <see cref="WaPopup.Placement"/>, <see cref="WaDropdown.Placement"/> and
/// <see cref="WaColorPicker.Placement"/>. Components with a narrower placement union use their own enums:
/// <see cref="WaListboxPlacement"/>, <see cref="WaPickerPlacement"/> and <see cref="WaTooltipSide"/>.
/// </summary>
public enum WaPlacement
{
    /// <summary>Places the element above its anchor.</summary>
    Top,
    /// <summary>Places the element below its anchor.</summary>
    Bottom,
    /// <summary>Places the element to the left of its anchor.</summary>
    Left,
    /// <summary>Places the element to the right of its anchor.</summary>
    Right,
    /// <summary>Places the element above and aligned to the start of its anchor.</summary>
    TopStart,
    /// <summary>Places the element above and aligned to the end of its anchor.</summary>
    TopEnd,
    /// <summary>Places the element below and aligned to the start of its anchor.</summary>
    BottomStart,
    /// <summary>Places the element below and aligned to the end of its anchor.</summary>
    BottomEnd,
    /// <summary>Places the element to the left and aligned to the start of its anchor.</summary>
    LeftStart,
    /// <summary>Places the element to the left and aligned to the end of its anchor.</summary>
    LeftEnd,
    /// <summary>Places the element to the right and aligned to the start of its anchor.</summary>
    RightStart,
    /// <summary>Places the element to the right and aligned to the end of its anchor.</summary>
    RightEnd
}

/// <summary>
/// Preferred placement of the listbox popup relative to its field, for the select and combobox components, whose
/// placement attribute takes 'top' | 'bottom'. Used by <see cref="WaSelect.Placement"/> and
/// <see cref="WaCombobox.Placement"/>.
/// </summary>
public enum WaListboxPlacement
{
    /// <summary>Opens the listbox above the field.</summary>
    Top,
    /// <summary>Opens the listbox below the field (Web Awesome default).</summary>
    Bottom
}

/// <summary>
/// Preferred placement of the picker popup relative to its field, for the date input and time input components, whose
/// placement attribute takes 'top' | 'top-start' | 'top-end' | 'bottom' | 'bottom-start' | 'bottom-end'. Used by
/// <see cref="Base.WaDateInputBase{TValue}.Placement"/> and <see cref="WaTimeInput.Placement"/>.
/// </summary>
public enum WaPickerPlacement
{
    /// <summary>Opens the popup above the field, centered.</summary>
    Top,
    /// <summary>Opens the popup above the field, aligned to its start.</summary>
    TopStart,
    /// <summary>Opens the popup above the field, aligned to its end.</summary>
    TopEnd,
    /// <summary>Opens the popup below the field, centered.</summary>
    Bottom,
    /// <summary>Opens the popup below the field, aligned to its start (Web Awesome default).</summary>
    BottomStart,
    /// <summary>Opens the popup below the field, aligned to its end.</summary>
    BottomEnd
}

/// <summary>
/// Side of the host on which a built-in tooltip is shown, for the slider and copy button components, whose
/// tooltip-placement attribute takes 'top' | 'right' | 'bottom' | 'left'. Used by
/// <see cref="Base.WaSliderBase{TValue}.TooltipPlacement"/> (<see cref="WaSlider"/>, <see cref="WaRange"/>) and <see cref="WaCopyButton.TooltipPlacement"/>.
/// </summary>
public enum WaTooltipSide
{
    /// <summary>Shows the tooltip above the host (Web Awesome default).</summary>
    Top,
    /// <summary>Shows the tooltip to the right of the host.</summary>
    Right,
    /// <summary>Shows the tooltip below the host.</summary>
    Bottom,
    /// <summary>Shows the tooltip to the left of the host.</summary>
    Left
}

/// <summary>
/// Arrow placement for Web Awesome components
/// </summary>
public enum WaArrowPlacement
{
    /// <summary>Positions the arrow relative to the anchor element.</summary>
    Anchor,
    /// <summary>Positions the arrow at the start of the popup.</summary>
    Start,
    /// <summary>Positions the arrow at the end of the popup.</summary>
    End,
    /// <summary>Centers the arrow on the popup.</summary>
    Center
}

#endregion

#region ------ Form & Input Enums ------

/// <summary>
/// Button type for Web Awesome buttons
/// </summary>
public enum WaButtonType
{
    /// <summary>A plain, non-submitting button.</summary>
    Button,
    /// <summary>Submits the associated form.</summary>
    Submit,
    /// <summary>Resets the associated form.</summary>
    Reset
}

/// <summary>
/// Input types for Web Awesome input components
/// </summary>
public enum WaInputType
{
    /// <summary>Plain text input.</summary>
    Text,
    /// <summary>Email address input.</summary>
    Email,
    /// <summary>Numeric input.</summary>
    Number,
    /// <summary>Password input with obscured characters.</summary>
    Password,
    /// <summary>Search input.</summary>
    Search,
    /// <summary>Telephone number input.</summary>
    Tel,
    /// <summary>URL input.</summary>
    Url,
    /// <summary>Date input.</summary>
    Date,
    /// <summary>Local date and time input.</summary>
    DateTimeLocal,
    /// <summary>Time input.</summary>
    Time
}

/// <summary>
/// Whether and how text entered by the user is automatically capitalized (the HTML autocapitalize attribute).
/// </summary>
public enum WaAutoCapitalize
{
    /// <summary>No automatic capitalization ("off").</summary>
    Off,
    /// <summary>No automatic capitalization ("none", a synonym of off).</summary>
    None,
    /// <summary>Capitalizes the first letter of each sentence ("on").</summary>
    On,
    /// <summary>Capitalizes the first letter of each sentence ("sentences", a synonym of on).</summary>
    Sentences,
    /// <summary>Capitalizes the first letter of each word.</summary>
    Words,
    /// <summary>Capitalizes every letter.</summary>
    Characters
}

/// <summary>
/// The label or icon of the Enter key on virtual keyboards (the HTML enterkeyhint attribute).
/// </summary>
public enum WaEnterKeyHint
{
    /// <summary>Inserts a new line.</summary>
    Enter,
    /// <summary>Nothing more to input; closes the keyboard.</summary>
    Done,
    /// <summary>Takes the user to the target of the typed text.</summary>
    Go,
    /// <summary>Moves to the next field.</summary>
    Next,
    /// <summary>Moves to the previous field.</summary>
    Previous,
    /// <summary>Searches for the typed text.</summary>
    Search,
    /// <summary>Sends the typed text.</summary>
    Send
}

/// <summary>
/// The kind of data the user enters, which selects the virtual keyboard (the HTML inputmode attribute).
/// </summary>
public enum WaInputMode
{
    /// <summary>No virtual keyboard.</summary>
    None,
    /// <summary>The locale's standard text keyboard.</summary>
    Text,
    /// <summary>A numeric keyboard with the locale's decimal separator.</summary>
    Decimal,
    /// <summary>A numeric keyboard.</summary>
    Numeric,
    /// <summary>A telephone keypad.</summary>
    Tel,
    /// <summary>A keyboard optimized for search.</summary>
    Search,
    /// <summary>A keyboard optimized for email addresses.</summary>
    Email,
    /// <summary>A keyboard optimized for URLs.</summary>
    Url
}

/// <summary>
/// The virtual keyboard of a number input: digits only, or digits with the decimal separator.
/// </summary>
public enum WaNumberInputMode
{
    /// <summary>A numeric keyboard (the Web Awesome default).</summary>
    Numeric,
    /// <summary>A numeric keyboard with the locale's decimal separator.</summary>
    Decimal
}

/// <summary>
/// The browsing context a link opens in (the HTML target attribute of a link).
/// </summary>
public enum WaLinkTarget
{
    /// <summary>A new tab or window ("_blank").</summary>
    Blank,
    /// <summary>The parent browsing context ("_parent").</summary>
    Parent,
    /// <summary>The current browsing context ("_self").</summary>
    Self,
    /// <summary>The topmost browsing context ("_top").</summary>
    Top
}

/// <summary>
/// The HTTP method a submit button uses to submit its form (the HTML formmethod attribute).
/// </summary>
public enum WaFormMethod
{
    /// <summary>Submits the form data in the request body.</summary>
    Post,
    /// <summary>Submits the form data in the URL query.</summary>
    Get
}

/// <summary>
/// How a submit button encodes its form data (the HTML formenctype attribute).
/// </summary>
public enum WaFormEncType
{
    /// <summary>application/x-www-form-urlencoded, the browser default.</summary>
    UrlEncoded,
    /// <summary>multipart/form-data, required to upload files.</summary>
    MultipartFormData,
    /// <summary>text/plain.</summary>
    TextPlain
}

#endregion

#region ------ Popup Enums ------

/// <summary>
/// How a popup picks its placement when it is flipped and no fallback placement fits.
/// </summary>
public enum WaFlipFallbackStrategy
{
    /// <summary>Uses the fallback placement with the most space ("best-fit").</summary>
    BestFit,
    /// <summary>Uses the initial placement ("initial").</summary>
    Initial
}

/// <summary>
/// The boundary a popup's flip and shift behaviour keeps the popup within.
/// </summary>
public enum WaPopupBoundary
{
    /// <summary>The viewport.</summary>
    Viewport,
    /// <summary>The popup's scroll container.</summary>
    Scroll
}

#endregion

#region ------ Animation Enums ------

/// <summary>
/// Animation direction for Web Awesome animation component
/// </summary>
public enum WaAnimationDirection
{
    /// <summary>Plays the animation forward.</summary>
    Normal,
    /// <summary>Plays the animation backward.</summary>
    Reverse,
    /// <summary>Alternates between forward and backward on each iteration.</summary>
    Alternate,
    /// <summary>Alternates between backward and forward on each iteration.</summary>
    AlternateReverse
}

/// <summary>
/// Animation easing functions
/// </summary>
public enum WaAnimationEasing
{
    /// <summary>Constant speed throughout the animation.</summary>
    Linear,
    /// <summary>Starts slow, speeds up, then slows down.</summary>
    Ease,
    /// <summary>Starts slow and accelerates.</summary>
    EaseIn,
    /// <summary>Starts fast and decelerates.</summary>
    EaseOut,
    /// <summary>Starts slow, speeds up in the middle, and slows down again.</summary>
    EaseInOut
}

/// <summary>
/// Animation fill mode
/// </summary>
public enum WaAnimationFill
{
    /// <summary>No styles are applied before or after the animation runs.</summary>
    None,
    /// <summary>Retains the styles from the last keyframe after the animation ends.</summary>
    Forwards,
    /// <summary>Applies the styles from the first keyframe before the animation starts.</summary>
    Backwards,
    /// <summary>Combines the effects of <see cref="Forwards"/> and <see cref="Backwards"/>.</summary>
    Both,
    /// <summary>
    /// Lets the Web Animations API decide (Web Awesome default); for keyframe effects it behaves like <see cref="None"/>.
    /// </summary>
    Auto
}

#endregion

#region ------ Component-Specific Enums ------

/// <summary>
/// Avatar shape variants
/// </summary>
public enum WaAvatarShape
{
    /// <summary>Circular shape.</summary>
    Circle,
    /// <summary>Square shape with sharp corners.</summary>
    Square,
    /// <summary>Square shape with rounded corners.</summary>
    Rounded
}

/// <summary>
/// Format number types
/// </summary>
public enum WaFormatNumberType
{
    /// <summary>Formats the number as a plain decimal.</summary>
    Decimal,
    /// <summary>Formats the number as a currency value.</summary>
    Currency,
    /// <summary>Formats the number as a percentage.</summary>
    Percent
}

/// <summary>
/// Currency display modes
/// </summary>
public enum WaCurrencyDisplay
{
    /// <summary>Displays the currency symbol, e.g. "$".</summary>
    Symbol,
    /// <summary>Displays the narrow currency symbol, e.g. "$" instead of "US$".</summary>
    NarrowSymbol,
    /// <summary>Displays the currency code, e.g. "USD".</summary>
    Code,
    /// <summary>Displays the localized currency name, e.g. "dollar".</summary>
    Name
}

/// <summary>
/// Auto-size behavior for popups; the auto-size attribute takes 'horizontal' | 'vertical' | 'both'
/// </summary>
[Flags]
public enum WaAutoSize
{
    /// <summary>No auto-sizing is applied; the popup omits the auto-size attribute.</summary>
    None = 0,
    /// <summary>Automatically resizes the width to fit available space.</summary>
    Horizontal = 1,
    /// <summary>Automatically resizes the height to fit available space.</summary>
    Vertical = 2,
    /// <summary>Automatically resizes both width and height to fit available space.</summary>
    Both = Horizontal | Vertical
}

/// <summary>
/// Sync width/height behavior
/// </summary>
[Flags]
public enum WaSync
{
    /// <summary>No dimension is synced.</summary>
    None = 0,
    /// <summary>Syncs the width to match the anchor element.</summary>
    Width = 1,
    /// <summary>Syncs the height to match the anchor element.</summary>
    Height = 2,
    /// <summary>Syncs both width and height to match the anchor element.</summary>
    Both = Width | Height
}

/// <summary>
/// QR Code error correction levels
/// </summary>
public enum WaErrorCorrection
{
    /// <summary>Low error correction (~7% of codewords can be restored).</summary>
    L,
    /// <summary>Medium error correction (~15% of codewords can be restored).</summary>
    M,
    /// <summary>Quartile error correction (~25% of codewords can be restored).</summary>
    Q,
    /// <summary>High error correction (~30% of codewords can be restored).</summary>
    H
}

/// <summary>
/// Skeleton animation effects
/// </summary>
public enum WaEffect
{
    /// <summary>Animates a sheen sweeping across the skeleton.</summary>
    Sheen,
    /// <summary>Animates the skeleton with a pulsing opacity.</summary>
    Pulse,
    /// <summary>No animation effect is applied.</summary>
    None
}

/// <summary>
/// Textarea resize behavior
/// </summary>
public enum WaResize
{
    /// <summary>The textarea cannot be resized.</summary>
    None,
    /// <summary>The textarea can only be resized vertically.</summary>
    Vertical,
    /// <summary>The textarea can only be resized horizontally.</summary>
    Horizontal,
    /// <summary>The textarea can be resized both vertically and horizontally.</summary>
    Both,
    /// <summary>The textarea automatically resizes to fit its content.</summary>
    Auto,
}

/// <summary>
/// Tab group activation modes
/// </summary>
public enum WaActivation
{
    /// <summary>Navigating tabs with the keyboard immediately activates the corresponding panel.</summary>
    Auto,
    /// <summary>Navigating tabs with the keyboard only moves focus; activation requires an explicit action.</summary>
    Manual
}

/// <summary>
/// Image fit modes
/// </summary>
public enum WaFit
{
    /// <summary>Stretches the image to fill the container, ignoring aspect ratio.</summary>
    Fill,
    /// <summary>Scales the image to fit within the container while preserving aspect ratio.</summary>
    Contain,
    /// <summary>Scales the image to cover the container while preserving aspect ratio, cropping as needed.</summary>
    Cover,
    /// <summary>Scales the image down to fit the container if it is larger, without scaling up.</summary>
    ScaleDown,
    /// <summary>Renders the image at its natural size, ignoring the container.</summary>
    None
}

/// <summary>
/// Image loading behavior
/// </summary>
public enum WaLoading
{
    /// <summary>Loads the image immediately, regardless of its position on the page.</summary>
    Eager,
    /// <summary>Defers loading the image until it is near the viewport.</summary>
    Lazy
}

/// <summary>
/// Relative time formatting styles, passed to <c>Intl.RelativeTimeFormat</c> as its <c>style</c> option
/// </summary>
public enum WaRelativeTimeFormat
{
    /// <summary>Uses full unit names, e.g. "3 hours ago" (Web Awesome default).</summary>
    Long,
    /// <summary>Uses abbreviated unit names, e.g. "3 hr. ago".</summary>
    Short,
    /// <summary>Uses the most compact unit names, e.g. "3h ago"; some locales render this the same as short.</summary>
    Narrow
}

/// <summary>
/// Relative time numeric output modes, passed to <c>Intl.RelativeTimeFormat</c> as its <c>numeric</c> option
/// </summary>
public enum WaRelativeTimeNumeric
{
    /// <summary>Uses idiomatic phrases when available, e.g. "yesterday" instead of "1 day ago" (Web Awesome default).</summary>
    Auto,
    /// <summary>Always uses numeric phrasing, e.g. "1 day ago" instead of "yesterday".</summary>
    Always
}

/// <summary>
/// Format bytes display modes
/// </summary>
public enum WaDisplay
{
    /// <summary>Uses short unit abbreviations, e.g. "KB".</summary>
    Short,
    /// <summary>Uses long unit names, e.g. "kilobytes".</summary>
    Long,
    /// <summary>Uses the narrowest unit form with no separating space, e.g. "100b".</summary>
    Narrow
}

/// <summary>
/// Include request modes
/// </summary>
public enum WaMode
{
    /// <summary>Allows cross-origin requests using CORS.</summary>
    Cors,
    /// <summary>Restricts the request to same-origin behavior without CORS headers.</summary>
    NoCors,
    /// <summary>Restricts the request to the same origin only.</summary>
    SameOrigin
}

/// <summary>
/// Split panel primary side
/// </summary>
public enum WaPrimary
{
    /// <summary>The start panel is designated as primary.</summary>
    Start,
    /// <summary>The end panel is designated as primary.</summary>
    End
}

/// <summary>
/// Menu item types
/// </summary>
public enum WaMenuItemType
{
    /// <summary>A regular, non-selectable menu item.</summary>
    Normal,
    /// <summary>A menu item that can be independently toggled on or off.</summary>
    Checkbox,
    /// <summary>A menu item that is mutually exclusive within its group.</summary>
    Radio
}

/// <summary>
/// Dropdown item types
/// </summary>
public enum WaDropdownItemType
{
    /// <summary>A regular, non-selectable dropdown item.</summary>
    Normal,
    /// <summary>A dropdown item that can be independently toggled on or off.</summary>
    Checkbox
}

/// <summary>
/// Theme variant of a dropdown item
/// </summary>
public enum WaDropdownItemVariant
{
    /// <summary>The regular dropdown item styling (Web Awesome default).</summary>
    Default,
    /// <summary>Danger styling, for destructive actions.</summary>
    Danger
}

/// <summary>
/// Radio appearance styles
/// </summary>
public enum WaRadioAppearance
{
    /// <summary>Default radio circle appearance (Web Awesome default).</summary>
    Default,
    /// <summary>Button-style radio appearance</summary>
    Button
}

/// <summary>
/// Tooltip trigger types. The wa-tooltip trigger attribute is a space-separated list of tokens,
/// so values can be combined, e.g. <c>WaTrigger.Hover | WaTrigger.Focus</c> (the Web Awesome default).
/// A value of 0 is not valid and is rejected when converted to its attribute string.
/// </summary>
[Flags]
public enum WaTrigger
{
    /// <summary>Toggles the tooltip when the anchor is clicked; clicking again dismisses it.</summary>
    Click = 1,
    /// <summary>Shows the tooltip while the pointer hovers over the anchor.</summary>
    Hover = 2,
    /// <summary>Shows the tooltip while the anchor has keyboard focus.</summary>
    Focus = 4,
    /// <summary>
    /// The tooltip is opened and closed only programmatically, via <see cref="WaTooltip.Open"/>
    /// (or <see cref="WaTooltip.ShowAsync"/>/<see cref="WaTooltip.HideAsync"/>). A manual tooltip never
    /// light dismisses and ignores Escape, also when combined with other triggers.
    /// </summary>
    Manual = 8
}

/// <summary>
/// Dialog and drawer placement options
/// </summary>
public enum WaDrawerPlacement
{
    /// <summary>Slides in from the logical start edge.</summary>
    Start,
    /// <summary>Slides in from the logical end edge.</summary>
    End,
    /// <summary>Slides in from the top edge.</summary>
    Top,
    /// <summary>Slides in from the bottom edge.</summary>
    Bottom
}

/// <summary>
/// Byte unit for format-bytes component
/// </summary>
public enum WaByteUnit
{
    /// <summary>Formats using byte-based units (e.g. KB, MB).</summary>
    Byte,
    /// <summary>Formats using bit-based units (e.g. kb, Mb).</summary>
    Bit
}

/// <summary>
/// Month style option for the format-date component ('numeric' | '2-digit' | 'narrow' | 'short' | 'long')
/// </summary>
public enum WaDateTimeStyle
{
    /// <summary>Long form, e.g. "January".</summary>
    Long,
    /// <summary>Short form, e.g. "Jan".</summary>
    Short,
    /// <summary>Narrow form, e.g. "J".</summary>
    Narrow,
    /// <summary>Numeric form, e.g. "1".</summary>
    Numeric,
    /// <summary>Two-digit numeric form, e.g. "01".</summary>
    TwoDigit
}

/// <summary>
/// Textual style option for the format-date component's weekday and era ('narrow' | 'short' | 'long')
/// </summary>
public enum WaDateTimeTextStyle
{
    /// <summary>Narrow form, e.g. "M" for Monday or "A" for AD.</summary>
    Narrow,
    /// <summary>Short form, e.g. "Mon" or "AD".</summary>
    Short,
    /// <summary>Long form, e.g. "Monday" or "Anno Domini".</summary>
    Long
}

/// <summary>
/// Numeric style option for the format-date component's year, day, hour, minute and second ('numeric' | '2-digit')
/// </summary>
public enum WaDateTimeNumericStyle
{
    /// <summary>Numeric form, e.g. "1".</summary>
    Numeric,
    /// <summary>Two-digit numeric form, e.g. "01".</summary>
    TwoDigit
}

/// <summary>
/// Time zone name style option for the format-date component ('short' | 'long')
/// </summary>
public enum WaTimeZoneNameStyle
{
    /// <summary>Short form, e.g. "PST".</summary>
    Short,
    /// <summary>Long form, e.g. "Pacific Standard Time".</summary>
    Long
}

/// <summary>
/// Hour format for format-date component
/// </summary>
public enum WaHourFormat
{
    /// <summary>Follows the resolved locale (Web Awesome default).</summary>
    Auto,
    /// <summary>Uses a 12-hour clock.</summary>
    Twelve,
    /// <summary>Uses a 24-hour clock.</summary>
    TwentyFour
}

/// <summary>
/// Color format for color picker component
/// </summary>
public enum WaColorFormat
{
    /// <summary>Hexadecimal color format, e.g. "#ff0000".</summary>
    Hex,
    /// <summary>RGB color format, e.g. "rgb(255, 0, 0)".</summary>
    Rgb,
    /// <summary>HSL color format, e.g. "hsl(0, 100%, 50%)".</summary>
    Hsl,
    /// <summary>HSV color format, e.g. "hsv(0, 100%, 100%)".</summary>
    Hsv
}

/// <summary>
/// Tab placement for tab group component
/// </summary>
public enum WaTabPlacement
{
    /// <summary>Places the tabs above the panels.</summary>
    Top,
    /// <summary>Places the tabs below the panels.</summary>
    Bottom,
    /// <summary>Places the tabs at the logical start, beside the panels.</summary>
    Start,
    /// <summary>Places the tabs at the logical end, beside the panels.</summary>
    End
}

/// <summary>
/// Icon placement for details component
/// </summary>
public enum WaIconPlacement
{
    /// <summary>Places the icon at the start.</summary>
    Start,
    /// <summary>Places the icon at the end.</summary>
    End
}

/// <summary>
/// Placement of the navigation in the mobile viewport for the page component.
/// </summary>
public enum WaPageNavigationPlacement
{
    /// <summary>Places the navigation at the logical start edge.</summary>
    Start,
    /// <summary>Places the navigation at the logical end edge.</summary>
    End
}

/// <summary>
/// View mode of the page component, reflecting its width relative to the mobile breakpoint.
/// </summary>
public enum WaPageView
{
    /// <summary>The page is narrower than the mobile breakpoint.</summary>
    Mobile,
    /// <summary>The page is at least as wide as the mobile breakpoint.</summary>
    Desktop
}

/// <summary>
/// Selection behavior for the tree component
/// </summary>
public enum WaTreeSelection
{
    /// <summary>Only one node can be selected at a time.</summary>
    Single,
    /// <summary>Displays checkboxes and allows more than one node to be selected.</summary>
    Multiple,
    /// <summary>Only leaf nodes can be selected.</summary>
    Leaf,
    /// <summary>Multiple leaf nodes can be selected while parent nodes only expand and collapse.</summary>
    LeafMultiple
}

/// <summary>
/// Sparkline visual appearance
/// </summary>
public enum WaSparklineAppearance
{
    /// <summary>Draws a gradient fill beneath the sparkline.</summary>
    Gradient,
    /// <summary>Draws only the sparkline stroke.</summary>
    Line,
    /// <summary>Draws a solid fill beneath the sparkline.</summary>
    Solid
}

/// <summary>
/// Sparkline curve interpolation
/// </summary>
public enum WaSparklineCurve
{
    /// <summary>Connects data points with straight line segments.</summary>
    Linear,
    /// <summary>Connects data points with a smooth, natural curve.</summary>
    Natural,
    /// <summary>Connects data points with a stepped line.</summary>
    Step
}

/// <summary>
/// Sparkline trend indicator
/// </summary>
public enum WaSparklineTrend
{
    /// <summary>Represents a positive trend.</summary>
    Positive,
    /// <summary>Represents a negative trend.</summary>
    Negative,
    /// <summary>Represents a neutral trend.</summary>
    Neutral
}

/// <summary>
/// Icon animation effects
/// </summary>
public enum WaIconAnimation
{
    /// <summary>Pulses the icon by scaling it up and down.</summary>
    Beat,
    /// <summary>Fades the icon in and out.</summary>
    Fade,
    /// <summary>Combines the beat and fade animations.</summary>
    BeatFade,
    /// <summary>Bounces the icon vertically.</summary>
    Bounce,
    /// <summary>Flips the icon horizontally on a repeating cycle.</summary>
    Flip,
    /// <summary>Shakes the icon from side to side.</summary>
    Shake,
    /// <summary>Spins the icon continuously.</summary>
    Spin,
    /// <summary>Spins the icon continuously with a pulsing, stepped motion.</summary>
    SpinPulse,
    /// <summary>Spins the icon continuously in the reverse direction.</summary>
    SpinReverse,
    /// <summary>Flips the icon all the way around in one smooth rotation.</summary>
    Flip360,
    /// <summary>Rotates the icon in distinct steps with a pause on each, like a clock's second hand.</summary>
    SpinSnap,
    /// <summary>Rotates the icon in snapping steps with four stops.</summary>
    SpinSnap4,
    /// <summary>Rotates the icon in snapping steps with eight stops.</summary>
    SpinSnap8,
    /// <summary>Vibrates the icon quickly with rapid decay, like a phone buzzing on a table.</summary>
    Buzz,
    /// <summary>Sways the top of the icon back and forth around a bottom anchor, with a slow decay.</summary>
    Wag,
    /// <summary>Drifts the icon slowly up and down.</summary>
    Float,
    /// <summary>Dangles the icon with a subtle swing and a slow decay.</summary>
    Swing,
    /// <summary>Jiggles the icon playfully.</summary>
    Jello
}

/// <summary>
/// Icon flip directions
/// </summary>
public enum WaFlip
{
    /// <summary>Flips the icon along the horizontal axis.</summary>
    X,
    /// <summary>Flips the icon along the vertical axis.</summary>
    Y,
    /// <summary>Flips the icon along both the horizontal and vertical axes.</summary>
    Both
}

/// <summary>
/// The type of chart rendered by the chart components.
/// </summary>
public enum WaChartType
{
    /// <summary>A bar chart.</summary>
    Bar,
    /// <summary>A line chart.</summary>
    Line,
    /// <summary>A pie chart.</summary>
    Pie,
    /// <summary>A doughnut chart.</summary>
    Doughnut,
    /// <summary>A polar area chart.</summary>
    PolarArea,
    /// <summary>A radar chart.</summary>
    Radar,
    /// <summary>A scatter chart.</summary>
    Scatter,
    /// <summary>A bubble chart.</summary>
    Bubble
}

/// <summary>
/// Which axes a chart shows grid lines on.
/// </summary>
public enum WaChartGrid
{
    /// <summary>Grid lines on the x-axis only.</summary>
    X,
    /// <summary>Grid lines on the y-axis only.</summary>
    Y,
    /// <summary>Grid lines on both axes.</summary>
    Both,
    /// <summary>No grid lines.</summary>
    None
}

/// <summary>
/// The base axis of a chart's dataset.
/// </summary>
public enum WaChartAxis
{
    /// <summary>The x-axis (vertical bars).</summary>
    X,
    /// <summary>The y-axis (horizontal bars).</summary>
    Y
}

/// <summary>
/// The position of a chart's legend relative to the chart.
/// </summary>
public enum WaChartLegendPosition
{
    /// <summary>Above the chart.</summary>
    Top,
    /// <summary>To the left of the chart.</summary>
    Left,
    /// <summary>Below the chart.</summary>
    Bottom,
    /// <summary>To the right of the chart.</summary>
    Right,
    /// <summary>At the logical start of the chart.</summary>
    Start,
    /// <summary>At the logical end of the chart.</summary>
    End,
    /// <summary>Inside the chart area (the Chart.js "chartArea" legend position).</summary>
    ChartArea
}

/// <summary>
/// The placement of a toast stack on the screen.
/// </summary>
public enum WaToastPlacement
{
    /// <summary>Top, aligned to the logical start.</summary>
    TopStart,
    /// <summary>Top, centered.</summary>
    TopCenter,
    /// <summary>Top, aligned to the logical end.</summary>
    TopEnd,
    /// <summary>Bottom, aligned to the logical start.</summary>
    BottomStart,
    /// <summary>Bottom, centered.</summary>
    BottomCenter,
    /// <summary>Bottom, aligned to the logical end.</summary>
    BottomEnd
}

/// <summary>
/// Controls preset for the video player, determining which playback controls are shown.
/// </summary>
public enum WaVideoControls
{
    /// <summary>Shows the timeline, play/pause, volume, captions, and fullscreen.</summary>
    Standard,
    /// <summary>No controls are shown.</summary>
    None,
    /// <summary>Everything in standard, plus playback speed and picture-in-picture.</summary>
    Full
}

/// <summary>
/// Controls how the browser preloads video data.
/// </summary>
public enum WaVideoPreload
{
    /// <summary>Preloads only metadata (dimensions, duration) to minimize data usage.</summary>
    Metadata,
    /// <summary>Lets the browser decide, potentially preloading the entire video.</summary>
    Auto,
    /// <summary>Preloads nothing until playback is requested.</summary>
    None
}

/// <summary>
/// Controls the built-in tooltip behavior of a copy button.
/// </summary>
public enum WaCopyButtonTooltip
{
    /// <summary>Shows the tooltip on hover and focus and during copy feedback.</summary>
    Full,
    /// <summary>Keeps the tooltip silent on hover/focus and only shows it briefly to confirm a copy.</summary>
    Copy,
    /// <summary>Disables the tooltip entirely.</summary>
    None
}

/// <summary>
/// Controls how items in an accordion can be expanded.
/// </summary>
public enum WaAccordionMode
{
    /// <summary>Only one item may be open at a time; clicking an open item does not collapse it.</summary>
    Single,
    /// <summary>Like <see cref="Single"/>, but clicking the open item collapses it (zero open items is valid).</summary>
    SingleCollapsible,
    /// <summary>Any number of items may be open at once.</summary>
    Multiple
}

/// <summary>
/// The first day of the week in a calendar.
/// </summary>
public enum WaFirstDayOfWeek
{
    /// <summary>Uses the resolved locale's week info to determine the first day.</summary>
    Auto,
    /// <summary>Sunday.</summary>
    Sun,
    /// <summary>Monday.</summary>
    Mon,
    /// <summary>Tuesday.</summary>
    Tue,
    /// <summary>Wednesday.</summary>
    Wed,
    /// <summary>Thursday.</summary>
    Thu,
    /// <summary>Friday.</summary>
    Fri,
    /// <summary>Saturday.</summary>
    Sat
}

/// <summary>
/// Whether a calendar's prev/next navigation advances by the visible range or one month at a time.
/// </summary>
public enum WaDatePageBy
{
    /// <summary>Advances by the visible range of months.</summary>
    Months,
    /// <summary>Advances one month at a time.</summary>
    Single
}

/// <summary>
/// The weekday header format in a calendar.
/// </summary>
public enum WaWeekdayFormat
{
    /// <summary>Narrow form, e.g. "M".</summary>
    Narrow,
    /// <summary>Short form, e.g. "Mon".</summary>
    Short,
    /// <summary>Long form, e.g. "Monday".</summary>
    Long
}

/// <summary>
/// The active view of the date picker.
/// </summary>
[JsonConverter(typeof(WaDatePickerViewJsonConverter))]
public enum WaDatePickerView
{
    /// <summary>Shows the days-of-month grid.</summary>
    Days,
    /// <summary>Shows the month picker.</summary>
    Months,
    /// <summary>Shows the year picker.</summary>
    Years
}

/// <summary>
/// Whether the time input's UI uses a 12-hour or 24-hour clock.
/// </summary>
public enum WaTimeHourFormat
{
    /// <summary>Follows the resolved locale.</summary>
    Auto,
    /// <summary>Uses a 12-hour clock.</summary>
    Twelve,
    /// <summary>Uses a 24-hour clock.</summary>
    TwentyFour
}

/// <summary>
/// The camera or microphone to use when capturing media on mobile devices.
/// </summary>
public enum WaCaptureMode
{
    /// <summary>The front-facing camera or microphone.</summary>
    User,
    /// <summary>The rear-facing camera or microphone.</summary>
    Environment
}

/// <summary>
/// The box an icon is centered within (the icon canvas). Unset renders as fixed (1.25em × 1em).
/// Mirrors Font Awesome's fa-fixed-width, fa-width-auto, fa-canvas-square, and fa-canvas-roomy.
/// </summary>
public enum WaIconCanvas
{
    /// <summary>Fixed 1.25em × 1em box (matches Font Awesome fa-fixed-width).</summary>
    Fixed,
    /// <summary>Hugs the icon's own width (matches Font Awesome fa-width-auto).</summary>
    Auto,
    /// <summary>Square 1.25em × 1.25em box (matches Font Awesome fa-canvas-square).</summary>
    Square,
    /// <summary>Roomy 1.5em × 1.5em box (matches Font Awesome fa-canvas-roomy).</summary>
    Roomy
}

/// <summary>
/// Entrance animation for newly shown children of a random-content component.
/// </summary>
public enum WaRandomContentAnimation
{
    /// <summary>No entrance animation.</summary>
    None,
    /// <summary>Fades the content in.</summary>
    Fade,
    /// <summary>Fades the content in while translating it upward.</summary>
    FadeUp,
    /// <summary>Fades the content in while translating it downward.</summary>
    FadeDown,
    /// <summary>Fades the content in while translating it from the left.</summary>
    FadeLeft,
    /// <summary>Fades the content in while translating it from the right.</summary>
    FadeRight
}

/// <summary>
/// Selection strategy a random-content component uses when choosing which children to show.
/// </summary>
public enum WaRandomContentMode
{
    /// <summary>Selects children at random, allowing repeats across rotations.</summary>
    Random,
    /// <summary>Selects children without repeating until the pool is exhausted (the default).</summary>
    Unique,
    /// <summary>Selects children in document order, cycling through the pool.</summary>
    Sequence
}

/// <summary>
/// Visual appearance for the pagination component.
/// </summary>
public enum WaPaginationAppearance
{
    /// <summary>Draws each page control with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws each page control with a filled background.</summary>
    Filled,
    /// <summary>Draws each page control as plain text with no border or fill.</summary>
    Plain
}

/// <summary>
/// Layout format for the pagination component.
/// </summary>
public enum WaPaginationFormat
{
    /// <summary>Shows the full page list with ellipses.</summary>
    Standard,
    /// <summary>Collapses the control into a short "1 of 5" label flanked by the previous and next buttons.</summary>
    Compact
}

/// <summary>
/// Visual appearance for the OTP input component's segments.
/// </summary>
public enum WaOtpInputAppearance
{
    /// <summary>Draws each segment with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws each segment with a filled background.</summary>
    Filled,
    /// <summary>Draws each segment with both an outline and a filled background.</summary>
    FilledOutlined,
    /// <summary>Draws the segments as a single contained group with no gaps between them.</summary>
    Contained
}

/// <summary>
/// Allowed character class for the OTP input component.
/// </summary>
public enum WaOtpInputType
{
    /// <summary>Only digits are accepted.</summary>
    Numeric,
    /// <summary>Only letters are accepted.</summary>
    Alpha,
    /// <summary>Letters and digits are accepted.</summary>
    Alphanumeric
}

/// <summary>
/// Case transformation applied to characters entered into the OTP input component.
/// </summary>
public enum WaOtpInputCase
{
    /// <summary>Keeps the entered character's case as typed.</summary>
    Preserve,
    /// <summary>Converts entered characters to upper case.</summary>
    Upper,
    /// <summary>Converts entered characters to lower case.</summary>
    Lower
}

/// <summary>
/// Visual appearance for the data grid component.
/// </summary>
public enum WaDataGridAppearance
{
    /// <summary>Draws the grid with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws the grid with minimal, unstyled chrome.</summary>
    Plain
}

/// <summary>
/// Row-selection mode for the data grid component.
/// </summary>
public enum WaDataGridSelectable
{
    /// <summary>Row selection is disabled.</summary>
    None,
    /// <summary>Only one row can be selected at a time.</summary>
    Single,
    /// <summary>
    /// Multiple rows can be selected. Upstream also treats a bare <c>selectable</c> attribute (an empty
    /// string value) as this mode; the wrapper always emits the explicit <c>multiple</c> token.
    /// </summary>
    Multiple
}

/// <summary>
/// The heading element an accordion wraps each item's trigger in, or none.
/// </summary>
public enum WaHeadingLevel
{
    /// <summary>An h1 heading.</summary>
    H1,
    /// <summary>An h2 heading.</summary>
    H2,
    /// <summary>An h3 heading (the Web Awesome default).</summary>
    H3,
    /// <summary>An h4 heading.</summary>
    H4,
    /// <summary>An h5 heading.</summary>
    H5,
    /// <summary>An h6 heading.</summary>
    H6,
    /// <summary>No heading wrapper; the trigger button is rendered on its own.</summary>
    None
}

/// <summary>
/// The referrer a framed document's requests send (the HTML referrerpolicy attribute).
/// </summary>
public enum WaReferrerPolicy
{
    /// <summary>Sends no referrer ("no-referrer").</summary>
    NoReferrer,
    /// <summary>Sends no referrer to a less secure origin ("no-referrer-when-downgrade").</summary>
    NoReferrerWhenDowngrade,
    /// <summary>Sends only the origin ("origin").</summary>
    Origin,
    /// <summary>Sends the full URL to the same origin, only the origin elsewhere ("origin-when-cross-origin").</summary>
    OriginWhenCrossOrigin,
    /// <summary>Sends the referrer to the same origin only ("same-origin").</summary>
    SameOrigin,
    /// <summary>Sends only the origin, and nothing to a less secure origin ("strict-origin").</summary>
    StrictOrigin,
    /// <summary>The browser default: the full URL to the same origin, the origin elsewhere, nothing on a downgrade ("strict-origin-when-cross-origin").</summary>
    StrictOriginWhenCrossOrigin,
    /// <summary>Always sends the full URL ("unsafe-url").</summary>
    UnsafeUrl
}

/// <summary>
/// The restrictions an iframe sandbox lifts (the tokens of the HTML sandbox attribute). Any combination is valid;
/// <see cref="None"/> applies every restriction.
/// </summary>
[Flags]
public enum WaIframeSandbox
{
    /// <summary>Lifts no restriction: the frame is fully sandboxed (an empty sandbox attribute).</summary>
    None = 0,
    /// <summary>Allows downloads ("allow-downloads").</summary>
    AllowDownloads = 1,
    /// <summary>Allows form submission ("allow-forms").</summary>
    AllowForms = 2,
    /// <summary>Allows modal dialogs ("allow-modals").</summary>
    AllowModals = 4,
    /// <summary>Allows locking the screen orientation ("allow-orientation-lock").</summary>
    AllowOrientationLock = 8,
    /// <summary>Allows the Pointer Lock API ("allow-pointer-lock").</summary>
    AllowPointerLock = 16,
    /// <summary>Allows popups ("allow-popups").</summary>
    AllowPopups = 32,
    /// <summary>Lets popups escape the sandbox ("allow-popups-to-escape-sandbox").</summary>
    AllowPopupsToEscapeSandbox = 64,
    /// <summary>Allows starting a presentation session ("allow-presentation").</summary>
    AllowPresentation = 128,
    /// <summary>Treats the content as its own origin ("allow-same-origin").</summary>
    AllowSameOrigin = 256,
    /// <summary>Allows scripts ("allow-scripts").</summary>
    AllowScripts = 512,
    /// <summary>Allows navigating the top-level context ("allow-top-navigation").</summary>
    AllowTopNavigation = 1024,
    /// <summary>Allows navigating the top-level context after a user gesture ("allow-top-navigation-by-user-activation").</summary>
    AllowTopNavigationByUserActivation = 2048,
    /// <summary>Allows navigating the top-level context to a non-HTTP protocol ("allow-top-navigation-to-custom-protocols").</summary>
    AllowTopNavigationToCustomProtocols = 4096
}

/// <summary>
/// The sections of a page layout whose sticky positioning can be turned off (the tokens of wa-page's
/// disable-sticky attribute). Any combination is valid.
/// </summary>
[Flags]
public enum WaPageSections
{
    /// <summary>The banner section ("banner").</summary>
    Banner = 1,
    /// <summary>The header section ("header").</summary>
    Header = 2,
    /// <summary>The subheader section ("subheader").</summary>
    Subheader = 4,
    /// <summary>The aside section ("aside").</summary>
    Aside = 8,
    /// <summary>The menu section ("menu").</summary>
    Menu = 16
}

#endregion

#region ------ Event and Model Enums ------

/// <summary>
/// The phase of a rating hover (the wa-hover event's detail.phase).
/// </summary>
[JsonConverter(typeof(WaRatingHoverPhaseJsonConverter))]
public enum WaRatingHoverPhase
{
    /// <summary>The pointer entered the rating.</summary>
    Start,
    /// <summary>The pointer moved over the rating.</summary>
    Move,
    /// <summary>The pointer left the rating.</summary>
    End
}

/// <summary>
/// The kind of DOM change a mutation record describes (MutationRecord.type).
/// </summary>
[JsonConverter(typeof(WaMutationTypeJsonConverter))]
public enum WaMutationType
{
    /// <summary>An attribute changed.</summary>
    Attributes,
    /// <summary>The character data of a text node changed.</summary>
    CharacterData,
    /// <summary>Child nodes were added or removed.</summary>
    ChildList
}

/// <summary>
/// The edge of the data grid a column is pinned to.
/// </summary>
[JsonConverter(typeof(WaDataGridPinSideJsonConverter))]
public enum WaDataGridPinSide
{
    /// <summary>The left edge.</summary>
    Left,
    /// <summary>The right edge.</summary>
    Right
}

/// <summary>
/// The horizontal alignment of a data grid column's cells or header.
/// </summary>
[JsonConverter(typeof(WaDataGridAlignJsonConverter))]
public enum WaDataGridAlign
{
    /// <summary>Aligns to the start edge.</summary>
    Start,
    /// <summary>Centers the content.</summary>
    Center,
    /// <summary>Aligns to the end edge.</summary>
    End
}

/// <summary>
/// The built-in comparison a data grid column sorts with.
/// </summary>
[JsonConverter(typeof(WaDataGridSortFnJsonConverter))]
public enum WaDataGridSortFn
{
    /// <summary>Mixed strings and numbers (the Web Awesome default).</summary>
    Alphanumeric,
    /// <summary>Mixed strings and numbers, case-sensitive.</summary>
    AlphanumericCaseSensitive,
    /// <summary>Strings only; faster.</summary>
    Text,
    /// <summary>Strings only, case-sensitive.</summary>
    TextCaseSensitive,
    /// <summary>Date objects or date strings.</summary>
    Datetime,
    /// <summary>Plain greater-than/less-than comparison; fastest.</summary>
    Basic
}

/// <summary>
/// Where a data grid column sorts its null and undefined values. Unset leaves them in place (the Web Awesome default).
/// </summary>
[JsonConverter(typeof(WaDataGridSortUndefinedJsonConverter))]
public enum WaDataGridSortUndefined
{
    /// <summary>First, whatever the sort direction ("first").</summary>
    First,
    /// <summary>Last, whatever the sort direction ("last").</summary>
    Last,
    /// <summary>Ranked below every value, so first when ascending (-1).</summary>
    Lower,
    /// <summary>Ranked above every value, so last when ascending (1).</summary>
    Higher
}

/// <summary>
/// How a data grid column's filter matches.
/// </summary>
[JsonConverter(typeof(WaDataGridFilterTypeJsonConverter))]
public enum WaDataGridFilterType
{
    /// <summary>A case-insensitive substring match (the Web Awesome default).</summary>
    Text,
    /// <summary>An exact string match ("equals").</summary>
    ExactMatch,
    /// <summary>A [min, max] numeric window with min/max inputs ("number-range").</summary>
    NumberRange,
    /// <summary>A [from, to] date window with two date inputs ("date-range").</summary>
    DateRange,
    /// <summary>One of the chosen values, picked from a multi-select of distinct values ("set").</summary>
    Set,
    /// <summary>An array-valued cell containing any chosen value ("includes-any").</summary>
    IncludesAny,
    /// <summary>An array-valued cell containing every chosen value ("includes-all").</summary>
    IncludesAll
}

/// <summary>
/// The built-in aggregation a data grid column applies on grouped rows.
/// </summary>
[JsonConverter(typeof(WaDataGridAggregationJsonConverter))]
public enum WaDataGridAggregation
{
    /// <summary>The sum of the values.</summary>
    Sum,
    /// <summary>The smallest value.</summary>
    Min,
    /// <summary>The largest value.</summary>
    Max,
    /// <summary>The smallest and largest value.</summary>
    Extent,
    /// <summary>The arithmetic mean.</summary>
    Mean,
    /// <summary>The median.</summary>
    Median,
    /// <summary>The distinct values.</summary>
    Unique,
    /// <summary>The number of distinct values ("uniqueCount").</summary>
    UniqueCount,
    /// <summary>The number of rows.</summary>
    Count
}

#endregion

#region ------ Extension Methods ------

/// <summary>
/// Extension methods for converting Web Awesome enums to HTML attribute values
/// </summary>
public static class WaEnumExtensions
{
    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="size">The size value to convert</param>
    /// <returns>
    /// The short-form attribute string, e.g. "s"; Web Awesome 3.12 deprecates the long forms "small", "medium"
    /// and "large" in favor of "s", "m" and "l"
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaSize size)
    {
        return size switch
        {
            WaSize.ExtraSmall => "xs",
            WaSize.Small => "s",
            WaSize.Medium => "m",
            WaSize.Large => "l",
            WaSize.ExtraLarge => "xl",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="canvas">The icon canvas value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "square"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="canvas"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaIconCanvas canvas)
    {
        return canvas switch
        {
            WaIconCanvas.Fixed => "fixed",
            WaIconCanvas.Auto => "auto",
            WaIconCanvas.Square => "square",
            WaIconCanvas.Roomy => "roomy",
            _ => throw new ArgumentOutOfRangeException(nameof(canvas), canvas, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="animation">The random-content animation value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "fade-up"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="animation"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRandomContentAnimation animation)
    {
        return animation switch
        {
            WaRandomContentAnimation.None => "none",
            WaRandomContentAnimation.Fade => "fade",
            WaRandomContentAnimation.FadeUp => "fade-up",
            WaRandomContentAnimation.FadeDown => "fade-down",
            WaRandomContentAnimation.FadeLeft => "fade-left",
            WaRandomContentAnimation.FadeRight => "fade-right",
            _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="mode">The random-content selection mode to convert</param>
    /// <returns>The lowercase attribute string, e.g. "unique"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mode"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRandomContentMode mode)
    {
        return mode switch
        {
            WaRandomContentMode.Random => "random",
            WaRandomContentMode.Unique => "unique",
            WaRandomContentMode.Sequence => "sequence",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="variant">The variant value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "neutral"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="variant"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The appearance value to convert</param>
    /// <returns>The attribute string, e.g. "filled" or "filled-outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAppearance appearance)
    {
        return appearance switch
        {
            WaAppearance.Filled => "filled",
            WaAppearance.Outlined => "outlined",
            WaAppearance.OutlinedFilled => "filled-outlined",
            WaAppearance.Plain => "plain",
            WaAppearance.Accent => "accent",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The details/accordion appearance value to convert</param>
    /// <returns>The attribute string, e.g. "filled" or "filled-outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDetailsAppearance appearance)
    {
        return appearance switch
        {
            WaDetailsAppearance.Filled => "filled",
            WaDetailsAppearance.Outlined => "outlined",
            WaDetailsAppearance.FilledOutlined => "filled-outlined",
            WaDetailsAppearance.Plain => "plain",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The badge/tag appearance value to convert</param>
    /// <returns>The attribute string, e.g. "accent" or "filled-outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaBadgeAppearance appearance)
    {
        return appearance switch
        {
            WaBadgeAppearance.Accent => "accent",
            WaBadgeAppearance.Filled => "filled",
            WaBadgeAppearance.Outlined => "outlined",
            WaBadgeAppearance.FilledOutlined => "filled-outlined",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaInputAppearance appearance)
    {
        return appearance switch
        {
            WaInputAppearance.Outlined => "outlined",
            WaInputAppearance.Filled => "filled",
            WaInputAppearance.FilledOutlined => "filled-outlined",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The input type value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "text"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
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
            WaInputType.DateTimeLocal => "datetime-local",
            WaInputType.Time => "time",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The button type value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "button"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="resize">The resize value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "none"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="resize"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The placement value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "top-start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPlacement placement)
    {
        return placement switch
        {
            WaPlacement.Top => "top",
            WaPlacement.Bottom => "bottom",
            WaPlacement.Left => "left",
            WaPlacement.Right => "right",
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The listbox placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "bottom"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaListboxPlacement placement)
    {
        return placement switch
        {
            WaListboxPlacement.Top => "top",
            WaListboxPlacement.Bottom => "bottom",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The picker placement value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "bottom-start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPickerPlacement placement)
    {
        return placement switch
        {
            WaPickerPlacement.Top => "top",
            WaPickerPlacement.TopStart => "top-start",
            WaPickerPlacement.TopEnd => "top-end",
            WaPickerPlacement.Bottom => "bottom",
            WaPickerPlacement.BottomStart => "bottom-start",
            WaPickerPlacement.BottomEnd => "bottom-end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="side">The tooltip side value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "top"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="side"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTooltipSide side)
    {
        return side switch
        {
            WaTooltipSide.Top => "top",
            WaTooltipSide.Right => "right",
            WaTooltipSide.Bottom => "bottom",
            WaTooltipSide.Left => "left",
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="attention">The attention value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "pulse"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attention"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAttention attention)
    {
        return attention switch
        {
            WaAttention.None => "none",
            WaAttention.Pulse => "pulse",
            WaAttention.Bounce => "bounce",
            _ => throw new ArgumentOutOfRangeException(nameof(attention), attention, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="orientation">The orientation value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "horizontal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="orientation"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="orientation">The stepper orientation value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "horizontal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="orientation"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaStepperOrientation orientation)
    {
        return orientation switch
        {
            WaStepperOrientation.Horizontal => "horizontal",
            WaStepperOrientation.Vertical => "vertical",
            WaStepperOrientation.Auto => "auto",
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The divider label placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "center"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDividerLabelPlacement placement)
    {
        return placement switch
        {
            WaDividerLabelPlacement.Start => "start",
            WaDividerLabelPlacement.Center => "center",
            WaDividerLabelPlacement.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The radio appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "default"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRadioAppearance appearance)
    {
        return appearance switch
        {
            WaRadioAppearance.Default => "default",
            WaRadioAppearance.Button => "button",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="shape">The avatar shape value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "circle"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="shape"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="effect">The effect value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "sheen"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="effect"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="loading">The loading value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "eager"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="loading"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="trigger">The trigger flags to convert</param>
    /// <returns>
    /// The set flags as space-separated lowercase tokens in the stable order click, hover, focus, manual,
    /// e.g. "hover focus"
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="trigger"/> is 0 or contains undefined bits</exception>
    public static string ToHtmlValue(this WaTrigger trigger)
    {
        const WaTrigger allTriggers = WaTrigger.Click | WaTrigger.Hover | WaTrigger.Focus | WaTrigger.Manual;
        if (trigger == 0 || (trigger & ~allTriggers) != 0)
            throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);

        var tokens = new List<string>(4);
        if ((trigger & WaTrigger.Click) != 0) tokens.Add("click");
        if ((trigger & WaTrigger.Hover) != 0) tokens.Add("hover");
        if ((trigger & WaTrigger.Focus) != 0) tokens.Add("focus");
        if ((trigger & WaTrigger.Manual) != 0) tokens.Add("manual");
        return string.Join(' ', tokens);
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The drawer placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The dropdown item type value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "normal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDropdownItemType type)
    {
        return type switch
        {
            WaDropdownItemType.Normal => "normal",
            WaDropdownItemType.Checkbox => "checkbox",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="variant">The dropdown item variant value to convert</param>
    /// <returns>The lowercase attribute string, "default" or "danger"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="variant"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDropdownItemVariant variant)
    {
        return variant switch
        {
            WaDropdownItemVariant.Default => "default",
            WaDropdownItemVariant.Danger => "danger",
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="unit">The byte unit value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "byte"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unit"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="style">The date/time style value to convert</param>
    /// <returns>The attribute string, e.g. "long" or "2-digit"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="style"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="style">The textual date/time style value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "short"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="style"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDateTimeTextStyle style)
    {
        return style switch
        {
            WaDateTimeTextStyle.Narrow => "narrow",
            WaDateTimeTextStyle.Short => "short",
            WaDateTimeTextStyle.Long => "long",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="style">The numeric date/time style value to convert</param>
    /// <returns>The attribute string, "numeric" or "2-digit"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="style"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDateTimeNumericStyle style)
    {
        return style switch
        {
            WaDateTimeNumericStyle.Numeric => "numeric",
            WaDateTimeNumericStyle.TwoDigit => "2-digit",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="style">The time zone name style value to convert</param>
    /// <returns>The lowercase attribute string, "short" or "long"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="style"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTimeZoneNameStyle style)
    {
        return style switch
        {
            WaTimeZoneNameStyle.Short => "short",
            WaTimeZoneNameStyle.Long => "long",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The hour format value to convert</param>
    /// <returns>The attribute string, "auto", "12" or "24"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaHourFormat format)
    {
        return format switch
        {
            WaHourFormat.Auto => "auto",
            WaHourFormat.Twelve => "12",
            WaHourFormat.TwentyFour => "24",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The format number type value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "decimal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="display">The currency display value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "symbol"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="display"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaCurrencyDisplay display)
    {
        return display switch
        {
            WaCurrencyDisplay.Symbol => "symbol",
            WaCurrencyDisplay.NarrowSymbol => "narrowSymbol",
            WaCurrencyDisplay.Code => "code",
            WaCurrencyDisplay.Name => "name",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The color format value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "hex"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The tab placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "top"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="activation">The activation value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "auto"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="activation"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The menu item type value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "normal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The icon placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaIconPlacement placement)
    {
        return placement switch
        {
            WaIconPlacement.Start => "start",
            WaIconPlacement.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="primary">The primary panel value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="primary"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="direction">The animation direction value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "alternate-reverse"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="direction"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="easing">The animation easing value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "ease-in-out"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="easing"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="fill">The animation fill mode value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "forwards"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fill"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAnimationFill fill)
    {
        return fill switch
        {
            WaAnimationFill.None => "none",
            WaAnimationFill.Forwards => "forwards",
            WaAnimationFill.Backwards => "backwards",
            WaAnimationFill.Both => "both",
            WaAnimationFill.Auto => "auto",
            _ => throw new ArgumentOutOfRangeException(nameof(fill), fill, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="errorCorrection">The error correction level to convert</param>
    /// <returns>The single-letter attribute string, e.g. "L"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="errorCorrection"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The arrow placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "anchor"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="autoSize">The auto-size flags to convert</param>
    /// <returns>The lowercase attribute string, e.g. "horizontal", or an empty string for <see cref="WaAutoSize.None"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="autoSize"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAutoSize autoSize)
    {
        return autoSize switch
        {
            WaAutoSize.None => "",
            WaAutoSize.Horizontal => "horizontal",
            WaAutoSize.Vertical => "vertical",
            WaAutoSize.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(autoSize), autoSize, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="sync">The sync flags to convert</param>
    /// <returns>The lowercase attribute string, e.g. "width", or an empty string for <see cref="WaSync.None"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sync"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="mode">The request mode value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "no-cors"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mode"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The relative time format value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "short"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRelativeTimeFormat format)
    {
        return format switch
        {
            WaRelativeTimeFormat.Long => "long",
            WaRelativeTimeFormat.Short => "short",
            WaRelativeTimeFormat.Narrow => "narrow",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="numeric">The relative time numeric mode value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "always"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numeric"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRelativeTimeNumeric numeric)
    {
        return numeric switch
        {
            WaRelativeTimeNumeric.Auto => "auto",
            WaRelativeTimeNumeric.Always => "always",
            _ => throw new ArgumentOutOfRangeException(nameof(numeric), numeric, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="display">The byte display mode value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "short"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="display"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDisplay display)
    {
        return display switch
        {
            WaDisplay.Short => "short",
            WaDisplay.Long => "long",
            WaDisplay.Narrow => "narrow",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="selection">The tree selection mode value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "multiple"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="selection"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTreeSelection selection)
    {
        return selection switch
        {
            WaTreeSelection.Single => "single",
            WaTreeSelection.Multiple => "multiple",
            WaTreeSelection.Leaf => "leaf",
            WaTreeSelection.LeafMultiple => "leaf-multiple",
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The page navigation placement value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "start"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPageNavigationPlacement placement)
    {
        return placement switch
        {
            WaPageNavigationPlacement.Start => "start",
            WaPageNavigationPlacement.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="view">The page view value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "desktop"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="view"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPageView view)
    {
        return view switch
        {
            WaPageView.Mobile => "mobile",
            WaPageView.Desktop => "desktop",
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The sparkline appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "solid"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaSparklineAppearance appearance)
    {
        return appearance switch
        {
            WaSparklineAppearance.Gradient => "gradient",
            WaSparklineAppearance.Line => "line",
            WaSparklineAppearance.Solid => "solid",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="curve">The sparkline curve value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "linear"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="curve"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaSparklineCurve curve)
    {
        return curve switch
        {
            WaSparklineCurve.Linear => "linear",
            WaSparklineCurve.Natural => "natural",
            WaSparklineCurve.Step => "step",
            _ => throw new ArgumentOutOfRangeException(nameof(curve), curve, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="trend">The sparkline trend value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "positive"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="trend"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaSparklineTrend trend)
    {
        return trend switch
        {
            WaSparklineTrend.Positive => "positive",
            WaSparklineTrend.Negative => "negative",
            WaSparklineTrend.Neutral => "neutral",
            _ => throw new ArgumentOutOfRangeException(nameof(trend), trend, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="animation">The icon animation value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "beat-fade"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="animation"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaIconAnimation animation)
    {
        return animation switch
        {
            WaIconAnimation.Beat => "beat",
            WaIconAnimation.Fade => "fade",
            WaIconAnimation.BeatFade => "beat-fade",
            WaIconAnimation.Bounce => "bounce",
            WaIconAnimation.Flip => "flip",
            WaIconAnimation.Shake => "shake",
            WaIconAnimation.Spin => "spin",
            WaIconAnimation.SpinPulse => "spin-pulse",
            WaIconAnimation.SpinReverse => "spin-reverse",
            WaIconAnimation.Flip360 => "flip-360",
            WaIconAnimation.SpinSnap => "spin-snap",
            WaIconAnimation.SpinSnap4 => "spin-snap-4",
            WaIconAnimation.SpinSnap8 => "spin-snap-8",
            WaIconAnimation.Buzz => "buzz",
            WaIconAnimation.Wag => "wag",
            WaIconAnimation.Float => "float",
            WaIconAnimation.Swing => "swing",
            WaIconAnimation.Jello => "jello",
            _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="flip">The icon flip direction value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "x"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="flip"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaFlip flip)
    {
        return flip switch
        {
            WaFlip.X => "x",
            WaFlip.Y => "y",
            WaFlip.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(flip), flip, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The chart type value to convert</param>
    /// <returns>The attribute string, e.g. "bar" or "polarArea"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaChartType type)
    {
        return type switch
        {
            WaChartType.Bar => "bar",
            WaChartType.Line => "line",
            WaChartType.Pie => "pie",
            WaChartType.Doughnut => "doughnut",
            WaChartType.PolarArea => "polarArea",
            WaChartType.Radar => "radar",
            WaChartType.Scatter => "scatter",
            WaChartType.Bubble => "bubble",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="grid">The chart grid value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "both"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaChartGrid grid)
    {
        return grid switch
        {
            WaChartGrid.X => "x",
            WaChartGrid.Y => "y",
            WaChartGrid.Both => "both",
            WaChartGrid.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(grid), grid, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="axis">The chart axis value to convert</param>
    /// <returns>The lowercase attribute string, "x" or "y"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="axis"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaChartAxis axis)
    {
        return axis switch
        {
            WaChartAxis.X => "x",
            WaChartAxis.Y => "y",
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="position">The chart legend position value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "top"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaChartLegendPosition position)
    {
        return position switch
        {
            WaChartLegendPosition.Top => "top",
            WaChartLegendPosition.Left => "left",
            WaChartLegendPosition.Bottom => "bottom",
            WaChartLegendPosition.Right => "right",
            WaChartLegendPosition.Start => "start",
            WaChartLegendPosition.End => "end",
            WaChartLegendPosition.ChartArea => "chartArea",
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="placement">The toast placement value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "top-end"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="placement"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaToastPlacement placement)
    {
        return placement switch
        {
            WaToastPlacement.TopStart => "top-start",
            WaToastPlacement.TopCenter => "top-center",
            WaToastPlacement.TopEnd => "top-end",
            WaToastPlacement.BottomStart => "bottom-start",
            WaToastPlacement.BottomCenter => "bottom-center",
            WaToastPlacement.BottomEnd => "bottom-end",
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="controls">The video controls preset value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "standard"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="controls"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaVideoControls controls)
    {
        return controls switch
        {
            WaVideoControls.Standard => "standard",
            WaVideoControls.None => "none",
            WaVideoControls.Full => "full",
            _ => throw new ArgumentOutOfRangeException(nameof(controls), controls, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="preload">The video preload value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "metadata"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="preload"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaVideoPreload preload)
    {
        return preload switch
        {
            WaVideoPreload.Metadata => "metadata",
            WaVideoPreload.Auto => "auto",
            WaVideoPreload.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(preload), preload, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="tooltip">The copy button tooltip value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "full"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tooltip"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaCopyButtonTooltip tooltip)
    {
        return tooltip switch
        {
            WaCopyButtonTooltip.Full => "full",
            WaCopyButtonTooltip.Copy => "copy",
            WaCopyButtonTooltip.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(tooltip), tooltip, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="mode">The accordion mode value to convert</param>
    /// <returns>The kebab-case attribute string, e.g. "single-collapsible"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mode"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAccordionMode mode)
    {
        return mode switch
        {
            WaAccordionMode.Single => "single",
            WaAccordionMode.SingleCollapsible => "single-collapsible",
            WaAccordionMode.Multiple => "multiple",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="day">The first-day-of-week value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "mon"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="day"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaFirstDayOfWeek day)
    {
        return day switch
        {
            WaFirstDayOfWeek.Auto => "auto",
            WaFirstDayOfWeek.Sun => "sun",
            WaFirstDayOfWeek.Mon => "mon",
            WaFirstDayOfWeek.Tue => "tue",
            WaFirstDayOfWeek.Wed => "wed",
            WaFirstDayOfWeek.Thu => "thu",
            WaFirstDayOfWeek.Fri => "fri",
            WaFirstDayOfWeek.Sat => "sat",
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="pageBy">The page-by value to convert</param>
    /// <returns>The lowercase attribute string, "months" or "single"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageBy"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDatePageBy pageBy)
    {
        return pageBy switch
        {
            WaDatePageBy.Months => "months",
            WaDatePageBy.Single => "single",
            _ => throw new ArgumentOutOfRangeException(nameof(pageBy), pageBy, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The weekday format value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "short"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaWeekdayFormat format)
    {
        return format switch
        {
            WaWeekdayFormat.Narrow => "narrow",
            WaWeekdayFormat.Short => "short",
            WaWeekdayFormat.Long => "long",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="view">The date picker view value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "days"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="view"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDatePickerView view)
    {
        return view switch
        {
            WaDatePickerView.Days => "days",
            WaDatePickerView.Months => "months",
            WaDatePickerView.Years => "years",
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The time hour format value to convert</param>
    /// <returns>The attribute string, "auto", "12", or "24"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTimeHourFormat format)
    {
        return format switch
        {
            WaTimeHourFormat.Auto => "auto",
            WaTimeHourFormat.Twelve => "12",
            WaTimeHourFormat.TwentyFour => "24",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="capture">The capture mode value to convert</param>
    /// <returns>The lowercase attribute string, "user" or "environment"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capture"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaCaptureMode capture)
    {
        return capture switch
        {
            WaCaptureMode.User => "user",
            WaCaptureMode.Environment => "environment",
            _ => throw new ArgumentOutOfRangeException(nameof(capture), capture, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The pagination appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPaginationAppearance appearance)
    {
        return appearance switch
        {
            WaPaginationAppearance.Outlined => "outlined",
            WaPaginationAppearance.Filled => "filled",
            WaPaginationAppearance.Plain => "plain",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="format">The pagination format value to convert</param>
    /// <returns>The lowercase attribute string, "standard" or "compact"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPaginationFormat format)
    {
        return format switch
        {
            WaPaginationFormat.Standard => "standard",
            WaPaginationFormat.Compact => "compact",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The OTP input appearance value to convert</param>
    /// <returns>The attribute string, e.g. "filled-outlined" or "contained"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaOtpInputAppearance appearance)
    {
        return appearance switch
        {
            WaOtpInputAppearance.Outlined => "outlined",
            WaOtpInputAppearance.Filled => "filled",
            WaOtpInputAppearance.FilledOutlined => "filled-outlined",
            WaOtpInputAppearance.Contained => "contained",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="type">The OTP input character class to convert</param>
    /// <returns>The lowercase attribute string, e.g. "alphanumeric"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaOtpInputType type)
    {
        return type switch
        {
            WaOtpInputType.Numeric => "numeric",
            WaOtpInputType.Alpha => "alpha",
            WaOtpInputType.Alphanumeric => "alphanumeric",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="caseValue">The OTP input case transformation to convert</param>
    /// <returns>The lowercase attribute string, e.g. "preserve"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="caseValue"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaOtpInputCase caseValue)
    {
        return caseValue switch
        {
            WaOtpInputCase.Preserve => "preserve",
            WaOtpInputCase.Upper => "upper",
            WaOtpInputCase.Lower => "lower",
            _ => throw new ArgumentOutOfRangeException(nameof(caseValue), caseValue, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="appearance">The data grid appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "outlined"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridAppearance appearance)
    {
        return appearance switch
        {
            WaDataGridAppearance.Outlined => "outlined",
            WaDataGridAppearance.Plain => "plain",
            _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="selectable">The data grid row-selection mode to convert</param>
    /// <returns>The lowercase attribute string, e.g. "multiple"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="selectable"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridSelectable selectable)
    {
        return selectable switch
        {
            WaDataGridSelectable.None => "none",
            WaDataGridSelectable.Single => "single",
            WaDataGridSelectable.Multiple => "multiple",
            _ => throw new ArgumentOutOfRangeException(nameof(selectable), selectable, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The autocapitalize value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "sentences"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaAutoCapitalize value)
    {
        return value switch
        {
            WaAutoCapitalize.Off => "off",
            WaAutoCapitalize.None => "none",
            WaAutoCapitalize.On => "on",
            WaAutoCapitalize.Sentences => "sentences",
            WaAutoCapitalize.Words => "words",
            WaAutoCapitalize.Characters => "characters",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The enter key hint to convert</param>
    /// <returns>The lowercase attribute string, e.g. "next"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaEnterKeyHint value)
    {
        return value switch
        {
            WaEnterKeyHint.Enter => "enter",
            WaEnterKeyHint.Done => "done",
            WaEnterKeyHint.Go => "go",
            WaEnterKeyHint.Next => "next",
            WaEnterKeyHint.Previous => "previous",
            WaEnterKeyHint.Search => "search",
            WaEnterKeyHint.Send => "send",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The input mode to convert</param>
    /// <returns>The lowercase attribute string, e.g. "numeric"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaInputMode value)
    {
        return value switch
        {
            WaInputMode.None => "none",
            WaInputMode.Text => "text",
            WaInputMode.Decimal => "decimal",
            WaInputMode.Numeric => "numeric",
            WaInputMode.Tel => "tel",
            WaInputMode.Search => "search",
            WaInputMode.Email => "email",
            WaInputMode.Url => "url",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="value">The number input mode to convert</param>
    /// <returns>The lowercase attribute string, "numeric" or "decimal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaNumberInputMode value)
    {
        return value switch
        {
            WaNumberInputMode.Numeric => "numeric",
            WaNumberInputMode.Decimal => "decimal",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The link target to convert</param>
    /// <returns>The keyword, e.g. "_blank"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaLinkTarget value)
    {
        return value switch
        {
            WaLinkTarget.Blank => "_blank",
            WaLinkTarget.Parent => "_parent",
            WaLinkTarget.Self => "_self",
            WaLinkTarget.Top => "_top",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The form method to convert</param>
    /// <returns>The lowercase attribute string, "post" or "get"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaFormMethod value)
    {
        return value switch
        {
            WaFormMethod.Post => "post",
            WaFormMethod.Get => "get",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The form encoding to convert</param>
    /// <returns>The MIME type, e.g. "multipart/form-data"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaFormEncType value)
    {
        return value switch
        {
            WaFormEncType.UrlEncoded => "application/x-www-form-urlencoded",
            WaFormEncType.MultipartFormData => "multipart/form-data",
            WaFormEncType.TextPlain => "text/plain",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="value">The flip fallback strategy to convert</param>
    /// <returns>The attribute string, "best-fit" or "initial"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaFlipFallbackStrategy value)
    {
        return value switch
        {
            WaFlipFallbackStrategy.BestFit => "best-fit",
            WaFlipFallbackStrategy.Initial => "initial",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="value">The popup boundary to convert</param>
    /// <returns>The lowercase attribute string, "viewport" or "scroll"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaPopupBoundary value)
    {
        return value switch
        {
            WaPopupBoundary.Viewport => "viewport",
            WaPopupBoundary.Scroll => "scroll",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="value">The heading level to convert</param>
    /// <returns>"1" to "6", or "none"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaHeadingLevel value)
    {
        return value switch
        {
            WaHeadingLevel.H1 => "1",
            WaHeadingLevel.H2 => "2",
            WaHeadingLevel.H3 => "3",
            WaHeadingLevel.H4 => "4",
            WaHeadingLevel.H5 => "5",
            WaHeadingLevel.H6 => "6",
            WaHeadingLevel.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The referrer policy to convert</param>
    /// <returns>The policy keyword, e.g. "no-referrer"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaReferrerPolicy value)
    {
        return value switch
        {
            WaReferrerPolicy.NoReferrer => "no-referrer",
            WaReferrerPolicy.NoReferrerWhenDowngrade => "no-referrer-when-downgrade",
            WaReferrerPolicy.Origin => "origin",
            WaReferrerPolicy.OriginWhenCrossOrigin => "origin-when-cross-origin",
            WaReferrerPolicy.SameOrigin => "same-origin",
            WaReferrerPolicy.StrictOrigin => "strict-origin",
            WaReferrerPolicy.StrictOriginWhenCrossOrigin => "strict-origin-when-cross-origin",
            WaReferrerPolicy.UnsafeUrl => "unsafe-url",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its HTML attribute string.
    /// </summary>
    /// <param name="value">The sandbox flags to convert</param>
    /// <returns>The set flags as space-separated tokens in declaration order, e.g. "allow-forms allow-scripts";
    /// empty for <see cref="WaIframeSandbox.None"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> contains undefined bits</exception>
    public static string ToHtmlValue(this WaIframeSandbox value)
    {
        return FlagTokens(value, nameof(value), SandboxTokens);
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="value">The page sections to convert</param>
    /// <returns>The set sections as space-separated tokens in the order banner, header, subheader, aside, menu</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is 0 or contains undefined bits</exception>
    public static string ToHtmlValue(this WaPageSections value)
    {
        if (value == 0) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        return FlagTokens(value, nameof(value), PageSectionTokens);
    }

    /// <summary>
    /// Converts the value to its Web Awesome event string.
    /// </summary>
    /// <param name="value">The hover phase to convert</param>
    /// <returns>"start", "move" or "end"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaRatingHoverPhase value)
    {
        return value switch
        {
            WaRatingHoverPhase.Start => "start",
            WaRatingHoverPhase.Move => "move",
            WaRatingHoverPhase.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its DOM string.
    /// </summary>
    /// <param name="value">The mutation type to convert</param>
    /// <returns>"attributes", "characterData" or "childList"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaMutationType value)
    {
        return value switch
        {
            WaMutationType.Attributes => "attributes",
            WaMutationType.CharacterData => "characterData",
            WaMutationType.ChildList => "childList",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string.
    /// </summary>
    /// <param name="value">The pin side to convert</param>
    /// <returns>"left" or "right"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridPinSide value)
    {
        return value switch
        {
            WaDataGridPinSide.Left => "left",
            WaDataGridPinSide.Right => "right",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string.
    /// </summary>
    /// <param name="value">The alignment to convert</param>
    /// <returns>"start", "center" or "end"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridAlign value)
    {
        return value switch
        {
            WaDataGridAlign.Start => "start",
            WaDataGridAlign.Center => "center",
            WaDataGridAlign.End => "end",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string.
    /// </summary>
    /// <param name="value">The sort function to convert</param>
    /// <returns>The camelCase name, e.g. "alphanumericCaseSensitive"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridSortFn value)
    {
        return value switch
        {
            WaDataGridSortFn.Alphanumeric => "alphanumeric",
            WaDataGridSortFn.AlphanumericCaseSensitive => "alphanumericCaseSensitive",
            WaDataGridSortFn.Text => "text",
            WaDataGridSortFn.TextCaseSensitive => "textCaseSensitive",
            WaDataGridSortFn.Datetime => "datetime",
            WaDataGridSortFn.Basic => "basic",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string; the numeric members (<see cref="WaDataGridSortUndefined.Lower"/>,
    /// <see cref="WaDataGridSortUndefined.Higher"/>) are sent as the JSON numbers -1 and 1.
    /// </summary>
    /// <param name="value">The sort position to convert</param>
    /// <returns>"first", "last", "-1" or "1"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridSortUndefined value)
    {
        return value switch
        {
            WaDataGridSortUndefined.First => "first",
            WaDataGridSortUndefined.Last => "last",
            WaDataGridSortUndefined.Lower => "-1",
            WaDataGridSortUndefined.Higher => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string.
    /// </summary>
    /// <param name="value">The filter type to convert</param>
    /// <returns>The filter name, e.g. "number-range"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridFilterType value)
    {
        return value switch
        {
            WaDataGridFilterType.Text => "text",
            WaDataGridFilterType.ExactMatch => "equals",
            WaDataGridFilterType.NumberRange => "number-range",
            WaDataGridFilterType.DateRange => "date-range",
            WaDataGridFilterType.Set => "set",
            WaDataGridFilterType.IncludesAny => "includes-any",
            WaDataGridFilterType.IncludesAll => "includes-all",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome string.
    /// </summary>
    /// <param name="value">The aggregation to convert</param>
    /// <returns>The aggregation name, e.g. "uniqueCount"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDataGridAggregation value)
    {
        return value switch
        {
            WaDataGridAggregation.Sum => "sum",
            WaDataGridAggregation.Min => "min",
            WaDataGridAggregation.Max => "max",
            WaDataGridAggregation.Extent => "extent",
            WaDataGridAggregation.Mean => "mean",
            WaDataGridAggregation.Median => "median",
            WaDataGridAggregation.Unique => "unique",
            WaDataGridAggregation.UniqueCount => "uniqueCount",
            WaDataGridAggregation.Count => "count",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    #region ------ Internals ------

    // the tokens of the WaIframeSandbox flags, in declaration order
    private static readonly (WaIframeSandbox Flag, string Token)[] SandboxTokens =
    {
        (WaIframeSandbox.AllowDownloads, "allow-downloads"),
        (WaIframeSandbox.AllowForms, "allow-forms"),
        (WaIframeSandbox.AllowModals, "allow-modals"),
        (WaIframeSandbox.AllowOrientationLock, "allow-orientation-lock"),
        (WaIframeSandbox.AllowPointerLock, "allow-pointer-lock"),
        (WaIframeSandbox.AllowPopups, "allow-popups"),
        (WaIframeSandbox.AllowPopupsToEscapeSandbox, "allow-popups-to-escape-sandbox"),
        (WaIframeSandbox.AllowPresentation, "allow-presentation"),
        (WaIframeSandbox.AllowSameOrigin, "allow-same-origin"),
        (WaIframeSandbox.AllowScripts, "allow-scripts"),
        (WaIframeSandbox.AllowTopNavigation, "allow-top-navigation"),
        (WaIframeSandbox.AllowTopNavigationByUserActivation, "allow-top-navigation-by-user-activation"),
        (WaIframeSandbox.AllowTopNavigationToCustomProtocols, "allow-top-navigation-to-custom-protocols"),
    };

    // the tokens of the WaPageSections flags, in the order the page documents them
    private static readonly (WaPageSections Flag, string Token)[] PageSectionTokens =
    {
        (WaPageSections.Banner, "banner"),
        (WaPageSections.Header, "header"),
        (WaPageSections.Subheader, "subheader"),
        (WaPageSections.Aside, "aside"),
        (WaPageSections.Menu, "menu"),
    };

    // joins the tokens of the set flags with a space, rejecting bits no token stands for
    private static string FlagTokens<TFlags>(TFlags value, string parameterName, (TFlags Flag, string Token)[] tokens)
        where TFlags : struct, Enum
    {
        var bits = Convert.ToUInt64(value);
        var known = 0UL;
        var result = new List<string>(tokens.Length);
        foreach (var (flag, token) in tokens)
        {
            var flagBits = Convert.ToUInt64(flag);
            known |= flagBits;
            if ((bits & flagBits) != 0) result.Add(token);
        }

        if ((bits & ~known) != 0) throw new ArgumentOutOfRangeException(parameterName, value, null);
        return string.Join(' ', result);
    }

    #endregion
}

#endregion
