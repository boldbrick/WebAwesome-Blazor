using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Custom content for the day cell of one date in a calendar: rendered into the element's <c>day-YYYY-MM-DD</c> slot
/// (e.g. <c>day-2026-12-25</c>), where it replaces the day's number while the cell keeps its button, ARIA and click
/// handling. Useful for holidays, badges or dots. Place it in the ChildContent of <see cref="WaDatePicker"/>,
/// <see cref="WaDateRangePicker"/>, <see cref="WaDateInput"/> or <see cref="WaDateRangeInput"/> (which forwards it
/// to its popup calendar); it renders a <c>&lt;span slot="day-…"&gt;</c> as a direct child of that element.
/// </summary>
/// <remarks>
/// <para>
/// The slot replaces the whole label, so include the day number in the content when it should stay visible.
/// </para>
/// <para>
/// Several day contents for the same date are all shown, in document order: every element assigned to one slot
/// name is shown in it, and wa-date-input forwards each name once. Content for a date outside the visible month
/// shows when the user navigates there. Without ChildContent nothing is rendered and the day keeps its number.
/// </para>
/// <para>
/// Web Awesome's JS-only <c>dayContent</c> callback (content computed for every rendered cell) is not supported:
/// it would need a JS round-trip per cell. Render one <see cref="WaDayContent"/> per date instead, e.g. with a
/// loop over the dates of interest.
/// </para>
/// </remarks>
public class WaDayContent : ComponentBase, IDisposable
{
    /// <summary>
    /// The date whose day cell shows the content.
    /// </summary>
    [Parameter, EditorRequired] public DateOnly Date { get; set; }

    /// <summary>
    /// The content shown in the day cell, in place of the day's number.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #region ------ Overrides ------

    /// <summary>
    /// Validates the placement and tells the host when this content's day slot appeared, moved to another date or
    /// disappeared.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the day content is not placed in the ChildContent of a
    /// date input or date picker</exception>
    protected override void OnParametersSet()
    {
        if (Host is null)
        {
            throw new InvalidOperationException(
                $"{nameof(WaDayContent)} must be placed in the ChildContent of {nameof(WaDatePicker)}, {nameof(WaDateRangePicker)}, " +
                $"{nameof(WaDateInput)} or {nameof(WaDateRangeInput)}: it renders into a day-YYYY-MM-DD slot of that element, " +
                "which exists nowhere else.");
        }

        // only a rendered slot (with content) exists for the host
        DateOnly? slotDate = ChildContent is null ? null : Date;
        if (slotDate == registeredSlotDate) return;

        registeredSlotDate = slotDate;
        Host.DayContentChanged();
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddSlotContent(0, WaWireFormat.FormatDaySlotName(Date), ChildContent);
    }

    #endregion

    #region ------ Implementation of IDisposable ------

    /// <summary>
    /// Tells the host that this content's day slot disappeared.
    /// </summary>
    public void Dispose()
    {
        if (registeredSlotDate is null) return;

        registeredSlotDate = null;
        Host?.DayContentChanged();
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// The date input or date picker whose ChildContent holds this content, cascaded by it; null outside one.
    /// </summary>
    [CascadingParameter] private IWaDayContentHost? Host { get; set; }

    // the date of the slot the host was last told about, null while none is rendered
    private DateOnly? registeredSlotDate;

    #endregion
}
