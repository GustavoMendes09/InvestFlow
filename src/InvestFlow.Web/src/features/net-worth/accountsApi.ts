import { z } from 'zod'
import { apiClient } from '../../shared/api/client'
import { accountSchema } from '../../shared/api/schemas'

const accountsSchema = z.array(accountSchema)

export interface SaveAccountInput {
  name: string
  balance: number
  isDebt: boolean
}

export const accountsApi = {
  getAll: (signal?: AbortSignal) => apiClient.get('/accounts', accountsSchema, signal),
  create: (input: SaveAccountInput) => apiClient.post('/accounts', input, accountSchema),
  delete: (id: string) => apiClient.delete(`/accounts/${id}`),
}
