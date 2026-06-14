<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <!-- Search Bar -->
    <div class="mb-6">
      <UInput
        v-model="search"
        :placeholder="t('library.searchPlaceholder')"
        size="lg"
        icon="i-lucide-search"
        class="max-w-md"
      />
    </div>

    <!-- Tag Filters -->
    <div class="flex gap-2 flex-wrap mb-8">
      <button
        v-for="tag in tags"
        :key="tag"
        class="px-4 py-1.5 rounded-full text-sm font-medium transition-all"
        :style="activeTag === tag
          ? 'background: var(--cp-primary); color: white;'
          : 'background: var(--cp-surface); color: var(--cp-text-secondary); border: 1px solid var(--cp-border);'"
        @click="activeTag = tag"
      >
        {{ tag }}
      </button>
    </div>

    <!-- Loading -->
    <div
      v-if="quizStore.isLoading"
      class="flex justify-center py-20"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-4xl"
        style="color: var(--cp-primary);"
      />
    </div>

    <template v-else>
      <!-- Featured -->
      <section class="mb-10">
        <h2 class="text-lg font-bold mb-4">
          {{ t('library.featured') }}
        </h2>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <QuizCard
            v-for="quiz in featured"
            :key="quiz.id"
            :quiz="quiz"
          />
        </div>
      </section>

      <!-- Latest -->
      <section>
        <h2 class="text-lg font-bold mb-4">
          {{ t('library.latest') }}
        </h2>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <QuizCard
            v-for="quiz in latest"
            :key="quiz.id"
            :quiz="quiz"
          />
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { useQuizStore } from '~/stores/quiz'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const quizStore = useQuizStore()

const search = ref('')
const activeTag = ref('全部')

const tags = ['全部', 'Kubernetes', 'React', 'AWS', 'SQL', 'TypeScript', 'Go', 'System Design']

const filteredQuizzes = computed(() => {
  let list = quizStore.quizzes
  if (activeTag.value !== '全部') {
    list = list.filter(q => q.tags.includes(activeTag.value))
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(quiz => quiz.title.toLowerCase().includes(q))
  }
  return list
})

const featured = computed(() => filteredQuizzes.value.slice(0, 4))
const latest = computed(() => filteredQuizzes.value.slice(4))

onMounted(() => quizStore.fetchQuizzes())
</script>

<script lang="ts">
export default {
  name: 'LibraryPage'
}
</script>

<style scoped lang="scss">
</style>
