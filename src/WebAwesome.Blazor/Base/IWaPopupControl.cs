using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// A form control with a popup (a listbox, a picker panel or a calendar): the "open" attribute, the show() and
/// hide() methods, and the wa-show, wa-after-show, wa-hide and wa-after-hide events. Groups the members for
/// consumers and tests; <see cref="WaPopupInputBase{TValue}"/> declares and renders them. The popup's placement
/// stays on each wrapper, because every element takes a different set of placements.
/// </summary>
public interface IWaPopupControl
{
    /// <summary>
    /// Whether the popup is open.
    /// </summary>
    bool Open { get; }

    /// <summary>
    /// Invoked when the popup opens, before its animation (the element's wa-show event).
    /// </summary>
    EventCallback<EventArgs> OnShow { get; }

    /// <summary>
    /// Invoked when the popup closes, before its animation (the element's wa-hide event).
    /// </summary>
    EventCallback<EventArgs> OnHide { get; }

    /// <summary>
    /// Invoked after the popup opens and all animations are complete.
    /// </summary>
    EventCallback<EventArgs> OnAfterShow { get; }

    /// <summary>
    /// Invoked after the popup closes and all animations are complete.
    /// </summary>
    EventCallback<EventArgs> OnAfterHide { get; }

    /// <summary>
    /// Opens the popup.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    Task ShowAsync();

    /// <summary>
    /// Closes the popup.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    Task HideAsync();
}
