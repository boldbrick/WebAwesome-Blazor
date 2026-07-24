using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An individual notification displayed within a <see cref="WaToast"/> stack.
/// Corresponds to the wa-toast-item Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaToastItem : ComponentBase
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The length of time, in milliseconds, before the toast item is automatically dismissed.
    /// Set to 0 to keep the toast item open until the user dismisses it.
    /// </summary>
    [Parameter] public int? Duration { get; set; }

    /// <summary>
    /// The toast item's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// The toast item's variant.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true when slotting content into the "icon" slot (via
    /// <see cref="IconContent"/>) so the server-rendered markup includes the icon before the component hydrates
    /// on the client.
    /// </summary>
    [Parameter] public bool WithIcon { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the toast item begins to show.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked after the toast item has finished showing.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked when the toast item begins to hide.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the toast item has finished hiding.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The toast item's message content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Rich markup rendered into the "icon" slot, shown at the start of the toast item.
    /// </summary>
    [Parameter] public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="IconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? IconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-toast-item");

        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "duration", Duration);
        builder.AddAttributeIfNotNull(5, "size", Size?.ToHtmlValue());
        builder.AddAttributeIfNotNull(6, "variant", Variant?.ToHtmlValue());
        builder.AddAttribute(7, "with-icon", WithIcon);

        // event handlers (onwa- prefix; all four events are registered in the JS initializer)
        builder.AddAttributeIfHasDelegate(10, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(11, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(12, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(13, "onwa-after-hide", OnAfterHide);

        builder.AddElementReferenceCapture(20, __toastItemReference => Element = __toastItemReference);

        // icon slot (icon-shaped: fragment wins, icon name is the convenience fallback)
        if (IconContent is not null)
        {
            builder.OpenElement(25, "span");
            builder.AddAttribute(26, "slot", "icon");
            builder.AddContent(27, IconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(28, "icon", IconName);
        }

        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Hides the toast item with animation and removes it from the DOM.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot hide the toast item before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    #endregion

    #region ------ Internals ------

    private string GetCombinedCssClass()
    {
        return string.IsNullOrEmpty(Class) ? string.Empty : Class;
    }

    #endregion
}
