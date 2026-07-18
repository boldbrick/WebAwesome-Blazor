using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An icon component that displays symbols from icon libraries or custom SVG sources.
/// Corresponds to the wa-icon Web Awesome component.
/// </summary>
/// <remarks>
/// Icon library registration is handled through the WaIconLibraryService.
/// The default library is Font Awesome Free. Custom libraries can be registered
/// using the WaIconLibraryService which provides methods for Font Awesome Pro,
/// Heroicons, Lucide, and custom icon libraries. See WaIconLibraryService documentation.
///
/// With Font Awesome 7, icons have fixed width by default. Use AutoWidth to allow variable width.
/// SwapOpacity is available for duotone icons to swap the opacity of the primary and secondary layers.
/// </remarks>
public class WaIcon : ComponentBase
{
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

    // Icon properties
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Library { get; set; }
    [Parameter] public string? Family { get; set; }
    [Parameter] public string? Variant { get; set; }
    [Parameter] public string? Src { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool AutoWidth { get; set; }
    [Parameter] public bool SwapOpacity { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }
    [Parameter] public EventCallback<ErrorEventArgs> OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-icon");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add icon-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "name", Name);
        builder.AddAttributeIfNotNullOrEmpty(11, "library", Library);
        builder.AddAttributeIfNotNullOrEmpty(12, "family", Family);
        builder.AddAttributeIfNotNullOrEmpty(13, "variant", Variant);
        builder.AddAttributeIfNotNullOrEmpty(14, "src", Src);
        builder.AddAttributeIfNotNullOrEmpty(15, "label", Label);
        builder.AddAttribute(16, "auto-width", AutoWidth);
        builder.AddAttribute(17, "swap-opacity", SwapOpacity);

        // Add event handlers
        if (OnLoad.HasDelegate)
            builder.AddAttribute(20, "wa-load", OnLoad);

        if (OnError.HasDelegate)
            builder.AddAttribute(21, "wa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(22, __iconReference => Element = __iconReference);

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
