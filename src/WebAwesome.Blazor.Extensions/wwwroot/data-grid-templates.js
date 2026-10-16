/**
 * WebAwesome.Blazor.Extensions - the HTML of static Razor cell templates
 *
 * WaDataGridRazorTemplates<TRow> renders a Razor template for each row and cell in .NET (HtmlRenderer) and sends the
 * HTML with the row. The preset razorTemplate (data-grid-presets.js) shows it in the cell with templateNode below.
 * Design: tasks/WA-3.14/WAB-49/datagrid-cell-rendering-design.md, section 10.
 *
 * This module holds the only HTML parse of the package. The HTML comes from the Razor renderer of the app, which encodes
 * text and attribute values, so row data cannot become markup (a MarkupString with user content can: the documentation
 * of WaDataGridRazorTemplates says so). The parse uses a <template> element: its content is an inert document, so no
 * script runs and no custom element upgrades while it is parsed, and a clone of a parsed script never runs.
 */

// the most parsed templates that the cache keeps; then it starts again, so HTML from row data cannot grow it without limit
const maxTemplates = 1000;

// the wrapper of the cloned content takes no layout box
const wrapperStyle = 'display: contents;';

// HTML text -> its parsed <template>
const parsedTemplates = new Map();

function parsedTemplate(html) {
    let template = parsedTemplates.get(html);
    if (!template) {
        if (parsedTemplates.size >= maxTemplates) parsedTemplates.clear();

        template = document.createElement('template');
        template.innerHTML = html;
        parsedTemplates.set(html, template);
    }

    return template;
}

/**
 * A new node with a copy of the content of an HTML text: a span with display: contents that holds a copy of the
 * parsed template, imported into the page document. Each HTML text is parsed once.
 * @param {string} html - The HTML of the template, from WaDataGridRazorTemplates in .NET
 * @returns {HTMLSpanElement} The node for the cell
 */
export function templateNode(html) {
    const node = document.createElement('span');
    node.setAttribute('style', wrapperStyle);
    node.append(document.importNode(parsedTemplate(html).content, true));
    return node;
}
