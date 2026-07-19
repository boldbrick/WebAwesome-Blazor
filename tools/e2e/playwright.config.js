// @ts-check
const { defineConfig, devices } = require('@playwright/test');

const baseURL = process.env.DEMO_BASE_URL || 'http://localhost:5000';

module.exports = defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  timeout: 30_000,
  use: {
    baseURL,
    trace: 'retain-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  // The demo app (WebAwesome.Blazor.Demo, a Blazor WASM standalone app) must already be running.
  // Start it yourself before running tests: dotnet run --project src\WebAwesome.Blazor.Demo
  // Uncomment the webServer block below to have Playwright manage it automatically instead.
  // webServer: {
  //   command: 'dotnet run --project ..\\..\\src\\WebAwesome.Blazor.Demo\\WebAwesome.Blazor.Demo.csproj --configuration Debug --no-build',
  //   url: baseURL,
  //   reuseExistingServer: !process.env.CI,
  //   timeout: 60_000,
  // },
});
