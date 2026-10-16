<template>
  <LoadingSpinner v-if="gameSessionStore.isLoading" />

  <div
    v-else-if="!session"
    class="max-w-3xl mx-auto px-6 py-20 text-center"
  >
    <p class="text-sm text-cp-text-secondary mb-4">
      {{ t('history.detail.notFound') }}
    </p>
    <NuxtLink to="/history">
      <UButton
        color="primary"
        size="lg"
        class="rounded-full px-6 font-bold"
      >
        {{ t('history.detail.back') }}
      </UButton>
    </NuxtLink>
  </div>

  <div
    v-else
    class="max-w-6xl mx-auto px-6 py-8"
  >
    <BackLink
      to="/history"
      :label="t('history.detail.back')"
      class="mb-6"
    />

    <div class="grid grid-cols-1 lg:grid-cols-[minmax(0,320px)_1fr] gap-6 items-start">
      <!-- ─── Left: Game info ─── -->
      <div class="flex flex-col gap-5">
        <GameInfoCard :session="session" />
        <WrongAnswerReview
          v-if="!session.isHost"
          :wrong-answers="session.myWrongAnswers"
        />
      </div>

      <!-- ─── Right: Podium + ranking ─── -->
      <div class="flex flex-col gap-6">
        <section class="rounded-2xl bg-white border border-cp-border p-6">
          <h2 class="text-sm font-bold text-center mb-6">
            {{ t('history.detail.podiumTitle') }}
          </h2>
          <ResultPodium
            :rankings="session.rankings"
            size="sm"
            animated
            :highlight-nickname="myNickname"
            :highlight-suffix="t('history.detail.you')"
          />
        </section>

        <section class="rounded-2xl bg-white overflow-hidden border border-cp-border">
          <h2 class="text-sm font-bold px-5 pt-4 pb-3">
            {{ t('history.detail.fullRanking') }}
          </h2>
          <RankingBarList
            :rankings="session.rankings"
            :highlight-nickname="myNickname"
            :highlight-suffix="t('history.detail.you')"
          />
        </section>

        <QuestionBreakdownList
          v-if="session.isHost"
          :questions="session.questionBreakdown"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import BackLink from '~/components/common/BackLink.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import ResultPodium from '~/components/common/ResultPodium.vue'
import GameInfoCard from '~/components/history/GameInfoCard.vue'
import QuestionBreakdownList from '~/components/history/QuestionBreakdownList.vue'
import RankingBarList from '~/components/history/RankingBarList.vue'
import WrongAnswerReview from '~/components/history/WrongAnswerReview.vue'
import { useGameSessionStore } from '~/stores/gameSession'

definePageMeta({ layout: 'content', middleware: ['auth'] })

const { t } = useI18n()
const route = useRoute()
const gameSessionStore = useGameSessionStore()

const session = computed(() => gameSessionStore.currentSession)

// 主持人沒有自己的名次，不需要標示「你」
const myNickname = computed(() => {
  if (!session.value || session.value.isHost) return undefined
  return session.value.rankings.find(r => r.rank === session.value?.myRank)?.nickname
})

onMounted(() => {
  gameSessionStore.fetchSessionById(route.params.id as string)
})
</script>

<script lang="ts">
export default {
  name: 'HistoryDetailPage'
}
</script>

<style scoped lang="scss"></style>
