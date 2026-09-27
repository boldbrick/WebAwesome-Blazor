using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class for the form controls whose element has the full label and hint cluster (the "label" and "hint"
/// attributes and slots, and the "with-label" and "with-hint" flags): declares those parameters once and renders
/// them through the shared cluster renderer.
/// </summary>
/// <remarks>
/// Derive a form control wrapper from this class only when its element declares every member of the cluster in
/// the CEM; a control with a partial subset (e.g. wa-checkbox, whose label is its default slot) derives from
/// <see cref="WaInputBase{TValue}"/> and declares its subset itself.
/// </remarks>
/// <typeparam name="TValue">The type of value bound to the input</typeparam>
public abstract class WaLabeledInputBase<TValue> : WaInputBase<TValue>, IWaLabeledControl
{
    /// <inheritdoc />
    [Parameter] public string? Label { get; set; }

    /// <inheritdoc />
    [Parameter] public string? Hint { get; set; }

    /// <inheritdoc />
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <inheritdoc />
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    /// <inheritdoc />
    [Parameter] public bool WithLabel { get; set; }

    /// <inheritdoc />
    [Parameter] public bool WithHint { get; set; }

    #region ------ Interface for descendants ------

    /// <summary>
    /// Adds the "label" and "hint" attributes, each only when set, at sequence + 0..1. Call it with sequence 12,
    /// the place <see cref="WaInputBase{TValue}.AddCommonAttributes"/> reserves for them, after the other
    /// element-specific form control attributes.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    protected void AddLabelAndHintAttributes(RenderTreeBuilder builder, int sequence)
        => FormControlRendering.AddLabelAndHintAttributes(builder, sequence, this);

    /// <summary>
    /// Adds the "with-hint" and "with-label" flags at sequence + 0..1, in that order. Call it with sequence 14, the
    /// place reserved for them, where the element's attribute order puts them.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    protected void AddWithHintAndLabelAttributes(RenderTreeBuilder builder, int sequence)
        => FormControlRendering.AddWithHintAndLabelAttributes(builder, sequence, this);

    /// <summary>
    /// Adds <see cref="MarkupLabel"/> and <see cref="MarkupHint"/> to the element's "label" and "hint" slots, each
    /// only when set, at sequence + 0..5.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <returns>The next available sequence number (sequence + 6)</returns>
    protected int AddLabelAndHintSlots(RenderTreeBuilder builder, int sequence)
        => FormControlRendering.AddLabelAndHintSlots(builder, sequence, this);

    #endregion
}
