<template>
  <div v-if="quiz">
    <!-- ── Hero ── -->
    <div
      class="px-10 pt-10 pb-0 flex gap-9 items-end rounded-t-2xl mx-6 mt-6"
      :style="`background: linear-gradient(180deg, #1a1a2e 0%, #16213e 70%, var(--cp-bg) 100%);`"
    >
      <!-- Cover art -->
      <div
        class="w-48 h-48 rounded-xl flex items-center justify-center text-7xl shrink-0"
        :style="`${quiz.coverGradient}; box-shadow: 0 16px 48px rgba(0,0,0,0.5);`"
      >
        {{ quiz.coverEmoji }}
      </div>

      <!-- Title block -->
      <div class="pb-6 flex-1">
        <p
          class="text-xs font-bold mb-2 tracking-wider"
          style="color: rgba(255,255,255,0.6);"
        >
          {{ t('libraryDetail.knowledgePack') }} · {{ DIFFICULTY_LABEL[quiz.difficulty] }}
        </p>
        <h1 class="text-4xl font-black text-white mb-4 leading-tight">
          {{ quiz.title }}
        </h1>
        <div class="flex items-center gap-2 text-sm">
          <div
            class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0"
            style="background: var(--cp-primary);"
          >
            {{ quiz.creatorName[0] }}
          </div>
          <span
            class="font-semibold"
            style="color: rgba(255,255,255,0.9);"
          >{{ quiz.creatorName }}</span>
          <span style="color: rgba(255,255,255,0.3);">·</span>
          <span style="color: rgba(255,255,255,0.6);">{{ t('libraryDetail.questions', { count: quiz.questionCount }) }}</span>
          <span style="color: rgba(255,255,255,0.3);">·</span>
          <span style="color: rgba(255,255,255,0.6);">{{ t('libraryDetail.challenges', { count: quiz.challengeCount.toLocaleString() }) }}</span>
          <span style="color: rgba(255,255,255,0.3);">·</span>
          <span style="color: rgba(255,255,255,0.6);">{{ t('libraryDetail.passRate', { rate: quiz.passRate }) }}</span>
        </div>
      </div>
    </div>

    <!-- ── Action bar ── -->
    <div
      class="flex items-center gap-4 px-10 py-5 mx-6 bg-white rounded-b-2xl mb-7"
      style="border: 1px solid var(--cp-border); border-top: none;"
    >
      <!-- Play button -->
      <button
        class="w-14 h-14 rounded-full flex items-center justify-center text-2xl text-white shrink-0 transition-transform hover:scale-105"
        style="background: var(--cp-primary);"
        @click="isStartModalOpen = true"
      >
        ▶
      </button>

      <button
        class="px-5 py-2 rounded-full text-sm font-semibold transition-colors"
        style="border: 1.5px solid var(--cp-border); color: var(--cp-text-primary);"
        onmouseover="this.style.borderColor='var(--cp-primary)'"
        onmouseout="this.style.borderColor='var(--cp-border)'"
      >
        {{ t('libraryDetail.bookmark') }}
      </button>
      <button
        class="px-5 py-2 rounded-full text-sm font-semibold transition-colors"
        style="border: 1.5px solid var(--cp-border); color: var(--cp-text-primary);"
      >
        {{ t('libraryDetail.share') }}
      </button>
      <button
        class="px-5 py-2 rounded-full text-sm font-semibold text-white transition-transform hover:scale-105"
        style="background: var(--cp-secondary); border: none;"
        @click="isStartModalOpen = true"
      >
        {{ t('libraryDetail.useForGame') }}
      </button>

      <div class="ml-auto flex gap-2 items-center">
        <span
          class="text-xs px-3 py-1 rounded-full font-semibold"
          :style="`background: ${quiz.passRate >= 65 ? 'var(--cp-success-bg)' : quiz.passRate >= 40 ? 'var(--cp-warning-bg)' : 'var(--cp-danger-bg)'}; color: ${passRateColor};`"
        >
          {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
        </span>
        <span
          class="text-xs px-3 py-1 rounded-full font-semibold"
          style="background: var(--cp-primary-light); color: var(--cp-primary);"
        >
          {{ DIFFICULTY_LABEL[quiz.difficulty] }}
        </span>
      </div>
    </div>

    <!-- ── Content ── -->
    <div class="grid grid-cols-[1fr_320px] gap-6 px-6 pb-10">
      <!-- Left: Question list + Comments -->
      <div>
        <!-- Column headers -->
        <div
          class="grid gap-3 px-4 pb-3 text-xs font-semibold tracking-wider"
          style="grid-template-columns: 40px 1fr 80px 80px; color: var(--cp-text-muted); border-bottom: 1px solid var(--cp-border);"
        >
          <span class="text-center">{{ t('libraryDetail.questionTable.index') }}</span>
          <span>{{ t('libraryDetail.questionTable.question') }}</span>
          <span class="text-center">{{ t('libraryDetail.questionTable.passRate') }}</span>
          <span class="text-center">{{ t('libraryDetail.questionTable.timeLimit') }}</span>
        </div>

        <!-- Questions -->
        <div>
          <div
            v-for="(q, i) in quiz.questions"
            :key="q.id"
            class="grid gap-3 px-4 py-4 rounded-xl cursor-pointer transition-colors items-center"
            style="grid-template-columns: 40px 1fr 80px 80px;"
            onmouseover="this.style.background='var(--cp-surface-muted)'"
            onmouseout="this.style.background='transparent'"
          >
            <div
              class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold"
              style="background: var(--cp-primary-light); color: var(--cp-primary);"
            >
              {{ i + 1 }}
            </div>
            <div>
              <p class="text-sm font-medium mb-1">
                {{ q.text }}
              </p>
              <p
                class="text-xs"
                style="color: var(--cp-text-muted);"
              >
                {{ t('libraryDetail.questionTable.singleChoice') }}
              </p>
            </div>
            <div
              class="text-center text-sm font-semibold"
              :style="`color: ${passRateColor};`"
            >
              —
            </div>
            <div
              class="text-center text-xs"
              style="color: var(--cp-text-muted);"
            >
              {{ t('libraryDetail.questionTable.timeApprox') }}
            </div>
          </div>
        </div>

        <!-- Comments -->
        <div
          class="mt-8 pt-6"
          style="border-top: 1px solid var(--cp-border);"
        >
          <h3 class="text-base font-bold mb-5">
            {{ t('libraryDetail.comments.title') }}
          </h3>

          <!-- Comment input -->
          <div class="flex gap-3 mb-6">
            <div
              class="w-9 h-9 rounded-full flex items-center justify-center text-sm font-bold text-white shrink-0"
              style="background: var(--cp-primary);"
            >
              M
            </div>
            <div
              class="flex-1 rounded-full px-4 py-2.5 text-sm cursor-text"
              style="background: var(--cp-surface-muted); border: 1px solid var(--cp-border); color: var(--cp-text-muted);"
            >
              {{ t('libraryDetail.comments.placeholder') }}
            </div>
          </div>

          <!-- Sample comments -->
          <div class="flex flex-col gap-5">
            <div class="flex gap-3">
              <div
                class="w-9 h-9 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0"
                style="background: #4338ca;"
              >
                Y
              </div>
              <div>
                <div
                  class="rounded-tl-none rounded-2xl px-4 py-3 text-sm"
                  style="background: var(--cp-surface-muted);"
                >
                  <p class="font-semibold text-xs mb-1.5">
                    YuHao
                    <span
                      class="font-normal ml-1"
                      style="color: var(--cp-text-muted);"
                    >· {{ t('libraryDetail.comments.daysAgo', { days: 2 }) }}</span>
                  </p>
                  這題庫出得很到位，第 3 題我猜了兩次才搞懂 😂
                </div>
              </div>
            </div>
            <div class="flex gap-3">
              <div
                class="w-9 h-9 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0"
                style="background: #065f46;"
              >
                C
              </div>
              <div>
                <div
                  class="rounded-tl-none rounded-2xl px-4 py-3 text-sm"
                  style="background: var(--cp-surface-muted);"
                >
                  <p class="font-semibold text-xs mb-1.5">
                    ChiaEn
                    <span
                      class="font-normal ml-1"
                      style="color: var(--cp-text-muted);"
                    >· {{ t('libraryDetail.comments.daysAgo', { days: 4 }) }}</span>
                  </p>
                  題庫品質很高！解析說明很清楚，推薦給正在準備 CKA 的朋友 👍
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right: Creator + Related -->
      <div class="flex flex-col gap-4">
        <!-- Creator card -->
        <div
          class="rounded-2xl p-5 bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <p
            class="text-xs font-semibold mb-4"
            style="color: var(--cp-text-muted);"
          >
            {{ t('libraryDetail.creator.title') }}
          </p>
          <div class="flex gap-3 items-center mb-4">
            <div
              class="w-12 h-12 rounded-full flex items-center justify-center text-lg font-bold text-white shrink-0"
              style="background: var(--cp-primary);"
            >
              {{ quiz.creatorName[0] }}
            </div>
            <div>
              <p class="font-bold text-sm mb-0.5">
                {{ quiz.creatorName }} ✓
              </p>
              <p
                class="text-xs"
                style="color: var(--cp-text-muted);"
              >
                {{ t('libraryDetail.creator.verified') }}
              </p>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-3 mb-4">
            <div
              class="rounded-xl p-3 text-center"
              style="background: var(--cp-surface-muted);"
            >
              <p
                class="text-xl font-bold"
                style="color: var(--cp-primary);"
              >
                12
              </p>
              <p
                class="text-xs"
                style="color: var(--cp-text-secondary);"
              >
                {{ t('libraryDetail.creator.quizCount') }}
              </p>
            </div>
            <div
              class="rounded-xl p-3 text-center"
              style="background: var(--cp-surface-muted);"
            >
              <p
                class="text-xl font-bold"
                style="color: var(--cp-primary);"
              >
                1.2k
              </p>
              <p
                class="text-xs"
                style="color: var(--cp-text-secondary);"
              >
                {{ t('libraryDetail.creator.challengeCount') }}
              </p>
            </div>
          </div>
          <button
            class="w-full rounded-full py-2 text-sm font-semibold transition-colors"
            style="border: 1.5px solid var(--cp-primary); color: var(--cp-primary);"
          >
            {{ t('libraryDetail.creator.follow') }}
          </button>
        </div>

        <!-- Related quizzes -->
        <div
          class="rounded-2xl p-5 bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <p
            class="text-xs font-semibold mb-4"
            style="color: var(--cp-text-muted);"
          >
            {{ t('libraryDetail.related') }}
          </p>
          <div class="flex flex-col gap-2">
            <NuxtLink
              v-for="related in relatedQuizzes"
              :key="related.title"
              to="#"
              class="flex items-center gap-3 p-2 rounded-xl transition-colors"
              onmouseover="this.style.background='var(--cp-surface-muted)'"
              onmouseout="this.style.background='transparent'"
            >
              <div
                class="w-12 h-12 rounded-lg flex items-center justify-center text-xl shrink-0"
                :style="related.gradient"
              >
                {{ related.emoji }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold truncate">{{ related.title }}</p>
                <p
                  class="text-xs truncate"
                  style="color: var(--cp-text-muted);"
                >{{ related.meta }}</p>
              </div>
              <span style="color: var(--cp-primary); font-size: 12px;">▶</span>
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Start Game Modal ── -->
    <Transition name="fade">
      <div
        v-if="isStartModalOpen"
        class="fixed inset-0 z-50 flex items-center justify-center"
        style="background: rgba(0,0,0,0.5);"
        @click.self="isStartModalOpen = false"
      >
        <div
          class="rounded-2xl bg-white w-full max-w-md mx-4 overflow-hidden"
          style="box-shadow: var(--cp-shadow-xl);"
        >
          <div class="px-6 pt-6 pb-4">
            <h2 class="text-xl font-bold mb-1">
              {{ t('libraryDetail.modal.title') }}
            </h2>
            <p
              class="text-sm"
              style="color: var(--cp-text-secondary);"
            >
              {{ t('libraryDetail.modal.subtitle') }}
            </p>
          </div>
          <div class="px-6 pb-6">
            <!-- Quiz info -->
            <div
              class="flex gap-4 items-center p-4 rounded-xl mb-5"
              style="background: var(--cp-surface-muted);"
            >
              <div
                class="w-14 h-14 rounded-xl flex items-center justify-center text-2xl shrink-0"
                :style="quiz.coverGradient"
              >
                {{ quiz.coverEmoji }}
              </div>
              <div>
                <p class="font-semibold text-sm mb-1">
                  {{ quiz.title }}
                </p>
                <p
                  class="text-xs"
                  style="color: var(--cp-text-secondary);"
                >
                  {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ DIFFICULTY_LABEL[quiz.difficulty] }} · {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
                </p>
              </div>
            </div>

            <!-- Settings -->
            <p
              class="text-xs font-semibold mb-3"
              style="color: var(--cp-text-muted); letter-spacing: 0.05em;"
            >
              {{ t('libraryDetail.modal.settings') }}
            </p>
            <div class="flex flex-col gap-2 mb-6">
              <div
                v-for="setting in gameSettings"
                :key="setting.label"
                class="flex justify-between items-center p-3 rounded-xl"
                style="border: 1px solid var(--cp-border);"
              >
                <div>
                  <p class="text-sm font-medium">
                    {{ setting.label }}
                  </p>
                  <p
                    class="text-xs"
                    style="color: var(--cp-text-muted);"
                  >
                    {{ setting.value }}
                  </p>
                </div>
                <span style="color: var(--cp-success); font-weight: bold;">✓</span>
              </div>
            </div>

            <UButton
              block
              size="lg"
              color="primary"
              :loading="isCreatingRoom"
              @click="handleCreateRoom"
            >
              {{ t('libraryDetail.modal.createRoom') }}
            </UButton>
            <UButton
              block
              size="md"
              color="neutral"
              variant="ghost"
              class="mt-2"
              @click="isStartModalOpen = false"
            >
              {{ t('libraryDetail.modal.cancel') }}
            </UButton>
          </div>
        </div>
      </div>
    </Transition>
  </div>

  <!-- Loading -->
  <div
    v-else
    class="flex justify-center py-20"
  >
    <UIcon
      name="i-lucide-loader-2"
      class="animate-spin text-4xl"
      style="color: var(--cp-primary);"
    />
  </div>
</template>

<script setup lang="ts">
import { DIFFICULTY_LABEL } from '~/types/quiz'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const route = useRoute()
const quizStore = useQuizStore()
const gameRoom = useGameRoom()

const quiz = computed(() => quizStore.currentQuiz)
const isStartModalOpen = ref(false)
const isCreatingRoom = ref(false)

await quizStore.fetchQuizById(route.params.id as string)

const passRateColor = computed(() => {
  const r = quiz.value?.passRate ?? 0
  if (r >= 65) return 'var(--cp-success)'
  if (r >= 40) return 'var(--cp-warning)'
  return 'var(--cp-danger)'
})

const handleCreateRoom = async () => {
  if (!quiz.value) return
  isCreatingRoom.value = true
  try {
    await gameRoom.createRoom({ quizId: quiz.value.id, questionIds: quiz.value.questions.map(q => q.id) })
  } catch {
    isCreatingRoom.value = false
  }
}

const relatedQuizzes = [
  { emoji: '⚛️', gradient: 'background: linear-gradient(135deg,#0d0d0d,#2d1b69);', title: 'React Hooks 深入解析', meta: 'Alice · 183 人挑戰' },
  { emoji: '☁️', gradient: 'background: linear-gradient(135deg,#001a2e,#00509d);', title: 'AWS 架構設計原則', meta: 'Eva · 24 人挑戰' },
  { emoji: '🗄️', gradient: 'background: linear-gradient(135deg,#0a2e1a,#2d6a4f);', title: 'SQL 效能優化技巧', meta: 'Bob · 97 人挑戰' }
]

const gameSettings = computed(() => [
  { label: t('libraryDetail.modal.allQuestions'), value: t('libraryDetail.modal.allQuestionsValue') },
  { label: t('libraryDetail.modal.timeLimit'), value: t('libraryDetail.modal.timeLimitValue') },
  { label: t('libraryDetail.modal.joinMethod'), value: t('libraryDetail.modal.joinMethodValue') }
])
</script>

<script lang="ts">
export default {
  name: 'LibraryDetailPage'
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
