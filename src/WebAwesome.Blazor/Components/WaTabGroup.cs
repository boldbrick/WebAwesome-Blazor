using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tab group component that organizes content into a container showing one section at a time.
/// Corresponds to the wa-tab-group Web Awesome component.
/// </summary>
public class WaTabGroup : ComponentBase
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

    // Tab group properties
    [Parameter] public string? Active { get; set; }
    [Parameter] public WaTabPlacement Placement { get; set; } = WaTabPlacement.Top;
    [Parameter] public WaActivation Activation { get; set; } = WaActivation.Auto;

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<WaTabChangeEventArgs> OnTabChange { get; set; }
    [Parameter] public EventCallback<WaTabCloseEventArgs> OnTabClose { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tab group's content (WaTab and WaTabPanel components)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Navigation slot for additional elements like close buttons
    /// </summary>
    [Parameter] public RenderFragment? NavContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tab-group");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "active", Active);
        builder.AddAttribute(5, "placement", Placement.ToHtmlValue());
        builder.AddAttribute(6, "activation", Activation.ToHtmlValue());

        // Add event handlers
        // TODO: These events need to be mapped to the Web Awesome component events
        // wa-tab-change, wa-tab-close
        if (OnTabChange.HasDelegate)
        {
            // Custom event handler will need JavaScript interop
        }

        if (OnTabClose.HasDelegate)
        {
            // Custom event handler will need JavaScript interop
        }

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __tabGroupReference => Element = __tabGroupReference);

        // Add nav slot content
        if (NavContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "nav");
            builder.AddContent(22, NavContent);
            builder.CloseElement();
        }

        // Add child content (tabs and tab panels)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Programmatically shows the specified tab panel.
    /// </summary>
    /// <param name="panelName">The name of the tab panel to show</param>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-tab-group's show method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void ShowTab(string panelName)
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.show(panelName) method on the underlying wa-tab-group element
        throw new NotImplementedException("ShowTab requires JavaScript interop implementation. " +
            "This should call the underlying wa-tab-group element's show method.");
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
/// Event arguments for tab change events
/// </summary>
public class WaTabChangeEventArgs : EventArgs
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Event arguments for tab close events
/// </summary>
public class WaTabCloseEventArgs : EventArgs
{
    public string Name { get; set; } = string.Empty;
}
