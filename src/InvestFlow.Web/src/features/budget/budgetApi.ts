import { z } from 'zod'
import { apiClient } from '../../shared/api/client'
import { budgetSchema } from '../../shared/api/schemas'

const budgetsSchema = z.array(budgetSchema)

export interface SaveBudgetInput {
  categoryId: string
  month: string
  amount: number
}

export const budgetApi = {
  getAll: (month: string, signal?: AbortSignal) =>
    apiClient.get(`/budgets?month=${month}`, budgetsSchema, signal),
  save: ({ categoryId, ...request }: SaveBudgetInput) =>
    apiClient.putWithoutResponse(`/budgets/${categoryId}`, request),
}
