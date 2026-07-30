<!-- Source: reference doc bundled in the Web Awesome 3.11.0 release zip (dist/skills/webawesome/references/components/scatter-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/scatter-chart -->

# Scatter Chart [Pro]

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).

`<wa-scatter-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Scatter charts reveal relationships between two variables by plotting data points on a grid. They are ideal for identifying correlations, clusters, and outliers in datasets.

**[Get Scatter Chart with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=scatter-chart)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Scatter Chart!

```html
<wa-scatter-chart
  label="Height vs. Weight"
  description="A scatter chart showing the relationship between height and weight"
>
  <script type="application/json">
    {
      "data": {
        "datasets": [
          {
            "label": "Measurements",
            "data": [
              { "x": 158, "y": 55 },
              { "x": 163, "y": 62 },
              { "x": 165, "y": 68 },
              { "x": 170, "y": 72 },
              { "x": 173, "y": 75 },
              { "x": 175, "y": 80 },
              { "x": 178, "y": 78 },
              { "x": 180, "y": 85 },
              { "x": 183, "y": 82 },
              { "x": 188, "y": 90 }
            ]
          }
        ]
      }
    }
  </script>
</wa-scatter-chart>
```

Unlike bar or line charts, scatter data is an array of `{x, y}` point objects:

| Property | Description |
| --- | --- |
| \`x\` | Position along the x-axis |
| \`y\` | Position along the y-axis |

See [`<wa-chart>`](https://webawesome.com/docs/components/chart) for advanced configuration, custom plugins, and direct Chart.js access.

## API

### Importing

If you're using the autoloader or a hosted project, components load on demand — no manual import needed. To cherry-pick a component manually, use one of the following snippets.

\*\*CDN\*\*

Import this component directly from the CDN:

```js
import 'https://ka-f.webawesome.com/webawesome@3.11.0/components/scatter-chart/scatter-chart.js';
```

\*\*npm\*\*

After installing Web Awesome via npm, import this component:

```js
import '@awesome.me/webawesome/dist/components/scatter-chart/scatter-chart.js';
```

\*\*Self-Hosted\*\*

If you're self-hosting Web Awesome, import this component from your server:

```js
import './webawesome/dist/components/scatter-chart/scatter-chart.js';
```

\*\*React\*\*

To import this component for React 18 or below, use the following code:

```js
import WaScatterChart from '@awesome.me/webawesome/dist/react/scatter-chart/index.js';
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
| \`type\` type | \`bar\` The type of chart to render. Valid types include , line, pie, doughnut, polarArea, radar, scatter, and bubble. Type ChartType Default 'scatter' | |
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

Place a `<script type="application/json">` tag inside the component. Each data point is an object with `x` and `y` properties. The JSON follows the [Chart.js configuration format](https://www.chartjs.org/docs/latest/configuration/).

```html
<wa-scatter-chart
  label="Test Results"
  description="A scatter chart showing the correlation between study hours and test scores"
>
  <script type="application/json">
    {
      "data": {
        "datasets": [
          {
            "label": "Students",
            "data": [
              { "x": 2, "y": 65 },
              { "x": 3, "y": 72 },
              { "x": 4, "y": 78 },
              { "x": 5, "y": 82 },
              { "x": 6, "y": 88 },
              { "x": 7, "y": 85 },
              { "x": 8, "y": 92 },
              { "x": 9, "y": 95 }
            ]
          }
        ]
      }
    }
  </script>
