<!-- Source: https://webawesome.com/docs/components/combobox (Web Awesome 3.1.0 Pro component, not in public GitHub docs) -->
<!--
Pro component — not documented in the public GitHub repo; ingested from the public web docs.
combobox was introduced in Web Awesome 3.1 (the page's own badge reads "Since 3.1"). The public
web docs are served only at the current live version (3.10.0 at fetch time on 2026-07-22), so the
"Importing" CDN version token and any API added after 3.1 reflect 3.10.0. The wrapper follows the
target 3.1.0 Custom Elements Manifest.
-->

# Combobox

`<wa-combobox>`

ProIncluded with Web Awesome Pro Stable [Forms](/docs/components/?category=forms) [Since 3.1](/docs/resources/changelog#wa_310)

Comboboxes combine a text input with a listbox, allowing users to filter and select from predefined options or enter custom values.

This component follows the [ARIA APG Combobox pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/) and uses live region announcements for result filtering in screen readers.

Option 1 Option 2 Option 3 Option 4 Option 5 Option 6

```html
<wa-combobox name="foo" label="Type to filter...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
  <wa-option value="option-4">Option 4</wa-option>
  <wa-option value="option-5">Option 5</wa-option>
  <wa-option value="option-6">Option 6</wa-option>
</wa-combobox>
```

Espresso Latte Cappuccino

```html
<wa-combobox label="Coffee order" hint="Start typing to filter." value="latte">
  <wa-option value="espresso">Espresso</wa-option>
  <wa-option value="latte">Latte</wa-option>
  <wa-option value="cappuccino">Cappuccino</wa-option>
</wa-combobox>
```

This component works with standard `<form>` elements. Please refer to the section on [form controls](/docs/form-controls) to learn more about form submission and client-side validation.

## Examples

### Label

Use the `label` attribute to give the combobox an accessible label. For labels that contain HTML, use the `label` slot instead.

Apple Banana Orange

```html
<wa-combobox label="Choose a fruit">
  <wa-option value="apple">Apple</wa-option>
  <wa-option value="banana">Banana</wa-option>
  <wa-option value="orange">Orange</wa-option>
</wa-combobox>
```

### Hint

Add descriptive hint to a combobox with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

Apple Banana Cherry Grape Orange

```html
<wa-combobox label="Favorite Fruit" hint="Start typing to filter options.">
  <wa-option value="apple">Apple</wa-option>
  <wa-option value="banana">Banana</wa-option>
  <wa-option value="cherry">Cherry</wa-option>
  <wa-option value="grape">Grape</wa-option>
  <wa-option value="orange">Orange</wa-option>
</wa-combobox>
```

### Placeholder

Use the `placeholder` attribute to add a placeholder.

Option 1 Option 2 Option 3

```html
<wa-combobox placeholder="Type to search...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Clearable

Use the `with-clear` attribute to make the control clearable. The clear button only appears when the combobox has a value or text input.

Option 1 Option 2 Option 3

```html
<wa-combobox with-clear value="option-1">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Multiple

To allow multiple options to be selected, use the `multiple` attribute. Selected options appear as tags and the input filters the list. Pair it with `with-clear` to reset the selection, and use `max-options-visible` to cap how many tags show before the rest collapse into a count.

TypeScript Go Rust Python Ruby Swift

```html
<wa-combobox label="Languages" multiple with-clear max-options-visible="2" placeholder="Type to filter...">
  <wa-option value="ts" selected>TypeScript</wa-option>
  <wa-option value="go" selected>Go</wa-option>
  <wa-option value="rust" selected>Rust</wa-option>
  <wa-option value="python">Python</wa-option>
  <wa-option value="ruby">Ruby</wa-option>
  <wa-option value="swift">Swift</wa-option>
</wa-combobox>
```

In multiple mode, the text input is used for filtering options only. After selecting an option, the input is cleared so you can continue filtering and selecting more options.

### Initial Value

Use the `selected` attribute on individual options to set the initial selection, similar to native HTML.

Option 1 Option 2 Option 3 Option 4

```html
<wa-combobox label="Pre-selected option">
  <wa-option value="option-1" selected>Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
  <wa-option value="option-4">Option 4</wa-option>
</wa-combobox>
```

For multiple selections, apply it to all selected options.

Option 1 Option 2 Option 3 Option 4

```html
<wa-combobox multiple with-clear>
  <wa-option value="option-1" selected>Option 1</wa-option>
  <wa-option value="option-2" selected>Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
  <wa-option value="option-4">Option 4</wa-option>
</wa-combobox>
```

Framework users can bind directly to the `value` property for reactive data binding and form state management.

### Allowing Custom Values

By default, the combobox only accepts values that match an option. Use `allow-custom-value` to let users enter arbitrary values.

Red Green Blue

```html
<wa-combobox allow-custom-value label="Enter or select a color" placeholder="Type a color...">
  <wa-option value="red">Red</wa-option>
  <wa-option value="green">Green</wa-option>
  <wa-option value="blue">Blue</wa-option>
</wa-combobox>
```

### Grouping Options

Use [`<wa-divider>`](/docs/components/divider) to group listbox items visually. You can also use `<small>` to provide labels, but they won't be announced by most assistive devices.

Fruits Apple Banana Orange Vegetables Carrot Broccoli Spinach

```html
<wa-combobox label="Grouped Options">
  <small>Fruits</small>
  <wa-option value="apple">Apple</wa-option>
  <wa-option value="banana">Banana</wa-option>
  <wa-option value="orange">Orange</wa-option>
  <wa-divider></wa-divider>
  <small>Vegetables</small>
  <wa-option value="carrot">Carrot</wa-option>
  <wa-option value="broccoli">Broccoli</wa-option>
  <wa-option value="spinach">Spinach</wa-option>
</wa-combobox>
```

### Appearance

Use the `appearance` attribute to change the combobox's visual appearance.

Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3

```html
<wa-combobox appearance="filled" placeholder="Filled">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
<br />
<wa-combobox appearance="filled-outlined" placeholder="Filled Outlined">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
<br />
<wa-combobox appearance="outlined" placeholder="Outlined">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Pill

Use the `pill` attribute to give comboboxes rounded edges.

Option 1 Option 2 Option 3

```html
<wa-combobox pill placeholder="Search...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Size

Use the `size` attribute to change a combobox's size.

Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3  
Option 1 Option 2 Option 3

```html
<wa-combobox placeholder="Extra Small" size="xs">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>

<br />

<wa-combobox placeholder="Small" size="s">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>

<br />

<wa-combobox placeholder="Medium" size="m">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>

<br />

<wa-combobox placeholder="Large" size="l">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>

<br />

<wa-combobox placeholder="Extra Large" size="xl">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Placement

The preferred placement of the combobox's listbox can be set with the `placement` attribute. Note that the actual position may vary to ensure the panel remains in the viewport. Valid placements are `top` and `bottom`.

Option 1 Option 2 Option 3

```html
<wa-combobox placement="top" placeholder="Opens above">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Start & End Decorations

Use the `start` and `end` slots to add presentational elements like [`<wa-icon>`](/docs/components/icon) within the combobox.

New York Los Angeles Chicago  
New York Los Angeles Chicago  
New York Los Angeles Chicago

```html
<wa-combobox placeholder="Search locations..." size="s" with-clear>
  <wa-icon slot="start" name="magnifying-glass"></wa-icon>
  <wa-icon slot="end" name="location-dot"></wa-icon>
  <wa-option value="new-york">New York</wa-option>
  <wa-option value="los-angeles">Los Angeles</wa-option>
  <wa-option value="chicago">Chicago</wa-option>
</wa-combobox>
<br />
<wa-combobox placeholder="Search locations..." size="m" with-clear>
  <wa-icon slot="start" name="magnifying-glass"></wa-icon>
  <wa-icon slot="end" name="location-dot"></wa-icon>
  <wa-option value="new-york">New York</wa-option>
  <wa-option value="los-angeles">Los Angeles</wa-option>
  <wa-option value="chicago">Chicago</wa-option>
</wa-combobox>
<br />
<wa-combobox placeholder="Search locations..." size="l" with-clear>
  <wa-icon slot="start" name="magnifying-glass"></wa-icon>
  <wa-icon slot="end" name="location-dot"></wa-icon>
  <wa-option value="new-york">New York</wa-option>
  <wa-option value="los-angeles">Los Angeles</wa-option>
  <wa-option value="chicago">Chicago</wa-option>
</wa-combobox>
```

### Disabled

Use the `disabled` attribute to disable a combobox.

Option 1 Option 2 Option 3

```html
<wa-combobox placeholder="Disabled" disabled>
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Creating New Items

Use the `allow-create` attribute to let users create new options on the fly. When the user types text that doesn't match any existing option, a "Create \[value\]" option appears at the bottom of the listbox. Selecting it adds a new [`<wa-option>`](/docs/components/option) to the DOM and selects it.

Bug Feature Docs

```html
<wa-combobox allow-create label="Select or create a tag" placeholder="Type to search or create...">
  <wa-option value="bug">Bug</wa-option>
  <wa-option value="feature">Feature</wa-option>
  <wa-option value="docs">Docs</wa-option>
</wa-combobox>
```

This also works with `multiple` mode.

Bug Feature Docs

```html
<wa-combobox allow-create multiple with-clear label="Select or create tags" placeholder="Type to search or create...">
  <wa-option value="bug" selected>Bug</wa-option>
  <wa-option value="feature">Feature</wa-option>
  <wa-option value="docs">Docs</wa-option>
</wa-combobox>
```

For advanced use cases, listen for the `wa-create` event and call `preventDefault()` to handle creation yourself. This is useful when you need to normalize values, validate input, or call an API before creating the option.

Bug Feature await customElements.whenDefined('wa-combobox'); const combobox = document.querySelector('.custom-create-combobox'); combobox.addEventListener('wa-create', event => { event.preventDefault(); const { inputValue } = event.detail; // Normalize the value (e.g. lowercase, slugify) const option = document.createElement('wa-option'); option.value = inputValue.toLowerCase().replace(/\\s+/g, '-'); option.textContent = inputValue; combobox.appendChild(option); combobox.value = option.value; });

```html
<wa-combobox allow-create label="Add a tag" placeholder="Type to create..." class="custom-create-combobox">
  <wa-option value="bug">Bug</wa-option>
  <wa-option value="feature">Feature</wa-option>
</wa-combobox>

<script type="module">
  await customElements.whenDefined('wa-combobox');
  const combobox = document.querySelector('.custom-create-combobox');

  combobox.addEventListener('wa-create', event => {
    event.preventDefault();

    const { inputValue } = event.detail;

    // Normalize the value (e.g. lowercase, slugify)
    const option = document.createElement('wa-option');
    option.value = inputValue.toLowerCase().replace(/\s+/g, '-');
    option.textContent = inputValue;
    combobox.appendChild(option);
    combobox.value = option.value;
  });
</script>
```

### Custom Filter Function

You can provide a custom filter function to control how options are matched. The function receives the option element and the current query string, and should return `true` to show the option or `false` to hide it.

By default, the combobox filters options that contain the query anywhere in the label, but you can customize this to implement fuzzy matching, prefix-only matching, or apply any other filtering logic.

Apple Pineapple Banana Grape Grapefruit await customElements.whenDefined('wa-combobox'); const combobox = document.querySelector('.custom-filter'); // Custom filter that matches anywhere in the label (not just the start) combobox.filter = (option, query) => { return option.label.toLowerCase().includes(query.toLowerCase()); };

```html
<wa-combobox label="Search (includes match)" placeholder="Search anywhere in text..." class="custom-filter">
  <wa-option value="apple">Apple</wa-option>
  <wa-option value="pineapple">Pineapple</wa-option>
  <wa-option value="banana">Banana</wa-option>
  <wa-option value="grape">Grape</wa-option>
  <wa-option value="grapefruit">Grapefruit</wa-option>
</wa-combobox>

<script type="module">
  await customElements.whenDefined('wa-combobox');
  const combobox = document.querySelector('.custom-filter');

  // Custom filter that matches anywhere in the label (not just the start)
  combobox.filter = (option, query) => {
    return option.label.toLowerCase().includes(query.toLowerCase());
  };
</script>
```

### Custom Tags

When multiple options can be selected, you can provide custom tags by passing a function to the `getTag` property. Your function can return a string of HTML, a [Lit Template](https://lit.dev/docs/templates/overview/), or an [`HTMLElement`](https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement). The `getTag()` function will be called for each option. The first argument is an [`<wa-option>`](/docs/components/option) element and the second argument is the tag's index (its position in the tag list).

Remember that custom tags are rendered in a shadow root. To style them, you can use the `style` attribute in your template or you can add your own [parts](/docs/customizing/#css-parts) and target them with the [`::part()`](https://developer.mozilla.org/en-US/docs/Web/CSS/::part) selector.

Email Phone Chat await customElements.whenDefined('wa-combobox'); const combobox = document.querySelector('.custom-tag-combobox'); await combobox.updateComplete; combobox.getTag = (option, index) => { // Use the same icon used in wa-option const name = option.querySelector('wa-icon\[slot="start"\]').name; // You can return a string, a Lit Template, or an HTMLElement here // Important: include data-value so the tag can be removed properly return \` <wa-tag with-remove data-value="${option.value}"> <wa-icon name="${name}"></wa-icon> ${option.label} </wa-tag> \`; };

```html
<wa-combobox placeholder="Select contacts..." multiple with-clear class="custom-tag-combobox">
  <wa-option value="email" selected>
    <wa-icon slot="start" name="envelope" variant="solid"></wa-icon>
    Email
  </wa-option>
  <wa-option value="phone" selected>
    <wa-icon slot="start" name="phone" variant="solid"></wa-icon>
    Phone
  </wa-option>
  <wa-option value="chat">
    <wa-icon slot="start" name="comment" variant="solid"></wa-icon>
    Chat
  </wa-option>
</wa-combobox>

<script type="module">
  await customElements.whenDefined('wa-combobox');
  const combobox = document.querySelector('.custom-tag-combobox');
  await combobox.updateComplete;

  combobox.getTag = (option, index) => {
    // Use the same icon used in wa-option
    const name = option.querySelector('wa-icon[slot="start"]').name;

    // You can return a string, a Lit Template, or an HTMLElement here
    // Important: include data-value so the tag can be removed properly
    return `
      <wa-tag with-remove data-value="${option.value}">
        <wa-icon name="${name}"></wa-icon>
        ${option.label}
      </wa-tag>
    `;
  };
</script>
```

Be sure you trust the content you are outputting! Passing unsanitized user input to `getTag()` can result in XSS vulnerabilities.

When using custom tags with `with-remove`, you must include the `data-value` attribute set to the option's value. This allows the select to identify which option to deselect when the tag's remove button is clicked.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

CDN npm Self-Hosted React

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.10.0/components/combobox/combobox.js';
```

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/combobox/combobox.js';
```

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/combobox/combobox.js';
```

To import this component for React 18 or below, use the following code:

```js
import WaCombobox from '@awesome.me/webawesome/dist/react/combobox/index.js';
```

### Slots

Learn more about [using slots](/docs/usage/#slots).

Name

Description

(default)

The listbox options. Must be [`<wa-option>`](/docs/components/option) elements. You can use [`<wa-divider>`](/docs/components/divider) to group items visually.

`clear-icon`

An icon to use in lieu of the default clear icon.

`end`

An element, such as [`<wa-icon>`](/docs/components/icon), placed at the end of the combobox.

`expand-icon`

The icon to show when the control is expanded and collapsed. Rotates on open and close.

`hint`

Text that describes how to use the input. Alternatively, you can use the `hint` attribute.

`label`

The input's label. Alternatively, you can use the `label` attribute.

`start`

An element, such as [`<wa-icon>`](/docs/components/icon), placed at the start of the combobox.

### Attributes & Properties

Learn more about [attributes and properties](/docs/usage/#attributes-and-properties).

Name

Description

Reflects

`allowCreate`  

`allow-create`

When true, if the user types text that doesn't match any existing option, a "Create \[value\]" option appears in the listbox. Selecting it creates a new [`<wa-option>`](/docs/components/option) in the DOM and selects it. A cancelable `wa-create` event fires before creation.

**Type** `boolean`

**Default** `false`

`allowCustomValue`  

`allow-custom-value`

When true, allows the user to enter a value that doesn't match any of the options. Only applies to single-select comboboxes. When false, the combobox will only accept values that match an option.

**Type** `boolean`

**Default** `false`

`appearance`  

`appearance`

The combobox's visual appearance.

**Type** `'filled' | 'outlined' | 'filled-outlined'`

**Default** `'outlined'`

`autocapitalize`  

`autocapitalize`

Controls whether and how text input is automatically capitalized as it is entered/edited by the user.

**Type** `'off' | 'none' | 'on' | 'sentences' | 'words' | 'characters'`

`autocorrect`  

`autocorrect`

Indicates whether the browser's autocorrect feature is on or off. When set as an attribute, use `"off"` or `"on"`. When set as a property, use `true` or `false`.

**Type** `boolean`

`disabled`  

`disabled`

Disables the combobox control.

**Type** `boolean`

**Default** `false`

`enterkeyhint`  

`enterkeyhint`

Used to customize the label or icon of the Enter key on virtual keyboards.

**Type** `'enter' | 'done' | 'go' | 'next' | 'previous' | 'search' | 'send'`

`filter`  

A function that customizes how options are filtered based on the input value. The function receives the option and the current input query string. Return `true` to include the option in the filtered list, `false` to exclude. By default, options are filtered by checking if the option's label contains the query (case-insensitive).

**Type** `((option: WaOption, query: string) => boolean) | null`

**Default** `null`

`form`  

By default, form controls are associated with the nearest containing `<form>` element. This attribute allows you to place the form control outside of a form and associate it with the form that has this `id`. The form must be in the same document or shadow root for this to work.

**Type** `HTMLFormElement | null`

`getTag`  

A function that customizes the tags to be rendered when multiple=true. The first argument is the option, the second is the current tag's index. The function should return either a Lit TemplateResult or a string containing trusted HTML of the symbol to render at the specified value.

**Type** `(option: WaOption, index: number) => TemplateResult | string | HTMLElement`

`hint`  

`hint`

The combobox's hint. If you need to display HTML, use the `hint` slot instead.

**Type** `string`

**Default** `''`

`inputmode`  

`inputmode`

Tells the browser what type of data will be entered by the user, allowing it to display the appropriate virtual keyboard on supportive devices.

**Type** `'none' | 'text' | 'decimal' | 'numeric' | 'tel' | 'search' | 'email' | 'url'`

`inputValue`  

The current text value in the input field.

**Type** `string`

**Default** `''`

`label`  

`label`

The combobox's label. If you need to display HTML, use the `label` slot instead.

**Type** `string`

**Default** `''`

`maxOptionsVisible`  

`max-options-visible`

The maximum number of selected options to show when `multiple` is true. After the maximum, "+n" will be shown to indicate the number of additional items that are selected. Set to 0 to remove the limit.

**Type** `number`

**Default** `3`

`multiple`  

`multiple`

Allows more than one option to be selected.

**Type** `boolean`

**Default** `false`

`name`  

`name`

The name of the combobox, submitted as a name/value pair with form data.

**Type** `string | null`

**Default** `''`

`open`  

`open`

Indicates whether or not the combobox is open. You can toggle this attribute to show and hide the menu, or you can use the `show()` and `hide()` methods and this attribute will reflect the combobox's open state.

**Type** `boolean`

**Default** `false`

`pill`  

`pill`

Draws a pill-style combobox with rounded edges.

**Type** `boolean`

**Default** `false`

`placeholder`  

`placeholder`

Placeholder text to show as a hint when the combobox is empty.

**Type** `string`

**Default** `''`

`placement`  

`placement`

The preferred placement of the combobox's menu. Note that the actual placement may vary as needed to keep the listbox inside of the viewport.

**Type** `'top' | 'bottom'`

**Default** `'bottom'`

`required`  

`required`

The combobox's required attribute.

**Type** `boolean`

**Default** `false`

`size`  

`size`

The combobox's size.

**Type** `'xs' | 's' | 'm' | 'l' | 'xl' | 'small' | 'medium' | 'large'`

**Default** `'m'`

`spellcheck`  

`spellcheck`

Enables spell checking on the combobox.

**Type** `boolean`

**Default** `false`

`validationTarget`  

Where to anchor native constraint validation

**Type** `undefined | HTMLElement`

`validators`  

Validators are static because they have `observedAttributes`, essentially attributes to "watch" for changes. Whenever these attributes change, we want to be notified and update the validator.

**Type** `Validator[]`

**Default** `[]`

`value`  

`value`

The combobox's value. This will be a string for single select or an array for multi-select.

`withClear`  

`with-clear`

Adds a clear button when the combobox is not empty.

**Type** `boolean`

**Default** `false`

`withHint`  

`with-hint`

Only required for SSR. Set to `true` if you're slotting in a `hint` element so the server-rendered markup includes the hint before the component hydrates on the client.

**Type** `boolean`

**Default** `false`

`withLabel`  

`with-label`

Only required for SSR. Set to `true` if you're slotting in a `label` element so the server-rendered markup includes the label before the component hydrates on the client.

**Type** `boolean`

**Default** `false`

### Methods

Learn more about [methods](/docs/usage/#methods).

Name

Description

Arguments

`blur()`

Removes focus from the control.

`focus()`

Sets focus on the control.

`options: FocusOptions`

`formStateRestoreCallback()`

Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue.

`state: string | File | FormData | null, reason: 'autocomplete' | 'restore'`

`hide()`

Hides the listbox.

`resetValidity()`

Reset validity is a way of removing manual custom errors and native validation.

`setCustomValidity()`

Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators.

`message: string`

`show()`

Shows the listbox.

### Events

Learn more about [events](/docs/usage/#events).

Name

Description

`blur`

Emitted when the control loses focus.

`change`

Emitted when the control's value changes.

`focus`

Emitted when the control gains focus.

`input`

Emitted when the control receives input.

`wa-after-hide`

Emitted after the combobox's menu closes and all animations are complete.

`wa-after-show`

Emitted after the combobox's menu opens and all animations are complete.

`wa-clear`

Emitted when the control's value is cleared.

`wa-create`

Emitted when the user selects the "create" option. Call `event.preventDefault()` to handle creation yourself. The event `detail` contains `{ inputValue: string }`.

`wa-hide`

Emitted when the combobox's menu closes.

`wa-invalid`

Emitted when the form control has been checked for validity and its constraints aren't satisfied.

`wa-show`

Emitted when the combobox's menu opens.

### CSS Custom Properties

Learn more about [CSS custom properties](/docs/usage/#custom-properties).

Name

Description

`--hide-duration`

The duration of the hide animation.

**Default** `var(--wa-transition-fast)`

`--show-duration`

The duration of the show animation.

**Default** `var(--wa-transition-fast)`

`--tag-max-size`

When using `multiple`, the max size of tags before their content is truncated.

**Default** `10ch`

### Custom States

Learn more about [custom states](/docs/usage/#custom-states).

Name

Description

CSS selector

`blank`

The combobox is empty.

`:state(blank)`

`disabled`

The combobox is disabled.

`:state(disabled)`

### CSS Parts

Learn more about [CSS parts](/docs/usage/#css-parts).

Name

Description

CSS selector

`clear-button`

The clear button.

`::part(clear-button)`

`combobox`

The container the wraps the start, end, value, clear icon, and expand button.

`::part(combobox)`

`combobox-input`

The text input element.

`::part(combobox-input)`

`end`

The container that wraps the `end` slot.

`::part(end)`

`expand-icon`

The container that wraps the expand icon.

`::part(expand-icon)`

`form-control`

The form control that wraps the label, input, and hint.

`::part(form-control)`

`form-control-input`

The combobox's wrapper.

`::part(form-control-input)`

`form-control-label`

The label's wrapper.

`::part(form-control-label)`

`hint`

The hint's wrapper.

`::part(hint)`

`listbox`

The listbox container where options are slotted.

`::part(listbox)`

`start`

The container that wraps the `start` slot.

`::part(start)`

`tag`

The individual tags that represent each multiselect option.

`::part(tag)`

`tag__content`

The tag's content part.

`::part(tag__content)`

`tag__remove-button`

The tag's remove button.

`::part(tag__remove-button)`

`tag__remove-button__base`

The tag's remove button base part.

`::part(tag__remove-button__base)`

`tags`

The container that houses option tags when `multiselect` is used.

`::part(tags)`

### Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-button>`](/docs/components/button)
-   [`<wa-icon>`](/docs/components/icon)
-   [`<wa-option>`](/docs/components/option)
-   [`<wa-popup>`](/docs/components/popup)
-   [`<wa-spinner>`](/docs/components/spinner)
-   [`<wa-tag>`](/docs/components/tag)
