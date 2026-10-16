using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Answers the requests of <c>ctx.invoke(action, payload)</c> from the content of the data grids inside it: a cell
/// function, a detail panel or a footer asks .NET for a value and gets a Promise. Put the grid inside the component:
/// <code>
/// &lt;WaDataGridRequestHandler OnRequest="HandleRequestAsync"&gt;
///     &lt;WaDataGrid Data="orders" Columns="columns" RowKey="id" /&gt;
/// &lt;/WaDataGridRequestHandler&gt;
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// The component renders one <c>div</c> with <c>display: contents</c> (no layout box) around its content. After its
/// first render, the browser keeps a reference to the component for that element. <c>ctx.invoke</c> finds the handler
/// through the ancestors of the grid element when it is called, so a grid that the page renders again, or several grids,
/// use the same handler. When the reference is not registered yet (on the first cells), <c>ctx.invoke</c> waits for it up
/// to 2 seconds, then rejects.
/// </para>
/// <para>
/// The Promise resolves with the JSON of the value that <see cref="OnRequest"/> returns. It rejects with an <c>Error</c>
/// when the handler throws <see cref="WaDataGridRequestException"/> (with its message), when no handler is found, when
/// <see cref="OnRequest"/> is null, or when the request does not reach .NET (for example after a Blazor Server
/// disconnect). Each call is separate: concurrent calls get their own results.
/// </para>
/// <para>
/// A grid function is synchronous, so a cell that needs a value from .NET renders a pending state first and renders
/// again when the Promise resolves: keep the result per row, call <c>ctx.grid.requestUpdate()</c>, and use
/// <c>ctx.cache</c> for the pending node so that the request is not sent again on each render. See the package readme.
/// </para>
/// </remarks>
public class WaDataGridRequestHandler : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Answers one request of <c>ctx.invoke</c>. The argument holds the action, the payload, the column, the row key, the
    /// row and the value of the cell, as for a cell action (<c>GetRow&lt;T&gt;()</c>, <c>GetPayload&lt;T&gt;()</c>). The
    /// returned value must be JSON-serializable; null resolves the Promise with null. Throw
    /// <see cref="WaDataGridRequestException"/> to reject the Promise with a message.
    /// </summary>
    /// <remarks>
    /// Blazor Server runs the handler on the dispatcher of the circuit. When the handler changes the state of the page,
    /// call <c>StateHasChanged</c> of the page.
    /// </remarks>
    [Parameter] public Func<WaDataGridCellActionEventArgs, Task<object?>>? OnRequest { get; set; }

    /// <summary>
    /// The content: one or more <c>WaDataGrid</c> components and any other markup.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Unregisters the handler in the browser and releases the reference. A disconnected circuit is not an error.
    /// </summary>
    /// <returns>A task that completes when the handler is released</returns>
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;

        await UnregisterAsync();
        receiver?.Dispose();
    }

    #region ------ Internals ------

    // the module of the request handlers in the browser (served by the static web assets of the package)
    private const string ModulePath = "./_content/WebAwesome.Blazor.Extensions/data-grid-requests.js";
    private const string RegisterFunctionName = "registerRequestHandler";
    private const string UnregisterFunctionName = "unregisterRequestHandler";

    // the wrapper element takes no layout box: the grids inside it lay out as children of the parent
    private const string ScopeElementName = "div";
    private const string ScopeStyle = "display: contents";

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private ElementReference scope;
    private DotNetObjectReference<CellRequestReceiver>? receiver;
    private IJSObjectReference? module;
    private bool registered;
    private bool disposed;

    private async Task RegisterAsync()
    {
        receiver = DotNetObjectReference.Create(new CellRequestReceiver(() => OnRequest));
        try
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            if (disposed) return;

            await module.InvokeVoidAsync(RegisterFunctionName, scope, receiver);
            registered = true;
        }
        catch (JSDisconnectedException)
        {
            // the circuit is gone: nothing can call the handler any more
        }
        catch (TaskCanceledException)
        {
            // the circuit ended while the module loaded
        }

        // disposed while the module loaded: release what the registration created
        if (disposed) await UnregisterAsync();
    }

    private async Task UnregisterAsync()
    {
        var current = module;
        module = null;
        if (current is null) return;

        try
        {
            if (registered && receiver is not null) await current.InvokeVoidAsync(UnregisterFunctionName, scope, receiver);
            registered = false;
            await current.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // the circuit is gone, and with it the registration in the browser
        }
        catch (TaskCanceledException)
        {
            // the circuit ended during the call
        }
    }

    #endregion

    #region ------ Interface for descendants ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, ScopeElementName);
        builder.AddAttribute(1, "style", ScopeStyle);
        builder.AddElementReferenceCapture(2, reference => scope = reference);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !disposed) await RegisterAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion
}
