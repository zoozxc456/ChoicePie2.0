import type { UseFetchOptions } from '#app'

export const useApi = <T>(url: string, query?: UseFetchOptions<T>['query']) => {
  const config = useRuntimeConfig()
  const backendUrl = config.public.backendUrl as string || import.meta.env.NUXT_PUBLIC_BACKEND_URL
  const forwardedHeaders = import.meta.server
    ? useRequestHeaders(['cookie'])
    : undefined

  return useFetch<T>(url, {
    baseURL: backendUrl,
    cache: 'no-cache',
    // 預設 headers 可以在這裡擴充
    headers: {
      ...forwardedHeaders,
      'Cache-Control': 'no-cache'
    },
    query
  })
}
