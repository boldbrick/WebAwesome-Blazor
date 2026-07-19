using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
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
    #region ------ Injected Services ------

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

    // Animation properties
    /// <summary>
    /// The name of the built-in animation to use. For custom animations, use <see cref="SetKeyframesAsync"/>.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Plays the animation. When set to false, the animation will be paused.
    /// </summary>
    [Parameter] public bool Play { get; set; }

    /// <summary>
    /// The number of milliseconds each iteration of the animation takes to complete.
    /// </summary>
    [Parameter] public int Duration { get; set; } = 1000;

    /// <summary>
    /// The number of milliseconds to delay the start of the animation.
    /// </summary>
    [Parameter] public int Delay { get; set; } = 0;

    /// <summary>
    /// Determines the direction of playback as well as the behavior when reaching the end of an iteration.
    /// </summary>
    [Parameter] public WaAnimationDirection Direction { get; set; } = WaAnimationDirection.Normal;

    /// <summary>
    /// The easing function to use for the animation.
    /// </summary>
    [Parameter] public WaAnimationEasing Easing { get; set; } = WaAnimationEasing.Linear;

    /// <summary>
    /// The number of iterations to run before the animation completes. Use <see cref="decimal.MaxValue"/> for an
    /// infinite, looping animation.
    /// </summary>
    [Parameter] public decimal Iterations { get; set; } = 1;

    /// <summary>
    /// Sets how the animation applies styles to its target before and after its execution.
    /// </summary>
    [Parameter] public WaAnimationFill Fill { get; set; } = WaAnimationFill.None;

    /// <summary>
    /// The animation's playback rate. A value of <c>1</c> plays the animation at normal speed; a negative value
    /// reverses the animation.
    /// </summary>
    [Parameter] public decimal PlaybackRate { get; set; } = 1;

    /// <summary>
    /// The number of milliseconds to delay after the end of the animation.
    /// </summary>
    [Parameter] public int? EndDelay { get; set; }

    /// <summary>
    /// The offset at which to start the animation, usually between 0 (start) and 1 (end).
    /// </summary>
    [Parameter] public double? IterationStart { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the animation is canceled.
    /// </summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Invoked when the animation finishes.
    /// </summary>
    [Parameter] public EventCallback OnFinish { get; set; }

    /// <summary>
    /// Invoked when the animation starts or restarts.
    /// </summary>
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
        builder.AddAttributeIfNotNull(13, "end-delay", EndDelay);
        builder.AddAttributeIfNotNull(14, "iteration-start", IterationStart);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "wa-cancel", OnCancel);

        builder.AddAttributeIfHasDelegate(21, "wa-finish", OnFinish);

        builder.AddAttributeIfHasDelegate(22, "wa-start", OnStart);

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
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SetKeyframesAsync(object keyframes)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set keyframes: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "keyframes", keyframes);
    }

    /// <summary>
    /// Cancels the current animation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the method call fails</exception>
    public async Task CancelAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot cancel animation: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "cancel");
    }

    /// <summary>
    /// Finishes the current animation immediately.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FinishAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot finish animation: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "finish");
    }

    /// <summary>
    /// Gets the current time of the animation.
    /// </summary>
    /// <returns>The current time in milliseconds</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the property access fails</exception>
    public async Task<decimal> GetCurrentTimeAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get current time: component has not been rendered yet.");

        return await JSInterop.GetPropertyAsync<decimal>(Element.Value, "currentTime");
    }

    /// <summary>
    /// Sets the current time of the animation.
    /// </summary>
    /// <param name="time">The time in milliseconds</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SetCurrentTimeAsync(decimal time)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set current time: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "currentTime", time);
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
