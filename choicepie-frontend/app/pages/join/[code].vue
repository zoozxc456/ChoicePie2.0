<template>
  <div class="max-w-md mx-auto px-4 py-6">
    <WaitingRoom
      v-if="gameStore.phase === 'waiting'"
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

const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())

const gameStore = useGameStore()
const gameRoom = useGameRoom()

onMounted(async () => {
  // if (!gameStore.myNickname) {
  //   await navigateTo(`/join?code=${code.value}`)
  // }

  // 刻板用假資料，之後接上真實 API 後移除
  if (gameStore.phase === 'idle') {
    gameStore.setMyNickname('Player1')
    gameStore.setRoom({
      roomCode: code.value,
      quizTitle: 'Mock Quiz',
      status: 'waiting',
      players: [
        { connectionId: '1', nickname: 'Player1', score: 0, rank: 0, hasAnswered: false },
        { connectionId: '2', nickname: 'Player2', score: 0, rank: 0, hasAnswered: false },
        { connectionId: '3', nickname: 'Player3', score: 0, rank: 0, hasAnswered: false }
      ],
      currentQuestionIndex: 0,
      totalQuestions: 5,
      hostConnectionId: 'host'
    })

    // 刻板用假題目，之後接上真實 API 後移除
    gameStore.setQuestion({
      index: 0,
      total: 5,
      text: '台灣最高的山是哪一座？',
      options: ['玉山', '雪山', '合歡山', '阿里山'],
      timeLimit: 20
    })

    // 刻板用假結果，之後接上真實 API 後移除
    gameStore.selectAnswer(0)
    gameStore.setAnswerResult({
      isCorrect: true,
      correctAnswerIndex: 0,
      pointsEarned: 850
    })
    gameStore.setQuestionEnd({
      answerIndex: 0,
      explanation: '玉山海拔 3,952 公尺，是台灣及東亞最高峰。',
      rankings: [
        { rank: 1, nickname: 'Player1', score: 850 },
        { rank: 2, nickname: 'Player2', score: 620 },
        { rank: 3, nickname: 'Player3', score: 400 }
      ]
    })

    // 刻板用假最終排名，之後接上真實 API 後移除
    gameStore.endGame([
      { rank: 1, nickname: 'Player1', score: 4200 },
      { rank: 2, nickname: 'Player2', score: 3150 },
      { rank: 3, nickname: 'Player3', score: 2600 }
    ])
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
