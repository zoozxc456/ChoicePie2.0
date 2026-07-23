// 對應後端 ChoicePie.Backend.Shared.Hosting.API.Response.ApiResponse<T>
export interface ApiEnvelope<T> {
  code: string
  status: boolean
  data: T | null
  message: string
}

export interface MemberDto {
  id: string
  email: string
  name: string
  avatar: string | null
  isVerified: boolean
  createdAt: string
}

export interface RegisterMemberRequest {
  email: string
  name: string
  password: string
  confirmPassword: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface QuestionDto {
  id: string
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface CreateQuestionRequestItem {
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface UpdateQuestionRequest {
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface QuizDto {
  id: string
  title: string
  description: string | null
  coverEmoji: string
  coverGradient: string
  difficulty: string
  status: string
  challengeCount: number
  passRate: number
  creatorId: string
  creatorName: string
  creatorAvatar: string | null
  questions: QuestionDto[]
  tags: string[]
  shareCount: number
  createdAt: string
  updatedAt: string
  questionCount?: number
}

export interface QuizSummaryDto {
  id: string
  title: string
  description: string | null
  coverEmoji: string
  coverGradient: string
  difficulty: string
  status: string
  questionCount: number
  challengeCount: number
  passRate: number
  creatorId: string
  creatorName: string
  creatorAvatar: string | null
  tags: string[]
  createdAt: string
  updatedAt: string
}

export interface CreateQuizRequest {
  title: string
  description: string | null
  coverEmoji: string
  coverGradient: string
  difficulty: string
  tags: string[]
  questions: CreateQuestionRequestItem[]
}

export interface UpdateQuizRequest {
  title: string
  description: string | null
  tags: string[]
}

export interface PagedResult<T> {
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages?: number
  hasNextPage?: boolean
  items: T[]
}

export interface ListQuizzesQuery {
  tag?: string
  search?: string
  mine?: boolean
  page?: number
  pageSize?: number
}

export interface AddQuestionRequest {
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface GenerateQuizQuestionsRequest {
  content: string
  questionCount: number
  difficulty: string
}

export interface GeneratedQuestionDto {
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface GenerateQuestionsResultDto {
  questions: GeneratedQuestionDto[]
  tokensUsed: number
}

export interface QuestionForAttemptDto {
  id: string
  text: string
  options: string[]
}

export interface QuizForAttemptDto {
  id: string
  title: string
  description: string | null
  coverEmoji: string
  coverGradient: string
  difficulty: string
  creatorId: string
  creatorName: string
  creatorAvatar: string | null
  questions: QuestionForAttemptDto[]
  tags: string[]
}

export interface StartQuizAttemptRequest {
  quizId: string
}

export interface StartAttemptResultDto {
  attemptId: string
  quiz: QuizForAttemptDto
}

export interface SubmitQuizAttemptAnswerRequest {
  questionId: string
  selectedOptionIndex: number
}

export interface QuizAttemptAnswerResultDto {
  questionId: string
  questionText: string
  selectedOptionIndex: number | null
  correctOptionIndex: number
  isCorrect: boolean
  explanation: string
}

export interface QuizAttemptResultDto {
  id: string
  quizId: string
  quizTitle: string
  memberId: string
  score: number | null
  passed: boolean | null
  startedAt: string
  completedAt: string | null
  answers: QuizAttemptAnswerResultDto[]
}

export interface GameSessionSummaryDto {
  id: string
  roomCode: string
  quizId: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playedAtUtc: string
  playerCount: number
  questionCount: number
  topPlayerName: string
  topPlayerScore: number
  myRank: number | null
  myScore: number | null
}

export interface GameSessionRankEntryDto {
  rank: number
  nickname: string
  score: number
}

export interface GameSessionWrongAnswerDto {
  questionText: string
  options: string[]
  myAnswerIndex: number
  correctAnswerIndex: number
  explanation: string
}

export interface GameSessionOptionStatDto {
  text: string
  pickedCount: number
  isCorrect: boolean
}

export interface GameSessionQuestionBreakdownDto {
  questionText: string
  options: GameSessionOptionStatDto[]
  correctCount: number
  answeredCount: number
}

export interface GameSessionDetailDto {
  id: string
  roomCode: string
  quizId: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playedAtUtc: string
  playerCount: number
  questionCount: number
  isHost: boolean
  rankings: GameSessionRankEntryDto[]
  myRank: number | null
  myScore: number | null
  myWrongAnswers: GameSessionWrongAnswerDto[]
  questionBreakdown: GameSessionQuestionBreakdownDto[]
}

export interface CommentDto {
  id: string
  quizId: string
  userId: string
  userName: string
  userAvatar: string | null
  text: string
  createdAt: string
}

export interface CreatorProfileDto {
  id: string
  name: string
  avatar: string | null
  quizCount: number
  challengeCount: number
  isFollowing: boolean
}

export interface AdminUserDto {
  id: string
  email: string
  name: string
  role: string
  isVerified: boolean
  createdAt: string
}

export interface AdminLoginRequest {
  email: string
  password: string
}
