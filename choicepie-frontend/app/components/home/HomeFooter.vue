<template>
  <div class="relative overflow-hidden bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)]">
    <!-- 裝飾層：柔和光暈，增加視覺層次 -->
    <div
      class="pointer-events-none absolute inset-0"
      aria-hidden="true"
    >
      <div class="absolute left-1/2 top-0 h-72 w-[42rem] -translate-x-1/2 -translate-y-1/2 rounded-full bg-primary-500/20 blur-3xl" />
    </div>

    <!-- CTA -->
    <div class="relative px-6 pt-20 pb-14">
      <div class="container mx-auto text-center">
        <p class="text-[32px] font-bold leading-[1.2] text-white">
          {{ auth.isLoggedIn ? '準備好開一場搶答派對了嗎？' : '準備好加入這場搶答派對了嗎？' }}
        </p>
        <p class="mt-2 text-white/60">
          {{ auth.isLoggedIn ? '幾秒內生成題目，馬上開一場屬於你的派對' : '輸入房間碼，不需要註冊帳號即可加入' }}
        </p>

        <UButton
          size="lg"
          variant="outline"
          class="mx-auto mt-6 block w-fit rounded-2xl border-2 border-white/30 px-7 py-4 text-lg font-bold text-white transition-colors hover:border-white hover:bg-white/10"
          @click="primaryAction"
        >
          {{ auth.isLoggedIn ? '✨ 建立遊戲' : '🎉 加入遊戲' }}
        </UButton>
      </div>
    </div>

    <!-- 頁尾連結列 -->
    <div class="relative border-t border-white/10 px-6 py-10">
      <div class="container mx-auto flex flex-col items-center gap-6 sm:flex-row sm:items-start sm:justify-between">
        <div class="flex flex-col items-center gap-1.5 text-center sm:items-start sm:text-left">
          <span class="flex items-center gap-2 text-lg font-bold text-white">🥧 ChoicePie</span>
          <p class="text-sm text-white/50">
            讓每場聚會都變成搶答派對
          </p>
        </div>

        <nav class="flex flex-wrap items-center justify-center gap-x-6 gap-y-2 text-sm text-white/60">
          <NuxtLink
            to="/how-to-use"
            class="hover:text-white"
          >
            如何使用
          </NuxtLink>
          <NuxtLink
            to="/about"
            class="hover:text-white"
          >
            關於我們
          </NuxtLink>
          <NuxtLink
            to="/library"
            class="hover:text-white"
          >
            探索題庫
          </NuxtLink>
          <NuxtLink
            to="/join"
            class="hover:text-white"
          >
            加入遊戲
          </NuxtLink>
        </nav>
      </div>

      <div class="mt-8 flex flex-wrap items-center justify-center gap-x-4 gap-y-1 text-xs text-white/30">
        <span>© {{ currentYear }} ChoicePie. All rights reserved.</span>
        <NuxtLink
          to="/terms"
          class="hover:text-white/60"
        >
          服務條款
        </NuxtLink>
        <NuxtLink
          to="/privacy"
          class="hover:text-white/60"
        >
          隱私權政策
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
const auth = useAuthStore()
const currentYear = new Date().getFullYear()

const primaryAction = async () => {
  await navigateTo(auth.isLoggedIn ? '/library/new' : '/join')
}
</script>

<style scoped lang="scss"></style>
