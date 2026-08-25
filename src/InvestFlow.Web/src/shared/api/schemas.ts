import { z } from 'zod'

export const transactionTypeSchema = z.union([z.literal(0), z.literal(1)])
export type TransactionType = z.infer<typeof transactionTypeSchema>

export const goalTypeSchema = z.union([
  z.literal(0),
  z.literal(1),
  z.literal(2),
  z.literal(3),
  z.literal(4),
  z.literal(5),
])
export type GoalType = z.infer<typeof goalTypeSchema>

export const categorySchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  color: z.string(),
  isIncome: z.boolean(),
})
export type Category = z.infer<typeof categorySchema>

export const accountSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  balance: z.number(),
  isDebt: z.boolean(),
})
export type Account = z.infer<typeof accountSchema>

export const transactionSchema = z.object({
  id: z.string().uuid(),
  type: transactionTypeSchema,
  amount: z.number(),
  date: z.string(),
  description: z.string().nullish(),
  category: categorySchema.nullish(),
  account: accountSchema.nullish(),
})
export type Transaction = z.infer<typeof transactionSchema>

export const budgetSchema = z.object({
  id: z.string().uuid(),
  categoryId: z.string().uuid(),
  category: categorySchema.nullish(),
  month: z.string(),
  amount: z.number(),
  spent: z.number(),
  remaining: z.number(),
})
export type Budget = z.infer<typeof budgetSchema>

export const contributionSchema = z.object({
  id: z.string().uuid(),
  amount: z.number(),
  date: z.string(),
})
export type Contribution = z.infer<typeof contributionSchema>

export const investmentSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  assetClass: z.string(),
  investedAmount: z.number(),
  currentValue: z.number(),
  updatedAt: z.string(),
  contributions: z.array(contributionSchema),
})
export type Investment = z.infer<typeof investmentSchema>

export const goalSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  type: goalTypeSchema,
  targetAmount: z.number(),
  currentAmount: z.number(),
  deadline: z.string().nullish(),
  progress: z.number(),
})
export type Goal = z.infer<typeof goalSchema>

export const dashboardSchema = z.object({
  month: z.string(),
  income: z.number(),
  expenses: z.number(),
  balance: z.number(),
  invested: z.number(),
  savingsRate: z.number(),
  netWorth: z.number(),
  netWorthVariation: z.number(),
  categoryImpact: z.array(z.object({
    categoryId: z.string().uuid().nullable(),
    name: z.string(),
    color: z.string(),
    amount: z.number(),
  })),
  history: z.array(z.object({
    month: z.string(),
    netWorth: z.number(),
  })),
})
export type Dashboard = z.infer<typeof dashboardSchema>

export const profileSchema = z.object({ email: z.string().email().nullable() })
export type Profile = z.infer<typeof profileSchema>
