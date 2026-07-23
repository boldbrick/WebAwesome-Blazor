<!-- Source: https://webawesome.com/docs/components/bar-chart (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Bar Chart Component Reference

## Component Summary

The `<wa-bar-chart>` component compares quantities across categories using rectangular bars. These visualizations work well for showing rankings, highlighting differences between groups, and tracking changes across time periods.

**Available Since:** 3.3

## Basic Usage

```html
<wa-bar-chart id="bar-hero" label="Quarterly Revenue" description="A bar chart comparing online and in-store revenue across four quarters"></wa-bar-chart>
<script type="module">
  const chart = document.querySelector('#bar-hero');
  chart.config = {
    data: {
      labels: ['Q1', 'Q2', 'Q3', 'Q4'],
      datasets: [
        { label: 'Online', data: [42, 58, 63, 71] },
        { label: 'In-Store', data: [65, 53, 48, 52] },
      ],
    },
  };
</script>
```

## Slots

| Name | Description |
|------|-------------|
| (default) | Optional `<script type="application/json">` element containing Chart.js configuration. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| config | ChartJS['config'] | — | Chart.js configuration object; setting triggers automatic re-render. |
| description | string \| null | null | Chart description for accessibility. |
| grid | 'x' \| 'y' \| 'both' \| 'none' | 'both' | Which axes display grid lines. |
| indexAxis | 'x' \| 'y' | 'x' | Base axis ('x' for vertical, 'y' for horizontal bars). |
| label | string \| null | null | Chart label for accessibility. |
| legendPosition | LayoutPosition \| 'start' \| 'end' | 'top' | Legend position relative to chart. |
| max | number \| null | null | Maximum value for value axis. |
| min | number \| null | null | Minimum value for value axis. |
| orientation | 'vertical' \| 'horizontal' | 'vertical' | Bar orientation. |
| plugins | array | [] | Additional Chart.js plugins to register. |
| stacked | boolean | false | Stack datasets on top of each other. |
| type | ChartType | 'bar' | Chart type to render. |
| withoutAnimation | boolean | false | Disables chart animations. |
| withoutLegend | boolean | false | Hides the legend. |
| withoutTooltip | boolean | false | Hides tooltips over data points. |
| xLabel | string \| null | null | X-axis label. |
| yLabel | string \| null | null | Y-axis label. |

## CSS Custom Properties

The same `--fill-color-1..6`, `--border-color-1..6`, `--border-radius`, `--border-width`, `--grid-border-width`, `--grid-color`, `--line-border-width`, `--point-radius` custom properties as `<wa-chart>`.

## Notes

- The `config` property is shallowly reactive; mutations require reassignment to trigger re-render.
- For advanced configuration (mixed types, custom plugins, direct Chart.js access), use `<wa-chart>`.
- Data can be provided via JavaScript property assignment or inline JSON script tag.
