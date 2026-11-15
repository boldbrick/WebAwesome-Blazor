// The calendar rule of the "Disabled Days with JavaScript" example on the date range input page. Blazor serves this
// collocated file at ./Pages/Components/DateRangeInputPage.razor.js, and IsDateDisabledFunction refers to its export with
// WaJsFunction.Module. The function gets the day as a Date at local midnight and a trailing ctx (ctx.args holds the
// WithArgs value).

// true (disabled) for the last ctx.args.days days of each month
export function monthEnd(date, ctx) {
  const lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
  return date.getDate() > lastDay - ctx.args.days;
}
