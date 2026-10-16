<template>
  <div>
    <LoadingSpinner
      v-if="favoritesStore.isLoading"
    />

    <EmptyState
      v-else-if="!favoritesStore.quizzes.length"
      :title="t('library.favorites.empty')"
      :description="t('library.favorites.emptyDesc')"
      icon="i-lucide-heart"
    >
      <UButton
        color="primary"
        size="lg"
        class="rounded-full px-6 font-bold"
        @click="emit('browse')"
      >
        {{ t('library.favorites.emptyCta') }}
      </UButton>
    </EmptyState>

    <template v-else>
      <p
        v-if="favoritesStore.removeError"
        class="text-sm text-cp-danger mb-3"
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
            class="absolute top-2 right-2 w-8 h-8 rounded-full bg-cp-surface text-cp-danger flex items-center justify-center shadow-cp-sm cursor-pointer transition-colors hover:bg-cp-danger-bg focus-visible:outline-2 focus-visible:outline-cp-primary disabled:opacity-60"
            :title="t('library.favorites.remove')"
            :aria-label="t('library.favorites.remove')"
            :disabled="favoritesStore.removingId === quiz.id"
            @click="favoritesStore.unfavorite(quiz.id)"
          >
            <UIcon
              name="i-lucide-heart"
              class="fill-current"
              aria-hidden="true"
            />
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
import EmptyState from '~/components/common/EmptyState.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import QuizCard from '~/components/library/QuizCard.vue'
import { useFavoritesStore } from '~/stores/favorites'

const emit = defineEmits<{ browse: [] }>()

const { t } = useI18n()
const favoritesStore = useFavoritesStore()

await useAsyncData('library-favorites', async () => {
  await favoritesStore.fetchFavorites()
  // 回傳非 undefined，hydration 時 getCachedData 才拿得到 SSR 結果
  return true
}, {
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
