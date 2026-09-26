using System;
using System.Collections.Generic;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// The calendar options wa-date-input and wa-date-picker share (the date input forwards them to its popup
/// calendar): the selectable range, the disabled dates and the calendar's layout. Groups the members for consumers
/// and tests; the [Parameter] properties are declared by the implementing wrapper, and one renderer emits them in
/// Web Awesome's culture-free wire formats (ISO <c>yyyy-MM-dd</c> dates, <c>sun</c> … <c>sat</c> weekday tokens).
/// </summary>
public interface IWaCalendarOptions
{
    /// <summary>
    /// The earliest selectable date; null leaves the calendar unbounded.
    /// </summary>
    DateOnly? Min { get; }

    /// <summary>
    /// The latest selectable date; null leaves the calendar unbounded.
    /// </summary>
    DateOnly? Max { get; }

    /// <summary>
    /// Overrides the date considered "today"; null uses the browser's current date.
    /// </summary>
    DateOnly? Today { get; }

    /// <summary>
    /// Dates that cannot be selected; null or an empty set disables none.
    /// </summary>
    IReadOnlySet<DateOnly>? DisabledDates { get; }

    /// <summary>
    /// Days of the week that cannot be selected; null or an empty set disables none.
    /// </summary>
    IReadOnlySet<DayOfWeek>? DisabledDaysOfWeek { get; }

    /// <summary>
    /// Disables all dates strictly before today.
    /// </summary>
    bool DisablePast { get; }

    /// <summary>
    /// Disables all dates strictly after today.
    /// </summary>
    bool DisableFuture { get; }

    /// <summary>
    /// The first day of the week.
    /// </summary>
    WaFirstDayOfWeek? FirstDayOfWeek { get; }

    /// <summary>
    /// The number of months rendered side by side, 1 or 2.
    /// </summary>
    int? Months { get; }

    /// <summary>
    /// Whether previous/next pages by the visible range or one month at a time.
    /// </summary>
    WaDatePageBy? PageBy { get; }

    /// <summary>
    /// The weekday header format.
    /// </summary>
    WaWeekdayFormat? WeekdayFormat { get; }

    /// <summary>
    /// Shows leading and trailing days from adjacent months.
    /// </summary>
    bool WithOutsideDays { get; }

    /// <summary>
    /// Shows an ISO 8601 week-number column.
    /// </summary>
    bool WithWeekNumbers { get; }
}
