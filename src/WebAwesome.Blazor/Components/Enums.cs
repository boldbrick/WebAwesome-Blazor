using System;

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
/// Visual appearance style for Web Awesome components
/// </summary>
public enum WaAppearance
{
    /// <summary>Draws the component with a filled background.</summary>
    Filled,
    /// <summary>Draws the component with an outline and no fill.</summary>
    Outlined,
    /// <summary>Draws the component with both an outline and a filled background.</summary>
    OutlinedFilled,
    /// <summary>Draws the component as plain text with no border or fill.</summary>
    Text,
    /// <summary>Draws the component with minimal, unstyled chrome.</summary>
    Plain,
    /// <summary>Draws the component using the accent theme styling.</summary>
    Accent
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

#endregion

#region ------ Placement & Positioning ------

/// <summary>
/// Placement positions for Web Awesome components
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
    /// <summary>Places the element at the logical start of its anchor.</summary>
    Start,
    /// <summary>Places the element at the logical end of its anchor.</summary>
    End,
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
    Date
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
    Both
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
/// Dropdown trigger types
/// </summary>
[Flags]
public enum WaDropdownTrigger
{
    /// <summary>No trigger is active.</summary>
    None = 0,
    /// <summary>Opens the dropdown on click.</summary>
    Click = 1,
    /// <summary>Opens the dropdown on hover.</summary>
    Hover = 2,
    /// <summary>Opens the dropdown on focus.</summary>
    Focus = 4,
    /// <summary>The dropdown is opened and closed only via the API.</summary>
    Manual = 8,
    /// <summary>Opens the dropdown on both hover and focus.</summary>
    HoverFocus = Hover | Focus
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
    /// <summary>Displays the currency code, e.g. "USD".</summary>
    Code,
    /// <summary>Displays the localized currency name, e.g. "dollar".</summary>
    Name
}

/// <summary>
/// Number notation types
/// </summary>
public enum WaNotation
{
    /// <summary>Plain number formatting.</summary>
    Standard,
    /// <summary>Scientific notation, e.g. "1.2E3".</summary>
    Scientific,
    /// <summary>Engineering notation, exponents are multiples of three.</summary>
    Engineering,
    /// <summary>Compact notation using unit prefixes, e.g. "1.2K".</summary>
    Compact
}

/// <summary>
/// Compact display modes
/// </summary>
public enum WaCompactDisplay
{
    /// <summary>Uses short compact suffixes, e.g. "1.2K".</summary>
    Short,
    /// <summary>Uses long compact suffixes, e.g. "1.2 thousand".</summary>
    Long
}

/// <summary>
/// Auto-size behavior for popups
/// </summary>
[Flags]
public enum WaAutoSize
{
    /// <summary>No auto-sizing is applied.</summary>
    None = 0,
    /// <summary>Automatically resizes the width to fit available space.</summary>
    Width = 1,
    /// <summary>Automatically resizes the height to fit available space.</summary>
    Height = 2,
    /// <summary>Automatically resizes both width and height to fit available space.</summary>
    Both = Width | Height
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
/// Relative time format styles
/// </summary>
public enum WaFormat
{
    /// <summary>Uses friendly terms such as "yesterday" or "tomorrow" when possible.</summary>
    Auto,
    /// <summary>Always uses relative phrasing, e.g. "1 day ago".</summary>
    Relative,
    /// <summary>Always uses numeric phrasing, e.g. "in 1 day".</summary>
    Numeric
}

/// <summary>
/// Format bytes display modes
/// </summary>
public enum WaDisplay
{
    /// <summary>Uses short unit abbreviations, e.g. "KB".</summary>
    Short,
    /// <summary>Uses long unit names, e.g. "kilobytes".</summary>
    Long
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
/// Tooltip trigger types
/// </summary>
[Flags]
public enum WaTriggerType
{
    /// <summary>No trigger is active.</summary>
    None = 0,
    /// <summary>Activates on hover.</summary>
    Hover = 1,
    /// <summary>Activates on focus.</summary>
    Focus = 2,
    /// <summary>Activated and deactivated only via the API.</summary>
    Manual = 4,
    /// <summary>Activates on both hover and focus.</summary>
    HoverFocus = Hover | Focus
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
    Checkbox,
    /// <summary>A dropdown item that is mutually exclusive within its group.</summary>
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
    /// <summary>Opens on hover.</summary>
    Hover,
    /// <summary>Opens on click.</summary>
    Click,
    /// <summary>Opened and closed only via the API.</summary>
    Manual
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
/// Date/time style options for format-date component
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
/// Hour format for format-date component
/// </summary>
public enum WaHourFormat
{
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
    SpinReverse
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
    End
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
/// Selection mode for the date input and date picker.
/// </summary>
public enum WaDateSelectionMode
{
    /// <summary>Selects a single date.</summary>
    Single,
    /// <summary>Selects a start/end date range.</summary>
    Range
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
    /// <returns>The lowercase attribute string, e.g. "small"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaSize size)
    {
        return size switch
        {
            WaSize.ExtraSmall => "xs",
            WaSize.Small => "small",
            WaSize.Medium => "medium",
            WaSize.Large => "large",
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
            WaAppearance.Text => "text",
            WaAppearance.Plain => "plain",
            WaAppearance.Accent => "accent",
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="attention">The attention value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "pulse"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attention"/> is not a defined enum value</exception>
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
    /// <param name="appearance">The radio appearance value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "normal"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="appearance"/> is not a defined enum value</exception>
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
    /// <param name="trigger">The trigger value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "hover"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="trigger"/> is not a defined enum value</exception>
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
            WaDropdownItemType.Radio => "radio",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
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
    /// <param name="format">The hour format value to convert</param>
    /// <returns>The attribute string, "12" or "24"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
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
            WaCurrencyDisplay.Code => "code",
            WaCurrencyDisplay.Name => "name",
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="notation">The notation value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "standard"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="notation"/> is not a defined enum value</exception>
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
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="display">The compact display value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "short"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="display"/> is not a defined enum value</exception>
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
    /// <returns>The lowercase attribute string, e.g. "width", or an empty string for <see cref="WaAutoSize.None"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="autoSize"/> is not a defined enum value</exception>
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
    /// <returns>The lowercase attribute string, e.g. "auto"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a defined enum value</exception>
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
    /// <param name="mode">The date selection mode value to convert</param>
    /// <returns>The lowercase attribute string, "single" or "range"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mode"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaDateSelectionMode mode)
    {
        return mode switch
        {
            WaDateSelectionMode.Single => "single",
            WaDateSelectionMode.Range => "range",
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
}

#endregion
