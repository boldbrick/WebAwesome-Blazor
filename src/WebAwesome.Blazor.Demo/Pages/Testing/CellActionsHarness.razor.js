// Functions of the cell actions harness (/testing/cell-actions). Blazor serves this collocated file at
// ./Pages/Testing/CellActionsHarness.razor.js, and the grids refer to its exports with WaJsFunction.Module. They use the
// ctx helpers of WebAwesome.Blazor.Extensions (ctx.h, ctx.emit, ctx.preset, ctx.format, ctx.cache) and the declarative
// action attributes (data-wablazor-action, data-wablazor-action-payload).

// a cell built with the helpers: a badge preset, the amount from ctx.format, a button with an action prop, a declarative
// button, and a button that calls ctx.emit; ctx.cache keeps the node while the state and the amount stay the same
export function ticketCell(value, row, ctx) {
  return ctx.cache(`${row.state}|${row.amount}`, () => ctx.h('span', { class: 'ca-custom', style: 'display: inline-flex; gap: 4px; align-items: center;' },
    ctx.preset('badge', { variants: { open: 'brand', closed: 'neutral' }, labels: { open: 'Open', closed: 'Closed' } }),
    ctx.h('span', { class: 'ca-amount' }, ctx.format.currency(row.amount, { currency: 'EUR', culture: 'en-US' })),
    ctx.h('button', { type: 'button', action: 'approve', payload: { level: 2 } }, 'Approve'),
    ctx.h('button', { type: 'button', 'data-wablazor-action': 'reject', 'data-wablazor-action-payload': JSON.stringify({ reason: 'test' }) }, 'Reject'),
    ctx.h('button', { type: 'button', onClick: event => ctx.emit('custom', { via: 'emit' }, event) }, 'Emit')));
}

// the detail panel: a button with an action prop (ctx.emit knows the row) and a declarative button, which has no
// wa-cell-click to pair with and is dropped with a warning
export function ticketDetail(row, ctx) {
  return ctx.h('div', { class: 'ca-detail' },
    ctx.h('span', {}, `Detail of ${row.title} `),
    ctx.h('button', { type: 'button', action: 'detail-open' }, 'Open from detail'),
    ctx.h('button', { type: 'button', 'data-wablazor-action': 'detail-declarative' }, 'Declarative in detail'));
}

// the footer of the amount column: the localized sum and a button whose action has no row
export function amountFooter(rows, ctx) {
  return ctx.h('span', { class: 'ca-footer' },
    ctx.format.number(rows.reduce((sum, row) => sum + row.amount, 0), { culture: 'en-US', minFractionDigits: 2 }), ' ',
    ctx.h('button', { type: 'button', action: 'recalculate', payload: { rows: rows.length } }, 'Recalculate'));
}
