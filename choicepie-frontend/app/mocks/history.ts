import type { HostedGameSummary, PlayedGameSummary, WrongAnswerReview } from '~/types/gameRoom'
import type { RankEntry } from '~/types/other'

const NAME_POOL = ['Ivy', 'Leo', '小美', 'Alice', 'Bob', 'Ken', 'Mia', 'Noah', 'Zoe', '阿翔', '小華', 'Grace']

/** 依冠軍分數往下遞減，產生一份看起來合理的完整排名 */
const buildRankings = (playerCount: number, topName: string, topScore: number): RankEntry[] => {
  const count = Math.min(playerCount, NAME_POOL.length + 1)
  const names = [topName, ...NAME_POOL.filter(n => n !== topName)].slice(0, count)
  return names.map((nickname, i) => ({
    rank: i + 1,
    nickname,
    score: Math.max(topScore - i * Math.round(topScore / (count + 2)), 10)
  }))
}

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
    playedAt: '2026-07-02T14:30:00.000Z',
    rankings: buildRankings(18, 'Ivy', 940)
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
    playedAt: '2026-06-28T09:15:00.000Z',
    rankings: buildRankings(24, 'Leo', 810)
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
    playedAt: '2026-06-20T18:45:00.000Z',
    rankings: buildRankings(9, '小美', 720)
  }
]

const AWS_WRONG_ANSWERS: WrongAnswerReview[] = [
  {
    questionText: '以下哪一個 AWS 服務最適合用來做無伺服器運算？',
    options: ['EC2', 'Lambda', 'RDS', 'S3'],
    myAnswerIndex: 0,
    correctAnswerIndex: 1,
    explanation: 'Lambda 是 AWS 的無伺服器運算服務，可以在不管理伺服器的情況下執行程式碼；EC2 則是需要自行管理的虛擬機器。'
  }
]

const SQL_WRONG_ANSWERS: WrongAnswerReview[] = [
  {
    questionText: '哪一個 SQL 指令用來移除資料表中符合條件的資料列？',
    options: ['DROP', 'DELETE', 'TRUNCATE', 'REMOVE'],
    myAnswerIndex: 0,
    correctAnswerIndex: 1,
    explanation: 'DELETE 可依條件刪除資料列；DROP 會直接刪除整張資料表結構，TRUNCATE 會清空整張表但保留結構。'
  },
  {
    questionText: 'JOIN 種類中，哪一種會保留左表所有資料，右表無對應則補 NULL？',
    options: ['INNER JOIN', 'LEFT JOIN', 'RIGHT JOIN', 'CROSS JOIN'],
    myAnswerIndex: 3,
    correctAnswerIndex: 1,
    explanation: 'LEFT JOIN 會保留左表的所有資料列，右表沒有對應資料時以 NULL 補齊。'
  },
  {
    questionText: '哪一個關鍵字用來對查詢結果進行去重複？',
    options: ['UNIQUE', 'DISTINCT', 'FILTER', 'GROUP'],
    myAnswerIndex: 0,
    correctAnswerIndex: 1,
    explanation: 'DISTINCT 用於移除查詢結果中重複的資料列。'
  }
]

const SYSTEM_DESIGN_WRONG_ANSWERS: WrongAnswerReview[] = [
  {
    questionText: '當系統需要水平擴展且要避免單點故障時，最適合採用哪種架構？',
    options: ['單體式架構', '負載平衡 + 多台伺服器', '單一資料庫直連', '本機快取'],
    myAnswerIndex: 2,
    correctAnswerIndex: 1,
    explanation: '負載平衡搭配多台伺服器可以將流量分散，任一台伺服器故障時其他伺服器仍可服務請求，避免單點故障。'
  },
  {
    questionText: '哪一種快取策略會在寫入資料庫的同時更新快取？',
    options: ['Cache Aside', 'Write Through', 'Write Back', 'Read Through'],
    myAnswerIndex: 0,
    correctAnswerIndex: 1,
    explanation: 'Write Through 策略會在寫入資料庫的同時同步寫入快取，確保快取與資料庫一致。'
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
    playedAt: '2026-07-04T20:00:00.000Z',
    rankings: buildRankings(32, '你', 1180),
    wrongAnswers: AWS_WRONG_ANSWERS
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
    playedAt: '2026-06-30T13:20:00.000Z',
    rankings: buildRankings(15, 'Alice', 720).map(r =>
      r.rank === 3 ? { ...r, nickname: '你', score: 640 } : r
    ),
    wrongAnswers: SQL_WRONG_ANSWERS
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
    playedAt: '2026-06-22T11:10:00.000Z',
    rankings: buildRankings(20, 'Noah', 890).map(r =>
      r.rank === 7 ? { ...r, nickname: '你', score: 410 } : r
    ),
    wrongAnswers: SYSTEM_DESIGN_WRONG_ANSWERS
  }
]
