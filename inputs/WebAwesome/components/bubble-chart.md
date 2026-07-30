<!-- Source: reference doc bundled in the Web Awesome 3.11.0 release zip (dist/skills/webawesome/references/components/bubble-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/bubble-chart -->

# Bubble Chart [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-bubble-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Bubble charts add a third dimension to scatter plots by varying the size of each data point. They are useful for visualizing relationships where a third variable adds meaning beyond a simple x/y correlation.

**[Get Bubble Chart with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=bubble-chart)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Bubble Chart!

```html
<wa-bubble-chart
  label="City Comparison"
  description="A bubble chart comparing cities by cost of living, quality of life, and population"
>
  <script type="application/json">
    {
      "data": {
        "datasets": [
          {
            "label": "North America",
            "data": [
              { "x": 65, "y": 7.8, "r": 18 },
              { "x": 50, "y": 7.0, "r": 12 },
              { "x": 55, "y": 7.5, "r": 14 }
            ]
          },
          {
            "label": "Europe",
            "data": [
              { "x": 40, "y": 8.2, "r": 16 },
              { "x": 30, "y": 7.6, "r": 10 },
              { "x": 45, "y": 8.0, "r": 13 }
            ]
          }
        ]
      }
    }
  </script>
</wa-bubble-chart>
```

Unlike bar or line charts, bubble data is an array of `{x, y, r}` point objects:

| Property | Description |
| --- | --- |
| \`x\` | Position along the x-axis |
| \`y\` | Position along the y-axis |
| \`r\` | Bubble radius in pixels, fixed and not scaled to the axes |

See [`<wa-chart>`](https://webawesome.com/docs/components/chart) for advanced configuration, custom plugins, and direct Chart.js access.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.11.0/components/bubble-chart/bubble-chart.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/bubble-chart/bubble-chart.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/bubble-chart/bubble-chart.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaBubbleChart from '@awesome.me/webawesome/dist/react/bubble-chart/index.js';
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
| \`type\` type | \`bar\` The type of chart to render. Valid types include , line, pie, doughnut, polarArea, radar, scatter, and bubble. Type ChartType Default 'bubble' | |
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

Place a `<script type="application/json">` tag inside the component. Each data point is an object with `x`, `y`, and `r` properties. The JSON follows the [Chart.js configuration format](https://www.chartjs.org/docs/latest/configuration/).

```html
<wa-bubble-chart
  label="Product Analysis"
  description="A bubble chart showing products by price, rating, and sales volume"
>
  <script type="application/json">
    {
      "data": {
        "datasets": [
          {
            "label": "Products",
            "data": [
              { "x": 30, "y": 4.2, "r": 18 },
              { "x": 50, "y": 4.5, "r": 14 },
              { "x": 25, "y": 3.9, "r": 22 },
              { "x": 55, "y": 4.7, "r": 10 },
              { "x": 40, "y": 4.0, "r": 16 }
            ]
          }
        ]
      }
    }
  </script>
</wa-bubble-chart>
```

### Providing Data with JavaScript

Set the `config` property from JavaScript when your data comes from code rather than static markup. The chart re-renders automatically each time you assign it. For data that updates at runtime, try the live controls in [Accessing the Chart.js Instance](https://webawesome.com/docs/components/chart#accessing-the-chartjs-instance).

```html
<wa-bubble-chart
  id="bubble-js-example"
  label="Product Analysis"
  description="A bubble chart of products by price, rating, and volume"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-js-example');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Products',
          data: [
            { x: 30, y: 4.2, r: 18 },
            { x: 50, y: 4.5, r: 14 },
            { x: 25, y: 3.9, r: 22 },
            { x: 55, y: 4.7, r: 10 },
            { x: 40, y: 4.0, r: 16 },
          ],
        },
      ],
    },
  };
</script>
```

**`config` is shallowly reactive.**  
If you mutate the object in place, reassign it to trigger a re-render.

### Custom Tooltips

Bubble tooltips show raw `x, y, r` values by default. Attach a property to each data point and read it in a [tooltip callback](https://webawesome.com/docs/components/chart#custom-tooltips) to name points and spell out what the bubble size means.

```html
<wa-bubble-chart
  id="bubble-tooltips"
  x-label="Price"
  y-label="Rating"
  label="Menu Performance"
  description="A bubble chart whose tooltips name each drink and its sales instead of showing raw coordinates"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-tooltips');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Menu Items',
          data: [
            { x: 3.25, y: 4.3, r: 12, name: 'Drip Coffee' },
            { x: 4.25, y: 4.5, r: 18, name: 'Espresso' },
            { x: 4.75, y: 4.6, r: 22, name: 'Latte' },
            { x: 5.0, y: 4.7, r: 10, name: 'Cold Brew' },
          ],
        },
      ],
    },
    options: {
      plugins: {
        tooltip: {
          callbacks: {
            label: context => {
              const { name, x, y, r } = context.raw;
              return `${name}: $${x}, ${y}★, ${r} sold`;
            },
          },
        },
      },
    },
  };
