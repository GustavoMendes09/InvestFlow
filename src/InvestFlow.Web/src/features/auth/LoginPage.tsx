import { useState, type FormEvent } from 'react'
import { ArrowRight } from 'lucide-react'
import { Field } from '../../shared/components/Field'
import { Notice } from '../../shared/components/Notice'
import { getErrorMessage } from '../../shared/hooks/useQuery'
import { useI18n } from '../../shared/i18n/i18n'
import { AuthLayout } from './AuthLayout'
import { authApi, type LoginCredentials } from './authApi'

interface LoginPageProps {
  registrationComplete: boolean
  onSuccess: () => void
  onCreateAccount: () => void
}

export function LoginPage({
  registrationComplete,
  onSuccess,
  onCreateAccount,
}: LoginPageProps) {
  const { t } = useI18n()
  const [error, setError] = useState<string | null>(null)
  const [isPending, setIsPending] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setIsPending(true)

    const form = new FormData(event.currentTarget)
    const credentials: LoginCredentials = {
      login: String(form.get('login')).trim(),
      password: String(form.get('password')),
    }

    try {
      await authApi.login(credentials)
      onSuccess()
    } catch (caughtError) {
      setError(getErrorMessage(caughtError))
    } finally {
      setIsPending(false)
    }
  }

  return (
    <AuthLayout
      eyebrow={t('auth.welcomeBack')}
      title={t('auth.signInTitle')}
      description={t('auth.signInDescription')}
    >
      <form onSubmit={handleSubmit} className="mt-8 space-y-5">
        <Field label={t('auth.usernameOrEmail')}>
          <input
            className="field"
            type="text"
            name="login"
            autoComplete="username"
            placeholder="admin or you@example.com"
            required
          />
        </Field>
        <Field label={t('auth.password')}>
          <input
            className="field"
            type="password"
            name="password"
            minLength={3}
            autoComplete="current-password"
            placeholder={t('auth.yourPassword')}
            required
          />
        </Field>
        {registrationComplete && (
          <Notice message={t('auth.accountCreated')} tone="success" />
        )}
        {error && <Notice message={error} />}
        <button disabled={isPending} className="btn-primary w-full">
          {isPending ? t('auth.signingIn') : t('auth.signIn')}
          <ArrowRight size={16} />
        </button>
      </form>
      <p className="mt-7 text-center text-sm text-[#69736e]">
        {t('auth.newToInvestFlow')}{' '}
        <button
          type="button"
          onClick={onCreateAccount}
          className="font-semibold text-[#216c4d] hover:underline"
        >
          {t('auth.createAccount')}
        </button>
      </p>
    </AuthLayout>
  )
}
