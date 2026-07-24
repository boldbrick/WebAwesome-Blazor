<!-- Source: reference doc bundled in the Web Awesome 3.7.0 release zip (dist/skills/webawesome/references/components/file-input.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/file-input -->

# File Input [Pro]

**Full documentation:** https://webawesome.com/docs/components/file-input

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-file-input>` Stable Since 3.2 Pro Included with Web Awesome Pro

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

### Labels

Use the `label` attribute to give the file input an accessible label. For labels that contain HTML, use the `label` slot instead.

```html
<wa-file-input label="Upload your resume"></wa-file-input>
```

### Hints

Add descriptive help text with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

```html
<wa-file-input
  label="Profile photo"
  hint="Upload a photo to personalize your account."
></wa-file-input>
```

### Multiple Files

Add the `multiple` attribute to allow the file input to accept more than one file. If the user drops a folder, all files within it will be added to the file input.

```html
<wa-file-input
  label="Upload documents"
  hint="You can select multiple files at once."
  multiple
></wa-file-input>
```

### Accepting File Types

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
<wa-file-input
  label="Upload a document"
  hint="PDF and Word documents only."
  accept=".pdf, .doc, .docx"
></wa-file-input>
```

### Disabled

Use the `disabled` attribute to disable the file input.

```html
<wa-file-input label="Upload disabled" disabled></wa-file-input>
```

### Sizes

Use the `size` attribute to change the file input's size.

```html
<wa-file-input label="Extra Small" size="xs"></wa-file-input>
<br>
<wa-file-input label="Small" size="s"></wa-file-input>
<br>
<wa-file-input label="Medium" size="m"></wa-file-input>
<br>
<wa-file-input label="Large" size="l"></wa-file-input>
<br>
<wa-file-input label="Extra Large" size="xl"></wa-file-input>
```

### Custom Dropzone Content

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

The `files` property gives you access to an array of selected files. Unlike the native file input's `FileList`, this is a standard JavaScript array, making it easier to manipulate.

```html
<wa-file-input
  id="file-input-demo"
  label="Select some files"
  hint="Try the buttons below after selecting files."
  multiple
></wa-file-input>

<br>

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

When uploading files from a form, add `method="post"` and `enctype="multipart/form-data"` to the form so files are sent correctly to the server.

```html
<form
  id="upload-form"
  method="post"
  enctype="multipart/form-data"
  action="about:blank"
>
  <wa-file-input
    name="documents"
    label="Select files to upload"
    multiple
  ></wa-file-input>
  <br>
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

Add the `required` attribute to make file selection mandatory. Form submission will be blocked until a file is selected.

```html
<form id="required-form" action="about:blank" method="get">
  <wa-file-input
    name="file"
    label="Required file"
    hint="You must select a file to submit."
    required
  ></wa-file-input>
  <br>
  <wa-button appearance="filled" type="submit" variant="brand">Submit</wa-button>
  <wa-button type="reset" appearance="filled">Reset</wa-button>
</form>
```

### Custom Validation

Use the `setCustomValidity()` method to set a custom error message. This will override standard validation and prevent form submission. Clear the error by passing an empty string.

```html
<form id="custom-validation-form" action="about:blank" method="get">
  <wa-file-input
    id="custom-file-input"
    name="file"
    label="Upload a small file"
    hint="Files must be smaller than 500 KB."
  ></wa-file-input>
  <br>
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

Use the `:state(user-valid)` and `:state(user-invalid)` custom states to style the file input based on its validation status. These states only apply after the user has interacted with the control or attempted to submit the form.

```html
<form id="styling-form" action="about:blank" method="get" class="validation-styles">
  <wa-file-input
    name="file"
    label="Select a file"
    required
  ></wa-file-input>
  <br>
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
<wa-file-input
  class="drag-styles"
  label="Watch the border change while dragging"
  multiple
></wa-file-input>

<style>
  .drag-styles::part(dropzone) {
    transition: transform 0.2s ease;
  }

  .drag-styles:state(dragging)::part(dropzone) {
    transform: scale(1.02);
  }
</style>
```

## Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.7.0/components/file-input/file-input.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/file-input/file-input.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/file-input/file-input.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaFileInput from '@awesome.me/webawesome/dist/react/file-input/index.js';
```

## Slots

Learn more about [using slots](https://webawesome.com/docs/usage/#slots).

| Name | Description |
| --- | --- |
| \`dropzone\` | Custom content to show in the dropzone. |
| \`hint\` | \`hint\` Text that describes how to use the file input. Alternatively, you can use the attribute. |
| \`label\` | \`label\` The file input's . Alternatively, you can use the label attribute. |

## Attributes & Properties

Learn more about [attributes and properties](https://webawesome.com/docs/usage/#attributes-and-properties).

| Name | Description | Reflects |
| --- | --- | --- |
| \`accept\` accept | \`string\` A comma-separated list of acceptable file types. Must be a list of unique file type specifiers. Type Default '' | | |
| \`css\` | \`CSSResultGroup \\| undefined\` One or more CSSResultGroup to include in the component's shadow root. Host styles are automatically prepended. Type Default \[sizeStyles, formControlStyles, styles\] | | |
| \`disabled\` disabled | \`boolean\` Disables the form control. Type Default false | | |
| \`dragging\` | \`boolean\` Whether files are being dragged over the dropzone. Type Default false | | |
| \`fileCount\` | \`number\` The of selected files. Used for validation. Type number | | |
| \`files\` | \`File\[\]\` The selected files. Type Default \[\] | | |
| \`form\` | \`

\` By default, form controls are associated with the nearest containing element. This attribute allows you to place the form control outside of a form and associate it with the form that has this id. The form must be in the same document or shadow root for this to work. Type HTMLFormElement \\| null | | |
| \`hint\` hint | \`hint\` The file input's . If you need to display HTML, use the hint slot instead. Type string Default '' | | |
| \`label\` label | \`label\` The file input's . If you need to display HTML, use the label slot instead. Type string Default '' | | |
| \`multiple\` multiple | \`boolean\` Allows more than one file to be selected. Type Default false | | |
| \`name\` name | \`string \\| null\` The name of the input, submitted as a name/value pair with form data. Type Default null | | |
| \`required\` required | \`boolean\` Makes the file input a required field. Type Default false | | |
| \`size\` size | \`'xs' \\| 's' \\| 'm' \\| 'l' \\| 'xl' \\| 'small' \\| 'medium' \\| 'large'\` The file input's size. Type Default 'm' | | |
| \`validationTarget\` | \`undefined \\| HTMLElement\` Override this to change where constraint validation popups are anchored. Type | | |
| \`validators\` | \`observedAttributes\` Validators are static because they have , essentially attributes to "watch" for changes. Whenever these attributes change, we want to be notified and update the validator. Type Validator\[\] Default \[\] | | |
| \`withHint\` with-hint | \`true\` Only required for SSR. Set to if you're slotting in a hint element so the server-rendered markup includes the hint before the component hydrates on the client. Type boolean Default false | | |
| \`withLabel\` with-label | \`true\` Only required for SSR. Set to if you're slotting in a label element so the server-rendered markup includes the label before the component hydrates on the client. Type boolean Default false | | |

## Methods

Learn more about [methods](https://webawesome.com/docs/usage/#methods).

| Name | Description | Arguments |
| --- | --- | --- |
| \`blur()\` | Removes focus from the file input. | |
| \`focus()\` | Sets focus on the file input. | \`options: FocusOptions\` |
| \`formStateRestoreCallback()\` | Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue. | \`state: string \\| File \\| FormData \\| null, reason: 'autocomplete' \\| 'restore'\` |
| \`resetValidity()\` | Reset validity is a way of removing manual custom errors and native validation. | |
| \`setCustomValidity()\` | Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators. | \`message: string\` |

## Events

Learn more about [events](https://webawesome.com/docs/usage/#events).

| Name | Description |
| --- | --- |
| \`blur\` | Emitted when the dropzone loses focus. |
| \`change\` | Emitted when files are added or removed. |
| \`focus\` | Emitted when the dropzone gains focus. |
| \`input\` | Emitted when file selection changes. |
| \`wa-invalid\` | Emitted when the form control has been checked for validity and its constraints aren't satisfied. |

## Custom States

Learn more about [custom states](https://webawesome.com/docs/usage/#custom-states).

| Name | Description | CSS selector |
| --- | --- | --- |
| \`blank\` | No files selected. | \`:state(blank)\` |
| \`dragging\` | Files being dragged over dropzone. | \`:state(dragging)\` |

## CSS parts

Learn more about [CSS parts](https://webawesome.com/docs/usage/#css-parts).

| Name | Description | CSS selector |
| --- | --- | --- |
| \`base\` | The main component wrapper. | \`::part(base)\` |
| \`dropzone\` | The drag-and-drop area. | \`::part(dropzone)\` |
| \`dropzone-icon\` | The upload icon in the dropzone. | \`::part(dropzone-icon)\` |
| \`dropzone-text\` | The instruction text in the dropzone. | \`::part(dropzone-text)\` |
| \`file\` | Individual file item container. | \`::part(file)\` |
| \`file-details\` | Container for file name and size. | \`::part(file-details)\` |
| \`file-icon\` | The icon for non-image files. | \`::part(file-icon)\` |
| \`file-image\` | The image element for image thumbnails. | \`::part(file-image)\` |
| \`file-list\` | The container for selected files. | \`::part(file-list)\` |
| \`file-name\` | The file name text. | \`::part(file-name)\` |
| \`file-size\` | The file size text. | \`::part(file-size)\` |
| \`file-thumbnail\` | The thumbnail/icon container for a file. | \`::part(file-thumbnail)\` |
| \`hint\` | The hint element. | \`::part(hint)\` |
| \`label\` | The label element. | \`::part(label)\` |
| \`remove-button\` | The remove button for each file. | \`::part(remove-button)\` |

## Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-button>`](https://webawesome.com/docs/components/button)
-   [`<wa-format-bytes>`](https://webawesome.com/docs/components/format-bytes)
-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)
-   [`<wa-spinner>`](https://webawesome.com/docs/components/spinner)

**Need a hand?** Report a bug Ask for help