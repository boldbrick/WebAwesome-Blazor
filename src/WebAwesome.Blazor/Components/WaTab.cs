using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tab component used inside tab groups to represent and activate tab panels.
/// Corresponds to the wa-tab Web Awesome component.
/// </summary>
public class WaTab : ComponentBase
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
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // Tab properties
    /// <summary>
    /// The name of the tab panel this tab is associated with. The panel must be located in the same tab group.
    /// </summary>
    [Parameter] public string? Panel { get; set; }

    /// <summary>
    /// Whether this tab is the currently active tab within its tab group.
    /// </summary>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// Disables the tab and prevents selection.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Shows a close button on the tab, allowing the user to remove it.
    /// </summary>
    [Parameter] public bool Closable { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the tab is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Invoked when the tab gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the tab loses focus.
    /// </summary>
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
        builder.AddAttributeIfHasDelegate(10, "onclick", OnClick);

        builder.AddAttributeIfHasDelegate(11, "onfocus", OnFocus);

        builder.AddAttributeIfHasDelegate(12, "onblur", OnBlur);

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
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus tab: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Programmatically removes focus from the tab.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur tab: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
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
