using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A button component that represents actions available to the user.
/// Corresponds to the wa-button Web Awesome component.
/// </summary>
public class WaButton : ComponentBase, IFormValidation
{
    #region ------ Dependency Injection ------

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

    // Visual & behavior properties
    [Parameter] public WaVariant? Variant { get; set; }
    [Parameter] public WaAppearance? Appearance { get; set; }
    [Parameter] public WaSize? Size { get; set; }
    [Parameter] public bool Pill { get; set; }
    [Parameter] public bool WithCaret { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public WaButtonType? Type { get; set; }

    // Link behavior (when href is provided, renders as <a>)
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public string? Download { get; set; }
    [Parameter] public string? Rel { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The button's content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to display at the start of the button (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the button (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-button");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "variant", Variant?.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(6, "size", Size?.ToHtmlValue());
        builder.AddAttribute(7, "pill", Pill);
        builder.AddAttribute(8, "with-caret", WithCaret);
        builder.AddAttribute(9, "loading", Loading);
        builder.AddAttribute(10, "disabled", Disabled);
        builder.AddAttributeIfNotNull(11, "type", Type?.ToHtmlValue());

        // Link behavior attributes
        builder.AddAttributeIfNotNullOrEmpty(12, "href", Href);
        builder.AddAttributeIfNotNullOrEmpty(13, "target", Target);
        builder.AddAttributeIfNotNullOrEmpty(14, "download", Download);
        builder.AddAttributeIfNotNullOrEmpty(15, "rel", Rel);

        // Add event handlers
        if (OnClick.HasDelegate)
            builder.AddAttribute(20, "onclick", OnClick);

        if (OnFocus.HasDelegate)
            builder.AddAttribute(21, "onfocus", OnFocus);

        if (OnBlur.HasDelegate)
            builder.AddAttribute(22, "onblur", OnBlur);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __buttonReference => Element = __buttonReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "start");
            builder.AddContent(32, StartContent);
            builder.CloseElement();
        }

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "end");
            builder.AddContent(52, EndContent);
            builder.CloseElement();
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

    #region ------ Implementation of IFormValidation ------

    /// <inheritdoc />
    public async Task SetCustomValidityAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set custom validity before the component is rendered. Element reference is null.");

        await JSInterop.SetCustomValidityAsync(Element.Value, message);
    }

    #endregion
}
