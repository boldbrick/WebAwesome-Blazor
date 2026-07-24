<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/toast.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/toast -->

# Toast [Pro]

**Full documentation:** https://webawesome.com/docs/components/toast

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-toast>`

ProIncluded with Web Awesome Pro Stable [Feedback](https://webawesome.com/docs/components/?category=feedback) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Toasts display brief, non-blocking notifications that appear temporarily above the page content.

**[Get Toast with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=toast)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Toast!

Adding a single `<wa-toast>` element to the page gives you the ability to dispatch notifications at any time. Toast notifications appear in a stack that renders in the [top layer](https://developer.mozilla.org/en-US/docs/Glossary/Top_layer), showing above everything else on the page.

You can put the `<wa-toast>` element anywhere in the DOM, as long as it's somewhere inside the `<body>`. In most apps, a single toast element is optimal.

```html
<div id="toast-basic">
  <wa-button appearance="filled">Show notification</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-basic');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', () => {
    toast.create('This is a toast notification!');
  });
</script>
```

Toasts have unique accessibility challenges, but these can be mitigated with thoughtful implementation. Read our [Accessibility Considerations](#accessibility-considerations) before using toasts.

This documentation page features numerous toast elements for demonstrative purposes. As a result, it's possible to trigger overlapping toast items. This isn't a bug! Most apps should only have a single `<wa-toast>` element on the page. We're just showing off the many features of this component.

## Examples

Link to This Section

### Variants

Link to This Section

Set the `variant` option to `brand`, `success`, `warning`, `danger`, or `neutral` to change the type of notification.

```html
<div id="toast-variant">
  <div class="wa-cluster wa-gap-xs">
    <wa-button appearance="filled" data-variant="neutral" data-icon="gear">Neutral</wa-button>
    <wa-button appearance="filled" data-variant="brand" data-icon="circle-info">Brand</wa-button>
    <wa-button appearance="filled" data-variant="success" data-icon="check">Success</wa-button>
    <wa-button appearance="filled" data-variant="warning" data-icon="circle-exclamation">Warning</wa-button>
    <wa-button appearance="filled" data-variant="danger" data-icon="triangle-exclamation">Danger</wa-button>
  </div>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-variant');
  const toast = container.querySelector('wa-toast');

  container.addEventListener('click', event => {
    const button = event.target.closest('[data-variant]');
    if (!button) return;

    const variant = button.getAttribute('data-variant');
    const icon = button.getAttribute('data-icon');

    toast.create(`This is a ${variant} notification`, {
      variant,
      icon,
    });
  });
</script>
```

### Sizes

Link to This Section

Set the `size` option to `xs`, `s`, `m`, `l`, or `xl` to change the size of the toast item.

```html
<div id="toast-size">
  <div class="wa-cluster wa-gap-xs">
    <wa-button appearance="filled" data-size="xs" data-icon="shrimp">Extra Small</wa-button>
    <wa-button appearance="filled" data-size="s" data-icon="mouse-field">Small</wa-button>
    <wa-button appearance="filled" data-size="m" data-icon="horse">Medium</wa-button>
    <wa-button appearance="filled" data-size="l" data-icon="elephant">Large</wa-button>
    <wa-button appearance="filled" data-size="xl" data-icon="whale">Extra Large</wa-button>
  </div>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-size');
  const toast = container.querySelector('wa-toast');

  container.addEventListener('click', event => {
    const button = event.target.closest('[data-size]');
    if (!button) return;

    const size = button.getAttribute('data-size');
    const icon = button.getAttribute('data-icon');

    toast.create(`This is a ${size} notification`, {
      size,
      icon,
    });
  });
</script>
```

### With Icons

Link to This Section

Pass an `icon` option to display an icon at the start of the toast item. You can pass a simple string for the icon name, or an object with additional options like `library`, `family`, and `variant`.

```html
<div id="toast-icons">
  <wa-button appearance="filled">Show notification with icon</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-icons');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', () => {
    toast.create('Your message has been sent successfully!', {
      variant: 'success',
      icon: 'paper-plane',
    });
  });
