<!-- Source: reference doc bundled in the Web Awesome 3.7.0 release zip (dist/skills/webawesome/references/components/video.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/video -->

# Video [Pro]

**Full documentation:** https://webawesome.com/docs/components/video

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-video>` Experimental Since 3.6 Pro Included with Web Awesome Pro

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
<wa-video title="Web Awesome"  controls="full">
  <source src="https://uploads.webawesome.com/waks_compressed.mp4" type="video/mp4" />
</wa-video>
```

## Examples

### Adding Video Sources

The simplest way to add a video is with the `src` attribute.

```html
<wa-video src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" title="Using Kits in Your Project" poster="/assets/images/fa-part-1.jpg"></wa-video>
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

The video player offers three control presets: `none`, `standard`, and `full`.

#### None

No controls are displayed. The video can still be played programmatically via JavaScript, and the poster overlay and captions remain visible.

```html
<wa-video controls="none" title="My Video" poster="/assets/images/kits.jpg">
  <source src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4" type="video/mp4" />
</wa-video>
```

#### Standard

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

Add a poster image that displays before the video plays. If no `poster` is provided, no overlay is shown and the browser will display the first frame of the video instead.

```html
<wa-video title="Using Kits in Your Project" poster="/assets/images/teams.jpg" controls="full">
  <source src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4" type="video/mp4" />
</wa-video>
```

### Captions and Subtitles

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

Several slots like `poster-icon` and `pause-icon` are provided to let you customize which icons you'd like to show.

```html
<wa-video title="Using Kits in Your Project" poster="/assets/images/teams.jpg" controls="full">
  <wa-icon slot="poster-icon" name="kiwi-bird" family="duotone" variant="solid"></wa-icon>
  <wa-icon slot="play-icon" name="fish" family="duotone" variant="solid"></wa-icon>  
  <wa-icon slot="pause-icon" name="fish-bones" family="duotone" variant="solid"></wa-icon>        
  <source src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4" type="video/mp4" />
</wa-video>
```

### Playlists

To group multiple videos into a playlist, use [`<wa-video-playlist>`](https://webawesome.com/docs/components/video-playlist).

### Video Recommendations

Recommended to ensure fast loading, broad browser compatibility, and the best playback experience across devices.

#### Video Encoding

| Setting | Recommended | Reason |
| --- | --- | --- |
| Codec | H.264 (MP4) | Broadest browser and device support |
| Resolution | 1280×720 (720p) | Good balance of quality and file size |
| Frame rate | 24–30fps | Smooth motion without excess data |
| Bitrate | 2–5 megabits/s | Good quality at 720p without buffering |  

#### Poster Images

| Setting | Recommended | Reason |
| --- | --- | --- |
| Format | JPEG (80–85%) or WebP | Small file size with wide browser support |
| File size | Under 200KB | Fast initial load before video starts |
| Aspect ratio | 16:9 | Matches standard video dimensions |
| Resolution | Match video exactly | Prevents layout shift on load |  

#### Caption Files

| Setting | Recommended | Reason |
| --- | --- | --- |
| Format | WebVTT (.vtt) | \`\` Only format supported by the HTML element |
| Encoding | UTF-8 | Ensures special characters and non-Latin scripts render correctly |
| Timing | Frame accurate | Prevents captions from appearing early or late |  

## Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.7.0/components/video/video.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/video/video.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/video/video.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaVideo from '@awesome.me/webawesome/dist/react/video/index.js';
```

## Slots

Learn more about [using slots](https://webawesome.com/docs/usage/#slots).

| Name | Description |
| --- | --- |
| (default) | \`\` The default slot. Place and  elements for a single video. Alternatively, use the src attribute for a single source. |
| \`controls-after-play\` | \`\` Content inserted immediately after the play/pause button. Used by to inject the next button. |
| \`controls-start\` | \`\` Content inserted at the start of the controls bar (before play/pause). Used by to inject the prev button. |
| \`exit-fullscreen-icon\` | Icon shown on the fullscreen button when in fullscreen. |
| \`fullscreen-icon\` | Icon shown on the fullscreen button when not in fullscreen. |
| \`mute-icon\` | Icon shown on the volume/mute button when muted or volume is 0. |
| \`pause-icon\` | Icon shown on the play/pause button when playing. |
| \`play-icon\` | Icon shown on the play/pause button when paused. |
| \`poster-icon\` | Icon shown on the poster play button. Defaults to a play-circle icon. |
| \`volume-icon\` | Icon shown on the volume/mute button when audio is active. |

## Attributes & Properties

Learn more about [attributes and properties](https://webawesome.com/docs/usage/#attributes-and-properties).

| Name | Description | Reflects |
| --- | --- | --- |
| \`autoplay\` autoplay | \`boolean\` Enables autoplay when the component connects. Type Default false | | |
| \`autoplayMuted\` autoplay-muted | \`boolean\` Enables autoplay in a muted state. Type Default false | | |
| \`autoplayOnVisible\` autoplay-on-visible | \`boolean\` Automatically resumes playback when the player scrolls back into view after being paused by scrolling out. Type Default false | | |
| \`controls\` controls | \`none\` The video's controls preset. - — no controls are shown. - standard — shows the timeline, play/pause, volume, captions, and fullscreen. - full — all of the above plus playback speed and picture-in-picture. Type 'none' \\| 'standard' \\| 'full' Default 'standard' | | |
| \`css\` | \`CSSResultGroup \\| undefined\` One or more CSSResultGroup to include in the component's shadow root. Host styles are automatically prepended. Type Default styles | | |
| \`currentTime\` currentTime | \`number\` The current playback position in seconds. Type Default 0 | | |
| \`duration\` duration | \`number\` The total duration of the video in seconds. Type Default 0 | | |
| \`iconLibrary\` icon-library | \`string\` Icon library used for all built-in control icons. Defaults to 'system'. Type Default 'system' | | |
| \`loop\` loop | \`boolean\` Loops the video when playback ends. Type Default false | | |
| \`muted\` muted | \`boolean\` When set, the video will be muted. Type Default false | | |
| \`playing\` playing | \`boolean\` Indicates whether the video is currently playing. Type Default false | | |
| \`poster\` poster | \`string\` Poster image URL Type Default '' | | |
| \`preload\` preload | \`'auto' \\| 'metadata' \\| 'none'\` Controls how the browser preloads the video. Defaults to 'metadata' to minimize data usage. Type Default 'metadata' | | |
| \`src\` src | \`\` The URL of the video source. For multiple formats, use elements instead. Type string Default '' | | |
| \`thumbnails\` thumbnails | \`string\` A URL pointing to a WebVTT file for timeline thumbnail previews. Type Default '' | | |
| \`title\` title | \`string\` The video's title. Type Default '' | | |
| \`volume\` volume | \`number\` The video's volume. Type Default 1 | | |

## Methods

Learn more about [methods](https://webawesome.com/docs/usage/#methods).

| Name | Description | Arguments |
| --- | --- | --- |
| \`exitFullscreen()\` | Exits fullscreen mode. | |
| \`getState()\` | Gets the current playback state. | |
| \`getVideoElement()\` | Gets the native video element. | |
| \`pause()\` | Pauses playback. | |
| \`play()\` | Starts playback. | |
| \`requestFullscreen()\` | Enters fullscreen mode. | |
| \`seek()\` | Seeks to a specific time in the video. | \`time: number\` |
| \`setPlaybackRate()\` | Sets the playback rate (speed). | \`rate: number\` |
| \`setVolume()\` | Sets the volume level. | \`volume: number\` |
| \`toggleMute()\` | Toggles the muted state. | |
| \`togglePlay()\` | Toggles between play and pause. | |

## Events

Learn more about [events](https://webawesome.com/docs/usage/#events).

| Name | Description |
| --- | --- |
| \`ended\` | Emitted when playback ends. |
| \`error\` | Emitted when an error occurs while loading/playing. |
| \`loadedmetadata\` | Emitted when metadata has been loaded. |
| \`pause\` | Emitted when playback stops. |
| \`play\` | Emitted when playback begins. |
| \`timeupdate\` | Emitted when the time changes. |
| \`volumechange\` | Emitted when the volume changes. |

## CSS custom properties

Learn more about [CSS custom properties](https://webawesome.com/docs/usage/#custom-properties).

| Name | Description |
| --- | --- |
| \`--controls-background\` | \`var(--wa-color-surface-default)\` The background of the controls bar and mobile controls. Default |
| \`--controls-color\` | \`white\` The text and icon color used throughout the controls overlay, title overlay, and mobile controls. Default |
| \`--poster-play-button-background\` | \`var(--wa-color-surface-default)\` The background of the play button shown over the poster image. Also used to derive the hover state via color-mix(). Default |

## CSS parts

Learn more about [CSS parts](https://webawesome.com/docs/usage/#css-parts).

| Name | Description | CSS selector |
| --- | --- | --- |
| \`base\` | The component's base wrapper. | \`::part(base)\` |
| \`caption\` | The caption text element. | \`::part(caption)\` |
| \`caption-overlay\` | The custom caption overlay container. | \`::part(caption-overlay)\` |
| \`controls\` | The controls container. | \`::part(controls)\` |
| \`controls-overlay\` | The overlay wrapping timeline and controls bar. | \`::part(controls-overlay)\` |
| \`poster-overlay\` | The poster image overlay. | \`::part(poster-overlay)\` |
| \`poster-play-button\` | The play button on the poster overlay. | \`::part(poster-play-button)\` |
| \`progress\` | The progress bar. | \`::part(progress)\` |
| \`thumbnail\` | The thumbnail preview. | \`::part(thumbnail)\` |
| \`timeline\` | The timeline/scrubber container. | \`::part(timeline)\` |
| \`timeline-indicator\` | The timeline slider's filled indicator (forwarded from wa-slider). | \`::part(timeline-indicator)\` |
| \`timeline-thumb\` | The timeline slider's thumb (forwarded from wa-slider). | \`::part(timeline-thumb)\` |
| \`timeline-track\` | The timeline slider's track (forwarded from wa-slider). | \`::part(timeline-track)\` |
| \`video\` | The video element. | \`::part(video)\` |
| \`video-title-overlay\` | The title text overlay. | \`::part(video-title-overlay)\` |

## Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-button>`](https://webawesome.com/docs/components/button)
-   [`<wa-dropdown>`](https://webawesome.com/docs/components/dropdown)
-   [`<wa-dropdown-item>`](https://webawesome.com/docs/components/dropdown-item)
-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)
-   [`<wa-popover>`](https://webawesome.com/docs/components/popover)
-   [`<wa-popup>`](https://webawesome.com/docs/components/popup)
-   [`<wa-slider>`](https://webawesome.com/docs/components/slider)
-   [`<wa-spinner>`](https://webawesome.com/docs/components/spinner)
-   [`<wa-tooltip>`](https://webawesome.com/docs/components/tooltip)

**Need a hand?** Report a bug Ask for help