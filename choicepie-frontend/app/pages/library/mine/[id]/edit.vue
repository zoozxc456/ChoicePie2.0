<template>
  <div
    v-if="quiz"
    class="max-w-3xl mx-auto px-6 py-8"
  >
    <NuxtLink
      to="/library?tab=mine"
      class="text-[13px] text-cp-text-secondary mb-4 inline-block"
    >
      ← {{ t('myQuizzesEdit.backToList') }}
    </NuxtLink>

    <h1 class="text-2xl font-extrabold mb-6 text-cp-text-primary">
      {{ t('myQuizzesEdit.title') }}
    </h1>

    <!-- Metadata -->
    <div class="bg-cp-surface rounded-2xl border border-cp-border p-5 flex flex-col gap-4 mb-6">
      <div class="flex flex-col gap-2">
        <label class="text-[13px] font-bold text-cp-text-primary">
          {{ t('myQuizzesEdit.titleLabel') }}
        </label>
        <UInput
          v-model="metaTitle"
          size="lg"
          class="w-full"
          :ui="{ base: 'h-11 text-sm px-3.5' }"
        />
      </div>
      <div class="flex flex-col gap-2">
        <label class="text-[13px] font-bold text-cp-text-primary">
          {{ t('myQuizzesEdit.descriptionLabel') }}
        </label>
        <textarea
          v-model="metaDescription"
          rows="3"
          class="w-full rounded-lg p-3 text-sm resize-none outline-none bg-cp-surface-muted border-[1.5px] border-transparent focus:border-cp-primary"
          :placeholder="t('myQuizzesEdit.descriptionPlaceholder')"
        />
      </div>
      <div class="flex flex-col gap-2">
        <label class="text-[13px] font-bold text-cp-text-primary">
          {{ t('myQuizzesEdit.tagsLabel') }}
        </label>
        <UInput
          v-model="metaTagsInput"
          size="lg"
          class="w-full"
          :placeholder="t('myQuizzesEdit.tagsPlaceholder')"
          :ui="{ base: 'h-11 text-sm px-3.5' }"
        />
      </div>
      <UButton
        class="self-start rounded-full font-bold"
        color="primary"
        :loading="isSavingMeta"
        @click="handleSaveMeta"
      >
        {{ t('myQuizzesEdit.saveMeta') }}
      </UButton>
    </div>

    <!-- Cover -->
    <div class="mb-6 flex flex-col gap-3">
      <CoverImagePicker v-model="cover" />
      <UButton
        class="self-start rounded-full font-bold"
        color="primary"
        :loading="isSavingCover"
        @click="handleSaveCover"
      >
        {{ t('coverPicker.saveCover') }}
      </UButton>
    </div>

    <!-- Questions -->
    <h2 class="text-lg font-bold mb-3 text-cp-text-primary">
      {{ t('myQuizzesEdit.questionsTitle') }}
    </h2>

    <div class="flex flex-col gap-3 mb-4">
      <div
        v-for="(q, qi) in drafts"
        :key="q.id"
      >
        <HostQuestionEditor
          :question="q"
          :index="qi"
          :expanded="editingIndex === qi"
          :can-delete="drafts.length > 1"
          @toggle="editingIndex = editingIndex === qi ? null : qi"
          @delete="handleDeleteQuestion(q.id)"
          @set-answer="(oi) => { q.answerIndex = oi }"
          @update:text="(val) => { q.text = val }"
          @update:option="(oi, val) => { q.options[oi] = val }"
          @update:explanation="(val) => { q.explanation = val }"
        />
        <div
          v-if="editingIndex === qi"
          class="flex justify-end mt-2"
        >
          <UButton
            size="sm"
            color="primary"
            class="rounded-full font-semibold"
            :loading="savingQuestionId === q.id"
            @click="handleSaveQuestion(q)"
          >
            {{ t('myQuizzesEdit.saveQuestion') }}
          </UButton>
        </div>
      </div>
    </div>

    <button
      class="w-full h-12 rounded-2xl border-[1.5px] border-dashed border-cp-border hover:bg-cp-surface-muted text-cp-text-secondary font-semibold text-sm"
      :disabled="isAddingQuestion"
      @click="handleAddQuestion"
    >
      ＋ {{ t('myQuizzesEdit.addQuestion') }}
    </button>
  </div>

  <!-- Loading -->
  <div
    v-else
    class="flex justify-center py-20"
  >
    <UIcon
      name="i-lucide-loader-2"
      class="animate-spin text-4xl text-cp-primary"
    />
  </div>
