using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that asynchronously loads and includes external HTML content.
/// Corresponds to the wa-include Web Awesome component.
/// </summary>
/// <remarks>
/// Uses window.fetch() for loading content. Requests are cached automatically.
/// The included content is inserted into the component's default slot.
/// </remarks>
public class WaInclude : ComponentBase
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
    /// May be <see langword="null"/> if accessed before the component is rendered.
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

    // Include-specific properties
    /// <summary>
    /// The location of the HTML file to include. Be sure you trust the content you are including, as it will
    /// be executed as code and can result in XSS attacks.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// The fetch mode to use when requesting the included content.
    /// </summary>
    [Parameter] public WaMode Mode { get; set; } = WaMode.Cors;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the content loads successfully
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Emitted when the content fails to load
    /// </summary>
    [Parameter] public EventCallback<IncludeErrorEventArgs> OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-include");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add include-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "src", Src);
        if (Mode != WaMode.Cors)
            builder.AddAttribute(11, "mode", Mode.ToHtmlValue());

        // Add event handlers
        if (OnLoad.HasDelegate)
            builder.AddAttribute(20, "wa-load", OnLoad);
        if (OnError.HasDelegate)
            builder.AddAttribute(21, "wa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __includeReference => Element = __includeReference);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Reloads the included content
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ReloadAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reload content: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reload");
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
