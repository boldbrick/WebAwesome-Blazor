using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An accordion that groups <see cref="WaAccordionItem"/> elements and controls how they expand and collapse.
/// Corresponds to the wa-accordion Web Awesome component.
/// </summary>
public class WaAccordion : ComponentBase
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

    // Accordion properties
    /// <summary>
    /// The accordion's visual appearance.
    /// </summary>
    [Parameter] public WaAppearance Appearance { get; set; } = WaAppearance.Outlined;

    /// <summary>
    /// The heading level for child item triggers (1–6), or "none" to omit the heading wrapper. Defaults to "3".
    /// </summary>
    [Parameter] public string? HeadingLevel { get; set; }

    /// <summary>
    /// The location of the expand/collapse icon in child items.
    /// </summary>
    [Parameter] public WaIconPlacement IconPlacement { get; set; } = WaIconPlacement.End;

    /// <summary>
    /// Controls how items can be expanded.
    /// </summary>
    [Parameter] public WaAccordionMode Mode { get; set; } = WaAccordionMode.Multiple;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The accordion's items (one or more <see cref="WaAccordionItem"/> elements).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked before an item expands. Cancelable on the underlying element.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnExpand { get; set; }

    /// <summary>
    /// Invoked after an item finishes expanding.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterExpand { get; set; }

    /// <summary>
    /// Invoked before an item collapses. Cancelable on the underlying element.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnCollapse { get; set; }

    /// <summary>
    /// Invoked after an item finishes collapsing.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterCollapse { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-accordion");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add accordion-specific attributes
        if (Appearance != WaAppearance.Outlined)
            builder.AddAttribute(10, "appearance", Appearance.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(11, "heading-level", HeadingLevel);
        if (IconPlacement != WaIconPlacement.End)
            builder.AddAttribute(12, "icon-placement", IconPlacement.ToHtmlValue());
        if (Mode != WaAccordionMode.Multiple)
            builder.AddAttribute(13, "mode", Mode.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-expand", OnExpand);
        builder.AddAttributeIfHasDelegate(21, "onwa-after-expand", OnAfterExpand);
        builder.AddAttributeIfHasDelegate(22, "onwa-collapse", OnCollapse);
        builder.AddAttributeIfHasDelegate(23, "onwa-after-collapse", OnAfterCollapse);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __accordionReference => Element = __accordionReference);

        // Add child content (accordion items)
        if (ChildContent is not null)
            builder.AddContent(40, ChildContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Expands all accordion items. No-op when <see cref="Mode"/> is <see cref="WaAccordionMode.Single"/> or
    /// <see cref="WaAccordionMode.SingleCollapsible"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ExpandAllAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot expand accordion items: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "expandAll");
    }

    /// <summary>
    /// Collapses all accordion items.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task CollapseAllAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot collapse accordion items: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "collapseAll");
    }

    #endregion

    #region ------ Internals ------

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
