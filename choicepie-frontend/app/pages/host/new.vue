<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold mb-1">
        {{ t('hostNew.title') }}
      </h1>
      <p style="color: var(--cp-text-secondary); font-size: 14px;">
        {{ t('hostNew.subtitle') }}
      </p>
    </div>

    <!-- ────────────────────── STEP 1: 輸入 ────────────────────── -->
    <template v-if="step === 'input'">
      <!-- Content Input -->
      <div
        class="rounded-2xl p-6 bg-white mb-4"
        style="border: 1px solid var(--cp-border);"
      >
        <label class="text-sm font-semibold mb-2 block">{{ t('hostNew.step1.inputLabel') }}</label>

        <textarea
          v-model="content"
          rows="8"
          class="w-full rounded-xl p-4 text-sm resize-none outline-none transition-colors"
          style="
            background: var(--cp-surface-muted);
            border: 1.5px solid var(--cp-border);
            color: var(--cp-text-primary);
            font-family: inherit;
          "
          :style="content ? 'border-color: var(--cp-primary);' : ''"
          :placeholder="t('hostNew.step1.placeholder')"
          @focus="($event.target as HTMLElement).style.borderColor = 'var(--cp-primary)'"
          @blur="($event.target as HTMLElement).style.borderColor = content ? 'var(--cp-primary)' : 'var(--cp-border)'"
        />

        <div class="flex items-center justify-between mt-2">
          <span
            class="text-xs"
            style="color: var(--cp-text-muted);"
          >
            <template v-if="isUrlInput">{{ t('hostNew.step1.urlHint') }}</template>
            <template v-else>{{ t('hostNew.step1.charCount', { count: content.length }) }}</template>
          </span>
          <span
            v-if="content.length > 0 && content.length < 30"
            class="text-xs"
            style="color: var(--cp-warning);"
          >
            {{ t('hostNew.step1.minCharsHint') }}
          </span>
        </div>
      </div>

      <!-- Settings -->
      <div
        class="rounded-2xl p-6 bg-white mb-6"
        style="border: 1px solid var(--cp-border);"
      >
        <div class="grid grid-cols-2 gap-8">
          <!-- 題數 -->
          <div>
            <label class="text-sm font-semibold mb-3 block">{{ t('hostNew.step1.questionCountLabel') }}</label>
            <div class="flex gap-2">
              <button
                v-for="n in questionCountOptions"
                :key="n"
                class="flex-1 py-2 rounded-lg text-sm font-semibold transition-all"
                :style="questionCount === n
                  ? 'background: var(--cp-primary); color: white; border: 1.5px solid var(--cp-primary);'
                  : 'background: white; color: var(--cp-text-secondary); border: 1.5px solid var(--cp-border);'"
                @click="questionCount = n"
              >
                {{ n }}{{ t('hostNew.step1.questionCountUnit') }}
              </button>
            </div>
          </div>

          <!-- 難易度 -->
          <div>
            <label class="text-sm font-semibold mb-3 block">{{ t('hostNew.step1.difficultyLabel') }}</label>
            <div class="flex gap-2">
              <button
                v-for="d in difficultyOptions"
                :key="d"
                class="flex-1 py-2 rounded-lg text-sm font-semibold transition-all"
                :style="difficulty === d
                  ? 'background: var(--cp-secondary); color: white; border: 1.5px solid var(--cp-secondary);'
                  : 'background: white; color: var(--cp-text-secondary); border: 1.5px solid var(--cp-border);'"
                @click="difficulty = d"
              >
                {{ t(`hostNew.step1.difficulty.${d}`) }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Generate Button -->
      <UButton
        block
        size="xl"
        color="primary"
        :loading="quizStore.isGenerating"
        :disabled="!canGenerate"
        @click="handleGenerate"
      >
        <template v-if="quizStore.isGenerating">
          {{ t('hostNew.step1.generatingBtn') }}
        </template>
        <template v-else>
          {{ t('hostNew.step1.generateBtn', { count: questionCount, difficulty: t(`hostNew.step1.difficulty.${difficulty}`) }) }}
        </template>
      </UButton>

      <p
        v-if="quizStore.error"
        class="text-sm mt-3 text-center"
        style="color: var(--cp-danger);"
      >
        {{ quizStore.error }}
      </p>
    </template>

    <!-- ────────────────────── STEP 2: 預覽 ────────────────────── -->
    <template v-else>
      <!-- Header Actions -->
      <div class="flex items-center justify-between mb-6">
        <button
          class="flex items-center gap-1.5 text-sm font-medium"
          style="color: var(--cp-text-secondary);"
          @click="step = 'input'"
        >
          {{ t('hostNew.step2.back') }}
        </button>
        <span
          class="text-sm"
          style="color: var(--cp-text-muted);"
        >
          {{ t('hostNew.step2.editHint') }}
        </span>
      </div>

      <!-- Question List -->
      <div class="flex flex-col gap-3 mb-6">
        <div
          v-for="(q, qi) in questions"
          :key="qi"
          class="rounded-2xl bg-white overflow-hidden transition-shadow"
          :style="`border: 1.5px solid ${editingIndex === qi ? 'var(--cp-primary)' : 'var(--cp-border)'};`"
        >
          <!-- Question Header -->
          <button
            class="w-full flex items-start gap-4 p-5 text-left"
            @click="startEdit(qi)"
          >
            <span
              class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0"
              style="background: var(--cp-primary-light); color: var(--cp-primary);"
            >
              {{ qi + 1 }}
            </span>
            <div class="flex-1 min-w-0">
              <p class="font-medium text-sm leading-relaxed">
                {{ q.text }}
              </p>
              <div class="flex gap-2 mt-2 flex-wrap">
                <span
                  v-for="(opt, oi) in q.options"
                  :key="oi"
                  class="text-xs px-2 py-0.5 rounded"
                  :style="oi === q.answerIndex
                    ? 'background: var(--cp-success-bg); color: #2e7d32;'
                    : 'background: var(--cp-surface-muted); color: var(--cp-text-muted);'"
                >
                  {{ ['A', 'B', 'C', 'D'][oi] }}. {{ opt }}
                </span>
              </div>
            </div>
            <UIcon
              :name="editingIndex === qi ? 'i-lucide-chevron-up' : 'i-lucide-chevron-down'"
              class="shrink-0 mt-0.5"
              style="color: var(--cp-text-muted);"
            />
          </button>

          <!-- Edit Panel -->
          <Transition name="expand">
            <div
              v-if="editingIndex === qi"
              class="border-t px-5 pb-5 pt-4"
              style="border-color: var(--cp-border);"
            >
              <!-- Question Text -->
              <div class="mb-4">
                <label
                  class="text-xs font-semibold mb-1.5 block"
                  style="color: var(--cp-text-secondary);"
                >{{ t('hostNew.step2.questionLabel') }}</label>
                <textarea
                  :value="q.text"
                  rows="2"
                  class="w-full rounded-lg p-3 text-sm resize-none outline-none"
                  style="background: var(--cp-surface-muted); border: 1.5px solid var(--cp-border); font-family: inherit;"
                  @input="updateQuestion(qi, 'text', ($event.target as HTMLTextAreaElement).value)"
                />
              </div>

              <!-- Options -->
              <div class="mb-4">
                <label
                  class="text-xs font-semibold mb-1.5 block"
                  style="color: var(--cp-text-secondary);"
                >
                  {{ t('hostNew.step2.optionsLabel') }}
                </label>
                <div class="flex flex-col gap-2">
                  <div
                    v-for="(opt, oi) in q.options"
                    :key="oi"
                    class="flex items-center gap-3"
                  >
                    <button
                      class="w-6 h-6 rounded-full border-2 shrink-0 flex items-center justify-center transition-all"
                      :style="oi === q.answerIndex
                        ? 'border-color: var(--cp-success); background: var(--cp-success);'
                        : 'border-color: var(--cp-border);'"
                      @click="setCorrectAnswer(qi, oi)"
                    >
                      <span
                        v-if="oi === q.answerIndex"
                        class="text-white text-xs font-bold"
                      >✓</span>
                    </button>
                    <span
                      class="w-6 text-xs font-bold shrink-0"
                      style="color: var(--cp-text-muted);"
                    >
                      {{ ['A', 'B', 'C', 'D'][oi] }}
                    </span>
                    <input
                      :value="opt"
                      class="flex-1 rounded-lg px-3 py-2 text-sm outline-none"
                      style="background: var(--cp-surface-muted); border: 1.5px solid var(--cp-border); font-family: inherit;"
                      @input="updateOption(qi, oi, ($event.target as HTMLInputElement).value)"
                    >
                  </div>
                </div>
              </div>

              <!-- Explanation -->
              <div>
                <label
                  class="text-xs font-semibold mb-1.5 block"
                  style="color: var(--cp-text-secondary);"
                >{{ t('hostNew.step2.explanationLabel') }}</label>
                <textarea
                  :value="q.explanation"
                  rows="2"
                  class="w-full rounded-lg p-3 text-sm resize-none outline-none"
                  style="background: var(--cp-surface-muted); border: 1.5px solid var(--cp-border); font-family: inherit;"
                  @input="updateQuestion(qi, 'explanation', ($event.target as HTMLTextAreaElement).value)"
                />
              </div>
            </div>
          </Transition>
        </div>
      </div>

      <!-- Create Room Button -->
      <div
        class="rounded-2xl p-5 bg-white mb-4"
        style="border: 1px solid var(--cp-border);"
      >
        <div class="flex items-center justify-between mb-4">
          <div>
            <p class="font-semibold text-sm mb-0.5">
              {{ t('hostNew.step2.readyTitle') }}
            </p>
            <p
              class="text-xs"
              style="color: var(--cp-text-muted);"
            >
              {{ t('hostNew.step2.readySubtitle') }}
            </p>
          </div>
          <div class="text-right">
            <p
              class="text-2xl font-bold"
              style="color: var(--cp-primary);"
            >
              {{ questions.length }}
            </p>
            <p
              class="text-xs"
              style="color: var(--cp-text-muted);"
            >
              {{ t('hostNew.step2.questionCount') }}
            </p>
          </div>
        </div>
        <UButton
          block
          size="lg"
          color="primary"
          :loading="isCreatingRoom"
          @click="handleCreateRoom"
        >
          {{ t('hostNew.step2.createRoomBtn') }}
        </UButton>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { Difficulty, Question } from '~/types/quiz'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()
const gameRoom = useGameRoom()

const content = ref('')
const questionCount = ref<3 | 5 | 10>(5)
const difficulty = ref<Difficulty>('intermediate')

const questionCountOptions: (3 | 5 | 10)[] = [3, 5, 10]
const difficultyOptions: Difficulty[] = ['beginner', 'intermediate', 'expert']

const step = ref<'input' | 'preview'>('input')
const questions = ref<Question[]>([])
const editingIndex = ref<number | null>(null)

const isUrlInput = computed(() => {
  const v = content.value.trim()
  return v.startsWith('http://') || v.startsWith('https://')
})

const canGenerate = computed(() => content.value.trim().length >= 30)

const handleGenerate = async () => {
  try {
    await quizStore.generateQuestions(content.value, questionCount.value, difficulty.value)
    questions.value = quizStore.generatedQuestions.map(q => ({ ...q }))
    step.value = 'preview'
  } catch {
    // error 由 store 管理
  }
}

const startEdit = (index: number) => {
  editingIndex.value = editingIndex.value === index ? null : index
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

const isCreatingRoom = ref(false)

const handleCreateRoom = async () => {
  isCreatingRoom.value = true
  try {
    await gameRoom.createRoom({ questionIds: questions.value.map(q => q.id) })
  } catch {
    isCreatingRoom.value = false
  }
}
</script>

<script lang="ts">
export default {
  name: 'HostNewPage'
}
</script>

<style scoped>
.expand-enter-active,
.expand-leave-active {
  transition: opacity var(--cp-duration-normal), transform var(--cp-duration-normal);
}
.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
