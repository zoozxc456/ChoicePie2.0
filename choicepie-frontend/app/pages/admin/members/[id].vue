<template>
  <div>
    <NuxtLink
      to="/admin/members"
      class="inline-flex items-center gap-1 text-base text-neutral-500 hover:text-neutral-800 mb-4"
    >
      <UIcon name="i-lucide-arrow-left" />
      {{ t('adminMemberDetail.back') }}
    </NuxtLink>

    <div
      v-if="adminMemberStore.isLoadingDetail"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!member"
      class="text-base text-neutral-400 text-center py-16"
    >
      {{ t('adminMemberDetail.notFound') }}
    </p>

    <div
      v-else
      class="relative grid grid-cols-[1fr_180px] rounded-2xl bg-white border border-neutral-200 shadow-sm overflow-hidden"
    >
      <div class="p-6 min-w-0">
        <div class="flex items-center gap-3 min-w-0">
          <img
            v-if="member.avatar"
            :src="member.avatar"
            class="w-12 h-12 rounded-full object-cover shrink-0"
            alt=""
          >
          <div
            v-else
            class="w-12 h-12 rounded-full bg-neutral-100 flex items-center justify-center text-lg font-bold text-neutral-400 shrink-0"
          >
            {{ member.name.charAt(0) }}
          </div>
          <div class="min-w-0">
            <p class="text-lg font-bold truncate">
              {{ member.name }}
            </p>
            <p class="text-base text-neutral-400 truncate">
              {{ member.email }}
            </p>
          </div>
        </div>

        <dl class="grid grid-cols-2 gap-4 mt-5 pt-5 text-base border-t border-dashed border-neutral-200">
          <div>
            <dt class="text-neutral-400">
              {{ t('adminMemberDetail.createdAt') }}
            </dt>
            <dd class="mt-0.5 font-medium">
              {{ formatDate(member.createdAt) }}
            </dd>
          </div>
          <div>
            <dt class="text-neutral-400">
              {{ t('adminMemberDetail.lastAiGenerationAt') }}
            </dt>
            <dd class="mt-0.5 font-medium">
              {{ member.lastAiGenerationAt ? formatDate(member.lastAiGenerationAt) : t('adminMemberDetail.never') }}
            </dd>
          </div>
        </dl>

        <div class="flex justify-end mt-6">
          <UButton
            v-if="!member.isSuspended"
            size="sm"
            color="error"
            variant="soft"
            @click="isModalOpen = true"
          >
            {{ t('adminMembers.suspendAction') }}
          </UButton>
          <UButton
            v-else
            size="sm"
            color="primary"
            variant="soft"
            :loading="adminMemberStore.isUnsuspending"
            @click="handleUnsuspend"
          >
            {{ t('adminMembers.unsuspendAction') }}
          </UButton>
        </div>
      </div>

      <div
        class="relative border-l border-dashed"
        :class="member.isSuspended ? 'border-error-200' : 'border-neutral-200'"
      >
        <span
          class="absolute -top-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100"
        />
        <span
          class="absolute -bottom-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100"
        />

        <div
          class="h-full flex flex-col items-center justify-center text-center gap-2 px-4 py-5"
          :class="member.isSuspended ? 'bg-error-50' : 'bg-success-50'"
        >
          <div class="flex items-center gap-1.5">
            <UIcon
              :name="member.isSuspended ? 'i-lucide-shield-off' : 'i-lucide-shield-check'"
              class="text-base shrink-0"
              :class="member.isSuspended ? 'text-error-600' : 'text-success-600'"
            />
            <span
              class="text-base font-bold"
              :class="member.isSuspended ? 'text-error-700' : 'text-success-700'"
            >
              {{ member.isSuspended ? t('adminMembers.statusSuspended') : t('adminMembers.statusActive') }}
            </span>
          </div>

          <p
            v-if="!member.isSuspended"
            class="text-sm text-neutral-400"
          >
            {{ t('adminMemberDetail.statusHealthy') }}
          </p>
          <p
            v-else
            class="text-sm text-error-600"
          >
            {{ member.suspendedUntil
              ? t('adminMembers.suspendedUntil', { date: formatDate(member.suspendedUntil) })
              : t('adminMembers.suspendedPermanently') }}
          </p>

          <p
            v-if="member.isSuspended"
            class="w-full mt-1 pt-2.5 border-t border-dashed border-error-200 text-xs leading-relaxed text-error-600"
          >
            {{ member.suspendedReason }}
          </p>
        </div>
      </div>
    </div>

    <div
      v-if="member"
      class="grid grid-cols-1 lg:grid-cols-2 gap-4 mt-4"
    >
      <!-- 出題題庫 -->
      <div class="rounded-2xl bg-white border border-neutral-200 p-5">
        <p class="text-lg font-bold mb-3">
          {{ t('adminMemberDetail.quizzesTitle') }}
        </p>
        <div
          v-if="adminMemberStore.isLoadingQuizzes"
          class="flex justify-center py-8"
        >
          <UIcon
            name="i-lucide-loader-2"
            class="animate-spin text-2xl text-primary-500"
          />
        </div>
        <p
          v-else-if="!quizzes.length"
          class="text-base text-neutral-400 text-center py-8"
        >
          {{ t('adminMemberDetail.quizzesEmpty') }}
        </p>
        <ul
          v-else
          class="flex flex-col divide-y divide-neutral-100"
        >
          <li
            v-for="quiz in quizzes"
            :key="quiz.id"
          >
            <NuxtLink
              :to="`/admin/quizzes/${quiz.id}`"
              class="py-3 flex items-center justify-between gap-3 group"
            >
              <span class="text-base font-medium truncate group-hover:text-primary-500 group-hover:underline">
                {{ quiz.title }}
              </span>
              <span
                class="text-[11px] px-2 py-0.5 rounded-full font-semibold whitespace-nowrap shrink-0"
                :class="quizStatusBadgeClass(quiz.status)"
              >
                {{ quizStatusLabel(quiz.status) }}
              </span>
            </NuxtLink>
          </li>
        </ul>
      </div>

      <!-- Host 歷史 -->
      <div class="rounded-2xl bg-white border border-neutral-200 p-5">
        <p class="text-lg font-bold mb-3">
          {{ t('adminMemberDetail.hostedSessionsTitle') }}
        </p>
        <div
          v-if="adminMemberStore.isLoadingHostedSessions"
          class="flex justify-center py-8"
        >
          <UIcon
            name="i-lucide-loader-2"
            class="animate-spin text-2xl text-primary-500"
          />
        </div>
        <p
          v-else-if="!hostedSessions.length"
          class="text-base text-neutral-400 text-center py-8"
        >
          {{ t('adminMemberDetail.hostedSessionsEmpty') }}
        </p>
        <ul
          v-else
          class="flex flex-col divide-y divide-neutral-100"
        >
          <li
            v-for="session in hostedSessions"
            :key="session.id"
            class="py-3 flex items-center justify-between gap-3"
          >
            <span class="text-base font-medium truncate">
              {{ session.quizTitle }}
            </span>
            <span class="text-xs text-neutral-400 shrink-0">
              {{ formatDate(session.playedAtUtc) }}
            </span>
          </li>
        </ul>
      </div>

      <!-- 遊玩歷史 -->
      <div class="rounded-2xl bg-white border border-neutral-200 p-5">
        <p class="text-lg font-bold mb-3">
          {{ t('adminMemberDetail.playedSessionsTitle') }}
        </p>
        <div
          v-if="adminMemberStore.isLoadingPlayedSessions"
          class="flex justify-center py-8"
        >
          <UIcon
            name="i-lucide-loader-2"
            class="animate-spin text-2xl text-primary-500"
          />
        </div>
        <p
          v-else-if="!playedSessions.length"
          class="text-base text-neutral-400 text-center py-8"
        >
          {{ t('adminMemberDetail.playedSessionsEmpty') }}
        </p>
        <template v-else>
          <p class="text-xs text-neutral-400 mb-2">
            {{ t('adminMemberDetail.playedSessionsCount', { count: playedSessions.length }) }}
          </p>
          <ul class="flex flex-col divide-y divide-neutral-100">
            <li
              v-for="session in playedSessions"
              :key="session.id"
              class="py-3 flex items-center justify-between gap-3"
            >
              <span class="text-base font-medium truncate">
                {{ session.quizTitle }}
              </span>
              <span class="text-xs text-neutral-400 shrink-0">
                {{ formatDate(session.playedAtUtc) }}
              </span>
            </li>
          </ul>
        </template>
      </div>

      <!-- 留言 -->
      <div class="rounded-2xl bg-white border border-neutral-200 p-5">
        <p class="text-lg font-bold mb-3">
          {{ t('adminMemberDetail.commentsTitle') }}
        </p>
        <div
          v-if="adminMemberStore.isLoadingComments"
          class="flex justify-center py-8"
        >
          <UIcon
            name="i-lucide-loader-2"
            class="animate-spin text-2xl text-primary-500"
          />
        </div>
        <p
          v-else-if="!comments.length"
          class="text-base text-neutral-400 text-center py-8"
        >
          {{ t('adminMemberDetail.commentsEmpty') }}
        </p>
        <ul
          v-else
          class="flex flex-col divide-y divide-neutral-100"
        >
          <li
            v-for="comment in comments"
            :key="comment.id"
            class="py-3"
          >
            <div class="flex items-baseline gap-2">
              <p class="text-base font-medium truncate">
                {{ comment.quizTitle }}
              </p>
              <p class="text-xs text-neutral-400 shrink-0">
                {{ formatDate(comment.createdAt) }}
              </p>
            </div>
            <p class="text-base text-neutral-600 mt-0.5 whitespace-pre-wrap">
              {{ comment.text }}
            </p>
          </li>
        </ul>
      </div>
    </div>

    <AdminSuspendMemberModal
      :open="isModalOpen"
      :is-submitting="adminMemberStore.isSuspending"
      @confirm="handleSuspend"
      @cancel="isModalOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t, locale } = useI18n()
