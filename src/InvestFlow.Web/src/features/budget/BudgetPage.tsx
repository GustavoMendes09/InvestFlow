import { useMemo, useState } from 'react'
import { Check, CreditCard, FolderTree, PiggyBank, WalletCards } from 'lucide-react'
import type { Budget, Category } from '../../shared/api/schemas'
import { MetricCard } from '../../shared/components/MetricCard'
import { Notice } from '../../shared/components/Notice'
import { LoadingState } from '../../shared/components/LoadingState'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { useI18n } from '../../shared/i18n/i18n'
import { categoriesApi } from '../categories/categoriesApi'
import { budgetApi, type SaveBudgetInput } from './budgetApi'

export function BudgetPage({ month }: { month: string }) {
  const { locale, t } = useI18n()
  const budgets = useQuery(`budgets:${month}`, signal => budgetApi.getAll(month, signal))
  const categories = useQuery('categories:budget', categoriesApi.getAll)
  const [savingCategoryId, setSavingCategoryId] = useState<string | null>(null)
  const saveBudget = useMutation(budgetApi.save, budgets.refetch)
  const budgetByCategory = useMemo(
    () => new Map(budgets.data?.map(budget => [budget.categoryId, budget])),
    [budgets.data],
  )

  if ((budgets.isLoading && !budgets.data) || (categories.isLoading && !categories.data)) return <LoadingState />

  async function handleSave(categoryId: string, amount: number) {
    setSavingCategoryId(categoryId)
    const input: SaveBudgetInput = { categoryId, month: `${month}-01`, amount }
    await saveBudget.execute(input)
    setSavingCategoryId(null)
  }

  const expenseCategories = categories.data?.filter(category => !category.isIncome) ?? []
  const total = budgets.data?.reduce((sum, budget) => sum + budget.amount, 0) ?? 0
  const spent = budgets.data?.reduce((sum, budget) => sum + budget.spent, 0) ?? 0
  const error = budgets.error ?? categories.error ?? saveBudget.error

  return (
    <div className="space-y-5">
      {error && <Notice message={error} />}
      <div className="grid gap-4 sm:grid-cols-3">
        <MetricCard label={t('budget.total')} value={formatMoney(total, locale)} icon={PiggyBank} tone="blue" />
        <MetricCard label={t('budget.spent')} value={formatMoney(spent, locale)} icon={CreditCard} tone="coral" emphasizeValue />
        <MetricCard label={t('budget.available')} value={formatMoney(total - spent, locale)} icon={WalletCards} tone={total - spent >= 0 ? 'green' : 'coral'} emphasizeValue />
      </div>
      <section className="card overflow-hidden">
        <div className="border-b border-[#e6e7e1] px-5 py-4"><h2 className="font-bold">{t('budget.categoryLimits')}</h2></div>
        {expenseCategories.length ? (
          <div className="divide-y divide-[#ebebe6]">
            {expenseCategories.map(category => (
              <BudgetRow key={category.id} category={category} budget={budgetByCategory.get(category.id)} isSaving={savingCategoryId === category.id} onSave={amount => handleSave(category.id, amount)} />
            ))}
          </div>
        ) : (
          <div className="flex min-h-56 flex-col items-center justify-center text-[#7a847f]"><FolderTree size={24} /><p className="mt-3">{t('budget.empty')}</p></div>
        )}
      </section>
    </div>
  )
}

function BudgetRow({ category, budget, isSaving, onSave }: { category: Category; budget?: Budget; isSaving: boolean; onSave: (amount: number) => void }) {
  const { locale, t } = useI18n()
  const [amount, setAmount] = useState(budget?.amount ?? 0)
  const usedPercentage = budget?.amount ? Math.min(100, budget.spent / budget.amount * 100) : 0

  return (
    <div className="grid gap-4 p-5 md:grid-cols-[1fr_1.2fr_180px] md:items-center">
      <div className="flex items-center gap-3"><i className="size-3 rounded-full" style={{ background: category.color }} /><div><div className="font-semibold">{category.name}</div><div className="text-xs font-semibold text-[#c4483a]">{t('budget.spentValue', { value: formatMoney(budget?.spent ?? 0, locale) })}</div></div></div>
      <div><div className="mb-2 flex justify-between text-xs text-[#737d78]"><span className="font-semibold text-[#c4483a]">{t('budget.used', { value: Math.round(usedPercentage) })}</span><span className={(budget?.remaining ?? amount) >= 0 ? 'font-semibold text-[#18734d]' : 'font-semibold text-[#c4483a]'}>{t('budget.left', { value: formatMoney(budget?.remaining ?? amount, locale) })}</span></div><div className="h-2 overflow-hidden rounded-full bg-[#f4dfdb]"><div className={`h-full rounded-full ${usedPercentage >= 100 ? 'bg-[#bd3f32]' : 'bg-[#d75a4c]'}`} style={{ width: `${usedPercentage}%` }} /></div></div>
      <div className="flex gap-2"><input aria-label={t('budget.inputLabel', { name: category.name })} className="field" type="number" min="0" step="10" value={amount} onChange={event => setAmount(Number(event.target.value))} /><button type="button" aria-label={t('budget.saveLabel', { name: category.name })} disabled={isSaving || amount === budget?.amount} onClick={() => onSave(amount)} className="btn-secondary !px-3">{isSaving ? '…' : <Check size={16} />}</button></div>
    </div>
  )
}
