import z from 'zod'

export const useAdminLoginSchema = () => {
  const { t } = useI18n()

  return z.object({
    email: z.email({
      pattern: z.regexes.html5Email,
      error: t('adminLogin.validation.emailInvalid')
    }),
    password: z.string().min(1, t('adminLogin.validation.passwordRequired'))
  })
}

export type AdminLoginSchema = z.output<ReturnType<typeof useAdminLoginSchema>>
