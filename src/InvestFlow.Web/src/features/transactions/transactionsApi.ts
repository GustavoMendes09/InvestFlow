import { apiClient } from '../../shared/api/client'
import { transactionSchema, transactionTypeSchema, type TransactionType } from '../../shared/api/schemas'
import { z } from 'zod'

const transactionsSchema = z.array(transactionSchema)

export interface SaveTransactionInput {
  type: TransactionType
  amount: number
  date: string
  categoryId: string | null
  description: string | null
  accountId: string | null
}

export const transactionsApi = {
  getAll: (month: string, signal?: AbortSignal) =>
    apiClient.get(`/transactions?month=${month}`, transactionsSchema, signal),
  create: (input: SaveTransactionInput) =>
    apiClient.post('/transactions', input, transactionSchema),
  delete: (id: string) => apiClient.delete(`/transactions/${id}`),
}

export function parseTransactionType(value: FormDataEntryValue | null): TransactionType {
  return transactionTypeSchema.parse(Number(value))
}
