export default defineNuxtPlugin(() => {
  const auth = useAuthStore()

  // 不 await：只是背景把 header 顯示的會員資料同步成最新，不應該讓整個 app 的
  // hydration/互動性卡在這個請求上（曾發生後端連線變慢時，漢堡選單等按鈕點了 15 秒都沒反應）。
  if (auth.isLoggedIn) {
    auth.fetchMe()
  }
})
