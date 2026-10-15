// The symbol function of the "Custom Symbols with JavaScript" example on the rating page. Blazor serves this collocated
// file at ./Pages/Components/RatingPage.razor.js, and GetSymbolFunction refers to its export with WaJsFunction.Module.
// The function gets the symbol's value, whether it is selected, and a trailing ctx (ctx.args holds the WithArgs value).

// a solid icon for a selected symbol and a regular one otherwise; wa-rating expects a string of trusted HTML, and the
// icon name comes from C#, never from user input
export function iconSymbol(value, isSelected, ctx) {
  const variant = isSelected ? 'solid' : 'regular';
  return `<wa-icon name="${ctx.args.icon}" variant="${variant}"></wa-icon>`;
}
