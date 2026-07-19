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
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Tab group properties
    /// <summary>
    /// The name of the panel belonging to the currently active tab.
    /// </summary>
    [Parameter] public string? Active { get; set; }

    /// <summary>
    /// The placement of the tabs.
    /// </summary>
    [Parameter] public WaTabPlacement Placement { get; set; } = WaTabPlacement.Top;

    /// <summary>
    /// When <see cref="WaActivation.Auto"/>, navigating tabs with the arrow keys instantly shows the corresponding panel. When <see cref="WaActivation.Manual"/>, the tab receives focus but is not shown until the user presses spacebar or enter.
    /// </summary>
    [Parameter] public WaActivation Activation { get; set; } = WaActivation.Auto;

    /// <summary>
    /// Disables the scroll arrows that appear when tabs overflow the tab group's width.
    /// </summary>
    [Parameter] public bool WithoutScrollControls { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the active tab changes.
    /// </summary>
    [Parameter] public EventCallback<WaTabChangeEventArgs> OnTabChange { get; set; }

    /// <summary>
    /// Invoked when a closable tab's close button is activated.
    /// </summary>
    [Parameter] public EventCallback<WaTabCloseEventArgs> OnTabClose { get; set; }

    /// <summary>
    /// Invoked when a tab panel is shown.
    /// </summary>
    [Parameter] public EventCallback<WaTabChangeEventArgs> OnTabShow { get; set; }

    /// <summary>
    /// Invoked when a tab panel is hidden.
    /// </summary>
    [Parameter] public EventCallback<WaTabChangeEventArgs> OnTabHide { get; set; }

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
        builder.AddAttribute(9, "without-scroll-controls", WithoutScrollControls);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(7, "wa-tab-change", OnTabChange);

        builder.AddAttributeIfHasDelegate(8, "wa-tab-close", OnTabClose);

        builder.AddAttributeIfHasDelegate(11, "wa-tab-show", OnTabShow);

        builder.AddAttributeIfHasDelegate(12, "wa-tab-hide", OnTabHide);

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

