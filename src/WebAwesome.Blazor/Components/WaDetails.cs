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
    #region ------ Dependency Injection ------

    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

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

    // Details properties
    [Parameter] public string? Summary { get; set; }
    [Parameter] public bool Open { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public WaAppearance? Appearance { get; set; }
    [Parameter] public WaIconPlacement IconPlacement { get; set; } = WaIconPlacement.End;
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
        builder.AddAttribute(8, "icon-placement", IconPlacement.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(9, "name", Name);

        // Add event handlers
        if (OnToggle.HasDelegate)
            builder.AddAttribute(20, "wa-show", EventCallback.Factory.Create(this, () => HandleToggleEvent(true)));
        if (OnToggle.HasDelegate)
            builder.AddAttribute(21, "wa-hide", EventCallback.Factory.Create(this, () => HandleToggleEvent(false)));

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
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show details: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Programmatically hides the details content.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide details: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
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

    /// <summary>
    /// Handles toggle events from the Web Awesome component
    /// </summary>
    private async Task HandleToggleEvent(bool isOpen)
    {
        var args = new WaDetailsToggleEventArgs { IsOpen = isOpen };
        await OnToggle.InvokeAsync(args);
    }

    #endregion
}
