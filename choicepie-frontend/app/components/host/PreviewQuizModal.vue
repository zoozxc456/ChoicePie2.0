<template>
  <Transition name="fade">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4"
      @click.self="$emit('cancel')"
    >
      <div class="rounded-2xl bg-white w-full max-w-lg max-h-[85vh] overflow-hidden flex flex-col shadow-cp-xl">
        <div class="px-6 pt-6 pb-4 shrink-0">
          <h2 class="text-xl font-bold mb-1">
            {{ t('host.previewModal.title') }}
          </h2>
          <p class="text-sm text-neutral-600">
            {{ t('host.previewModal.subtitle') }}
          </p>
        </div>

        <div class="px-6 pb-4 overflow-y-auto flex-1">
          <div
            v-if="quizTitle"
            class="mb-4"
          >
            <p class="text-xs font-semibold text-neutral-400 mb-1">
              {{ t('host.previewModal.quizTitleLabel') }}
            </p>
            <p class="text-base font-bold">
              {{ quizTitle }}
            </p>
          </div>

          <div class="flex flex-col gap-3">
            <div
              v-for="(q, qi) in questions"
              :key="qi"
              class="rounded-xl border border-neutral-200 p-4"
            >
              <div class="flex items-center gap-3 mb-2.5">
                <span class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 bg-primary-100 text-primary-500">
                  {{ qi + 1 }}
                </span>
                <p class="text-sm font-semibold flex-1">
                  {{ q.text }}
                </p>
              </div>
              <div class="flex flex-col gap-1.5 ml-10">
                <div
                  v-for="(opt, oi) in q.options"
                  :key="oi"
                  class="text-xs px-2.5 py-1.5 rounded-lg flex items-center justify-between gap-2"
                  :class="oi === q.answerIndex
                    ? 'bg-success-100 text-success-800 font-semibold'
                    : 'bg-neutral-100 text-neutral-600'"
                >
                  <span>{{ ['A', 'B', 'C', 'D'][oi] }}. {{ opt }}</span>
                  <span v-if="oi === q.answerIndex">✓</span>
                </div>
              </div>
              <p
                v-if="q.explanation"
                class="text-xs text-neutral-400 mt-2 ml-10"
              >
                💡 {{ q.explanation }}
              </p>
            </div>
          </div>
        </div>

        <div class="px-6 pb-6 pt-2 shrink-0">
          <UButton
            block
            size="lg"
            color="primary"
            :loading="loading"
            @click="$emit('confirm')"
          >
            {{ t('host.previewModal.confirm') }}
          </UButton>
          <UButton
            block
            size="md"
            color="neutral"
            variant="ghost"
            class="mt-2"
            @click="$emit('cancel')"
          >
            {{ t('host.previewModal.cancel') }}
          </UButton>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import type { Question } from '~/types/quiz'

interface Props {
  open: boolean
  questions: Question[]
  quizTitle?: string
  loading?: boolean
}

defineProps<Props>()
defineEmits<{
  confirm: []
  cancel: []
}>()

const { t } = useI18n()
</script>

<style scoped lang="scss">
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
