<!-- Source: reference doc bundled in the Web Awesome 3.10.0 release zip (dist/skills/webawesome/references/components/date-input.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/date-input -->

# Date Input [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-date-input>`

ProIncluded with Web Awesome Pro Experimental [Forms](https://webawesome.com/docs/components/?category=forms) [Since 3.8](https://webawesome.com/docs/resources/changelog#wa_380)

Date inputs let users enter a date through a segmented field or select one visually from a popup calendar. They support locale-aware segment order, min and max constraints, and form validation.

**[Get Date Input with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=date-input)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   Responsive [Layout Tools](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma) Newer additions to Web Awesome, like [`<wa-toast>`](https://webawesome.com/docs/components/toast), aren't included in the currently available kit, but a new version is in the works.  
    Track its progress on GitHub.
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Date Input!

Date Input is the form-associated counterpart to [`<wa-date-picker>`](https://webawesome.com/docs/components/date-picker). It renders a **segmented input** with discrete month, day, and year spinbutton segments in the user's locale order, alongside a popup calendar for visual selection. The segment order, separators, and submitted ISO value all derive from the page's locale.

Use the arrow keys to step through values, type digits to fill segments (focus auto-advances when the segment can accept no further digit), and use `Alt+Down Arrow` to open the popup calendar. The entire field is one tab stop.

```html
<wa-date-input label="Pick a date"></wa-date-input>
```

The submitted form value is always ISO 8601 (`YYYY-MM-DD` for single dates, `YYYY-MM-DD/YYYY-MM-DD` for ranges). The displayed input text follows the user's locale, which is inherited from the `lang` attribute on the host element or an ancestor.

## Form Submission

The hidden form value is canonical ISO 8601, regardless of the user's locale:

-   **Single mode**: `YYYY-MM-DD` (e.g., `2026-01-23`).
-   **Range mode**: `YYYY-MM-DD/YYYY-MM-DD`
-   **Partial input**: the form value is empty until the input parses successfully.

The example below renders a working form. Submit it (or change the date) and watch the console. The date input submits its value just like a native `<input>`, regardless of how the user typed or what locale they used.

```html
<form id="dp-form-demo">
  <wa-date-input name="event_date" label="Event date" required value="2026-05-20"></wa-date-input>
  <br />
  <wa-button type="submit" appearance="filled" variant="neutral">Submit</wa-button>
</form>

<pre id="dp-form-demo-output"></pre>

<style>
  #dp-form-demo-output {
    margin-block-start: 1rem;
    margin-block-end: 0;
    padding: 0.75rem;
    background: var(--wa-color-surface-lowered);
    border-radius: var(--wa-border-radius-m);
    font-size: 0.875em;
  }

  #dp-form-demo-output:empty {
    display: none;
  }
</style>

<script>
  const form = document.getElementById('dp-form-demo');
  const output = document.getElementById('dp-form-demo-output');

  form.addEventListener('submit', event => {
    event.preventDefault();
    const data = new FormData(form);
    const entries = Object.fromEntries(data.entries());
    const formatted = JSON.stringify(entries, null, 2);
    console.log('Submitted FormData:', entries);
    output.textContent = 'Submitted FormData:\n' + formatted;
  });
</script>
```

## Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.10.0/components/date-input/date-input.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/date-input/date-input.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/date-input/date-input.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaDateInput from '@awesome.me/webawesome/dist/react/date-input/index.js';
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value is
silently ignored and the element falls back to the default slot):

- `label` — The date input's label. Alternatively, use the `label` attribute.
- `hint` — Text that describes how to use the date input. Alternatively, use the `hint` attribute.
- `start` — An element placed at the start of the input.
- `end` — An element placed at the end of the input.
- `clear-icon` — An icon to use in lieu of the default clear icon.
- `expand-icon` — The icon to show on the date picker toggle button. Defaults to a calendar icon.
- `footer` — Content shown below the date picker inside the popup.
- `previous-icon` — Icon for the date picker's previous-page button. Forwarded to `<wa-date-picker>`.
- `next-icon` — Icon for the date picker's next-page button. Forwarded to `<wa-date-picker>`.
- `day-YYYY-MM-DD` — Custom content for a specific day in the popup date picker. Slot name is dynamic (e.g., `day-2026-05-25`). Forwarded to `<wa-date-picker>`.

## Attributes & Properties

| Property | Attribute | Description | Type | Default |
| --- | --- | --- | --- | --- |
| `validators` | — | Validators are static because they have `observedAttributes`, essentially attributes to "watch" for changes. Whenever these attributes change, we want to be notified and update the validator. | `Validator[]` | `[]` |
| `assumeInteractionOn` | — | Native `input` events do not fire on `role=spinbutton` elements (they aren't real `<input>`s). The component dispatches a composed host `input` event on every segment edit, every step, and on calendar selection, so a single `input` is enough to mark the field as interacted with. | `string[]` | `['input']` |
| `validationTarget` | — | Override this to change where constraint validation popups are anchored. | `undefined \| HTMLElement` | — |
| `name` | `name` | The date input's name, submitted as a name/value pair with form data. | `string \| null` | `''` |
| `value` | — | The date input's value. ISO 8601 `YYYY-MM-DD` for single mode, `YYYY-MM-DD/YYYY-MM-DD` for range mode (with `from <= to`). The setter also accepts a `Date` or a range object with `from` and `to` properties. | `string` | — |
| `defaultValue` | `value` | The default value of the form control. Used for form reset. | `string` | — |
| `disabled` | `disabled` | Disables the date input. | `boolean` | `false` |
| `required` | `required` | Makes the date input required for form submission. | `boolean` | `false` |
| `readonly` | `readonly` | Makes the input non-editable. The popup still opens for browsing. | `boolean` | `false` |
| `size` | `size` | The date input's size. | `WaDateInputSize \| 'small' \| 'medium' \| 'large'` | `'m'` |
| `appearance` | `appearance` | The date input's visual appearance. | `'filled' \| 'outlined' \| 'filled-outlined'` | `'outlined'` |
| `pill` | `pill` | Draws a pill-style date input with rounded edges. | `boolean` | `false` |
| `label` | `label` | The date input's label. If you need to display HTML, use the `label` slot instead. | `string` | `''` |
| `hint` | `hint` | The date input's hint. If you need to display HTML, use the `hint` slot instead. | `string` | `''` |
| `autocomplete` | `autocomplete` | Forwarded to the hidden form input (e.g., `'bday'`, `'cc-exp'`) to enable browser autofill. | `string` | `''` |
| `withClear` | `with-clear` | Shows a clear button when the date input has a value. | `boolean` | `false` |
| `withLabel` | `with-label` | Only required for SSR. Set to `true` if you're slotting in a `label` element. | `boolean` | `false` |
| `withHint` | `with-hint` | Only required for SSR. Set to `true` if you're slotting in a `hint` element. | `boolean` | `false` |
| `mode` | `mode` | Selection mode. | `WaDateInputMode` | `'single'` |
| `min` | `min` | Earliest selectable date as `YYYY-MM-DD`. Out-of-range dates are disabled in the popup calendar and a committed value before `min` fails constraint validation with `rangeUnderflow`. | `string` | `''` |
| `max` | `max` | Latest selectable date as `YYYY-MM-DD`. Out-of-range dates are disabled in the popup calendar and a committed value after `max` fails constraint validation with `rangeOverflow`. | `string` | `''` |
| `today` | `today` | Override "today" as `YYYY-MM-DD` (defaults to the runtime date). | `string` | `''` |
| `firstDayOfWeek` | `first-day-of-week` | The first day of the week in the popup calendar. | `WaDateInputFirstDayOfWeek` | `'auto'` |
| `disabledDates` | `disabled-dates` | Dates that cannot be selected. | `string \| string[] \| Date[]` | `''` |
| `disabledDaysOfWeek` | `disabled-days-of-week` | Days of the week that cannot be selected. Accepts a space-separated list of three-letter weekday names. | `string` | `''` |
| `disablePast` | `disable-past` | Disable all dates strictly before today. | `boolean` | `false` |
| `disableFuture` | `disable-future` | Disable all dates strictly after today. | `boolean` | `false` |
| `minRange` | `min-range` | Minimum range length in days (range mode only). `0` disables. | `number` | `0` |
| `maxRange` | `max-range` | Maximum range length in days (range mode only). `0` disables. | `number` | `0` |
| `isDateDisabled` | — | JS-only callback for custom date disabling. Forwarded to the popup calendar. | `(date: Date) => boolean \| undefined` | — |
| `dayContent` | — | JS-only callback for custom day-cell content. Forwarded to the popup calendar. | `WaDateInputDayContent \| undefined` | — |
| `months` | `months` | Number of months rendered in the popup calendar. | `1 \| 2` | `1` |
| `pageBy` | `page-by` | Whether prev/next pages by the visible range or one month at a time. | `'months' \| 'single'` | `'months'` |
| `withOutsideDays` | `with-outside-days` | Show leading/trailing days from adjacent months in the popup calendar. | `boolean` | `false` |
| `withWeekNumbers` | `with-week-numbers` | Show ISO 8601 week numbers in the popup calendar. | `boolean` | `false` |
| `weekdayFormat` | `weekday-format` | Weekday header format in the popup calendar. | `'narrow' \| 'short' \| 'long'` | `'short'` |
| `open` | `open` | Whether the popup calendar is open. | `boolean` | `false` |
| `placement` | `placement` | Preferred popup placement. | `WaDateInputPlacement` | `'bottom-start'` |
| `distance` | `distance` | Distance in pixels between the popup and the input. | `number` | `0` |
| `valueAsDate` | — | The selected date as a `Date` (single mode only). | `Date \| null` | — |
| `valueAsRange` | — | The selected range as an object with `from` and `to` properties (range mode only). | `{ from: Date \| null; to: Date \| null }` | — |
| `form` | — | By default, form controls are associated with the nearest containing `<form>` element. This attribute allows you to place the form control outside of a form and associate it with the form that has this `id`. The form must be in the same document or shadow root for this to work. | `HTMLFormElement \| null` | — |

## Methods

| Name | Description | Arguments |
| --- | --- | --- |
| `focus()` | Sets focus on the first empty (else first) segment. | `options: FocusOptions` |
| `blur()` | Removes focus from the date input. | — |
| `show()` | Opens the popup calendar. | — |
| `hide()` | Closes the popup calendar. | — |
| `clear()` | Clears the current value and emits `wa-clear`, `input`, and `change`. Mirrors activating the clear button. No-op when already empty or when disabled/readonly. | — |
| `formStateRestoreCallback()` | Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue. | `state: string \\| File \\| FormData \\| null` |
| `setCustomValidity()` | Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators. | `message: string` |
| `resetValidity()` | Reset validity is a way of removing manual custom errors and native validation. | — |

## Events

| Name | Description |
| --- | --- |
| `input` | Emitted on every segment edit, step, calendar interaction, and clear, even while the value is incomplete. |
| `change` | Emitted on every committed value transition (each completed date edit, calendar selection, or clear), mirroring native `<input type="date">` rather than the commit-on-blur behavior of `<wa-input>`/`<wa-select>`. This matches the sibling `<wa-time-input>`. It does NOT fire while a value is still incomplete. |
| `focus` | Emitted when the control receives focus. |
| `blur` | Emitted when the control loses focus. |
| `wa-clear` | Emitted when the clear button is activated. |
| `wa-show` | Emitted when the popup is about to open. Cancelable. |
| `wa-after-show` | Emitted after the popup opens and animations complete. |
| `wa-hide` | Emitted when the popup is about to close. Cancelable. |
| `wa-after-hide` | Emitted after the popup closes and animations complete. |
| `wa-invalid` | Emitted when the form control has been checked for validity and its constraints aren't satisfied. |

## CSS Custom Properties

| Name | Description |
| --- | --- |
| \`--hide-duration\` | \`var(--wa-transition-fast)\` The duration of the hide animation. Default |
| \`--show-duration\` | \`var(--wa-transition-fast)\` The duration of the show animation. Default |

## Custom States

| Name | Description | CSS selector |
| --- | --- | --- |
| `blank` | The date input has no committed value. | `:state(blank)` |
| `open` | The popup is open. | `:state(open)` |
| `range` | The date input is in range mode. | `:state(range)` |
| `disabled` | The date input is disabled. | `:state(disabled)` |

## CSS Parts

| Name | Description | CSS selector |
| --- | --- | --- |
| \`base\` | The component's base wrapper. | \`::part(base)\` |
| \`clear-button\` | The clear button. | \`::part(clear-button)\` |
| \`date-picker\` | \`\` The popup's element. | \`::part(date-picker)\` |
| \`end\` | \`end\` The container that wraps the slot. | \`::part(end)\` |
| \`expand-button\` | The date picker toggle button. | \`::part(expand-button)\` |
| \`expand-icon\` | The expand icon wrapper. | \`::part(expand-icon)\` |
| \`form-control\` | The form control that wraps the label, input, and hint. | \`::part(form-control)\` |
| \`form-control-input\` | The input's wrapper. | \`::part(form-control-input)\` |
| \`form-control-label\` | The label's wrapper. | \`::part(form-control-label)\` |
| \`hint\` | The hint's wrapper. | \`::part(hint)\` |
| \`input\` | The segmented input group. | \`::part(input)\` |
| \`input-wrapper\` | The container that wraps the start slot, segmented input, clear button, and expand button. | \`::part(input-wrapper)\` |
| \`popup\` | The popup container. | \`::part(popup)\` |
| \`range-separator\` | The literal between the two groups in range mode. | \`::part(range-separator)\` |
| \`segment\` | \`\[part~="segment"\]\` Each editable segment (month/day/year spinbutton). Use to style all. | \`::part(segment)\` |
| \`segment-literal\` | Inert literal text between segments (separators). | \`::part(segment-literal)\` |
| \`start\` | \`start\` The container that wraps the slot. | \`::part(start)\` |

## Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-date-picker>`](https://webawesome.com/docs/components/date-picker)
-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)
-   [`<wa-popup>`](https://webawesome.com/docs/components/popup)

## Examples

### Initial Value

Set the `value` attribute to an ISO date to pre-populate the input.

```html
<wa-date-input label="Date of birth" value="1990-04-15"></wa-date-input>
```

### Labels

Use the `label` attribute to give the date input an accessible label. For labels that contain HTML, use the `label` slot instead.

```html
<wa-date-input label="When did you start?"></wa-date-input>
```

### Hint

Add descriptive hint to a date input with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

```html
<wa-date-input label="Departure" hint="Choose the day you want to leave."></wa-date-input>
```

### Start & End Decorations

Use the `start` and `end` slots to add presentational elements like [`<wa-icon>`](https://webawesome.com/docs/components/icon) inside the input.

```html
<wa-date-input label="Departure">
  <wa-icon name="plane-departure" slot="start"></wa-icon>
</wa-date-input>
<br />
<wa-date-input label="Arrival">
  <wa-icon name="plane-arrival" slot="end"></wa-icon>
</wa-date-input>
```

### Required + Clear Button

Combine `required` with `with-clear` to enforce a value while still letting users wipe their selection in a single click.

```html
<form>
  <wa-date-input name="due" label="Due date" required with-clear></wa-date-input>
  <br />
  <wa-button type="submit" appearance="filled" variant="neutral">Submit</wa-button>
</form>
```

### Min & Max

Use `min` and `max` to constrain the selectable range. Dates outside the range render as disabled in the popup, can't be selected by clicking, and are skipped by keyboard navigation.

```html
<wa-date-input label="Check-in" min="2026-01-01" max="2026-12-31"></wa-date-input>
```

### Disable Past or Future

Use `disable-past` or `disable-future` to block all dates strictly before or after today, without having to recalculate `min`/`max` every day.

```html
<wa-date-input label="Future bookings only" disable-past></wa-date-input>
```

### Date Range

Use `mode="range"` to let users select a start and end date. The calendar opens in range mode automatically.

```html
<wa-date-input label="Booking" mode="range" months="2"></wa-date-input>
```

### Sizes

Use the `size` attribute to match the date input to surrounding form controls.

```html
<wa-date-input size="xs" label="Extra small"></wa-date-input>
<br />
<wa-date-input size="s" label="Small"></wa-date-input>
<br />
<wa-date-input size="m" label="Medium"></wa-date-input>
<br />
<wa-date-input size="l" label="Large"></wa-date-input>
<br />
<wa-date-input size="xl" label="Extra large"></wa-date-input>
```

### Filled Appearance

Use the `appearance` attribute to switch between the default outlined input, a filled background, or a filled input with an outlined border.

```html
<wa-date-input appearance="filled" label="Filled"></wa-date-input>
<br />
<wa-date-input appearance="filled-outlined" label="Filled outlined"></wa-date-input>
```

### Pill

Use the `pill` attribute to give the input fully rounded edges.

```html
<wa-date-input pill label="Pill"></wa-date-input>
```

### Disabled

Use the `disabled` attribute to disable the date input entirely. Disabled date inputs don't accept input, are skipped during tabbing, and don't submit a value with the form.

```html
<wa-date-input label="Disabled" value="2026-05-20" disabled></wa-date-input>
```

### Read-Only

Use the `readonly` attribute to make the date input non-editable while still allowing it to be focused and to submit its value with the form. The popup still opens for browsing.

```html
<wa-date-input label="Read-only" value="2026-05-20" readonly></wa-date-input>
```

### Disable Specific Dates & Days of the Week

Use `disabled-days-of-week` to block recurring weekdays (e.g., weekends), and `disabled-dates` to block specific calendar dates such as holidays.

```html
<wa-date-input label="Pick a weekday" disable-past disabled-days-of-week="sun sat"></wa-date-input>
<br />
<br />
<wa-date-input label="Excludes holidays" disabled-dates="2026-07-04 2026-12-25 2026-12-31"></wa-date-input>
```

### Range Length Constraints

In range mode, use `min-range` and `max-range` to require the selection to fall within a specific number of days.

```html
<wa-date-input label="Trip length (3–14 days)" mode="range" months="2" min-range="3" max-range="14"></wa-date-input>
```

### Localized

Set `lang` on the picker (or anywhere up the tree) to localize the input format and popup calendar.

```html
<wa-date-input label="Veranstaltungsdatum" lang="de-DE" value="2026-01-23"></wa-date-input>
<br />
<br />
<wa-date-input label="Date d'événement" lang="fr-FR" value="2026-01-23"></wa-date-input>
<br />
<br />
<wa-date-input label="日付" lang="ja-JP" value="2026-01-23"></wa-date-input>
```

### Customizing the Popup

Use `placement` to anchor the popup relative to the input and `distance` to control the gap between them.

```html
<wa-date-input label="Anchored above" placement="top-start" distance="8"></wa-date-input>
```

### Custom Programmatic Disabling

Set the `isDateDisabled` property to a function that returns `true` for any date that should be unselectable. This runs in addition to the declarative `min`, `max`, `disabled-dates`, and `disabled-days-of-week` rules.

```html
<wa-date-input id="dp-workdays" label="Workdays only"></wa-date-input>

<script type="module">
  const dp = document.getElementById('dp-workdays');

  await customElements.whenDefined('wa-date-input');
  await dp.updateComplete;
  dp.isDateDisabled = date => {
    const day = date.getDay();
    return day === 0 || day === 6;
  };
</script>
```

### Custom Day Content

Forward the `dayContent` callback to render anything inside individual day cells (badges, dots, availability counts).

```html
<wa-date-input id="dp-day-content" label="With custom day content"></wa-date-input>

<script type="module">
  const dp = document.getElementById('dp-day-content');

  await customElements.whenDefined('wa-date-input');
  await dp.updateComplete;

  dp.dayContent = date => {
    // Add a small dot to weekends.
    const day = date.getDay();
    if (day === 0 || day === 6) {
      return (
        date.getDate() +
        ' <span style="display:inline-block;width:.4em;height:.4em;border-radius:50%;background:var(--wa-color-brand-fill-loud);vertical-align:super;"></span>'
      );
    }
    return null; // default
  };
</script>
```

### Custom Per-Day Slots

Set `slot="day-YYYY-MM-DD"` on a child element to override that specific day's content.

```html
<wa-date-input label="With holiday markers">
  <span slot="day-2026-07-04" title="Independence Day">🇺🇸</span>
  <span slot="day-2026-12-25" title="Christmas">🎄</span>
</wa-date-input>
```

### Custom Navigation Icons

Use the `previous-icon` and `next-icon` slots to replace the calendar's default paging arrows.

```html
<wa-date-input label="With custom arrows">
  <wa-icon slot="previous-icon" name="arrow-left"></wa-icon>
  <wa-icon slot="next-icon" name="arrow-right"></wa-icon>
</wa-date-input>
```

### Slotting a Footer

The `footer` slot is forwarded to the popup calendar.

```html
<wa-date-input label="With footer">
  <div slot="footer" style="display:flex; gap:.5rem; justify-content:flex-end;">
    <wa-button
      size="s"
      appearance="filled"
      variant="neutral"
      onclick="this.closest('wa-date-input').value = new Date().toISOString().slice(0,10)"
      >Today</wa-button
    >
    <wa-button size="s" appearance="plain" onclick="this.closest('wa-date-input').value = ''">Clear</wa-button>
  </div>
</wa-date-input>
```

### Programmatic API

Set or read the value, listen for changes, and open or close the popup from JavaScript.

```html
<wa-date-input id="dp" label="Programmatic"></wa-date-input>

<script>
  const dp = document.getElementById('dp');

  // Set a value
  dp.value = '2026-01-23';

  // Or set with a Date
  dp.value = new Date(2026, 0, 23);

  // Read parsed value
  console.log(dp.valueAsDate);

  // Open/close
  await dp.show();
  await dp.hide();

  // Listen for changes
  dp.addEventListener('change', e => console.log('value:', e.target.value));
  dp.addEventListener('wa-after-show', () => console.log('popup opened'));
</script>
```
