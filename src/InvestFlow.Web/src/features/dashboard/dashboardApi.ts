import { apiClient } from '../../shared/api/client'
import { dashboardSchema } from '../../shared/api/schemas'

export const dashboardApi = {
  get: (month: string, signal?: AbortSignal) =>
    apiClient.get(`/dashboard?month=${month}`, dashboardSchema, signal),
}
