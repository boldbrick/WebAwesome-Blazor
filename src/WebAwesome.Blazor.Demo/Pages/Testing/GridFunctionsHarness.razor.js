// Functions of the grid functions harness (/testing/grid-functions). Blazor serves this collocated file at
// ./Pages/Testing/GridFunctionsHarness.razor.js, and the grids refer to its exports with WaJsFunction.Module. Each
// function gets the grid's arguments and a trailing ctx (ctx.args holds the WithArgs value, ctx.grid the grid,
// ctx.rowKey the key of the row of the call).

// the computed value of the total column: it drives sort, search and the CSV export
export function lineTotal(row) {
  return row.quantity * row.price;
}

// the footer of the total column: the sum of the filtered rows in a badge
export function totalFooter(rows) {
  const badge = document.createElement('wa-badge');
  badge.textContent = `Σ ${rows.reduce((sum, row) => sum + row.quantity * row.price, 0)}`;
  return badge;
}

// the class of a quantity cell at or below the limit in ctx.args
export function lowQuantityClass(value, row, ctx) {
  return value <= ctx.args.low ? 'gf-low' : '';
}

// the column filter: keeps the rows whose quantity is at least the filter value
export function atLeast(value, filterValue) {
  return Number(value) >= Number(filterValue);
}

// the class of a row whose total reaches the minimum in ctx.args
export function bigOrderRow(row, ctx) {
  return row.quantity * row.price >= ctx.args.minimum ? 'gf-big' : null;
}

// locks the rows marked as locked
export function unlockedRow(row) {
  return !row.locked;
}

// the global search without case and diacritics: "jose" finds "José"
export function foldedSearch(value, searchTerm) {
  const fold = text => String(text ?? '').normalize('NFD').replace(/\p{M}/gu, '').toLowerCase();
  return fold(value).includes(fold(searchTerm));
}

// the detail panel of an order line: a tag with the region, the line and the row key of ctx
export function orderDetail(row, ctx) {
  const panel = document.createElement('div');
  panel.className = 'gf-detail';
  const tag = document.createElement('wa-tag');
  tag.textContent = row.region;
  const text = document.createElement('span');
  text.textContent = ` ${row.quantity} × ${row.product} (key ${ctx.rowKey})`;
  panel.append(tag, text);
  return panel;
}

// a custom aggregation: the lowest and the highest value of the group
export function range(values) {
  return `${Math.min(...values)}–${Math.max(...values)}`;
}

// the child rows of a folder, by its row key, from ctx.args
export function childrenOf(row, ctx) {
  return ctx.args.children[ctx.rowKey];
}