</template>

<script setup lang="ts">
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import CoverImagePicker from '~/components/library/CoverImagePicker.vue'
import { useQuizStore } from '~/stores/quiz'
import type { QuizCoverInput } from '~/stores/quiz'
import type { Question } from '~/types/quiz'

definePageMeta({ layout: 'default', middleware: ['auth'] })

const { t } = useI18n()
const route = useRoute()
const quizStore = useQuizStore()

const quizId = route.params.id as string
const quiz = computed(() => quizStore.currentQuiz)

const metaTitle = ref('')
const metaDescription = ref('')
const metaTagsInput = ref('')
const isSavingMeta = ref(false)
const cover = ref<QuizCoverInput>({ coverImageUrl: null, coverEmoji: '📝', coverGradient: 'primary' })
const isSavingCover = ref(false)
const isAddingQuestion = ref(false)
const savingQuestionId = ref<string | null>(null)
const editingIndex = ref<number | null>(null)

/** 本地編輯草稿，避免每次按鍵都打 API；按「儲存本題」才送出 */
const drafts = ref<Question[]>([])

const blankQuestion = (): Omit<Question, 'id'> => ({
  text: '',
  options: ['', '', '', ''],
  answerIndex: 0,
  explanation: ''
})

watch(quiz, (value) => {
  if (!value) return
  metaTitle.value = value.title
  metaDescription.value = value.description ?? ''
  metaTagsInput.value = value.tags.join(', ')
  cover.value = { coverImageUrl: value.coverImageUrl, coverEmoji: value.coverEmoji, coverGradient: value.coverGradient }
  drafts.value = value.questions.map(q => ({ ...q, options: [...q.options] }))
}, { immediate: true })

const handleSaveMeta = async () => {
  isSavingMeta.value = true
  try {
    const tags = metaTagsInput.value.split(',').map(tag => tag.trim()).filter(Boolean)
    await quizStore.updateQuiz(quizId, {
      title: metaTitle.value.trim(),
      description: metaDescription.value.trim() || null,
      tags
    })
  } finally {
    isSavingMeta.value = false
  }
}

const handleSaveCover = async () => {
  isSavingCover.value = true
  try {
    await quizStore.updateQuizCover(quizId, cover.value)
  } finally {
    isSavingCover.value = false
  }
}

const handleAddQuestion = async () => {
  isAddingQuestion.value = true
  try {
    await quizStore.addQuestion(quizId, blankQuestion())
    editingIndex.value = drafts.value.length - 1
  } finally {
    isAddingQuestion.value = false
  }
}

const handleSaveQuestion = async (question: Question) => {
  savingQuestionId.value = question.id
  try {
    await quizStore.updateQuestion(quizId, question.id, {
      text: question.text,
      options: question.options,
      answerIndex: question.answerIndex,
      explanation: question.explanation
    })
  } finally {
    savingQuestionId.value = null
  }
}

const handleDeleteQuestion = async (questionId: string) => {
  await quizStore.removeQuestion(quizId, questionId)
  editingIndex.value = null
}

await quizStore.fetchQuizById(quizId)
</script>

<script lang="ts">
export default {
  name: 'MyQuizEditPage'
}
</script>

<style scoped lang="scss"></style>