</script>
```

For more control over the icon, pass an object with `name` and optional `library`, `family`, or `variant` properties.

```html
<div id="toast-icons-advanced">
  <div class="wa-cluster wa-gap-xs">
    <wa-button appearance="filled">Duotone icon</wa-button>
    <wa-button appearance="filled">Brand icon</wa-button>
  </div>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-icons-advanced');
  const toast = container.querySelector('wa-toast');
  const buttons = container.querySelectorAll('wa-button');

  buttons[0].addEventListener('click', () => {
    toast.create('This uses a duotone icon!', {
      variant: 'brand',
      icon: {
        name: 'bell',
        family: 'duotone',
        variant: 'solid',
      },
    });
  });

  buttons[1].addEventListener('click', () => {
    toast.create('Follow us on GitHub!', {
      variant: 'neutral',
      icon: {
        name: 'github',
        family: 'brands',
      },
    });
  });
</script>
```

### Duration

Link to This Section

Set the `duration` option to control how long notifications show before disappearing. The value is in milliseconds and defaults to `5000` (5 seconds). A value of `0` will keep the notification open until the user dismisses it.

```html
<div id="toast-duration">
  <div class="wa-cluster wa-gap-xs">
    <wa-button appearance="filled" data-duration="3000">3 seconds</wa-button>
    <wa-button appearance="filled" data-duration="10000">10 seconds</wa-button>
    <wa-button appearance="filled" data-duration="0">Until dismissed</wa-button>
  </div>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-duration');
  const toast = container.querySelector('wa-toast');

  container.addEventListener('click', event => {
    const button = event.target.closest('[data-duration]');
    if (!button) return;

    const duration = parseInt(button.getAttribute('data-duration'));

    toast.create(duration > 0 ? `This will disappear in ${duration / 1000} seconds` : 'Dismiss me manually!', {
      duration,
      variant: 'brand',
    });
  });
</script>
```

### Placement

Link to This Section

Use the `placement` attribute to set the position of the toast stack on the screen.

```html
<div id="toast-placement">
  <wa-select label="Placement" value="top-end">
    <wa-option value="top-start">top-start</wa-option>
    <wa-option value="top-center">top-center</wa-option>
    <wa-option value="top-end">top-end</wa-option>
    <wa-option value="bottom-start">bottom-start</wa-option>
    <wa-option value="bottom-center">bottom-center</wa-option>
    <wa-option value="bottom-end">bottom-end</wa-option>
  </wa-select>
  <br />
  <wa-button appearance="filled">Show notification</wa-button>
  <wa-toast placement="top-end"></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-placement');
  const toast = container.querySelector('wa-toast');
  const select = container.querySelector('wa-select');
  const button = container.querySelector('wa-button');

  select.addEventListener('change', () => {
    toast.placement = select.value;
  });

  button.addEventListener('click', () => {
    toast.create('This notification appears at ' + toast.placement, {
      variant: 'brand',
      icon: 'location-dot',
    });
  });
</script>
```

### Hover and Focus Behavior

Link to This Section

Toast items automatically pause their countdown timer when you hover over them or when the close button receives focus. This gives users more time to read the content before it disappears. When the mouse leaves or focus moves away, the timer resets and starts counting down again.

```html
<div id="toast-pause">
  <wa-button appearance="filled">Show notification (hover to pause)</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-pause');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', () => {
    toast.create('Hover over me to pause the countdown timer!', {
      duration: 5000,
      variant: 'brand',
      icon: 'clock',
    });
  });
</script>
```

### Using HTML Content

Link to This Section

Set `allowHtml` to `true` to render HTML content in notifications. Make sure you trust the content to avoid XSS vulnerabilities.

```html
<div id="toast-html">
  <wa-button appearance="filled">Show notification with HTML</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-html');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', () => {
    toast.create(
      `
      <wa-icon slot="icon" name="bell"></wa-icon>
      <strong>New message!</strong><br>
      You have <em>3 unread messages</em> in your inbox.
    `,
      {
        allowHtml: true,
        variant: 'brand',
        duration: 0,
      },
    );
  });
