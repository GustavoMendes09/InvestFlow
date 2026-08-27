import { Activity } from 'lucide-react'
import { EmptyState } from '../../shared/components/EmptyState'
import { LoadingState } from '../../shared/components/LoadingState'
import { Notice } from '../../shared/components/Notice'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { formatMonth } from '../../shared/lib/date'
import { useI18n } from '../../shared/i18n/i18n'
import { dashboardApi } from '../dashboard/dashboardApi'

export function MonthlyXrayPage({ month }: { month: string }) {
  const { locale, t } = useI18n()
  const query = useQuery(`monthly-xray:${month}`, signal => dashboardApi.get(month, signal))

  if (query.error) return <Notice message={query.error} />
  if (query.isLoading || !query.data) return <LoadingState />

  const dashboard = query.data
  const biggestCategory = dashboard.categoryImpact[0]
  const spentPercentage = dashboard.income ? dashboard.expenses / dashboard.income * 100 : 0

  return (
    <div className="space-y-5">
      <section className="grid gap-5 xl:grid-cols-[1.1fr_.9fr]">
        <div className={`overflow-hidden rounded-3xl p-7 text-white sm:p-9 ${dashboard.balance >= 0 ? 'bg-[#173f30]' : 'bg-[#6f2924]'}`}>
          <p className={`text-xs font-bold uppercase tracking-[.16em] ${dashboard.balance >= 0 ? 'text-[#9bc4b2]' : 'text-[#f3b6ae]'}`}>{t('xray.glance', { month: formatMonth(month, locale) })}</p>
          <h2 className="display mt-4 max-w-xl text-3xl font-extrabold sm:text-4xl">
            {dashboard.balance >= 0
              ? t('xray.positiveTitle')
              : t('xray.negativeTitle')}
          </h2>
          <p className={`mt-4 max-w-lg ${dashboard.balance >= 0 ? 'text-[#bfd5cc]' : 'text-[#f4d2cd]'}`}>
            {dashboard.balance >= 0
              ? t('xray.positiveDescription', { value: formatMoney(dashboard.balance, locale) })
              : t('xray.negativeDescription', { value: formatMoney(Math.abs(dashboard.balance), locale) })}
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <span className="rounded-full bg-[#c4483a]/35 px-4 py-2 text-sm font-semibold text-[#ffe5e0]">{t('xray.incomeSpent', { value: spentPercentage.toFixed(0) })}</span>
            <span className="rounded-full bg-white/10 px-4 py-2 text-sm">{t('xray.invested', { value: dashboard.savingsRate })}</span>
          </div>
        </div>

        <div className="card p-6">
          <p className="text-sm font-semibold text-[#69736e]">{t('xray.mattered')}</p>
          {biggestCategory ? (
            <>
              <div className="mt-5 flex items-center gap-3"><i className="size-4 rounded-full" style={{ background: biggestCategory.color }} /><h3 className="display text-2xl font-extrabold">{biggestCategory.name}</h3></div>
              <p className="display mt-3 text-4xl font-extrabold text-[#c4483a]">{formatMoney(biggestCategory.amount, locale)}</p>
              <p className="mt-2 text-sm font-medium text-[#a9473c]">{dashboard.expenses ? t('xray.expenseShare', { value: (biggestCategory.amount / dashboard.expenses * 100).toFixed(0) }) : t('xray.noExpenses')}</p>
            </>
          ) : (
            <EmptyState icon={Activity} title={t('xray.emptyTitle')} text={t('xray.emptyText')} />
          )}
        </div>
      </section>

      <section className="card p-6">
        <h2 className="font-bold">{t('xray.breakdown')}</h2>
        {dashboard.categoryImpact.length ? (
          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            {dashboard.categoryImpact.map((category, index) => (
              <div key={category.categoryId ?? category.name} className="rounded-2xl bg-[#f7f8f4] p-4">
                <div className="flex items-center justify-between"><span className="flex items-center gap-2 text-sm font-semibold"><span className="grid size-6 place-items-center rounded-lg bg-[#fbeae6] text-xs text-[#b7473a]">{index + 1}</span><i className="size-2 rounded-full" style={{ background: category.color }} />{category.name}</span><span className="font-bold text-[#c4483a]">{formatMoney(category.amount, locale)}</span></div>
                <div className="mt-3 h-1.5 overflow-hidden rounded-full bg-[#f4dfdb]"><div className="h-full rounded-full bg-[#d75a4c]" style={{ width: `${category.amount / (biggestCategory?.amount ?? 1) * 100}%` }} /></div>
              </div>
            ))}
          </div>
        ) : (
          <p className="mt-5 text-sm text-[#7a847f]">{t('xray.noData')}</p>
        )}
      </section>
    </div>
  )
}
