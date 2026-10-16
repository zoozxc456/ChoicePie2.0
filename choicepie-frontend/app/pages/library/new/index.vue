<template>
  <div class="min-h-[calc(100vh-56px)] flex flex-col items-center gap-8 px-6 py-16">
    <BackLink
      to="/library"
      :label="t('libraryDetail.backToLibrary')"
      class="self-start max-w-190 w-full mx-auto"
    />

    <div class="text-center">
      <h1 class="text-2xl font-extrabold text-cp-text-primary">
        {{ t('hostNew.choice.title') }}
      </h1>
      <p class="text-sm text-cp-text-secondary mt-1.5">
        {{ t('hostNew.choice.subtitle') }}
      </p>
    </div>

    <div class="flex gap-5 flex-wrap justify-center max-w-190">
      <CreateMethodCard
        to="/library/new/manual"
        icon="i-lucide-pencil-line"
        icon-class="text-cp-secondary"
        cta-class="bg-cp-secondary"
        :title="t('hostNew.choice.manual.title')"
        :description="t('hostNew.choice.manual.desc')"
        :cta-label="t('hostNew.choice.manual.cta')"
      />
      <CreateMethodCard
        to="/library/new/ai"
        icon="i-lucide-sparkles"
        :dimmed="!quizStore.canUseAiToday"
        :cta-class="quizStore.canUseAiToday ? 'bg-cp-primary' : 'bg-cp-disabled'"
        :title="t('hostNew.choice.ai.title')"
        :description="t('hostNew.choice.ai.desc')"
        :cta-label="quizStore.canUseAiToday ? t('hostNew.choice.ai.cta') : t('hostNew.choice.ai.ctaLocked')"
      >
        <template #badge>
          <StatusBadge :tone="quizStore.canUseAiToday ? 'success' : 'error'">
            {{ quizStore.canUseAiToday ? t('hostNew.choice.ai.quotaAvailable') : t('hostNew.choice.ai.quotaUsed') }}
          </StatusBadge>
        </template>
      </CreateMethodCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import BackLink from '~/components/common/BackLink.vue'
import StatusBadge from '~/components/common/StatusBadge.vue'
import CreateMethodCard from '~/components/library/editor/CreateMethodCard.vue'
import { useQuizStore } from '~/stores/quiz'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()
</script>

<script lang="ts">
export default {
  name: 'HostNewChoicePage'
}
</script>

<style scoped lang="scss"></style>
