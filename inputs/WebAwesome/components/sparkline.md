<!-- Source: reference doc bundled in the Web Awesome 3.9.0 release zip (dist/skills/webawesome/references/components/sparkline.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/sparkline -->

# Sparkline [Pro]

**Full documentation:** https://webawesome.com/docs/components/sparkline

> This component requires [Web Awesome Pro](https://webawesome.com/purchase).
`<wa-sparkline>`

ProIncluded with Web Awesome Pro Stable [Data Viz](https://webawesome.com/docs/components/?category=data-viz) [Since 3.2](https://webawesome.com/docs/resources/changelog#wa_320)

Sparklines display inline data trends as compact, visual charts.

**[Get Sparkline with Web Awesome Pro!](https://webawesome.com/purchase?from=pro-docs&component=sparkline)** Subscribing to Web Awesome Pro gives you every Pro component, plus premium themes, color tools, team collaboration, and more.

-   Pro [Components](https://webawesome.com/docs/components)
-   Responsive [Layout Tools](https://webawesome.com/docs/utilities)
-   Ever-Growing [Pattern Library](https://webawesome.com/docs/patterns)
-   Unlimited Hosted Projects
-   Pre-Built [Pro Themes](https://webawesome.com/docs/themes)
-   Pro Theme Builder
-   Pro Color Tools
-   Official [Figma Design Kit](https://webawesome.com/docs/resources/figma)
-   [WA Pro Perpetual License](https://webawesome.com/license/pro)
-   Actual Human™ Support

Get Web Awesome Pro + Sparkline!

```html
<wa-sparkline
  label="Weekly sales performance showing growth"
  data="10 25 15 40 30 45 35"
  style="height: 2rem;"
></wa-sparkline>
```

Sparklines are small, word-sized graphics designed to fit within text or table cells. They show data trends at a glance without requiring dedicated chart space.

Always include a descriptive `label` attribute for accessibility. The label won't be visible but will be announced by screen readers.

## Examples

Link to This Section

### Basic Usage

Link to This Section

Provide numeric data as space-separated values. At least two values are required to generate the sparkline.

```html
<p>
  Server response times
  <wa-sparkline
    label="Server response times showing stable performance"
    data="45 52 48 55 50 47 51"
  ></wa-sparkline>
  remain stable.
</p>
```

### Appearance

Link to This Section

Use the `appearance` attribute to control how the sparkline fills. The default is `solid` which shows a filled area under the line. You can also choose `gradient` for a faded fill or `line` for stroke only.

```html
<div class="wa-cluster wa-gap-l">
    <wa-sparkline
      appearance="solid"
      data="10 20 15 25 20 30"
      label="Solid appearance example"
      style="height: 2rem;"
    ></wa-sparkline>

    <wa-sparkline
      appearance="gradient"
      data="10 20 15 25 20 30"
      label="Gradient appearance example"
      style="height: 2rem;"
    ></wa-sparkline>

    <wa-sparkline
      appearance="line"
      data="10 20 15 25 20 30"
      label="Line appearance example"
      style="height: 2rem;"
    ></wa-sparkline>
</div>
```

### Trend Colors

Link to This Section

Apply semantic coloring with the `trend` attribute to visually indicate the nature of the data.

```html
<div class="wa-cluster wa-gap-l">
  <div>
    <wa-sparkline
      trend="positive"
      appearance="gradient"
      data="20 25 22 30 35 40 50"
      label="Revenue showing positive growth"
      style="height: 2rem;"
    ></wa-sparkline>
    <div class="wa-caption-s" style="text-align: center; margin-block-start: 1rem;">
      Revenue +25%
    </div>
  </div>
  <div>
    <wa-sparkline
      trend="negative"
      appearance="gradient"
      data="50 45 48 40 35 30 25"
      label="Churn rate showing negative trend"
      style="height: 2rem;"
    ></wa-sparkline>
    <div class="wa-caption-s" style="text-align: center; margin-block-start: 1rem;">
      Churn -15%
    </div>
  </div>
  <div>
    <wa-sparkline
      trend="neutral"
      appearance="gradient"
      data="30 32 28 31 29 30 31"
      label="Active users showing stable trend"
      style="height: 2rem;"
    ></wa-sparkline>
    <div class="wa-caption-s" style="text-align: center; margin-block-start: 1rem;">
      Users stable
    </div>
  </div>
</div>
```

### Curve Types

Link to This Section

Control how data points connect with the `curve` attribute. Use `linear` (default), `natural`, or `step`.

```html
<div class="wa-cluster wa-gap-l">
  <wa-sparkline
    curve="linear"
    appearance="gradient"
    data="10 40 20 50 30 60"
    label="Linear interpolation example"
    style="height: 2rem;"
  ></wa-sparkline>

  <wa-sparkline
    curve="natural"
    appearance="gradient"
    data="10 40 20 50 30 60"
    label="Natural curve interpolation example"
    style="height: 2rem;"
  ></wa-sparkline>

  <wa-sparkline
    curve="step"
    appearance="gradient"
    data="10 40 20 50 30 60"
    label="Step interpolation example"
    style="height: 2rem;"
  ></wa-sparkline>
</div>
```

### Sizing

Link to This Section

Sparklines default to `height: 1em` and `aspect-ratio: 4/1` so they fit naturally within text. Override these with CSS for larger displays.

```html
<wa-sparkline
  appearance="gradient"
  curve="natural"
  data="10 15 12 18 22 20 28 25 32 30 35"
  label="Full-width sparkline showing monthly metrics"
  style="width: 100%; height: 6rem;"
></wa-sparkline>
```

### Custom Colors

Link to This Section

Override the default colors using CSS custom properties.

```html
<wa-sparkline
  appearance="gradient"
  data="10 20 15 25 20 30"
  label="Custom purple sparkline"
  style="
    height: 3rem;
    --line-color: var(--wa-color-purple-50);
    --fill-color: var(--wa-color-purple-50);
    --line-width: 3;
  "
></wa-sparkline>
```

### In Tables

Link to This Section

Sparklines work well in data tables to visualize trends alongside other metrics.

```html
<table class="wa-native">
  <thead>
    <tr>
      <th>Metric</th>
      <th>Trend</th>
      <th>Change</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Page Views</td>
      <td>
        <wa-sparkline
          trend="positive"
          data="1200 1350 1280 1420 1580"
          label="Page views trend"
        ></wa-sparkline>
      </td>
      <td><wa-badge variant="success">+31%</wa-badge></td>
    </tr>
    <tr>
      <td>Bounce Rate</td>
      <td>
        <wa-sparkline
          trend="negative"
          data="45 42 48 38 35"
          label="Bounce rate trend"
        ></wa-sparkline>
      </td>
      <td><wa-badge variant="danger">+8%</wa-badge></td>
    </tr>
    <tr>
      <td>Avg. Session</td>
      <td>
        <wa-sparkline
          trend="neutral"
          data="180 175 182 178 180"
          label="Average session duration"
        ></wa-sparkline>
      </td>
      <td><wa-badge variant="neutral">0%</wa-badge></td>
    </tr>
  </tbody>
</table>
```

## Attributes & Properties

| Attribute | Property | Type | Default | Description |
| --- | --- | --- | --- | --- |
| `label` |  | `string` | `''` | An accessible label describing the sparkline for screen readers. |
| `data` |  | `string` | `''` | Space-separated numeric values to visualize (e.g., "10 20 40 25 35"). |
| `appearance` |  | `'gradient' \| 'line' \| 'solid'` | `'solid'` | The visual fill style of the sparkline. |
| `trend` |  | `'positive' \| 'negative' \| 'neutral'` |  | A trend to indicate, which will affect the sparkline's default color. |
| `curve` |  | `'linear' \| 'natural' \| 'step'` | `'linear'` | The type of curve used to connect data points. |
| `dir` |  | `string` |  |  |
| `lang` |  | `string` |  |  |
| `did-ssr` | `didSSR` |  |  |  |

## CSS Parts

| Part | Description |
| --- | --- |
| `base` | The SVG container element. |
| `line` | The sparkline stroke path. |
| `fill` | The filled area under the line (visible with gradient or solid appearance). |

## CSS Custom Properties

| Property | Default | Description |
| --- | --- | --- |
| `--fill-color` |  | The fill color for the area under the line. |
| `--line-color` |  | The color of the sparkline stroke. |
| `--line-width` |  | The width of the sparkline stroke. |
