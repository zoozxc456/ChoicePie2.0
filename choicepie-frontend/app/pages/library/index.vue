<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <!-- AI Banner -->
    <NuxtLink
      to="/library/new"
      class="cursor-pointer bg-linear-to-br from-[#1a1a2e] to-secondary-800 rounded-2xl px-6 py-5 mb-6 flex items-center justify-between gap-4 flex-wrap"
    >
      <div>
        <p class="text-base font-bold text-white">
          {{ t('library.aiBanner.title') }}
        </p>
        <p class="text-[13px] text-white/60 mt-1">
          {{ t('library.aiBanner.subtitle') }}
        </p>
      </div>
      <div class="h-10 px-5 rounded-full bg-primary-500 text-white font-bold text-[13px] flex items-center whitespace-nowrap">
        {{ t('library.aiBanner.cta') }}
      </div>
    </NuxtLink>

    <!-- Search Bar -->
    <div class="mb-5 max-w-120">
      <UInput
        v-model="search"
        :placeholder="t('library.searchPlaceholder')"
        size="lg"
        icon="i-lucide-search"
        class="w-full"
        :ui="{ base: 'h-12 text-sm px-4' }"
      />
    </div>

    <!-- Tag Filters -->
    <div class="flex gap-2 overflow-x-auto pb-2 mb-7">
      <button
        v-for="tag in tags"
        :key="tag"
        class="shrink-0 px-4 py-2 rounded-full text-[13px] font-semibold whitespace-nowrap transition-all"
        :class="activeTag === tag
          ? 'bg-primary-500 text-white'
          : 'bg-neutral-100 text-neutral-600'"
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
        class="animate-spin text-4xl text-primary-500"
      />
    </div>

    <template v-else>
      <!-- Featured -->
      <section
        v-if="featured.length"
        class="mb-10"
      >
        <h2 class="text-lg font-bold mb-4">
          {{ t('library.featured') }}
        </h2>
        <div class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(220px,1fr))]">
          <QuizCard
            v-for="quiz in featured"
            :key="quiz.id"
            :quiz="quiz"
            featured
          />
        </div>
      </section>

      <!-- Latest -->
      <section v-if="latest.length">
        <h2 class="text-lg font-bold mb-4">
          {{ t('library.latest') }}
        </h2>
        <div class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(220px,1fr))]">
          <QuizCard
            v-for="quiz in latest"
            :key="quiz.id"
            :quiz="quiz"
          />
        </div>
      </section>

      <!-- No results -->
      <div
        v-if="!featured.length && !latest.length"
        class="text-center py-16 text-sm text-neutral-400"
      >
        {{ t('library.noResults') }}
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import QuizCard from '~/components/library/QuizCard.vue'
import { useQuizStore } from '~/stores/quiz'
import { mockQuizzes } from '~/mocks/quiz'

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

onMounted(() => {
  // 暫時用假資料，之後接上真實 API 後移除
  quizStore.quizzes = mockQuizzes
})
</script>

<script lang="ts">
export default {
  name: 'LibraryPage'
}
</script>

<style scoped lang="scss">
</style>
