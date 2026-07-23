<!-- Source: https://webawesome.com/docs/components/toast (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Toast Component Reference

## Component Summary

The `<wa-toast>` component displays brief, non-blocking notifications that appear temporarily above page content. Toast notifications stack and render in the top layer, appearing above all other page elements. A single toast element can manage multiple notifications, and it may be placed anywhere within the `<body>`.

**Available Since:** 3.3

## Primary Usage Example

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

## Attributes & Properties

| Name | Type | Default | Reflects | Description |
|------|------|---------|----------|-------------|
| placement | `'top-start' \| 'top-center' \| 'top-end' \| 'bottom-start' \| 'bottom-center' \| 'bottom-end'` | `'top-end'` | Yes | The placement of the toast stack on the screen. |

## Slots

| Name | Description |
|------|-------------|
| (default) | Place `<wa-toast-item>` elements here to show them as notifications. |

## Methods

| Name | Description | Arguments |
|------|-------------|-----------|
| `create()` | Creates a toast notification programmatically and adds it to the stack. Returns a reference to the created toast item element. | `message: string, options?: ToastCreateOptions` |

## CSS Custom Properties

| Name | Default | Description |
|------|---------|-------------|
| `--gap` | `var(--wa-space-s)` | The gap between stacked toast items. |
| `--width` | `28rem` | The width of the toast stack. |

## CSS Parts

| Name | Description |
|------|-------------|
| `stack` | The container that holds the toast items. |

## Dependencies

- `<wa-icon>`
- `<wa-progress-ring>`
- `<wa-toast-item>`
