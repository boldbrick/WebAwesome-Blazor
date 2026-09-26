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

    /// <summary>
    /// Adds the handlers of the popup events, each only when its callback is set, in the order show, hide,
    /// after-show, after-hide: the wa-* events at sequence + 0..3, or, for an element that dispatches them as
    /// non-bubbling events, their relays (with Blazor's stopPropagation, see
    /// <see cref="RenderTreeBuilderExtensions.AddRelayedEventIfHasDelegate{T}"/>) at sequence + 0..7.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The popup control</param>
    /// <param name="relayed">Whether to bind the relayed events instead of the wa-* events</param>
    public static void AddPopupEventHandlers(RenderTreeBuilder builder, int sequence, IWaPopupControl control, bool relayed)
    {
        if (relayed)
        {
            builder.AddRelayedEventIfHasDelegate(sequence + 0, Constants.RelayedShowEventAttribute, control.OnShow);
            builder.AddRelayedEventIfHasDelegate(sequence + 2, Constants.RelayedHideEventAttribute, control.OnHide);
            builder.AddRelayedEventIfHasDelegate(sequence + 4, Constants.RelayedAfterShowEventAttribute, control.OnAfterShow);
            builder.AddRelayedEventIfHasDelegate(sequence + 6, Constants.RelayedAfterHideEventAttribute, control.OnAfterHide);
            return;
        }

        builder.AddAttributeIfHasDelegate(sequence + 0, ShowEventAttribute, control.OnShow);
        builder.AddAttributeIfHasDelegate(sequence + 1, HideEventAttribute, control.OnHide);
        builder.AddAttributeIfHasDelegate(sequence + 2, AfterShowEventAttribute, control.OnAfterShow);
        builder.AddAttributeIfHasDelegate(sequence + 3, AfterHideEventAttribute, control.OnAfterHide);
    }

    /// <summary>
    /// Adds the "with-clear" flag (present when true).
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant sequence number</param>
    /// <param name="control">The clearable control</param>
    public static void AddWithClearAttribute(RenderTreeBuilder builder, int sequence, IWaClearableControl control)
        => builder.AddAttribute(sequence, WithClearAttribute, control.WithClear);

    /// <summary>
    /// Adds the wa-clear handler when <see cref="IWaClearableControl.OnClear"/> is set.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant sequence number</param>
    /// <param name="control">The clearable control</param>
    public static void AddClearEventHandler(RenderTreeBuilder builder, int sequence, IWaClearableControl control)
        => builder.AddAttributeIfHasDelegate(sequence, ClearEventAttribute, control.OnClear);

    /// <summary>
    /// Adds <see cref="IWaClearableControl.ClearIconContent"/> to the "clear-icon" slot when set, at sequence + 0..2.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The clearable control</param>
    public static void AddClearIconSlot(RenderTreeBuilder builder, int sequence, IWaClearableControl control)
        => builder.AddSlotContent(sequence, ClearIconSlot, control.ClearIconContent);

    /// <summary>
    /// Adds the "start" and "end" slot content: each fragment when set (start at sequence + 0..2, end at
    /// sequence + 5..7), otherwise the icon of its icon-name shortcut when one is given (start at sequence + 40..42,
    /// end at sequence + 45..47), so the fragment wins.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    /// <param name="control">The affixed control</param>
    /// <param name="startIconName">The start icon-name shortcut, or null when the wrapper has none</param>
    /// <param name="endIconName">The end icon-name shortcut, or null when the wrapper has none</param>
    public static void AddAffixSlots(RenderTreeBuilder builder, int sequence, IWaAffixedControl control, string? startIconName = null,
        string? endIconName = null)
    {
        if (control.StartContent is not null)
            builder.AddSlotContent(sequence + 0, StartSlot, control.StartContent);
        else
            builder.AddIconSlot(sequence + AffixIconSequenceOffset, StartSlot, startIconName);

        if (control.EndContent is not null)
            builder.AddSlotContent(sequence + 5, EndSlot, control.EndContent);
        else
            builder.AddIconSlot(sequence + 5 + AffixIconSequenceOffset, EndSlot, endIconName);
    }

    #region ------ Internals ------

    private const string WithClearAttribute = "with-clear";
    private const string ClearEventAttribute = "onwa-clear";
    private const string ClearIconSlot = "clear-icon";
    private const string StartSlot = "start";
    private const string EndSlot = "end";

    // the icon of an affix shortcut is numbered apart from the slot's fragment wrapper
    private const int AffixIconSequenceOffset = 40;

    private const string ShowEventAttribute = "onwa-show";
    private const string HideEventAttribute = "onwa-hide";
    private const string AfterShowEventAttribute = "onwa-after-show";
    private const string AfterHideEventAttribute = "onwa-after-hide";

    // the label and hint attributes share their names with their slots
    private const string LabelSlot = "label";
    private const string HintSlot = "hint";
    private const string WithLabelAttribute = "with-label";
    private const string WithHintAttribute = "with-hint";

    #endregion
}
