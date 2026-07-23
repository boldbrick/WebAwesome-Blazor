<!-- Source: https://webawesome.com/docs/components/toast-item (fetched 2026-07-23 for Web Awesome 3.3.0; public web docs — component absent from the public GitHub docs tree). Note: the target CEM (expected-api-surface.json) is authoritative for the wrapper surface; it lists size as 'small' | 'medium' | 'large' (default 'medium') and does not expose a withIcon attribute. -->

# Toast Item Component Reference

## Component Summary

The `<wa-toast-item>` component represents individual notifications displayed within a `<wa-toast>` container that manages their lifecycle and positioning.

**Available Since:** 3.3

## Primary Usage Example

```html
<wa-toast-item variant="brand" duration="0">
  <wa-icon slot="icon" name="bell"></wa-icon>
  This is how a toast item looks!
</wa-toast-item>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | The toast item's message content. |
| icon | An optional icon to show at the start of the toast item. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `duration` | number | 5000 | Length of time in milliseconds before auto-dismissal. Set to 0 to keep open until user dismisses it. |
| `size` | 'small' \| 'medium' \| 'large' | 'medium' | The toast item's size. (CEM-authoritative values.) |
| `variant` | 'brand' \| 'success' \| 'warning' \| 'danger' \| 'neutral' | 'neutral' | The toast item's visual style variant. |

## Methods

| Name | Description | Arguments |
|------|-------------|-----------|
| `hide()` | Hides the toast item with animation and removes it from the DOM. | None |

## Events

| Name | Description |
|------|-------------|
| `wa-show` | Emitted when the toast item begins to show. |
| `wa-after-show` | Emitted after the toast item has finished showing. |
| `wa-hide` | Emitted when the toast item begins to hide. |
| `wa-after-hide` | Emitted after the toast item has finished hiding. |

## CSS Parts

`toast-item`, `accent`, `icon`, `content`, `close-button`, `close-icon`, `close-icon__svg`, `progress-ring`, `progress-ring__base`, `progress-ring__indicator`, `progress-ring__track`, `progress-ring__label`.

## Dependencies

- `<wa-icon>`
- `<wa-progress-ring>`
