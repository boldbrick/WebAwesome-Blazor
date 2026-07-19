using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A single node within a <see cref="WaTree"/>, optionally containing nested <see cref="WaTreeItem"/> children.
/// Corresponds to the wa-tree-item Web Awesome component.
/// </summary>
public class WaTreeItem : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

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

    // Tree item properties
    /// <summary>
    /// Disables the tree item, preventing selection and expansion.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Expands the tree item, showing its children.
    /// </summary>
    [Parameter] public bool Expanded { get; set; }

    /// <summary>
    /// Enables lazy loading behavior. Remove this after appending items in response to
    /// <see cref="OnLazyLoad"/> to end the loading state and update the tree.
    /// </summary>
    [Parameter] public bool Lazy { get; set; }

    /// <summary>
    /// Draws the tree item in a selected state.
    /// </summary>
    [Parameter] public bool Selected { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked after the tree item collapses and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback OnAfterCollapse { get; set; }

    /// <summary>
    /// Invoked after the tree item expands and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback OnAfterExpand { get; set; }

    /// <summary>
    /// Invoked when the tree item collapses.
    /// </summary>
    [Parameter] public EventCallback OnCollapse { get; set; }

    /// <summary>
    /// Invoked when the tree item expands.
    /// </summary>
    [Parameter] public EventCallback OnExpand { get; set; }

    /// <summary>
    /// Invoked when the tree item's lazy state changes.
    /// </summary>
    [Parameter] public EventCallback OnLazyChange { get; set; }

    /// <summary>
    /// Invoked when a lazy item is selected. Use this to asynchronously load data and append child items
    /// before removing <see cref="Lazy"/>.
    /// </summary>
    [Parameter] public EventCallback OnLazyLoad { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tree item's content (label and, optionally, nested WaTreeItem children)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon to display when the tree item is expanded, replacing the default. Works best with WaIcon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// Icon to display when the tree item is collapsed, replacing the default. Works best with WaIcon.
    /// </summary>
    [Parameter] public RenderFragment? CollapseIconContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tree-item");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tree item-specific attributes
        builder.AddAttribute(4, "disabled", Disabled);
        builder.AddAttribute(5, "expanded", Expanded);
        builder.AddAttribute(6, "lazy", Lazy);
        builder.AddAttribute(7, "selected", Selected);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(10, "wa-after-collapse", OnAfterCollapse);

        builder.AddAttributeIfHasDelegate(11, "wa-after-expand", OnAfterExpand);

        builder.AddAttributeIfHasDelegate(12, "wa-collapse", OnCollapse);

        builder.AddAttributeIfHasDelegate(13, "wa-expand", OnExpand);

        builder.AddAttributeIfHasDelegate(14, "wa-lazy-change", OnLazyChange);

        builder.AddAttributeIfHasDelegate(15, "wa-lazy-load", OnLazyLoad);

        // Add element reference capture
        builder.AddElementReferenceCapture(16, __treeItemReference => Element = __treeItemReference);

        // Add main content (label and nested items)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        // Add expand icon slot content
        if (ExpandIconContent is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "expand-icon");
            builder.AddContent(32, ExpandIconContent);
            builder.CloseElement();
        }

        // Add collapse icon slot content
        if (CollapseIconContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "collapse-icon");
            builder.AddContent(42, CollapseIconContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Gets the tree item's nested child tree items.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a raw
    /// <see cref="JsonElement"/> representing the array of child <c>wa-tree-item</c> elements, since arbitrary
    /// DOM element arrays returned from JavaScript cannot be marshaled into live <see cref="ElementReference"/>s
    /// or wrapper instances.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<JsonElement> GetChildrenItemsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get children items: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<JsonElement>(Element.Value, "getChildrenItems");
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
