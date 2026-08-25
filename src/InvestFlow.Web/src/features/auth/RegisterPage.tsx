import { useState, type FormEvent } from 'react'
import { ArrowRight } from 'lucide-react'
import { Field } from '../../shared/components/Field'
import { Notice } from '../../shared/components/Notice'
import { getErrorMessage } from '../../shared/hooks/useQuery'
import { useI18n } from '../../shared/i18n/i18n'
import { AuthLayout } from './AuthLayout'
import { authApi, type RegistrationCredentials } from './authApi'

interface RegisterPageProps {
  onRegistered: () => void
  onSignIn: () => void
}

export function RegisterPage({ onRegistered, onSignIn }: RegisterPageProps) {
  const { t } = useI18n()
  const [error, setError] = useState<string | null>(null)
  const [isPending, setIsPending] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)

    const form = new FormData(event.currentTarget)
    const password = String(form.get('password'))
    const passwordConfirmation = String(form.get('passwordConfirmation'))

    if (password !== passwordConfirmation) {
      setError(t('auth.passwordMismatch'))
      return
    }

    const credentials: RegistrationCredentials = {
      email: String(form.get('email')).trim(),
      password,
    }

    setIsPending(true)
    try {
      await authApi.register(credentials)
      onRegistered()
    } catch (caughtError) {
      setError(getErrorMessage(caughtError))
    } finally {
      setIsPending(false)
    }
  }

  return (
    <AuthLayout
      eyebrow={t('auth.startJourney')}
      title={t('auth.createAccountTitle')}
      description={t('auth.createAccountDescription')}
    >
      <form onSubmit={handleSubmit} className="mt-8 space-y-5">
        <Field label="Email">
          <input
            className="field"
            type="email"
            name="email"
            autoComplete="email"
            placeholder="you@example.com"
            required
          />
        </Field>
        <div>
          <Field label={t('auth.password')}>
            <input
              className="field"
              type="password"
              name="password"
              minLength={8}
              autoComplete="new-password"
              aria-describedby="password-requirements"
              required
            />
          </Field>
          <p id="password-requirements" className="mt-2 text-xs text-[#69736e]">
            {t('auth.passwordRequirements')}
          </p>
        </div>
        <Field label={t('auth.confirmPassword')}>
          <input
            className="field"
            type="password"
            name="passwordConfirmation"
            minLength={8}
            autoComplete="new-password"
            required
          />
        </Field>
        {error && <Notice message={error} />}
        <button disabled={isPending} className="btn-primary w-full">
          {isPending ? t('auth.creatingAccount') : t('auth.createAccountButton')}
          <ArrowRight size={16} />
        </button>
      </form>
      <p className="mt-7 text-center text-sm text-[#69736e]">
        {t('auth.alreadyHaveAccount')}{' '}
        <button
          type="button"
          onClick={onSignIn}
          className="font-semibold text-[#216c4d] hover:underline"
        >
          {t('auth.signIn')}
        </button>
      </p>
    </AuthLayout>
  )
}
