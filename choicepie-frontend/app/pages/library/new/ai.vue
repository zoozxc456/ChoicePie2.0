<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <BackLink
      to="/library/new"
      :label="t('hostNew.backToChoice')"
      class="mb-4"
    />

    <h1 class="text-2xl font-extrabold mb-6">
      {{ t('hostNew.ai.title') }}
    </h1>

    <AiQuotaLocked v-if="step === 'input' && !quizStore.canUseAiToday" />

    <!-- ── Input + settings ── -->
    <template v-else-if="step === 'input'">
      <AiSourceInput
        v-model="content"
        class="mb-5"
      />

      <div class="bg-white border border-neutral-200 rounded-2xl p-5 grid grid-cols-2 gap-5 mb-5">
        <div>
          <p class="text-[13px] font-bold mb-2.5">
            {{ t('hostNew.ai.questionCountLabel') }}
          </p>
          <ChipPicker
            v-model="questionCount"
            :options="questionCountOptions"
          />
        </div>
        <div>
          <p class="text-[13px] font-bold mb-2.5">
            {{ t('hostNew.ai.difficultyLabel') }}
          </p>
          <ChipPicker
            v-model="difficulty"
            :options="difficultyOptions"
            tone="secondary"
          />
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
          @toggle="toggleEditing(qi)"
          @set-answer="(oi) => setCorrectAnswer(qi, oi)"
          @update:text="(val) => updateText(qi, val)"
          @update:option="(oi, val) => updateOption(qi, oi, val)"
          @update:explanation="(val) => updateExplanation(qi, val)"
        />
      </div>

      <CoverImagePicker
        v-model="cover"
        class="mb-4"
      />

      <ReadySummaryCard
        class="mb-4"
        :title="t('hostNew.ai.readyTitle')"
        :subtitle="t('hostNew.ai.readySubtitle')"
        :count-label="t('hostNew.ai.questionCount', { count: questions.length })"
      />

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
import BackLink from '~/components/common/BackLink.vue'
import ChipPicker from '~/components/common/ChipPicker.vue'
import HostQuestionEditor from '~/components/host/QuestionEditor.vue'
import HostPreviewQuizModal from '~/components/host/PreviewQuizModal.vue'
import CoverImagePicker from '~/components/library/CoverImagePicker.vue'
import AiQuotaLocked from '~/components/library/editor/AiQuotaLocked.vue'
import AiSourceInput from '~/components/library/editor/AiSourceInput.vue'
import ReadySummaryCard from '~/components/library/editor/ReadySummaryCard.vue'
import { useQuestionDrafts } from '~/composables/useQuestionDrafts'
import { useQuizStore } from '~/stores/quiz'
import type { QuizCoverInput } from '~/stores/quiz'
import type { Difficulty } from '~/types/quiz'
import { hasEnoughSource } from '~/utils/aiSource'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { t } = useI18n()
const quizStore = useQuizStore()

type QuestionCount = 3 | 5 | 10

const content = ref('')
const questionCount = ref<QuestionCount>(5)
const difficulty = ref<Difficulty>('intermediate')

const questionCountOptions = computed(() => ([3, 5, 10] as QuestionCount[])
  .map(n => ({ value: n, label: t('hostNew.ai.questionCountUnit', { count: n }) })))
const difficultyOptions = computed(() => (['beginner', 'intermediate', 'expert'] as Difficulty[])
  .map(d => ({ value: d, label: t(`hostNew.ai.difficulty.${d}`) })))

const step = ref<'input' | 'preview'>('input')
const cover = ref<QuizCoverInput>({ coverImageUrl: null, coverEmoji: '📝', coverGradient: 'primary' })
const isCreatingRoom = ref(false)
const isPreviewOpen = ref(false)

const {
  questions,
  editingIndex,
  replaceAll,
  toggleEditing,
  updateText,
  updateExplanation,
  updateOption,
  setCorrectAnswer
} = useQuestionDrafts()

const canGenerate = computed(() => hasEnoughSource(content.value) && quizStore.canUseAiToday)

const handleGenerate = async () => {
  try {
    await quizStore.generateQuestions(content.value, questionCount.value, difficulty.value)
    replaceAll(quizStore.generatedQuestions)
    step.value = 'preview'
  } catch {
    // error 由 store 管理
  }
}

const handleSaveQuiz = async () => {
  isCreatingRoom.value = true
  try {
    const title = content.value.trim().slice(0, 30)
    const quiz = await quizStore.saveQuiz(questions.value, title, difficulty.value, cover.value)
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
