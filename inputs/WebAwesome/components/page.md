<!-- Source: https://webawesome.com/docs/components/page (fetched 2026-07-23 for Web Awesome 3.3.0; Pro component — absent from the public GitHub docs tree). -->

# Page Component Reference

## Overview

The `<wa-page>` component scaffolds complete application layouts with header, navigation, sidebar, main content, aside, and footer regions. It provides full pages with minimal markup and responsive behavior built in.

**Status:** Pro component.

## Slots

| Name | Description |
|------|-------------|
| (default) | The page's main content. |
| aside | Right-side content; sticks to top while scrolling. |
| banner | Banner displayed above the header; hidden if empty. |
| footer | Footer content below viewport, making page scrollable. |
| header | Top-of-page header; appears below banner if present. |
| main-footer | Footer displayed inline below main content. |
| main-header | Header displayed inline above main content. |
| menu | Left-side custom navigation; overrides navigation slot; sticks while scrolling. |
| navigation | Main navigation area; displayed left side on desktop; sticks while scrolling. |
| navigation-footer | Navigation area footer; becomes `<wa-drawer>` footer on mobile. |
| navigation-header | Navigation area header; becomes `<wa-drawer>` header on mobile. |
| navigation-toggle | Custom button for toggling navigation drawer. |
| navigation-toggle-icon | Custom icon for navigation toggle. |
| skip-to-content | Overridable "skip to content" link for keyboard navigation. |
| subheader | Subheader below main header (breadcrumbs, etc.); hidden if empty. |

## Attributes & Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| disable-navigation-toggle | boolean | false | Hides default hamburger button; auto-enabled if a `data-toggle-nav` element exists in light DOM. |
| mobile-breakpoint | string | '768px' | Viewport width threshold for collapsing to hamburger; accepts numbers (px) or CSS lengths. |
| navigation-placement | 'start' \| 'end' | 'start' | Mobile navigation drawer placement. |
| nav-open | boolean | false | Mobile navigation drawer open state (mobile only). |
| view | 'mobile' \| 'desktop' | 'desktop' | Reflects current viewport state relative to mobile-breakpoint. |

## Methods

| Name | Description | Arguments |
|------|-------------|-----------|
| hideNavigation() | Hides the mobile navigation drawer. | None |
| showNavigation() | Shows the mobile navigation drawer. | None |
| toggleNavigation() | Toggles the mobile navigation drawer. | None |
| visiblePixelsInViewport() | Internal scroll-gap layout helper. | element: HTMLElement \| null |

## CSS Custom Properties

`--aside-width` (auto), `--banner-height` (0px), `--header-height` (0px), `--main-width` (1fr), `--menu-width` (auto), `--subheader-height` (0px).
