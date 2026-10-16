<template>
  <div>
    <div
      v-if="favoritesStore.isLoading"
      class="flex justify-center py-20"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-4xl text-primary-500"
      />
    </div>

    <div
      v-else-if="!favoritesStore.quizzes.length"
      class="text-center py-20"
    >
      <div class="text-6xl mb-4">
        ♡
      </div>
      <p class="font-bold text-base mb-1.5">
        {{ t('library.favorites.empty') }}
      </p>
      <p class="text-sm text-neutral-600 mb-6">
        {{ t('library.favorites.emptyDesc') }}
      </p>
      <UButton
        color="primary"
        size="lg"
        class="rounded-full px-6 font-bold"
        @click="emit('browse')"
      >
        {{ t('library.favorites.emptyCta') }}
      </UButton>
    </div>

    <template v-else>
      <p
        v-if="favoritesStore.removeError"
        class="text-sm text-error-600 mb-3"
      >
        {{ t('library.favorites.removeError') }}
      </p>
      <div class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(220px,1fr))]">
        <div
          v-for="quiz in favoritesStore.quizzes"
          :key="quiz.id"
          class="relative"
        >
          <QuizCard :quiz="quiz" />
          <button
            class="absolute top-2 right-2 w-8 h-8 rounded-full bg-white/90 text-error-500 flex items-center justify-center shadow cursor-pointer disabled:opacity-60"
            :title="t('library.favorites.remove')"
            :aria-label="t('library.favorites.remove')"
            :disabled="favoritesStore.removingId === quiz.id"
            @click="favoritesStore.unfavorite(quiz.id)"
          >
            ♥
          </button>
        </div>
      </div>

      <div
        v-if="favoritesStore.hasNext"
        class="flex justify-center mt-8"
      >
        <UButton
          color="neutral"
          variant="outline"
          class="rounded-full px-6"
          :loading="favoritesStore.isLoadingMore"
          @click="favoritesStore.loadMore()"
        >
          {{ t('library.loadMore') }}
        </UButton>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import QuizCard from '~/components/library/QuizCard.vue'
import { useFavoritesStore } from '~/stores/favorites'

const emit = defineEmits<{ browse: [] }>()

const { t } = useI18n()
const favoritesStore = useFavoritesStore()

await useAsyncData('library-favorites', () => favoritesStore.fetchFavorites(), {
  // 切換分頁時重新載入，僅在 hydration 時沿用 SSR 資料
  getCachedData: (k, nuxtApp) => (nuxtApp.isHydrating ? nuxtApp.payload.data[k] : undefined)
})
</script>

<script lang="ts">
export default {
  name: 'FavoritesTab'
}
</script>

<style scoped lang="scss">
</style>
