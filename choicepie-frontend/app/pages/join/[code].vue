<template>
  <div class="max-w-md mx-auto px-4 py-6">
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
const joinError = ref('')

onMounted(async () => {
  if (!gameStore.myNickname) {
    await navigateTo(`/join?code=${code.value}`)
    return
  }

  try {
    await gameRoom.joinRoom(code.value, gameStore.myNickname)
  } catch {
    joinError.value = t('join.roomNotFound')
  }
})

onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'JoinRoomPage'
}
</script>

<style scoped lang="scss"></style>
