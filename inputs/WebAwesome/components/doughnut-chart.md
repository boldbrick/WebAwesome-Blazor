<!-- Source: https://webawesome.com/docs/components/doughnut-chart (fetched 2026-07-23 for Web Awesome 3.3.0; public web docs — component absent from the public GitHub docs tree) -->

# Doughnut Chart Component Reference

## Component Summary

The `<wa-doughnut-chart>` component displays proportional data as slices arranged in a ring with a hollow center. They offer a cleaner look than pie charts and work well in dashboards where the center space can provide additional context.

**Available Since:** 3.3

## Basic Usage

```html
<wa-doughnut-chart id="doughnut-hero" label="Project Time Allocation" description="A doughnut chart showing how project time is distributed across activities"></wa-doughnut-chart>
<script type="module">
  const chart = document.querySelector('#doughnut-hero');
  chart.config = {
    data: {
      labels: ['Development', 'Design', 'Testing', 'Meetings', 'Planning'],
      datasets: [{ label: 'Hours', data: [35, 20, 15, 18, 12] }]
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
| type | ChartType | 'doughnut' | The type of chart to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | A label for the x-axis. |
| yLabel | string \| null | null | A label for the y-axis. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
