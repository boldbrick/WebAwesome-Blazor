using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental date input with segmented text entry and a popup calendar.
/// Corresponds to the wa-date-input Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaDateInput : WaDateInputBase<string?>
{
    /// <summary>
    /// Selection mode.
    /// </summary>
    [Parameter] public WaDateSelectionMode Mode { get; set; } = WaDateSelectionMode.Single;

    /// <summary>
    /// Minimum range length in days (range mode only). <c>0</c> disables.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// Maximum range length in days (range mode only). <c>0</c> disables.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #endregion

    #region ------ Internals ------

    /// <inheritdoc />
    private protected override void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
        if (Mode != WaDateSelectionMode.Single)
            builder.AddAttribute(sequence + 0, "mode", Mode.ToHtmlValue());
        builder.AddAttributeIfNotNull(sequence + 1, "min-range", MinRange);
        builder.AddAttributeIfNotNull(sequence + 2, "max-range", MaxRange);
    }

    #endregion
}
