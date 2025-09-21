using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that provides a declarative interface to the MutationObserver API.
/// Corresponds to the wa-mutation-observer Web Awesome component.
/// </summary>
/// <remarks>
/// Reports changes to the content it wraps through the wa-mutation event.
/// Must specify at least one of: attr, child-list, or char-data attributes.
/// </remarks>
public class WaMutationObserver : ComponentBase
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

    // MutationObserver options
    [Parameter] public bool Attr { get; set; }
    [Parameter] public bool ChildList { get; set; }
    [Parameter] public bool CharData { get; set; }
    [Parameter] public bool Subtree { get; set; }
    [Parameter] public bool AttributeOldValue { get; set; }
    [Parameter] public bool CharacterDataOldValue { get; set; }
    [Parameter] public string? AttributeFilter { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to observe for mutations
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when mutations are observed
    /// </summary>
    [Parameter] public EventCallback<MutationEventArgs> OnMutation { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-mutation-observer");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add mutation observer attributes
        builder.AddAttribute(10, "attr", Attr);
        builder.AddAttribute(11, "child-list", ChildList);
        builder.AddAttribute(12, "char-data", CharData);
        builder.AddAttribute(13, "subtree", Subtree);
        builder.AddAttribute(14, "attribute-old-value", AttributeOldValue);
        builder.AddAttribute(15, "character-data-old-value", CharacterDataOldValue);
        builder.AddAttributeIfNotNullOrEmpty(16, "attribute-filter", AttributeFilter);

        // Add event handlers
        if (OnMutation.HasDelegate)
            builder.AddAttribute(20, "wa-mutation", OnMutation);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __observerReference => Element = __observerReference);

        // Add child content to observe
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: JS Interop needed
            // Initialize MutationObserver with specified options
            // Call: await JSRuntime.InvokeVoidAsync("webAwesome.mutationObserver.initialize", Element);

            // The JavaScript should:
            // 1. Create new MutationObserver instance
            // 2. Parse component attributes to build MutationObserverInit options:
            //    - attributes: attr attribute value
            //    - childList: child-list attribute value
            //    - characterData: char-data attribute value
            //    - subtree: subtree attribute value
            //    - attributeOldValue: attribute-old-value attribute value
            //    - characterDataOldValue: character-data-old-value attribute value
            //    - attributeFilter: parse attribute-filter as array of strings
            // 3. Observe the component's child content with these options
            // 4. When mutations occur, emit wa-mutation event with MutationRecord array
            // 5. Validate that at least one of attr, child-list, or char-data is true
            // 6. Handle disconnection/reconnection on component updates
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Disconnects the mutation observer
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Stop observing mutations
    /// </remarks>
    public async Task DisconnectAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.mutationObserver.disconnect", Element);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Reconnects the mutation observer with current settings
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Resume observing mutations
    /// </remarks>
    public async Task ReconnectAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.mutationObserver.reconnect", Element);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets pending mutation records without waiting for callback
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Return current mutation records
    /// </remarks>
    public async Task<object[]> TakeRecordsAsync()
    {
        // TODO: JS Interop needed
        // return await JSRuntime.InvokeAsync<object[]>("webAwesome.mutationObserver.takeRecords", Element);
        return await Task.FromResult(Array.Empty<object>());
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
/// Event args for mutation events
/// </summary>
public class MutationEventArgs : EventArgs
{
    /// <summary>
    /// Array of MutationRecord objects describing the mutations
    /// </summary>
    public object[]? MutationRecords { get; set; }
}
