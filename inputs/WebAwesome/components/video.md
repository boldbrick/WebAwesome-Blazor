<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/video.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/video -->

# Video [Pro]

**Full documentation:** https://webawesome.com/docs/components/video

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-video>`

ProIncluded with Web Awesome Pro Experimental [Media](https://webawesome.com/docs/components/?category=media) [Since 3.7](https://webawesome.com/docs/resources/changelog#wa_370)

Videos are used to embed and play video content with custom controls and captions.

**[Get Video with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=video)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   Responsive [Layout Tools](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma)
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Video!

```html
<wa-video title="Web Awesome" controls="full">
  <source src="https://uploads.webawesome.com/waks_compressed.mp4" type="video/mp4" />
</wa-video>
```

## Examples

Link to This Section

### Adding Video Sources

Link to This Section

The simplest way to add a video is with the `src` attribute.

```html
<wa-video
  src="https://uploads.webawesome.com/01-create-your-first-kit.mp4"
  title="Using Kits in Your Project"
  poster="/assets/images/fa-part-1.jpg"
></wa-video>
```

For multiple formats or additional options, use [`<source>`](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/source) elements instead.

```html
<wa-video title="Using Kits in Your Project" poster="/assets/images/fa-part-1.jpg">
  <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
  <source src="https://uploads.webawesome.com/01-create-your-first-kit.ogv" type="video/ogg" />
  <source src="https://uploads.webawesome.com/01-create-your-first-kit.webm" type="video/webm" />
</wa-video>
```

### Controls

Link to This Section

The video player offers three control presets: `none`, `standard`, and `full`.

#### None

Link to This Section

No controls are displayed. The video can still be played programmatically via JavaScript, and the poster overlay and captions remain visible.

```html
<wa-video controls="none" title="My Video" poster="/assets/images/kits.jpg">
  <source
    src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4"
    type="video/mp4"
  />
</wa-video>
```

#### Standard

Link to This Section

Displays playback, a seekable timeline, elapsed/total time, volume, captions, and fullscreen controls.

```html
<wa-video controls="standard" title="My Video" poster="/assets/images/fa-part-2.jpg">
  <source src="https://uploads.webawesome.com/02-using-kits-in-your-project.mp4" type="video/mp4" />
  <track
    src="https://uploads.webawesome.com/02-using-kits-in-your-project.vtt"
    kind="subtitles"
    srclang="en"
    label="English"
  />
</wa-video>
```

#### Full

Link to This Section

Includes everything in standard, plus playback speed selection and picture-in-picture.

```html
<wa-video controls="full" title="My Video" poster="/assets/images/fa-part-1.jpg">
  <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
  <track
    src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
    kind="subtitles"
    srclang="en"
    label="English"
  />
</wa-video>
```

### Poster Images

Link to This Section

Add a poster image that displays before the video plays. If no `poster` is provided, no overlay is shown and the browser will display the first frame of the video instead.

```html
<wa-video title="Using Kits in Your Project" poster="/assets/images/teams.jpg" controls="full">
  <source
    src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
    type="video/mp4"
  />
</wa-video>
```

### Captions and Subtitles

Link to This Section

Add a [`<track>`](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/track) element to enable captions using [standard WebVTT](https://developer.mozilla.org/en-US/docs/Web/API/WebVTT_API/Web_Video_Text_Tracks_Format) files.

```html
<wa-video controls="standard" title="Using Kits in Your Project" poster="/assets/images/fa-part-1.jpg">
  <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
  <track
    src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
    kind="subtitles"
    srclang="en"
    label="English"
  />
</wa-video>
```

Captions are rendered above the video controls and automatically adjust position when controls show or hide.

### Icon Slots

Link to This Section

Several slots like `poster-icon` and `pause-icon` are provided to let you customize which icons you'd like to show.

```html
<wa-video title="Using Kits in Your Project" poster="/assets/images/teams.jpg" controls="full">
  <wa-icon slot="poster-icon" name="kiwi-bird" family="duotone" variant="solid"></wa-icon>
  <wa-icon slot="play-icon" name="fish" family="duotone" variant="solid"></wa-icon>
  <wa-icon slot="pause-icon" name="fish-bones" family="duotone" variant="solid"></wa-icon>
  <source
    src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
    type="video/mp4"
  />
