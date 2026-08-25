import { apiClient } from '../../shared/api/client'
import { profileSchema } from '../../shared/api/schemas'

export interface LoginCredentials {
  login: string
  password: string
}

export interface RegistrationCredentials {
  email: string
  password: string
}

export const authApi = {
  getProfile: (signal?: AbortSignal) => apiClient.get('/profile', profileSchema, signal),
  register: (credentials: RegistrationCredentials) =>
    apiClient.postWithoutResponse('/auth/register', credentials),
  login: (credentials: LoginCredentials) =>
    apiClient.postWithoutResponse('/auth/login?useCookies=true', {
      email: credentials.login,
      password: credentials.password,
    }),
  logout: () => apiClient.postWithoutResponse('/auth/logout'),
}
