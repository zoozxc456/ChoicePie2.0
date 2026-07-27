<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <NuxtLink
      to="/library/new"
      class="text-[13px] text-neutral-600 mb-4 inline-block"
    >
      ← {{ t('hostNew.backToChoice') }}
    </NuxtLink>

    <h1 class="text-2xl font-extrabold mb-6">
      {{ t('hostNew.ai.title') }}
    </h1>

    <!-- ── Quota locked ── -->
    <div
      v-if="step === 'input' && !quizStore.canUseAiToday"
      class="bg-white border border-neutral-200 rounded-2xl px-7 py-10 flex flex-col items-center text-center gap-3"
    >
      <div class="text-4xl">
        ⏳
      </div>
      <p class="text-lg font-bold">
        {{ t('hostNew.ai.locked.title') }}
      </p>
      <p class="text-[13px] text-neutral-600 leading-relaxed whitespace-pre-line">
        {{ t('hostNew.ai.locked.desc') }}
      </p>
      <NuxtLink
        to="/library/new/manual"
        class="mt-2 h-12 px-6 rounded-2xl bg-primary-500 text-white font-bold text-sm flex items-center whitespace-nowrap"
      >
        {{ t('hostNew.ai.locked.switchToManual') }}
      </NuxtLink>
    </div>

    <!-- ── Input + settings ── -->
    <template v-else-if="step === 'input'">
      <div class="bg-white border border-neutral-200 rounded-2xl p-5 flex flex-col gap-2 mb-5">
        <textarea
          v-model="content"
          rows="7"
          class="w-full rounded-xl p-3.5 text-sm resize-none outline-none bg-neutral-100 border-[1.5px] transition-colors"
          :class="content ? 'border-primary-500' : 'border-neutral-200'"
          :placeholder="t('hostNew.ai.contentPlaceholder')"
        />
        <div class="flex items-center justify-between">
          <span class="text-xs text-neutral-400">
            <template v-if="isUrlInput">{{ t('hostNew.ai.urlHint') }}</template>
            <template v-else>{{ t('hostNew.ai.charCount', { count: content.length }) }}</template>
          </span>
          <span
            v-if="content.length > 0 && content.length < 30"
            class="text-xs text-warning-500"
          >
            {{ t('hostNew.ai.minCharsHint') }}
          </span>
        </div>
      </div>

      <div class="bg-white border border-neutral-200 rounded-2xl p-5 grid grid-cols-2 gap-5 mb-5">
        <div>
          <p class="text-[13px] font-bold mb-2.5">
            {{ t('hostNew.ai.questionCountLabel') }}
          </p>
          <div class="flex gap-2">
            <button
              v-for="n in questionCountOptions"
              :key="n"
              class="flex-1 text-center py-2.5 rounded-full text-sm font-bold"
              :class="questionCount === n ? 'bg-primary-500 text-white' : 'bg-neutral-100 text-neutral-600'"
              @click="questionCount = n"
            >
              {{ t('hostNew.ai.questionCountUnit', { count: n }) }}
            </button>
          </div>
        </div>
        <div>
          <p class="text-[13px] font-bold mb-2.5">
            {{ t('hostNew.ai.difficultyLabel') }}
          </p>
          <div class="flex gap-2">
            <button
              v-for="d in difficultyOptions"
              :key="d"
              class="flex-1 text-center py-2.5 rounded-full text-[13px] font-bold"
              :class="difficulty === d ? 'bg-secondary-800 text-white' : 'bg-neutral-100 text-neutral-600'"
              @click="difficulty = d"
            >
              {{ t(`hostNew.ai.difficulty.${d}`) }}
            </button>
          </div>
        </div>
      </div>

      <UButton
        block
        size="xl"
        color="primary"
        :loading="quizStore.isGenerating"
        :disabled="!canGenerate"
        @click="handleGenerate"
      >
        {{ quizStore.isGenerating ? t('hostNew.ai.generatingBtn') : t('hostNew.ai.generateBtn') }}
      </UButton>

      <p
        v-if="quizStore.error"
        class="text-sm mt-3 text-center text-error-500"
      >
        {{ quizStore.error }}
      </p>
    </template>

    <!-- ── Preview + edit ── -->
    <template v-else>
      <div class="flex flex-col gap-3 mb-6">
        <HostQuestionEditor
          v-for="(q, qi) in questions"
          :key="qi"
          :question="q"
          :index="qi"
          :expanded="editingIndex === qi"
          @toggle="editingIndex = editingIndex === qi ? null : qi"
          @set-answer="(oi) => setCorrectAnswer(qi, oi)"
          @update:text="(val) => updateQuestion(qi, 'text', val)"
          @update:option="(oi, val) => updateOption(qi, oi, val)"
          @update:explanation="(val) => updateQuestion(qi, 'explanation', val)"
        />
      </div>

      <div class="bg-white border border-neutral-200 rounded-2xl p-6 flex items-center justify-between gap-5 flex-wrap mb-4">
        <div>
          <p class="font-semibold text-sm mb-0.5">
            {{ t('hostNew.ai.readyTitle') }}
          </p>
          <p class="text-[13px] text-neutral-600">
            {{ t('hostNew.ai.readySubtitle') }}
          </p>
        </div>
        <p class="text-4xl font-extrabold text-primary-500">
          {{ t('hostNew.ai.questionCount', { count: questions.length }) }}
        </p>
      </div>

      <UButton
        block
        size="xl"
        color="primary"
        @click="isPreviewOpen = true"
      >
        {{ t('hostNew.ai.createRoomBtn') }}
      </UButton>

      <HostPreviewQuizModal
        :open="isPreviewOpen"
        :questions="questions"
        :loading="isCreatingRoom"
        @confirm="handleSaveQuiz"
        @cancel="isPreviewOpen = false"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import HostPreviewQuizModal from '~/components/host/PreviewQuizModal.vue'
