<template>
  <div class="max-w-5xl mx-auto px-6 py-8">
    <HostWaitingRoom
      v-if="gameStore.phase === 'waiting'"
      :code="code"
    />
    <HostGamingRoom
      v-else-if="gameStore.phase === 'question' || gameStore.phase === 'result'"
      :code="code"
    />
    <HostGameEnded v-else-if="gameStore.phase === 'ended'" />
    <HostRejoinFailed v-else-if="rejoinFailed" />
    <HostReconnecting v-else />
  </div>
</template>

<script setup lang="ts">
import HostWaitingRoom from '~/components/gameRoom/host/HostWaitingRoom.vue'
import HostGamingRoom from '~/components/gameRoom/host/HostGamingRoom.vue'
import HostGameEnded from '~/components/gameRoom/host/HostGameEnded.vue'
import HostRejoinFailed from '~/components/gameRoom/host/HostRejoinFailed.vue'
import HostReconnecting from '~/components/gameRoom/host/HostReconnecting.vue'

definePageMeta({ layout: 'default', middleware: ['auth'] })

const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())

const gameStore = useGameStore()
const gameRoom = useGameRoom()

const rejoinFailed = ref(false)

// 後端本身不會自動計時，SkipQuestion 是切換式（question -> reveal -> question），故由 Host 端主動呼叫推進。
// 公布答案（result）階段只能靠倒數結束推進下一題；作答（question）階段則倒數結束或所有人送出答案（提前結算）都會推進。
watch(() => gameStore.timeLeft, (timeLeft) => {
  if (timeLeft === 0 && (gameStore.phase === 'question' || gameStore.phase === 'result')) {
    gameRoom.skipQuestion(code.value)
  }
})

watch(() => gameStore.answeredCount, () => {
  if (gameStore.isAllAnsweredEarly) {
    gameRoom.skipQuestion(code.value)
  }
})

onMounted(async () => {
  // 若非從建立房間流程 client-side 導航過來（例如重新整理、直接開啟連結），
  // gameStore 尚未有此房間的狀態，向後端取得目前房間快照
  if (gameStore.roomCode !== code.value) {
    try {
      await gameRoom.rejoinAsHost(code.value)
    } catch {
      rejoinFailed.value = true
    }
  }
})

onUnmounted(() => {
  gameRoom.disconnect()
})
</script>

<script lang="ts">
export default {
  name: 'HostRoomPage'
}
</script>

<style scoped lang="scss"></style>
