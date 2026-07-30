<!-- Source: reference doc bundled in the Web Awesome 3.11.0 release zip (dist/skills/webawesome/references/components/pie-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/pie-chart -->

# Pie Chart [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-pie-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Pie charts show the proportional composition of a whole as slices of a circle. They work best with a small number of categories where the relative proportions matter more than exact values.

**[Get Pie Chart with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=pie-chart)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Pie Chart!

```html
<wa-pie-chart
  label="Browser Market Share"
  description="A pie chart showing browser market share with Chrome leading at 65%"
>
  <script type="application/json">
    {
      "data": {
        "labels": ["Chrome", "Safari", "Firefox", "Edge", "Other"],
        "datasets": [{ "label": "Market Share", "data": [65, 18, 8, 5, 4] }]
      }
    }
  </script>
</wa-pie-chart>
```

See [`<wa-chart>`](https://webawesome.com/docs/components/chart) for advanced configuration, custom plugins, and direct Chart.js access.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.11.0/components/pie-chart/pie-chart.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/pie-chart/pie-chart.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/pie-chart/pie-chart.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaPieChart from '@awesome.me/webawesome/dist/react/pie-chart/index.js';
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
| \`type\` type | \`bar\` The type of chart to render. Valid types include , line, pie, doughnut, polarArea, radar, scatter, and bubble. Type ChartType Default 'pie' | |
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

Place a `<script type="application/json">` tag inside the component with your chart data. Each value in the `data` array corresponds to a label. The JSON follows the [Chart.js configuration format](https://www.chartjs.org/docs/latest/configuration/).

```html
<wa-pie-chart label="Budget Allocation" description="A pie chart showing how a budget is allocated across departments">
  <script type="application/json">
    {
      "data": {
        "labels": ["Engineering", "Marketing", "Sales", "Operations"],
        "datasets": [
          {
            "label": "Budget",
            "data": [40, 25, 20, 15]
          }
        ]
      }
    }
  </script>
</wa-pie-chart>
```

### Providing Data with JavaScript

Set the `config` property from JavaScript when your data comes from code rather than static markup. The chart re-renders automatically each time you assign it. For data that updates at runtime, try the live controls in [Accessing the Chart.js Instance](https://webawesome.com/docs/components/chart#accessing-the-chartjs-instance).

```html
<wa-pie-chart id="pie-js-example" label="Budget Allocation" description="A pie chart of budget allocation">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-js-example');

  chart.config = {
    data: {
      labels: ['Engineering', 'Marketing', 'Sales', 'Operations'],
      datasets: [
        {
          label: 'Budget',
          data: [40, 25, 20, 15],
        },
      ],
    },
  };
</script>
```

**`config` is shallowly reactive.**  
If you mutate the object in place, reassign it to trigger a re-render.

### Colors

Override the default color palette using the `--fill-color-*` and `--border-color-*` CSS custom properties to apply custom colors to each slice.

```html
<wa-pie-chart
  id="pie-colors"
  label="Custom Colors"
  description="A pie chart with custom slice colors"
  style="
    --fill-color-1: color-mix(in srgb, var(--wa-color-blue-60) 70%, transparent);
    --border-color-1: var(--wa-color-blue-60);
    --fill-color-2: color-mix(in srgb, var(--wa-color-cyan-60) 70%, transparent);
    --border-color-2: var(--wa-color-cyan-60);
    --fill-color-3: color-mix(in srgb, var(--wa-color-purple-60) 70%, transparent);
    --border-color-3: var(--wa-color-purple-60);
  "
>
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-colors');

  chart.config = {
    data: {
      labels: ['Desktop', 'Mobile', 'Tablet'],
      datasets: [
        {
          label: 'Traffic',
          data: [55, 35, 10],
        },
      ],
    },
  };
</script>
```

### Legend

Use the `legend-position` attribute to control where the legend appears. For pie charts, placing the legend on the side can help prevent overlap. Add `without-legend` to hide it entirely.

```html
<wa-pie-chart
  id="pie-legend"
  legend-position="right"
  label="Legend on Right"
  description="A pie chart with the legend on the right side"
>
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-legend');

  chart.config = {
    data: {
      labels: ['Rent', 'Food', 'Transport', 'Entertainment', 'Savings'],
      datasets: [
        {
          label: 'Monthly Spending',
          data: [35, 25, 15, 10, 15],
        },
      ],
    },
  };
</script>
```

### Disabling Features

Use `without-tooltip` to hide hover tooltips and `without-animation` to disable transitions.

```html
<wa-pie-chart
  id="pie-disabled"
  without-tooltip
  without-animation
  label="Minimal"
  description="A pie chart with tooltips and animations disabled"
>
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-disabled');

  chart.config = {
    data: {
      labels: ['Latte', 'Cold Brew', 'Espresso'],
      datasets: [
        {
          label: 'Daily Orders',
          data: [52, 30, 18],
        },
      ],
    },
  };
</script>
```
