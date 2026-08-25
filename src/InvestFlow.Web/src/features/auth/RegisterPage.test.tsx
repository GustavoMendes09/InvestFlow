import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { authApi } from './authApi'
import { RegisterPage } from './RegisterPage'

vi.mock('./authApi', () => ({
  authApi: {
    register: vi.fn(),
  },
}))

describe('RegisterPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('registers a new account and returns to sign in', async () => {
    vi.mocked(authApi.register).mockResolvedValue(undefined)
    const onRegistered = vi.fn()
    const user = userEvent.setup()

    render(<RegisterPage onRegistered={onRegistered} onSignIn={vi.fn()} />)

    await user.type(screen.getByLabelText('Email'), 'person@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password123')
    await user.type(screen.getByLabelText('Confirm password'), 'Password123')
    await user.click(screen.getByRole('button', { name: /^create account$/i }))

    expect(authApi.register).toHaveBeenCalledWith({
      email: 'person@example.com',
      password: 'Password123',
    })
    expect(onRegistered).toHaveBeenCalledOnce()
  })

  it('does not submit when password confirmation differs', async () => {
    const user = userEvent.setup()
    render(<RegisterPage onRegistered={vi.fn()} onSignIn={vi.fn()} />)

    await user.type(screen.getByLabelText('Email'), 'person@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password123')
    await user.type(screen.getByLabelText('Confirm password'), 'Password456')
    await user.click(screen.getByRole('button', { name: /^create account$/i }))

    expect(screen.getByRole('alert')).toHaveTextContent('The passwords do not match.')
    expect(authApi.register).not.toHaveBeenCalled()
  })
})
