<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/toast-item.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/toast-item -->

# Toast Item [Pro]

**Full documentation:** https://webawesome.com/docs/components/toast-item

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-toast-item>`

ProIncluded with Web Awesome Pro Stable [Feedback](https://webawesome.com/docs/components/?category=feedback) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Toast items are individual notifications displayed within a toast container.

**[Get Toast Item with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=toast-item)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Toast Item!

Toast items are the individual notifications that appear within a [`<wa-toast>`](https://webawesome.com/docs/components/toast) container. This page documents the toast item's anatomy and styling options.

```html
<wa-toast-item variant="brand" duration="0">
  <wa-icon slot="icon" name="bell"></wa-icon>
  This is how a toast item looks!
</wa-toast-item>
```

Toast items are designed to be used within a [`<wa-toast>`](https://webawesome.com/docs/components/toast) container, which manages their lifecycle and positioning. For usage examples showing how to display notifications, see the [Toast documentation](https://webawesome.com/docs/components/toast).

## Examples

Link to This Section

### Variants

Link to This Section

Use the `variant` attribute to change the toast item's visual style. The variant determines the accent color on the left side and the icon color. Available variants are `neutral` (default), `brand`, `success`, `warning`, and `danger`.

```html
<div class="wa-stack">
  <wa-toast-item variant="neutral" duration="0">
    <wa-icon slot="icon" name="gear"></wa-icon>
    Neutral variant (default)
  </wa-toast-item>

  <wa-toast-item variant="brand" duration="0">
    <wa-icon slot="icon" name="circle-info"></wa-icon>
    Brand variant
  </wa-toast-item>

  <wa-toast-item variant="success" duration="0">
    <wa-icon slot="icon" name="check"></wa-icon>
    Success variant
  </wa-toast-item>

  <wa-toast-item variant="warning" duration="0">
    <wa-icon slot="icon" name="circle-exclamation"></wa-icon>
    Warning variant
  </wa-toast-item>

  <wa-toast-item variant="danger" duration="0">
    <wa-icon slot="icon" name="triangle-exclamation"></wa-icon>
    Danger variant
  </wa-toast-item>
</div>
```

### Sizes

Link to This Section

Use the `size` attribute to change the toast item's size.

```html
<div class="wa-stack">
  <wa-toast-item size="xs" duration="0">
    <wa-icon slot="icon" name="shrimp"></wa-icon>
    Extra Small
  </wa-toast-item>

  <wa-toast-item size="s" duration="0">
    <wa-icon slot="icon" name="mouse-field"></wa-icon>
    Small
  </wa-toast-item>

  <wa-toast-item size="m" duration="0">
    <wa-icon slot="icon" name="horse"></wa-icon>
    Medium (default)
  </wa-toast-item>

  <wa-toast-item size="l" duration="0">
    <wa-icon slot="icon" name="elephant"></wa-icon>
    Large
  </wa-toast-item>

  <wa-toast-item size="xl" duration="0">
    <wa-icon slot="icon" name="whale"></wa-icon>
    Extra Large
  </wa-toast-item>
</div>
```

### Icons

Link to This Section

Use the `icon` slot to display an icon at the start of the toast item. The icon color automatically matches the variant's accent color.

```html
<div class="wa-stack">
  <wa-toast-item variant="success" duration="0">
    <wa-icon slot="icon" name="check"></wa-icon>
    Your changes have been saved!
  </wa-toast-item>

  <wa-toast-item variant="brand" duration="0">
    <wa-icon slot="icon" name="envelope"></wa-icon>
    You have 3 new messages
  </wa-toast-item>

  <wa-toast-item variant="warning" duration="0">
    <wa-icon slot="icon" name="clock"></wa-icon>
    Your session will expire soon
  </wa-toast-item>
</div>
```

Toast items work fine without icons too.

```html
<wa-toast-item variant="neutral" duration="0"> A simple notification without an icon. </wa-toast-item>
```

### Rich Content

Link to This Section

The default slot accepts any HTML content, allowing you to create rich notifications with formatted text, links, and interactive elements.

```html
<div class="wa-stack">
  <wa-toast-item variant="brand" duration="0">
    <wa-icon slot="icon" name="bell"></wa-icon>
    <strong>New message from Alex</strong><br />
    Hey, are you available for a quick call?
  </wa-toast-item>

  <wa-toast-item variant="success" duration="0">
    <wa-icon slot="icon" name="cloud-arrow-up"></wa-icon>
    <strong>Upload complete</strong><br />
    <a href="#">View file</a> · <a href="#">Share</a>
  </wa-toast-item>

  <wa-toast-item variant="brand" duration="0">
    <wa-icon slot="icon" name="gift"></wa-icon>
    You've earned a reward!
    <div style="margin-block-start: var(--wa-space-xs);">
      <wa-button variant="brand" size="s">Claim Now</wa-button>
      <wa-button appearance="filled" size="s">Later</wa-button>
    </div>
  </wa-toast-item>
