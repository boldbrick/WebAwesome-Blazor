<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/bar-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/bar-chart -->

# Bar Chart [Pro]

**Full documentation:** https://webawesome.com/docs/components/bar-chart

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-bar-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Bar charts compare quantities across categories using rectangular bars. They work well for showing rankings, highlighting differences between groups, and tracking changes across time periods.

**[Get Bar Chart with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=bar-chart)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Bar Chart!

```html
<wa-bar-chart
  id="bar-hero"
  label="Quarterly Revenue"
  description="A bar chart comparing online and in-store revenue across four quarters"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-hero');

  chart.config = {
    data: {
      labels: ['Q1', 'Q2', 'Q3', 'Q4'],
      datasets: [
        { label: 'Online', data: [42, 58, 63, 71] },
        { label: 'In-Store', data: [65, 53, 48, 52] },
      ],
    },
  };
</script>
```

For advanced configuration such as mixed chart types, custom plugins, and direct Chart.js access, see [`<wa-chart>`](https://webawesome.com/docs/components/chart).

## Examples

Link to This Section

### Providing Data with JavaScript

Link to This Section

For dynamic data, set the `config` property directly. The chart will re-render automatically.

```html
<wa-bar-chart id="bar-js-example" label="Survey Results" description="A bar chart of survey results by category">
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-js-example');

  chart.config = {
    data: {
      labels: ['Excellent', 'Good', 'Average', 'Poor'],
      datasets: [
        {
          label: 'Responses',
          data: [45, 30, 18, 7],
        },
      ],
    },
  };
</script>
```

Note that `config` is shallowly reactive. If you mutate the existing object in place, you must reassign it to trigger a re-render!

### Providing Data with JSON

Link to This Section

Place a `<script type="application/json">` tag inside the component with your chart data. The `type` field can be omitted since `wa-bar-chart` already knows its chart type.

```html
<wa-bar-chart label="Survey Results" description="A bar chart of survey results by category">
  <script type="application/json">
    {
      "data": {
        "labels": ["Excellent", "Good", "Average", "Poor"],
        "datasets": [
          {
            "label": "Responses",
            "data": [45, 30, 18, 7]
          }
        ]
      }
    }
  </script>
</wa-bar-chart>
```

### Multiple Datasets

Link to This Section

Add multiple objects to the `datasets` array to compare groups side by side.

```html
<wa-bar-chart
  id="bar-multi"
  label="Quarterly Sales by Channel"
  description="A bar chart comparing three sales channels across four quarters"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-multi');

  chart.config = {
    data: {
      labels: ['Q1', 'Q2', 'Q3', 'Q4'],
      datasets: [
        { label: 'Online', data: [42, 58, 63, 71] },
        { label: 'In-Store', data: [65, 53, 48, 52] },
        { label: 'Wholesale', data: [28, 32, 35, 40] },
      ],
    },
  };
</script>
```

### Custom Colors

Link to This Section

Override the default color palette using the `--fill-color-*` and `--border-color-*` CSS custom properties on the component.

```html
<wa-bar-chart
  id="bar-colors"
  label="Custom Colors"
  description="A bar chart with custom purple and cyan colors"
  style="
    --fill-color-1: color-mix(in srgb, var(--wa-color-purple-60) 50%, transparent);
    --border-color-1: var(--wa-color-purple-60);
    --fill-color-2: color-mix(in srgb, var(--wa-color-cyan-60) 50%, transparent);
    --border-color-2: var(--wa-color-cyan-60);
  "
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-colors');

  chart.config = {
    data: {
      labels: ['Q1', 'Q2', 'Q3', 'Q4'],
      datasets: [
        { label: 'Product A', data: [42, 58, 63, 71] },
        { label: 'Product B', data: [65, 53, 48, 52] },
      ],
    },
  };
</script>
```

### Horizontal Bars

Link to This Section

Use the `orientation="horizontal"` attribute to render bars horizontally. This is useful when category labels are long or when you want to emphasize ranking.

```html
<wa-bar-chart
  id="bar-horizontal"
  orientation="horizontal"
  label="Department Satisfaction"
  description="A horizontal bar chart showing satisfaction scores by department"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-horizontal');

  chart.config = {
    data: {
      labels: ['Engineering', 'Marketing', 'Sales', 'Design', 'Support'],
      datasets: [
        {
          label: 'Satisfaction Score',
          data: [88, 76, 82, 91, 79],
        },
      ],
    },
  };
</script>
```

### Stacked Bars

Link to This Section

Use the `stacked` attribute to stack datasets on top of each other. This is helpful for showing how parts contribute to a total.

```html
<wa-bar-chart
  id="bar-stacked"
  stacked
  label="Monthly Expenses"
  description="A stacked bar chart showing expenses broken into categories"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-stacked');

  chart.config = {
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [
        { label: 'Rent', data: [1200, 1200, 1200, 1200, 1200, 1200] },
        { label: 'Utilities', data: [180, 160, 150, 140, 170, 190] },
        { label: 'Groceries', data: [420, 380, 450, 400, 430, 410] },
      ],
    },
  };
</script>
```

### Border Width

Link to This Section

Use the `--border-width` CSS custom property to control the thickness of bar borders.

```html
<wa-bar-chart
  id="bar-border"
  style="--border-width: 4px"
  label="Thick Borders"
  description="A bar chart with thicker bar borders"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-border');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C', 'D'],
      datasets: [
        {
          label: 'Values',
          data: [30, 50, 20, 40],
        },
      ],
    },
  };
</script>
```

### Legend

Link to This Section

Use the `legend-position` attribute to control where the legend appears. Supported values include `top` (default), `bottom`, `left`, `right`, `start`, and `end`. The `start` and `end` values are direction-aware and will flip in RTL layouts. Add `without-legend` to hide it entirely.

```html
<wa-bar-chart
  id="bar-legend"
  legend-position="bottom"
  label="Legend at Bottom"
  description="A bar chart with the legend positioned at the bottom"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-legend');

  chart.config = {
    data: {
      labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
      datasets: [
        { label: 'Completed', data: [12, 19, 8, 15, 22] },
        { label: 'Pending', data: [5, 3, 10, 7, 2] },
      ],
    },
  };
</script>
```

### Grid Lines

Link to This Section

Use the `grid` attribute to control which axes show grid lines. Options are `both` (default), `x`, `y`, and `none`.

```html
<wa-bar-chart
  id="bar-grid"
  grid="y"
  label="Y-Axis Grid Only"
  description="A bar chart showing only horizontal grid lines"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-grid');

  chart.config = {
    data: {
      labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
      datasets: [
        {
          label: 'Tasks Completed',
          data: [12, 19, 8, 15, 22],
        },
      ],
    },
  };
</script>
```

### Axis Labels

Link to This Section

Use the `x-label` and `y-label` attributes to add descriptive labels to each axis.

```html
<wa-bar-chart
  id="bar-axis"
  x-label="Product"
  y-label="Units Sold"
  label="Product Sales"
  description="A bar chart with labeled axes showing units sold per product"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-axis');

  chart.config = {
    data: {
      labels: ['Widget', 'Gadget', 'Gizmo', 'Doohickey'],
      datasets: [
        {
          label: 'Units Sold',
          data: [340, 220, 180, 95],
        },
      ],
    },
  };
</script>
```

### Axis Range

Link to This Section

Use the `min` and `max` attributes to constrain the value axis.

```html
<wa-bar-chart
  id="bar-range"
  min="0"
  max="100"
  label="Test Scores"
  description="A bar chart with a constrained y-axis from 0 to 100"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-range');

  chart.config = {
    data: {
      labels: ['Alice', 'Bob', 'Carol', 'Dave'],
      datasets: [
        {
          label: 'Score',
          data: [82, 91, 76, 88],
        },
      ],
    },
  };
</script>
```

### Disabling Tooltips

Link to This Section

Use the `without-tooltip` attribute to hide the tooltips that appear when hovering over data points.

```html
<wa-bar-chart id="bar-tooltip" without-tooltip label="No Tooltips" description="A bar chart with tooltips disabled">
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-tooltip');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C', 'D'],
      datasets: [
        {
          label: 'Values',
          data: [30, 50, 20, 40],
        },
      ],
    },
  };
</script>
```

### Disabling Animations

Link to This Section

Use the `without-animation` attribute to disable chart transitions.

```html
<wa-bar-chart id="bar-anim" without-animation label="No Animation" description="A bar chart with animation disabled">
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-anim');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C', 'D'],
      datasets: [
        {
          label: 'Values',
          data: [30, 50, 20, 40],
        },
      ],
    },
  };
</script>
```

## Slots

Valid slot names for this component (use exactly these — any other `slot` value
is silently ignored and the element falls back to the default slot):

- `(default)` — An optional `<script type="application/json">` element containing the Chart.js configuration object.

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `type` |  | `ChartType` | `'bar'` | The type of chart to render. Valid types include `bar`, `line`, `pie`, `doughnut`, `polarArea`, `radar`, `scatter`, and `bubble`. |
| `orientation` |  | `'vertical' \| 'horizontal'` | `'vertical'` |  |
| `label` |  | `string \| null` | `null` | A label for the chart, used for accessibility. |
| `description` |  | `string \| null` | `null` | A description of the chart, used for accessibility. |
| `xLabel` |  | `string \| null` | `null` | A label for the x-axis. |
| `yLabel` |  | `string \| null` | `null` | A label for the y-axis. |
| `legend-position` | `legendPosition` | `LayoutPosition \| 'start' \| 'end'` | `'top'` | The position of the legend relative to the chart. |
| `stacked` |  | `boolean` | `false` | Stacks datasets on top of each other along the value axis. |
| `index-axis` | `indexAxis` | `'x' \| 'y'` | `'x'` | The base axis of the dataset. 'x' for vertical bars and 'y' for horizontal bars. |
| `grid` |  | `'x' \| 'y' \| 'both' \| 'none'` | `'both'` | Which axes to show grid lines on. |
| `min` |  | `number \| null` | `null` | The minimum value for the value axis. |
| `max` |  | `number \| null` | `null` | The maximum value for the value axis. |
| `without-animation` | `withoutAnimation` | `boolean` | `false` | Disables chart animations |
| `without-legend` | `withoutLegend` | `boolean` | `false` | Hides the legend |
| `without-tooltip` | `withoutTooltip` | `boolean` | `false` | Hides tooltips over data points |
| `plugins` |  | `array` | `[]` | Additional Chart.js plugins to register for this chart instance. |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--fill-color-1` | `color-mix(in srgb, var(--wa-color-blue-60) 40%, transparent)` | Fill color for the first dataset. |
| `--fill-color-2` | `color-mix(in srgb, var(--wa-color-pink-60) 40%, transparent)` | Fill color for the second dataset. |
| `--fill-color-3` | `color-mix(in srgb, var(--wa-color-green-60) 40%, transparent)` | Fill color for the third dataset. |
| `--fill-color-4` | `color-mix(in srgb, var(--wa-color-yellow-60) 40%, transparent)` | Fill color for the fourth dataset. |
| `--fill-color-5` | `color-mix(in srgb, var(--wa-color-purple-60) 40%, transparent)` | Fill color for the fifth dataset. |
| `--fill-color-6` | `color-mix(in srgb, var(--wa-color-orange-60) 40%, transparent)` | Fill color for the sixth dataset. |
| `--border-color-1` | `var(--wa-color-blue-60)` | Border color for the first dataset. |
| `--border-color-2` | `var(--wa-color-pink-60)` | Border color for the second dataset. |
| `--border-color-3` | `var(--wa-color-green-60)` | Border color for the third dataset. |
| `--border-color-4` | `var(--wa-color-yellow-60)` | Border color for the fourth dataset. |
| `--border-color-5` | `var(--wa-color-purple-60)` | Border color for the fifth dataset. |
| `--border-color-6` | `var(--wa-color-orange-60)` | Border color for the sixth dataset. |
| `--grid-color` | `var(--wa-color-neutral-border-quiet)` | Color of the chart grid lines and axis borders. |
| `--border-width` | `var(--wa-border-width-s)` | Border width for bars and arcs. |
| `--border-radius` | `var(--wa-border-radius-s)` | Border radius for bar charts. |
| `--grid-border-width` | `var(--wa-border-width-s)` | Border width for chart grid lines and axis borders. |
| `--line-border-width` | `var(--wa-border-width-m)` | Border width for line and radar charts. |
| `--point-radius` | `var(--wa-border-width-m)` | Radius of data point dots. |
