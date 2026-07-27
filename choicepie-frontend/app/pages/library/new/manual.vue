<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <NuxtLink
      to="/library/new"
      class="text-[13px] text-neutral-600 mb-4 inline-block"
    >
      ← {{ t('hostNew.backToChoice') }}
    </NuxtLink>

    <h1 class="text-2xl font-extrabold mb-6">
      {{ t('hostNew.manual.title') }}
    </h1>

    <div class="bg-white border border-neutral-200 rounded-2xl p-5 flex flex-col gap-2 mb-5">
      <label class="text-[13px] font-bold">
        {{ t('hostNew.manual.titleLabel') }}
      </label>
      <UInput
        v-model="title"
        size="lg"
        :placeholder="t('hostNew.manual.titlePlaceholder')"
        class="w-full"
        :ui="{ base: 'h-11 text-sm px-3.5' }"
      />
      <p
        v-if="titleTouched && !title.trim()"
        class="text-xs text-error-500"
      >
        {{ t('hostNew.manual.titleRequired') }}
      </p>
    </div>

    <div class="flex flex-col gap-3 mb-4">
      <HostQuestionEditor
        v-for="(q, qi) in questions"
        :key="qi"
        :question="q"
        :index="qi"
        :expanded="editingIndex === qi"
        :can-delete="questions.length > 1"
        @toggle="editingIndex = editingIndex === qi ? null : qi"
        @delete="removeQuestion(qi)"
        @set-answer="(oi) => setCorrectAnswer(qi, oi)"
        @update:text="(val) => updateQuestion(qi, 'text', val)"
        @update:option="(oi, val) => updateOption(qi, oi, val)"
        @update:explanation="(val) => updateQuestion(qi, 'explanation', val)"
      />
    </div>

    <button
      class="w-full h-12 rounded-2xl border-[1.5px] border-dashed border-neutral-200 hover:bg-neutral-100 text-neutral-600 font-semibold text-sm mb-6"
      @click="addQuestion"
    >
      ＋ {{ t('hostNew.manual.addQuestion') }}
    </button>

    <div class="bg-white border border-neutral-200 rounded-2xl p-6 flex items-center justify-between gap-5 flex-wrap mb-4">
      <div>
        <p class="font-semibold text-sm mb-0.5">
          {{ t('hostNew.manual.readyTitle') }}
        </p>
        <p class="text-[13px] text-neutral-600">
          {{ t('hostNew.manual.readySubtitle') }}
        </p>
      </div>
      <p class="text-4xl font-extrabold text-primary-500">
        {{ t('hostNew.manual.questionCount', { count: validQuestionCount }) }}
      </p>
    </div>

    <UButton
      block
      size="xl"
      color="primary"
      :disabled="titleTouched && !canOpenPreview"
      @click="openPreview"
    >
      {{ t('hostNew.manual.createRoomBtn') }}
    </UButton>

    <HostPreviewQuizModal
      :open="isPreviewOpen"
      :questions="validQuestions"
      :quiz-title="title"
      :loading="isCreatingRoom"
      @confirm="handleSaveQuiz"
      @cancel="isPreviewOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import HostPreviewQuizModal from '~/components/host/PreviewQuizModal.vue'
import { useQuizStore } from '~/stores/quiz'
import type { Question } from '~/types/quiz'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()

const blankQuestion = (): Question => ({
  id: `manual-q-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
  text: '',
  options: ['', '', '', ''],
  answerIndex: 0,
  explanation: ''
})

const title = ref('')
const titleTouched = ref(false)
const questions = ref<Question[]>([blankQuestion()])
const editingIndex = ref<number | null>(0)
const isCreatingRoom = ref(false)
const isPreviewOpen = ref(false)

const addQuestion = () => {
  questions.value.push(blankQuestion())
  editingIndex.value = questions.value.length - 1
}

const removeQuestion = (index: number) => {
  questions.value.splice(index, 1)
  if (editingIndex.value === index) editingIndex.value = null
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

const validQuestions = computed(() =>
  questions.value.filter(q => q.text.trim() && q.options.every(o => o.trim()))
)
const validQuestionCount = computed(() => validQuestions.value.length)
const canOpenPreview = computed(() => !!title.value.trim() && validQuestionCount.value > 0)

const openPreview = () => {
  titleTouched.value = true
  if (!canOpenPreview.value) return
  isPreviewOpen.value = true
}

const handleSaveQuiz = async () => {
  isCreatingRoom.value = true
  try {
    const quiz = await quizStore.saveQuiz(validQuestions.value, title.value.trim(), 'intermediate')
    await navigateTo(`/library/${quiz.id}`)
  } catch {
    isCreatingRoom.value = false
  }
}
</script>

<script lang="ts">
export default {
  name: 'HostNewManualPage'
}
</script>

<style scoped lang="scss"></style>