</script>
```

### Responding to Custom Buttons

Link to This Section

You can add custom buttons or other interactive elements to a toast item using `allowHtml`. Use the returned toast item reference to query for your elements and attach event listeners.

```html
<div id="toast-custom-button">
  <wa-button appearance="filled">Show notification with button</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-custom-button');
  const toast = container.querySelector('wa-toast');
  const showButton = container.querySelector('wa-button');

  showButton.addEventListener('click', async () => {
    const toastItem = await toast.create(
      `
      <wa-icon slot="icon" name="gift"></wa-icon>
      You have a new reward!
      <div style="margin-block-start: var(--wa-space-xs);">
        <wa-button class="claim-button" variant="brand" size="s">Claim</wa-button>
        <wa-button class="dismiss-button" appearance="filled" size="s">No Thanks</wa-button>
      </div>
    `,
      {
        allowHtml: true,
        variant: 'brand',
        duration: 0,
      },
    );

    const claimButton = toastItem.querySelector('.claim-button');
    const dismissButton = toastItem.querySelector('.dismiss-button');

    // When the claim button is clicked
    claimButton.addEventListener('click', () => {
      //
      // Add custom claim logic here
      //
      toastItem.hide();
    });

    // When the dismiss button is clicked
    dismissButton.addEventListener('click', () => {
      toastItem.hide();
    });
  });
</script>
```

### Responding to Events

Link to This Section

The `create()` method returns a promise that resolves to the generated toast item. You can use this reference to add event listeners.

```html
<div id="toast-events">
  <wa-button appearance="filled">Show notification</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-events');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', async () => {
    const toastItem = await toast.create('Click me or let me disappear...', {
      variant: 'brand',
      icon: 'hand-pointer',
    });

    toastItem.addEventListener('click', () => {
      console.log('Toast item was clicked!');
    });

    toastItem.addEventListener('wa-after-hide', () => {
      console.log('Toast item was dismissed');
    });
  });
</script>
```

### Creating Toast Items Manually

Link to This Section

While `toast.create()` is the easiest way to show notifications, you can also create [`<wa-toast-item>`](https://webawesome.com/docs/components/toast-item) elements manually. This approach gives you full control over the toast item's content and is useful when you need to add custom elements or complex layouts.

```html
<div id="toast-append">
  <wa-button appearance="filled">Create toast item manually</wa-button>
  <wa-toast></wa-toast>
</div>

<script>
  const container = document.getElementById('toast-append');
  const toast = container.querySelector('wa-toast');
  const button = container.querySelector('wa-button');

  button.addEventListener('click', () => {
    const toastItem = document.createElement('wa-toast-item');
    toastItem.variant = 'brand';
    toastItem.innerHTML = `
      <wa-icon slot="icon" name="paw"></wa-icon>
      This notification was created manually!
    `;

    toast.prepend(toastItem);
  });
</script>
```

## Accessibility Considerations

Link to This Section

Toasts have a number of accessibility limitations. For example:

-   Due to their transient nature, toasts can be missed entirely by screen magnifier users and difficult to perceive for others depending on their visual and cognitive abilities.
-   Due to their DOM placement and vague position in a page's reading order, toasts can be difficult for keyboard users to navigate to.
-   Due to their positioning on the page, toasts can obscure other essential elements, especially for users who use browser or OS zooming features.

Rarely will toasts offer the best usability for all of your users, so consider if other UX patterns, like dialogs or inline messages, offer a better experience. If toasts are the right fit for your use case, consider these tips:

-   Keep toasts short and sweet.
-   Set the `duration` to `5000` ms (5 seconds) or longer to give users enough time to locate and understand the toast item.
-   Consider allowing users to adjust the timing. While the duration of a toast item resets on hover (see [Hover Behavior](#hover-behavior)), a setting in your application can allow users to control the timing of toasts to best suit their needs and abilities.
-   Choose a consistent placement for toasts in your application and stick with it. Otherwise, users need to guess where transient notifications will appear and risk missing them entirely.
-   If a transient toast item contains an action, ensure that action is available elsewhere on the page. This ensures that users can still execute the action even if they miss the toast.

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

- `(default)` — Place `<wa-toast-item>` elements here to show them as notifications.

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `placement` |  | `\| 'top-start' \| 'top-center' \| 'top-end' \| 'bottom-start' \| 'bottom-center' \| 'bottom-end'` | `'top-end'` | The placement of the toast stack on the screen. |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## Methods

| Method | Description | Arguments |
| --- | --- | --- |
| `create` | Creates a toast notification programmatically and adds it to the stack. Returns a reference to the created toast item element. | `message: string, options: ToastCreateOptions` |

## Custom States

| State | Description |
| --- | --- |
| `visible` | Applied when the toast stack has one or more visible toast items. |

## CSS Parts

| Part | Description |
| --- | --- |
| `stack` | The container that holds the toast items. |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--gap` | `var(--wa-space-s)` | The gap between stacked toast items. |
| `--width` | `28rem` | The width of the toast stack. |
