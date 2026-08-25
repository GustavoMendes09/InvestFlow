import type { TransactionType } from '../api/schemas'

export const TransactionTypes = {
  Income: 0,
  Expense: 1,
} as const satisfies Record<string, TransactionType>

export function isIncome(type: TransactionType): boolean {
  return type === TransactionTypes.Income
}
