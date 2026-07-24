<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/file-input.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/file-input -->

# File Input [Pro]

**Full documentation:** https://webawesome.com/docs/components/file-input

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-file-input>`

ProIncluded with Web Awesome Pro Stable [Forms](https://webawesome.com/docs/components/?category=forms) [Since 3.2](https://webawesome.com/docs/resources/changelog#wa_320)

File inputs allow users to select files from their device.

**[Get File Input with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=file-input)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + File Input!

File inputs allow users to select one or more files from their device using a dropzone that supports both click and drag-and-drop interactions.

```html
<wa-file-input label="Select a file"></wa-file-input>
```

This component works with standard `<form>` elements. Please refer to the section on [form controls](https://webawesome.com/docs/form-controls) to learn more about form submission and client-side validation.

## Examples

Link to This Section

### Labels

Link to This Section

Use the `label` attribute to give the file input an accessible label. For labels that contain HTML, use the `label` slot instead.

```html
<wa-file-input label="Upload your resume"></wa-file-input>
```

### Hints

Link to This Section

Add descriptive help text with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

```html
<wa-file-input label="Profile photo" hint="Upload a photo to personalize your account."></wa-file-input>
```

### Multiple Files

Link to This Section

Add the `multiple` attribute to allow the file input to accept more than one file. If the user drops a folder, all files within it will be added to the file input.

```html
<wa-file-input label="Upload documents" hint="You can select multiple files at once." multiple></wa-file-input>
```

### Accepting File Types

Link to This Section

Use the `accept` attribute to limit the file input to certain file types. Set it to a comma-separated string of [unique file type specifiers](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#unique_file_type_specifiers).

```html
<wa-file-input
  label="Upload an image"
  hint="Only JPEG, PNG, GIF, and WebP images are allowed."
  accept="image/jpeg, image/png, image/gif, image/webp"
></wa-file-input>
```

You can also use file extensions such as `accept="pdf, .doc, .docx"`.

```html
<wa-file-input label="Upload a document" hint="PDF and Word documents only." accept=".pdf, .doc, .docx"></wa-file-input>
```

### Capturing from a Camera

Link to This Section

On mobile devices, use the `capture` attribute to capture media directly from the device's camera or microphone instead of selecting an existing file. Set it to `user` for the front-facing camera or `environment` for the rear-facing one.

The `capture` attribute is only used when `accept` includes an image, video, or audio type. On desktop browsers and devices without the corresponding hardware, it is ignored and the file input behaves normally.

```html
<wa-file-input
  label="Take a photo"
  hint="Opens the rear-facing camera on supported mobile devices."
  accept="image/*"
  capture="environment"
></wa-file-input>
```

### Disabled

Link to This Section

Use the `disabled` attribute to disable the file input.

```html
<wa-file-input label="Upload disabled" disabled></wa-file-input>
```

### Sizes

Link to This Section

Use the `size` attribute to change the file input's size.

```html
<wa-file-input label="Extra Small" size="xs"></wa-file-input>
<br />
<wa-file-input label="Small" size="s"></wa-file-input>
<br />
<wa-file-input label="Medium" size="m"></wa-file-input>
<br />
<wa-file-input label="Large" size="l"></wa-file-input>
<br />
<wa-file-input label="Extra Large" size="xl"></wa-file-input>
```

### Custom Dropzone Content

Link to This Section

Use the `dropzone` slot to customize what appears inside the dropzone area.

```html
<wa-file-input label="Upload files" multiple>
  <div slot="dropzone" style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
    <wa-icon name="cloud-arrow-up" style="font-size: 2.5rem;"></wa-icon>
    <strong>Drag and drop your files here</strong>
    <span style="color: var(--wa-color-neutral-on-quiet);">or click to browse</span>
  </div>
</wa-file-input>
```

### Working with Files

Link to This Section

The `files` property gives you access to an array of selected files. Unlike the native file input's `FileList`, this is a standard JavaScript array, making it easier to manipulate.

```html
<wa-file-input
  id="file-input-demo"
  label="Select some files"
  hint="Try the buttons below after selecting files."
  multiple
></wa-file-input>

