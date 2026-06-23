module.exports = {
  testDir: '.',
  timeout: 60000,
  fullyParallel: false,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: '../reports/playwright-html' }]],
  use: {
    baseURL: 'http://localhost:5253',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  webServer: {
    command: 'dotnet run --project ../../backend/backend.csproj --launch-profile http',
    url: 'http://localhost:5253/login',
    timeout: 240000,
    reuseExistingServer: true,
  },
}
