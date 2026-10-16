// Functions of the cell requests harness (/testing/cell-requests). Blazor serves this collocated file at
// ./Pages/Testing/CellRequestsHarness.razor.js, and the grid refers to its exports with WaJsFunction.Module. They use
// ctx.invoke of WebAwesome.Blazor.Extensions, which asks the WaDataGridRequestHandler around the grid.

// the answers by row key and the field the answer depends on: { state: 'pending' | 'done' | 'failed', text }. Not by the
// row object: WaDataGrid pushes Data on every parameter set (for example when the request handler renders the page),
// which gives new row objects with the same values.
const prices = new Map();

// the price cell: a pending state first, then the answer of .NET. A grid function is synchronous, so the cell renders
// again when the answer is there (ctx.grid.requestUpdate()). ctx.cache keeps the pending node, and the map keeps the
// request, so a render while the request runs does not send it again.
export function priceCell(value, row, ctx) {
  const key = `${ctx.rowKey}|${row.stock}`;
  let entry = prices.get(key);
  if (!entry) {
    entry = { state: 'pending', text: '' };
    prices.set(key, entry);
    ctx.invoke('price', { id: row.id }).then(
      price => { entry.state = 'done'; entry.text = `${price.amount} ${price.currency}`; },
      error => { entry.state = 'failed'; entry.text = `Error: ${error.message}`; })
      .then(() => ctx.grid.requestUpdate());
  }

  if (entry.state === 'pending') return ctx.cache('pending', () => ctx.h('span', { class: 'cr-pending' }, 'Loading…'));
  return entry.text;
}

// the answers of the detail panels (by row key) and of the footer. The grid builds the detail panel and the footer
// again on each render (also when the request handler renders the page, which pushes Data again), so the answers live
// here, and the grid renders again when one arrives.
const stockAnswers = new Map();
let totalAnswer = '';

// the detail panel: a button that asks .NET for the stock of the row and shows the answer
export function productDetail(row, ctx) {
  const key = ctx.rowKey;
  return ctx.h('div', { class: 'cr-detail' },
    ctx.h('button', {
      type: 'button',
      class: 'cr-detail-ask',
      onClick: () => ctx.invoke('stock').then(stock => { stockAnswers.set(key, `Stock: ${stock}`); ctx.grid.requestUpdate(); }),
    }, `Stock of ${row.name}`),
    ' ', ctx.h('span', { class: 'cr-detail-answer' }, stockAnswers.get(key) ?? ''));
}

// the footer: a button that asks .NET for the total stock of all rows
export function priceFooter(rows, ctx) {
  return ctx.h('span', { class: 'cr-footer' },
    ctx.h('button', {
      type: 'button',
      class: 'cr-footer-ask',
      onClick: () => ctx.invoke('total', { rows: rows.length }).then(total => { totalAnswer = `Total stock: ${total}`; ctx.grid.requestUpdate(); }),
    }, 'Total stock'),
    ' ', ctx.h('span', { class: 'cr-footer-answer' }, totalAnswer));
}