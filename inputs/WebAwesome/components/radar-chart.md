<!-- Source: https://webawesome.com/docs/components/radar-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Radar Chart Component Reference

## Component Summary

The `<wa-radar-chart>` component compares multiple variables at once by plotting data on a radial grid. It excels at comparing profiles across several dimensions, such as skill assessments, product attributes, or performance metrics.

**Available Since:** 3.3

## Primary Usage Example

```html
<wa-radar-chart id="radar-hero" label="Employee Skills" description="A radar chart comparing skill levels across six categories"></wa-radar-chart>
<script type="module">
  const chart = document.querySelector('#radar-hero');
  chart.config = {
    data: {
      labels: ['Communication', 'Technical', 'Leadership', 'Creativity', 'Teamwork', 'Problem Solving'],
      datasets: [
        { label: 'Alice', data: [85, 92, 70, 88, 95, 78], fill: true },
        { label: 'Bob', data: [72, 78, 90, 65, 80, 88], fill: true }
      ]
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
| config | ChartJS['config'] | — | The Chart.js configuration object. Setting this property will automatically re-render the chart. |
| description | string \| null | null | A description of the chart, used for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes to show grid lines on. |
| indexAxis | 'x' \| 'y' | 'x' | The base axis of the dataset. |
| label | string \| null | null | A label for the chart, used for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | The position of the legend relative to the chart. |
| max | number \| null | null | The maximum value for the value axis. |
| min | number \| null | null | The minimum value for the value axis. |
| plugins | array | [] | Additional Chart.js plugins to register for this chart instance. |
| stacked | boolean | false | Stacks datasets on top of each other along the value axis. |
| type | ChartType | 'radar' | The type of chart to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | A label for the x-axis. |
| yLabel | string \| null | null | A label for the y-axis. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
