// Cell functions of the "Custom Cell Rendering with JavaScript" example on the data grid page. Blazor serves this
// collocated file at ./Pages/Components/DataGridPage.razor.js, and the columns refer to its exports with
// WaJsFunction.Module. Each function gets the grid's arguments and a trailing ctx (ctx.args holds the WithArgs value).

// a badge for the status; ctx.args.variants maps each status to a badge variant
export function statusBadge(value, row, ctx) {
  const badge = document.createElement('wa-badge');
  badge.variant = ctx.args.variants[value] ?? 'neutral';
  badge.textContent = value;
  return badge;
}

// sorts the statuses in workflow order (ctx.args.order) instead of alphabetically
export function byWorkflowOrder(a, b, rowA, rowB, ctx) {
  return ctx.args.order.indexOf(a) - ctx.args.order.indexOf(b);
}

// a progress bar with the percentage as its label
export function progressBar(value) {
  const bar = document.createElement('wa-progress-bar');
  bar.value = value;
  bar.textContent = `${value}%`;
  return bar;
}
