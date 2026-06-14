<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-56px)] px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-6">
        <h1 class="text-2xl font-bold mb-1">
          {{ t('join.title') }}
        </h1>
        <p style="color: var(--cp-text-secondary); font-size: 14px;">
          {{ t('join.subtitle') }}
        </p>
      </div>

      <div
        class="rounded-2xl p-6 bg-white"
        style="border: 1px solid var(--cp-border); box-shadow: var(--cp-shadow-md);"
      >
        <UForm
          :schema="joinSchema"
          :state="joinState"
          class="flex flex-col gap-4"
          @submit="handleJoin"
        >
          <!-- Room Code -->
          <UFormField
            name="roomCode"
            :label="t('join.roomCode')"
          >
            <UInput
              v-model="joinState.roomCode"
              :placeholder="t('join.roomCodePlaceholder')"
              size="lg"
              maxlength="6"
              class="text-center tracking-widest text-xl font-bold uppercase w-full"
              style="color: var(--cp-primary);"
              @input="joinState.roomCode = joinState.roomCode.toUpperCase()"
            />
          </UFormField>

          <!-- Nickname -->
          <UFormField
            name="nickname"
            :label="t('join.nickname')"
          >
            <UInput
              v-model="joinState.nickname"
              :placeholder="t('join.nicknamePlaceholder')"
              size="lg"
              maxlength="12"
              class="w-full"
            />
          </UFormField>

          <p
            v-if="error"
            class="text-sm"
            style="color: var(--cp-danger);"
          >
            {{ error }}
          </p>

          <UButton
            type="submit"
            block
            size="lg"
            color="primary"
            :loading="isJoining"
          >
            {{ t('join.submit') }}
          </UButton>
        </UForm>
      </div>

      <!-- Steps -->
      <div
        class="mt-4 rounded-xl p-4 bg-white"
        style="border: 1px solid var(--cp-border);"
      >
        <p
          class="text-xs font-semibold mb-3"
          style="color: var(--cp-text-muted);"
        >
          {{ t('join.steps.title') }}
        </p>
        <ol class="flex flex-col gap-3">
          <li
            v-for="(step, i) in steps"
            :key="i"
            class="flex items-start gap-3"
          >
            <span
              class="w-6 h-6 rounded-full text-xs font-bold flex items-center justify-center shrink-0 text-white"
              style="background: var(--cp-primary);"
            >{{ i + 1 }}</span>
            <div>
              <p class="text-sm font-medium">
                {{ step.title }}
              </p>
              <p
                class="text-xs"
                style="color: var(--cp-text-muted);"
              >
                {{ step.desc }}
              </p>
            </div>
          </li>
        </ol>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as z from 'zod'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const gameRoom = useGameRoom()

const isJoining = ref(false)
const error = ref('')

const joinSchema = z.object({
  roomCode: z.string().length(6, t('join.validation.roomCodeLength')),
  nickname: z.string().min(2, t('join.validation.nicknameMin')).max(12, t('join.validation.nicknameMax'))
})

type JoinForm = z.infer<typeof joinSchema>

const joinState = reactive<JoinForm>({
  roomCode: '',
  nickname: ''
})

const handleJoin = async () => {
  isJoining.value = true
  error.value = ''
  try {
    await gameRoom.joinRoom(joinState.roomCode.toUpperCase(), joinState.nickname.trim())
    await navigateTo(`/join/${joinState.roomCode.toUpperCase()}`)
  } catch {
    error.value = t('join.roomNotFound')
    isJoining.value = false
  }
}

const steps = computed(() => [
  { title: t('join.steps.step1Title'), desc: t('join.steps.step1Desc') },
  { title: t('join.steps.step2Title'), desc: t('join.steps.step2Desc') },
  { title: t('join.steps.step3Title'), desc: t('join.steps.step3Desc') }
])

// 從 QR Code 帶入的 roomCode query
const route = useRoute()
if (route.query.code) {
  joinState.roomCode = (route.query.code as string).toUpperCase()
}
</script>

<script lang="ts">
export default {
  name: 'JoinPage'
}
</script>

<style scoped lang="scss">
</style>
