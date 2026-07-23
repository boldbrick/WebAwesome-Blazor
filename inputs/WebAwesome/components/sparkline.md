<!-- Source: https://webawesome.com/docs/components/sparkline (public web docs -- component absent from the public GitHub docs tree; carried forward and re-verified 2026-07-23 for Web Awesome 3.5.0, no 3.5.0 API changes). -->

# Sparkline Component Reference

## Overview

The `<wa-sparkline>` component displays inline data trends as compact, visual charts. Sparklines are small, word-sized graphics designed to fit within text or table cells.

**Status:** Pro component. **Category:** Data Viz. **Available Since:** 3.2

## Basic Usage

```html
<wa-sparkline
  label="Weekly sales performance showing growth"
  data="10 25 15 40 30 45 35"
  style="height: 2rem;"
></wa-sparkline>
```

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `appearance` | `'solid'` \| `'gradient'` \| `'line'` | `'solid'` | The visual fill style of the sparkline. |
| `curve` | `'linear'` \| `'natural'` \| `'step'` | `'linear'` | The curve used to connect data points. |
| `data` | string | `''` | Space-separated numeric values to visualize. |
| `label` | string | `''` | An accessible label describing the sparkline. |
| `trend` | `'positive'` \| `'negative'` \| `'neutral'` | `'neutral'` | A trend indicator affecting the default color. |

## CSS Custom Properties

`--fill-color`, `--line-color`, `--line-width`.

## CSS Parts

`sparkline`, `fill`, `line`.
