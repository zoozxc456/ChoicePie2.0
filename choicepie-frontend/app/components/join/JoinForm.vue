<template>
  <div class="rounded-2xl p-6 bg-white border border-neutral-200 shadow-cp-md">
    <UForm
      :schema="joinSchema"
      :state="joinState"
      class="flex flex-col gap-4"
      @submit="handleSubmit"
    >
      <UFormField name="roomCode">
        <UInput
          v-model="joinState.roomCode"
          data-testid="room-code-input"
          :placeholder="t('join.roomCodePlaceholder')"
          size="lg"
          maxlength="6"
          class="w-full"
          :ui="{
            base: 'bg-cp-surface-muted h-14 text-center text-lg md:text-xl font-bold tracking-widest uppercase'
          }"
        />
      </UFormField>

      <UFormField name="nickname">
        <UInput
          v-model="joinState.nickname"
          data-testid="nickname-input"
          :placeholder="t('join.nicknamePlaceholder')"
          size="lg"
          maxlength="12"
          class="w-full"
          :ui="{
            base: 'bg-cp-surface-muted h-14 text-center md:text-lg'
          }"
        />
      </UFormField>

      <p
        v-if="error"
        data-testid="join-error"
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
        :loading="loading"
      >
        {{ t('join.submit') }}
      </UButton>
    </UForm>
  </div>
</template>

<script setup lang="ts">
import * as z from 'zod'

interface Props {
  /** 從 QR Code 帶入的房間碼 */
  initialRoomCode?: string
  error?: string
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  initialRoomCode: '',
  error: '',
  loading: false
})

const emit = defineEmits<{
  submit: [payload: { roomCode: string, nickname: string }]
}>()

const { t } = useI18n()

const joinSchema = z.object({
  roomCode: z.string().length(6, t('join.validation.roomCodeLength')),
  nickname: z.string().trim().min(2, t('join.validation.nicknameMin')).max(12, t('join.validation.nicknameMax'))
})

type JoinFormState = z.input<typeof joinSchema>

const joinState = reactive<JoinFormState>({
  roomCode: props.initialRoomCode.toUpperCase(),
  nickname: ''
})

// 房間碼一律大寫；用 watch 而非 @input，因為 input 事件先於 v-model 更新，會轉到上一次的值
watch(() => joinState.roomCode, (code) => {
  const upper = code.toUpperCase()
  if (upper !== code) joinState.roomCode = upper
})

const handleSubmit = () => {
  emit('submit', {
    roomCode: joinState.roomCode.toUpperCase(),
    nickname: joinState.nickname.trim()
  })
}
</script>

<script lang="ts">
export default {
  name: 'JoinForm'
}
</script>

<style scoped lang="scss"></style>
