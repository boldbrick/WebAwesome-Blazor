namespace WebAwesome.Blazor.Base;

/// <summary>
/// Shared constant values used across the WebAwesome.Blazor library.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Render-tree attribute name of the "numericchange" event, an alias of the browser "change" event registered by
    /// the JS initializer (WebAwesome.Blazor.lib.module.js) for elements whose live value is a JS number (wa-slider,
    /// wa-rating). Blazor's built-in change reader cannot carry a number, so these wrappers bind the alias, whose
    /// payload carries the value as a string; being a custom event type, its
    /// <see cref="Microsoft.AspNetCore.Components.ChangeEventArgs.Value"/> arrives as a JSON element, read it with
    /// <see cref="ChangeEventArgsExtensions.GetStringValue"/>.
    /// </summary>
    internal const string NumericChangeEventAttribute = "onnumericchange";

    /// <summary>
    /// Render-tree attribute name of the "numericinput" event, the "input" counterpart of
    /// <see cref="NumericChangeEventAttribute"/>.
    /// </summary>
    internal const string NumericInputEventAttribute = "onnumericinput";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-show" event, the relay of wa-color-picker's wa-show. The JS
    /// initializer (WebAwesome.Blazor.lib.module.js, relayedEvents) re-dispatches an event Blazor cannot receive
    /// where Web Awesome dispatches it (non-bubbling, or with its propagation stopped in the shadow root) as a
    /// bubbling, composed event under this private name on the host, with the original event's payload. Bind it
    /// with <see cref="RenderTreeBuilderExtensions.AddRelayedEventIfHasDelegate{T}"/>, which adds Blazor's
    /// stopPropagation, so the relayed event reaches no other wrapper of the same element further up the tree.
    /// </summary>
    internal const string RelayedShowEventAttribute = "onwablazor-show";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-after-show" event, the relay of wa-color-picker's
    /// wa-after-show; see <see cref="RelayedShowEventAttribute"/>.
    /// </summary>
    internal const string RelayedAfterShowEventAttribute = "onwablazor-after-show";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-hide" event, the relay of wa-color-picker's wa-hide; see
    /// <see cref="RelayedShowEventAttribute"/>.
    /// </summary>
    internal const string RelayedHideEventAttribute = "onwablazor-hide";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-after-hide" event, the relay of wa-color-picker's
    /// wa-after-hide; see <see cref="RelayedShowEventAttribute"/>.
    /// </summary>
    internal const string RelayedAfterHideEventAttribute = "onwablazor-after-hide";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-intersect" event, the relay of wa-intersection-observer's
    /// non-bubbling wa-intersect; see <see cref="RelayedShowEventAttribute"/>.
    /// </summary>
    internal const string RelayedIntersectEventAttribute = "onwablazor-intersect";

    /// <summary>
    /// Render-tree attribute name of the "wablazor-keydown" event, the relay of the keydown of wa-select,
    /// wa-combobox and wa-color-picker, which stop its propagation in their shadow root; its payload is a
    /// <see cref="Microsoft.AspNetCore.Components.Web.KeyboardEventArgs"/>. See <see cref="RelayedShowEventAttribute"/>.
    /// </summary>
    internal const string RelayedKeyDownEventAttribute = "onwablazor-keydown";

    /// <summary>
    /// Attribute value read as true by Web Awesome's "true"/"false" attribute converters (e.g. spellcheck).
    /// </summary>
    internal const string TrueAttributeValue = "true";

    /// <summary>
    /// Attribute value read as false by Web Awesome's "true"/"false" attribute converters (e.g. spellcheck);
    /// the comparison is case-sensitive, so "False" would read as true.
    /// </summary>
    internal const string FalseAttributeValue = "false";

    /// <summary>
    /// Attribute value read as true by Web Awesome's "on"/"off" attribute converters (autocorrect).
    /// </summary>
    internal const string OnAttributeValue = "on";

    /// <summary>
    /// Attribute value read as false by Web Awesome's "on"/"off" attribute converters (autocorrect).
    /// </summary>
    internal const string OffAttributeValue = "off";

    /// <summary>
    /// Inline style of the element a wrapper puts around a fragment to assign it to a slot whose layout treats every
    /// slotted element as an item (e.g. wa-slider's "reference" slot, a flex row spreading the labels): the wrapper
    /// then generates no box, so the fragment's own elements are laid out as the slot's items.
    /// </summary>
    internal const string TransparentSlotWrapperStyle = "display: contents";

    /// <summary>
    /// The selection-mode attribute of wa-date-input and wa-date-picker; the range wrappers render it as
    /// <see cref="RangeModeValue"/>, the single-date wrappers leave it unset (the element default, single).
    /// </summary>
    internal const string ModeAttribute = "mode";

    /// <summary>
    /// The <see cref="ModeAttribute"/> value of the range wrappers.
    /// </summary>
    internal const string RangeModeValue = "range";

    /// <summary>
    /// The minimum range length attribute of wa-date-input and wa-date-picker (range mode only).
    /// </summary>
    internal const string MinRangeAttribute = "min-range";

    /// <summary>
    /// The maximum range length attribute of wa-date-input and wa-date-picker (range mode only).
    /// </summary>
    internal const string MaxRangeAttribute = "max-range";

    /// <summary>
    /// Validation message of a date control whose element value is not an ISO date; {0} is the field's display name.
    /// Worded like Blazor's own InputDate.
    /// </summary>
    internal const string DateValidationMessageFormat = "The {0} field must be a date.";

    /// <summary>
    /// Validation message of a date-range control whose element value is not an ISO date or date range; {0} is the
    /// field's display name.
    /// </summary>
    internal const string DateRangeValidationMessageFormat = "The {0} field must be a date range.";

    /// <summary>
    /// Validation message of a time control whose element value is not a wire time (<c>HH:mm</c>, <c>HH:mm:ss</c>);
    /// {0} is the field's display name.
    /// </summary>
    internal const string TimeValidationMessageFormat = "The {0} field must be a time.";
}
