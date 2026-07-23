using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A declarative utility that renders markdown as plain HTML using the Marked library.
/// Corresponds to the wa-markdown Web Awesome component. The markdown source is provided through a child
/// <c>&lt;script type="text/markdown"&gt;</c> element, which this wrapper emits automatically from
/// <see cref="Content"/> or <see cref="ChildContent"/>.
/// </summary>
public class WaMarkdown : ComponentBase
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

    /// <summary>
    /// The tab stop width used when converting leading tabs to spaces during whitespace normalization. When unset,
    /// the Web Awesome default (4) applies.
    /// </summary>
    [Parameter] public int? TabSize { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The markdown source to render, emitted verbatim into the required child
    /// <c>&lt;script type="text/markdown"&gt;</c> element. Ignored when <see cref="ChildContent"/> is set.
    /// </summary>
    [Parameter] public string? Content { get; set; }

    /// <summary>
    /// Markdown source supplied as raw content, emitted into the child <c>&lt;script type="text/markdown"&gt;</c>
    /// element. Takes precedence over <see cref="Content"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-markdown");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "tab-size", TabSize);

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __markdownReference => Element = __markdownReference);

        // Emit the required <script type="text/markdown"> source child
        builder.OpenElement(20, "script");
        builder.AddAttribute(21, "type", "text/markdown");
        if (ChildContent is not null)
        {
            builder.AddContent(22, ChildContent);
        }
        else
        {
            builder.AddContent(23, Content);
        }
        builder.CloseElement();

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Reads the script content, normalizes whitespace, parses markdown, and injects the rendered result. Call
    /// this after changing the source to re-render (the component does not watch the script for changes).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task RenderMarkdownAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot render markdown: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "renderMarkdown");
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        return string.IsNullOrEmpty(Class) ? string.Empty : Class;
    }

    #endregion
}
