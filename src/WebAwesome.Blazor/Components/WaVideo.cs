using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Embeds and plays video content with custom controls and captions.
/// Corresponds to the wa-video Web Awesome component (Pro).
/// </summary>
public class WaVideo : ComponentBase
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
    /// The Web Awesome default of <see cref="Controls"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaVideoControls DefaultControls = WaVideoControls.Standard;

    /// <summary>
    /// The controls preset. When unset, the Web Awesome default (standard) applies.
    /// </summary>
    [Parameter] public WaVideoControls? Controls { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Preload"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaVideoPreload DefaultPreload = WaVideoPreload.Metadata;

    /// <summary>
    /// Controls how the browser preloads the video. When unset, the Web Awesome default (metadata) applies.
    /// </summary>
    [Parameter] public WaVideoPreload? Preload { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Src"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultSrc = "";

    /// <summary>
    /// The URL of the video source. For multiple formats, place <c>source</c> elements in the default slot instead.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Poster"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPoster = "";

    /// <summary>
    /// Poster image URL shown before the video plays.
    /// </summary>
    [Parameter] public string? Poster { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Thumbnails"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultThumbnails = "";

    /// <summary>
    /// A URL pointing to a WebVTT file for timeline thumbnail previews.
    /// </summary>
    [Parameter] public string? Thumbnails { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Title"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultTitle = "";

    /// <summary>
    /// The video's title.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="IconLibrary"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultIconLibrary = "system";

    /// <summary>
    /// Icon library used for all built-in control icons.
    /// </summary>
    [Parameter] public string? IconLibrary { get; set; }

    /// <summary>
    /// Indicates whether the video is currently playing.
    /// </summary>
    [Parameter] public bool Playing { get; set; }

    /// <summary>
    /// When set, the video will be muted.
    /// </summary>
    [Parameter] public bool Muted { get; set; }

    /// <summary>
    /// Enables autoplay when the component connects.
    /// </summary>
    [Parameter] public bool Autoplay { get; set; }

    /// <summary>
    /// Loops the video when playback ends.
    /// </summary>
    [Parameter] public bool Loop { get; set; }

    /// <summary>
    /// Enables autoplay in a muted state.
    /// </summary>
    [Parameter] public bool AutoplayMuted { get; set; }

    /// <summary>
    /// Automatically resumes playback when the player scrolls back into view after being paused by scrolling out.
    /// </summary>
    [Parameter] public bool AutoplayOnVisible { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Volume"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const double DefaultVolume = 1;

    /// <summary>
    /// The video's volume, between 0 and 1. When unset, the Web Awesome default (1) applies.
    /// </summary>
    [Parameter] public double? Volume { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Duration"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const double DefaultDuration = 0;

    /// <summary>
    /// The total duration of the video in seconds.
    /// </summary>
    [Parameter] public double? Duration { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="CurrentTime"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const double DefaultCurrentTime = 0;

    /// <summary>
    /// The current playback position in seconds.
    /// </summary>
    [Parameter] public double? CurrentTime { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when playback begins.
    /// </summary>
    [Parameter] public EventCallback OnPlay { get; set; }

    /// <summary>
    /// Invoked when playback stops.
    /// </summary>
    [Parameter] public EventCallback OnPause { get; set; }

    /// <summary>
    /// Invoked when playback ends.
    /// </summary>
    [Parameter] public EventCallback OnEnded { get; set; }

    /// <summary>
    /// Invoked when an error occurs while loading or playing.
    /// </summary>
    [Parameter] public EventCallback OnError { get; set; }

    /// <summary>
    /// Invoked when the playback time changes.
    /// </summary>
    [Parameter] public EventCallback OnTimeUpdate { get; set; }

    /// <summary>
    /// Invoked when the volume changes.
    /// </summary>
    [Parameter] public EventCallback OnVolumeChange { get; set; }

    /// <summary>
    /// Invoked when metadata has been loaded.
    /// </summary>
    [Parameter] public EventCallback OnLoadedMetadata { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// The default slot. Place <c>source</c> and <c>track</c> elements for a single video, or use <see cref="Src"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content inserted at the start of the controls bar (before play/pause).
    /// </summary>
    [Parameter] public RenderFragment? ControlsStartContent { get; set; }

    /// <summary>
    /// Content inserted immediately after the play/pause button.
    /// </summary>
    [Parameter] public RenderFragment? ControlsAfterPlayContent { get; set; }

    /// <summary>
    /// Icon shown on the poster play button.
    /// </summary>
    [Parameter] public RenderFragment? PosterIcon { get; set; }

    /// <summary>
    /// Icon shown on the play/pause button when paused.
    /// </summary>
    [Parameter] public RenderFragment? PlayIcon { get; set; }

    /// <summary>
    /// Icon shown on the play/pause button when playing.
    /// </summary>
    [Parameter] public RenderFragment? PauseIcon { get; set; }

    /// <summary>
    /// Icon shown on the volume/mute button when audio is active.
    /// </summary>
    [Parameter] public RenderFragment? VolumeIcon { get; set; }

    /// <summary>
    /// Icon shown on the volume/mute button when muted or volume is 0.
    /// </summary>
    [Parameter] public RenderFragment? MuteIcon { get; set; }

    /// <summary>
    /// Icon shown on the fullscreen button when not in fullscreen.
    /// </summary>
    [Parameter] public RenderFragment? FullscreenIcon { get; set; }

    /// <summary>
    /// Icon shown on the fullscreen button when in fullscreen.
    /// </summary>
    [Parameter] public RenderFragment? ExitFullscreenIcon { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PosterIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PosterIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PlayIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PlayIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PauseIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PauseIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="VolumeIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? VolumeIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="MuteIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? MuteIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="FullscreenIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? FullscreenIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ExitFullscreenIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ExitFullscreenIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-video");

        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", Class);
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        builder.AddAttributeIfNotNull(attributes, 10, "controls", Controls?.ToHtmlValue(), DefaultControls.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 11, "preload", Preload?.ToHtmlValue(), DefaultPreload.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "src", Src, DefaultSrc);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 13, "poster", Poster, DefaultPoster);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 14, "thumbnails", Thumbnails, DefaultThumbnails);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 15, "title", Title, DefaultTitle);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 16, "icon-library", IconLibrary, DefaultIconLibrary);
        builder.AddAttribute(17, "playing", Playing);
        builder.AddAttribute(18, "muted", Muted);
        builder.AddAttribute(19, "autoplay", Autoplay);
        builder.AddAttribute(20, "loop", Loop);
        builder.AddAttribute(21, "autoplay-muted", AutoplayMuted);
        builder.AddAttribute(22, "autoplay-on-visible", AutoplayOnVisible);
        builder.AddAttributeIfNotNull(attributes, 23, "volume", Volume, DefaultVolume);
        builder.AddAttributeIfNotNull(attributes, 24, "duration", Duration, DefaultDuration);
        builder.AddAttributeIfNotNull(attributes, 25, "currentTime", CurrentTime, DefaultCurrentTime);

        // native media events re-dispatched by wa-video on the host element; delivered through
        // Blazor's built-in non-bubbling event registration (no registerCustomEventType needed)
        builder.AddAttributeIfHasDelegate(40, "onplay", OnPlay);
        builder.AddAttributeIfHasDelegate(41, "onpause", OnPause);
        builder.AddAttributeIfHasDelegate(42, "onended", OnEnded);
        builder.AddAttributeIfHasDelegate(43, "onerror", OnError);
        builder.AddAttributeIfHasDelegate(44, "ontimeupdate", OnTimeUpdate);
        builder.AddAttributeIfHasDelegate(45, "onvolumechange", OnVolumeChange);
        builder.AddAttributeIfHasDelegate(46, "onloadedmetadata", OnLoadedMetadata);

        builder.AddElementReferenceCapture(50, __videoReference => Element = __videoReference);

        AddNamedSlot(builder, 100, "poster-icon", PosterIcon, PosterIconName);
        AddNamedSlot(builder, 110, "play-icon", PlayIcon, PlayIconName);
        AddNamedSlot(builder, 120, "pause-icon", PauseIcon, PauseIconName);
        AddNamedSlot(builder, 130, "volume-icon", VolumeIcon, VolumeIconName);
        AddNamedSlot(builder, 140, "mute-icon", MuteIcon, MuteIconName);
        AddNamedSlot(builder, 150, "fullscreen-icon", FullscreenIcon, FullscreenIconName);
        AddNamedSlot(builder, 160, "exit-fullscreen-icon", ExitFullscreenIcon, ExitFullscreenIconName);
        AddFragmentSlot(builder, 170, "controls-start", ControlsStartContent);
        AddFragmentSlot(builder, 180, "controls-after-play", ControlsAfterPlayContent);

        if (ChildContent is not null)
            builder.AddContent(200, ChildContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Starts playback.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task PlayAsync() => InvokeVoidElementMethodAsync("play");

    /// <summary>
    /// Pauses playback.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task PauseAsync() => InvokeVoidElementMethodAsync("pause");

    /// <summary>
    /// Toggles between play and pause.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task TogglePlayAsync() => InvokeVoidElementMethodAsync("togglePlay");

    /// <summary>
    /// Toggles the muted state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task ToggleMuteAsync() => InvokeVoidElementMethodAsync("toggleMute");

    /// <summary>
    /// Seeks to a specific time in the video.
    /// </summary>
    /// <param name="time">Time in seconds</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task SeekAsync(double time) => InvokeVoidElementMethodAsync("seek", time);

    /// <summary>
    /// Sets the volume level.
    /// </summary>
    /// <param name="volume">Volume level between 0 and 1</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task SetVolumeAsync(double volume) => InvokeVoidElementMethodAsync("setVolume", volume);

    /// <summary>
    /// Sets the playback rate (speed).
    /// </summary>
    /// <param name="rate">Playback rate, e.g. 0.5, 1, 1.5, 2</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task SetPlaybackRateAsync(double rate) => InvokeVoidElementMethodAsync("setPlaybackRate", rate);

    /// <summary>
    /// Enters fullscreen mode.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task RequestFullscreenAsync() => InvokeVoidElementMethodAsync("requestFullscreen");

    /// <summary>
    /// Exits fullscreen mode.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task ExitFullscreenAsync() => InvokeVoidElementMethodAsync("exitFullscreen");

    /// <summary>
    /// Gets the current playback state.
    /// </summary>
    /// <returns>A snapshot of the video's playback state</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public Task<WaVideoState> GetStateAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get state: component has not been rendered yet.");

        return JSInterop.InvokeMethodAsync<WaVideoState>(Element.Value, "getState");
    }

    #endregion

    #region ------ Internals ------

    private Task InvokeVoidElementMethodAsync(string methodName, params object[] args)
    {
        if (Element == null)
            throw new InvalidOperationException($"Cannot invoke '{methodName}': component has not been rendered yet.");

        return JSInterop.InvokeMethodAsync(Element.Value, methodName, args);
    }

    // renders an icon-shaped slot: the fragment wins; otherwise a wa-icon is emitted for a non-empty name
    private static void AddNamedSlot(RenderTreeBuilder builder, int sequence, string slotName, RenderFragment? fragment, string? iconName)
    {
        if (fragment is not null)
        {
            builder.OpenElement(sequence, "span");
            builder.AddAttribute(sequence + 1, "slot", slotName);
            builder.AddContent(sequence + 2, fragment);
            builder.CloseElement();
            return;
        }

        builder.AddIconSlot(sequence + 3, slotName, iconName);
    }

    // renders arbitrary content into a named slot when the fragment is set
    private static void AddFragmentSlot(RenderTreeBuilder builder, int sequence, string slotName, RenderFragment? fragment)
    {
        if (fragment is null) return;

        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "slot", slotName);
        builder.AddContent(sequence + 2, fragment);
        builder.CloseElement();
    }

    #endregion
}
