<!-- Source: https://webawesome.com/docs/components/polar-area-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Polar Area Chart Component Reference

## Component Summary

The `<wa-polar-area-chart>` component compares values using segments that radiate from a center point with varying radius. Unlike pie charts, each segment maintains an equal angle while the radius varies, making it useful for comparing magnitudes without visual bias.

**Available Since:** 3.3

## Primary Usage Example

```html
<wa-polar-area-chart id="polar-hero" label="Monthly Rainfall" description="A polar area chart showing monthly rainfall in millimeters"></wa-polar-area-chart>
<script type="module">
  const chart = document.querySelector('#polar-hero');
  chart.config = {
    data: {
      labels: ['January', 'February', 'March', 'April', 'May', 'June'],
      datasets: [{ label: 'Rainfall (mm)', data: [78, 62, 85, 110, 95, 45] }]
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
| config | ChartJS['config'] | — | The Chart.js configuration object. Setting this property triggers automatic re-render. |
| description | string \| null | null | Chart description for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes display grid lines. |
| indexAxis | 'x' \| 'y' | 'x' | Base axis of the dataset. |
| label | string \| null | null | Chart label for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | Legend position relative to chart. |
| max | number \| null | null | Maximum value for the value axis. |
| min | number \| null | null | Minimum value for the value axis. |
| plugins | array | [] | Additional Chart.js plugins to register. |
| stacked | boolean | false | Stacks datasets along the value axis. |
| type | ChartType | 'polarArea' | Chart type to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | X-axis label. |
| yLabel | string \| null | null | Y-axis label. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.
