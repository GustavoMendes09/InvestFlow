import { StrictMode } from 'react'
import { act, render, screen } from '@testing-library/react'
import { beforeEach, expect, it, vi } from 'vitest'
import { authApi } from '../features/auth/authApi'
import App from './App'

vi.mock('../features/auth/authApi', () => ({
  authApi: {
    getProfile: vi.fn(),
  },
}))

vi.mock('./AppShell', () => ({
  AppShell: () => <div>authenticated content</div>,
}))

vi.mock('../features/auth/LoginPage', () => ({
  LoginPage: () => <div>login page</div>,
}))

beforeEach(() => vi.clearAllMocks())

it('keeps the user signed in when StrictMode cancels the first profile request', async () => {
  vi.mocked(authApi.getProfile)
    .mockImplementationOnce(signal => new Promise((_, reject) => {
      signal?.addEventListener('abort', () => {
        setTimeout(() => reject(new DOMException('Aborted', 'AbortError')), 10)
      })
    }))
    .mockResolvedValueOnce({ email: 'person@example.com' })

  render(<StrictMode><App /></StrictMode>)

  expect(await screen.findByText('authenticated content')).toBeInTheDocument()
  await act(() => new Promise(resolve => setTimeout(resolve, 30)))
  expect(screen.queryByText('login page')).not.toBeInTheDocument()
  expect(authApi.getProfile).toHaveBeenCalledTimes(2)
})
