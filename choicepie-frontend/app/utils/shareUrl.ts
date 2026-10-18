export type SocialShareChannel = 'line' | 'facebook' | 'x'

/** 題庫分享連結，ref 標記分享管道以便統計來源 */
export const buildQuizShareUrl = (origin: string, quizId: string, channel: string) =>
  `${origin}/library/${quizId}?ref=${channel}`

/** 各社群平台的分享頁網址 */
export const buildSocialShareUrl = (channel: SocialShareChannel, targetUrl: string, title: string) => {
  const url = encodeURIComponent(targetUrl)
  switch (channel) {
    case 'line':
      return `https://social-plugins.line.me/lineit/share?url=${url}`
    case 'facebook':
      return `https://www.facebook.com/sharer/sharer.php?u=${url}`
    case 'x':
      return `https://twitter.com/intent/tweet?text=${encodeURIComponent(title)}&url=${url}`
  }
}
