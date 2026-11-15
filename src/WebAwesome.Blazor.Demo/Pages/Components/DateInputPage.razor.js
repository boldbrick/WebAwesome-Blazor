// The calendar rule of the "Disabled Days with JavaScript" example on the date input page. Blazor serves this collocated
// file at ./Pages/Components/DateInputPage.razor.js, and IsDateDisabledFunction refers to its export with
// WaJsFunction.Module. The function gets the day as a Date at local midnight and a trailing ctx (ctx.args holds the
// WithArgs value).

// true (disabled) for every day of the week that is not an opening day (ctx.args.openDays, 0 is Sunday)
export function closedDay(date, ctx) {
  return !ctx.args.openDays.includes(date.getDay());
}
