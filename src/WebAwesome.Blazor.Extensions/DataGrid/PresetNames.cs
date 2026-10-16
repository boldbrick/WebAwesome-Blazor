namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// The names under which the Extensions JS initializer registers the data grid preset functions
/// (<c>wwwroot\data-grid-presets.js</c>, <c>presets</c>). The <c>wab.</c> prefix marks the functions of the packages, so
/// they do not collide with the names of an app.
/// </summary>
internal static class PresetNames
{
    /// <summary>
    /// The common prefix of the data grid presets.
    /// </summary>
    public const string Prefix = "wab.dataGrid.";

    /// <summary>A localized number.</summary>
    public const string Number = Prefix + "number";

    /// <summary>A localized amount of money.</summary>
    public const string Currency = Prefix + "currency";

    /// <summary>A localized percentage.</summary>
    public const string Percent = Prefix + "percent";

    /// <summary>A localized date.</summary>
    public const string Date = Prefix + "date";

    /// <summary>A localized date and time.</summary>
    public const string DateTime = Prefix + "dateTime";

    /// <summary>A localized time of day.</summary>
    public const string Time = Prefix + "time";

    /// <summary>A localized time relative to now.</summary>
    public const string RelativeTime = Prefix + "relativeTime";

    /// <summary>A localized size in bytes or bits.</summary>
    public const string Bytes = Prefix + "bytes";

    /// <summary>A localized duration.</summary>
    public const string Duration = Prefix + "duration";

    /// <summary>A display text for each value.</summary>
    public const string Lookup = Prefix + "lookup";

    /// <summary>A text for true, false and null.</summary>
    public const string Boolean = Prefix + "boolean";

    /// <summary>A text template with row placeholders.</summary>
    public const string Template = Prefix + "template";

    /// <summary>The items of an array, joined.</summary>
    public const string List = Prefix + "list";

    /// <summary>A wa-badge.</summary>
    public const string Badge = Prefix + "badge";

    /// <summary>A wa-tag.</summary>
    public const string Tag = Prefix + "tag";

    /// <summary>A wa-tag for each item of an array.</summary>
    public const string Tags = Prefix + "tags";

    /// <summary>A wa-icon for each value.</summary>
    public const string Icon = Prefix + "icon";

    /// <summary>A link with a URL template.</summary>
    public const string Link = Prefix + "link";

    /// <summary>A button that looks like a link and raises a cell action.</summary>
    public const string ActionLink = Prefix + "actionLink";

    /// <summary>A mailto link.</summary>
    public const string Email = Prefix + "email";

    /// <summary>A tel link.</summary>
    public const string Phone = Prefix + "phone";

    /// <summary>A sort by the position of the value in a list.</summary>
    public const string Ordinal = Prefix + "ordinal";

    /// <summary>A wa-button that raises a cell action.</summary>
    public const string Button = Prefix + "button";

    /// <summary>Several action buttons, and a menu for the rest.</summary>
    public const string Actions = Prefix + "actions";

    /// <summary>A menu of actions behind one trigger button.</summary>
    public const string Menu = Prefix + "menu";

    /// <summary>A wa-switch that raises a cell action when it changes.</summary>
    public const string Switch = Prefix + "switch";

    /// <summary>A wa-checkbox that raises a cell action when it changes.</summary>
    public const string Checkbox = Prefix + "checkbox";

    /// <summary>A wa-progress-bar.</summary>
    public const string ProgressBar = Prefix + "progressBar";

    /// <summary>A wa-progress-ring.</summary>
    public const string ProgressRing = Prefix + "progressRing";

    /// <summary>A read-only wa-rating.</summary>
    public const string Rating = Prefix + "rating";

    /// <summary>A wa-avatar.</summary>
    public const string Avatar = Prefix + "avatar";

    /// <summary>An img element.</summary>
    public const string Image = Prefix + "image";

    /// <summary>A color swatch.</summary>
    public const string ColorSwatch = Prefix + "colorSwatch";

    /// <summary>A wa-sparkline.</summary>
    public const string Sparkline = Prefix + "sparkline";

    /// <summary>A number with a trend icon and color.</summary>
    public const string Trend = Prefix + "trend";

    /// <summary>A horizontal bar for a number.</summary>
    public const string DataBar = Prefix + "dataBar";

    /// <summary>A wa-copy-button.</summary>
    public const string CopyButton = Prefix + "copyButton";

    /// <summary>A sort of text with Intl.Collator.</summary>
    public const string Collation = Prefix + "collation";

    /// <summary>The class of a cell by rules.</summary>
    public const string CellClassRules = Prefix + "cellClassRules";

    /// <summary>The class of a row by rules.</summary>
    public const string RowClassRules = Prefix + "rowClassRules";

    /// <summary>An aggregate of a column in its footer.</summary>
    public const string FooterAggregate = Prefix + "footerAggregate";

    /// <summary>The global search that also compares the labels of lookup and badge columns.</summary>
    public const string SearchFormatted = Prefix + "searchFormatted";

    /// <summary>A detail panel with a list of fields.</summary>
    public const string DetailFields = Prefix + "detailFields";

    /// <summary>The rows that a field marks as selectable.</summary>
    public const string SelectableField = Prefix + "selectableField";

    /// <summary>
    /// The text presets whose output does not depend on the row: they can format a value inside another preset (a
    /// footer, a trend, a data bar, a detail field).
    /// </summary>
    public static readonly string[] TextFormats = [Number, Currency, Percent, Date, DateTime, Time, RelativeTime, Bytes, Duration, Lookup, Boolean, List];

    /// <summary>
    /// The presets whose arguments hold the display texts of values (<c>labels</c>): the global search and the filter
    /// options can use them.
    /// </summary>
    public static readonly string[] Labeled = [Lookup, Badge, Tag, Tags, Icon];
}
