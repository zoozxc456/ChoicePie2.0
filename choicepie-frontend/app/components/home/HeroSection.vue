<template>
  <div
    class="w-screen h-screen bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)] flex items-center justify-center gap-20"
  >
    <div class="space-y-7 p-6">
      <h1 class="text-5xl font-extrabold max-w-180 text-white text-center mx-auto leading-[1.2]">
        AI 三秒出題，讓每場聚會都變成搶答派對
      </h1>
      <p class="text-[#FFFFFFB3] text-2xl text-center leading-[1.6]">
        貼上一段文字或一個網址，ChoicePie 自動生成互動問答遊戲，人人掃碼即可加入
      </p>
      <div class="flex justify-center items-center">
        <div class="ml-4 flex items-center gap-2 bg-[#FFFFFF1A] rounded-full px-4 py-2">
          <UInput
            v-model="gameCode"
            placeholder="輸入遊戲代碼"
            class="w-48"
            :ui="{ base: 'bg-transparent text-white placeholder-[#FFFFFF80] border-none! shadow-none! ring-0! outline-none!' }"
            @keyup.enter="joinGame"
          />

          <UButton
            color="primary"
            icon="i-lucide-arrow-right"
            size="md"
            class="ml-2 rounded-full"
            @click="joinGame"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
const gameCode = ref('')
const joinGame = () => {
  if (gameCode.value.trim() === '') {
    alert('請輸入遊戲代碼')
    return
  }
  navigateTo(`/join/${gameCode.value}`)
}
</script>

<style scoped lang="scss"></style>
