using System.Threading.Tasks;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Interface for form controls that support custom validation via setCustomValidity()
/// </summary>
public interface IFormValidation
{
    /// <summary>
    /// Sets a custom validation message. This will prevent the form from submitting
    /// and make the browser display the error message you provide.
    /// To clear the error, call this function with an empty string.
    /// </summary>
    /// <param name="message">The validation message to display, or empty string to clear</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <remarks>
    /// This method uses JavaScript interop to call the underlying web component's
    /// setCustomValidity method. Implementation depends on the Web Awesome library
    /// being properly loaded in the page.
    /// </remarks>
    Task SetCustomValidityAsync(string message);
}