// @ts-check
const { test } = require('./helpers/test');
const { runMethodCase, runKnownDefectCase } = require('./helpers/method-harness');
const group = require('./helpers/method-cases/media-content');

// The wrapper method suite, group "media-content": WaAnimatedImage, WaAnimation, WaZoomableFrame, WaVideo (Pro),
// WaVideoPlaylist (Pro), WaMarkdown, WaMutationObserver, WaResizeObserver, WaRandomContent, WaRelativeTime and
// WaCopyButton on /testing/methods-media-content. Each case calls wrapper methods from .NET through the wrapper's @ref
// and asserts their effect; the known defects run as expected failures. The cases are in
// helpers\method-cases\media-content.js, where method-coverage.spec.js reads which methods they prove. Every group spec
// has this same shape.

test.describe('methods media-content', () => {
  for (const c of group.cases) {
    test(`method: ${c.name}`, async ({ page, context }) => runMethodCase(group, c, page, context));
  }
  for (const c of group.knownDefects ?? []) {
    test(`method known defect: ${c.name}`, async ({ page, context }) => runKnownDefectCase(group, c, page, context));
  }
});
