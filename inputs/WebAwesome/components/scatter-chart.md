<!-- Source: https://webawesome.com/docs/components/scatter-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Scatter Chart Component Reference

## Overview

The `<wa-scatter-chart>` component reveals relationships between two variables by plotting data points on a grid. It's ideal for identifying correlations, clusters, and outliers in datasets.

**Available Since:** 3.3

## Primary Usage Example

```html
<wa-scatter-chart id="scatter-hero" label="Height vs. Weight" description="A scatter chart showing the relationship between height and weight"></wa-scatter-chart>
<script type="module">
  const chart = document.querySelector('#scatter-hero');
  chart.config = {
    data: {
      datasets: [{
        label: 'Measurements',
        data: [{ x: 158, y: 55 }, { x: 163, y: 62 }, { x: 170, y: 72 }, { x: 178, y: 78 }, { x: 188, y: 90 }]
      }]
    }
  };
</script>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | Optional `<script type="application/json">` element containing the Chart.js configuration object. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| config | ChartJS['config'] | — | Chart.js configuration object; setting this property auto-renders the chart. |
| description | string \| null | null | Chart description for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes display grid lines. |
| indexAxis | 'x' \| 'y' | 'x' | Base axis of dataset. |
| label | string \| null | null | Chart label for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | Legend position relative to chart. |
| max | number \| null | null | Maximum value for value axis. |
| min | number \| null | null | Minimum value for value axis. |
| plugins | array | [] | Additional Chart.js plugins to register. |
| stacked | boolean | false | Stack datasets on top of each other. |
| type | ChartType | 'scatter' | Chart type to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | Label for x-axis. |
| yLabel | string \| null | null | Label for y-axis. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
