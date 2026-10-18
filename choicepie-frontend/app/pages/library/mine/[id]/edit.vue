<template>
  <div
    v-if="quiz"
    class="max-w-3xl mx-auto px-6 py-8"
  >
    <BackLink
      to="/library?tab=mine"
      :label="t('myQuizzesEdit.backToList')"
      class="mb-4"
    />

    <h1 class="text-2xl font-extrabold mb-6 text-cp-text-primary">
      {{ t('myQuizzesEdit.title') }}
    </h1>

    <QuizMetaForm
      v-model:title="metaTitle"
      v-model:description="metaDescription"
      v-model:tags="metaTagsInput"
      class="mb-6"
      :saving="isSavingMeta"
      @save="handleSaveMeta"
    />

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

    <AddQuestionButton
      :label="t('myQuizzesEdit.addQuestion')"
      :disabled="isAddingQuestion"
      @add="handleAddQuestion"
    />
  </div>

  <!-- Loading -->
  <LoadingSpinner
    v-else
  />
</template>

<script setup lang="ts">
import BackLink from '~/components/common/BackLink.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import AddQuestionButton from '~/components/library/editor/AddQuestionButton.vue'
import QuizMetaForm from '~/components/library/editor/QuizMetaForm.vue'
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import CoverImagePicker from '~/components/library/CoverImagePicker.vue'
import { createBlankQuestion } from '~/composables/useQuestionDrafts'
import { useQuizStore } from '~/stores/quiz'
import type { QuizCoverInput } from '~/stores/quiz'
import type { Question } from '~/types/quiz'
import { createDefaultCover } from '~/utils/quizCover'
import { formatTags, parseTags } from '~/utils/tags'

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
const cover = ref<QuizCoverInput>(createDefaultCover())
const isSavingCover = ref(false)
const isAddingQuestion = ref(false)
const savingQuestionId = ref<string | null>(null)
const editingIndex = ref<number | null>(null)

/** 本地編輯草稿，避免每次按鍵都打 API；按「儲存本題」才送出 */
const drafts = ref<Question[]>([])

watch(quiz, (value) => {
  if (!value) return
  metaTitle.value = value.title
  metaDescription.value = value.description ?? ''
  metaTagsInput.value = formatTags(value.tags)
  cover.value = { coverImageUrl: value.coverImageUrl, coverEmoji: value.coverEmoji, coverGradient: value.coverGradient }
  drafts.value = value.questions.map(q => ({ ...q, options: [...q.options] }))
}, { immediate: true })

const handleSaveMeta = async () => {
  isSavingMeta.value = true
  try {
    await quizStore.updateQuiz(quizId, {
      title: metaTitle.value.trim(),
      description: metaDescription.value.trim() || null,
      tags: parseTags(metaTagsInput.value)
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
    const { id: _draftId, ...blank } = createBlankQuestion()
    await quizStore.addQuestion(quizId, blank)
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