import { useQuizStore } from '~/stores/quiz'
import type { Difficulty, Question } from '~/types/quiz'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()

const content = ref('')
const questionCount = ref<3 | 5 | 10>(5)
const difficulty = ref<Difficulty>('intermediate')

const questionCountOptions: (3 | 5 | 10)[] = [3, 5, 10]
const difficultyOptions: Difficulty[] = ['beginner', 'intermediate', 'expert']

const step = ref<'input' | 'preview'>('input')
const questions = ref<Question[]>([])
const editingIndex = ref<number | null>(null)
const isCreatingRoom = ref(false)
const isPreviewOpen = ref(false)

const isUrlInput = computed(() => {
  const v = content.value.trim()
  return v.startsWith('http://') || v.startsWith('https://')
})

const canGenerate = computed(() => content.value.trim().length >= 30 && quizStore.canUseAiToday)

const handleGenerate = async () => {
  try {
    await quizStore.generateQuestions(content.value, questionCount.value, difficulty.value)
    questions.value = quizStore.generatedQuestions.map(q => ({ ...q }))
    step.value = 'preview'
  } catch {
    // error 由 store 管理
  }
}

const updateQuestion = (index: number, field: keyof Question, value: string | string[] | number) => {
  questions.value[index] = { ...questions.value[index], [field]: value } as Question
}

const updateOption = (qIndex: number, optIndex: number, value: string) => {
  const opts = [...(questions.value[qIndex]?.options ?? [])]
  opts[optIndex] = value
  updateQuestion(qIndex, 'options', opts)
}

const setCorrectAnswer = (qIndex: number, optIndex: number) => {
  updateQuestion(qIndex, 'answerIndex', optIndex)
}

const handleSaveQuiz = async () => {
  isCreatingRoom.value = true
  try {
    const title = content.value.trim().slice(0, 30)
    const quiz = await quizStore.saveQuiz(questions.value, title, difficulty.value)
    await navigateTo(`/library/${quiz.id}`)
  } catch {
    isCreatingRoom.value = false
  }
}
</script>

<script lang="ts">
export default {
  name: 'HostNewAiPage'
}
</script>

<style scoped lang="scss"></style>