</script>
```

### Bubble Sizes

The `r` property on each data point sets the bubble radius in pixels. Unlike x and y, this value is absolute and not mapped to a chart scale. Use larger values to represent greater magnitude.

```html
<wa-bubble-chart
  id="bubble-size-example"
  label="Investment Portfolio"
  description="A bubble chart where bubble size represents investment amount"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-size-example');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Investments',
          data: [
            { x: 4, y: 12, r: 28 },
            { x: 6, y: 8, r: 14 },
            { x: 7, y: 14, r: 22 },
            { x: 3, y: 7, r: 10 },
            { x: 8, y: 16, r: 18 },
          ],
        },
      ],
    },
  };
</script>
```

### Multiple Datasets

Use multiple datasets to compare groups of bubbles. Each group gets its own color.

```html
<wa-bubble-chart
  id="bubble-multi"
  label="Industry Comparison"
  description="A bubble chart comparing metrics across two industry sectors"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-multi');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Technology',
          data: [
            { x: 70, y: 8.5, r: 18 },
            { x: 55, y: 7.8, r: 14 },
            { x: 65, y: 8.2, r: 12 },
          ],
        },
        {
          label: 'Healthcare',
          data: [
            { x: 40, y: 7.2, r: 16 },
            { x: 50, y: 7.6, r: 18 },
            { x: 35, y: 7.5, r: 12 },
          ],
        },
      ],
    },
  };
</script>
```

### Colors

Override the default color palette using the `--fill-color-*` and `--border-color-*` CSS custom properties on the component.

```html
<wa-bubble-chart
  id="bubble-colors"
  label="Custom Colors"
  description="A bubble chart with custom colors"
  style="
    --fill-color-1: color-mix(in srgb, var(--wa-color-orange-60) 50%, transparent);
    --border-color-1: var(--wa-color-orange-60);
  "
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-colors');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Menu Items',
          data: [
            { x: 3.25, y: 4.3, r: 16 },
            { x: 4.75, y: 4.6, r: 22 },
            { x: 4.25, y: 4.5, r: 18 },
            { x: 5.0, y: 4.7, r: 14 },
          ],
        },
      ],
    },
  };
</script>
```

### Legend

Use the `legend-position` attribute to control where the legend appears. Add `without-legend` to hide it entirely.

```html
<wa-bubble-chart
  id="bubble-legend"
  legend-position="bottom"
  label="Legend at Bottom"
  description="A bubble chart with the legend at the bottom"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-legend');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Paperbacks',
          data: [
            { x: 320, y: 4.2, r: 16 },
            { x: 280, y: 4.0, r: 14 },
          ],
        },
        {
          label: 'Hardcovers',
          data: [
            { x: 400, y: 4.4, r: 12 },
            { x: 448, y: 4.5, r: 18 },
          ],
        },
      ],
    },
  };
</script>
```

### Grid Lines

Use the `grid` attribute to control which axes show grid lines. Options are `both` (default), `x`, `y`, and `none`.

```html
<wa-bubble-chart id="bubble-grid" grid="none" label="No Grid" description="A bubble chart with grid lines hidden">
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-grid');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Planets',
          data: [
            { x: 0.4, y: 167, r: 9 },
            { x: 0.7, y: 464, r: 21 },
            { x: 1.0, y: 15, r: 22 },
            { x: 1.5, y: -65, r: 12 },
          ],
        },
      ],
    },
  };
</script>
```

### Axis Labels

Use the `x-label` and `y-label` attributes to add descriptive labels to each axis.

```html
<wa-bubble-chart
  id="bubble-axis"
  x-label="Cost Index"
  y-label="Quality Score"
  label="City Analysis"
  description="A bubble chart with labeled axes comparing city cost and quality"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-axis');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Cities',
          data: [
            { x: 40, y: 7.2, r: 16 },
            { x: 55, y: 8.2, r: 18 },
            { x: 50, y: 7.8, r: 12 },
            { x: 60, y: 7.6, r: 10 },
          ],
        },
      ],
    },
  };
</script>
```

### Disabling Features

Use `without-tooltip` to hide hover tooltips and `without-animation` to disable transitions.

```html
<wa-bubble-chart
  id="bubble-disabled"
  without-tooltip
  without-animation
  label="Minimal"
  description="A bubble chart with tooltips and animations disabled"
>
</wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-disabled');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Trails',
          data: [
            { x: 3, y: 6, r: 10 },
            { x: 8, y: 20, r: 18 },
            { x: 5, y: 12, r: 14 },
          ],
        },
      ],
    },
  };
</script>
```
