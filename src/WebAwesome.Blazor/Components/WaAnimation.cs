using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component for animating elements declaratively with baked-in presets or custom keyframes.
/// Corresponds to the wa-animation Web Awesome component.
/// </summary>
public class WaAnimation : ComponentBase
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

    // Animation properties
    [Parameter] public string? Name { get; set; }
    [Parameter] public bool Play { get; set; }
    [Parameter] public int Duration { get; set; } = 1000;
    [Parameter] public int Delay { get; set; } = 0;
    [Parameter] public WaAnimationDirection Direction { get; set; } = WaAnimationDirection.Normal;
    [Parameter] public WaAnimationEasing Easing { get; set; } = WaAnimationEasing.Linear;
    [Parameter] public decimal Iterations { get; set; } = 1;
    [Parameter] public WaAnimationFill Fill { get; set; } = WaAnimationFill.None;
    [Parameter] public decimal PlaybackRate { get; set; } = 1;

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnFinish { get; set; }
    [Parameter] public EventCallback OnStart { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The element to animate (only the first child will be animated)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-animation");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "name", Name);
        builder.AddAttribute(5, "play", Play);
        builder.AddAttribute(6, "duration", Duration);
        builder.AddAttribute(7, "delay", Delay);
        builder.AddAttribute(8, "direction", Direction.ToHtmlValue());
        builder.AddAttribute(9, "easing", Easing.ToHtmlValue());
        builder.AddAttribute(10, "iterations", Iterations == decimal.MaxValue ? "Infinity" : Iterations.ToString());
        builder.AddAttribute(11, "fill", Fill.ToHtmlValue());
        builder.AddAttribute(12, "playback-rate", PlaybackRate);

        // Add event handlers
        if (OnCancel.HasDelegate)
            builder.AddAttribute(20, "wa-cancel", OnCancel);

        if (OnFinish.HasDelegate)
            builder.AddAttribute(21, "wa-finish", OnFinish);

        if (OnStart.HasDelegate)
            builder.AddAttribute(22, "wa-start", OnStart);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __animationReference => Element = __animationReference);

        // Add child content (element to animate)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets custom keyframes for the animation.
    /// </summary>
    /// <param name="keyframes">JavaScript keyframes array object</param>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to set the underlying wa-animation's keyframes property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task SetKeyframesAsync(object keyframes)
    {
        // TODO: Implement JavaScript interop call
        // Should set Element.keyframes = keyframes on the underlying wa-animation element
        throw new NotImplementedException("SetKeyframesAsync requires JavaScript interop implementation. " +
            "This should set the underlying wa-animation element's keyframes property.");
    }

    /// <summary>
    /// Cancels the current animation.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-animation's cancel method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task CancelAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.cancel() method on the underlying wa-animation element
        throw new NotImplementedException("CancelAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-animation element's cancel method.");
    }

    /// <summary>
    /// Finishes the current animation immediately.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-animation's finish method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task FinishAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.finish() method on the underlying wa-animation element
        throw new NotImplementedException("FinishAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-animation element's finish method.");
    }

    /// <summary>
    /// Gets the current time of the animation.
    /// </summary>
    /// <returns>The current time in milliseconds</returns>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to get the underlying wa-animation's currentTime property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task<decimal> GetCurrentTimeAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should return Element.currentTime property on the underlying wa-animation element
        throw new NotImplementedException("GetCurrentTimeAsync requires JavaScript interop implementation. " +
            "This should get the underlying wa-animation element's currentTime property.");
    }

    /// <summary>
    /// Sets the current time of the animation.
    /// </summary>
    /// <param name="time">The time in milliseconds</param>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to set the underlying wa-animation's currentTime property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task SetCurrentTimeAsync(decimal time)
    {
        // TODO: Implement JavaScript interop call
        // Should set Element.currentTime = time on the underlying wa-animation element
        throw new NotImplementedException("SetCurrentTimeAsync requires JavaScript interop implementation. " +
            "This should set the underlying wa-animation element's currentTime property.");
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
