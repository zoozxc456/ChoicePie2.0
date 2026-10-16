<template>
  <div class="max-w-md mx-auto px-4 py-6">
    <HostOfflineBanner />
    <p
      v-if="joinError"
      class="text-center text-sm text-error-500"
    >
      {{ joinError }}
    </p>
    <WaitingRoom
      v-else-if="gameStore.phase === 'waiting'"
      :code="code"
    />
    <GamingRoom
      v-else-if="gameStore.phase === 'question'"
      :code="code"
    />
    <QuizResult v-else-if="gameStore.phase === 'result'" />
    <GameEnded v-else-if="gameStore.phase === 'ended'" />
  </div>
</template>

<script setup lang="ts">
import HostOfflineBanner from '~/components/gameRoom/HostOfflineBanner.vue'
import WaitingRoom from '~/components/gameRoom/WaitingRoom.vue'
import GamingRoom from '~/components/gameRoom/GamingRoom.vue'
import QuizResult from '~/components/gameRoom/result/QuizResult.vue'
import GameEnded from '~/components/gameRoom/GameEnded.vue'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())

const gameStore = useGameStore()
const gameRoom = useGameRoom()
const playerSession = usePlayerSession()
const joinError = ref('')

/** 以全新玩家身分加入；暱稱重複時回到加入表單提示換暱稱 */
const joinFresh = async (nickname: string) => {
  try {
    await gameRoom.joinRoom(code.value, nickname)
  } catch (e) {
    if (extractHubErrorCode(e) === 'GAME_ROOM_DUPLICATE_NICKNAME') {
      await navigateTo(`/join?code=${code.value}&error=duplicateNickname`)
      return
    }
    joinError.value = t('join.roomNotFound')
  }
}

onMounted(async () => {
  const stored = playerSession.load(code.value)

  if (stored) {
    try {
      await gameRoom.rejoinAsPlayer(code.value, stored.reconnectToken)
      return
    } catch {
      // 憑證失效或房間已不存在：清除後以原暱稱重新加入
      playerSession.clear(code.value)
    }
  }

  const nickname = stored?.nickname || gameStore.myNickname
  if (!nickname) {
    await navigateTo(`/join?code=${code.value}`)
    return
  }

  await joinFresh(nickname)
})

onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'JoinRoomPage'
}
</script>

<style scoped lang="scss"></style>
