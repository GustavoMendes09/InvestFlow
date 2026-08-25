import { z } from 'zod'
import { apiClient } from '../../shared/api/client'
import { contributionSchema, investmentSchema } from '../../shared/api/schemas'

const investmentsSchema = z.array(investmentSchema)

export interface SaveInvestmentInput {
  name: string
  assetClass: string
  investedAmount: number
  currentValue: number
  updatedAt: string
}

export interface RecordContributionInput {
  investmentId: string
  amount: number
  date: string
}

export const investmentsApi = {
  getAll: (signal?: AbortSignal) => apiClient.get('/investments', investmentsSchema, signal),
  create: (input: SaveInvestmentInput) => apiClient.post('/investments', input, investmentSchema),
  recordContribution: ({ investmentId, ...request }: RecordContributionInput) =>
    apiClient.post(`/investments/${investmentId}/contributions`, request, contributionSchema),
  delete: (id: string) => apiClient.delete(`/investments/${id}`),
}
