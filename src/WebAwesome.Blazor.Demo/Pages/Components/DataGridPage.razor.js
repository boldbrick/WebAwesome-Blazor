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

// Functions of the "More JavaScript Members" example: a class per cell, computed values, footers and a detail panel.

// the class of an hours cell over the budget in ctx.args; ShadowStyles styles it
export function overBudget(value, row, ctx) {
  return value > ctx.args.budget ? 'over-budget' : '';
}

// the computed amount of a line: it sorts, searches and exports like a field
export function amount(row) {
  return row.hours * row.rate;
}

// the footers: the sums of the filtered rows
export function sumOfHours(rows) {
  return String(rows.reduce((sum, row) => sum + row.hours, 0));
}

export function sumOfAmounts(rows) {
  return String(rows.reduce((sum, row) => sum + amount(row), 0));
}

// the detail panel of an expanded row
export function billingDetail(row) {
  const detail = document.createElement('p');
  detail.textContent = `${row.item}: ${row.hours} h at ${row.rate} per hour`;
  return detail;
}
