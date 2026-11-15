// The link function of the "Links from a JavaScript Function" example on the pagination page. Blazor serves this
// collocated file at ./Pages/Components/PaginationPage.razor.js, and HrefTemplateFunction refers to its export with
// WaJsFunction.Module. The function gets the page number and a trailing ctx (ctx.args holds the WithArgs value).

// a relative URL with the page, the page size and the sort order (URLSearchParams encodes the values)
export function pageLink(page, ctx) {
  const query = new URLSearchParams({ page, size: ctx.args.size, sort: ctx.args.sort });
  return `?${query}`;
}
