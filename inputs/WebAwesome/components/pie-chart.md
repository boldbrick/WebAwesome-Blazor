<!-- Source: reference doc bundled in the Web Awesome 3.15.0 release zip (dist/skills/webawesome/references/components/pie-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/pie-chart -->

# Pie Chart [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-pie-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Pie charts show the proportional composition of a whole as slices of a circle. They work best with a small number of categories where the relative proportions matter more than exact values.

**[Get Pie Chart with Web Awesome Pro!](https://webawesome.com/pro?from=pro-docs&component=pie-chart)**

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
import 'https://ka-f.webawesome.com/webawesome@3.15.0/components/pie-chart/pie-chart.js';
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

Valid slot names for this component (use exactly these — any other `slot` value is
silently ignored and the element falls back to the default slot):

- `(default)` — An optional `<script type="application/json">` element containing the Chart.js configuration object.

### Attributes & Properties

| Property | Attribute | Description | Type | Default |
| --- | --- | --- | --- | --- |
| `type` | `type` | The type of chart to render. Valid types include `bar`, `line`, `pie`, `doughnut`, `polarArea`, `radar`, `scatter`, and `bubble`. | `ChartType` | `'pie'` |
| `label` | `label` | A label for the chart, used for accessibility. | `string \| null` | `null` |
| `description` | `description` | A description of the chart, used for accessibility. | `string \| null` | `null` |
| `xLabel` | `x-label` | A label for the x-axis. | `string \| null` | `null` |
| `yLabel` | `y-label` | A label for the y-axis. | `string \| null` | `null` |
| `legendPosition` | `legend-position` | The position of the legend relative to the chart. | `LayoutPosition \| 'start' \| 'end'` | `'top'` |
| `stacked` | `stacked` | Stacks datasets on top of each other along the value axis. | `boolean` | `false` |
| `indexAxis` | `index-axis` | The base axis of the dataset. 'x' for vertical bars and 'y' for horizontal bars. | `'x' \| 'y'` | `'x'` |
| `grid` | `grid` | Which axes to show grid lines on. | `'x' \| 'y' \| 'both' \| 'none'` | `'both'` |
| `min` | `min` | The minimum value for the value axis. | `number \| null` | `null` |
| `max` | `max` | The maximum value for the value axis. | `number \| null` | `null` |
| `withoutAnimation` | `without-animation` | Disables chart animations | `boolean` | `false` |
| `withoutLegend` | `without-legend` | Hides the legend | `boolean` | `false` |
| `withoutTooltip` | `without-tooltip` | Hides tooltips over data points | `boolean` | `false` |
| `config` | — | The Chart.js configuration object. Setting this property will automatically re-render the chart. | `ChartJS['config']` | — |
| `plugins` | `plugins` | Additional Chart.js plugins to register for this chart instance. | `array` | `[]` |

### CSS Custom Properties

| Name | Description | Default |
| --- | --- | --- |
| `--fill-color-1` | Fill color for the first dataset. | `color-mix(in srgb, var(--wa-color-blue-60) 40%, transparent)` |
| `--fill-color-2` | Fill color for the second dataset. | `color-mix(in srgb, var(--wa-color-pink-60) 40%, transparent)` |
| `--fill-color-3` | Fill color for the third dataset. | `color-mix(in srgb, var(--wa-color-green-60) 40%, transparent)` |
| `--fill-color-4` | Fill color for the fourth dataset. | `color-mix(in srgb, var(--wa-color-yellow-60) 40%, transparent)` |
| `--fill-color-5` | Fill color for the fifth dataset. | `color-mix(in srgb, var(--wa-color-purple-60) 40%, transparent)` |
| `--fill-color-6` | Fill color for the sixth dataset. | `color-mix(in srgb, var(--wa-color-orange-60) 40%, transparent)` |
| `--border-color-1` | Border color for the first dataset. | `var(--wa-color-blue-60)` |
| `--border-color-2` | Border color for the second dataset. | `var(--wa-color-pink-60)` |
| `--border-color-3` | Border color for the third dataset. | `var(--wa-color-green-60)` |
| `--border-color-4` | Border color for the fourth dataset. | `var(--wa-color-yellow-60)` |
| `--border-color-5` | Border color for the fifth dataset. | `var(--wa-color-purple-60)` |
| `--border-color-6` | Border color for the sixth dataset. | `var(--wa-color-orange-60)` |
| `--grid-color` | Color of the chart grid lines and axis borders. | `var(--wa-color-neutral-border-quiet)` |
| `--border-width` | Border width for bars and arcs. | `var(--wa-border-width-s)` |
| `--border-radius` | Border radius for bar charts. | `var(--wa-border-radius-s)` |
| `--grid-border-width` | Border width for chart grid lines and axis borders. | `var(--wa-border-width-s)` |
| `--line-border-width` | Border width for line and radar charts. | `var(--wa-border-width-m)` |
| `--point-radius` | Radius of data point dots. | `var(--wa-border-width-m)` |

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