<br />

<wa-button id="reverse-btn" appearance="filled">Reverse Order</wa-button>
<wa-button id="clear-btn" appearance="filled">Clear All</wa-button>
<wa-button id="log-btn" appearance="filled">Log Files</wa-button>

<script>
  const fileInput = document.getElementById('file-input-demo');
  const reverseBtn = document.getElementById('reverse-btn');
  const clearBtn = document.getElementById('clear-btn');
  const logBtn = document.getElementById('log-btn');

  reverseBtn.addEventListener('click', () => {
    fileInput.files = fileInput.files.toReversed();
  });

  clearBtn.addEventListener('click', () => {
    fileInput.files = [];
  });

  logBtn.addEventListener('click', () => {
    console.log('Selected files:', fileInput.files);
  });
</script>
```

The `files` property must be reassigned, not mutated! Avoid using functions that mutate the array, such as `reverse()` and `sort()`, because they won't trigger an update. Use non-mutating alternatives like `toReversed()` and `toSorted()` instead.

### Uploading with Forms

Link to This Section

When uploading files from a form, add `method="post"` and `enctype="multipart/form-data"` to the form so files are sent correctly to the server.

```html
<form id="upload-form" method="post" enctype="multipart/form-data" action="about:blank">
  <wa-file-input name="documents" label="Select files to upload" multiple></wa-file-input>
  <br />
  <wa-button appearance="filled" type="submit" variant="brand">Upload</wa-button>
</form>

<script>
  const form = document.getElementById('upload-form');

  form.addEventListener('submit', event => {
    event.preventDefault();
    const formData = new FormData(form);
    console.log('Files to upload:', [...formData.getAll('documents')]);
  });
</script>
```

### Required Validation

Link to This Section

Add the `required` attribute to make file selection mandatory. Form submission will be blocked until a file is selected.

```html
<form id="required-form" action="about:blank" method="get">
  <wa-file-input name="file" label="Required file" hint="You must select a file to submit." required></wa-file-input>
  <br />
  <wa-button appearance="filled" type="submit" variant="brand">Submit</wa-button>
  <wa-button type="reset" appearance="filled">Reset</wa-button>
</form>
```

### Custom Validation

Link to This Section

Use the `setCustomValidity()` method to set a custom error message. This will override standard validation and prevent form submission. Clear the error by passing an empty string.

```html
<form id="custom-validation-form" action="about:blank" method="get">
  <wa-file-input
    id="custom-file-input"
    name="file"
    label="Upload a small file"
    hint="Files must be smaller than 500 KB."
  ></wa-file-input>
  <br />
  <wa-button appearance="filled" type="submit" variant="brand">Submit</wa-button>
  <wa-button type="reset" appearance="filled">Reset</wa-button>
</form>

<script type="module">
  const form = document.getElementById('custom-validation-form');
  const fileInput = document.getElementById('custom-file-input');
  const maxSize = 500 * 1024; // 500 KB

  fileInput.addEventListener('change', () => {
    const tooLarge = fileInput.files.some(file => file.size > maxSize);

    if (tooLarge) {
      fileInput.setCustomValidity('One or more files exceed the 500 KB limit.');
    } else {
      fileInput.setCustomValidity('');
    }
  });

  // Don't actually submit in the demo
  form.addEventListener('submit', event => {
    event.preventDefault();
  });
</script>
```

### Styling Validation States

Link to This Section

Use the `:state(user-valid)` and `:state(user-invalid)` custom states to style the file input based on its validation status. These states only apply after the user has interacted with the control or attempted to submit the form.

```html
<form id="styling-form" action="about:blank" method="get" class="validation-styles">
  <wa-file-input name="file" label="Select a file" required></wa-file-input>
  <br />
  <wa-button appearance="filled" type="submit" variant="brand">Submit</wa-button>
  <wa-button type="reset" appearance="filled">Reset</wa-button>
</form>

<style>
  .validation-styles wa-file-input:state(user-valid) {
    outline: solid 2px var(--wa-color-success-fill-loud);
    outline-offset: 0.5rem;
  }

  .validation-styles wa-file-input:state(user-invalid) {
    outline: solid 2px var(--wa-color-danger-fill-loud);
    outline-offset: 0.5rem;
  }
