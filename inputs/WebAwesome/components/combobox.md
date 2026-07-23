<!-- Source: https://webawesome.com/docs/components/combobox (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Combobox Component Reference

## Overview

The `<wa-combobox>` component combines a text input with a listbox, allowing users to filter and select from predefined options or enter custom values. It follows the ARIA APG Combobox pattern.

**Status:** Pro component. **Category:** Forms. **Available Since:** v3.1

## Basic Usage

```html
<wa-combobox name="foo" label="Type to filter...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | The listbox options. Must be `<wa-option>` elements. |
| clear-icon | An icon to use in lieu of the default clear icon. |
| end | An element, such as `<wa-icon>`, placed at the end of the combobox. |
| expand-icon | The icon to show when the control is expanded and collapsed. |
| hint | Text that describes how to use the input. |
| label | The input's label. |
| start | An element, such as `<wa-icon>`, placed at the start of the combobox. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| allow-create | boolean | false | Show a "Create [value]" option when typed text matches no option. |
| allow-custom-value | boolean | false | Allow a value that doesn't match any option. Single-select only. |
| appearance | 'filled' \| 'outlined' \| 'filled-outlined' | 'outlined' | The combobox's visual appearance. |
| autocapitalize | 'off' \| 'none' \| 'on' \| 'sentences' \| 'words' \| 'characters' | - | Automatic capitalization of text input. |
| autocorrect | boolean | - | Browser autocorrect on/off. |
| custom-error | string \| null | null | Manually defined custom error (managed via setCustomValidity). |
| disabled | boolean | false | Disables the combobox control. |
| enterkeyhint | 'enter' \| 'done' \| 'go' \| 'next' \| 'previous' \| 'search' \| 'send' | - | Enter-key hint for virtual keyboards. |
| hint | string | '' | The combobox's hint. |
| inputmode | 'none' \| 'text' \| 'decimal' \| 'numeric' \| 'tel' \| 'search' \| 'email' \| 'url' | - | Virtual keyboard input mode. |
| label | string | '' | The combobox's label. |
| max-options-visible | number | 3 | Max selected options shown when multiple. 0 removes the limit. |
| multiple | boolean | false | Allow more than one option selected. |
| name | string \| null | '' | Name submitted with form data. |
| open | boolean | false | Whether the combobox is open. |
| pill | boolean | false | Pill-style combobox with rounded edges. |
| placeholder | string | '' | Placeholder text. |
| placement | 'top' \| 'bottom' | 'bottom' | Preferred menu placement. |
| required | boolean | false | Required attribute. |
| size | 'small' \| 'medium' \| 'large' | 'medium' | The combobox's size. |
| spellcheck | boolean | false | Enables spell checking. |
| value | string \| array | - | String for single select, array for multi-select. |
| with-clear | boolean | false | Adds a clear button when not empty. |

## Methods

| Name | Arguments | Description |
|------|-----------|-------------|
| blur() | - | Removes focus from the control. |
| focus() | options: FocusOptions | Sets focus on the control. |
| formStateRestoreCallback() | state, reason | Called when the browser restores element state or fulfills autofill. |
| hide() | - | Hides the listbox. |
| resetValidity() | - | Remove manual custom errors and native validation. |
| setCustomValidity() | message: string | Set a manually defined custom error. |
| show() | - | Shows the listbox. |

## Events

| Name | Description |
|------|-------------|
| blur | Emitted when the control loses focus. |
| change | Emitted when the control's value changes. |
| focus | Emitted when the control gains focus. |
| input | Emitted when the control receives input. |
| wa-after-hide | Emitted after the menu closes and animations complete. |
| wa-after-show | Emitted after the menu opens and animations complete. |
| wa-clear | Emitted when the control's value is cleared. |
| wa-create | Emitted when the user selects the "create" option. Detail: { inputValue: string }. |
| wa-hide | Emitted when the menu closes. |
| wa-invalid | Emitted when constraints aren't satisfied. |
| wa-show | Emitted when the menu opens. |

## Dependencies

`<wa-button>`, `<wa-icon>`, `<wa-option>`, `<wa-popup>`, `<wa-spinner>`, `<wa-tag>`.