</wa-video>
```

### Playlists

Link to This Section

To group multiple videos into a playlist, use [`<wa-video-playlist>`](https://webawesome.com/docs/components/video-playlist).

### Video Recommendations

Link to This Section

Recommended to ensure fast loading, broad browser compatibility, and the best playback experience across devices.

#### Video Encoding

Link to This Section | Setting | Recommended | Reason |
| --- | --- | --- |
| Codec | H.264 (MP4) | Broadest browser and device support |
| Resolution | 1280×720 (720p) | Good balance of quality and file size |
| Frame rate | 24–30fps | Smooth motion without excess data |
| Bitrate | 2–5 megabits/s | Good quality at 720p without buffering |  

#### Poster Images

Link to This Section | Setting | Recommended | Reason |
| --- | --- | --- |
| Format | JPEG (80–85%) or WebP | Small file size with wide browser support |
| File size | Under 200KB | Fast initial load before video starts |
| Aspect ratio | 16:9 | Matches standard video dimensions |
| Resolution | Match video exactly | Prevents layout shift on load |  

#### Caption Files

Link to This Section | Setting | Recommended | Reason |
| --- | --- | --- |
| Format | WebVTT (.vtt) | \`\` Only format supported by the HTML element |
| Encoding | UTF-8 | Ensures special characters and non-Latin scripts render correctly |
| Timing | Frame accurate | Prevents captions from appearing early or late |

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

- `(default)` — The default slot. Place `<source>` and `<track>` elements for a single video. Alternatively, use the `src` attribute for a single source.
- `controls-start` — Content inserted at the start of the controls bar (before play/pause). Used by `<wa-video-playlist>` to inject the prev button.
- `controls-after-play` — Content inserted immediately after the play/pause button. Used by `<wa-video-playlist>` to inject the next button.
- `poster-icon` — Icon shown on the poster play button. Defaults to a play-circle icon.
- `play-icon` — Icon shown on the play/pause button when paused.
- `pause-icon` — Icon shown on the play/pause button when playing.
- `volume-icon` — Icon shown on the volume/mute button when audio is active.
- `mute-icon` — Icon shown on the volume/mute button when muted or volume is 0.
- `fullscreen-icon` — Icon shown on the fullscreen button when not in fullscreen.
- `exit-fullscreen-icon` — Icon shown on the fullscreen button when in fullscreen.

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `controls` |  | `'none' \| 'standard' \| 'full'` | `'standard'` | The video's controls preset. - `none` — no controls are shown. - `standard` — shows the timeline, play/pause, volume, captions, and fullscreen. - `full` — all of the above plus playback speed and picture-in-picture. |
| `thumbnails` |  | `string` | `''` | A URL pointing to a WebVTT file for timeline thumbnail previews. |
| `src` |  | `string` | `''` | The URL of the video source. For multiple formats, use `<source>` elements instead. |
| `poster` |  | `string` | `''` | Poster image URL |
| `title` |  | `string` | `''` | The video's title. |
| `playing` |  | `boolean` | `false` | Indicates whether the video is currently playing. |
| `muted` |  | `boolean` | `false` | When set, the video will be muted. |
| `volume` |  | `number` | `1` | The video's volume. |
| `duration` |  | `number` | `0` | The total duration of the video in seconds. |
| `currentTime` |  | `number` | `0` | The current playback position in seconds. |
| `autoplay` |  | `boolean` | `false` | Enables autoplay when the component connects. |
| `loop` |  | `boolean` | `false` | Loops the video when playback ends. |
| `autoplay-muted` | `autoplayMuted` | `boolean` | `false` | Enables autoplay in a muted state. |
| `autoplay-on-visible` | `autoplayOnVisible` | `boolean` | `false` | Automatically resumes playback when the player scrolls back into view after being paused by scrolling out. |
| `preload` |  | `'auto' \| 'metadata' \| 'none'` | `'metadata'` | Controls how the browser preloads the video. Defaults to 'metadata' to minimize data usage. |
| `icon-library` | `iconLibrary` | `string` | `'system'` | Icon library used for all built-in control icons. Defaults to 'system'. |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## Methods

| Method | Description | Arguments |
| --- | --- | --- |
| `play` | Starts playback. |  |
| `pause` | Pauses playback. |  |
| `togglePlay` | Toggles between play and pause. |  |
| `toggleMute` | Toggles the muted state. |  |
| `seek` | Seeks to a specific time in the video. | `time: number` |
| `setVolume` | Sets the volume level. | `volume: number` |
| `setPlaybackRate` | Sets the playback rate (speed). | `rate: number` |
| `requestFullscreen` | Enters fullscreen mode. |  |
| `exitFullscreen` | Exits fullscreen mode. |  |
| `getVideoElement` | Gets the native video element. |  |
| `getState` | Gets the current playback state. |  |

## Events

| Event | Description |
| --- | --- |
| `timeupdate` | Emitted when the time changes. |
| `play` | Emitted when playback begins. |
| `pause` | Emitted when playback stops. |
| `volumechange` | Emitted when the volume changes. |
| `error` | Emitted when an error occurs while loading/playing. |
| `ended` | Emitted when playback ends. |
| `loadedmetadata` | Emitted when metadata has been loaded. |

## CSS Parts

| Part | Description |
| --- | --- |
| `base` | The component's base wrapper. |
| `video` | The video element. |
| `controls` | The controls container. |
| `controls-overlay` | The overlay wrapping timeline and controls bar. |
| `timeline` | The timeline/scrubber container. |
| `progress` | The progress bar. |
| `thumbnail` | The thumbnail preview. |
| `poster-overlay` | The poster image overlay. |
| `poster-play-button` | The play button on the poster overlay. |
| `video-title-overlay` | The title text overlay. |
| `caption-overlay` | The custom caption overlay container. |
| `caption` | The caption text element. |
| `timeline-track` | The timeline slider's track (forwarded from wa-slider). |
| `timeline-indicator` | The timeline slider's filled indicator (forwarded from wa-slider). |
| `timeline-thumb` | The timeline slider's thumb (forwarded from wa-slider). |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--controls-color` | `white` | The text and icon color used throughout the controls overlay, title overlay, and mobile controls. |
| `--controls-background` | `var(--wa-color-surface-default)` | The background of the controls bar and mobile controls. |
| `--poster-play-button-background` | `var(--wa-color-surface-default)` | The background of the play button shown over the poster image. Also used to derive the hover state via color-mix(). |
