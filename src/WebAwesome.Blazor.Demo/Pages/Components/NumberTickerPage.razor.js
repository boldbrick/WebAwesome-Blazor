// The value formatter of the "Formatting the Value with JavaScript" example on the number ticker page. Blazor serves
// this collocated file at ./Pages/Components/NumberTickerPage.razor.js, and ValueFormatterFunction refers to its export
// with WaJsFunction.Module. The function gets the unrounded value of each frame and a trailing ctx (ctx.args holds the
// WithArgs value).

// the value in compact notation (2.5M) in the given locale; the invariant culture has no name, so an empty locale uses
// the browser's
export function formatCompact(value, ctx) {
  return new Intl.NumberFormat(ctx.args.locale || undefined, { notation: 'compact', maximumFractionDigits: 1 }).format(value);
}
