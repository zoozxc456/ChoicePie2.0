<template>
  <div class="max-w-md mx-auto px-4 py-6">
    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting'">
      <WaitingRoom :code="code" />
    </template>

    <!-- ─── QUESTION ─── -->
    <template v-else-if="gameStore.phase === 'question'">
      <GamingRoom :code="code" />
    </template>

    <!-- ─── RESULT ─── -->
    <template v-else-if="gameStore.phase === 'result'">
      <QuizResult />
    </template>

    <!-- ─── ENDED ─── -->
    <template v-else-if="gameStore.phase === 'ended'">
      <div class="text-center mb-6">
        <div class="text-5xl mb-3">
          🏆
        </div>
        <h1 class="text-2xl font-bold mb-1">
          {{ t('room.ended.title') }}
        </h1>
      </div>

      <!-- My result -->
      <div class="rounded-2xl p-5 mb-5 text-center bg-primary-50 border-2 border-primary-200">
        <p class="text-xs font-semibold mb-1 text-primary-500">
          {{ t('room.ended.yourResult') }}
        </p>
        <p class="text-4xl font-black mb-1 text-primary-500">
          {{ t('room.result.rank', { rank: gameStore.myRank }) }}
        </p>
        <p class="text-xl font-bold text-secondary-800">
          {{ gameStore.myScore.toLocaleString() }}
        </p>
      </div>

      <div class="mb-4">
        <RankingList />
      </div>

      <UButton
        block
        size="lg"
        variant="outline"
        @click="$router.push('/join')"
      >
        {{ t('room.ended.playAgain') }}
      </UButton>
    </template>
  </div>
</template>

<script setup lang="ts">
import WaitingRoom from '~/components/gameRoom/WaitingRoom.vue'
import GamingRoom from '~/components/gameRoom/GamingRoom.vue'
import QuizResult from '~/components/gameRoom/result/QuizResult.vue'
import RankingList from '~/components/gameRoom/result/RankingList.vue'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
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
