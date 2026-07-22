<!--
Source: https://webawesome.com/docs/components/page/
Pro component — not documented in the public GitHub repo; ingested from the public web docs.
Note: the "Importing" version tokens and the visiblePixelsInViewport() method reflect the live
docs at fetch time; the Web Awesome 3.0.0 Custom Elements Manifest exposes hideNavigation,
showNavigation, and toggleNavigation. The wrapper follows the 3.0.0 CEM.
-->

# wa-page

Pages scaffold an entire application layout with header, navigation, sidebar, main content, aside, and footer regions. The `<wa-page>` component structures full pages with minimal markup and includes built-in responsive behavior for desktop and mobile navigation without requiring extensive custom code.

## Importing

**CDN:**
```javascript
import 'https://early.webawesome.com/webawesome@3.0.0/dist/components/page/page.js';
```

**npm:**
```javascript
import '@awesome.me/webawesome/dist/components/page/page.js';
```

**Self-Hosted:**
```javascript
import './webawesome/dist/components/page/page.js';
```

## Slots

| Name | Description |
|------|-------------|
| (default) | The page's main content |
| aside | Content shown on the right side of the page; typically contains a table of contents, ads, etc. Sticks to the top as the page scrolls |
| banner | Banner displayed above the header; hidden if no content is provided |
| footer | Footer content displayed underneath the viewport |
| header | Top-of-page header; hidden if no content is provided |
| main-footer | Footer displayed inline below the main content |
| main-header | Header displayed inline above the main content |
| menu | Left-side page content; overrides the default navigation slot when used |
| navigation | Main navigation area content displayed on the left side |
| navigation-footer | Footer for the navigation area; becomes the drawer footer on mobile |
| navigation-header | Header for the navigation area; becomes the drawer header on mobile |
| navigation-toggle | Custom button for toggling the navigation drawer |
| navigation-toggle-icon | Custom icon for the navigation drawer toggle |
| skip-to-content | Overridable "skip to content" link text |
| subheader | Subheader below the main header; suitable for breadcrumbs |

## Attributes & Properties

| Name | Description | Type | Default |
|------|-------------|------|---------|
| disable-navigation-toggle | Determines whether to hide the default hamburger button. Automatically flips to `true` if you add an element with `data-toggle-nav` anywhere in the light DOM. Generally set for you unless you use SSR, in which case set it manually for initial page loads | boolean | false |
| mobile-breakpoint | At what page width to hide the navigation slot and collapse into a hamburger button. Accepts numbers (interpreted as px) and CSS lengths (e.g. `50em`), resolved based on the root element | string | '768px' |
| navigation-placement | Where to place the navigation when in the mobile viewport | 'start' \| 'end' | 'start' |
| nav-open | Whether the navigation drawer is open. Note: the navigation drawer is only "open" on mobile views | boolean | false |
| view | A reflection of the `mobile-breakpoint`; when the page is larger than the breakpoint (768px by default) it is a "desktop" view, otherwise "mobile". Distinguishes when to show/hide the navigation. Defaults to "desktop" because the mobile navigation drawer isn't accessible via SSR | 'mobile' \| 'desktop' | 'desktop' |

## Methods

| Name | Description | Arguments |
|------|-------------|-----------|
| hideNavigation() | Hides the mobile navigation drawer | — |
| showNavigation() | Shows the mobile navigation drawer | — |
| toggleNavigation() | Toggles the mobile navigation drawer | — |

## CSS Custom Properties

| Name | Description | Default |
|------|-------------|---------|
| --aside-width | Width of the "aside" section | auto |
| --banner-height | Height of the banner; can be preset to prevent layout shifts | 0px |
| --header-height | Height of the header; can be preset to prevent layout shifts | 0px |
| --main-width | Width of the "main" section | 1fr |
| --menu-width | Width of the "menu" section | auto |
| --subheader-height | Height of the subheader; can be preset to prevent layout shifts | 0px |

## CSS Parts

| Name | Description |
|------|-------------|
| aside | Right-side page content area |
| banner | Banner above the header |
| base | Component base wrapper |
| body | Wrapper around menu, main, and aside |
| dialog-wrapper | Wrapper for dialogs and modal elements |
| footer | Page footer below the initial viewport |
| header | Top-level header for navigation/branding |
| main-content | Main content area |
| main-footer | Footer below the main content |
| main-header | Header above the main content |
| menu | Left-side navigation area |
| navigation | `<nav>` wrapping the navigation slots on desktop |
| navigation-footer | Footer for the navigation area |
| navigation-header | Header for the navigation area |
| navigation-toggle | Default button toggling the mobile drawer |
| navigation-toggle-icon | Icon inside the navigation-toggle button |
| skip-link | "Skip to main content" link |
| skip-links | Wrapper around the skip-link |
| subheader | Area below the header for breadcrumbs |
