import { defineConfig, devices } from '@playwright/test'

// 測試對象是 compose.yaml 起的整套環境（pnpm stack:up），這裡不負責啟動任何服務。
export default defineConfig({
  testDir: './tests',
  // 每局遊戲有好幾段倒數，單一測試會跑數十秒。
  timeout: 120_000,
  expect: { timeout: 15_000 },
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    // 鎖定語系，避免 i18n 依瀏覽器語言切換畫面文字。
    locale: 'zh-TW',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure'
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }]
})
