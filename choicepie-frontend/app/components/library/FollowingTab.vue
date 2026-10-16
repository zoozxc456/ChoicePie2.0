<template>
  <div>
    <div
      v-if="followingStore.isLoadingCreators || followingStore.isLoadingFeed"
      class="flex justify-center py-20"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-4xl text-cp-primary"
      />
    </div>

    <div
      v-else-if="!followingStore.creators.length"
      class="text-center py-20"
    >
      <UIcon
        name="i-lucide-users"
        class="text-5xl mb-4 text-cp-text-muted"
      />
      <p class="font-bold text-base mb-1.5 text-cp-text-primary">
        {{ t('library.following.empty') }}
      </p>
      <p class="text-sm text-cp-text-secondary mb-6">
        {{ t('library.following.emptyDesc') }}
      </p>
      <UButton
        color="primary"
        size="lg"
        class="rounded-full px-6 font-bold"
        @click="emit('browse')"
      >
        {{ t('library.following.emptyCta') }}
      </UButton>
    </div>

    <template v-else>
      <!-- Followed creators -->
      <section class="mb-10">
        <h2 class="text-lg font-bold mb-4 text-cp-text-primary">
          {{ t('library.following.creators') }}
        </h2>
        <div class="flex gap-4 overflow-x-auto pb-2">
          <NuxtLink
            v-for="creator in followingStore.creators"
            :key="creator.id"
            :to="`/creators/${creator.id}`"
            class="shrink-0 w-28 flex flex-col items-center gap-2 text-center rounded-xl"
          >
            <CreatorAvatar
              :name="creator.name"
              :avatar="creator.avatar"
              size="lg"
            />
            <span class="text-sm font-semibold w-full truncate text-cp-text-primary">{{ creator.name }}</span>
            <span class="text-xs text-cp-text-muted">
              {{ t('libraryDetail.creator.quizCount', { count: creator.quizCount }) }}
            </span>
          </NuxtLink>
          <UButton
            v-if="followingStore.hasMoreCreators"
            color="neutral"
            variant="ghost"
            class="shrink-0 self-center"
            :loading="followingStore.isLoadingMoreCreators"
            @click="followingStore.loadMoreCreators()"
          >
            {{ t('library.loadMore') }}
          </UButton>
        </div>
      </section>

      <!-- Feed -->
      <section>
        <h2 class="text-lg font-bold mb-4 text-cp-text-primary">
          {{ t('library.following.feed') }}
        </h2>
        <p
          v-if="!followingStore.feed.length"
          class="text-sm text-cp-text-muted py-8 text-center"
        >
          {{ t('library.following.feedEmpty') }}
        </p>
        <div
          v-else
          class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(220px,1fr))]"
        >
          <QuizCard
            v-for="quiz in followingStore.feed"
            :key="quiz.id"
            :quiz="quiz"
          />
        </div>
        <div
          v-if="followingStore.hasMoreFeed"
          class="flex justify-center mt-8"
        >
          <UButton
            color="neutral"
            variant="outline"
            class="rounded-full px-6"
            :loading="followingStore.isLoadingMoreFeed"
            @click="followingStore.loadMoreFeed()"
          >
            {{ t('library.loadMore') }}
          </UButton>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import CreatorAvatar from '~/components/creator/CreatorAvatar.vue'
import QuizCard from '~/components/library/QuizCard.vue'
import { useFollowingStore } from '~/stores/following'

const emit = defineEmits<{ browse: [] }>()

const { t } = useI18n()
const followingStore = useFollowingStore()

await useAsyncData('library-following', () => followingStore.fetchAll(), {
  // 切換分頁時重新載入，僅在 hydration 時沿用 SSR 資料
  getCachedData: (k, nuxtApp) => (nuxtApp.isHydrating ? nuxtApp.payload.data[k] : undefined)
})
</script>

<script lang="ts">
export default {
  name: 'FollowingTab'
}
</script>

<style scoped lang="scss">
</style>
