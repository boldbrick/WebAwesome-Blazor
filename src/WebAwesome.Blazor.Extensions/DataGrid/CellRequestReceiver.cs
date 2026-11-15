using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// The .NET target of the requests of <c>ctx.invoke</c> (<c>wwwroot\data-grid-requests.js</c>). The browser keeps a
/// <see cref="DotNetObjectReference{TValue}"/> of it per <see cref="WaDataGridRequestHandler"/> element and calls
/// <see cref="InvokeCellRequestAsync"/> for each request.
/// </summary>
internal sealed class CellRequestReceiver
{
    /// <summary>
    /// The name of the invokable method; <c>invokeMethodName</c> in <c>data-grid-requests.js</c>.
    /// </summary>
    public const string InvokeMethodName = "InvokeCellRequestAsync";

    /// <summary>
    /// Runs the handler for one request.
    /// </summary>
    /// <param name="request">The request: the cell, the action and the payload of <c>ctx.invoke</c></param>
    /// <returns>The response: the value of the handler, or the message of a rejection</returns>
    /// <remarks>
    /// An exception other than <see cref="WaDataGridRequestException"/> is not caught: the JS interop of Blazor rejects
    /// the call with its own message (on Blazor Server without the details, unless detailed errors are on).
    /// </remarks>
    [JSInvokable(InvokeMethodName)]
    public async Task<CellRequestResponse> InvokeCellRequestAsync(WaDataGridCellActionEventArgs request)
    {
        var handler = handlerAccessor();
        if (handler is null)
            return CellRequestResponse.Failure(string.Format(NoHandlerMessageFormat, request?.Action));

        try
        {
            return CellRequestResponse.Success(await handler(request ?? new WaDataGridCellActionEventArgs()));
        }
        catch (WaDataGridRequestException exception)
        {
            return CellRequestResponse.Failure(exception.Message);
        }
    }

    #region ------ Constructors ------

    /// <summary>
    /// Creates the receiver.
    /// </summary>
    /// <param name="handlerAccessor">Returns the current handler of the component, or null when it has none</param>
    public CellRequestReceiver(Func<Func<WaDataGridCellActionEventArgs, Task<object?>>?> handlerAccessor)
    {
        this.handlerAccessor = handlerAccessor ?? throw new ArgumentNullException(nameof(handlerAccessor));
    }

    #endregion

    #region ------ Internals ------

    private const string NoHandlerMessageFormat = "WebAwesome.Blazor.Extensions: the request '{0}' reached a WaDataGridRequestHandler without OnRequest.";

    private readonly Func<Func<WaDataGridCellActionEventArgs, Task<object?>>?> handlerAccessor;

    #endregion
}

/// <summary>
/// The response to one request of <c>ctx.invoke</c>: the browser resolves the Promise with <see cref="Value"/>, or rejects
/// it with <see cref="Error"/> when that is not null.
/// </summary>
/// <param name="Value">The JSON-serializable value of the handler</param>
/// <param name="Error">The message of a rejection, or null</param>
internal sealed record CellRequestResponse(object? Value, string? Error)
{
    /// <summary>
    /// A response that resolves the Promise.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The response</returns>
    public static CellRequestResponse Success(object? value) => new(value, null);

    /// <summary>
    /// A response that rejects the Promise.
    /// </summary>
    /// <param name="message">The message of the rejection</param>
    /// <returns>The response</returns>
    public static CellRequestResponse Failure(string message) => new(null, message);
}
