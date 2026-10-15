// @ts-check
const { defineConfig, devices } = require('@playwright/test');

const baseURL = process.env.DEMO_BASE_URL || 'http://localhost:5000';

module.exports = defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  // per test: a page boots the WASM runtime and loads the Web Awesome modules from the CDN before a test can act,
  // and under the default worker count (half the logical CPUs) that alone can take most of 30 s; every assertion
  // keeps its own expect() timeout, so a real failure still fails fast
  timeout: 60_000,
  use: {
    baseURL,
    trace: 'retain-on-failure',
    // a fake audio output: the media clock of a playing element runs on the audio sink, and a real sink can stall
    // (the "Remote Audio" device of a Windows RDP session keeps currentTime at 0), so media tests must not depend on
    // the machine's audio devices
    launchOptions: { args: ['--disable-audio-output'] },
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
