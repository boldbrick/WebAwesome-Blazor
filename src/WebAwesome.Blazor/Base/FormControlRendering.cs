using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Shared render code of the form control capability clusters (<see cref="IWaLabeledControl"/>, …), so each
/// wrapper stops duplicating it. Each helper emits one cluster's frames in a fixed order at constant sequence
/// numbers relative to the base it is given; the wrapper calls it where its element's attribute order puts the
/// cluster.
/// </summary>
internal static class FormControlRendering
{
    /// <summary>
    /// Adds the "label" attribute at sequence + 0 and the "hint" attribute at sequence + 1, each only when set.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The labeled control</param>
    public static void AddLabelAndHintAttributes(RenderTreeBuilder builder, int sequence, IWaLabeledControl control)
    {
        builder.AddAttributeIfNotNullOrEmpty(sequence + 0, LabelSlot, control.Label);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 1, HintSlot, control.Hint);
    }

    /// <summary>
    /// Adds the "with-hint" flag at sequence + 0 and the "with-label" flag at sequence + 1 (present when true).
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The labeled control</param>
    public static void AddWithHintAndLabelAttributes(RenderTreeBuilder builder, int sequence, IWaLabeledControl control)
    {
        builder.AddAttribute(sequence + 0, WithHintAttribute, control.WithHint);
        builder.AddAttribute(sequence + 1, WithLabelAttribute, control.WithLabel);
    }

    /// <summary>
    /// Adds the markup label and hint to the element's "label" and "hint" slots, each only when set, at
    /// sequence + 0..5.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="markupLabel">Content of the "label" slot, or null</param>
    /// <param name="markupHint">Content of the "hint" slot, or null</param>
    /// <returns>The next available sequence number (sequence + 6)</returns>
    public static int AddLabelAndHintSlots(RenderTreeBuilder builder, int sequence, RenderFragment? markupLabel, RenderFragment? markupHint)
    {
        builder.AddSlotContent(sequence + 0, LabelSlot, markupLabel);
        builder.AddSlotContent(sequence + 3, HintSlot, markupHint);

        return sequence + 6;
    }

    /// <summary>
    /// Adds the control's markup label and hint to the element's "label" and "hint" slots, each only when set, at
    /// sequence + 0..5.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The labeled control</param>
    /// <returns>The next available sequence number (sequence + 6)</returns>
    public static int AddLabelAndHintSlots(RenderTreeBuilder builder, int sequence, IWaLabeledControl control)
        => AddLabelAndHintSlots(builder, sequence, control.MarkupLabel, control.MarkupHint);

    #region ------ Internals ------

    // the label and hint attributes share their names with their slots
    private const string LabelSlot = "label";
    private const string HintSlot = "hint";
    private const string WithLabelAttribute = "with-label";
    private const string WithHintAttribute = "with-hint";

    #endregion
}