const route = useRoute()
const adminMemberStore = useAdminMemberStore()

const memberId = route.params.id as string
const isModalOpen = ref(false)

const member = computed(() => adminMemberStore.currentMember)
const quizzes = computed(() => adminMemberStore.memberQuizzes?.items ?? [])
const hostedSessions = computed(() => adminMemberStore.memberHostedSessions?.items ?? [])
const playedSessions = computed(() => adminMemberStore.memberPlayedSessions?.items ?? [])
const comments = computed(() => adminMemberStore.memberComments?.items ?? [])

await Promise.all([
  adminMemberStore.fetchMemberById(memberId),
  adminMemberStore.fetchMemberQuizzes(memberId),
  adminMemberStore.fetchMemberHostedSessions(memberId),
  adminMemberStore.fetchMemberPlayedSessions(memberId),
  adminMemberStore.fetchMemberComments(memberId)
])

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)

const quizStatusLabel = (status: string) => ({
  draft: t('adminQuizzes.status.draft'),
  published: t('adminQuizzes.status.published'),
  archived: t('adminQuizzes.status.archived'),
  deleted: t('adminQuizzes.status.deleted'),
  takendown: t('adminQuizzes.status.takendown')
}[status] ?? status)

const quizStatusBadgeClass = (status: string) => ({
  draft: 'bg-neutral-100 text-neutral-600',
  published: 'bg-success-100 text-success-800',
  archived: 'bg-warning-100 text-warning-800',
  deleted: 'bg-neutral-100 text-neutral-600',
  takendown: 'bg-error-100 text-error-800'
}[status] ?? 'bg-neutral-100 text-neutral-600')

const handleSuspend = async (reason: string, until: string | null) => {
  try {
    await adminMemberStore.suspendMember(memberId, reason, until)
    isModalOpen.value = false
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}

const handleUnsuspend = async () => {
  try {
    await adminMemberStore.unsuspendMember(memberId)
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminMemberDetailPage'
}
</script>

<style scoped lang="scss"></style>
