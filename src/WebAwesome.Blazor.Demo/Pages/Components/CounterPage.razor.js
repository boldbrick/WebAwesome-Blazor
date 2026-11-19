// The value formatter of the "Formatting the Value with JavaScript" example on the counter page. Blazor serves this
// collocated file at ./Pages/Components/CounterPage.razor.js, and ValueFormatterFunction refers to its export with
// WaJsFunction.Module. The function gets the value of one unit, the unit's name and a trailing ctx (ctx.args holds the
// WithArgs value).

// the value padded to two digits, except in the units the args list as unpadded
export function padExcept(value, unit, ctx) {
  return ctx.args.unpadded.includes(unit) ? String(value) : String(value).padStart(2, '0');
}
