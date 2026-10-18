// @vitest-environment nuxt
import { describe, it, expect, beforeEach } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import BigscreenQuestion from '~/components/bigscreen/BigscreenQuestion.vue'

const question = { index: 1, total: 5, text: 'Largest planet?', options: ['Mercury', 'Venus', 'Jupiter', 'Mars'], timeLimit: 20 }

describe('BigscreenQuestion', () => {
  let gameStore: ReturnType<typeof useGameStore>

  beforeEach(() => {
    gameStore = useGameStore()
    gameStore.$patch({
      currentQuestion: question,
      timeLeft: 15,
      isPaused: false,
      correctAnswerIndex: null,
      answeredCount: 0,
      totalCount: 0
    })
  })

  it('顯示題號與題目', async () => {
    const wrapper = await mountSuspended(BigscreenQuestion)

    const progress = wrapper.find('[data-testid="question-progress"]').text()
    expect(progress).toContain('2')
    expect(progress).toContain('5')
    expect(wrapper.find('[data-testid="question-text"]').text()).toBe('Largest planet?')
  })

  it('渲染四個選項', async () => {
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.findAll('[data-testid="bigscreen-option"]')).toHaveLength(4)
  })

  it('剩餘秒數充足時計時器為主色', async () => {
    const wrapper = await mountSuspended(BigscreenQuestion)

    const timer = wrapper.find('[data-testid="timer"]')
    expect(timer.text()).toBe('15')
    expect(timer.classes()).toContain('text-cp-primary')
  })

  it('剩 5 秒內計時器轉為警示色', async () => {
    gameStore.$patch({ timeLeft: 3 })
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.find('[data-testid="timer"]').classes()).toContain('text-cp-danger')
  })

  it('暫停時顯示暫停遮罩', async () => {
    gameStore.$patch({ isPaused: true })
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.find('[data-testid="paused-overlay"]').exists()).toBe(true)
  })

  it('未暫停時不顯示遮罩', async () => {
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.find('[data-testid="paused-overlay"]').exists()).toBe(false)
  })

  it('作答進度條依已作答比例', async () => {
    gameStore.$patch({ answeredCount: 2, totalCount: 4 })
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.find('[data-testid="answer-progress-bar"]').attributes('style')).toContain('width: 50%')
  })

  it('沒有玩家時作答進度為 0%', async () => {
    const wrapper = await mountSuspended(BigscreenQuestion)

    expect(wrapper.find('[data-testid="answer-progress-bar"]').attributes('style')).toContain('width: 0%')
  })
})
