<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/date-input.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/date-input -->

# Date Input [Pro]

**Full documentation:** https://webawesome.com/docs/components/date-input

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
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma)
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

Link to This Section

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

## Examples

Link to This Section

### Initial Value

Link to This Section

Set the `value` attribute to an ISO date to pre-populate the input.

```html
<wa-date-input label="Date of birth" value="1990-04-15"></wa-date-input>
```

### Labels

Link to This Section

Use the `label` attribute to give the date input an accessible label. For labels that contain HTML, use the `label` slot instead.

```html
<wa-date-input label="When did you start?"></wa-date-input>
```

### Hint

Link to This Section

Add descriptive hint to a date input with the `hint` attribute. For hints that contain HTML, use the `hint` slot instead.

```html
<wa-date-input label="Departure" hint="Choose the day you want to leave."></wa-date-input>
```

### Start & End Decorations

Link to This Section

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

Link to This Section

Combine `required` with `with-clear` to enforce a value while still letting users wipe their selection in a single click.

```html
<form>
  <wa-date-input name="due" label="Due date" required with-clear></wa-date-input>
  <br />
  <wa-button type="submit" appearance="filled" variant="neutral">Submit</wa-button>
</form>
```

### Min and Max

Link to This Section

Use `min` and `max` to constrain the selectable range. Dates outside the range render as disabled in the popup, can't be selected by clicking, and are skipped by keyboard navigation.

```html
<wa-date-input label="Check-in" min="2026-01-01" max="2026-12-31"></wa-date-input>
```

### Disable Past or Future

Link to This Section

Use `disable-past` or `disable-future` to block all dates strictly before or after today, without having to recalculate `min`/`max` every day.

```html
<wa-date-input label="Future bookings only" disable-past></wa-date-input>
```

### Date Range

Link to This Section

Use `mode="range"` to let users select a start and end date. The calendar opens in range mode automatically.

```html
<wa-date-input label="Booking" mode="range" months="2"></wa-date-input>
```

### Sizes

Link to This Section

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

Link to This Section

Use the `appearance` attribute to switch between the default outlined input, a filled background, or a filled input with an outlined border.

```html
<wa-date-input appearance="filled" label="Filled"></wa-date-input>
<br />
<wa-date-input appearance="filled-outlined" label="Filled outlined"></wa-date-input>
```

### Pill

Link to This Section

Use the `pill` attribute to give the input fully rounded edges.

```html
<wa-date-input pill label="Pill"></wa-date-input>
```

### Disabled

Link to This Section

Use the `disabled` attribute to disable the date input entirely. Disabled date inputs don't accept input, are skipped during tabbing, and don't submit a value with the form.

```html
<wa-date-input label="Disabled" value="2026-05-20" disabled></wa-date-input>
```

### Read-only

Link to This Section

Use the `readonly` attribute to make the date input non-editable while still allowing it to be focused and to submit its value with the form. The popup still opens for browsing.

```html
<wa-date-input label="Read-only" value="2026-05-20" readonly></wa-date-input>
```

### Disable Specific Dates and Days of the Week

Link to This Section

Use `disabled-days-of-week` to block recurring weekdays (e.g., weekends), and `disabled-dates` to block specific calendar dates such as holidays.

```html
<wa-date-input label="Pick a weekday" disable-past disabled-days-of-week="sun sat"></wa-date-input>
<br />
<br />
<wa-date-input label="Excludes holidays" disabled-dates="2026-07-04 2026-12-25 2026-12-31"></wa-date-input>
```

### Range Length Constraints

Link to This Section

In range mode, use `min-range` and `max-range` to require the selection to fall within a specific number of days.

```html
<wa-date-input label="Trip length (3–14 days)" mode="range" months="2" min-range="3" max-range="14"></wa-date-input>
```

### Localized

Link to This Section

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

Link to This Section

Use `placement` to anchor the popup relative to the input and `distance` to control the gap between them.

```html
<wa-date-input label="Anchored above" placement="top-start" distance="8"></wa-date-input>
```

### Custom Programmatic Disabling

Link to This Section

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

Link to This Section

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

Link to This Section

Set `slot="day-YYYY-MM-DD"` on a child element to override that specific day's content.

```html
<wa-date-input label="With holiday markers">
  <span slot="day-2026-07-04" title="Independence Day">🇺🇸</span>
  <span slot="day-2026-12-25" title="Christmas">🎄</span>
</wa-date-input>
```

### Custom Navigation Icons

Link to This Section

Use the `previous-icon` and `next-icon` slots to replace the calendar's default paging arrows.

```html
<wa-date-input label="With custom arrows">
  <wa-icon slot="previous-icon" name="arrow-left"></wa-icon>
  <wa-icon slot="next-icon" name="arrow-right"></wa-icon>
</wa-date-input>
```

### Slotting a Footer

Link to This Section

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

