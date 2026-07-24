using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A hierarchical tree view component that allows a user to navigate and, optionally, select items
/// from a nested list.
/// Corresponds to the wa-tree Web Awesome component.
/// </summary>
public class WaTree : ComponentBase
{
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

    // Tree properties
    /// <summary>
    /// The selection behavior of the tree. <see cref="WaTreeSelection.Single"/> allows only one node to be
    /// selected at a time. <see cref="WaTreeSelection.Multiple"/> displays checkboxes and allows more than one
    /// node to be selected. <see cref="WaTreeSelection.Leaf"/> allows only leaf nodes to be selected.
    /// <see cref="WaTreeSelection.LeafMultiple"/> allows multiple leaf nodes to be selected while parent nodes
    /// only expand and collapse.
    /// </summary>
    [Parameter] public WaTreeSelection Selection { get; set; } = WaTreeSelection.Single;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when a tree item is selected or deselected.
    /// </summary>
    [Parameter] public EventCallback<WaTreeSelectionChangeEventArgs> OnSelectionChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tree's content (WaTreeItem components)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon rendered into the expand-icon slot for every item, replacing the default expand icon.
    /// </summary>
    [Parameter] public string? ExpandIconName { get; set; }

    /// <summary>
    /// Icon rendered into the collapse-icon slot for every item, replacing the default collapse icon.
    /// </summary>
    [Parameter] public string? CollapseIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tree");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tree-specific attributes
        builder.AddAttribute(4, "selection", Selection.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(10, "onwa-selection-change", OnSelectionChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __treeReference => Element = __treeReference);

        // Add child content (tree items)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        // Add icon slots
        builder.AddIconSlot(30, "expand-icon", ExpandIconName);
        builder.AddIconSlot(35, "collapse-icon", CollapseIconName);

        builder.CloseElement();
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
