<template>
  <div
    v-if="auth.isLoggedIn"
    class="bg-white border border-neutral-200 rounded-2xl p-5"
  >
    <h2 class="text-base font-bold mb-3">
      {{ t('attempt.history.title') }}
    </h2>

    <LoadingSpinner
      v-if="quizAttemptStore.isLoadingHistory"
      size="sm"
    />

    <p
      v-else-if="quizAttemptStore.history.length === 0"
      class="text-sm text-neutral-400"
    >
      {{ t('attempt.history.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-2"
    >
      <div
        v-for="item in quizAttemptStore.history"
        :key="item.id"
        class="flex items-center justify-between gap-3 px-3 py-2.5 rounded-xl bg-neutral-50"
      >
        <div class="flex items-center gap-2">
          <span
            class="text-xs font-bold px-2 py-1 rounded-full"
            :class="item.passed ? 'bg-success-100 text-success-800' : 'bg-error-100 text-error-800'"
          >
            {{ t('attempt.history.score', { score: item.score }) }}
          </span>
          <span class="text-xs text-neutral-400">
            {{ t('attempt.history.duration', { seconds: Math.round(item.durationMs / 1000) }) }}
          </span>
        </div>
        <span class="text-xs text-neutral-400 whitespace-nowrap">
          {{ formatDate(item.completedAt) }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'

interface Props {
  quizId: string
}

const props = defineProps<Props>()

const { t, locale } = useI18n()
const auth = useAuthStore()
const quizAttemptStore = useQuizAttemptStore()

const formatDate = (iso: string) => formatLocaleDate(iso, locale.value)

watch(() => props.quizId, (quizId) => {
  if (auth.isLoggedIn) {
    quizAttemptStore.fetchAttemptHistory(quizId)
  }
}, { immediate: true })
</script>

<script lang="ts">
export default {
  name: 'AttemptHistoryList'
}
</script>

<style scoped lang="scss"></style>
