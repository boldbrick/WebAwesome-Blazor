// Enumerates every demo route so the sweep test always covers the full site without a
// hand-maintained list. Component routes are read straight from the same api-surface.json
// that drives the demo's own navigation and API tables (src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json),
// so newly generated component pages are covered automatically. Layout routes have no such
// generated manifest (WaCluster/WaFlank/... are hand-authored, not CEM-derived) and are kept
// in sync manually with MainLayout.razor's LayoutLinks array.
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
  return ['/', ...getComponentRoutes(), ...LAYOUT_ROUTES];
}

module.exports = { getComponentRoutes, getAllRoutes, LAYOUT_ROUTES };
