<!-- Source: https://webawesome.com/docs/components/chart (fetched 2026-07-23 for Web Awesome 3.3.0; public web docs — component absent from the public GitHub docs tree) -->

# Chart Component Reference

## Component Summary

The `<wa-chart>` component is a flexible wrapper around Chart.js for building themed data visualizations. It supports advanced configuration including mixed chart types, custom plugins, and direct Chart.js instance access. Charts automatically adapt to light and dark modes with no extra code.

**Category:** Data Viz
**Available Since:** 3.3

## Primary Usage Example

```html
<wa-chart label="Monthly Performance" description="A chart comparing monthly revenue and expenses over six months">
  <script type="application/json">
    {
      "type": "bar",
      "data": {
        "labels": ["January", "February", "March", "April", "May", "June"],
        "datasets": [
          { "label": "Revenue", "data": [48, 56, 62, 58, 71, 68] },
          { "label": "Expenses", "data": [35, 38, 40, 42, 45, 43] }
        ]
      }
    }
  </script>
</wa-chart>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | An optional `<script type="application/json">` element containing the Chart.js configuration object. |

## Attributes & Properties

| Name | Type | Default | Reflects | Description |
|------|------|---------|----------|-------------|
| `config` | `ChartJS['config']` | — | — | The Chart.js configuration object. Setting this property will automatically re-render the chart. |
| `description` | `string \| null` | `null` | — | A description of the chart, used for accessibility. |
| `grid` | `'x' \| 'y' \| 'both' \| 'none'` | `'both'` | ✓ | Which axes to show grid lines on. |
| `indexAxis` | `'x' \| 'y'` | `'x'` | ✓ | The base axis of the dataset. 'x' for vertical bars and 'y' for horizontal bars. |
| `label` | `string \| null` | `null` | ✓ | A label for the chart, used for accessibility. |
| `legendPosition` | `LayoutPosition \| 'start' \| 'end'` | `'top'` | ✓ | The position of the legend relative to the chart. |
| `max` | `number \| null` | `null` | ✓ | The maximum value for the value axis. |
| `min` | `number \| null` | `null` | ✓ | The minimum value for the value axis. |
| `plugins` | `array` | `[]` | — | Additional Chart.js plugins to register for this chart instance. |
| `stacked` | `boolean` | `false` | ✓ | Stacks datasets on top of each other along the value axis. |
| `type` | `ChartType` | `'bar'` | ✓ | The type of chart to render. Valid types include bar, line, pie, doughnut, polarArea, radar, scatter, and bubble. |
| `withoutAnimation` | `boolean` | `false` | ✓ | Disables chart animations. |
| `withoutLegend` | `boolean` | `false` | ✓ | Hides the legend. |
| `withoutTooltip` | `boolean` | `false` | ✓ | Hides tooltips over data points. |
| `xLabel` | `string \| null` | `null` | ✓ | A label for the x-axis. |
| `yLabel` | `string \| null` | `null` | ✓ | A label for the y-axis. |

## CSS Custom Properties

| Name | Default | Description |
|------|---------|-------------|
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
| `--border-radius` | `var(--wa-border-radius-s)` | Border radius for bar charts. |
| `--border-width` | `var(--wa-border-width-s)` | Border width for bars and arcs. |
| `--grid-border-width` | `var(--wa-border-width-s)` | Border width for chart grid lines and axis borders. |
| `--line-border-width` | `var(--wa-border-width-m)` | Border width for line and radar charts. |
| `--point-radius` | `var(--wa-border-width-m)` | Radius of data point dots. |

## Methods

The component exposes the underlying Chart.js instance via the `chart` property after `renderChart()` is called, enabling programmatic access to all Chart.js API methods.
