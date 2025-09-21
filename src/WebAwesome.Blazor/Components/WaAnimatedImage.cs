using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component for displaying animated GIFs and WEBPs that play and pause on interaction.
/// Corresponds to the wa-animated-image Web Awesome component.
/// </summary>
public class WaAnimatedImage : ComponentBase
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

    // Image properties
    [Parameter] public string? Src { get; set; }
    [Parameter] public string? Alt { get; set; }
    [Parameter] public bool Play { get; set; } = true;

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback OnLoad { get; set; }
    [Parameter] public EventCallback OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-animated-image");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "src", Src);
        builder.AddAttributeIfNotNullOrEmpty(5, "alt", Alt);
        builder.AddAttribute(6, "play", Play);

        // Add event handlers
        if (OnLoad.HasDelegate)
            builder.AddAttribute(10, "load", OnLoad);

        if (OnError.HasDelegate)
            builder.AddAttribute(11, "error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __animatedImageReference => Element = __animatedImageReference);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Starts playing the animation.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-animated-image's play method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task PlayAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.play() method on the underlying wa-animated-image element
        throw new NotImplementedException("PlayAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-animated-image element's play method.");
    }

    /// <summary>
    /// Pauses the animation.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-animated-image's pause method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task PauseAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.pause() method on the underlying wa-animated-image element
        throw new NotImplementedException("PauseAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-animated-image element's pause method.");
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
