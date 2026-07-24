<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/doughnut-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/doughnut-chart -->

# Doughnut Chart [Pro]

**Full documentation:** https://webawesome.com/docs/components/doughnut-chart

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-doughnut-chart>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.3](https://webawesome.com/docs/resources/changelog#wa_330)

Doughnut charts show proportional data as slices of a ring with a hollow center. They offer a cleaner look than pie charts and work well in dashboards where the center space can provide additional context.

**[Get Doughnut Chart with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=doughnut-chart)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

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

Get Web Awesome Pro + Doughnut Chart!

Similar to [pie charts](https://webawesome.com/docs/components/pie-chart) but with an empty center.

```html
<wa-doughnut-chart id="doughnut-hero" label="Project Time Allocation" description="A doughnut chart showing how project time is distributed across activities">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-hero');

  chart.config = {
    data: {
      labels: ['Development', 'Design', 'Testing', 'Meetings', 'Planning'],
      datasets: [{
        label: 'Hours',
        data: [35, 20, 15, 18, 12]
      }]
    }
  };
</script>
```

For advanced configuration such as custom plugins and direct Chart.js access, see [`<wa-chart>`](https://webawesome.com/docs/components/chart).

## Examples

Link to This Section

### Providing Data with JavaScript

Link to This Section

For dynamic data, set the `config` property directly. The chart will re-render automatically.

```html
<wa-doughnut-chart id="doughnut-js-example" label="Revenue by Region" description="A doughnut chart of revenue by region">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-js-example');

  chart.config = {
    data: {
      labels: ['North America', 'Europe', 'Asia', 'Other'],
      datasets: [{
        label: 'Revenue',
        data: [42, 28, 22, 8]
      }]
    }
  };
</script>
```

Note that `config` is shallowly reactive. If you mutate the existing object in place, you must reassign it to trigger a re-render!

### Providing Data with JSON

Link to This Section

Place a `<script type="application/json">` tag inside the component with your chart data. Each value in the `data` array corresponds to a label.

```html
<wa-doughnut-chart label="Revenue by Region" description="A doughnut chart showing revenue distribution across regions">
  <script type="application/json">
    {
      "data": {
        "labels": ["North America", "Europe", "Asia", "Other"],
        "datasets": [{
          "label": "Revenue",
          "data": [42, 28, 22, 8]
        }]
      }
    }
  </script>
</wa-doughnut-chart>
```

### Custom Slice Colors

Link to This Section

Override the default color palette using the `--fill-color-*` and `--border-color-*` CSS custom properties to apply custom colors to each slice.

```html
<wa-doughnut-chart
  id="doughnut-colors"
  label="Custom Colors"
  description="A doughnut chart with custom slice colors"
  style="
    --fill-color-1: color-mix(in srgb, var(--wa-color-purple-60) 70%, transparent);
    --border-color-1: var(--wa-color-purple-60);
    --fill-color-2: color-mix(in srgb, var(--wa-color-orange-60) 70%, transparent);
    --border-color-2: var(--wa-color-orange-60);
    --fill-color-3: color-mix(in srgb, var(--wa-color-cyan-60) 70%, transparent);
    --border-color-3: var(--wa-color-cyan-60);
  "
>
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-colors');

  chart.config = {
    data: {
      labels: ['Completed', 'In Progress', 'Pending'],
      datasets: [{
        label: 'Tasks',
        data: [45, 30, 25]
      }]
    }
  };
</script>
```

### Cutout Size

Link to This Section

Control the size of the center hole through the Chart.js `cutout` option. The default is `'50%'`. Use a higher percentage for a thinner ring or a lower one for a thicker ring.

```html
<wa-doughnut-chart id="doughnut-cutout-example" label="Thin Ring" description="A doughnut chart with a larger center cutout">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-cutout-example');

  chart.config = {
    data: {
      labels: ['Used', 'Available'],
      datasets: [{
        label: 'Storage',
        data: [72, 28]
      }]
    },
    options: {
      cutout: '75%'
    }
  };
</script>
```

### Legend

Link to This Section

Use the `legend-position` attribute to control where the legend appears. Add `without-legend` to hide it entirely.

```html
<wa-doughnut-chart id="doughnut-legend" legend-position="bottom" label="Legend at Bottom" description="A doughnut chart with the legend at the bottom">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-legend');

  chart.config = {
    data: {
      labels: ['Email', 'Social', 'Direct', 'Referral'],
      datasets: [{
        label: 'Traffic Source',
        data: [30, 25, 28, 17]
      }]
    }
  };
</script>
```

### Disabling Tooltips

Link to This Section

Use the `without-tooltip` attribute to hide the tooltips that appear when hovering over slices.

```html
<wa-doughnut-chart id="doughnut-tooltip" without-tooltip label="No Tooltips" description="A doughnut chart with tooltips disabled">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-tooltip');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C'],
      datasets: [{
        label: 'Values',
        data: [50, 30, 20]
      }]
    }
  };
</script>
```

### Disabling Animations

Link to This Section

Use the `without-animation` attribute to disable chart transitions.

```html
<wa-doughnut-chart id="doughnut-anim" without-animation label="No Animation" description="A doughnut chart with animation disabled">
</wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-anim');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C'],
      datasets: [{
        label: 'Values',
        data: [50, 30, 20]
      }]
    }
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
| `type` |  | `ChartType` | `'doughnut'` | The type of chart to render. Valid types include `bar`, `line`, `pie`, `doughnut`, `polarArea`, `radar`, `scatter`, and `bubble`. |
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
