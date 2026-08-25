import { describe, expect, it } from 'vitest'
import { transactionSchema } from './schemas'

describe('API schemas', () => {
  it('accepts the transaction contract returned by the backend', () => {
    const result = transactionSchema.safeParse({
      id: '80b44cbe-2aad-473b-bb89-e11f62887a86',
      type: 1,
      amount: 125.5,
      date: '2026-08-25',
      description: 'Groceries',
      category: null,
      account: null,
    })

    expect(result.success).toBe(true)
  })

  it('rejects undocumented enum values', () => {
    const result = transactionSchema.safeParse({
      id: '80b44cbe-2aad-473b-bb89-e11f62887a86',
      type: 'Expense',
      amount: 125.5,
      date: '2026-08-25',
    })

    expect(result.success).toBe(false)
  })
})
