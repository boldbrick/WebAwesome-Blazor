// The day content function of the "Day Content with JavaScript" example on the date range picker page. Blazor serves
// this collocated file at ./Pages/Components/DateRangePickerPage.razor.js, and DayContentFunction refers to its export
// with WaJsFunction.Module. The function gets the day as a Date at local midnight and a trailing ctx (ctx.args holds the
// WithArgs value).

// the day number with the price of a night under it (trusted HTML: the prices are numbers from C#)
export function nightPrice(date, ctx) {
  const price = ctx.args.prices[date.getDay()];
  return `<span class="night">${date.getDate()}<span class="night-price">€${price}</span></span>`;
}
