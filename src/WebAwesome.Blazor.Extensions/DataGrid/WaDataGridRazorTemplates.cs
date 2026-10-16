using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Static Razor cell templates for <c>WaDataGrid</c>: write the content of a cell as Razor markup, for example a
/// <c>WaBadge</c>, and show it in a column. .NET renders each template to HTML for each row, the HTML travels with the row
/// in a separate property, and the formatter of the column clones it into the cell.
/// <code>
/// private static readonly WaDataGridRazorTemplates&lt;Order&gt; templates = new WaDataGridRazorTemplates&lt;Order&gt;()
///     .Add("state", order =&gt; @&lt;WaBadge Variant="@VariantOf(order.State)"&gt;@order.State&lt;/WaBadge&gt;);
///
/// private readonly WaDataGridColumn[] columns =
/// [
///     new() { Field = "state", Label = "State", Formatter = templates.Formatter("state", "wa-badge") },
/// ];
///
/// // @inject IServiceProvider Services
/// rows = await templates.RenderAsync(Services, orders);   // the Data of the grid
/// </code>
/// </summary>
/// <typeparam name="TRow">The row type of the app</typeparam>
/// <remarks>
/// <para>
/// The HTML is static. Interactivity works only through <c>data-wablazor-action</c> (and
/// <c>data-wablazor-action-payload</c>), which raises a cell action (<c>@onwablazor-cell-action</c>). <c>@onclick</c>
/// and other event handlers do not work, and a wrapper that needs JS interop after its render (for example a method
/// call or a property that the wrapper pushes) does not get it: only its rendered markup and attributes arrive.
/// </para>
/// <para>
/// Security: Razor encodes text and attribute values, so row data cannot become markup. A <see cref="MarkupString"/>
/// with user content is not safe: it goes into the cell as HTML.
/// </para>
/// <para>
/// Cost: <see cref="RenderAsync"/> renders each template once per row on each call, on the server (Blazor Server) or in
/// the browser (WebAssembly), and the HTML adds to the payload of each row. Use it for server mode and small pages (tens
/// of rows), not for thousands of rows; the presets of <see cref="WaDataGridFormatters"/> cost nothing per row in .NET.
/// </para>
/// <para>
/// Elements in the HTML render in the shadow root of the grid, where the autoloader does not look: declare the
/// <c>wa-*</c> elements of a template in <see cref="Formatter"/>. Sort, filter, search and the CSV export use the row
/// values; the template changes only what the cell shows.
/// </para>
/// </remarks>
public sealed class WaDataGridRazorTemplates<TRow>
{
    /// <summary>
    /// The keys of the templates, in the order of <see cref="Add"/>.
    /// </summary>
    public IReadOnlyCollection<string> Keys => templates.Select(entry => entry.Key).ToArray();

    /// <summary>
    /// Adds a template. Configure the templates once (for example in a static field), before the first
    /// <see cref="RenderAsync"/>.
    /// </summary>
    /// <param name="key">The key of the template; <see cref="Formatter"/> refers to it</param>
    /// <param name="template">The Razor template of the cell, for a row</param>
    /// <returns>This instance</returns>
    /// <exception cref="ArgumentException">Thrown when the key is empty, has leading or trailing whitespace, or is
    /// already used</exception>
    /// <exception cref="ArgumentNullException">Thrown when the template is null</exception>
    public WaDataGridRazorTemplates<TRow> Add(string key, RenderFragment<TRow> template)
    {
        ValidateKey(key, nameof(key));
        ArgumentNullException.ThrowIfNull(template);
        if (templates.Any(entry => entry.Key == key))
            throw new ArgumentException($"The template key '{key}' is already used.", nameof(key));

        templates.Add(new KeyValuePair<string, RenderFragment<TRow>>(key, template));
        return this;
    }

    /// <summary>
    /// The formatter of a column that shows a template: a reference to the registered preset function
    /// <c>wab.dataGrid.razorTemplate</c>, which clones the HTML of the template for the row of each cell. A row without
    /// the HTML (data that <see cref="RenderAsync"/> did not render) shows an empty cell.
    /// </summary>
    /// <param name="key">The key of the template</param>
    /// <param name="uses">The <c>wa-*</c> elements that the template renders, so that they are registered before the
    /// cells render, for example <c>"wa-badge"</c></param>
    /// <returns>The function reference for <c>WaDataGridColumn.Formatter</c></returns>
    /// <exception cref="ArgumentException">Thrown when no template has the key, or a tag is not a <c>wa-*</c>
    /// name</exception>
    public WaJsFunction Formatter(string key, params string[] uses)
    {
        ValidateKey(key, nameof(key));
        if (templates.All(entry => entry.Key != key))
            throw new ArgumentException($"No template has the key '{key}'. Add it first.", nameof(key));

        return new PresetArgs().Set(KeyOption, key).ToFunction(PresetNames.RazorTemplate, uses ?? []);
    }

    /// <summary>
    /// Renders every template for every row and returns the rows for the <c>Data</c> of the grid: each row as it
    /// serializes, plus the HTML of its templates in a separate property. Call it again when the rows change.
    /// </summary>
    /// <param name="services">The services of the page (<c>@inject IServiceProvider Services</c>); the components of
    /// the templates get their services from it</param>
    /// <param name="rows">The rows of the app</param>
    /// <param name="cancellationToken">Stops the render between two rows</param>
    /// <returns>The rows for <c>WaDataGrid.Data</c>, in the given order</returns>
    /// <exception cref="ArgumentNullException">Thrown when the services or the rows are null</exception>
    /// <exception cref="ArgumentException">Thrown when a row is null</exception>
    /// <remarks>
    /// Each call creates one <see cref="HtmlRenderer"/> from <paramref name="services"/>, renders on its dispatcher and
    /// disposes it. A long-lived renderer would keep every component that it rendered until it is disposed.
    /// </remarks>
    public async Task<IReadOnlyList<object>> RenderAsync(IServiceProvider services, IEnumerable<TRow> rows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(rows);

        var items = rows.ToArray();
        for (var index = 0; index < items.Length; index++)
        {
            if (items[index] is null) throw new ArgumentException($"The row at index {index} is null.", nameof(rows));
        }

        var entries = templates.ToArray();
        var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        await using var renderer = new HtmlRenderer(services, loggerFactory);
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = new object[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = items[index]!;
                var html = new Dictionary<string, string>(entries.Length, StringComparer.Ordinal);
                foreach (var (key, template) in entries)
                {
                    var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(RazorTemplateHost.Content)] = template(row) });
                    var output = await renderer.RenderComponentAsync<RazorTemplateHost>(parameters);
                    html[key] = output.ToHtmlString();
                }

                result[index] = new RazorTemplateRow(row, html);
            }

            return (IReadOnlyList<object>)result;
        });
    }

    #region ------ Internals ------

    // the option of the preset function that names the template (data-grid-presets.js, razorTemplate)
    private const string KeyOption = "key";

    private readonly List<KeyValuePair<string, RenderFragment<TRow>>> templates = [];

    private static void ValidateKey(string key, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Trim() != key)
            throw new ArgumentException("A template key must be a non-empty text without leading or trailing whitespace.", parameterName);
    }

    #endregion
}
