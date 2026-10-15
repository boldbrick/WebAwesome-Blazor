using System;
using System.Diagnostics.CodeAnalysis;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// An icon that a cell preset renders as a <c>wa-icon</c> element. A string converts to an icon with that name, so a
/// map can be written as <c>["active"] = "circle-check"</c>.
/// </summary>
public sealed record WaDataGridIcon
{
    /// <summary>
    /// The icon name, for example <c>circle-check</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The icon family, for example <c>classic</c> or <c>sharp</c>; null for the default family.
    /// </summary>
    public string? Family { get; init; }

    /// <summary>
    /// The icon variant, for example <c>solid</c> or <c>regular</c>; null for the default variant.
    /// </summary>
    public string? Variant { get; init; }

    /// <summary>
    /// The name of a registered icon library; null for the default library.
    /// </summary>
    public string? Library { get; init; }

    /// <summary>
    /// Converts an icon name to an icon of the default family, variant and library; null stays null.
    /// </summary>
    /// <param name="name">The icon name</param>
    [return: NotNullIfNotNull(nameof(name))]
    public static implicit operator WaDataGridIcon?(string? name) => name is null ? null : new(name);

    #region ------ Constructors ------

    /// <summary>
    /// Creates an icon.
    /// </summary>
    /// <param name="name">The icon name, for example <c>circle-check</c></param>
    /// <exception cref="ArgumentException">Thrown when the name is empty or whitespace</exception>
    public WaDataGridIcon(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("An icon name must not be empty.", nameof(name));
        Name = name;
    }

    #endregion
}
