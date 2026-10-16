<template>
  <div class="bg-cp-surface rounded-2xl border border-cp-border p-5 flex flex-col items-center gap-2.5 text-center">
    <div class="w-12 h-12 rounded-full flex items-center justify-center text-lg font-bold text-white bg-cp-secondary shrink-0">
      {{ creatorName[0] }}
    </div>
    <NuxtLink
      data-testid="creator-link"
      :to="`/creators/${creatorId}`"
      class="text-sm font-bold flex items-center gap-1 text-cp-text-primary hover:underline"
    >
      {{ creatorName }}
    </NuxtLink>
    <template v-if="profile">
      <p
        data-testid="creator-stats"
        class="text-xs text-cp-text-muted"
      >
        {{ t('libraryDetail.creator.quizCount', { count: profile.quizCount }) }} ·
        {{ t('libraryDetail.creator.challengeCount', { count: profile.challengeCount }) }}
      </p>
      <FollowButton
        v-if="canFollow"
        size="sm"
        :is-following="profile.isFollowing"
        :loading="isTogglingFollow"
        @toggle="emit('toggleFollow')"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
import FollowButton from '~/components/creator/FollowButton.vue'
import type { CreatorProfileDto } from '~/types/api'

interface Props {
  creatorId: string
  creatorName: string
  profile: CreatorProfileDto | null
  /** 自己的題庫不顯示追蹤按鈕 */
  canFollow: boolean
  isTogglingFollow?: boolean
}

withDefaults(defineProps<Props>(), {
  isTogglingFollow: false
})

const emit = defineEmits<{
  toggleFollow: []
}>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'CreatorSideCard'
}
</script>

<style scoped lang="scss"></style>
