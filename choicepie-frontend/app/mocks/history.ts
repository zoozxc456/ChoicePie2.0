import type { HostedGameSummary } from '~/types/gameRoom'

export const mockHostedGames: HostedGameSummary[] = [
  {
    id: 'h1',
    quizId: 'k8s-basics',
    quizTitle: 'Kubernetes 核心概念入門',
    coverEmoji: '⚓',
    coverGradient: 'background: linear-gradient(135deg,#0f3460,#533483);',
    playerCount: 18,
    questionCount: 10,
    topPlayerName: 'Ivy',
    topPlayerScore: 940,
    playedAt: '2026-07-02T14:30:00.000Z'
  },
  {
    id: 'h2',
    quizId: 'react-hooks',
    quizTitle: 'React Hooks 深入解析',
    coverEmoji: '⚛️',
    coverGradient: 'background: linear-gradient(135deg,#0d0d0d,#2d1b69);',
    playerCount: 24,
    questionCount: 8,
    topPlayerName: 'Leo',
    topPlayerScore: 810,
    playedAt: '2026-06-28T09:15:00.000Z'
  },
  {
    id: 'h3',
    quizId: 'k8s-basics',
    quizTitle: 'Kubernetes 核心概念入門',
    coverEmoji: '⚓',
    coverGradient: 'background: linear-gradient(135deg,#0f3460,#533483);',
    playerCount: 9,
    questionCount: 10,
    topPlayerName: '小美',
    topPlayerScore: 720,
    playedAt: '2026-06-20T18:45:00.000Z'
  }
]
