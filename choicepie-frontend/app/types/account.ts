export interface AccountDto {
  id: string
  name: string
  email: string
  avatar: string | null
  isVerified: boolean
  hasPassword: boolean
  googleLinked: boolean
}

export interface ProfileDto {
  name: string
  avatar: string | null
}

export interface ChangePasswordRequest {
  /** 已有密碼的帳號必填；純 Google 帳號「設定密碼」時省略 */
  currentPassword?: string
  newPassword: string
}

export interface DeleteAccountRequest {
  /** 有密碼的帳號必填 */
  password?: string
  /** 純 Google 帳號必填，需與帳號 email 相同 */
  confirmEmail?: string
}
