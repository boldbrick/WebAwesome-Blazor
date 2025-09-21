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

    // Include-specific properties
    [Parameter] public string? Src { get; set; }
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: JS Interop needed
            // Initialize fetch mechanism for loading external content
            // Call: await JSRuntime.InvokeVoidAsync("webAwesome.include.initialize", Element);

            // The JavaScript should:
            // 1. Set up fetch() calls with proper CORS mode
            // 2. Cache requests to avoid duplicate loads
            // 3. Insert loaded HTML into the component's slot
            // 4. Emit wa-load and wa-error events with proper event details
            // 5. Handle different modes (cors, no-cors, same-origin)
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Reloads the included content
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Force reload the content, bypassing cache
    /// </remarks>
    public async Task ReloadAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.include.reload", Element);
        await Task.CompletedTask;
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

/// <summary>
/// Event args for include error events
/// </summary>
public class IncludeErrorEventArgs : EventArgs
{
    /// <summary>
    /// HTTP status code of the failed request (e.g., 404)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error message describing the failure
    /// </summary>
    public string? Message { get; set; }
}
