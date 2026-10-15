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
}
