using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental standalone calendar for selecting a date or date range. Unlike <see cref="WaDateInput"/>
/// it is not a form-associated control; bind its value with <c>@bind-Value</c>.
/// Corresponds to the wa-date-picker Web Awesome component.
/// </summary>
public class WaDatePicker : WaDatePickerBase<string?>
{
    /// <summary>
    /// The selection mode.
    /// </summary>
    [Parameter] public WaDateSelectionMode? Mode { get; set; }

    /// <summary>
    /// Minimum range length in days (range mode only). <c>0</c> disables the check.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// Maximum range length in days (range mode only). <c>0</c> disables the check.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

    #region ------ Internals ------

    /// <inheritdoc />
    private protected override string? FormatValue(string? value) => value;

    /// <inheritdoc />
    private protected override string? ParseValue(string? value) => value;

    /// <inheritdoc />
    private protected override void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttributeIfNotNull(sequence + 0, "mode", Mode?.ToHtmlValue());
        builder.AddAttributeIfNotNull(sequence + 1, "min-range", MinRange);
        builder.AddAttributeIfNotNull(sequence + 2, "max-range", MaxRange);
    }

    #endregion
}
