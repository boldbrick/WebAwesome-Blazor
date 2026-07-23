<!-- Source: https://webawesome.com/docs/components/sparkline (fetched 2026-07-23 for Web Awesome 3.2.0).
     The public GitHub docs repo does not yet include this component; content mirrored from the public web docs.
     The CEM API surface (temp/wa-api/surface_3.2.0.json) is authoritative where this reference differs. -->

# Sparkline

The `<wa-sparkline>` component displays inline data trends as compact, visual charts. Sparklines are small, word-sized graphics designed to fit within text or table cells, showing data trends at a glance without requiring dedicated chart space.

Available since 3.2 (experimental).

## Attributes & Properties

| Name       | Type                                | Default   | Description |
|------------|-------------------------------------|-----------|-------------|
| appearance | `'gradient' \| 'line' \| 'solid'`   | `'solid'` | The visual fill style of the sparkline. |
| curve      | `'linear' \| 'natural' \| 'step'`   | `'linear'`| The type of curve used to connect data points. |
| data       | string                              | `''`      | Space-separated numeric values to visualize (e.g., "10 20 40 25 35"). |
| label      | string                              | `''`      | An accessible label describing the sparkline for screen readers. |
| trend      | `'positive' \| 'negative' \| 'neutral'` | —     | A trend to indicate, which will affect the sparkline's default color. |

## CSS Custom Properties

| Property        | Description |
|-----------------|-------------|
| `--fill-color`  | The fill color for the area under the line. |
| `--line-color`  | The color of the sparkline stroke. |
| `--line-width`  | The width of the sparkline stroke. |

## CSS Parts

`base` (deprecated; use `sparkline`), `sparkline`, `fill`, `line`.

## Notes

- Always provide a descriptive `label` for accessibility.
- At least two numeric data values are required to render a sparkline.
- Data values are space-separated numbers.
- Default sizing is `height: 1em` and `aspect-ratio: 4/1` for inline integration.
