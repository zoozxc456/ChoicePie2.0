import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const HubConnectionState = {
  Disconnected: 'Disconnected',
  Connected: 'Connected'
}

class HttpError extends Error {
  statusCode: number
  constructor(message: string, statusCode: number) {
    super(message)
    this.statusCode = statusCode
  }
}

const LogLevel = { Warning: 2 }

const hubConnectionMock = vi.hoisted(() => ({
  state: 'Disconnected' as string,
  handlers: new Map<string, (...args: unknown[]) => void>(),
  start: vi.fn(),
  stop: vi.fn(),
  invoke: vi.fn(),
  on: vi.fn(),
  onreconnecting: vi.fn(),
  onreconnected: vi.fn(),
  onclose: vi.fn()
}))

const resetHubConnectionMock = () => {
  hubConnectionMock.state = 'Disconnected'
  hubConnectionMock.handlers.clear()
  hubConnectionMock.start.mockReset().mockResolvedValue(undefined)
  hubConnectionMock.stop.mockReset().mockResolvedValue(undefined)
  hubConnectionMock.invoke.mockReset().mockResolvedValue(undefined)
  hubConnectionMock.on.mockReset().mockImplementation((event: string, handler: (...args: unknown[]) => void) => {
    hubConnectionMock.handlers.set(event, handler)
  })
  hubConnectionMock.onreconnecting.mockReset()
  hubConnectionMock.onreconnected.mockReset()
  hubConnectionMock.onclose.mockReset()
}
resetHubConnectionMock()

const builderMock = vi.hoisted(() => ({
  withUrl: vi.fn(),
  withAutomaticReconnect: vi.fn(),
  configureLogging: vi.fn(),
  build: vi.fn()
}))

const resetBuilderMock = () => {
  builderMock.withUrl.mockReset().mockReturnValue(builderMock)
  builderMock.withAutomaticReconnect.mockReset().mockReturnValue(builderMock)
  builderMock.configureLogging.mockReset().mockReturnValue(builderMock)
  builderMock.build.mockReset().mockReturnValue(hubConnectionMock)
}
resetBuilderMock()

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: vi.fn().mockImplementation(function HubConnectionBuilder(this: unknown) {
    return builderMock
  }),
  HubConnectionState,
  HttpError,
  LogLevel
}))

const useGameStoreMock = vi.hoisted(() => vi.fn())
const useAuthStoreMock = vi.hoisted(() => vi.fn())
const navigateToMock = vi.hoisted(() => vi.fn())

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { apiBaseUrl: 'https://api.example.test' }
}))
mockNuxtImport('useGameStore', () => useGameStoreMock)
mockNuxtImport('useAuthStore', () => useAuthStoreMock)
mockNuxtImport('navigateTo', () => navigateToMock)

export {
  hubConnectionMock,
  resetHubConnectionMock,
  builderMock,
  resetBuilderMock,
  useGameStoreMock,
  useAuthStoreMock,
  navigateToMock,
  HttpError
}
