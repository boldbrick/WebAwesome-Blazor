using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class for the labeled form controls with a popup (a listbox, a picker panel or a calendar): declares the
/// "open" attribute, the show/hide methods and the four popup events once, and renders the event handlers through
/// the shared cluster renderer, relayed where the element dispatches them as non-bubbling events.
/// </summary>
/// <remarks>
/// The popup's placement (and distance) stays on each wrapper: every element takes a different set of placements.
/// </remarks>
/// <typeparam name="TValue">The type of value bound to the input</typeparam>
public abstract class WaPopupInputBase<TValue> : WaLabeledInputBase<TValue>, IWaPopupControl
{
    /// <inheritdoc />
    [Parameter] public bool Open { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <inheritdoc />
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    /// <inheritdoc />
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <inheritdoc />
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    #region ------ Internals ------

    /// <summary>
    /// Whether the element dispatches its popup events as non-bubbling events, which Blazor never receives, so that
    /// <see cref="AddPopupEventHandlers"/> binds their relays (see <see cref="Constants.RelayedShowEventAttribute"/>)
    /// instead of the wa-* events.
    /// </summary>
    internal virtual bool RelaysPopupEvents => false;

    #endregion

    #region ------ Interface for descendants ------

    /// <summary>
    /// Adds the handlers of the popup events, each only when its callback is set, in the order show, hide,
    /// after-show, after-hide: at sequence + 0..3, or at sequence + 0..7 when the events are relayed.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The constant base sequence number</param>
    protected void AddPopupEventHandlers(RenderTreeBuilder builder, int sequence)
        => FormControlRendering.AddPopupEventHandlers(builder, sequence, this, RelaysPopupEvents);

    #endregion
}
