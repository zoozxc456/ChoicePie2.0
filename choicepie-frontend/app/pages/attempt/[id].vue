<template>
  <div class="max-w-2xl mx-auto px-4 py-8">
    <!-- Loading -->
    <div
      v-if="isLoading && !quiz"
      class="flex justify-center py-20"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-4xl text-primary-500"
      />
    </div>

    <template v-else-if="quiz && currentQuestion">
      <!-- Progress -->
      <div class="flex items-center justify-between mb-3 text-sm font-semibold text-neutral-400">
        <span>{{ t('attempt.question.questionOf', { current: questionIndex + 1, total: quiz.questions.length }) }}</span>
      </div>
      <div class="h-1.5 rounded-full bg-neutral-100 mb-6 overflow-hidden">
        <div
          class="h-full rounded-full bg-primary-500 transition-[width] duration-300 ease-linear"
          :style="{ width: `${((questionIndex + 1) / quiz.questions.length) * 100}%` }"
        />
      </div>

      <!-- Question -->
      <p class="text-base font-semibold leading-relaxed mb-5">
        {{ currentQuestion.text }}
      </p>

      <div class="flex flex-col gap-3">
        <QuizOption
          v-for="(option, i) in currentQuestion.options"
          :key="i"
          :letter="OPTION_LETTERS[i] ?? String(i + 1)"
          :text="option"
          :state="selectedOptionIndex === i ? 'selected' : 'default'"
          :disabled="isSubmitting"
          @click="selectOption(i)"
        />
      </div>

      <UButton
        block
        size="lg"
        color="primary"
        class="mt-6 rounded-xl font-bold"
        :loading="isSubmitting"
        :disabled="selectedOptionIndex === null"
        @click="handleNext"
      >
        {{ isLastQuestion ? t('attempt.question.finish') : t('attempt.question.next') }}
      </UButton>
    </template>

    <!-- Result -->
    <template v-else-if="result">
      <AttemptResult :result="result" />
    </template>
  </div>
</template>

<script setup lang="ts">
import QuizOption from '~/components/gameRoom/QuizOption.vue'
import AttemptResult from '~/components/attempt/AttemptResult.vue'

definePageMeta({ layout: 'content', middleware: ['auth'] })

const OPTION_LETTERS = ['A', 'B', 'C', 'D', 'E', 'F']

const { t } = useI18n()
const route = useRoute()
const quizAttemptStore = useQuizAttemptStore()

const attemptId = route.params.id as string
const quiz = computed(() => quizAttemptStore.currentAttempt?.quiz ?? null)
const result = computed(() => quizAttemptStore.result)
const isLoading = computed(() => quizAttemptStore.isLoading)

const questionIndex = ref(0)
const selectedOptionIndex = ref<number | null>(null)
const isSubmitting = ref(false)

const currentQuestion = computed(() => quiz.value?.questions[questionIndex.value] ?? null)
const isLastQuestion = computed(() => !!quiz.value && questionIndex.value === quiz.value.questions.length - 1)

const selectOption = (index: number) => {
  if (isSubmitting.value) return
  selectedOptionIndex.value = index
}

const handleNext = async () => {
  if (!currentQuestion.value || selectedOptionIndex.value === null) return
  isSubmitting.value = true
  try {
    await quizAttemptStore.submitAnswer(attemptId, currentQuestion.value.id, selectedOptionIndex.value)
    if (isLastQuestion.value) {
      await quizAttemptStore.completeAttempt(attemptId)
    } else {
      questionIndex.value++
      selectedOptionIndex.value = null
    }
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  if (!quizAttemptStore.currentAttempt || quizAttemptStore.currentAttempt.attemptId !== attemptId) {
    quizAttemptStore.reset()
  }
})
</script>

<script lang="ts">
export default {
  name: 'AttemptPage'
}
</script>

<style scoped lang="scss"></style>
