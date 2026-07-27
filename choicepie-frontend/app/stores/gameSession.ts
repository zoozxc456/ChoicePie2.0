import { defineStore } from 'pinia'
import { useGameSessionClientApi } from '~/services/gameSession/client'
import type { GameSessionSummaryDto, GameSessionDetailDto } from '~/types/api'

export const useGameSessionStore = defineStore('gameSession', () => {
  const gameSessionApi = useGameSessionClientApi()

  const hostedSessions = ref<GameSessionSummaryDto[]>([])
  const playedSessions = ref<GameSessionSummaryDto[]>([])
  const currentSession = ref<GameSessionDetailDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const fetchHostedSessions = async () => {
    isLoading.value = true
    error.value = null
    try {
      const data = await gameSessionApi.fetchHostedSessions()
      hostedSessions.value = data.items
    } catch (e) {
      error.value = '無法載入主持紀錄'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const fetchPlayedSessions = async () => {
    isLoading.value = true
    error.value = null
    try {
      const data = await gameSessionApi.fetchPlayedSessions()
      playedSessions.value = data.items
    } catch (e) {
      error.value = '無法載入遊玩紀錄'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const fetchSessionById = async (id: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await gameSessionApi.fetchSessionById(id)
      currentSession.value = data
      return data
    } catch (e) {
      error.value = '無法載入這場遊戲紀錄'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  return {
    hostedSessions, playedSessions, currentSession, isLoading, error,
    fetchHostedSessions, fetchPlayedSessions, fetchSessionById
  }
})
