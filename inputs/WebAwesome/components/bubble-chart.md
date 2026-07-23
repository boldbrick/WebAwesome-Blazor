<!-- Source: https://webawesome.com/docs/components/bubble-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Bubble Chart Component Reference

## Component Summary

The `<wa-bubble-chart>` component adds a third dimension to scatter plots by varying the size of each data point. It visualizes relationships where a third variable adds meaning beyond simple x/y correlation.

**Available Since:** 3.3

## Usage Example

```html
<wa-bubble-chart id="bubble-hero" label="City Comparison" description="A bubble chart comparing cities by cost of living, quality of life, and population"></wa-bubble-chart>
<script type="module">
  const chart = document.querySelector('#bubble-hero');
  chart.config = {
    data: {
      datasets: [
        { label: 'North America', data: [{ x: 65, y: 7.8, r: 18 }, { x: 50, y: 7.0, r: 12 }, { x: 55, y: 7.5, r: 14 }] },
        { label: 'Europe', data: [{ x: 40, y: 8.2, r: 16 }, { x: 30, y: 7.6, r: 10 }, { x: 45, y: 8.0, r: 13 }] },
      ],
    },
  };
</script>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | An optional `<script type="application/json">` element containing the Chart.js configuration object. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| config | ChartJS['config'] | — | The Chart.js configuration object; setting this property automatically re-renders the chart. |
| description | string \| null | null | A description of the chart, used for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes to show grid lines on. |
| indexAxis | 'x' \| 'y' | 'x' | The base axis of the dataset. |
| label | string \| null | null | A label for the chart, used for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | The position of the legend relative to the chart. |
| max | number \| null | null | The maximum value for the value axis. |
| min | number \| null | null | The minimum value for the value axis. |
| plugins | array | [] | Additional Chart.js plugins to register for this chart instance. |
| stacked | boolean | false | Stacks datasets on top of each other along the value axis. |
| type | ChartType | 'bubble' | The type of chart to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | A label for the x-axis. |
| yLabel | string \| null | null | A label for the y-axis. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
