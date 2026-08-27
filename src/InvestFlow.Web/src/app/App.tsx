import { useEffect, useState } from 'react'
import { authApi } from '../features/auth/authApi'
import { LoginPage } from '../features/auth/LoginPage'
import { RegisterPage } from '../features/auth/RegisterPage'
import { Brand } from '../shared/components/Brand'
import { AppShell } from './AppShell'

type AuthenticationState = 'loading' | 'signed-in' | 'signed-out'
type AuthenticationScreen = 'login' | 'register'

export default function App() {
  const [authentication, setAuthentication] = useState<AuthenticationState>('loading')
  const [authenticationScreen, setAuthenticationScreen] = useState<AuthenticationScreen>('login')
  const [registrationComplete, setRegistrationComplete] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    authApi.getProfile(controller.signal)
      .then(() => setAuthentication('signed-in'))
      .catch(error => {
        if (controller.signal.aborted || (error instanceof DOMException && error.name === 'AbortError')) {
          return
        }

        setAuthentication('signed-out')
      })
    return () => controller.abort()
  }, [])

  if (authentication === 'loading') {
    return <div className="flex min-h-screen items-center justify-center"><Brand /></div>
  }

  if (authentication === 'signed-out') {
    if (authenticationScreen === 'register') {
      return (
        <RegisterPage
          onRegistered={() => {
            setRegistrationComplete(true)
            setAuthenticationScreen('login')
          }}
          onSignIn={() => setAuthenticationScreen('login')}
        />
      )
    }

    return (
      <LoginPage
        registrationComplete={registrationComplete}
        onSuccess={() => setAuthentication('signed-in')}
        onCreateAccount={() => {
          setRegistrationComplete(false)
          setAuthenticationScreen('register')
        }}
      />
    )
  }

  return (
    <AppShell
      onSignedOut={() => {
        setAuthenticationScreen('login')
        setAuthentication('signed-out')
      }}
    />
  )
}
