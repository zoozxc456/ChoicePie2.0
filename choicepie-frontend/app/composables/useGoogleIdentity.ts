interface GoogleCredentialResponse {
  credential: string
}

interface GoogleAccountsId {
  initialize: (config: { client_id: string, callback: (response: GoogleCredentialResponse) => void }) => void
  prompt: (momentListener?: (notification: unknown) => void) => void
}

declare global {
  interface Window {
    google?: { accounts: { id: GoogleAccountsId } }
  }
}

const GOOGLE_GSI_SCRIPT_SRC = 'https://accounts.google.com/gsi/client'

let scriptLoadPromise: Promise<void> | null = null

const loadGoogleScript = () => {
  if (scriptLoadPromise) return scriptLoadPromise

  scriptLoadPromise = new Promise<void>((resolve, reject) => {
    if (window.google?.accounts?.id) {
      resolve()
      return
    }

    const script = document.createElement('script')
    script.src = GOOGLE_GSI_SCRIPT_SRC
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Failed to load Google Identity Services script'))
    document.head.appendChild(script)
  })

  return scriptLoadPromise
}

/** 包裝 Google Identity Services，取得已簽章的 ID Token 交給後端驗證 */
export const useGoogleIdentity = () => {
  const config = useRuntimeConfig()

  const requestIdToken = async (): Promise<string> => {
    await loadGoogleScript()

    const clientId = config.public.googleClientId as string
    if (!clientId) {
      throw new Error('Google Client ID is not configured')
    }

    return new Promise<string>((resolve, reject) => {
      window.google!.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => {
          if (response.credential) {
            resolve(response.credential)
          } else {
            reject(new Error('Google login was cancelled or failed'))
          }
        }
      })

      window.google!.accounts.id.prompt((notification) => {
        const skipped = notification as { isNotDisplayed?: () => boolean, isSkippedMoment?: () => boolean }
        if (skipped.isNotDisplayed?.() || skipped.isSkippedMoment?.()) {
          reject(new Error('Google login was cancelled or failed'))
        }
      })
    })
  }

  return { requestIdToken }
}
