import z from 'zod'

export const useLoginSchema = () => {
  const { t } = useI18n()

  return z.object({
    email: z.email({
      pattern: z.regexes.html5Email,
      error: t('login.validation.emailInvalid')
    }),
    password: z.string().min(6, t('login.validation.passwordMin'))
  })
}

export type LoginSchema = z.output<ReturnType<typeof useLoginSchema>>

export const useRegisterSchema = () => {
  const { t } = useI18n()

  return z
    .object({
      email: z.email({
        pattern: z.regexes.html5Email,
        error: t('signIn.validation.emailInvalid')
      }),
      password: z.string().min(1, t('signIn.validation.passwordMin')),
      confirmPassword: z.string(),
      name: z.string().min(2, t('signIn.validation.nameMin'))
    })
    .refine(data => data.password === data.confirmPassword, {
      message: t('signIn.validation.confirmPasswordMatch'),
      path: ['confirmPassword']
    })
}

export type RegisterSchema = z.output<ReturnType<typeof useRegisterSchema>>

export const useForgotPasswordSchema = () => {
  const { t } = useI18n()

  return z.object({
    email: z.email({
      pattern: z.regexes.html5Email,
      error: t('forgotPassword.validation.emailInvalid')
    })
  })
}

export type ForgotPasswordSchema = z.output<ReturnType<typeof useForgotPasswordSchema>>

export const useResetPasswordSchema = () => {
  const { t } = useI18n()

  return z
    .object({
      password: z.string().min(8, t('resetPassword.validation.passwordMin')),
      confirmPassword: z.string()
    })
    .refine(data => data.password === data.confirmPassword, {
      message: t('resetPassword.validation.confirmPasswordMatch'),
      path: ['confirmPassword']
    })
}

export type ResetPasswordSchema = z.output<ReturnType<typeof useResetPasswordSchema>>
