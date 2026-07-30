<!-- Source: reference doc bundled in the Web Awesome 3.11.0 release zip (dist/skills/webawesome/references/components/bar-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/bar-chart -->

# Bar Chart [Pro]

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
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma) Newer additions to Web Awesome, like [`<wa-toast>`](https://webawesome.com/docs/components/toast), aren't included in the currently available kit, but a new version is in the works.  
    Track its progress on GitHub.
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Bar Chart!

```html
<wa-bar-chart
  label="Quarterly Revenue"
  description="A bar chart comparing online and in-store revenue across four quarters"
>
  <script type="application/json">
    {
      "data": {
        "labels": ["Q1", "Q2", "Q3", "Q4"],
        "datasets": [
          { "label": "Online", "data": [42, 58, 63, 71] },
          { "label": "In-Store", "data": [65, 53, 48, 52] }
        ]
      }
    }
  </script>
</wa-bar-chart>
```

See [`<wa-chart>`](https://webawesome.com/docs/components/chart) for advanced configuration, custom plugins, and direct Chart.js access.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.11.0/components/bar-chart/bar-chart.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/bar-chart/bar-chart.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/bar-chart/bar-chart.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaBarChart from '@awesome.me/webawesome/dist/react/bar-chart/index.js';
```

### Slots

| Name | Description |
| --- | --- |
| (default) | \`

### Attributes & Properties

| Name | Description | Reflects |
| --- | --- | --- |
| \`config\` | \`ChartJS\['config'\]\` The Chart.js configuration object. Setting this property will automatically re-render the chart. Type | |
| \`description\` description | \`string \\| null\` A description of the chart, used for accessibility. Type Default null | |
| \`grid\` grid | \`'x' \\| 'y' \\| 'both' \\| 'none'\` Which axes to show grid lines on. Type Default 'both' | |
| \`indexAxis\` index-axis | \`'x' \\| 'y'\` The base axis of the dataset. 'x' for vertical bars and 'y' for horizontal bars. Type Default 'x' | |
| \`label\` label | \`string \\| null\` A label for the chart, used for accessibility. Type Default null | |
| \`legendPosition\` legend-position | \`LayoutPosition \\| 'start' \\| 'end'\` The position of the legend relative to the chart. Type Default 'top' | |
| \`max\` max | \`number \\| null\` The maximum value for the value axis. Type Default null | |
| \`min\` min | \`number \\| null\` The minimum value for the value axis. Type Default null | |
| \`plugins\` plugins | \`array\` Additional Chart.js plugins to register for this chart instance. Type Default \[\] | |
| \`stacked\` stacked | \`boolean\` Stacks datasets on top of each other along the value axis. Type Default false | |
| \`type\` type | \`bar\` The type of chart to render. Valid types include , line, pie, doughnut, polarArea, radar, scatter, and bubble. Type ChartType Default 'bar' | |
| \`withoutAnimation\` without-animation | \`boolean\` Disables chart animations Type Default false | |
| \`withoutLegend\` without-legend | \`boolean\` Hides the legend Type Default false | |
| \`withoutTooltip\` without-tooltip | \`boolean\` Hides tooltips over data points Type Default false | |
| \`xLabel\` x-label | \`string \\| null\` A label for the x-axis. Type Default null | |
| \`yLabel\` y-label | \`string \\| null\` A label for the y-axis. Type Default null | |

### CSS Custom Properties

| Name | Description |
| --- | --- |
| \`--border-color-1\` | \`var(--wa-color-blue-60)\` Border color for the first dataset. Default |
| \`--border-color-2\` | \`var(--wa-color-pink-60)\` Border color for the second dataset. Default |
| \`--border-color-3\` | \`var(--wa-color-green-60)\` Border color for the third dataset. Default |
| \`--border-color-4\` | \`var(--wa-color-yellow-60)\` Border color for the fourth dataset. Default |
| \`--border-color-5\` | \`var(--wa-color-purple-60)\` Border color for the fifth dataset. Default |
| \`--border-color-6\` | \`var(--wa-color-orange-60)\` Border color for the sixth dataset. Default |
| \`--border-radius\` | \`var(--wa-border-radius-s)\` Border radius for bar charts. Default |
| \`--border-width\` | \`var(--wa-border-width-s)\` Border width for bars and arcs. Default |
| \`--fill-color-1\` | \`color-mix(in srgb, var(--wa-color-blue-60) 40%, transparent)\` Fill color for the first dataset. Default |
| \`--fill-color-2\` | \`color-mix(in srgb, var(--wa-color-pink-60) 40%, transparent)\` Fill color for the second dataset. Default |
| \`--fill-color-3\` | \`color-mix(in srgb, var(--wa-color-green-60) 40%, transparent)\` Fill color for the third dataset. Default |
| \`--fill-color-4\` | \`color-mix(in srgb, var(--wa-color-yellow-60) 40%, transparent)\` Fill color for the fourth dataset. Default |
| \`--fill-color-5\` | \`color-mix(in srgb, var(--wa-color-purple-60) 40%, transparent)\` Fill color for the fifth dataset. Default |
| \`--fill-color-6\` | \`color-mix(in srgb, var(--wa-color-orange-60) 40%, transparent)\` Fill color for the sixth dataset. Default |
| \`--grid-border-width\` | \`var(--wa-border-width-s)\` Border width for chart grid lines and axis borders. Default |
| \`--grid-color\` | \`var(--wa-color-neutral-border-quiet)\` Color of the chart grid lines and axis borders. Default |
| \`--line-border-width\` | \`var(--wa-border-width-m)\` Border width for line and radar charts. Default |
| \`--point-radius\` | \`var(--wa-border-width-m)\` Radius of data point dots. Default |

## Examples

### Providing Data with JSON

Place a `<script type="application/json">` tag inside the component with your chart data. The `type` field can be omitted since `wa-bar-chart` already knows its chart type. The JSON follows the [Chart.js configuration format](https://www.chartjs.org/docs/latest/configuration/).

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

### Providing Data with JavaScript

Set the `config` property from JavaScript when your data comes from code rather than static markup. The chart re-renders automatically each time you assign it. For data that updates at runtime, try the live controls in [Accessing the Chart.js Instance](https://webawesome.com/docs/components/chart#accessing-the-chartjs-instance).

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

**`config` is shallowly reactive.**  
If you mutate the object in place, reassign it to trigger a re-render.

### Multiple Datasets

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

### Horizontal Bars

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

### Colors

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
        { label: 'Hot Brew', data: [72, 54, 48, 68] },
        { label: 'Cold Brew', data: [38, 61, 74, 45] },
      ],
    },
  };
</script>
```

### Border Width

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
      labels: ['Oak Grove', 'Pine Ridge', 'Maple Hill', 'Willow Creek'],
      datasets: [
        {
          label: 'Trees Planted',
          data: [180, 240, 320, 150],
        },
      ],
    },
  };
</script>
```

### Legend

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

### Disabling Features

Use `without-tooltip` to hide hover tooltips and `without-animation` to disable transitions.

```html
<wa-bar-chart
  id="bar-disabled"
  without-tooltip
  without-animation
  label="Minimal"
  description="A bar chart with tooltips and animations disabled"
>
</wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-disabled');

  chart.config = {
    data: {
      labels: ['Fiction', 'Mystery', 'Sci-Fi', 'Biography'],
      datasets: [
        {
          label: 'Checkouts',
          data: [420, 310, 260, 180],
        },
      ],
    },
  };
</script>
```
