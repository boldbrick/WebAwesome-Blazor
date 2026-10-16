using System;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// A color step of <see cref="WaDataGridFormatters.ProgressBar"/> and <see cref="WaDataGridFormatters.ProgressRing"/>:
/// a value at or above <see cref="From"/> uses <see cref="Variant"/>, until the next step.
/// </summary>
public sealed record WaDataGridThreshold
{
    /// <summary>
    /// The lowest value of the step, in the scale of the column values (0.8 for a fraction, 80 for a percentage).
    /// </summary>
    public double From { get; }

    /// <summary>
    /// The variant whose color the indicator takes.
    /// </summary>
    public WaVariant Variant { get; }

    #region ------ Constructors ------

    /// <summary>
    /// Creates a color step.
    /// </summary>
    /// <param name="from">The lowest value of the step, in the scale of the column values</param>
    /// <param name="variant">The variant whose color the indicator takes</param>
    /// <exception cref="ArgumentException">Thrown when from is not a finite number</exception>
    public WaDataGridThreshold(double from, WaVariant variant)
    {
        if (!double.IsFinite(from)) throw new ArgumentException("A threshold must be a finite number.", nameof(from));
        From = from;
        Variant = variant;
    }

    #endregion
}
