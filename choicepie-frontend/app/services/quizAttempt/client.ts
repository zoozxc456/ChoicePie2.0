import type { StartAttemptResultDto, QuizAttemptResultDto, QuizAttemptHistoryItemDto } from '~/types/api'

export const useQuizAttemptClientApi = () => {
  const api = useApi()

  return {
    startAttempt: (quizId: string) =>
      api.post<StartAttemptResultDto>('/api/v1/quiz-attempts', { quizId }),
    submitAnswer: (attemptId: string, questionId: string, selectedOptionIndex: number) =>
      api.post(`/api/v1/quiz-attempts/${attemptId}/answers`, { questionId, selectedOptionIndex }),
    completeAttempt: (attemptId: string) =>
      api.post<QuizAttemptResultDto>(`/api/v1/quiz-attempts/${attemptId}/complete`),
    fetchAttemptById: (attemptId: string) =>
      api.get<QuizAttemptResultDto>(`/api/v1/quiz-attempts/${attemptId}`),
    fetchAttemptHistory: (quizId: string) =>
      api.get<QuizAttemptHistoryItemDto[]>('/api/v1/quiz-attempts/history', { quizId })
  }
}
