<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <BackLink
      to="/library/new"
      :label="t('hostNew.backToChoice')"
      class="mb-4"
    />

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

    <CoverImagePicker
      v-model="cover"
      class="mb-5"
    />

    <div class="flex flex-col gap-3 mb-4">
      <HostQuestionEditor
        v-for="(q, qi) in questions"
        :key="q.id"
        :question="q"
        :index="qi"
        :expanded="editingIndex === qi"
        :can-delete="questions.length > 1"
        @toggle="toggleEditing(qi)"
        @delete="removeQuestion(qi)"
        @set-answer="(oi) => setCorrectAnswer(qi, oi)"
        @update:text="(val) => updateText(qi, val)"
        @update:option="(oi, val) => updateOption(qi, oi, val)"
        @update:explanation="(val) => updateExplanation(qi, val)"
      />
    </div>

    <AddQuestionButton
      class="mb-6"
      :label="t('hostNew.manual.addQuestion')"
      @add="addQuestion"
    />

    <ReadySummaryCard
      class="mb-4"
      :title="t('hostNew.manual.readyTitle')"
      :subtitle="t('hostNew.manual.readySubtitle')"
      :count-label="t('hostNew.manual.questionCount', { count: validQuestions.length })"
    />

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
import BackLink from '~/components/common/BackLink.vue'
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import HostPreviewQuizModal from '~/components/host/PreviewQuizModal.vue'
import CoverImagePicker from '~/components/library/CoverImagePicker.vue'
import AddQuestionButton from '~/components/library/editor/AddQuestionButton.vue'
import ReadySummaryCard from '~/components/library/editor/ReadySummaryCard.vue'
import { createBlankQuestion, useQuestionDrafts } from '~/composables/useQuestionDrafts'
import { useQuizStore } from '~/stores/quiz'
import type { QuizCoverInput } from '~/stores/quiz'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()

const title = ref('')
const titleTouched = ref(false)
const cover = ref<QuizCoverInput>({ coverImageUrl: null, coverEmoji: '📝', coverGradient: 'primary' })
const isCreatingRoom = ref(false)
const isPreviewOpen = ref(false)

const {
  questions,
  editingIndex,
  validQuestions,
  addQuestion,
  removeQuestion,
  toggleEditing,
  updateText,
  updateExplanation,
  updateOption,
  setCorrectAnswer
} = useQuestionDrafts([createBlankQuestion()])
editingIndex.value = 0

const canOpenPreview = computed(() => !!title.value.trim() && validQuestions.value.length > 0)

const openPreview = () => {
  titleTouched.value = true
  if (!canOpenPreview.value) return
  isPreviewOpen.value = true
}

const handleSaveQuiz = async () => {
  isCreatingRoom.value = true
  try {
    const quiz = await quizStore.saveQuiz(validQuestions.value, title.value.trim(), 'intermediate', cover.value)
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
