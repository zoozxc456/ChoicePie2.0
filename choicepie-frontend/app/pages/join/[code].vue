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
      <!-- Answer feedback -->
      <div
        class="rounded-2xl p-5 mb-4 text-center"
        :style="gameStore.isCorrect
          ? 'background: var(--cp-success-bg); border: 2px solid var(--cp-success);'
          : 'background: var(--cp-danger-bg); border: 2px solid var(--cp-danger);'"
      >
        <div class="text-4xl mb-2">
          {{ gameStore.isCorrect ? '🎉' : '😅' }}
        </div>
        <p
          class="text-xl font-black mb-1"
          :style="`color: ${gameStore.isCorrect ? 'var(--cp-success)' : 'var(--cp-danger)'};`"
        >
          {{ gameStore.isCorrect ? t('room.result.correct') : t('room.result.wrong') }}
        </p>
        <p
          v-if="gameStore.isCorrect && gameStore.pointsEarned > 0"
          class="font-bold text-lg"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.points', { points: gameStore.pointsEarned.toLocaleString() }) }}
        </p>
      </div>

      <!-- Explanation -->
      <div
        v-if="gameStore.currentExplanation"
        class="rounded-2xl p-4 mb-4 explanation-text"
        style="background: var(--cp-surface-muted); border: 1px solid var(--cp-border); font-size: 14px; line-height: 1.7;"
      >
        <p
          class="text-xs font-semibold mb-2"
          style="color: var(--cp-text-muted);"
        >
          {{ t('room.result.explanation') }}
        </p>
        {{ gameStore.currentExplanation }}
      </div>

      <!-- Current rank -->
      <div
        class="rounded-2xl p-4 bg-white text-center"
        style="border: 1px solid var(--cp-border);"
      >
        <p
          class="text-xs font-semibold mb-1"
          style="color: var(--cp-text-muted);"
        >
          {{ t('room.result.currentRank') }}
        </p>
        <p
          class="text-3xl font-black"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.rank', { rank: gameStore.myRank }) }}
        </p>
        <p
          class="font-bold"
          style="color: var(--cp-text-secondary);"
        >
          {{ gameStore.myScore.toLocaleString() }}
        </p>
      </div>

      <p
        class="text-center text-sm mt-4"
        style="color: var(--cp-text-muted);"
      >
        {{ t('room.result.waitingNext') }}
      </p>
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
      <div
        class="rounded-2xl p-5 mb-5 text-center"
        style="background: var(--cp-primary-light); border: 2px solid var(--cp-primary-border);"
      >
        <p
          class="text-xs font-semibold mb-1"
          style="color: var(--cp-primary);"
        >
          {{ t('room.ended.yourResult') }}
        </p>
        <p
          class="text-4xl font-black mb-1"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.rank', { rank: gameStore.myRank }) }}
        </p>
        <p
          class="text-xl font-bold"
          style="color: var(--cp-secondary);"
        >
          {{ gameStore.myScore.toLocaleString() }}
        </p>
      </div>

      <!-- Full rankings -->
      <div
        class="rounded-2xl bg-white overflow-hidden mb-4"
        style="border: 1px solid var(--cp-border);"
      >
        <div
          v-for="(entry, i) in gameStore.rankings"
          :key="entry.nickname"
          class="flex items-center gap-3 px-4 py-3 transition-colors"
          :style="`
            ${entry.nickname === gameStore.myNickname ? 'background: var(--cp-primary-light);' : ''}
            ${i < gameStore.rankings.length - 1 ? 'border-bottom: 1px solid var(--cp-border);' : ''}
          `"
        >
          <span
            class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 text-white"
            :style="i === 0 ? 'background: var(--cp-primary);'
              : i === 1 ? 'background: var(--cp-info);'
                : i === 2 ? 'background: var(--cp-warning);'
                  : 'background: var(--cp-disabled); color: var(--cp-text-muted);'"
          >
            {{ i + 1 }}
          </span>
          <span
            class="flex-1 font-medium text-sm"
            :style="entry.nickname === gameStore.myNickname ? 'color: var(--cp-primary); font-weight: 700;' : ''"
          >
            {{ entry.nickname }}
            <span
              v-if="entry.nickname === gameStore.myNickname"
              class="text-xs ml-1"
            >{{ t('common.you') }}</span>
          </span>
          <span class="font-bold tabular-nums text-sm">{{ entry.score.toLocaleString() }}</span>
        </div>
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
  }
})



onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'JoinRoomPage'
}
</script>

<style scoped lang="scss">
</style>
