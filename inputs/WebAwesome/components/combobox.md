<!-- Source: reference doc bundled in the Web Awesome 3.14.0 release zip (dist/skills/webawesome/references/components/combobox.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/combobox -->

# Combobox [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-combobox>`

ProIncluded with Web Awesome Pro Stable [Forms](https://webawesome.com/docs/components/?category=forms) [Since 3.1](https://webawesome.com/docs/resources/changelog#wa_310)

Comboboxes combine a text input with a listbox, allowing users to filter and select from predefined options or enter custom values.

**[Get Combobox with Web Awesome Pro!](https://webawesome.com/pro?from=pro-docs&component=combobox)**

Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   [Native Styles](https://webawesome.com/docs/utilities/native)
-   [CSS + Layout Utilities](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma)
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Combobox!

This component follows the [ARIA APG Combobox pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/) and uses live region announcements for result filtering in screen readers.

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

```html
<wa-combobox label="Coffee order" hint="Start typing to filter." value="latte">
  <wa-option value="espresso">Espresso</wa-option>
  <wa-option value="latte">Latte</wa-option>
  <wa-option value="cappuccino">Cappuccino</wa-option>
</wa-combobox>
```

This component works with standard `<form>` elements. Please refer to the section on [form controls](https://webawesome.com/docs/form-controls) to learn more about form submission and client-side validation.

## Accessibility Considerations

Always give the combobox an accessible name with the `label` attribute or the `label` slot. Focus stays in the text field while the listbox is open, so the combobox uses a polite live region to announce each option as the user arrows to it, the number of options when the list changes, and any [status message](#status-messages) shown in place of the options.

If you replace a message using the `loading`, `no-results`, `empty`, or `error` slot, your text is announced instead. Keep it short, plain text without links or buttons, because screen reader users will hear it but can't interact with it.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.14.0/components/combobox/combobox.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/combobox/combobox.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/combobox/combobox.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaCombobox from '@awesome.me/webawesome/dist/react/combobox/index.js';
```

### Slots

| Name | Description |
| --- | --- |
| (default) | \`\` The listbox options. Must be elements. You can use to group items visually. |
| \`clear-icon\` | An icon to use in lieu of the default clear icon. |
| \`empty\` | Shown in the listbox when there are no options and no query has been typed. |
| \`end\` | \`\` An element, such as , placed at the start of the combobox. |

### Attributes & Properties

| Name | Description | Reflects |
| --- | --- | --- |
| \`allowCreate\` allow-create | \`\` When true, if the user types text that doesn't match any existing option, a "Create \[value\]" option appears in the listbox. Selecting it creates a new in the DOM and selects it. A cancelable wa-create event fires before creation. Type boolean Default false | |
| \`allowCustomValue\` allow-custom-value | \`boolean\` When true, allows the user to enter a value that doesn't match any of the options. Only applies to single-select comboboxes. When false, the combobox will only accept values that match an option. Type Default false | |
| \`appearance\` appearance | \`'filled' \\| 'outlined' \\| 'filled-outlined'\` The combobox's visual appearance. Type Default 'outlined' | |
| \`autocapitalize\` autocapitalize | \`'off' \\| 'none' \\| 'on' \\| 'sentences' \\| 'words' \\| 'characters'\` Controls whether and how text input is automatically capitalized as it is entered/edited by the user. Type | |
| \`autocorrect\` autocorrect | \`"off"\` Indicates whether the browser's autocorrect feature is on or off. When set as an attribute, use or "on". When set as a property, use true or false. Type boolean | |
| \`currentOption\` | \`undefined\` The option the user is keying through, or once it's unusable. In server mode a response or a consumer swap can remove or hide the highlighted option at any moment, and no reader — Enter above all — may act on it. Type WaOption \\| undefined | |
| \`dataSource\` | \`AbortSignal\` A callback that loads options from a server. It receives the current query and an and returns the options to show — an array of { value, label, disabled? } objects, a string of HTML, or an array of elements. Setting this puts the combobox in server mode, which turns off client-side filtering. HTML is inserted as-is and is never sanitized, so make sure you trust it. Type ((request: ComboboxRequest) => Promise \\| ComboboxOptions) \\| null Default null | |
| \`disabled\` disabled | \`boolean\` Disables the combobox control. Type Default false | |
| \`enterkeyhint\` enterkeyhint | \`'enter' \\| 'done' \\| 'go' \\| 'next' \\| 'previous' \\| 'search' \\| 'send'\` Used to customize the label or icon of the Enter key on virtual keyboards. Type | |
| \`filter\` | \`true\` A function that customizes how options are filtered based on the input value. The function receives the option and the current input query string. Return to include the option in the filtered list, false to exclude. By default, options are filtered by checking if the option's label contains the query (case-insensitive). Ignored in server mode — the server decides what matches. Type ((option: WaOption, query: string) => boolean) \\| null Default null | |
| \`filterDebounce\` filter-debounce | \`reload()\` How long to wait, in milliseconds, after the user stops typing before requesting options in server mode. Opening the listbox and calling request immediately. Type number Default 250 | |
| \`form\` | \`\` By default, form controls are associated with the nearest containing element. This attribute allows you to place the form control outside of a form and associate it with the form that has this id. The form must be in the same document or shadow root for this to work. Type HTMLFormElement \\| null | |
| \`getTag\` | \`(option: WaOption, index: number) => TemplateResult \\| string \\| HTMLElement\` A function that customizes the tags to be rendered when multiple=true. The first argument is the option, the second is the current tag's index. The function should return either a Lit TemplateResult or a string containing trusted HTML of the symbol to render at the specified value. Type | |
| \`hint\` hint | \`hint\` The combobox's . If you need to display HTML, use the hint slot instead. Type string Default '' | |
| \`inputmode\` inputmode | \`'none' \\| 'text' \\| 'decimal' \\| 'numeric' \\| 'tel' \\| 'search' \\| 'email' \\| 'url'\` Tells the browser what type of data will be entered by the user, allowing it to display the appropriate virtual keyboard on supportive devices. Type | |
| \`inputValue\` | \`string\` The current text value in the input field. Type Default '' | |
| \`label\` label | \`label\` The combobox's . If you need to display HTML, use the label slot instead. Type string Default '' | |
| \`loading\` loading | \`true\` Whether a request for options is pending. The combobox sets this to the moment a request is scheduled (including the debounce wait) and, with a dataSource, clears it when the request settles. In event mode, set it to false yourself once you've updated the options. Type boolean Default false | |
| \`maxOptionsVisible\` max-options-visible | \`multiple\` The maximum number of selected options to show when is true. After the maximum, "+n" will be shown to indicate the number of additional items that are selected. Set to 0 to remove the limit. Type number Default 3 | |
| \`multiple\` multiple | \`boolean\` Allows more than one option to be selected. Type Default false | |
| \`name\` name | \`string \\| null\` The name of the combobox, submitted as a name/value pair with form data. Type Default '' | |
| \`open\` open | \`show()\` Indicates whether or not the combobox is open. You can toggle this attribute to show and hide the menu, or you can use the and hide() methods and this attribute will reflect the combobox's open state. Type boolean Default false | |
| \`pill\` pill | \`boolean\` Draws a pill-style combobox with rounded edges. Type Default false | |
| \`placeholder\` placeholder | \`string\` Placeholder text to show as a hint when the combobox is empty. Type Default '' | |
| \`placement\` placement | \`'top' \\| 'bottom'\` The preferred placement of the combobox's menu. Note that the actual placement may vary as needed to keep the listbox inside of the viewport. Type Default 'bottom' | |
| \`required\` required | \`boolean\` The combobox's required attribute. Type Default false | |
| \`server\` server | \`dataSource\` Switches the combobox to server mode without a callback: client-side filtering is turned off and you swap the slotted elements yourself in response to wa-options-request, then set loading to false. Implied when dataSource is set. Type boolean Default false | |
| \`size\` size | \`'xs' \\| 's' \\| 'm' \\| 'l' \\| 'xl' \\| 'small' \\| 'medium' \\| 'large'\` The combobox's size. Type Default 'm' | |
| \`spellcheck\` spellcheck | \`boolean\` Enables spell checking on the combobox. Type Default false | |
| \`validationTarget\` | \`undefined \\| HTMLElement\` Where to anchor native constraint validation Type | |
| \`validators\` | \`observedAttributes\` Validators are static because they have , essentially attributes to "watch" for changes. Whenever these attributes change, we want to be notified and update the validator. Type Validator\[\] Default \[\] | |
| \`value\` value | The combobox's value. This will be a string for single select or an array for multi-select. | |
| \`withClear\` with-clear | \`boolean\` Adds a clear button when the combobox is not empty. Type Default false | |
| \`withHint\` with-hint | \`true\` Only required for SSR. Set to if you're slotting in a hint element so the server-rendered markup includes the hint before the component hydrates on the client. Type boolean Default false | |
| \`withLabel\` with-label | \`true\` Only required for SSR. Set to if you're slotting in a label element so the server-rendered markup includes the label before the component hydrates on the client. Type boolean Default false | |

### Methods

| Name | Description | Arguments |
| --- | --- | --- |
| \`blur()\` | Removes focus from the control. | |
| \`focus()\` | Sets focus on the control. | \`options: FocusOptions\` |
| \`formStateRestoreCallback()\` | Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue. | \`state: string \\| File \\| FormData \\| null, reason: 'autocomplete' \\| 'restore'\` |
| \`hide()\` | Hides the listbox. | |
| \`reload()\` | \`dataSource\` Re-requests options using the current query (or an empty query when the listbox is closed). Resolves once the response has been applied in mode, or immediately after wa-options-request is emitted in event mode, so await combobox.reload() followed by setting value works. | |
| \`resetValidity()\` | Reset validity is a way of removing manual custom errors and native validation. | |
| \`setCustomValidity()\` | Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators. | \`message: string\` |
| \`show()\` | Shows the listbox. | |

### Events

| Name | Description |
| --- | --- |
| \`blur\` | Emitted when the control loses focus. |
| \`change\` | Emitted when the control's value changes. |
| \`focus\` | Emitted when the control gains focus. |
| \`input\` | Emitted when the control receives input. |
| \`request\` | |
| \`wa-after-hide\` | Emitted after the combobox's menu closes and all animations are complete. |
| \`wa-after-show\` | Emitted after the combobox's menu opens and all animations are complete. |
| \`wa-clear\` | Emitted when the control's value is cleared. |
| \`wa-create\` | \`event.preventDefault()\` Emitted when the user selects the "create" option. Call to handle creation yourself. The event detail contains { inputValue: string }. |
| \`wa-hide\` | Emitted when the combobox's menu closes. |
| \`wa-invalid\` | Emitted when the form control has been checked for validity and its constraints aren't satisfied. |
| \`wa-options-error\` | \`dataSource\` Emitted when a request rejects. The event detail contains { error: unknown, request: { query: string } }. |
| \`wa-options-request\` | \`detail\` Emitted in server mode whenever a request for options starts. The event contains { query: string, signal: AbortSignal }. |
| \`wa-show\` | Emitted when the combobox's menu opens. |

### CSS Custom Properties

| Name | Description |
| --- | --- |
| \`--hide-duration\` | \`var(--wa-transition-fast)\` The duration of the hide animation. Default |
| \`--show-duration\` | \`var(--wa-transition-fast)\` The duration of the show animation. Default |
| \`--tag-max-size\` | \`multiple\` When using , the max size of tags before their content is truncated. Default 10ch |

### Custom States

| Name | Description | CSS selector |
| --- | --- | --- |
| \`blank\` | The combobox is empty. | \`:state(blank)\` |
| \`disabled\` | The combobox is disabled. | \`:state(disabled)\` |
| \`loading\` | A request for options is pending. | \`:state(loading)\` |
| \`showing-loading-row\` | The open listbox is showing its loading row, so the in-field spinner stays hidden. | \`:state(showing-loading-row)\` |

### CSS Parts

| Name | Description | CSS selector |
| --- | --- | --- |
| \`clear-button\` | The clear button. | \`::part(clear-button)\` |
| \`combobox\` | The container the wraps the start, end, value, clear icon, and expand button. | \`::part(combobox)\` |
| \`combobox-input\` | The text input element. | \`::part(combobox-input)\` |
| \`empty\` | The status row shown when there are no options and no query has been typed. | \`::part(empty)\` |
| \`end\` | \`end\` The container that wraps the slot. | \`::part(end)\` |
| \`error\` | \`dataSource\` The status row shown when the last request failed. | \`::part(error)\` |
| \`expand-icon\` | The container that wraps the expand icon. | \`::part(expand-icon)\` |
| \`form-control\` | The form control that wraps the label, input, and hint. | \`::part(form-control)\` |
| \`form-control-input\` | The combobox's wrapper. | \`::part(form-control-input)\` |
| \`form-control-label\` | The label. | \`::part(form-control-label)\` |
| \`hint\` | The hint's wrapper. | \`::part(hint)\` |
| \`listbox\` | The listbox container where options are slotted. | \`::part(listbox)\` |
| \`loading\` | The status row shown while options are loading and none are available yet. | \`::part(loading)\` |
| \`no-results\` | The status row shown when the query matched nothing. | \`::part(no-results)\` |
| \`spinner\` | The loading spinner shown in the field while options are loading. | \`::part(spinner)\` |
| \`start\` | \`start\` The container that wraps the slot. | \`::part(start)\` |
| \`status\` | The listbox status row shown in place of options. Also carries a state-specific part. | \`::part(status)\` |
| \`tag\` | The individual tags that represent each multiselect option. | \`::part(tag)\` |
| \`tag\_\_content\` | The tag's content part. | \`::part(tag\_\_content)\` |
| \`tag\_\_remove-button\` | The tag's remove button. | \`::part(tag\_\_remove-button)\` |
| \`tag\_\_remove-button\_\_base\` | The tag's remove button base part. | \`::part(tag\_\_remove-button\_\_base)\` |
| \`tags\` | \`multiselect\` The container that houses option tags when is used. | \`::part(tags)\` |
| \`label\` | \`form-control-label\` Deprecated. Use the part instead. | \`::part(label)\` |

### Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-button>`](https://webawesome.com/docs/components/button)
-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)
-   [`<wa-option>`](https://webawesome.com/docs/components/option)
-   [`<wa-popup>`](https://webawesome.com/docs/components/popup)
-   [`<wa-spinner>`](https://webawesome.com/docs/components/spinner)
-   [`<wa-tag>`](https://webawesome.com/docs/components/tag)

## Examples

### Label

Use the `label` attribute to give the combobox an accessible label. For labels that contain HTML, use the `label` slot instead.

```html
<wa-combobox label="Choose a fruit">
  <wa-option value="apple">Apple</wa-option>
  <wa-option value="banana">Banana</wa-option>
  <wa-option value="orange">Orange</wa-option>
</wa-combobox>
```

### Hint

Add descriptive hint to a combobox with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

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

```html
<wa-combobox placeholder="Type to search...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Clearable

Use the `with-clear` attribute to make the control clearable. The clear button only appears when the combobox has a value or text input.

```html
<wa-combobox with-clear value="option-1">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Multiple

To allow multiple options to be selected, use the `multiple` attribute. Selected options appear as tags and the input filters the list. Pair it with `with-clear` to reset the selection, and use `max-options-visible` to cap how many tags show before the rest collapse into a count.

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

```html
<wa-combobox label="Pre-selected option">
  <wa-option value="option-1" selected>Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
  <wa-option value="option-4">Option 4</wa-option>
</wa-combobox>
```

For multiple selections, apply it to all selected options.

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

```html
<wa-combobox allow-custom-value label="Enter or select a color" placeholder="Type a color...">
  <wa-option value="red">Red</wa-option>
  <wa-option value="green">Green</wa-option>
  <wa-option value="blue">Blue</wa-option>
</wa-combobox>
```

### Grouping Options

Use [`<wa-divider>`](https://webawesome.com/docs/components/divider) to group listbox items visually. You can also use `<small>` to provide labels, but they won't be announced by most assistive devices.

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

```html
<wa-combobox pill placeholder="Search...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Size

Use the `size` attribute to change a combobox's size.

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

```html
<wa-combobox placement="top" placeholder="Opens above">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Start & End Decorations

Use the `start` and `end` slots to add presentational elements like [`<wa-icon>`](https://webawesome.com/docs/components/icon) within the combobox.

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

```html
<wa-combobox placeholder="Disabled" disabled>
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

### Creating New Items

Use the `allow-create` attribute to let users create new options on the fly. When the user types text that doesn't match any existing option, a "Create \[value\]" option appears at the bottom of the listbox. Selecting it adds a new [`<wa-option>`](https://webawesome.com/docs/components/option) to the DOM and selects it.

```html
<wa-combobox allow-create label="Select or create a tag" placeholder="Type to search or create...">
  <wa-option value="bug">Bug</wa-option>
  <wa-option value="feature">Feature</wa-option>
  <wa-option value="docs">Docs</wa-option>
</wa-combobox>
```

This also works with `multiple` mode.

```html
<wa-combobox allow-create multiple with-clear label="Select or create tags" placeholder="Type to search or create...">
  <wa-option value="bug" selected>Bug</wa-option>
  <wa-option value="feature">Feature</wa-option>
  <wa-option value="docs">Docs</wa-option>
</wa-combobox>
```

For advanced use cases, listen for the `wa-create` event and call `preventDefault()` to handle creation yourself. This is useful when you need to normalize values, validate input, or call an API before creating the option.

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

### Status Messages

When the listbox has nothing to show, it shows a message in place of the options. Replace any of the defaults with a slot.

| Slot | Shows when | Default text |
| --- | --- | --- |
| \`empty\` | There are no options and the user hasn't typed anything | No options |
| \`no-results\` | The query matched nothing | No matching results |
| \`loading\` | A request is pending and no options are showing yet | Loading |
| \`error\` | \`dataSource\` The last request failed | Options could not be loaded |

Any visible option suppresses all four. The `loading` and `error` messages only appear when [loading options from a server](#loading-options-from-a-server).

```html
<div class="wa-stack">
  <wa-combobox label="Fruit" placeholder="Type to filter...">
    <span slot="no-results">No fruit by that name.</span>
    <wa-option value="apple">Apple</wa-option>
    <wa-option value="banana">Banana</wa-option>
    <wa-option value="cherry">Cherry</wa-option>
  </wa-combobox>

  <wa-combobox label="Assignee" placeholder="Nobody yet">
    <span slot="empty">Nobody has joined this project.</span>
  </wa-combobox>
</div>
```

### Loading Options from a Server

When there are too many options to render up front, the combobox can fetch them as the user types. There are two ways to connect it to your server.

| | Data source | Request event |
| --- | --- | --- |
| Turn it on with | \`dataSource\` The property | \`server\` The attribute |
| You provide | A callback that returns the options | \`\` elements you render yourself |
| Best for | Most cases | Frameworks that own the option elements |

Either way the combobox stops filtering on the client. Your server decides what matches, so the `filter` property is ignored.

The rest of this section uses the `dataSource` property. Set it to a function that returns the options for the user's query, where each returned option needs a `value` and a `label`. For the other approach, see [Rendering Options Yourself](#rendering-options-yourself).

```js
combobox.dataSource = async ({ query, signal }) => {
  const response = await fetch(`/api/countries?q=${encodeURIComponent(query)}`, { signal });
  return response.json(); // [{ value: 'ca', label: 'Canada' }, ...]
};
```

The combobox requests options when the listbox opens, using an empty `query`, and again as the user types. Requests wait for a 250ms pause in typing, which you can change with the `filter-debounce` attribute, and outdated ones can be canceled with `signal`.

```html
<wa-combobox id="combobox-server" label="Country" placeholder="Search countries" with-clear></wa-combobox>

<script type="module">
  const combobox = document.querySelector('#combobox-server');

  const countries = [
    { value: 'ar', label: 'Argentina' },
    { value: 'au', label: 'Australia' },
    { value: 'br', label: 'Brazil' },
    { value: 'ca', label: 'Canada' },
    { value: 'fr', label: 'France' },
    { value: 'de', label: 'Germany' },
    { value: 'in', label: 'India' },
    { value: 'jp', label: 'Japan' },
    { value: 'mx', label: 'Mexico' },
    { value: 'no', label: 'Norway' },
  ];

  // This stands in for a request to your server
  combobox.dataSource = async ({ query }) => {
    await new Promise(resolve => setTimeout(resolve, 500));
    return countries.filter(country => country.label.toLowerCase().includes(query.toLowerCase()));
  };
</script>
```

To start with a value selected, add it as a [`<wa-option>`](https://webawesome.com/docs/components/option) with the `selected` attribute, the same as without a data source. Selected options stay selected even when a later response doesn't include them.

```html
<wa-combobox label="Assignee">
  <wa-option value="grace" selected>Grace Hopper</wa-option>
</wa-combobox>
```

Setting the `value` property also works, as long as an option with that value exists. A value with no matching option is dropped, since the combobox only knows the labels of options it has.

```html
<wa-combobox label="Assignee" value="grace">
  <wa-option value="grace">Grace Hopper</wa-option>
</wa-combobox>
```

#### Custom Option Content

To show more than a label, return HTML instead. Use the same [`<wa-option>`](https://webawesome.com/docs/components/option) markup you'd write by hand, including icons in the `start` and `end` slots, [`<wa-divider>`](https://webawesome.com/docs/components/divider), and group headings. If an option contains text other than its label, such as a description, set the option's `label` attribute.

```html
<wa-combobox id="combobox-custom" label="Assignee" placeholder="Search people"></wa-combobox>

<style>
  #combobox-custom small {
    display: block;
    font-size: var(--wa-font-size-smaller);
  }
</style>

<script type="module">
  const combobox = document.querySelector('#combobox-custom');

  const people = [
    { value: 'ada', label: 'Ada Lovelace', role: 'Mathematician', icon: 'calculator' },
    { value: 'alan', label: 'Alan Turing', role: 'Cryptanalyst', icon: 'key' },
    { value: 'grace', label: 'Grace Hopper', role: 'Rear Admiral', icon: 'anchor' },
    { value: 'katherine', label: 'Katherine Johnson', role: 'Orbital Mechanic', icon: 'rocket' },
  ];

  // This stands in for a request to your server
  combobox.dataSource = async ({ query }) => {
    await new Promise(resolve => setTimeout(resolve, 500));

    return people
      .filter(person => person.label.toLowerCase().includes(query.toLowerCase()))
      .map(
        // The option holds the role too, so `label` tells the combobox which part is the label
        person => `
          <wa-option value="${person.value}" label="${person.label}">
            <wa-icon slot="start" name="${person.icon}"></wa-icon>
            ${person.label}
            <small>${person.role}</small>
          </wa-option>
        `,
      )
      .join('');
  };
</script>
```

**Only return HTML you trust.**  
Unsanitized user input returned from the `dataSource` callback can introduce XSS vulnerabilities.

#### Loading and Error Messages

Options that are already showing stay visible while new ones load. When there's nothing to show, the listbox shows a [status message](#status-messages) instead.

When the `dataSource` callback throws or rejects, the combobox shows the `error` message, emits the `wa-options-error` event, and tries again the next time the user opens the listbox or types. Search for `boom` below to see a failed request.

```html
<wa-combobox id="combobox-status" label="City" placeholder="Search cities">
  <span slot="empty">Start typing to find a city.</span>
  <span slot="no-results">No cities match your search.</span>
  <span slot="error">We couldn't reach the city service.</span>
</wa-combobox>

<script type="module">
  const combobox = document.querySelector('#combobox-status');
  const cities = ['Austin', 'Boston', 'Chicago', 'Denver', 'Portland', 'Seattle'];

  // This stands in for a request to your server
  combobox.dataSource = async ({ query }) => {
    await new Promise(resolve => setTimeout(resolve, 500));
    if (query === '') return [];
    if (query.toLowerCase() === 'boom') throw new Error('The city service is unavailable');

    return cities
      .filter(city => city.toLowerCase().includes(query.toLowerCase()))
      .map(city => ({ value: city.toLowerCase(), label: city }));
  };

  combobox.addEventListener('wa-options-error', event => {
    console.error('Options failed to load:', event.detail.error);
  });
</script>
```

#### Refreshing Options

Call the `reload()` method to request options again. With the `dataSource` property it returns a promise that resolves once the new options are in place, which makes it easy to select something you just created on the server. With the `server` attribute it resolves as soon as `wa-options-request` is emitted, so wait for your own update before you set the `value` property.

```html
<wa-combobox id="combobox-reload" label="Project" with-clear></wa-combobox>
<br />
<wa-button id="combobox-reload-button">Create a Project</wa-button>

<script type="module">
  const combobox = document.querySelector('#combobox-reload');
  const button = document.querySelector('#combobox-reload-button');

  const projects = [
    { value: 'apollo', label: 'Apollo' },
    { value: 'beacon', label: 'Beacon' },
    { value: 'cinder', label: 'Cinder' },
  ];

  // This stands in for a request to your server
  combobox.dataSource = async ({ query }) => {
    await new Promise(resolve => setTimeout(resolve, 500));
    return projects.filter(project => project.label.toLowerCase().includes(query.toLowerCase()));
  };

  button.addEventListener('click', async () => {
    const number = projects.length + 1;
    projects.push({ value: `project-${number}`, label: `Project ${number}` });

    await combobox.reload();
    combobox.value = `project-${number}`;
  });
</script>
```

#### Rendering Options Yourself

If your framework renders the [`<wa-option>`](https://webawesome.com/docs/components/option) elements, add the `server` attribute instead of setting the `dataSource` property. The combobox emits the `wa-options-request` event when it needs options and sets the `loading` property to `true`. Update the options for `event.detail.query`, then set the `loading` property back to `false`. Skip the update when `event.detail.signal.aborted` is `true`, because a newer request has replaced that one.

```html
<wa-combobox id="combobox-event" server label="Fruit" placeholder="Search fruit"></wa-combobox>

<script type="module">
  const combobox = document.querySelector('#combobox-event');
  const fruits = ['Apple', 'Banana', 'Cherry', 'Grape', 'Mango', 'Orange', 'Peach'];

  combobox.addEventListener('wa-options-request', async event => {
    const { query, signal } = event.detail;

    // This stands in for a request to your server
    await new Promise(resolve => setTimeout(resolve, 500));
    if (signal.aborted) return;

    combobox.innerHTML = fruits
      .filter(fruit => fruit.toLowerCase().includes(query.toLowerCase()))
      .map(fruit => `<wa-option value="${fruit.toLowerCase()}">${fruit}</wa-option>`)
      .join('');

    combobox.loading = false;
  });
</script>
```

### Custom Tags

When multiple options can be selected, you can provide custom tags by passing a function to the `getTag` property. Your function can return a string of HTML, a [Lit Template](https://lit.dev/docs/templates/overview/), or an [`HTMLElement`](https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement). The `getTag()` function will be called for each option. The first argument is an [`<wa-option>`](https://webawesome.com/docs/components/option) element and the second argument is the tag's index (its position in the tag list).

Remember that custom tags are rendered in a shadow root. To style them, you can use the `style` attribute in your template or you can add your own [parts](https://webawesome.com/docs/customizing/#css-parts) and target them with the [`::part()`](https://developer.mozilla.org/en-US/docs/Web/CSS/::part) selector.

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

**Only pass content you trust to `getTag()`.**  
Unsanitized user input can introduce XSS vulnerabilities.

When using custom tags with `with-remove`, you must include the `data-value` attribute set to the option's value. This allows the combobox to identify which option to deselect when the tag's remove button is clicked.
