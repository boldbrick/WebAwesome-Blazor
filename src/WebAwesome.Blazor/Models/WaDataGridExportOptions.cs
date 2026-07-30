using System.Collections.Generic;

namespace WebAwesome.Blazor.Models;

#region ------ Data Grid Export/Copy Option Models ------

/// <summary>
/// Options for <c>GetDataAsCsvAsync</c>.
/// </summary>
public class WaDataGridCsvOptions
{
    /// <summary>
    /// The column ids to include, in order. All visible columns (in their display order) when omitted.
    /// </summary>
    public IReadOnlyList<string>? ColumnIds { get; set; }

    /// <summary>
    /// Whether to include a header row. Defaults to true.
    /// </summary>
    public bool? IncludeHeaders { get; set; }

    /// <summary>
    /// The field delimiter. Defaults to a comma.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// When true, cells starting with <c>=</c>, <c>+</c>, <c>-</c>, or <c>@</c> are prefixed with an
    /// apostrophe so they can't execute as spreadsheet formulas.
    /// </summary>
    public bool? EscapeFormulas { get; set; }
}

/// <summary>
/// Options for <c>ExportDataAsCsvAsync</c>.
/// </summary>
public class WaDataGridCsvExportOptions
{
    /// <summary>
    /// The downloaded file's name (without requiring the <c>.csv</c> extension).
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The column ids to include, in order. All visible columns (in their display order) when omitted.
    /// </summary>
    public IReadOnlyList<string>? ColumnIds { get; set; }

    /// <summary>
    /// Whether to include a header row. Defaults to true.
    /// </summary>
    public bool? IncludeHeaders { get; set; }

    /// <summary>
    /// The field delimiter. Defaults to a comma.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// When true, cells starting with <c>=</c>, <c>+</c>, <c>-</c>, or <c>@</c> are prefixed with an
    /// apostrophe so they can't execute as spreadsheet formulas.
    /// </summary>
    public bool? EscapeFormulas { get; set; }
}

/// <summary>
/// Options for <c>CopySelectedRowsAsync</c>.
/// </summary>
public class WaDataGridCopyOptions
{
    /// <summary>
    /// The column ids to include, in order. All visible columns (in their display order) when omitted.
    /// </summary>
    public IReadOnlyList<string>? ColumnIds { get; set; }

    /// <summary>
    /// Whether to include a header row. Defaults to true.
    /// </summary>
    public bool? IncludeHeaders { get; set; }

    /// <summary>
    /// The clipboard format: <c>tsv</c> (default, pastes into spreadsheet cells) or <c>csv</c>.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// When true, cells starting with <c>=</c>, <c>+</c>, <c>-</c>, or <c>@</c> are prefixed with an
    /// apostrophe so they can't execute as spreadsheet formulas.
    /// </summary>
    public bool? EscapeFormulas { get; set; }
}

#endregion
