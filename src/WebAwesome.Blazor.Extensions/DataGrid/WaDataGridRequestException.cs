using System;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Rejects a request of <c>ctx.invoke</c> with a message for the browser. Throw it from the
/// <see cref="WaDataGridRequestHandler.OnRequest"/> handler: the Promise of <c>ctx.invoke</c> rejects with an
/// <c>Error</c> whose message is <see cref="Exception.Message"/>, so the cell can show it.
/// </summary>
/// <remarks>
/// The message goes to the browser as it is: write it for the user, and do not put internal details into it. Any other
/// exception of the handler goes to the JS interop error handling of Blazor, which rejects the Promise with its own
/// message: the exception text on WebAssembly, and on Blazor Server a general text (the details only when detailed
/// errors are on).
/// </remarks>
public class WaDataGridRequestException : Exception
{
    #region ------ Constructors ------

    /// <summary>
    /// Creates the exception with a message for the browser.
    /// </summary>
    /// <param name="message">The message that the rejected Promise carries</param>
    public WaDataGridRequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with a message for the browser and the exception that caused it.
    /// </summary>
    /// <param name="message">The message that the rejected Promise carries</param>
    /// <param name="innerException">The cause; it does not go to the browser</param>
    public WaDataGridRequestException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    #endregion
}
