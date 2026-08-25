import { z } from 'zod'
import { apiClient } from '../../shared/api/client'
import { goalSchema, goalTypeSchema, type GoalType } from '../../shared/api/schemas'

const goalsSchema = z.array(goalSchema)

export interface SaveGoalInput {
  name: string
  type: GoalType
  targetAmount: number
  currentAmount: number
  deadline: string | null
}

export const goalsApi = {
  getAll: (signal?: AbortSignal) => apiClient.get('/goals', goalsSchema, signal),
  create: (input: SaveGoalInput) => apiClient.post('/goals', input, goalSchema),
  delete: (id: string) => apiClient.delete(`/goals/${id}`),
}

export function parseGoalType(value: FormDataEntryValue | null): GoalType {
  return goalTypeSchema.parse(Number(value))
}
