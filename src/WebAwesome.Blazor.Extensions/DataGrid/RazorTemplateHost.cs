using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// The root component that <see cref="WaDataGridRazorTemplates{TRow}"/> renders with the <c>HtmlRenderer</c>: it renders
/// the template of one cell and nothing else.
/// </summary>
internal sealed class RazorTemplateHost : ComponentBase
{
    /// <summary>
    /// The template of the cell, applied to its row.
    /// </summary>
    [Parameter] public RenderFragment? Content { get; set; }

    #region ------ Internals ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, Content);
    }

    #endregion
}
