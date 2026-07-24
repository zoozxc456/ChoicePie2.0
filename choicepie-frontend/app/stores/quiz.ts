import { defineStore } from 'pinia'
import { useQuizClientApi } from '~/services/quiz/client'
import type { Quiz, Question, Difficulty } from '~/types/quiz'
import type { QuizDto, QuizSummaryDto, QuestionDto, CreateQuestionRequestItem, CommentDto } from '~/types/api'

const AI_DAILY_LIMIT = 1

const toQuestion = (dto: QuestionDto): Question => ({
  id: dto.id,
  text: dto.text,
  options: dto.options,
  answerIndex: dto.answerIndex,
  explanation: dto.explanation
})

const toQuiz = (dto: QuizDto): Quiz => ({
  id: dto.id,
  title: dto.title,
  description: dto.description ?? undefined,
  coverEmoji: dto.coverEmoji,
  coverGradient: dto.coverGradient,
  difficulty: dto.difficulty as Difficulty,
  questionCount: dto.questionCount ?? dto.questions.length,
  challengeCount: dto.challengeCount,
  passRate: dto.passRate,
  creatorId: dto.creatorId,
  creatorName: dto.creatorName,
  creatorAvatar: dto.creatorAvatar ?? undefined,
  questions: dto.questions.map(toQuestion),
  tags: dto.tags,
  isPublic: dto.status === 'Published',
  status: dto.status,
  shareCount: dto.shareCount,
  createdAt: dto.createdAt,
  updatedAt: dto.updatedAt
})

const toQuizFromSummary = (dto: QuizSummaryDto): Quiz => ({
  id: dto.id,
  title: dto.title,
  description: dto.description ?? undefined,
  coverEmoji: dto.coverEmoji,
  coverGradient: dto.coverGradient,
  difficulty: dto.difficulty as Difficulty,
  questionCount: dto.questionCount,
  challengeCount: dto.challengeCount,
  passRate: dto.passRate,
  creatorId: dto.creatorId,
  creatorName: dto.creatorName,
  creatorAvatar: dto.creatorAvatar ?? undefined,
  questions: [],
  tags: dto.tags,
  isPublic: dto.status === 'Published',
  status: dto.status,
  createdAt: dto.createdAt,
  updatedAt: dto.updatedAt
})

