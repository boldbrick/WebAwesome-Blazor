// The calendar functions of the "Rules & Day Content with JavaScript" example on the date picker page. Blazor serves
// this collocated file at ./Pages/Components/DatePickerPage.razor.js, and IsDateDisabledFunction and DayContentFunction
// refer to its exports with WaJsFunction.Module. Each function gets the day as a Date at local midnight and a trailing
// ctx (ctx.args holds the WithArgs value).

// the second Tuesday of each month is a maintenance day
export function secondTuesday(date) {
  return date.getDay() === 2 && date.getDate() > 7 && date.getDate() <= 14;
}

// the pay days (ctx.args.days) show their number in a span that ShadowStyles styles; null keeps the day number
export function payDay(date, ctx) {
  return ctx.args.days.includes(date.getDate()) ? `<span class="pay-day">${date.getDate()}</span>` : null;
}
