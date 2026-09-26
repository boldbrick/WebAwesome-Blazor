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
    /// The Web Awesome default of <see cref="Selection"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaTreeSelection DefaultSelection = WaTreeSelection.Single;

    /// <summary>
    /// The selection behavior of the tree. <see cref="WaTreeSelection.Single"/> allows only one node to be
    /// selected at a time. <see cref="WaTreeSelection.Multiple"/> displays checkboxes and allows more than one
    /// node to be selected. <see cref="WaTreeSelection.Leaf"/> allows only leaf nodes to be selected.
    /// <see cref="WaTreeSelection.LeafMultiple"/> allows multiple leaf nodes to be selected while parent nodes
    /// only expand and collapse.
    /// </summary>
    [Parameter] public WaTreeSelection Selection { get; set; } = DefaultSelection;

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
    /// The icon to show on every item when it is expanded, rendered into the element's "expand-icon" slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ExpandIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ExpandIconName { get; set; }

    /// <summary>
    /// The icon to show on every item when it is collapsed, rendered into the element's "collapse-icon" slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? CollapseIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="CollapseIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? CollapseIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-tree");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tree-specific attributes
        builder.AddDefaultedAttribute(attributes, 4, "selection", Selection.ToHtmlValue(), DefaultSelection.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(10, "onwa-selection-change", OnSelectionChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __treeReference => Element = __treeReference);

        // Add child content (tree items)
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
        else
        {
            builder.AddIconSlot(33, "expand-icon", ExpandIconName);
        }

        // Add collapse icon slot content
        if (CollapseIconContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "collapse-icon");
            builder.AddContent(42, CollapseIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(43, "collapse-icon", CollapseIconName);
        }

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
