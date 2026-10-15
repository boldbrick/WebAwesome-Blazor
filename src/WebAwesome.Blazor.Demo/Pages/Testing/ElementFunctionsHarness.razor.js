// Functions of the element functions harness (/testing/element-functions). Blazor serves this collocated file at
// ./Pages/Testing/ElementFunctionsHarness.razor.js, and the wrappers refer to its exports with WaJsFunction.Module.
// Each function gets the element's arguments and a trailing ctx (ctx.args holds the WithArgs value, ctx.element the
// element). The texts show what each function received, so the spec can read it.

// a select or combobox tag: the prefix from ctx.args, the option label, the tag index and the element of ctx
export function markedTag(option, index, ctx) {
  const tag = document.createElement('span');
  tag.className = 'ef-tag';
  tag.textContent = `${ctx.args.prefix}${option.label}#${index}|${ctx.element.localName}`;
  return tag;
}

// a rating symbol: a filled or an empty dot (trusted HTML, as wa-rating expects a string)
export function dotSymbol(value, isSelected, ctx) {
  return `<span class="ef-symbol" data-value="${value}">${isSelected ? ctx.args.on : ctx.args.off}</span>`;
}

// a slider value with the unit from ctx.args
export function withUnit(value, ctx) {
  return `${value}${ctx.args.unit}`;
}

// the URL of a pagination page
export function pageHref(page, ctx) {
  return `${ctx.args.base}?p=${page}`;
}

// a combobox filter: the options whose label starts with the typed text
export function startsWith(option, query) {
  return option.label.toLowerCase().startsWith(query.toLowerCase());
}

// a calendar rule: Saturdays and Sundays are disabled
export function weekend(date) {
  return date.getDay() === 0 || date.getDay() === 6;
}

// calendar content: a mark on the 15th of each month (trusted HTML), the day number otherwise
export function markFifteenth(date, ctx) {
  return date.getDate() === 15 ? `<span class="ef-day">${ctx.args.mark}</span>` : null;
}

// a data grid cell with a class, which the grid's ShadowStyles colors
export function classedCell(value) {
  const cell = document.createElement('span');
  cell.className = 'ef-cell';
  cell.textContent = value;
  return cell;
}
