<!-- Source: https://webawesome.com/docs/components/pie-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Pie Chart Component Reference

## Summary

The `<wa-pie-chart>` component displays the proportional composition of a whole as slices of a circle. It works best with a small number of categories where the relative proportions matter more than exact values.

**Available Since:** 3.3

## Primary Usage Example

```html
<wa-pie-chart id="pie-hero" label="Browser Market Share" description="A pie chart showing browser market share with Chrome leading at 65%"></wa-pie-chart>
<script type="module">
  const chart = document.querySelector('#pie-hero');
  chart.config = {
    data: {
      labels: ['Chrome', 'Safari', 'Firefox', 'Edge', 'Other'],
      datasets: [{ label: 'Market Share', data: [65, 18, 8, 5, 4] }]
    }
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
| config | ChartJS['config'] | — | Chart.js configuration object; setting this property automatically re-renders the chart. |
| description | string \| null | null | A description of the chart, used for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes to show grid lines on. |
| indexAxis | 'x' \| 'y' | 'x' | The base axis of the dataset. |
| label | string \| null | null | A label for the chart, used for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | The position of the legend relative to the chart. |
| max | number \| null | null | The maximum value for the value axis. |
| min | number \| null | null | The minimum value for the value axis. |
| plugins | array | [] | Additional Chart.js plugins to register. |
| stacked | boolean | false | Stacks datasets on top of each other along the value axis. |
| type | ChartType | 'pie' | The type of chart to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | A label for the x-axis. |
| yLabel | string \| null | null | A label for the y-axis. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
