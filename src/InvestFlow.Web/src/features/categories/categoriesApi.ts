import { z } from 'zod'
import { apiClient } from '../../shared/api/client'
import { categorySchema } from '../../shared/api/schemas'

const categoriesSchema = z.array(categorySchema)

export interface SaveCategoryInput {
  name: string
  color: string
  isIncome: boolean
}

export const categoriesApi = {
  getAll: (signal?: AbortSignal) => apiClient.get('/categories', categoriesSchema, signal),
  create: (input: SaveCategoryInput) => apiClient.post('/categories', input, categorySchema),
  delete: (id: string) => apiClient.delete(`/categories/${id}`),
}
