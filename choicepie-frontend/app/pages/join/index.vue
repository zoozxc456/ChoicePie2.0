<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-56px)] px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-6">
        <span class="text-6xl"> 🥧 </span>
        <h1 class="text-2xl font-bold mt-6 mb-1">
          {{ t('join.title') }}
        </h1>
        <p class="text-sm text-neutral-600">
          {{ t('join.subtitle') }}
        </p>
      </div>

      <div class="rounded-2xl p-6 bg-white border border-neutral-200 shadow-cp-md">
        <UForm
          :schema="joinSchema"
          :state="joinState"
          class="flex flex-col gap-4"
          @submit="handleJoin"
        >
          <!-- Room Code -->
          <UFormField name="roomCode">
            <UInput
              v-model="joinState.roomCode"
              :placeholder="t('join.roomCodePlaceholder')"
              size="lg"
              maxlength="6"
              class="w-full"
              :ui="{
                base: 'bg-[#F5F3EF] h-14 text-center text-lg md:text-xl font-bold tracking-widest uppercase'
              }"
              @input="joinState.roomCode = joinState.roomCode.toUpperCase()"
            />
          </UFormField>

          <!-- Nickname -->
          <UFormField name="nickname">
            <UInput
              v-model="joinState.nickname"
              :placeholder="t('join.nicknamePlaceholder')"
              size="lg"
              maxlength="12"
              class="w-full"
              :ui="{
                base: 'bg-[#F5F3EF] h-14 text-center md:text-lg'
              }"
            />
          </UFormField>

          <p
            v-if="error"
            class="text-sm text-error-500"
          >
            {{ error }}
          </p>

          <UButton
            type="submit"
            block
            size="lg"
            color="primary"
            class="rounded-xl h-12 font-bold"
            :loading="isJoining"
          >
            {{ t('join.submit') }}
          </UButton>
        </UForm>
      </div>

      <!-- Steps -->
      <div class="mt-4 rounded-xl p-4 bg-white border border-neutral-200">
        <p class="text-xs font-semibold mb-3 text-neutral-400">
          {{ t('join.steps.title') }}
        </p>
        <ol class="flex flex-col gap-3">
          <li
            v-for="(step, i) in steps"
            :key="i"
            class="flex items-center gap-3"
          >
            <span
              class="w-6 h-6 rounded-full text-xs font-bold flex items-center justify-center shrink-0 text-white bg-primary-500"
            >{{
              i + 1 }}</span>
            <div>
              <p class="text-sm font-medium">
                {{ step.title }}
              </p>
              <p class="text-xs text-neutral-400">
                {{ step.desc }}
              </p>
            </div>
          </li>
        </ol>
      </div>

      <NuxtLink
        to="/"
        class="block mt-6 text-center text-sm font-medium text-neutral-400"
      >
        {{ "<- 返回首頁" }} </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as z from 'zod'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const gameStore = useGameStore()

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
  gameStore.setMyNickname(joinState.nickname.trim())
  await navigateTo(`/join/${joinState.roomCode.toUpperCase()}`)
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

<style scoped lang="scss"></style>
