// @ts-check
const { defineConfig } = require('@playwright/test');

// The browser tests of the WA Inspector extension (tools\wa-inspector\extension), apart from the standard suite
// (playwright.config.js): each test launches its own Chromium with the extension loaded (a persistent context,
// inspector\fixtures.js), so the tests cannot share the standard browser fixture. They run against Demo.Server with
// the inspector metadata on:
//
//   $env:WebAwesome__EmitInspectorMetadata = 'true'
//   dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
//   cd tools\e2e; npm run test:inspector
//
// The standard sweep and the release preflight do not run these tests.

const baseURL = process.env.DEMO_BASE_URL || 'http://localhost:5100';

module.exports = defineConfig({
  testDir: './inspector',
  // one browser with an extension at a time: the tests share the demo server, and each launch is heavy
  workers: 1,
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  timeout: 60_000,
  use: {
    baseURL,
    trace: 'retain-on-failure',
  },
});
