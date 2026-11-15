// @ts-check
const { defineConfig } = require('@playwright/test');

// The capture of the screenshots of the demo's WA Inspector page (inspector\screenshots.capture.js): the setup of the
// inspector browser tests (playwright.inspector.config.js, inspector\fixtures.js), with a file pattern that the test
// configurations do not match. It runs against Demo.Server with the inspector metadata on:
//
//   $env:WebAwesome__EmitInspectorMetadata = 'true'
//   dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
//   cd tools\e2e; npm run capture:inspector-screenshots
//
// It writes the PNG files to src\WebAwesome.Blazor.Demo\wwwroot\images\tooling.

const baseURL = process.env.DEMO_BASE_URL || 'http://localhost:5100';

module.exports = defineConfig({
  testDir: './inspector',
  testMatch: '*.capture.js',
  workers: 1,
  fullyParallel: false,
  retries: 0,
  reporter: 'list',
  timeout: 120_000,
  use: {
    baseURL,
  },
});
