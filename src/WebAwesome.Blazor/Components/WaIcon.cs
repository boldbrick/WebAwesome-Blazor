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
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Icon properties
    /// <summary>
    /// The name of the icon to draw. Available names depend on the icon library being used.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// The name of a registered custom icon library.
    /// </summary>
    [Parameter] public string? Library { get; set; }

    /// <summary>
    /// The family of icons to choose from. For Font Awesome Free, valid options include <c>classic</c> and
    /// <c>brands</c>. For Font Awesome Pro subscribers, valid options also include <c>sharp</c>, <c>duotone</c>,
    /// and <c>sharp-duotone</c>.
    /// </summary>
    [Parameter] public string? Family { get; set; }

    /// <summary>
    /// The name of the icon's variant. For Font Awesome, valid options include <c>thin</c>, <c>light</c>,
    /// <c>regular</c>, and <c>solid</c> for the <c>classic</c> and <c>sharp</c> families. Custom icon libraries
    /// may or may not use this property.
    /// </summary>
    [Parameter] public string? Variant { get; set; }

    /// <summary>
    /// An external URL of an SVG file. Be sure you trust the content you are including, as it will be executed
    /// as code and can result in XSS attacks.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// An alternate description to use for assistive devices. If omitted, the icon is considered presentational
    /// and ignored by assistive devices.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Sets the width of the icon to match the cropped SVG viewBox, similar to the Font Awesome <c>fa-width-auto</c> class.
    /// </summary>
    [Parameter] public bool AutoWidth { get; set; }

    /// <summary>
    /// Swaps the opacity of duotone icons.
    /// </summary>
    [Parameter] public bool SwapOpacity { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the icon has loaded.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Invoked when the icon fails to load due to an error.
    /// </summary>
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
        builder.AddAttributeIfHasDelegate(20, "wa-load", OnLoad);

        builder.AddAttributeIfHasDelegate(21, "wa-error", OnError);

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
