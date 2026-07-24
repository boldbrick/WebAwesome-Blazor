<!-- Source: reference doc bundled in the Web Awesome 3.10.0 release zip (dist/skills/webawesome/references/components/video-playlist.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/video-playlist -->

# Video Playlist [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-video-playlist>`

ProIncluded with Web Awesome Pro Experimental [Media](https://webawesome.com/docs/components/?category=media) [Since 3.7](https://webawesome.com/docs/resources/changelog#wa_370)

Video playlists wrap multiple [`<wa-video>`](https://webawesome.com/docs/components/video) elements into a playlist with navigation controls.

**[Get Video Playlist with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=video-playlist)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   Responsive [Layout Tools](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma) Newer additions to Web Awesome, like [`<wa-toast>`](https://webawesome.com/docs/components/toast), aren't included in the currently available kit, but a new version is in the works.  
    Track its progress on GitHub.
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Video Playlist!

```html
<wa-video-playlist>
  <wa-video title="Creating a Font Awesome Kit" poster="/assets/images/fa-part-1.jpg">
    <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>

  <wa-video title="Using Kits in Your Projects" poster="/assets/images/fa-part-2.jpg">
    <source src="https://uploads.webawesome.com/02-using-kits-in-your-project.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/02-using-kits-in-your-project.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/kits.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4"
      type="video/mp4"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/teams.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
      type="video/mp4"
    />
  </wa-video>
</wa-video-playlist>
```

## Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.10.0/components/video-playlist/video-playlist.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/video-playlist/video-playlist.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/video-playlist/video-playlist.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaVideoPlaylist from '@awesome.me/webawesome/dist/react/video-playlist/index.js';
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value is
silently ignored and the element falls back to the default slot):

- `(default)` — The default slot. Place `<wa-video>` elements to create a playlist.

## Attributes & Properties

| Property | Attribute | Description | Type | Default |
| --- | --- | --- | --- | --- |
| `controls` | `controls` | The controls preset forwarded to each child `<wa-video>`. | `'none' \| 'standard' \| 'full'` | `'full'` |
| `iconLibrary` | `icon-library` | Icon library used for placeholder icons. | `string` | `'system'` |

## Methods

| Name | Description | Arguments |
| --- | --- | --- |
| `next()` | Plays the next video in the playlist. | — |
| `previous()` | Plays the previous video in the playlist. | — |
| `goTo()` | Jumps to the video at the given index. | `index: number` |

## Events

| Name | Description |
| --- | --- |
| `wa-video-change` | Emitted when the active video changes. |

## CSS Parts

| Name | Description | CSS selector |
| --- | --- | --- |
| \`base\` | The component's base wrapper. | \`::part(base)\` |
| \`playlist\` | The playlist sidebar container. | \`::part(playlist)\` |
| \`playlist-duration\` | The duration text within a playlist item. | \`::part(playlist-duration)\` |
| \`playlist-item\` | An individual playlist item button. | \`::part(playlist-item)\` |
| \`playlist-thumbnail\` | The thumbnail image within a playlist item. | \`::part(playlist-thumbnail)\` |
| \`playlist-title\` | The title text within a playlist item. | \`::part(playlist-title)\` |

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
-   [`<wa-video>`](https://webawesome.com/docs/components/video)

## Examples

### Controls Preset

Use the `controls` attribute to set the controls preset for all videos in the playlist. Accepts the same values as [`<wa-video>`](https://webawesome.com/docs/components/video): `none`, `standard`, or `full` (default).

```html
<wa-video-playlist controls="standard">
  <wa-video title="Creating a Font Awesome Kit" poster="/assets/images/fa-part-1.jpg">
    <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>

  <wa-video title="Using Kits in Your Projects" poster="/assets/images/fa-part-2.jpg">
    <source src="https://uploads.webawesome.com/02-using-kits-in-your-project.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/02-using-kits-in-your-project.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/kits.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4"
      type="video/mp4"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/teams.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
      type="video/mp4"
    />
  </wa-video>
</wa-video-playlist>
```

### Navigating Programmatically

Use the `next()`, `previous()`, and `goTo(index)` methods to navigate between videos programmatically.

```html
<div>
  <wa-video-playlist id="demo-playlist">
    <wa-video title="Creating a Font Awesome Kit" poster="/assets/images/fa-part-1.jpg">
      <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
      <track
        src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
        kind="subtitles"
        srclang="en"
        label="English"
      />
    </wa-video>

    <wa-video title="Using Kits in Your Projects" poster="/assets/images/fa-part-2.jpg">
      <source src="https://uploads.webawesome.com/02-using-kits-in-your-project.mp4" type="video/mp4" />
      <track
        src="https://uploads.webawesome.com/02-using-kits-in-your-project.vtt"
        kind="subtitles"
        srclang="en"
        label="English"
      />
    </wa-video>
    <wa-video title="Using Kits in Your Projects" poster="/assets/images/kits.jpg">
      <source
        src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4"
        type="video/mp4"
      />
    </wa-video>
    <wa-video title="Using Kits in Your Projects" poster="/assets/images/teams.jpg">
      <source
        src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
        type="video/mp4"
      />
    </wa-video>
  </wa-video-playlist>

  <wa-divider></wa-divider>

  <div class="wa-cluster">
    <wa-button appearance="filled" onclick="document.getElementById('demo-playlist').previous()">Previous</wa-button>
    <wa-button appearance="filled" onclick="document.getElementById('demo-playlist').goTo(0)">First</wa-button>
    <wa-button appearance="filled" onclick="document.getElementById('demo-playlist').next()">Next</wa-button>
  </div>
</div>
```

### Listening for Changes

The `wa-video-change` event fires when the active video changes, providing the previous index, current index, and the incoming video's metadata.

```html
<wa-video-playlist id="event-playlist">
  <wa-video title="Creating a Font Awesome Kit" poster="/assets/images/fa-part-1.jpg">
    <source src="https://uploads.webawesome.com/01-create-your-first-kit.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/01-create-your-first-kit.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>

  <wa-video title="Using Kits in Your Projects" poster="/assets/images/fa-part-2.jpg">
    <source src="https://uploads.webawesome.com/02-using-kits-in-your-project.mp4" type="video/mp4" />
    <track
      src="https://uploads.webawesome.com/02-using-kits-in-your-project.vtt"
      kind="subtitles"
      srclang="en"
      label="English"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/kits.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%203%20'Downloading%20Kits'.mp4"
      type="video/mp4"
    />
  </wa-video>
  <wa-video title="Using Kits in Your Projects" poster="/assets/images/teams.jpg">
    <source
      src="https://uploads.webawesome.com/Doing%20More%20with%20FA%20Ep.%202%20'Using%20Teams'.mp4"
      type="video/mp4"
    />
  </wa-video>
</wa-video-playlist>

<div id="event-log" style="margin-top: 1rem; font-size: 0.875rem; color: var(--wa-color-text-quiet);">
  No change yet.
</div>

<script>
  const playlist = document.getElementById('event-playlist');
  const log = document.getElementById('event-log');

  playlist.addEventListener('wa-video-change', event => {
    log.textContent = `Changed from index ${event.detail.previousIndex} → ${event.detail.currentIndex}: "${event.detail.video.title}"`;
  });
</script>
```
