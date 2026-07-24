using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A single item within a <see cref="WaAccordion"/>, with a header trigger and collapsible body.
/// Corresponds to the wa-accordion-item Web Awesome component.
/// </summary>
public class WaAccordionItem : ComponentBase
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

    // Accordion item properties
    /// <summary>
    /// The text label shown in the header. If you need HTML, use <see cref="LabelContent"/> instead.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Expands the accordion item.
    /// </summary>
    [Parameter] public bool Expanded { get; set; }

    /// <summary>
    /// Disables the accordion item so it can't be toggled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The accordion item's body content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The accordion item's label. Alternatively, use the <see cref="Label"/> attribute.
    /// </summary>
    [Parameter] public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Optional expand/collapse icon. Works best with <c>&lt;wa-icon&gt;</c>.
    /// </summary>
    [Parameter] public RenderFragment? IconContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-accordion-item");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add accordion-item-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "label", Label);
        builder.AddAttribute(11, "expanded", Expanded);
        builder.AddAttribute(12, "disabled", Disabled);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __accordionItemReference => Element = __accordionItemReference);

        // Add label slot content
        if (LabelContent is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "label");
            builder.AddContent(32, LabelContent);
            builder.CloseElement();
        }

        // Add icon slot content
        if (IconContent is not null)
        {
            builder.OpenElement(35, "span");
            builder.AddAttribute(36, "slot", "icon");
            builder.AddContent(37, IconContent);
            builder.CloseElement();
        }

        // Add body content
        if (ChildContent is not null)
            builder.AddContent(40, ChildContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Expands the accordion item with animation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ExpandAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot expand accordion item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "expand");
    }

    /// <summary>
    /// Collapses the accordion item with animation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task CollapseAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot collapse accordion item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "collapse");
    }

    /// <summary>
    /// Toggles the accordion item's expanded state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ToggleAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot toggle accordion item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "toggle");
    }

    /// <summary>
    /// Focuses the accordion item's trigger button.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus accordion item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
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
