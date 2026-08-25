import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { authApi } from './authApi'
import { LoginPage } from './LoginPage'

vi.mock('./authApi', () => ({
  authApi: {
    login: vi.fn(),
  },
}))

describe('LoginPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('accepts the development administrator credentials', async () => {
    vi.mocked(authApi.login).mockResolvedValue(undefined)
    const onSuccess = vi.fn()
    const user = userEvent.setup()

    render(
      <LoginPage
        registrationComplete={false}
        onSuccess={onSuccess}
        onCreateAccount={vi.fn()}
      />,
    )

    await user.type(screen.getByLabelText('Username or email'), 'admin')
    await user.type(screen.getByLabelText('Password'), '123')
    await user.click(screen.getByRole('button', { name: /^sign in$/i }))

    expect(authApi.login).toHaveBeenCalledWith({ login: 'admin', password: '123' })
    expect(onSuccess).toHaveBeenCalledOnce()
  })
})
