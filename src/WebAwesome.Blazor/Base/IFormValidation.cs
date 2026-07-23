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

    /// <summary>
    /// Clears any manually defined custom error and resets native constraint validation on the control.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <remarks>
    /// This maps to the Web Awesome form control's resetValidity method (introduced in Web Awesome 3.3.0)
    /// via JavaScript interop; it is the counterpart to <see cref="SetCustomValidityAsync"/> for removing
    /// custom errors without re-submitting the form.
    /// </remarks>
    Task ResetValidityAsync();
}