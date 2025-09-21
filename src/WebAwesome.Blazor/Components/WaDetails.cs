using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A details component that shows a brief summary and expands to show additional content.
/// Corresponds to the wa-details Web Awesome component.
/// </summary>
public class WaDetails : ComponentBase
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

    // Details properties
    [Parameter] public string? Summary { get; set; }
    [Parameter] public bool Open { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public WaAppearance? Appearance { get; set; }
    [Parameter] public WaIconPosition IconPosition { get; set; } = WaIconPosition.End;
    [Parameter] public string? Name { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<WaDetailsToggleEventArgs> OnToggle { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The details' main content (shown when expanded)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Custom summary content (alternative to Summary string)
    /// </summary>
    [Parameter] public RenderFragment? SummaryContent { get; set; }

    /// <summary>
    /// Custom expand icon
    /// </summary>
    [Parameter] public RenderFragment? ExpandIcon { get; set; }

    /// <summary>
    /// Custom collapse icon
    /// </summary>
    [Parameter] public RenderFragment? CollapseIcon { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-details");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "summary", Summary);
        builder.AddAttribute(5, "open", Open);
        builder.AddAttribute(6, "disabled", Disabled);
        builder.AddAttributeIfNotNull(7, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(8, "icon-position", IconPosition.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(9, "name", Name);

        // Add event handlers
        // TODO: This event needs to be mapped to the Web Awesome component events
        if (OnToggle.HasDelegate)
        {
            // Custom event handler will need JavaScript interop
        }

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __detailsReference => Element = __detailsReference);

        // Add summary slot content
        if (SummaryContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "summary");
            builder.AddContent(22, SummaryContent);
            builder.CloseElement();
        }

        // Add expand icon slot content
        if (ExpandIcon is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "expand-icon");
            builder.AddContent(32, ExpandIcon);
            builder.CloseElement();
        }

        // Add collapse icon slot content
        if (CollapseIcon is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "collapse-icon");
            builder.AddContent(42, CollapseIcon);
            builder.CloseElement();
        }

        // Add child content (main details content)
        if (ChildContent is not null)
        {
            builder.AddContent(50, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Programmatically shows the details content.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-details's show method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task ShowAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.show() method on the underlying wa-details element
        throw new NotImplementedException("ShowAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-details element's show method.");
    }

    /// <summary>
    /// Programmatically hides the details content.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-details's hide method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task HideAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.hide() method on the underlying wa-details element
        throw new NotImplementedException("HideAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-details element's hide method.");
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

/// <summary>
/// Event arguments for details toggle events
/// </summary>
public class WaDetailsToggleEventArgs : EventArgs
{
    public bool IsOpen { get; set; }
}
