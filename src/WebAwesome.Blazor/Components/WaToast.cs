using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A toast stack that displays brief, non-blocking notifications above page content.
/// A single instance manages multiple <see cref="WaToastItem"/> notifications.
/// Corresponds to the wa-toast Web Awesome component.
/// </summary>
public class WaToast : ComponentBase
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

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
    /// The placement of the toast stack on the screen.
    /// </summary>
    [Parameter] public WaToastPlacement? Placement { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The toast stack's content; place <see cref="WaToastItem"/> elements here to show them as notifications.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-toast");

        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "placement", Placement?.ToHtmlValue());

        builder.AddElementReferenceCapture(10, __toastReference => Element = __toastReference);

        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Creates a toast notification programmatically and adds it to the stack.
    /// </summary>
    /// <param name="message">The message to display in the toast notification</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task CreateAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot create a toast before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "create", message);
    }

    #endregion

    #region ------ Internals ------

    private string GetCombinedCssClass()
    {
        return string.IsNullOrEmpty(Class) ? string.Empty : Class;
    }

    #endregion
}
