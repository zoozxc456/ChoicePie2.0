<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <div
      v-if="creatorStore.profile"
      class="mb-10"
    >
      <div class="bg-white border border-neutral-200 rounded-2xl p-6 flex items-center gap-5 flex-wrap">
        <CreatorAvatar
          :name="creatorStore.profile.name"
          :avatar="creatorStore.profile.avatar"
          size="xl"
        />
        <div class="flex-1 min-w-48">
          <h1 class="text-2xl font-black">
            {{ creatorStore.profile.name }}
          </h1>
          <p class="text-sm text-neutral-500 mt-1">
            {{ t('libraryDetail.creator.quizCount', { count: creatorStore.profile.quizCount }) }} ·
            {{ t('libraryDetail.creator.challengeCount', { count: creatorStore.profile.challengeCount }) }}
          </p>
        </div>
        <NuxtLink
          v-if="!auth.isLoggedIn"
          :to="`/login?redirect=${encodeURIComponent(route.fullPath)}`"
          class="h-9 px-5 rounded-full text-sm font-semibold bg-primary-500 text-white flex items-center whitespace-nowrap"
        >
          {{ t('creatorPage.followToLogin') }}
        </NuxtLink>
        <button
          v-else-if="!isSelf"
          class="h-9 px-5 rounded-full text-sm font-semibold border whitespace-nowrap cursor-pointer disabled:opacity-60"
          :class="creatorStore.profile.isFollowing
            ? 'border-neutral-200 bg-white'
            : 'border-transparent bg-primary-500 text-white'"
          :disabled="creatorStore.isTogglingFollow"
          @click="handleToggleFollow"
        >
          {{ creatorStore.profile.isFollowing ? t('libraryDetail.creator.following') : t('libraryDetail.creator.follow') }}
        </button>
      </div>
    </div>

    <section v-if="creatorStore.profile">
      <h2 class="text-lg font-bold mb-4">
        {{ t('creatorPage.quizzes') }}
      </h2>

      <p
        v-if="creatorStore.quizzesHasError"
        class="text-sm text-error-600 mb-3"
      >
        {{ t('creatorPage.loadError') }}
      </p>

      <p
        v-if="!creatorStore.quizzes.length && !creatorStore.isLoadingQuizzes"
        class="text-sm text-neutral-400 py-12 text-center"
      >
        {{ t('creatorPage.noQuizzes') }}
      </p>
      <div
        v-else
        class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(220px,1fr))]"
      >
        <QuizCard
          v-for="quiz in creatorStore.quizzes"
          :key="quiz.id"
          :quiz="quiz"
        />
      </div>

      <div
        v-if="creatorStore.quizzesHasNext"
        class="flex justify-center mt-8"
      >
        <UButton
          color="neutral"
          variant="outline"
          class="rounded-full px-6"
          :loading="creatorStore.isLoadingMoreQuizzes"
          @click="creatorStore.loadMoreQuizzes()"
        >
          {{ t('library.loadMore') }}
        </UButton>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import CreatorAvatar from '~/components/creator/CreatorAvatar.vue'
import QuizCard from '~/components/library/QuizCard.vue'
import { useAuthStore } from '~/stores/auth'
import { useCreatorStore } from '~/stores/creator'

definePageMeta({ layout: 'content' })

const { t } = useI18n()
const route = useRoute()
const auth = useAuthStore()
const creatorStore = useCreatorStore()

const creatorId = route.params.id as string

// SSR：同時取得創作者資料與第一頁作品
await useAsyncData(`creator-${creatorId}`, async () => {
  await Promise.all([
    creatorStore.fetchCreatorProfile(creatorId),
    creatorStore.fetchCreatorQuizzes(creatorId)
  ])
  return true
})

if (!creatorStore.profile || creatorStore.profile.id !== creatorId) {
  throw createError({ statusCode: 404, statusMessage: t('creatorPage.notFound'), fatal: true })
}

const isSelf = computed(() => auth.user?.id === creatorId)

useSeoMeta({
  title: () => t('creatorPage.seoTitle', { name: creatorStore.profile?.name ?? '' }),
  ogTitle: () => t('creatorPage.seoTitle', { name: creatorStore.profile?.name ?? '' }),
  description: () => creatorStore.profile
    ? t('creatorPage.seoDescription', {
        name: creatorStore.profile.name,
        count: creatorStore.profile.quizCount,
        challenges: creatorStore.profile.challengeCount
      })
    : undefined,
  ogType: 'profile'
})

const handleToggleFollow = async () => {
  try {
    await creatorStore.toggleFollow(creatorId)
  } catch {
    // 錯誤已寫入 creatorStore.error
  }
}
</script>

<script lang="ts">
export default {
  name: 'CreatorPage'
}
</script>

<style scoped lang="scss">
</style>
