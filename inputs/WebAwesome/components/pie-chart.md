<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/pie-chart.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/pie-chart -->

# Pie Chart [Pro]

**Full documentation:** https://webawesome.com/docs/components/pie-chart

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
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma)
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Pie Chart!

For a variation with a hollow center, see [Doughnut Chart](https://webawesome.com/docs/components/doughnut-chart).

```html
<wa-pie-chart id="pie-hero" label="Browser Market Share" description="A pie chart showing browser market share with Chrome leading at 65%">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-hero');

  chart.config = {
    data: {
      labels: ['Chrome', 'Safari', 'Firefox', 'Edge', 'Other'],
      datasets: [{
        label: 'Market Share',
        data: [65, 18, 8, 5, 4]
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
<wa-pie-chart id="pie-js-example" label="Budget Allocation" description="A pie chart of budget allocation">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-js-example');

  chart.config = {
    data: {
      labels: ['Engineering', 'Marketing', 'Sales', 'Operations'],
      datasets: [{
        label: 'Budget',
        data: [40, 25, 20, 15]
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
<wa-pie-chart label="Budget Allocation" description="A pie chart showing how a budget is allocated across departments">
  <script type="application/json">
    {
      "data": {
        "labels": ["Engineering", "Marketing", "Sales", "Operations"],
        "datasets": [{
          "label": "Budget",
          "data": [40, 25, 20, 15]
        }]
      }
    }
  </script>
</wa-pie-chart>
```

### Custom Slice Colors

Link to This Section

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
      datasets: [{
        label: 'Traffic',
        data: [55, 35, 10]
      }]
    }
  };
</script>
```

### Legend

Link to This Section

Use the `legend-position` attribute to control where the legend appears. For pie charts, placing the legend on the side can help prevent overlap. Add `without-legend` to hide it entirely.

```html
<wa-pie-chart id="pie-legend" legend-position="right" label="Legend on Right" description="A pie chart with the legend on the right side">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-legend');

  chart.config = {
    data: {
      labels: ['Rent', 'Food', 'Transport', 'Entertainment', 'Savings'],
      datasets: [{
        label: 'Monthly Spending',
        data: [35, 25, 15, 10, 15]
      }]
    }
  };
</script>
```

### Disabling Tooltips

Link to This Section

Use the `without-tooltip` attribute to hide the tooltips that appear when hovering over slices.

```html
<wa-pie-chart id="pie-tooltip" without-tooltip label="No Tooltips" description="A pie chart with tooltips disabled">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-tooltip');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C'],
      datasets: [{
        label: 'Values',
        data: [40, 35, 25]
      }]
    }
  };
</script>
```

### Disabling Animations

Link to This Section

Use the `without-animation` attribute to disable chart transitions.

```html
<wa-pie-chart id="pie-anim" without-animation label="No Animation" description="A pie chart with animation disabled">
</wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-anim');

  chart.config = {
    data: {
      labels: ['A', 'B', 'C'],
      datasets: [{
        label: 'Values',
        data: [40, 35, 25]
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
| `type` |  | `ChartType` | `'pie'` | The type of chart to render. Valid types include `bar`, `line`, `pie`, `doughnut`, `polarArea`, `radar`, `scatter`, and `bubble`. |
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
