using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// A form control whose element has the full label and hint cluster: the "label" and "hint" attributes and slots
/// and the "with-label" and "with-hint" flags. Groups the members for consumers and tests; the [Parameter]
/// properties are declared by the implementing class (<see cref="WaLabeledInputBase{TValue}"/> for the
/// InputBase controls), and one shared renderer emits them for every implementation.
/// </summary>
public interface IWaLabeledControl
{
    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute; <see cref="MarkupLabel"/> takes precedence
    /// when set.
    /// </summary>
    string? Label { get; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute; <see cref="MarkupHint"/> takes precedence when
    /// set.
    /// </summary>
    string? Hint { get; }

    /// <summary>
    /// Rich markup label rendered into the element's "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    RenderFragment? MarkupLabel { get; }

    /// <summary>
    /// Rich markup hint rendered into the element's "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    RenderFragment? MarkupHint { get; }

    /// <summary>
    /// Reserves space for the label on the element's first render, before it sees slotted content; only needed
    /// for server-side rendering when slotting in a label through <see cref="MarkupLabel"/>.
    /// </summary>
    bool WithLabel { get; }

    /// <summary>
    /// Reserves space for the hint on the element's first render, before it sees slotted content; only needed
    /// for server-side rendering when slotting in a hint through <see cref="MarkupHint"/>.
    /// </summary>
    bool WithHint { get; }
}