export const useQuizStore = defineStore('quiz', () => {
  const quizApi = useQuizClientApi()

  const quizzes = ref<Quiz[]>([])
  const currentQuiz = ref<Quiz | null>(null)
  const generatedQuestions = ref<Question[]>([])
  const isGenerating = ref(false)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // ── 收藏 ──
  const isFavorited = ref(false)
  const isTogglingFavorite = ref(false)

  // ── 留言 ──
  const comments = ref<CommentDto[]>([])
  const isLoadingComments = ref(false)
  const isLoadingMoreComments = ref(false)
  const isPostingComment = ref(false)
  const isUpdatingComment = ref(false)
  const isDeletingComment = ref(false)
  const commentsPage = ref(1)
  const hasMoreComments = ref(false)

  // ── 相關題庫 ──
  const relatedQuizzes = ref<Quiz[]>([])
  const isLoadingRelated = ref(false)

  // ── AI 每日額度 ──
  const aiUsedDate = ref<string | null>(null)
  const aiUsedCount = ref(0)

  const todayStr = () => new Date().toISOString().slice(0, 10)

  const aiUsesToday = computed(() => aiUsedDate.value === todayStr() ? aiUsedCount.value : 0)
  const canUseAiToday = computed(() => aiUsesToday.value < AI_DAILY_LIMIT)

  // ── Library ──

  const fetchQuizzes = async (params?: { tag?: string, search?: string, mine?: boolean }) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await quizApi.fetchQuizzes(params)
      quizzes.value = data.items.map(toQuizFromSummary)
    } catch (e) {
      error.value = '載入題庫失敗，請稍後再試'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const fetchQuizById = async (id: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await quizApi.fetchQuizById(id)
      currentQuiz.value = toQuiz(data)
    } catch (e) {
      error.value = '無法載入此題庫'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const fetchQuizPreview = (id: string) => quizApi.fetchQuizPreview(id)

  const fetchTags = () => quizApi.fetchTags()

  // ── 題庫管理 ──

  const updateQuiz = async (id: string, payload: { title: string, description: string | null, tags: string[] }) => {
    const data = await quizApi.updateQuiz(id, payload)
    const quiz = toQuiz(data)
    if (currentQuiz.value?.id === id) currentQuiz.value = quiz
    const index = quizzes.value.findIndex(q => q.id === id)
    if (index !== -1) quizzes.value[index] = quiz
    return quiz
  }

  const deleteQuiz = async (id: string) => {
    await quizApi.deleteQuiz(id)
    quizzes.value = quizzes.value.filter(q => q.id !== id)
    if (currentQuiz.value?.id === id) currentQuiz.value = null
  }

  const addQuestion = async (quizId: string, question: Omit<Question, 'id'>) => {
    const data = await quizApi.addQuestion(quizId, question)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  const updateQuestion = async (quizId: string, questionId: string, question: Omit<Question, 'id'>) => {
    const data = await quizApi.updateQuestion(quizId, questionId, question)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  const removeQuestion = async (quizId: string, questionId: string) => {
    const data = await quizApi.removeQuestion(quizId, questionId)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  const publishQuiz = async (id: string) => {
    const data = await quizApi.publishQuiz(id)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  const unpublishQuiz = async (id: string) => {
    const data = await quizApi.unpublishQuiz(id)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  const archiveQuiz = async (id: string) => {
    const data = await quizApi.archiveQuiz(id)
    currentQuiz.value = toQuiz(data)
    return currentQuiz.value
  }

  // ── 收藏 ──

  const fetchFavoriteStatus = async (id: string) => {
    try {
      isFavorited.value = await quizApi.fetchFavoriteStatus(id)
    } catch (e) {
      console.error(e)
    }
  }

  const toggleFavorite = async (id: string) => {
    isTogglingFavorite.value = true
    const next = !isFavorited.value
    try {
      if (next) {
        await quizApi.addFavorite(id)
      } else {
        await quizApi.removeFavorite(id)
      }
      isFavorited.value = next
    } catch (e) {
      error.value = '操作失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isTogglingFavorite.value = false
    }
  }

  // ── 留言 ──

  const fetchComments = async (id: string) => {
    isLoadingComments.value = true
    try {
      const result = await quizApi.fetchComments(id, 1)
      comments.value = result.items
      commentsPage.value = 1
      hasMoreComments.value = result.hasNextPage ?? (result.pageNumber * result.pageSize < result.totalCount)
    } catch (e) {
      console.error(e)
    } finally {
      isLoadingComments.value = false
    }
  }

  const fetchMoreComments = async (id: string) => {
    if (isLoadingMoreComments.value || !hasMoreComments.value) return
    isLoadingMoreComments.value = true
    try {
      const nextPage = commentsPage.value + 1
      const result = await quizApi.fetchComments(id, nextPage)
      comments.value = [...comments.value, ...result.items]
      commentsPage.value = nextPage
      hasMoreComments.value = result.hasNextPage ?? (result.pageNumber * result.pageSize < result.totalCount)
    } catch (e) {
      console.error(e)
    } finally {
      isLoadingMoreComments.value = false
    }
  }

  const addComment = async (id: string, text: string) => {
    isPostingComment.value = true
    try {
      const comment = await quizApi.addComment(id, text)
      comments.value = [comment, ...comments.value]
      return comment
    } catch (e) {
      error.value = '留言送出失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isPostingComment.value = false
    }
  }

  const updateComment = async (id: string, commentId: string, text: string) => {
    isUpdatingComment.value = true
    try {
      const updated = await quizApi.updateComment(id, commentId, text)
      comments.value = comments.value.map(c => c.id === commentId ? updated : c)
      return updated
    } catch (e) {
      error.value = '留言更新失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isUpdatingComment.value = false
    }
  }

  const deleteComment = async (id: string, commentId: string) => {
    isDeletingComment.value = true
    try {
      await quizApi.deleteComment(id, commentId)
      comments.value = comments.value.filter(c => c.id !== commentId)
    } catch (e) {
      error.value = '留言刪除失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isDeletingComment.value = false
    }
  }

  // ── 相關題庫 ──

  const fetchRelatedQuizzes = async (id: string) => {
    isLoadingRelated.value = true
    try {
      const data = await quizApi.fetchRelatedQuizzes(id)
      relatedQuizzes.value = data.map(toQuizFromSummary)
    } catch (e) {
      console.error(e)
    } finally {
      isLoadingRelated.value = false
    }
  }

  // ── 分享 ──

  const recordShare = async (id: string) => {
    try {
      const shareCount = await quizApi.recordShare(id)
      if (currentQuiz.value?.id === id) currentQuiz.value = { ...currentQuiz.value, shareCount }
    } catch (e) {
      console.error(e)
    }
  }

  // ── 檢舉 ──

  const isReporting = ref(false)
  const hasReported = ref(false)

  const reportQuiz = async (id: string, reason: string, description?: string) => {
    isReporting.value = true
    try {
      await quizApi.reportQuiz(id, { reason, description })
      hasReported.value = true
    } catch (e) {
      error.value = '檢舉送出失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isReporting.value = false
    }
  }

  // ── AI 出題 ──

  const recordAiUsage = () => {
    const today = todayStr()
    aiUsedCount.value = aiUsedDate.value === today ? aiUsedCount.value + 1 : 1
    aiUsedDate.value = today
  }

  const generateQuestions = async (
    content: string,
    questionCount: 3 | 5 | 10,
    difficulty: Difficulty
  ) => {
    if (!canUseAiToday.value) {
      error.value = 'AI 協助出題每天限用 1 次，今日額度已使用完畢'
      throw new Error(error.value)
    }

    isGenerating.value = true
    error.value = null
    try {
      const data = await quizApi.generateQuestions(content, questionCount, difficulty)
      generatedQuestions.value = data.questions.map((q, i) => ({
        id: `ai-q-${Date.now()}-${i}`,
        text: q.text,
        options: q.options,
        answerIndex: q.answerIndex,
        explanation: q.explanation
      }))
      recordAiUsage()
    } catch (e) {
      error.value = 'AI 出題失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isGenerating.value = false
    }
  }

  const saveQuiz = async (questions: Question[], title: string, difficulty: Difficulty) => {
    const items: CreateQuestionRequestItem[] = questions.map(q => ({
      text: q.text,
      options: q.options,
      answerIndex: q.answerIndex,
      explanation: q.explanation
    }))
    const data = await quizApi.saveQuiz({
      title,
      description: null,
      coverEmoji: '📝',
      coverGradient: 'linear-gradient(135deg,#0f3460,#533483)',
      difficulty,
      tags: [],
      questions: items
    })
    const quiz = toQuiz(data)
    quizzes.value.unshift(quiz)
    return quiz
  }

  // ── Helpers ──

  const setCurrentQuiz = (quiz: Quiz) => {
    currentQuiz.value = quiz
  }

  const updateGeneratedQuestion = (index: number, updated: Partial<Question>) => {
    if (!generatedQuestions.value[index]) return
    generatedQuestions.value[index] = { ...generatedQuestions.value[index], ...updated }
  }

  const clearGenerated = () => {
    generatedQuestions.value = []
  }

  return {
    quizzes, currentQuiz, generatedQuestions,
    isGenerating, isLoading, error,
    aiUsesToday, canUseAiToday,
    isFavorited, isTogglingFavorite,
    comments, isLoadingComments, isLoadingMoreComments, isPostingComment, isUpdatingComment, isDeletingComment,
    hasMoreComments,
    relatedQuizzes, isLoadingRelated,
    fetchQuizzes, fetchQuizById, fetchQuizPreview, fetchTags,
    generateQuestions, saveQuiz,
    updateQuiz, deleteQuiz,
    addQuestion, updateQuestion, removeQuestion,
    publishQuiz, unpublishQuiz, archiveQuiz,
    fetchFavoriteStatus, toggleFavorite,
    fetchComments, fetchMoreComments, addComment, updateComment, deleteComment,
    fetchRelatedQuizzes,
    recordShare,
    isReporting, hasReported, reportQuiz,
    setCurrentQuiz, updateGeneratedQuestion, clearGenerated
  }
}, {
  persist: {
    pick: ['aiUsedDate', 'aiUsedCount']
  }
})
