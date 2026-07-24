<!-- Source: reference doc bundled in the Web Awesome 3.10.0 release zip (dist/skills/webawesome/references/components/date-picker.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/date-picker -->

# Date Picker [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-date-picker>`

ProIncluded with Web Awesome Pro Experimental [Forms](https://webawesome.com/docs/components/?category=forms) [Since 3.8](https://webawesome.com/docs/resources/changelog#wa_380)

Date pickers display a month grid for selecting a single date or a date range inline. Use them when dates need to remain visible, such as in scheduling interfaces or embedded inside another form control.

**[Get Date Picker with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=date-picker)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Date Picker!

Date pickers display an inline calendar that lets users choose a single date or a date range. They follow the [WAI-ARIA Date Picker pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/examples/datepicker-dialog/) and use `Intl.DateTimeFormat` for localization.

```html
<wa-date-picker style="max-width: 300px;"></wa-date-picker>
```

`<wa-date-picker>` is the inline calendar grid. It is _not_ form-associated and does not submit with forms. For an input that opens a calendar in a popover and participates in forms, use [`<wa-date-input>`](https://webawesome.com/docs/components/date-input).

## Listening for Changes

The `change` event fires when the user commits a new value. In range mode, `input` also fires after the first click of a new range, before commit.

```html
<wa-date-picker style="max-width: 600px;" mode="range" months="2" class="change-listener"></wa-date-picker>
<output class="change-output"></output>

<script type="module">
  import { allDefined } from '/dist/webawesome.js';
  await allDefined();

  const picker = document.querySelector('.change-listener');
  const output = document.querySelector('.change-output');

  picker.addEventListener('change', () => {
    output.textContent = `Value: ${picker.value}`;
  });

  picker.addEventListener('wa-focus-day', event => {
    console.log('focused', event.detail.date);
  });
</script>
```

## Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.10.0/components/date-picker/date-picker.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/date-picker/date-picker.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/date-picker/date-picker.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaDatePicker from '@awesome.me/webawesome/dist/react/date-picker/index.js';
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value is
silently ignored and the element falls back to the default slot):

- `previous-icon` — Icon shown inside the previous-page button. Defaults to a left chevron.
- `next-icon` — Icon shown inside the next-page button. Defaults to a right chevron.
- `header` — Replaces the entire header row including title and navigation buttons. Advanced use only.
- `footer` — Optional content rendered below the calendar grid. Empty by default.

## Attributes & Properties

| Property | Attribute | Description | Type | Default |
| --- | --- | --- | --- | --- |
| `mode` | `mode` | The selection mode. | `WaDatePickerMode` | `'single'` |
| `value` | `value` | The selected date(s). For `mode="single"`, an ISO date string (`YYYY-MM-DD`) or empty. For `mode="range"`, two ISO dates separated by `/` (`YYYY-MM-DD/YYYY-MM-DD`). The property setter also accepts `Date` objects and `{ from, to }` objects for ranges. | `string` | — |
| `min` | `min` | The earliest selectable date as `YYYY-MM-DD`. | `string` | `''` |
| `max` | `max` | The latest selectable date as `YYYY-MM-DD`. | `string` | `''` |
| `today` | `today` | Overrides the date considered "today". | `string` | `''` |
| `focusedDate` | `focused-date` | The currently focused date as `YYYY-MM-DD`. Drives roving tabindex and the visible month. | `string` | `''` |
| `view` | `view` | The current view. | `WaDatePickerView` | `'days'` |
| `months` | `months` | Number of months rendered side-by-side. Either `1` or `2`. Set to `2` to see both ends of a range at once. | `1 \| 2` | `1` |
| `pageBy` | `page-by` | Whether prev/next advances by the visible range (`months`) or one month at a time (`single`). | `WaDatePickerPageBy` | `'months'` |
| `firstDayOfWeek` | `first-day-of-week` | The first day of the week. The default `auto` uses the current locale's week info. To set a specific day, pass a three-letter weekday name: `sun`, `mon`, `tue`, `wed`, `thu`, `fri`, or `sat`. | `WaDatePickerFirstDayOfWeek` | `'auto'` |
| `withOutsideDays` | `with-outside-days` | Shows leading and trailing days from adjacent months. | `boolean` | `false` |
| `withWeekNumbers` | `with-week-numbers` | Shows an ISO week-number column. | `boolean` | `false` |
| `weekdayFormat` | `weekday-format` | The weekday header format. | `WaDatePickerWeekdayFormat` | `'short'` |
| `disabled` | `disabled` | Disables the entire picker. | `boolean` | `false` |
| `readonly` | `readonly` | Displays the current value without allowing changes. Cells remain focusable. | `boolean` | `false` |
| `disabledDates` | `disabled-dates` | A list of whitespace-separated ISO dates that should be disabled. The property accepts an array. | `string \| string[] \| Date[]` | — |
| `disabledDaysOfWeek` | `disabled-days-of-week` | Weekdays to disable. Accepts a space-separated list of three-letter weekday names: `sun`, `mon`, `tue`, `wed`, `thu`, `fri`, `sat` | `string` | `''` |
| `disablePast` | `disable-past` | Disable all dates strictly before `today`. | `boolean` | `false` |
| `disableFuture` | `disable-future` | Disable all dates strictly after `today`. | `boolean` | `false` |
| `minRange` | `min-range` | Minimum range length in days (`mode="range"` only). `0` disables the check. | `number` | `0` |
| `maxRange` | `max-range` | Maximum range length in days (`mode="range"` only). `0` disables the check. | `number` | `0` |
| `size` | `size` | Visual size. | `WaDatePickerSize \| 'small' \| 'medium' \| 'large'` | `'m'` |
| `locale` | `locale` | BCP-47 locale override. When empty, the inherited `lang` attribute is used. | `string` | `''` |
| `isDateDisabled` | — | Author-supplied predicate that returns `true` when a date should be disabled. Runs in addition to declarative `min` / `max` / `disabled-*` rules. JavaScript-only — set via property, not attribute. | `(date: Date) => boolean \| undefined` | — |
| `dayContent` | — | Author-supplied function that returns custom content for a day cell. Receives a `Date` and returns a string of HTML, a Lit `TemplateResult`, or `null` to use the default day number. Runs for every rendered day cell (including outside days). A `day-YYYY-MM-DD` slot, when provided for the same date, wins over this function. Property only. | `WaDatePickerDayContent \| undefined` | — |
| `valueAsDate` | — | Read-only convenience getter: returns the selected date in `mode="single"`. | `Date \| null` | — |
| `valueAsRange` | — | Read-only convenience getter: returns the selected range in `mode="range"`. | `WaDatePickerRange` | — |

## Methods

| Name | Description | Arguments |
| --- | --- | --- |
| `focus()` | Focuses the calendar at the currently focused day. | `options: FocusOptions` |
| `goToDate()` | Scrolls the view to show the given date and sets the focused day. | `date: string \\| Date` |
| `goToToday()` | Equivalent to `goToDate(today)`. | — |
| `clear()` | Clears the current selection and emits `input` then `change`. | — |

## Events

| Name | Description |
| --- | --- |
| `input` | Emitted when the value changes during interaction. In range mode, this fires after the first click of a new range. |
| `change` | Emitted when the user commits a new value. Read the current value from `event.target.value`. |
| `wa-focus-day` | Emitted when the focused day changes via keyboard navigation, paging, or pointer hover. `event.detail` is `{ date: Date }`. |
| `wa-view-change` | Emitted when the date picker switches between day, month, and year views. `event.detail` is `{ view, date }`. |

## Custom States

| Name | Description | CSS selector |
| --- | --- | --- |
| `disabled` | The date picker is disabled. | `:state(disabled)` |
| `readonly` | The date picker is readonly. | `:state(readonly)` |
| `range` | The date picker is in range mode. | `:state(range)` |

## CSS Parts

| Name | Description | CSS selector |
| --- | --- | --- |
| \`base\` | The component's outer wrapper. | \`::part(base)\` |
| \`day\` | \`day\` A cell button. State-specific parts are added in addition to day so you can target them with ::part(day-...). | \`::part(day)\` |
| \`day-disabled\` | Added when the day is disabled. | \`::part(day-disabled)\` |
| \`day-label\` | The label text inside a day button. | \`::part(day-label)\` |
| \`day-outside\` | \`with-outside-days\` Added when the day belongs to an adjacent month (requires ). | \`::part(day-outside)\` |
| \`day-placeholder\` | \`with-outside-days\` An empty cell rendered in trailing rows when is off, so the grid is always 6 rows tall and the calendar's height doesn't shift between months. | \`::part(day-placeholder)\` |
| \`day-range-end\` | Added to the second endpoint of a range. | \`::part(day-range-end)\` |
| \`day-range-inner\` | Added to days that fall between the two endpoints of a committed range. | \`::part(day-range-inner)\` |
| \`day-range-preview\` | Added to days inside the hover preview span during an in-progress range. | \`::part(day-range-preview)\` |
| \`day-range-start\` | Added to the first endpoint of a range. | \`::part(day-range-start)\` |
| \`day-selected\` | Added when the day is selected (single mode or a range endpoint). | \`::part(day-selected)\` |
| \`day-today\` | Added to the day cell that represents today. | \`::part(day-today)\` |
| \`day-weekend\` | Added when the day falls on a weekend per the locale's week info. | \`::part(day-weekend)\` |
| \`footer\` | \`footer\` The container wrapping the slot. | \`::part(footer)\` |
| \`grid\` | \`\` The day grid for a month. | \`::part(grid)\` |
| \`header\` | The header row containing the title and navigation buttons. | \`::part(header)\` |
| \`month\` | A single rendered month. | \`::part(month)\` |
| \`month-label\` | \`months\` The label rendered above each month when is greater than 1. | \`::part(month-label)\` |
| \`months\` | The container that holds the rendered month(s). | \`::part(months)\` |
| \`nav\` | The container around the previous and next buttons. | \`::part(nav)\` |
| \`next\` | The next-page button. | \`::part(next)\` |
| \`previous\` | The previous-page button. | \`::part(previous)\` |
| \`title\` | The clickable month/year title button that steps the view up (days → months → years). | \`::part(title)\` |
| \`view-cell\` | \`display: contents\` The gridcell wrapper around a single view item. Transparent to layout (). | \`::part(view-cell)\` |
| \`view-grid\` | The grid used when the picker is in month-select or year-select view. | \`::part(view-grid)\` |
| \`view-item\` | A single month or year button inside the view grid. State-specific parts are added as siblings. | \`::part(view-item)\` |
| \`view-item-disabled\` | Added when every day in the month/year is disabled. | \`::part(view-item-disabled)\` |
| \`view-item-selected\` | Added to the month/year that matches the current selection. | \`::part(view-item-selected)\` |
| \`view-item-today\` | Added to the month/year representing today. | \`::part(view-item-today)\` |
| \`view-row\` | \`display: contents\` A row of three items inside the view grid. Transparent to layout (). | \`::part(view-row)\` |
| \`weekday\` | A single weekday label cell. | \`::part(weekday)\` |
| \`weekdays\` | The row of weekday labels above each month grid. | \`::part(weekdays)\` |
| \`weeknumber\` | A single week-number cell. | \`::part(weeknumber)\` |
| \`weeknumbers\` | The week-number column header cell. | \`::part(weeknumbers)\` |

## Dependencies

This component automatically imports the following elements. Sub-dependencies, if any exist, will also be included in this list.

-   [`<wa-icon>`](https://webawesome.com/docs/components/icon)

## Examples

### Initial Value

By default, the calendar is in single-date mode. Set the `value` attribute to an ISO date string (`YYYY-MM-DD`) to preselect a date.

```html
<wa-date-picker style="max-width: 300px;" value="2026-05-11"></wa-date-picker>
```

The `value` attribute always uses ISO 8601 date-only strings: `YYYY-MM-DD` for single dates and `YYYY-MM-DD/YYYY-MM-DD` for ranges. The JavaScript setter also accepts `Date` objects and `{ from, to }` shapes for ergonomic use in code.

```html
<script type="module">
  const picker = document.querySelector('wa-date-picker');

  // Strings
  picker.value = '2026-05-11';
  picker.value = '2026-05-11/2026-05-18';

  // Date objects (single mode)
  picker.value = new Date(2026, 4, 11);

  // Range objects
  picker.value = { from: '2026-05-11', to: '2026-05-18' };
  picker.value = { from: new Date(2026, 4, 11), to: new Date(2026, 4, 18) };
</script>
```

Read the value back with the read-only `valueAsDate` and `valueAsRange` properties for typed access.

```html
<script type="module">
  const picker = document.querySelector('wa-date-picker');

  // mode="single"
  const date = picker.valueAsDate; // Date | null

  // mode="range"
  const { from, to } = picker.valueAsRange; // { from: Date | null; to: Date | null }
</script>
```

### Min & Max

Use `min` and `max` to bound the selectable range. Days outside the bounds are disabled and not focusable.

```html
<wa-date-picker style="max-width: 300px;" min="2026-05-08" max="2026-05-22" value="2026-05-15"></wa-date-picker>
```

### Disable Past & Future

Use `disable-past` or `disable-future` to disable all dates before or after today. These are convenience shortcuts that don't require knowing today's date in advance.

```html
<wa-date-picker style="max-width: 300px;" disable-past></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" disable-future></wa-date-picker>
```

### Date Range

Use `mode="range"` to let users pick a start and end date. In range mode, the value is two ISO dates separated by a forward slash, e.g. `2026-05-11/2026-05-18`.

```html
<wa-date-picker style="max-width: 600px;" mode="range" months="2" value="2026-05-11/2026-05-18"></wa-date-picker>
```

It's a good practice to set `months="2"` in range mode so users can see both ends of the range at once.

### Two-Month Display

Set `months="2"` to render two months side-by-side. This is especially useful in range mode so users can see both ends of a long range at once.

```html
<wa-date-picker style="max-width: 600px;" mode="range" months="2"></wa-date-picker>
```

By default, the previous/next buttons advance the visible range by two months. Set `page-by="single"` to advance one month at a time instead.

```html
<wa-date-picker style="max-width: 600px;" months="2" page-by="single"></wa-date-picker>
```

### Sizes

Use the `size` attribute to set the calendar's size.

```html
<wa-date-picker style="max-width: 300px;" size="s"></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" size="m"></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" size="l"></wa-date-picker>
```

### Disabled

Use the `disabled` attribute to disable the entire calendar.

```html
<wa-date-picker style="max-width: 300px;" disabled value="2026-05-11"></wa-date-picker>
```

### Read Only

Use the `readonly` attribute to display the current value without allowing changes. Cells remain focusable so users can still navigate the grid with the keyboard.

```html
<wa-date-picker style="max-width: 300px;" readonly value="2026-05-11"></wa-date-picker>
```

### Disabling Specific Dates

Use `disabled-dates` with a whitespace-separated list of ISO dates to disable individual days. This is useful for holidays or blackout dates. Whitespace around each date is trimmed, so you can put dates on separate lines for readability.

```html
<wa-date-picker
  style="max-width: 300px;"
  value="2026-05-11"
  disabled-dates="
    2026-05-25
    2026-05-26
    2026-07-04
  "
></wa-date-picker>
```

### Disabling Days of the Week

Use `disabled-days-of-week` to disable specific weekdays. The attribute accepts a space-separated list of three-letter weekday names (`sun`, `mon`, `tue`, `wed`, `thu`, `fri`, `sat`), in any order.

```html
<wa-date-picker style="max-width: 300px;" disabled-days-of-week="sun sat"></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" disabled-days-of-week="mon wed fri"></wa-date-picker>
```

### Range Length Constraints

Use `min-range` and `max-range` to enforce a minimum or maximum range length in days. If the user's second click violates the constraint, the range is not committed.

```html
<wa-date-picker style="max-width: 600px;" mode="range" months="2" min-range="3" max-range="14"></wa-date-picker>
```

### Localization

Set the `locale` attribute to a BCP-47 language tag. When omitted, the calendar uses the inherited `lang` attribute. Month and weekday names, the first day of the week, and weekend designations are all locale-aware.

```html
<wa-date-picker style="max-width: 300px;" locale="es"></wa-date-picker> <br /><br />
<wa-date-picker style="max-width: 300px;" locale="ko"></wa-date-picker>
```

### First Day of the Week

By default, the first day of the week is determined by the locale. Set `first-day-of-week` to a three-letter weekday name (`sun`, `mon`, `tue`, `wed`, `thu`, `fri`, or `sat`) to override.

```html
<wa-date-picker style="max-width: 300px;" first-day-of-week="mon"></wa-date-picker>
```

### Weekday Format

Use `weekday-format` to control how weekday headers are rendered.

```html
<wa-date-picker style="max-width: 300px;" weekday-format="narrow"></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" weekday-format="short"></wa-date-picker>
```

```html
<wa-date-picker weekday-format="long"></wa-date-picker>
```

### Week Numbers

Use `with-week-numbers` to display ISO 8601 week numbers in a column on the left.

```html
<wa-date-picker style="max-width: 300px;" with-week-numbers></wa-date-picker>
```

### Outside Days

Use `with-outside-days` to render the trailing days of the previous month and the leading days of the next month inside the grid.

```html
<wa-date-picker style="max-width: 300px;" with-outside-days></wa-date-picker>
```

### View Stepper

Use the `view` attribute to set the initial view. Accepted values are `days` (default), `months`, and `years`.

```html
<wa-date-picker style="max-width: 300px;" view="months"></wa-date-picker>
```

```html
<wa-date-picker style="max-width: 300px;" view="years"></wa-date-picker>
```

### Conditionally Disabling Dates

For complex rules (e.g. holidays loaded from an API, weekday + odd-week conditions), set the `isDateDisabled` JavaScript property to a function that receives a `Date` and returns `true` to disable it. This runs in addition to the declarative `disabled-*` attributes — a date is disabled if _any_ rule matches it.

```html
<wa-date-picker style="max-width: 300px;" class="custom-disabled"></wa-date-picker>

<script type="module">
  import { allDefined } from '/dist/webawesome.js';
  await allDefined();

  const picker = document.querySelector('.custom-disabled');
  // Disable every other Monday
  picker.isDateDisabled = date => {
    if (date.getDay() !== 1) return false;
    const week = Math.floor(date.getDate() / 7);
    return week % 2 === 0;
  };
</script>
```

### Custom Day Content

To render custom content inside a specific day cell, provide a slot named `day-YYYY-MM-DD`. The slotted content replaces the day's label but the button chrome (data attributes, ARIA, click handling) is preserved. This is useful for badges, dots, or holiday labels.

```html
<wa-date-picker style="max-width: 300px;" value="2026-12-25">
  <span slot="day-2026-01-01">🎆</span>
  <span slot="day-2026-02-14">❤️</span>
  <span slot="day-2026-03-17">🍀</span>
  <span slot="day-2026-04-22">🌍</span>
  <span slot="day-2026-10-31">🎃</span>
  <span slot="day-2026-12-25">🎄</span>
</wa-date-picker>
```

### Dynamic Day Content

For content that follows a rule across many dates (e.g. paydays, recurring events, moving holidays), set the `dayContent` JavaScript property to a function that receives a `Date` and returns a string of HTML, a Lit `TemplateResult`, or `null` to use the default label. This runs for every rendered day cell, so it scales to any year the user navigates to.

If both a `day-YYYY-MM-DD` slot and `dayContent` apply to the same date, the slot wins.

```html
<wa-date-picker style="max-width: 300px;" class="dynamic-days"></wa-date-picker>

<script type="module">
  import { allDefined } from '/dist/webawesome.js';
  await allDefined();

  const picker = document.querySelector('.dynamic-days');
  picker.dayContent = date => {
    const day = date.getDate();
    const lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
    let i = 1;
    if (day === 15 || day === lastDay)
      return `
      <wa-tooltip for="brian-${i}">I thought you'd never ask</wa-tooltip>
      <span id="brian-${i}">💰</span>
    `;
    i++;
    return null;
  };
</script>
```

### Custom Footer

Add buttons or other content below the calendar using the `footer` slot. The picker exposes `goToToday()` and `clear()` methods for common shortcut buttons.

```html
<wa-date-picker style="max-width: 300px;" class="footer-example">
  <div slot="footer">
    <wa-button class="today-btn" size="s" appearance="outlined">Today</wa-button>
    <wa-button class="clear-btn" size="s" appearance="outlined">Clear</wa-button>
  </div>
</wa-date-picker>

<script type="module">
  import { allDefined } from '/dist/webawesome.js';
  await allDefined();

  const picker = document.querySelector('.footer-example');
  picker.querySelector('.today-btn').addEventListener('click', () => {
    picker.value = new Date();
    picker.goToToday();
  });
  picker.querySelector('.clear-btn').addEventListener('click', () => picker.clear());
</script>
```

### Custom Navigation Icons

Replace the default chevrons by slotting your own icons into `previous-icon` and `next-icon`.

```html
<wa-date-picker style="max-width: 300px;">
  <wa-icon slot="previous-icon" name="caret-left" variant="solid"></wa-icon>
  <wa-icon slot="next-icon" name="caret-right" variant="solid"></wa-icon>
</wa-date-picker>
```
