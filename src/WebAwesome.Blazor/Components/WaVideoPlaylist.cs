using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Wraps multiple video elements into a playlist with navigation controls.
/// Corresponds to the wa-video-playlist Web Awesome component (Pro).
/// </summary>
public class WaVideoPlaylist : ComponentBase
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
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The controls preset forwarded to each child video. When unset, the Web Awesome default (full) applies.
    /// </summary>
    [Parameter] public WaVideoControls? Controls { get; set; }

    /// <summary>
    /// Icon library used for placeholder icons.
    /// </summary>
    [Parameter] public string? IconLibrary { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the active video changes.
    /// </summary>
    [Parameter] public EventCallback<WaVideoChangeEventArgs> OnVideoChange { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// The default slot. Place video elements to create a playlist.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-video-playlist");

        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", Class);
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "controls", Controls?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(5, "icon-library", IconLibrary);

        builder.AddAttributeIfHasDelegate(20, "onwa-video-change", OnVideoChange);

        builder.AddElementReferenceCapture(30, __playlistReference => Element = __playlistReference);

        if (ChildContent is not null)
            builder.AddContent(40, ChildContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Plays the next video in the playlist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task NextAsync() => InvokeVoidElementMethodAsync("next");

    /// <summary>
    /// Plays the previous video in the playlist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task PreviousAsync() => InvokeVoidElementMethodAsync("previous");

    /// <summary>
    /// Jumps to the video at the given index.
    /// </summary>
    /// <param name="index">Zero-based index of the video to activate</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task GoToAsync(int index) => InvokeVoidElementMethodAsync("goTo", index);

    #endregion

    #region ------ Internals ------

    private Task InvokeVoidElementMethodAsync(string methodName, params object[] args)
    {
        if (Element == null)
            throw new InvalidOperationException($"Cannot invoke '{methodName}': component has not been rendered yet.");

        return JSInterop.InvokeMethodAsync(Element.Value, methodName, args);
    }

    #endregion
}
