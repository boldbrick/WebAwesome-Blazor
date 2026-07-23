<!-- Source: https://webawesome.com/docs/components/file-input (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# File Input Component Reference

## Overview

The `<wa-file-input>` component enables users to select files from their device through a dropzone supporting both click and drag-and-drop interactions.

**Status:** Pro component.

## Basic Usage

```html
<wa-file-input label="Select a file"></wa-file-input>
```

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `accept` | string | `''` | A comma-separated list of acceptable file types. |
| `capture` | `'user'` \| `'environment'` | — | On mobile, which camera/microphone to use. |
| `custom-error` | string \| null | null | Manually defined custom error (managed via setCustomValidity). |
| `disabled` | boolean | `false` | Disables the form control. |
| `hint` | string | `''` | The file input's hint. |
| `label` | string | `''` | The file input's label. |
| `multiple` | boolean | `false` | Allows more than one file to be selected. |
| `name` | string \| null | `null` | Name submitted with form data. |
| `required` | boolean | `false` | Makes the file input a required field. |
| `size` | `'small'` \| `'medium'` \| `'large'` | `'medium'` | The file input's size. |

## Slots

| Name | Description |
|------|-------------|
| `dropzone` | Custom content to show in the dropzone. |
| `hint` | Text that describes how to use the file input. |
| `label` | The file input's label. |

## Methods

| Name | Arguments | Description |
|------|-----------|-------------|
| `blur()` | — | Removes focus from the file input. |
| `focus()` | options?: FocusOptions | Sets focus on the file input. |
| `formStateRestoreCallback()` | state, reason | Called when the browser restores element state. |
| `resetValidity()` | — | Remove manual custom errors and native validation. |
| `setCustomValidity()` | message: string | Set a manually defined custom error. |

## Events

| Name | Description |
|------|-------------|
| `blur` | Emitted when the dropzone loses focus. |
| `change` | Emitted when files are added or removed. |
| `focus` | Emitted when the dropzone gains focus. |
| `input` | Emitted when file selection changes. |
| `wa-invalid` | Emitted when constraints aren't satisfied. |

## Dependencies

`<wa-button>`, `<wa-format-bytes>`, `<wa-icon>`, `<wa-spinner>`.
