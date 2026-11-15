// Cell functions of the JavaScript functions harness (/testing/js-functions), referred to with WaJsFunction.Module.

// sorts by the position of the value in ctx.args.order
export function byListOrder(a, b, rowA, rowB, ctx) {
  return ctx.args.order.indexOf(a) - ctx.args.order.indexOf(b);
}

// the aggregate of a group row, with the number of the group's data rows
export function groupTotal(value, rows) {
  return `total ${value} in ${rows.length} rows`;
}

// a read-only rating; the column declares no Uses, so the grid registers wa-rating after the render
export function ratingCell(value, row, ctx) {
  const rating = document.createElement('wa-rating');
  rating.readonly = true;
  rating.value = value;
  rating.label = `Score of row ${ctx.rowKey}`;
  return rating;
}
