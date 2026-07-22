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
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
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

    /// <summary>
    /// Allows scripts included as part of the requested file to be executed. Be extra careful to use this feature
    /// only with trusted content, as it can result in XSS attacks.
    /// </summary>
    [Parameter] public bool AllowScripts { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the content loads successfully
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Emitted when the included file fails to load due to an error.
    /// </summary>
    [Parameter] public EventCallback<IncludeErrorEventArgs> OnIncludeError { get; set; }

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
        if (AllowScripts)
            builder.AddAttribute(12, "allow-scripts", AllowScripts);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-load", OnLoad);
        builder.AddAttributeIfHasDelegate(21, "onwa-include-error", OnIncludeError);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __includeReference => Element = __includeReference);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    // note: no ReloadAsync - wa-include exposes no reload() method in WA 3.0 and Lit's
    // reactive src property ignores same-value re-assignment; re-fetching requires changing
    // the Src parameter (e.g. via a cache-busting query string)

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
