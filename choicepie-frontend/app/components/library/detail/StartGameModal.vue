<template>
  <Transition name="fade">
    <div
      v-if="open"
      data-testid="modal-backdrop"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      @click.self="emit('cancel')"
    >
      <div
        role="dialog"
        aria-modal="true"
        class="rounded-2xl bg-cp-surface w-full max-w-md mx-4 overflow-hidden shadow-cp-xl"
      >
        <div class="px-6 pt-6 pb-4">
          <h2 class="text-xl font-bold mb-1 text-cp-text-primary">
            {{ t('libraryDetail.modal.title') }}
          </h2>
          <p class="text-sm text-cp-text-secondary">
            {{ t('libraryDetail.modal.subtitle') }}
          </p>
        </div>

        <div class="px-6 pb-6">
          <div
            data-testid="quiz-summary"
            class="flex gap-4 items-center p-4 rounded-xl mb-5 bg-cp-surface-muted"
          >
            <QuizCoverThumbnail
              :cover-image-url="quiz.coverImageUrl"
              :cover-emoji="quiz.coverEmoji"
              :cover-gradient="quiz.coverGradient"
              size="sm"
            />
            <div>
              <p class="font-semibold text-sm mb-1 text-cp-text-primary">
                {{ quiz.title }}
              </p>
              <p class="text-xs text-cp-text-secondary">
                {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ DIFFICULTY_LABEL[quiz.difficulty] }} · {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
              </p>
            </div>
          </div>

          <p class="text-xs font-semibold mb-3 text-cp-text-muted tracking-wide">
            {{ t('libraryDetail.modal.settings') }}
          </p>
          <div class="flex flex-col gap-2 mb-6">
            <div
              v-for="setting in fixedSettings"
              :key="setting.label"
              class="flex justify-between items-center p-3 rounded-xl bg-cp-surface-muted"
            >
              <div>
                <p class="text-sm font-medium text-cp-text-primary">
                  {{ setting.label }}
                </p>
                <p class="text-xs text-cp-text-muted">
                  {{ setting.value }}
                </p>
              </div>
              <UIcon
                name="i-lucide-check"
                class="text-cp-success text-lg"
              />
            </div>

            <div class="flex justify-between items-center p-3 rounded-xl bg-cp-surface-muted">
              <p class="text-sm font-medium text-cp-text-primary">
                {{ t('libraryDetail.modal.timeLimit') }}
              </p>
              <div class="flex gap-1.5">
                <button
                  v-for="seconds in TIME_LIMIT_OPTIONS"
                  :key="seconds"
                  data-testid="time-limit-option"
                  :data-seconds="seconds"
                  type="button"
                  :aria-pressed="seconds === timeLimit"
                  class="h-8 px-3 rounded-full text-xs font-semibold cursor-pointer transition-colors"
                  :class="seconds === timeLimit
                    ? 'bg-cp-primary text-white'
                    : 'bg-cp-surface text-cp-text-secondary'"
                  @click="timeLimit = seconds"
                >
                  {{ t('libraryDetail.modal.timeLimitSeconds', { seconds }) }}
                </button>
              </div>
            </div>
          </div>

          <UButton
            data-testid="create-room"
            block
            size="lg"
            color="primary"
            :loading="loading"
            @click="emit('create', timeLimit)"
          >
            {{ t('libraryDetail.modal.createRoom') }}
          </UButton>
          <UButton
            data-testid="cancel"
            block
            size="md"
            color="neutral"
            variant="ghost"
            class="mt-2"
            @click="emit('cancel')"
          >
            {{ t('libraryDetail.modal.cancel') }}
          </UButton>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import { DIFFICULTY_LABEL, type Quiz } from '~/types/quiz'

type TimeLimitSeconds = 10 | 20 | 30 | 60

interface Props {
  open: boolean
  quiz: Quiz
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  create: [timeLimit: TimeLimitSeconds]
  cancel: []
}>()

const { t } = useI18n()

const TIME_LIMIT_OPTIONS: readonly TimeLimitSeconds[] = [10, 20, 30, 60]
const timeLimit = ref<TimeLimitSeconds>(20)

const fixedSettings = computed(() => [
  { label: t('libraryDetail.modal.allQuestions'), value: t('libraryDetail.modal.allQuestionsValue') },
  { label: t('libraryDetail.modal.joinMethod'), value: t('libraryDetail.modal.joinMethodValue') }
])
</script>

<script lang="ts">
export default {
  name: 'StartGameModal'
}
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
