using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental date input with segmented text entry and a popup calendar, bound to a date range (a start and an
/// end date). Renders wa-date-input in range mode; for a single date, use <see cref="WaDateInput"/>.
/// Corresponds to the wa-date-input Web Awesome component with <c>mode="range"</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is a Pro component.
/// </para>
/// <para>
/// The value travels in the element's wire form, <c>yyyy-MM-dd/yyyy-MM-dd</c>, converted culture-free (see
/// <see cref="WaDateRange"/>). While the user has picked only the first date, the element reports that one date, which
/// binds as a half-filled range (<see cref="WaDateRange.From"/> set, <see cref="WaDateRange.To"/> null); the element
/// orders a complete range, so the earlier date binds as <see cref="WaDateRange.From"/>. An empty element value binds
/// as null; a value that is not an ISO date or date range adds the validation message "The {field} field must be a
/// date range." to the edit context.
/// </para>
/// </remarks>
public class WaDateRangeInput : WaDateInputBase<WaDateRange?>
{
    /// <summary>
    /// The Web Awesome default of <see cref="MinRange"/> (0, no limit): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultMinRange = 0;

    /// <summary>
    /// Minimum range length in days, counting both ends; null (or 0) sets no minimum.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="MaxRange"/> (0, no limit): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultMaxRange = 0;

    /// <summary>
    /// Maximum range length in days, counting both ends; null (or 0) sets no maximum.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

    #region ------ Overrides ------

    /// <summary>
    /// Formats the value in the element's wire form (<c>from/to</c>, or the one date of a half-filled range),
    /// culture-free; null and the empty range render no value.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The wire string</returns>
    protected override string? FormatValueAsString(WaDateRange? value)
        => value is { IsEmpty: false } range ? WaWireFormat.FormatDateRange(range) : null;

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out WaDateRange? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (WaWireFormat.TryParseDateRange(value, out var range))
        {
            result = range;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = FormatValidationMessage(Constants.DateRangeValidationMessageFormat);
        return false;
    }

    #endregion

    #region ------ Internals ------

    /// <inheritdoc />
    private protected override void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttribute(sequence + 0, Constants.ModeAttribute, Constants.RangeModeValue);
        builder.AddAttributeIfNotNull(WaAttributeMemory.Of(this), sequence + 1, Constants.MinRangeAttribute, MinRange, DefaultMinRange);
        builder.AddAttributeIfNotNull(WaAttributeMemory.Of(this), sequence + 2, Constants.MaxRangeAttribute, MaxRange, DefaultMaxRange);
    }

    #endregion
}
