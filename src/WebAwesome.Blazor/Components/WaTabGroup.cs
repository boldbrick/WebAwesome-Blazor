using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tab group component that organizes content into a container showing one section at a time.
/// Corresponds to the wa-tab-group Web Awesome component.
/// </summary>
public class WaTabGroup : ComponentBase
{
    #region ------ Injected Services ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

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
        if (OnTabChange.HasDelegate)
            builder.AddAttribute(7, "wa-tab-change", OnTabChange);

        if (OnTabClose.HasDelegate)
            builder.AddAttribute(8, "wa-tab-close", OnTabClose);

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
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowTabAsync(string panelName)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show tab: component has not been rendered yet.");

        if (string.IsNullOrEmpty(panelName))
            throw new ArgumentNullException(nameof(panelName));

        await JSInterop.SetPropertyAsync(Element.Value, "active", panelName);
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

