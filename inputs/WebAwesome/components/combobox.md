<!-- Source: https://webawesome.com/docs/components/combobox (fetched 2026-07-23 for Web Awesome 3.2.0).
     wa-combobox is a Pro component and is not documented in the public GitHub docs repo; content mirrored from the public web docs.
     The web docs track the latest release, so version-specific examples may drift. The CEM API surface
     (temp/wa-api/surface_3.2.0.json) is authoritative where this reference differs. -->

# Combobox (Pro)

The `<wa-combobox>` component merges a text input with a listbox, enabling users to filter and select from predefined options or enter custom values. It follows the ARIA APG Combobox pattern and includes live region announcements for screen readers.

## Basic Usage

```html
<wa-combobox name="foo" label="Type to filter...">
  <wa-option value="option-1">Option 1</wa-option>
  <wa-option value="option-2">Option 2</wa-option>
  <wa-option value="option-3">Option 3</wa-option>
</wa-combobox>
```

## Slots

| Name        | Description |
|-------------|-------------|
| (default)   | Listbox options using `<wa-option>` elements; use `<wa-divider>` for visual grouping. |
| clear-icon  | Custom icon for the clear button. |
| end         | Element (e.g., `<wa-icon>`) at the combobox end. |
| expand-icon | Icon displayed when expanded/collapsed; rotates on state change. |
| hint        | Descriptive text (or use the `hint` attribute). |
| label       | Input label (or use the `label` attribute). |
| start       | Element (e.g., `<wa-icon>`) at the combobox start. |

## Attributes & Properties

| Name                 | Type | Default | Description |
|----------------------|------|---------|-------------|
| allow-create         | boolean | false | Creates new options on-the-fly; fires `wa-create` before creation. |
| allow-custom-value   | boolean | false | Permits values not matching existing options (single-select only). |
| appearance           | `'filled' \| 'outlined' \| 'filled-outlined'` | `'outlined'` | Visual style. |
| autocapitalize       | string | — | Text capitalization behavior. |
| autocorrect          | boolean | false | Enables browser autocorrect. |
| disabled             | boolean | false | Disables the control. |
| enterkeyhint         | string | — | Virtual keyboard Enter key label. |
| hint                 | string | `''` | Descriptive hint text. |
| inputmode            | string | — | Virtual keyboard type. |
| label                | string | `''` | Control label. |
| max-options-visible  | number | 3 | Max selected options shown before collapse (0 = no limit). |
| multiple             | boolean | false | Allows multiple selections. |
| name                 | string \| null | `''` | Form submission name. |
| open                 | boolean | false | Menu visibility toggle. |
| pill                 | boolean | false | Rounded edges styling. |
| placeholder          | string | `''` | Empty state hint text. |
| placement            | `'top' \| 'bottom'` | `'bottom'` | Preferred menu position. |
| required             | boolean | false | Marks control as required. |
| size                 | `'small' \| 'medium' \| 'large'` | `'medium'` | Control size. |
| spellcheck           | boolean | false | Enables spell checking. |
| value                | string \| string[] | — | Selected value(s). |
| with-clear           | boolean | false | Adds clear button when not empty. |
| with-hint            | boolean | false | SSR only; pre-render hint. |
| with-label           | boolean | false | SSR only; pre-render label. |

## Methods

| Name                | Arguments | Description |
|---------------------|-----------|-------------|
| blur()              | — | Removes focus. |
| focus()             | `options: FocusOptions` | Sets focus. |
| hide()              | — | Closes the listbox. |
| show()              | — | Opens the listbox. |
| setCustomValidity() | `message: string` | Sets custom validation error. |

## Events

| Event         | Description |
|---------------|-------------|
| blur          | Control loses focus. |
| change        | Value changes. |
| focus         | Control gains focus. |
| input         | Control receives input. |
| wa-after-hide | Menu closes; animations complete. |
| wa-after-show | Menu opens; animations complete. |
| wa-clear      | Value is cleared. |
| wa-create     | User selects "create" option (cancelable; detail: `{ inputValue: string }`). |
| wa-hide       | Menu closes. |
| wa-invalid    | Validation fails. |
| wa-show       | Menu opens. |

## CSS Parts

`clear-button`, `combobox`, `combobox-input`, `end`, `expand-icon`, `form-control`, `form-control-input`, `form-control-label`, `hint`, `listbox`, `start`, `tag`, `tags`.

## Dependencies

Automatically imports `<wa-button>`, `<wa-icon>`, `<wa-option>`, `<wa-popup>`, `<wa-spinner>`, `<wa-tag>`.