</wa-scatter-chart>
```

### Providing Data with JavaScript

Set the `config` property from JavaScript when your data comes from code rather than static markup. The chart re-renders automatically each time you assign it. For data that updates at runtime, try the live controls in [Accessing the Chart.js Instance](https://webawesome.com/docs/components/chart#accessing-the-chartjs-instance).

```html
<wa-scatter-chart id="scatter-js-example" label="Test Results" description="A scatter chart of study hours vs. scores">
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-js-example');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Students',
          data: [
            { x: 2, y: 65 },
            { x: 3, y: 72 },
            { x: 4, y: 78 },
            { x: 5, y: 82 },
            { x: 6, y: 88 },
            { x: 7, y: 85 },
            { x: 8, y: 92 },
            { x: 9, y: 95 },
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

Scatter tooltips show raw `x, y` values by default. Attach a property to each data point and read it in a [tooltip callback](https://webawesome.com/docs/components/chart#custom-tooltips) to name points instead.

```html
<wa-scatter-chart
  id="scatter-tooltips"
  x-label="Monthly Cost Index"
  y-label="Quality of Life"
  label="City Comparison"
  description="A scatter chart whose tooltips name each city instead of showing raw coordinates"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-tooltips');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Cities',
          data: [
            { x: 62, y: 7.8, city: 'Lisbon' },
            { x: 78, y: 8.2, city: 'Vienna' },
            { x: 55, y: 7.1, city: 'Kraków' },
            { x: 88, y: 8.5, city: 'Zurich' },
            { x: 48, y: 6.9, city: 'Porto' },
          ],
        },
      ],
    },
    options: {
      plugins: {
        tooltip: {
          callbacks: {
            label: context => `${context.raw.city}: cost ${context.raw.x}, quality ${context.raw.y}`,
          },
        },
      },
    },
  };
</script>
```

### Multiple Datasets

Use multiple datasets to compare groups. Each dataset is plotted in its own color.

```html
<wa-scatter-chart
  id="scatter-multi"
  label="Group Comparison"
  description="A scatter chart comparing test results between two study groups"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-multi');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Tutored',
          data: [
            { x: 3, y: 78 },
            { x: 4, y: 82 },
            { x: 5, y: 86 },
            { x: 6, y: 90 },
            { x: 7, y: 94 },
          ],
        },
        {
          label: 'Self-study',
          data: [
            { x: 3, y: 68 },
            { x: 4, y: 72 },
            { x: 5, y: 77 },
            { x: 7, y: 84 },
            { x: 8, y: 89 },
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
<wa-scatter-chart
  id="scatter-colors"
  label="Custom Colors"
  description="A scatter chart with custom point colors"
  style="
    --fill-color-1: var(--wa-color-cyan-60);
    --border-color-1: var(--wa-color-cyan-60);
  "
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-colors');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Ice cream sales',
          data: [
            { x: 68, y: 120 },
            { x: 74, y: 175 },
            { x: 79, y: 210 },
            { x: 84, y: 280 },
            { x: 89, y: 340 },
            { x: 95, y: 410 },
          ],
        },
      ],
    },
  };
</script>
```

### Point Radius

Use the `--point-radius` CSS custom property to control the size of each plotted dot.

```html
<wa-scatter-chart
  id="scatter-points"
  style="--point-radius: 8px"
  label="Large Points"
  description="A scatter chart with larger data point dots"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-points');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Air temperature',
          data: [
            { x: 0, y: 70 },
            { x: 2, y: 63 },
            { x: 4, y: 56 },
            { x: 6, y: 49 },
            { x: 8, y: 42 },
            { x: 10, y: 35 },
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
<wa-scatter-chart
  id="scatter-legend"
  legend-position="right"
  label="Legend on Right"
  description="A scatter chart with the legend on the right side"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-legend');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Morning',
          data: [
            { x: 6, y: 15 },
            { x: 7, y: 22 },
            { x: 8, y: 30 },
            { x: 9, y: 28 },
          ],
        },
        {
          label: 'Afternoon',
          data: [
            { x: 12, y: 45 },
            { x: 13, y: 52 },
            { x: 14, y: 48 },
            { x: 15, y: 40 },
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
<wa-scatter-chart id="scatter-grid" grid="none" label="No Grid" description="A scatter chart with grid lines hidden">
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-grid');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Dinosaur sightings',
          data: [
            { x: 200, y: 8 },
            { x: 350, y: 14 },
            { x: 500, y: 19 },
            { x: 650, y: 24 },
            { x: 800, y: 29 },
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
<wa-scatter-chart
  id="scatter-axis"
  x-label="Hours Studied"
  y-label="Score"
  label="Study Correlation"
  description="A scatter chart with labeled axes showing study hours vs. score"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-axis');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Students',
          data: [
            { x: 1, y: 55 },
            { x: 3, y: 68 },
            { x: 5, y: 78 },
            { x: 7, y: 88 },
            { x: 9, y: 94 },
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
<wa-scatter-chart
  id="scatter-disabled"
  without-tooltip
  without-animation
  label="Minimal"
  description="A scatter chart with tooltips and animations disabled"
>
</wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-disabled');

  chart.config = {
    data: {
      datasets: [
        {
          label: 'Typing accuracy',
          data: [
            { x: 1, y: 42 },
            { x: 2, y: 31 },
            { x: 3, y: 24 },
            { x: 4, y: 15 },
            { x: 5, y: 9 },
          ],
        },
      ],
    },
  };
</script>
```
