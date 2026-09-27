using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// A form control with a clear button: the "with-clear" attribute, the "clear-icon" slot and the wa-clear event.
/// Groups the members for consumers and tests; the [Parameter] properties are declared by the implementing wrapper,
/// and one shared renderer emits them.
/// </summary>
public interface IWaClearableControl
{
    /// <summary>
    /// Adds a clear button when the control is not empty.
    /// </summary>
    bool WithClear { get; }

    /// <summary>
    /// An icon to use in lieu of the default clear icon, rendered into the element's "clear-icon" slot.
    /// </summary>
    RenderFragment? ClearIconContent { get; }

    /// <summary>
    /// Invoked when the clear button is activated and the value is cleared.
    /// </summary>
    EventCallback OnClear { get; }
}
