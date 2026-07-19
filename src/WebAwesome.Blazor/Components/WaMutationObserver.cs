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

    // MutationObserver options
    /// <summary>
    /// Watches for changes to attributes on the observed element.
    /// </summary>
    [Parameter] public bool Attr { get; set; }

    /// <summary>
    /// Watches for the addition or removal of child nodes.
    /// </summary>
    [Parameter] public bool ChildList { get; set; }

    /// <summary>
    /// Watches for changes to the character data contained within the node.
    /// </summary>
    [Parameter] public bool CharData { get; set; }

    /// <summary>
    /// Extends monitoring to the entire subtree of the observed element, not just its immediate children.
    /// </summary>
    [Parameter] public bool Subtree { get; set; }

    /// <summary>
    /// Indicates whether the attribute's previous value should be recorded when monitoring changes.
    /// </summary>
    [Parameter] public bool AttributeOldValue { get; set; }

    /// <summary>
    /// Indicates whether the previous value of the node's text should be recorded.
    /// </summary>
    [Parameter] public bool CharacterDataOldValue { get; set; }

    /// <summary>
    /// Restricts attribute change notifications to the specified space-separated list of attribute names,
    /// e.g. <c>class id title</c>. Use <c>*</c> to watch all attributes.
    /// </summary>
    [Parameter] public string? AttributeFilter { get; set; }

    /// <summary>
    /// Indicates whether the attribute's previous value should be recorded when monitoring changes.
    /// </summary>
    [Parameter] public bool AttrOldValue { get; set; }

    /// <summary>
    /// Disables the mutation observer.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Indicates whether the previous value of the node's character data should be recorded.
    /// </summary>
    [Parameter] public bool CharDataOldValue { get; set; }

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
        builder.AddAttribute(17, "attr-old-value", AttrOldValue);
        builder.AddAttribute(18, "disabled", Disabled);
        builder.AddAttribute(19, "char-data-old-value", CharDataOldValue);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "wa-mutation", OnMutation);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __observerReference => Element = __observerReference);

        // Add child content to observe
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Disconnects the mutation observer
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task DisconnectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot disconnect observer: component has not been rendered yet.");

        // the underlying wa-mutation-observer element exposes stopObserver()/startObserver(), not disconnect()/reconnect()
        await JSInterop.InvokeMethodAsync(Element.Value, "stopObserver");
    }

    /// <summary>
    /// Reconnects the mutation observer with current settings
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ReconnectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reconnect observer: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "startObserver");
    }

    /// <summary>
    /// Gets pending mutation records without waiting for callback
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of mutation records</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<object[]> TakeRecordsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot take records: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<object[]>(Element.Value, "takeRecords");
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
