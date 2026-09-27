using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// A form control with "start" and "end" slots for content placed before and after the value (an icon, a unit, a
/// button). Groups the members for consumers and tests; the [Parameter] properties are declared by the implementing
/// wrapper, and one shared renderer emits them, with the wrapper's icon-name shortcuts where it has them.
/// </summary>
public interface IWaAffixedControl
{
    /// <summary>
    /// Content to display at the start of the control, rendered into the element's "start" slot.
    /// </summary>
    RenderFragment? StartContent { get; }

    /// <summary>
    /// Content to display at the end of the control, rendered into the element's "end" slot.
    /// </summary>
    RenderFragment? EndContent { get; }
}
