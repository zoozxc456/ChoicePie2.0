<template>
  <NuxtLink
    :to="`/library/${quiz.id}`"
    class="bg-cp-surface rounded-2xl overflow-hidden border border-cp-border transition-all hover:shadow-cp-md hover:border-transparent hover:scale-[1.02]"
  >
    <div class="relative aspect-square">
      <QuizCoverThumbnail
        :cover-image-url="quiz.coverImageUrl"
        :cover-emoji="quiz.coverEmoji"
        :cover-gradient="quiz.coverGradient"
        size="full"
        rounded="none"
      />
      <div
        v-if="featured"
        class="absolute bottom-2.5 right-2.5 w-10 h-10 rounded-full bg-cp-primary text-white flex items-center justify-center text-base"
      >
        <UIcon name="i-lucide-play" />
      </div>
    </div>
    <div class="p-3">
      <p class="text-sm font-semibold truncate text-cp-text-primary">
        {{ quiz.title }}
      </p>
      <p class="text-xs text-cp-text-muted mt-1">
        {{ quiz.tags[0] }}
      </p>
      <span
        class="inline-block mt-2 text-[11px] font-semibold tracking-wide px-2 py-0.5 rounded-full"
        :class="difficultyClass"
      >
        {{ DIFFICULTY_LABEL[quiz.difficulty] }}
      </span>
    </div>
  </NuxtLink>
</template>

<script lang="ts" setup>
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import { DIFFICULTY_LABEL } from '~/types/quiz'
import type { Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
  featured?: boolean
}

const props = defineProps<Props>()

const difficultyClass = computed(() => ({
  beginner: 'bg-success-100 text-success-800',
  intermediate: 'bg-warning-100 text-warning-800',
  expert: 'bg-error-100 text-error-800'
}[props.quiz.difficulty]))
</script>

<style scoped lang="scss"></style>
