using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A utility component that applies Web Awesome text styling classes for typography.
/// Corresponds to the wa-body-*, wa-heading-*, and wa-caption-* CSS utility classes.
/// </summary>
public class WaText : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? ElementRef { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names appended to the wa-body/wa-heading/wa-caption utility class on the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Text styling properties
    /// <summary>
    /// Typography role (body, heading, or caption) that selects the wa-* utility class family and default semantic element.
    /// </summary>
    [Parameter] public TextVariant Variant { get; set; } = TextVariant.Body;

    /// <summary>
    /// Text size token that selects the wa-*-* utility class suffix and, for headings, the heading level.
    /// </summary>
    [Parameter] public TextSize Size { get; set; } = TextSize.M;

    /// <summary>
    /// Overrides the rendered HTML element name instead of the variant/size-based default.
    /// </summary>
    [Parameter] public string? Element { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The text content to be styled
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var elementName = GetElementName();

        builder.OpenElement(0, elementName);
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddElementReferenceCapture(4, __elementReference => ElementRef = __elementReference);

        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the HTML element name to use
    /// </summary>
    private string GetElementName()
    {
        // Use custom element if specified
        if (!string.IsNullOrEmpty(Element))
            return Element;

        // Default semantic elements based on variant
        return Variant switch
        {
            TextVariant.Heading => GetHeadingElement(),
            TextVariant.Caption => "small",
            TextVariant.Body => "p",
            _ => "span"
        };
    }

    /// <summary>
    /// Gets the appropriate heading element based on size
    /// </summary>
    private string GetHeadingElement()
    {
        return Size switch
        {
            TextSize.XS => "h6",
            TextSize.S => "h5",
            TextSize.M => "h4",
            TextSize.L => "h3",
            TextSize.XL => "h2",
            TextSize.XL2 => "h1",
            TextSize.XL3 => "h1",
            _ => "h4"
        };
    }

    /// <summary>
    /// Gets the CSS class string with text utility classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        // Add text utility class based on variant and size
        var textClass = BuildTextClass();
        classes.Add(textClass);

        // Add user classes
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    /// <summary>
    /// Builds the text utility class name
    /// </summary>
    private string BuildTextClass()
    {
        var variant = Variant.ToHtmlValue();
        var size = Size.ToHtmlValue();

        // Handle special cases for heading sizes beyond XL
        if (Variant == TextVariant.Heading)
        {
            return Size switch
            {
                TextSize.XL2 => "wa-heading-2xl",
                TextSize.XL3 => "wa-heading-3xl",
                _ => $"wa-{variant}-{size}"
            };
        }

        // For body and caption, only use sizes they support
        if (Variant == TextVariant.Caption && (Size == TextSize.XL2 || Size == TextSize.XL3))
        {
            // Caption doesn't support 2xl/3xl, fallback to xl
            return "wa-caption-xl";
        }

        return $"wa-{variant}-{size}";
    }

    #endregion
}
