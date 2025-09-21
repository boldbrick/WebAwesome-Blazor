using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tab component used inside tab groups to represent and activate tab panels.
/// Corresponds to the wa-tab Web Awesome component.
/// </summary>
public class WaTab : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Common styling parameters
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // Tab properties
    [Parameter] public string? Panel { get; set; }
    [Parameter] public bool Active { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Closable { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tab's content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tab");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "panel", Panel);
        builder.AddAttribute(5, "active", Active);
        builder.AddAttribute(6, "disabled", Disabled);
        builder.AddAttribute(7, "closable", Closable);

        // Add event handlers
        if (OnClick.HasDelegate)
            builder.AddAttribute(10, "onclick", OnClick);

        if (OnFocus.HasDelegate)
            builder.AddAttribute(11, "onfocus", OnFocus);

        if (OnBlur.HasDelegate)
            builder.AddAttribute(12, "onblur", OnBlur);

        // Add element reference capture
        builder.AddElementReferenceCapture(13, __tabReference => Element = __tabReference);

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Programmatically focuses the tab.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-tab's focus method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void Focus()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.focus() method on the underlying wa-tab element
        throw new NotImplementedException("Focus requires JavaScript interop implementation. " +
            "This should call the underlying wa-tab element's focus method.");
    }

    /// <summary>
    /// Programmatically removes focus from the tab.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-tab's blur method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void Blur()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.blur() method on the underlying wa-tab element
        throw new NotImplementedException("Blur requires JavaScript interop implementation. " +
            "This should call the underlying wa-tab element's blur method.");
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
