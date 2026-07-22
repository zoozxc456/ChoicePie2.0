<template>
  <div class="min-h-[calc(100vh-56px)] flex flex-col items-center gap-8 px-6 py-16">
    <NuxtLink
      to="/library"
      class="self-start max-w-190 w-full mx-auto text-[13px] text-neutral-600"
    >
      ← {{ t('libraryDetail.backToLibrary') }}
    </NuxtLink>

    <div class="text-center">
      <h1 class="text-2xl font-extrabold">
        {{ t('hostNew.choice.title') }}
      </h1>
      <p class="text-sm text-neutral-600 mt-1.5">
        {{ t('hostNew.choice.subtitle') }}
      </p>
    </div>

    <div class="flex gap-5 flex-wrap justify-center max-w-190">
      <NuxtLink
        to="/library/new/manual"
        class="cursor-pointer w-80 bg-white border-[1.5px] border-neutral-200 hover:border-primary-500 rounded-2xl px-7 py-8 flex flex-col items-center text-center gap-3 transition-colors"
      >
        <div class="text-4xl">
          ✍️
        </div>
        <p class="text-lg font-bold">
          {{ t('hostNew.choice.manual.title') }}
        </p>
        <p class="text-[13px] text-neutral-600 leading-relaxed">
          {{ t('hostNew.choice.manual.desc') }}
        </p>
        <div class="mt-2 h-10 px-5 rounded-full bg-secondary-800 text-white font-bold text-[13px] flex items-center whitespace-nowrap">
          {{ t('hostNew.choice.manual.cta') }}
        </div>
      </NuxtLink>

      <NuxtLink
        to="/library/new/ai"
        class="cursor-pointer w-80 bg-white border-[1.5px] rounded-2xl px-7 py-8 flex flex-col items-center text-center gap-3 transition-colors"
        :class="quizStore.canUseAiToday ? 'border-neutral-200 hover:border-primary-500' : 'border-neutral-200 opacity-60'"
      >
        <div class="text-4xl">
          ✨
        </div>
        <p class="text-lg font-bold">
          {{ t('hostNew.choice.ai.title') }}
        </p>
        <p class="text-[13px] text-neutral-600 leading-relaxed">
          {{ t('hostNew.choice.ai.desc') }}
        </p>
        <span
          class="text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap"
          :class="quizStore.canUseAiToday ? 'bg-success-100 text-success-800' : 'bg-error-100 text-error-800'"
        >
          {{ quizStore.canUseAiToday ? t('hostNew.choice.ai.quotaAvailable') : t('hostNew.choice.ai.quotaUsed') }}
        </span>
        <div
          class="mt-2 h-10 px-5 rounded-full text-white font-bold text-[13px] flex items-center whitespace-nowrap"
          :class="quizStore.canUseAiToday ? 'bg-primary-500' : 'bg-neutral-300'"
        >
          {{ quizStore.canUseAiToday ? t('hostNew.choice.ai.cta') : t('hostNew.choice.ai.ctaLocked') }}
        </div>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
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
