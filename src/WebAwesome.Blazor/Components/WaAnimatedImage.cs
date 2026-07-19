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
/// A component for displaying animated GIFs and WEBPs that play and pause on interaction.
/// Corresponds to the wa-animated-image Web Awesome component.
/// </summary>
public class WaAnimatedImage : ComponentBase
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

    // Image properties
    /// <summary>
    /// The path to the image to load.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// A description of the image used by assistive devices.
    /// </summary>
    [Parameter] public string? Alt { get; set; }

    /// <summary>
    /// Plays the animation. When set to false, the animation will pause.
    /// </summary>
    [Parameter] public bool Play { get; set; } = true;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Icon rendered into the play-icon slot, replacing the default play icon.
    /// </summary>
    [Parameter] public string? PlayIconName { get; set; }

    /// <summary>
    /// Icon rendered into the pause-icon slot, replacing the default pause icon.
    /// </summary>
    [Parameter] public string? PauseIconName { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the image loads successfully.
    /// </summary>
    [Parameter] public EventCallback OnLoad { get; set; }

    /// <summary>
    /// Invoked when the image fails to load.
    /// </summary>
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
        builder.AddAttributeIfHasDelegate(10, "load", OnLoad);

        builder.AddAttributeIfHasDelegate(11, "error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __animatedImageReference => Element = __animatedImageReference);

        // Add play/pause icon slots
        builder.AddIconSlot(30, "play-icon", PlayIconName);
        builder.AddIconSlot(35, "pause-icon", PauseIconName);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Starts playing the animation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task PlayAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot play animation: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "play", true);
    }

    /// <summary>
    /// Pauses the animation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task PauseAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot pause animation: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "play", false);
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
