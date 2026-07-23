<!-- Source: https://webawesome.com/docs/components/page (fetched 2026-07-23 for Web Awesome 3.2.0).
     wa-page is a Pro component and is not documented in the public GitHub docs repo; content mirrored from the public web docs.
     The web docs track the latest release, so version-specific examples may drift. The CEM API surface
     (temp/wa-api/surface_3.2.0.json) is authoritative where this reference differs. -->

# Page (Pro)

The `<wa-page>` component scaffolds full application layouts with header, navigation, sidebar, main content, aside, and footer regions. It is designed to structure complete pages with minimal markup and built-in responsive behavior.

Available since 3.0.

## Important Setup

When using `<wa-page>`, reset HTML and body styling:

```css
html,
body {
  min-height: 100%;
  padding: 0;
  margin: 0;
}
```

## Slots

| Name                    | Description |
|-------------------------|-------------|
| (default)               | The page's main content. |
| aside                   | Right-side content (table of contents, ads, etc.); sticky by default. |
| banner                  | Banner displayed above header. |
| footer                  | Footer content, always below viewport. |
| header                  | Top-of-page header. |
| main-footer             | Footer inline below main content. |
| main-header             | Header inline above main content. |
| menu                    | Left-side content; overrides default navigation slot. |
| navigation              | Main navigation area (left side, collapses to drawer on mobile). |
| navigation-footer       | Navigation area footer. |
| navigation-header       | Navigation area header. |
| navigation-toggle       | Custom button for toggling navigation drawer. |
| navigation-toggle-icon  | Custom icon for navigation toggle. |
| skip-to-content         | Overridable "skip to content" link. |
| subheader               | Subheader below header (breadcrumbs, etc.). |

## Attributes & Properties

| Name                        | Type | Default | Description |
|-----------------------------|------|---------|-------------|
| disable-navigation-toggle   | boolean | false | Hide the default hamburger button. |
| mobile-breakpoint           | string | `'768px'` | Viewport width to collapse navigation; accepts px or CSS lengths. |
| navigation-placement        | `'start' \| 'end'` | `'start'` | Navigation drawer placement on mobile. |
| nav-open                    | boolean | false | Whether navigation drawer is open (mobile only). |
| view                        | `'mobile' \| 'desktop'` | `'desktop'` | Reflects current viewport relative to mobile-breakpoint. |
| disable-sticky              | string | — | Space-delimited list of sections to exclude from sticky behavior. |

Sticky sections by default: banner, header, subheader, menu, aside.

## Methods

| Name                | Description | Arguments |
|---------------------|-------------|-----------|
| hideNavigation()    | Hides mobile navigation drawer. | — |
| showNavigation()    | Shows mobile navigation drawer. | — |
| toggleNavigation()  | Toggles mobile navigation drawer. | — |

## CSS Custom Properties

`--aside-width`, `--banner-height`, `--header-height`, `--main-width`, `--menu-width`, `--subheader-height`.

## CSS Parts

`page`, `banner`, `header`, `subheader`, `navigation`, `navigation-desktop`, `navigation-header`, `navigation-footer`, `navigation-toggle`, `navigation-toggle-icon`, `menu`, `body`, `main`, `main-header`, `main-content`, `main-footer`, `aside`, `footer`, `drawer`, `dialog-wrapper`, `skip-to-content`.

## Utility Classes

- `.wa-mobile-only` — hides element on desktop.
- `.wa-desktop-only` — hides element on mobile.
