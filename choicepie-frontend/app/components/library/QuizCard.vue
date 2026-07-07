<template>
  <NuxtLink
    :to="`/library/${quiz.id}`"
    class="bg-white rounded-2xl border border-neutral-200 overflow-hidden transition-transform hover:scale-[1.02]"
  >
    <div
      class="relative aspect-square flex items-center justify-center text-5xl"
      :style="quiz.coverGradient"
    >
      {{ quiz.coverEmoji }}
      <div
        v-if="featured"
        class="absolute bottom-2.5 right-2.5 w-10 h-10 rounded-full bg-primary-500 text-white flex items-center justify-center text-base shadow-lg"
      >
        ▶
      </div>
    </div>
    <div class="p-3">
      <p class="text-sm font-semibold truncate">
        {{ quiz.title }}
      </p>
      <p class="text-xs text-neutral-400 mt-1">
        {{ quiz.tags[0] }} · {{ t('library.card.questions', { count: quiz.questionCount }) }}
      </p>
      <span
        class="inline-block mt-2 text-[11px] font-semibold tracking-wide px-2 py-0.5 rounded"
        :class="difficultyClass"
      >
        {{ DIFFICULTY_LABEL[quiz.difficulty] }}
      </span>
    </div>
  </NuxtLink>
</template>

<script lang="ts" setup>
import { DIFFICULTY_LABEL } from '~/types/quiz'
import type { Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
  featured?: boolean
}

const props = defineProps<Props>()
const { t } = useI18n()

const difficultyClass = computed(() => ({
  beginner: 'bg-success-100 text-success-800',
  intermediate: 'bg-warning-100 text-warning-800',
  expert: 'bg-error-100 text-error-800'
}[props.quiz.difficulty]))
</script>

<style scoped lang="scss"></style>
