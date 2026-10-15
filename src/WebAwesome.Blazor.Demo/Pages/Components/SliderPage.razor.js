// The value formatter of the "Value Formatter with JavaScript" example on the slider page. Blazor serves this collocated
// file at ./Pages/Components/SliderPage.razor.js, and ValueFormatterFunction refers to its export with
// WaJsFunction.Module. The function gets the value and a trailing ctx (ctx.args holds the WithArgs value).

// the value with a unit (an Intl unit identifier) in the given locale; the invariant culture has no name, so an empty
// locale uses the browser's
export function formatUnit(value, ctx) {
  return new Intl.NumberFormat(ctx.args.locale || undefined, { style: 'unit', unit: ctx.args.unit }).format(value);
}
