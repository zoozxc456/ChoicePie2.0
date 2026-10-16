// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import JoinForm from '~/components/join/JoinForm.vue'

const codeInput = 'input[name="roomCode"], [data-testid="room-code-input"] input, input[data-testid="room-code-input"]'
const nicknameInput = 'input[name="nickname"], [data-testid="nickname-input"] input, input[data-testid="nickname-input"]'

const submit = async (wrapper: Awaited<ReturnType<typeof mountSuspended>>) => {
  await wrapper.find('form').trigger('submit')
  await flushPromises()
  await new Promise(resolve => setTimeout(resolve, 0))
  await flushPromises()
}

describe('JoinForm', () => {
  it('房間碼輸入時自動轉大寫', async () => {
    const wrapper = await mountSuspended(JoinForm)

    await wrapper.find(codeInput).setValue('abcd12')
    await nextTick()

    expect((wrapper.find(codeInput).element as HTMLInputElement).value).toBe('ABCD12')
  })

  it('帶入 initialRoomCode（QR Code）並轉大寫', async () => {
    const wrapper = await mountSuspended(JoinForm, { props: { initialRoomCode: 'xyz789' } })

    expect((wrapper.find(codeInput).element as HTMLInputElement).value).toBe('XYZ789')
  })

  it('資料有效時送出大寫房間碼與去除空白的暱稱', async () => {
    const wrapper = await mountSuspended(JoinForm)

    await wrapper.find(codeInput).setValue('abcd12')
    await wrapper.find(nicknameInput).setValue('  Amy ')
    await submit(wrapper)

    expect(wrapper.emitted('submit')).toEqual([[{ roomCode: 'ABCD12', nickname: 'Amy' }]])
  })

  it('房間碼長度不對時不送出', async () => {
    const wrapper = await mountSuspended(JoinForm)

    await wrapper.find(codeInput).setValue('ABC')
    await wrapper.find(nicknameInput).setValue('Amy')
    await submit(wrapper)

    expect(wrapper.emitted('submit')).toBeUndefined()
  })

  it('顯示外部傳入的錯誤訊息', async () => {
    const wrapper = await mountSuspended(JoinForm, { props: { error: 'Nickname taken' } })

    expect(wrapper.find('[data-testid="join-error"]').text()).toBe('Nickname taken')
  })
})
