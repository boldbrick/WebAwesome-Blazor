// Enumerates every demo route so the sweep test always covers the full site without a
// hand-maintained list. Component routes are read straight from the same api-surface.json
// that drives the demo's own navigation and API tables (src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json),
// so newly generated component pages are covered automatically. Layout and tooling routes have no such
// generated manifest (WaCluster/WaFlank/... are hand-authored, not CEM-derived) and are kept
// in sync manually with MainLayout.razor's LayoutLinks array. Harness routes (e2e-only pages
// outside the navigation) are hand-listed in HARNESS_ROUTES, plus the method suite's group pages
// (tools\e2e\data\method-groups.json), each once its case module exists.
const fs = require('fs');
const path = require('path');

const LAYOUT_ROUTES = [
  '/layout/cluster',
  '/layout/flank',
  '/layout/frame',
  '/layout/grid',
  '/layout/split',
  '/layout/stack',
  '/layout/text',
  '/layout/visually-hidden',
];

// Hand-designed showcase pages; kept in sync manually with MainLayout.razor's ShowcaseLinks array.
const SHOWCASE_ROUTES = [
  '/showcases/registration-form',
  '/showcases/dashboard',
  '/showcases/settings',
  '/showcases/overlays',
  '/showcases/media-gallery',
  '/showcases/content',
];

// Tooling pages; kept in sync manually with MainLayout.razor's ToolingLinks array.
const TOOLING_ROUTES = [
  '/tooling/inspector',
];

// Pages of wrappers rendering another wrapper's element in one of its modes (the range wrappers of wa-date-input
// and wa-date-picker), which api-surface.json, keyed by element tag, cannot list; kept in sync manually with
// ComponentCategoryMap.WrapperPages (src\WebAwesome.Blazor.Demo\Services\ComponentCategoryMap.cs).
const WRAPPER_ROUTES = [
  '/components/date-range-input',
  '/components/date-range-picker',
];

// e2e harness pages (src\WebAwesome.Blazor.Demo\Pages\Testing): deliberately outside /components/,
// so they are neither derived from api-surface.json nor linked from the sidebar - listed here so
// the sweep still visits them
const HARNESS_ROUTES = [
  '/testing/value-sync',
  '/testing/events-forms',
  '/testing/events-overlays',
  '/testing/events-content',
  '/testing/events-pro',
  '/testing/event-payloads',
  '/testing/date-typing',
  '/testing/day-content',
  '/testing/sticky-attributes',
  '/testing/toast-items',
  '/testing/carousel-slides',
  '/testing/js-functions',
  '/testing/cell-presets',
  '/testing/element-functions',
  '/testing/icon-library',
  '/testing/icon-path',
  '/testing/grid-functions',
  '/testing/cell-actions',
  '/testing/lit-cells',
  '/testing/cell-requests',
  '/testing/inspector',
  '/testing/grid-reload',
  ...methodHarnessRoutes(),
];

// the harness pages of the wrapper method suite: the route of every group in tools\e2e\data\method-groups.json that
// has a case module (tests\helpers\method-cases\<group>.js), so a group's page joins the sweep with its cases
function methodHarnessRoutes() {
  const groupsPath = path.resolve(__dirname, '..', '..', 'data', 'method-groups.json');
  const { groups } = JSON.parse(fs.readFileSync(groupsPath, 'utf8').replace(/^﻿/, ''));
  return groups
    .filter((/** @type {{ name: string }} */ g) => fs.existsSync(path.join(__dirname, 'method-cases', `${g.name}.js`)))
    .map((/** @type {{ route: string }} */ g) => g.route);
}

function getComponentRoutes() {
  const surfacePath = path.resolve(
    __dirname,
    '..', '..', '..', '..',
    'src', 'WebAwesome.Blazor.Demo', 'wwwroot', 'data', 'api-surface.json'
  );
  const raw = fs.readFileSync(surfacePath, 'utf8').replace(/^﻿/, '');
  const surface = JSON.parse(raw);
  return Object.keys(surface.components).map(tag => '/components/' + tag.replace(/^wa-/, ''));
}

function getAllRoutes() {
  return ['/', ...getComponentRoutes(), ...WRAPPER_ROUTES, ...LAYOUT_ROUTES, ...SHOWCASE_ROUTES, ...TOOLING_ROUTES, ...HARNESS_ROUTES];
}

module.exports = { getComponentRoutes, getAllRoutes, WRAPPER_ROUTES, LAYOUT_ROUTES, SHOWCASE_ROUTES, TOOLING_ROUTES, HARNESS_ROUTES };
