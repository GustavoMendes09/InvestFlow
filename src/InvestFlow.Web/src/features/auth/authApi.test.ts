import { afterEach, describe, expect, it, vi } from 'vitest'
import { authApi } from './authApi'

describe('authApi', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('maps a username to the Identity login contract', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true })
    vi.stubGlobal('fetch', fetchMock)

    await authApi.login({ login: 'admin', password: '123' })

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/auth/login?useCookies=true',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ email: 'admin', password: '123' }),
      }),
    )
  })
})
