import { randomUUID } from 'node:crypto'
import { expect, test, type Locator, type Page } from '@playwright/test'

// web-a 頁面的 SignalR 固定連 backend-a，web-b 固定連 backend-b（見 compose.yaml），
// Host 用 web-a、玩家用 web-b，房間內的每一則推播都必須透過 Redis backplane 跨節點送達。
const HOST_WEB = 'http://localhost:3000'
const PLAYER_WEB = 'http://localhost:3001'
const PASSWORD = 'Password123!'

// 頁面是 SSR 出來的：hydration 完成前按鈕看得到但點了沒反應，所以重做到預期的結果出現為止。
const retryUntilHydrated = (action: () => Promise<void>, done: () => Promise<void>) =>
  expect(async () => {
    await action()
    await done()
  }).toPass({ timeout: 20_000 })

const clickUntilVisible = (target: Locator, expected: Locator) =>
  retryUntilHydrated(() => target.click(), () => expect(expected).toBeVisible({ timeout: 1_000 }))

const registerAndLogin = async (page: Page, redirect: string) => {
  const email = `host-${randomUUID()}@example.com`
  const register = await page.request.post('/api/v1/auth/register', {
    data: { email, name: 'E2E Host', password: PASSWORD, confirmPassword: PASSWORD }
  })
  expect(register.ok(), await register.text()).toBeTruthy()

  await page.goto(`/login?redirect=${encodeURIComponent(redirect)}`)
  await retryUntilHydrated(async () => {
    await page.locator('input[type="email"]').fill(email)
    await page.locator('input[type="password"]').fill(PASSWORD)
    await page.locator('button[type="submit"]').click()
  }, () => page.waitForURL(url => url.pathname === redirect, { timeout: 2_000 }))
}

// 題庫走 BFF 的 REST（lb 會輪流分到兩台），和 UI 無關的前置資料不經過畫面建立。
const createPublishedQuiz = async (page: Page) => {
  const created = await page.request.post('/api/v1/quizzes', {
    data: {
      title: 'E2E Multi-node Quiz',
      description: 'played across two backend nodes',
      coverEmoji: '🎯',
      coverGradient: 'primary',
      difficulty: 'beginner',
      tags: ['e2e'],
      questions: [{ text: '1 + 1 = ?', options: ['1', '2', '3', '4'], answerIndex: 1, explanation: 'basic math' }]
    }
  })
  expect(created.ok(), await created.text()).toBeTruthy()
  const quizId: string = (await created.json()).data.id

  const published = await page.request.post(`/api/v1/quizzes/${quizId}/publish`)
  expect(published.ok(), await published.text()).toBeTruthy()
  return quizId
}

test('Host 與玩家連在不同台後端，透過不開 sticky session 的負載平衡器完整玩完一局', async ({ browser }) => {
  const hostContext = await browser.newContext({ baseURL: HOST_WEB })
  const playerContext = await browser.newContext({ baseURL: PLAYER_WEB })
  const host = await hostContext.newPage()
  const player = await playerContext.newPage()

  // 先登入再建題庫：建題庫需要登入後的 session cookie。
  await registerAndLogin(host, '/library')
  const quizId = await createPublishedQuiz(host)

  await test.step('Host 在 backend-a 建立房間', async () => {
    await host.goto(`/library/${quizId}`)
    await clickUntilVisible(host.getByTestId('action-start'), host.getByTestId('create-room'))
    await host.locator('[data-testid="time-limit-option"][data-seconds="10"]').click()
    await host.getByTestId('create-room').click()
    await host.waitForURL(/\/host\/room\/[A-Z0-9]+$/)
  })
  const roomCode = host.url().split('/').pop()!

  await test.step('玩家在 backend-b 加入，Host 收到通知', async () => {
    await player.goto('/join')
    await retryUntilHydrated(async () => {
      await player.locator('[data-testid="room-code-input"] input, input[data-testid="room-code-input"]').fill(roomCode)
      await player.locator('[data-testid="nickname-input"] input, input[data-testid="nickname-input"]').fill('Alice')
      await player.locator('button[type="submit"]').click()
    }, () => player.waitForURL(`**/join/${roomCode}`, { timeout: 2_000 }))

    await expect(player.locator('.player-chip', { hasText: 'Alice' })).toBeVisible()
    await expect(host.locator('.player-chip', { hasText: 'Alice' })).toBeVisible()
  })

  await test.step('Host 開始遊戲，玩家收到題目並作答', async () => {
    await host.getByTestId('start-game').click()

    await expect(player.locator('.quiz-text')).toHaveText('1 + 1 = ?')
    await player.locator('.option-card', { hasText: /^\s*B\s*2\s*$/ }).click()
  })

  await test.step('所有人都答完，Host 推進到結束，兩邊都看到結算畫面', async () => {
    // 作答階段全員答完會提前結算；公布答案階段要等 10 秒倒數，所以給比較長的等待時間。
    await expect(host.getByTestId('game-ended-title')).toBeVisible({ timeout: 45_000 })
    await expect(player.getByTestId('game-ended-title')).toBeVisible({ timeout: 45_000 })
  })

  await hostContext.close()
  await playerContext.close()
})