</style>
```

You can also style based on the `:state(blank)` and `:state(dragging)` states:

```html
<wa-file-input class="drag-styles" label="Watch the border change while dragging" multiple></wa-file-input>

<style>
  .drag-styles::part(dropzone) {
    transition: transform 0.2s ease;
  }

  .drag-styles:state(dragging)::part(dropzone) {
    transform: scale(1.02);
  }
</style>
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

- `label` — The file input's label. Alternatively, you can use the `label` attribute.
- `hint` — Text that describes how to use the file input. Alternatively, you can use the `hint` attribute.
- `dropzone` — Custom content to show in the dropzone.

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `size` |  | `'xs' \| 's' \| 'm' \| 'l' \| 'xl' \| 'small' \| 'medium' \| 'large'` | `'m'` | The file input's size. |
| `label` |  | `string` | `''` | The file input's label. If you need to display HTML, use the `label` slot instead. |
| `hint` |  | `string` | `''` | The file input's hint. If you need to display HTML, use the `hint` slot instead. |
| `multiple` |  | `boolean` | `false` | Allows more than one file to be selected. |
| `accept` |  | `string` | `''` | A comma-separated list of acceptable file types. Must be a list of [unique file type specifiers](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#unique_file_type_specifiers). |
| `required` |  | `boolean` | `false` | Makes the file input a required field. |
| `capture` |  | `'user' \| 'environment'` |  | On mobile devices, specifies which camera or microphone to use for capturing media. Use `user` for the front-facing camera/microphone or `environment` for the rear-facing one. This attribute is only used when `accept` includes an image, video, or audio type and may be ignored on devices that lack the corresponding hardware. |
| `with-label` | `withLabel` | `boolean` | `false` | Only required for SSR. Set to `true` if you're slotting in a `label` element so the server-rendered markup includes the label before the component hydrates on the client. |
| `with-hint` | `withHint` | `boolean` | `false` | Only required for SSR. Set to `true` if you're slotting in a `hint` element so the server-rendered markup includes the hint before the component hydrates on the client. |
| `name` |  | `string \| null` | `null` | The name of the input, submitted as a name/value pair with form data. |
| `disabled` |  | `boolean` | `false` | Disables the form control. |
| `custom-error` | `customError` | `string \| null` | `null` |  |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## Methods

| Method | Description | Arguments |
| --- | --- | --- |
| `focus` | Sets focus on the file input. | `options: FocusOptions` |
| `blur` | Removes focus from the file input. |  |
| `setCustomValidity` | Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators. | `message: string` |
| `formStateRestoreCallback` | Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue. | `state: string \| File \| FormData \| null, reason: 'autocomplete' \| 'restore'` |
| `resetValidity` | Reset validity is a way of removing manual custom errors and native validation. |  |

## Events

| Event | Description |
| --- | --- |
| `input` | Emitted when file selection changes. |
| `change` | Emitted when files are added or removed. |
| `focus` | Emitted when the dropzone gains focus. |
| `blur` | Emitted when the dropzone loses focus. |
| `wa-invalid` | Emitted when the form control has been checked for validity and its constraints aren't satisfied. |

## Custom States

| State | Description |
| --- | --- |
| `blank` | No files selected. |
| `dragging` | Files being dragged over dropzone. |

## CSS Parts

| Part | Description |
| --- | --- |
| `label` | The label element. |
| `hint` | The hint element. |
| `base` | The main component wrapper. |
| `dropzone` | The drag-and-drop area. |
| `dropzone-icon` | The upload icon in the dropzone. |
| `dropzone-text` | The instruction text in the dropzone. |
| `file-list` | The container for selected files. |
| `file` | Individual file item container. |
| `file-thumbnail` | The thumbnail/icon container for a file. |
| `file-image` | The image element for image thumbnails. |
| `file-icon` | The icon for non-image files. |
| `file-details` | Container for file name and size. |
| `file-name` | The file name text. |
| `file-size` | The file size text. |
| `remove-button` | The remove button for each file. |
