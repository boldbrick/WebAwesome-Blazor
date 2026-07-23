<!-- Source: https://webawesome.com/docs/components/file-input (fetched 2026-07-23 for Web Awesome 3.2.0).
     The public GitHub docs repo does not yet include this component; content mirrored from the public web docs.
     The CEM API surface (temp/wa-api/surface_3.2.0.json) is authoritative where this reference differs. -->

# File Input

The `<wa-file-input>` component allows users to select one or more files from their device using a dropzone that supports both click and drag-and-drop interactions.

Available since 3.2 (experimental).

## Slots

| Name      | Description                                                                                 |
|-----------|---------------------------------------------------------------------------------------------|
| dropzone  | Custom content to show in the dropzone.                                                      |
| file-icon | Custom icon for non-image files.                                                             |
| hint      | Text that describes how to use the file input. Alternatively, you can use the `hint` attribute. |
| label     | The file input's label. Alternatively, you can use the `label` attribute.                    |

## Attributes & Properties

| Name       | Type                          | Default   | Description |
|------------|-------------------------------|-----------|-------------|
| accept     | string                        | `''`      | A comma-separated list of acceptable file types. |
| hint       | string                        | `''`      | The file input's hint. If you need to display HTML, use the `hint` slot instead. |
| label      | string                        | `''`      | The file input's label. If you need to display HTML, use the `label` slot instead. |
| multiple   | boolean                       | `false`   | Allows more than one file to be selected. |
| required   | boolean                       | `false`   | Makes the file input a required field. |
| size       | `'small' \| 'medium' \| 'large'` | `'medium'` | The file input's size. |
| with-hint  | boolean                       | `false`   | Used for SSR. Determines if the SSRed component will have the hint slot rendered on initial paint. |
| with-label | boolean                       | `false`   | Used for SSR. Determines if the SSRed component will have the label slot rendered on initial paint. |

## Methods

| Name    | Description                          | Arguments             |
|---------|--------------------------------------|-----------------------|
| blur()  | Removes focus from the file input.   | —                     |
| focus() | Sets focus on the file input.        | `options?: FocusOptions` |

## Events

| Name       | Description |
|------------|-------------|
| blur       | Emitted when the dropzone loses focus. |
| focus      | Emitted when the dropzone gains focus. |
| change     | Emitted when files are added or removed. |
| input      | Emitted when file selection changes. |
| wa-invalid | Emitted when the form control has been checked for validity and its constraints aren't satisfied. |

## CSS Parts

`base` (deprecated), `dropzone`, `dropzone-icon`, `dropzone-text`, `file`, `file-details`, `file-icon`, `file-image`, `file-list`, `file-name`, `file-size`, `file-thumbnail`, `hint`, `label`, `remove-button`.

## Dependencies

Automatically imports `<wa-button>`, `<wa-format-bytes>`, `<wa-icon>`, `<wa-spinner>`.
