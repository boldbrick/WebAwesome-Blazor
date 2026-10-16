// The cell function of the Lit cells harness (/testing/lit-cells): it returns a TemplateResult of the app's own copy of
// Lit 3 (from the CDN), not of the copy inside Web Awesome. The grid renders it (risk R2 of the design). A @click
// binding calls ctx.emit, and a declarative button raises its action through data-wablazor-action.
import { html } from 'https://cdn.jsdelivr.net/npm/lit@3.3.1/+esm';

export function litCell(value, row, ctx) {
  return html`<span class="lit-cell"><strong>${value}</strong>
    <button type="button" @click=${event => ctx.emit('lit-click', { id: row.id }, event)}>Lit click</button>
    <button type="button" data-wablazor-action="lit-declarative">Declarative</button></span>`;
}