</div>
```

### Duration

Link to This Section

The `duration` attribute controls how long the toast item displays before automatically dismissing (in milliseconds). The default is `5000` (5 seconds). Set to `0` to disable auto-dismissal.

When a duration is set, a progress ring appears around the close button showing the remaining time.

```html
<wa-toast-item variant="brand" duration="5000">...</wa-toast-item>
<wa-toast-item variant="brand" duration="10000">...</wa-toast-item>
<wa-toast-item variant="brand" duration="0">...</wa-toast-item>
```

### Hover Behavior

Link to This Section

Toast items automatically pause their countdown timer when the user hovers over them, giving more time to read the content. When the mouse leaves, the timer resets and begins counting down again.

### The Close Button

Link to This Section

Every toast item includes a close button that allows users to dismiss the notification. When `duration` is greater than `0`, the close button displays a progress ring showing the remaining time.

```html
<wa-toast-item variant="neutral" duration="0">
  <wa-icon slot="icon" name="circle-info"></wa-icon>
  Click the close button on the right to dismiss →
</wa-toast-item>
```

### Customizing the Accent

Link to This Section

Use the `--accent-width` custom property to adjust the width of the accent line, or hide it entirely.

```html
<div class="wa-stack">
  <wa-toast-item variant="brand" duration="0" style="--accent-width: 8px;">
    <wa-icon slot="icon" name="star"></wa-icon>
    Thicker accent line
  </wa-toast-item>

  <wa-toast-item variant="success" duration="0" style="--accent-width: 0;">
    <wa-icon slot="icon" name="check"></wa-icon>
    No accent line
  </wa-toast-item>
</div>
```

### Customizing the Padding

Link to This Section

Use the `--padding` custom property to adjust the internal spacing.

```html
<div class="wa-stack">
  <wa-toast-item variant="brand" duration="0" style="--padding: var(--wa-space-xs);">
    <wa-icon slot="icon" name="compress"></wa-icon>
    Compact padding
  </wa-toast-item>

  <wa-toast-item variant="brand" duration="0" style="--padding: var(--wa-space-xl);">
    <wa-icon slot="icon" name="expand"></wa-icon>
    Spacious padding
  </wa-toast-item>
</div>
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

- `(default)` — The toast item's message content.
- `icon` — An optional icon to show at the start of the toast item.

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `variant` |  | `'brand' \| 'success' \| 'warning' \| 'danger' \| 'neutral'` | `'neutral'` | The toast item's variant. |
| `size` |  | `'xs' \| 's' \| 'm' \| 'l' \| 'xl' \| 'small' \| 'medium' \| 'large'` | `'m'` | The toast item's size. |
| `duration` |  | `number` | `5000` | The length of time in milliseconds before the toast item is automatically dismissed. Set to 0 to keep the toast item open until the user dismisses it. |
| `with-icon` | `withIcon` | `boolean` | `false` | Only required for SSR. Set to `true` if you're slotting in an `icon` element so the server-rendered markup includes the icon before the component hydrates on the client. |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## Methods

| Method | Description | Arguments |
| --- | --- | --- |
| `hide` | Hides the toast item with animation and removes it from the DOM. |  |

## Events

| Event | Description |
| --- | --- |
| `wa-show` | Emitted when the toast item begins to show. |
| `wa-after-show` | Emitted after the toast item has finished showing. |
| `wa-hide` | Emitted when the toast item begins to hide. |
| `wa-after-hide` | Emitted after the toast item has finished hiding. |

## CSS Parts

| Part | Description |
| --- | --- |
| `toast-item` | The toast item's main container. |
| `accent` | The colored accent line on the start side. |
| `icon` | The icon container. |
| `content` | The message content container. |
| `close-button` | The close button element. |
| `progress-ring` | The progress ring component. |
| `progress-ring__base` | The progress ring's exported base part. |
| `progress-ring__label` | The progress ring's exported label part. |
| `progress-ring__track` | The progress ring's exported track part. |
| `progress-ring__indicator` | The progress ring's exported indicator part. |
| `close-icon` | The close icon element. |
| `close-icon__svg` | The close icon's exported svg part. |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--accent-width` |  | The width of the accent line. Defaults to 4px. |
| `--show-duration` | `var(--wa-transition-normal)` | The animation duration when showing. |
| `--hide-duration` | `var(--wa-transition-normal)` | The animation duration when hiding. |
