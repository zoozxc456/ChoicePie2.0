export default defineNuxtPlugin((nuxtApp) => {
  const auth = useAuthStore()

  // 這一輪是 SSR 產生後拿來 hydrate 的（payload.serverRendered），代表 fetchMe()
  // 剛剛已經在伺服器端做完了，這裡不用再打一次，否則 access token 快到期時會
  // 在瀏覽器端多觸發一次非必要的 /refresh。只有純 client-side 啟動（例如背景分頁
  // 恢復、SPA fallback）才需要在這裡自己驗證一次。
  if (nuxtApp.payload.serverRendered) {
    return
  }

  // 不 await：只是背景把 header 顯示的會員資料同步成最新，不應該讓整個 app 的
  // hydration/互動性卡在這個請求上（曾發生後端連線變慢時，漢堡選單等按鈕點了 15 秒都沒反應）。
  if (auth.isLoggedIn) {
    auth.fetchMe()
  }
})
