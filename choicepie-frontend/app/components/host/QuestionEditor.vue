<template>
  <div
    class="rounded-2xl bg-white overflow-hidden transition-shadow border-[1.5px]"
    :class="expanded ? 'border-primary-500' : isComplete ? 'border-success-500' : 'border-warning-500'"
  >
    <button
      class="w-full flex items-start gap-4 p-5 text-left"
      @click="$emit('toggle')"
    >
      <span
        class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0"
        :class="isComplete ? 'bg-success-100 text-success-800' : 'bg-warning-100 text-warning-800'"
      >
        {{ index + 1 }}
      </span>
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2">
          <p class="font-medium text-sm leading-relaxed">
            {{ question.text || t('host.questionEditor.untitled') }}
          </p>
          <span
            class="shrink-0 text-[11px] font-semibold px-2 py-0.5 rounded-full whitespace-nowrap"
            :class="isComplete ? 'bg-success-100 text-success-800' : 'bg-warning-100 text-warning-800'"
          >
            {{ isComplete ? t('host.questionEditor.complete') : t('host.questionEditor.incomplete') }}
          </span>
        </div>
        <div class="flex gap-2 mt-2 flex-wrap">
          <span
            v-for="(opt, oi) in question.options"
            :key="oi"
            class="text-xs px-2 py-0.5 rounded"
            :class="oi === question.answerIndex
              ? 'bg-success-100 text-success-800'
              : 'bg-neutral-100 text-neutral-400'"
          >
            {{ ['A', 'B', 'C', 'D'][oi] }}. {{ opt }}
          </span>
        </div>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <UIcon
          v-if="canDelete"
          name="i-lucide-trash-2"
          class="text-neutral-400 hover:text-error-500"
          @click.stop="$emit('delete')"
        />
        <UIcon
          :name="expanded ? 'i-lucide-chevron-up' : 'i-lucide-chevron-down'"
          class="text-neutral-400"
        />
      </div>
    </button>

    <Transition name="expand">
      <div
        v-if="expanded"
        class="border-t border-neutral-200 px-5 pb-5 pt-4"
      >
        <div class="mb-4">
          <label class="text-xs font-semibold mb-1.5 block text-neutral-600">
            {{ t('host.questionEditor.questionLabel') }}
          </label>
          <textarea
            :value="question.text"
            rows="2"
            class="w-full rounded-lg p-3 text-sm resize-none outline-none bg-neutral-100 border-[1.5px] border-neutral-200 focus:border-primary-500"
            :placeholder="t('host.questionEditor.questionPlaceholder')"
            @input="$emit('update:text', ($event.target as HTMLTextAreaElement).value)"
          />
        </div>

        <div class="mb-4">
          <label class="text-xs font-semibold mb-1.5 block text-neutral-600">
            {{ t('host.questionEditor.optionsLabel') }}
          </label>
          <div class="flex flex-col gap-2">
            <div
              v-for="(opt, oi) in question.options"
              :key="oi"
              class="flex items-center gap-3"
            >
              <button
                class="w-6 h-6 rounded-full border-2 shrink-0 flex items-center justify-center transition-all"
                :class="oi === question.answerIndex
                  ? 'border-success-500 bg-success-500'
                  : 'border-neutral-200'"
                @click="$emit('setAnswer', oi)"
              >
                <span
                  v-if="oi === question.answerIndex"
                  class="text-white text-xs font-bold"
                >✓</span>
              </button>
              <span class="w-6 text-xs font-bold shrink-0 text-neutral-400">
                {{ ['A', 'B', 'C', 'D'][oi] }}
              </span>
              <input
                :value="opt"
                :placeholder="t('host.questionEditor.optionPlaceholder')"
                class="flex-1 rounded-lg px-3 py-2 text-sm outline-none bg-neutral-100 border-[1.5px] border-neutral-200 focus:border-primary-500"
                @input="$emit('update:option', oi, ($event.target as HTMLInputElement).value)"
              >
            </div>
          </div>
        </div>

        <div>
          <label class="text-xs font-semibold mb-1.5 block text-neutral-600">
            {{ t('host.questionEditor.explanationLabel') }}
          </label>
          <textarea
            :value="question.explanation"
            rows="2"
            class="w-full rounded-lg p-3 text-sm resize-none outline-none bg-neutral-100 border-[1.5px] border-neutral-200 focus:border-primary-500"
            :placeholder="t('host.questionEditor.explanationPlaceholder')"
            @input="$emit('update:explanation', ($event.target as HTMLTextAreaElement).value)"
          />
        </div>
      </div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import type { Question } from '~/types/quiz'

interface Props {
  question: Question
  index: number
  expanded: boolean
  canDelete?: boolean
}

const props = defineProps<Props>()
defineEmits<{
  'toggle': []
  'delete': []
  'setAnswer': [optionIndex: number]
  'update:text': [value: string]
  'update:option': [optionIndex: number, value: string]
  'update:explanation': [value: string]
}>()

const { t } = useI18n()

const isComplete = computed(() => !!props.question.text.trim() && props.question.options.every(o => o.trim()))
</script>

<style scoped lang="scss">
.expand-enter-active,
.expand-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
