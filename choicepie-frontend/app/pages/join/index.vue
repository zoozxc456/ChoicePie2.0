<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-56px)] px-4 py-10">
    <div class="w-full max-w-4xl grid grid-cols-1 md:grid-cols-2 gap-8 items-center">
      <!-- 預覽：進入房間後的畫面長相 -->
      <JoinPreviewMock class="hidden md:flex order-2 md:order-1" />

      <div class="order-1 md:order-2">
        <div class="text-center mb-6">
          <span class="text-6xl"> 🥧 </span>
          <h1 class="text-2xl font-bold mt-6 mb-1">
            {{ t('join.title') }}
          </h1>
          <p class="text-sm text-neutral-600">
            {{ t('join.subtitle') }}
          </p>
        </div>

        <JoinForm
          :initial-room-code="initialRoomCode"
          :error="error"
          :loading="isJoining"
          @submit="handleJoin"
        />

        <JoinSteps class="mt-4" />

        <NuxtLink
          to="/"
          class="block mt-6 text-center text-sm font-medium text-neutral-400"
        >
          ← {{ t('join.backHome') }}
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import JoinForm from '~/components/join/JoinForm.vue'
import JoinPreviewMock from '~/components/join/JoinPreviewMock.vue'
import JoinSteps from '~/components/join/JoinSteps.vue'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const route = useRoute()
const gameStore = useGameStore()
const playerSession = usePlayerSession()

const isJoining = ref(false)
const error = ref(route.query.error === 'duplicateNickname' ? t('join.duplicateNickname') : '')

// 從 QR Code 帶入的 roomCode query
const initialRoomCode = typeof route.query.code === 'string' ? route.query.code : ''

const handleJoin = async ({ roomCode, nickname }: { roomCode: string, nickname: string }) => {
  isJoining.value = true
  error.value = ''

  // 本機有同一房間、同一暱稱的重連憑證時保留（等同回到原本的玩家）；暱稱不同代表想用新身分加入
  const stored = playerSession.load(roomCode)
  if (stored && stored.nickname.toLowerCase() !== nickname.toLowerCase()) {
    playerSession.clear(roomCode)
  }

  gameStore.setMyNickname(nickname)
  await navigateTo(`/join/${roomCode}`)
}
</script>

<script lang="ts">
export default {
  name: 'JoinPage'
}
</script>

<style scoped lang="scss"></style>
