import type { HostedGameSummary, PlayedGameSummary } from '~/types/gameRoom'

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

export const mockPlayedGames: PlayedGameSummary[] = [
  {
    id: 'p1',
    quizTitle: 'AWS 雲端架構挑戰',
    coverEmoji: '☁️',
    coverGradient: 'background: linear-gradient(135deg,#1a2a6c,#2d3748);',
    playerCount: 32,
    questionCount: 12,
    myRank: 1,
    myScore: 1180,
    playedAt: '2026-07-04T20:00:00.000Z'
  },
  {
    id: 'p2',
    quizTitle: 'SQL 資料庫入門',
    coverEmoji: '🗄️',
    coverGradient: 'background: linear-gradient(135deg,#0f3460,#16213e);',
    playerCount: 15,
    questionCount: 10,
    myRank: 3,
    myScore: 640,
    playedAt: '2026-06-30T13:20:00.000Z'
  },
  {
    id: 'p3',
    quizTitle: 'System Design 面試題庫',
    coverEmoji: '🏗️',
    coverGradient: 'background: linear-gradient(135deg,#2d1b69,#0d0d0d);',
    playerCount: 20,
    questionCount: 15,
    myRank: 7,
    myScore: 410,
    playedAt: '2026-06-22T11:10:00.000Z'
  }
]