Link to This Section

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

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

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

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `name` |  | `string \| null` | `''` | The date input's name, submitted as a name/value pair with form data. |
| `value` | `defaultValue` | `string` |  | The default value of the form control. Used for form reset. |
| `disabled` |  | `boolean` | `false` | Disables the date input. |
| `required` |  | `boolean` | `false` | Makes the date input required for form submission. |
| `readonly` |  | `boolean` | `false` | Makes the input non-editable. The popup still opens for browsing. |
| `size` |  | `WaDateInputSize \| 'small' \| 'medium' \| 'large'` | `'m'` | The date input's size. |
| `appearance` |  | `'filled' \| 'outlined' \| 'filled-outlined'` | `'outlined'` | The date input's visual appearance. |
| `pill` |  | `boolean` | `false` | Draws a pill-style date input with rounded edges. |
| `label` |  | `string` | `''` | The date input's label. If you need to display HTML, use the `label` slot instead. |
| `hint` |  | `string` | `''` | The date input's hint. If you need to display HTML, use the `hint` slot instead. |
| `autocomplete` |  | `string` | `''` | Forwarded to the hidden form input (e.g., `'bday'`, `'cc-exp'`) to enable browser autofill. |
| `with-clear` | `withClear` | `boolean` | `false` | Shows a clear button when the date input has a value. |
| `with-label` | `withLabel` | `boolean` | `false` | Only required for SSR. Set to `true` if you're slotting in a `label` element. |
| `with-hint` | `withHint` | `boolean` | `false` | Only required for SSR. Set to `true` if you're slotting in a `hint` element. |
| `mode` |  | `WaDateInputMode` | `'single'` | Selection mode. |
| `min` |  | `string` | `''` | Earliest selectable date as `YYYY-MM-DD`. Out-of-range dates are disabled in the popup calendar and a committed value before `min` fails constraint validation with `rangeUnderflow`. |
| `max` |  | `string` | `''` | Latest selectable date as `YYYY-MM-DD`. Out-of-range dates are disabled in the popup calendar and a committed value after `max` fails constraint validation with `rangeOverflow`. |
| `today` |  | `string` | `''` | Override "today" as `YYYY-MM-DD` (defaults to the runtime date). |
| `first-day-of-week` | `firstDayOfWeek` | `WaDateInputFirstDayOfWeek` | `'auto'` | The first day of the week in the popup calendar. |
| `disabled-dates` | `disabledDates` | `string \| string[] \| Date[]` | `''` | Dates that cannot be selected. |
| `disabled-days-of-week` | `disabledDaysOfWeek` | `string` | `''` | Days of the week that cannot be selected. Accepts a space-separated list of three-letter weekday names. |
| `disable-past` | `disablePast` | `boolean` | `false` | Disable all dates strictly before today. |
| `disable-future` | `disableFuture` | `boolean` | `false` | Disable all dates strictly after today. |
| `min-range` | `minRange` | `number` | `0` | Minimum range length in days (range mode only). `0` disables. |
| `max-range` | `maxRange` | `number` | `0` | Maximum range length in days (range mode only). `0` disables. |
| `months` |  | `1 \| 2` | `1` | Number of months rendered in the popup calendar. |
| `page-by` | `pageBy` | `'months' \| 'single'` | `'months'` | Whether prev/next pages by the visible range or one month at a time. |
| `with-outside-days` | `withOutsideDays` | `boolean` | `false` | Show leading/trailing days from adjacent months in the popup calendar. |
| `with-week-numbers` | `withWeekNumbers` | `boolean` | `false` | Show ISO 8601 week numbers in the popup calendar. |
| `weekday-format` | `weekdayFormat` | `'narrow' \| 'short' \| 'long'` | `'short'` | Weekday header format in the popup calendar. |
| `open` |  | `boolean` | `false` | Whether the popup calendar is open. |
| `placement` |  | `WaDateInputPlacement` | `'bottom-start'` | Preferred popup placement. |
| `distance` |  | `number` | `0` | Distance in pixels between the popup and the input. |
| `custom-error` | `customError` | `string \| null` | `null` |  |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## Methods

| Method | Description | Arguments |
| --- | --- | --- |
| `focus` | Sets focus on the first empty (else first) segment. | `options: FocusOptions` |
| `blur` | Removes focus from the date input. |  |
| `show` | Opens the popup calendar. |  |
| `hide` | Closes the popup calendar. |  |
| `clear` | Clears the current value and emits `wa-clear`, `input`, and `change`. Mirrors activating the clear button. No-op when already empty or when disabled/readonly. |  |
| `formStateRestoreCallback` | Called when the browser is trying to restore element’s state to state in which case reason is "restore", or when the browser is trying to fulfill autofill on behalf of user in which case reason is "autocomplete". In the case of "restore", state is a string, File, or FormData object previously set as the second argument to setFormValue. | `state: string \| File \| FormData \| null` |
| `setCustomValidity` | Do not use this when creating a "Validator". This is intended for end users of components. We track manually defined custom errors so we don't clear them on accident in our validators. | `message: string` |
| `resetValidity` | Reset validity is a way of removing manual custom errors and native validation. |  |

## Events

| Event | Description |
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

## Custom States

| State | Description |
| --- | --- |
| `blank` | The date input has no committed value. |
| `open` | The popup is open. |
| `range` | The date input is in range mode. |
| `disabled` | The date input is disabled. |

## CSS Parts

| Part | Description |
| --- | --- |
| `form-control` | The form control that wraps the label, input, and hint. |
| `form-control-label` | The label's wrapper. |
| `form-control-input` | The input's wrapper. |
| `hint` | The hint's wrapper. |
| `base` | The component's base wrapper. |
| `input-wrapper` | The container that wraps the start slot, segmented input, clear button, and expand button. |
| `start` | The container that wraps the `start` slot. |
| `end` | The container that wraps the `end` slot. |
| `input` | The segmented input group. |
| `segment` | Each editable segment (month/day/year spinbutton). Use `[part~="segment"]` to style all. |
| `segment-literal` | Inert literal text between segments (separators). |
| `range-separator` | The literal between the two groups in range mode. |
| `clear-button` | The clear button. |
| `expand-button` | The date picker toggle button. |
| `expand-icon` | The expand icon wrapper. |
| `popup` | The popup container. |
| `date-picker` | The popup's `<wa-date-picker>` element. |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--show-duration` | `var(--wa-transition-fast)` | The duration of the show animation. |
| `--hide-duration` | `var(--wa-transition-fast)` | The duration of the hide animation. |
