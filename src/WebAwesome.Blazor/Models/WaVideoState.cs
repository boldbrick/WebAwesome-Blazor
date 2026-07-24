namespace WebAwesome.Blazor.Models;

/// <summary>
/// A snapshot of a video player's playback state, returned by the video wrapper's GetStateAsync method.
/// </summary>
/// <param name="Playing">Whether the video is currently playing.</param>
/// <param name="CurrentTime">The current playback position, in seconds.</param>
/// <param name="Duration">The total duration of the video, in seconds.</param>
/// <param name="Volume">The current volume level, between 0 and 1.</param>
/// <param name="Muted">Whether the video is currently muted.</param>
/// <param name="PlaybackRate">The current playback rate (speed).</param>
public record WaVideoState(
    bool Playing,
    double CurrentTime,
    double Duration,
    double Volume,
    bool Muted,
    double PlaybackRate);
