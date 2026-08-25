import { ArrowDownLeft, ArrowRight, ArrowUpRight, Landmark, ReceiptText, TrendingUp, WalletCards } from 'lucide-react'
import type { Page } from '../../app/navigation'
import { EmptyState } from '../../shared/components/EmptyState'
import { LoadingState } from '../../shared/components/LoadingState'
import { MetricCard } from '../../shared/components/MetricCard'
import { Notice } from '../../shared/components/Notice'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { useI18n } from '../../shared/i18n/i18n'
import { dashboardApi } from './dashboardApi'
import { NetWorthChart } from './NetWorthChart'

interface DashboardPageProps {
  month: string
  navigate: (page: Page) => void
}

export function DashboardPage({ month, navigate }: DashboardPageProps) {
  const { locale, t } = useI18n()
  const query = useQuery(`dashboard:${month}`, signal => dashboardApi.get(month, signal))

  if (query.error) return <Notice message={query.error} />
  if (query.isLoading || !query.data) return <LoadingState />

  const dashboard = query.data
  const maximumImpact = Math.max(...dashboard.categoryImpact.map(item => item.amount), 1)
  const retainedPercentage = dashboard.income
    ? Math.max(0, Math.round(dashboard.balance / dashboard.income * 100))
    : 0

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label={t('dashboard.income')} value={formatMoney(dashboard.income, locale)} icon={ArrowDownLeft} tone="green" />
        <MetricCard label={t('dashboard.expenses')} value={formatMoney(dashboard.expenses, locale)} icon={ArrowUpRight} tone="coral" />
        <MetricCard
          label={t('dashboard.leftThisMonth')}
          value={formatMoney(dashboard.balance, locale)}
          icon={WalletCards}
          tone={dashboard.balance >= 0 ? 'blue' : 'coral'}
          note={dashboard.income ? t('dashboard.percentOfIncome', { value: Math.round(dashboard.balance / dashboard.income * 100) }) : t('dashboard.addIncome')}
        />
        <MetricCard
          label={t('dashboard.invested')}
          value={formatMoney(dashboard.invested, locale)}
          icon={TrendingUp}
          tone="purple"
          note={t('dashboard.savingsRate', { value: dashboard.savingsRate })}
        />
      </div>

      <div className="grid gap-5 xl:grid-cols-[1.35fr_.65fr]">
        <section className="card p-5 sm:p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[.12em] text-[#7a847f]">{t('dashboard.netWorth')}</p>
              <p className="display mt-2 text-3xl font-extrabold">{formatMoney(dashboard.netWorth, locale)}</p>
              <p className={`mt-1 text-sm font-semibold ${dashboard.netWorthVariation >= 0 ? 'text-[#22835d]' : 'text-[#cf584a]'}`}>
                {t('dashboard.variation', { value: `${dashboard.netWorthVariation >= 0 ? '+' : ''}${formatMoney(dashboard.netWorthVariation, locale)}` })}
              </p>
            </div>
            <div className="rounded-xl bg-[#e7f1eb] p-3 text-[#216c4d]"><Landmark size={21} /></div>
          </div>
          <NetWorthChart history={dashboard.history} />
          <button type="button" onClick={() => navigate('net-worth')} className="mt-3 flex items-center gap-1 text-sm font-semibold text-[#216c4d]">
            {t('dashboard.seeNetWorth')} <ArrowRight size={15} />
          </button>
        </section>

        <section className="card p-5 sm:p-6">
          <div className="flex items-center justify-between">
            <h2 className="font-bold">{t('dashboard.biggestImpact')}</h2>
            <button type="button" onClick={() => navigate('xray')} className="text-xs font-semibold text-[#216c4d]">{t('dashboard.openXray')}</button>
          </div>
          {dashboard.categoryImpact.length ? (
            <div className="mt-6 space-y-5">
              {dashboard.categoryImpact.slice(0, 5).map(item => (
                <div key={item.categoryId ?? item.name}>
                  <div className="mb-2 flex justify-between text-sm">
                    <span className="flex items-center gap-2 font-medium">
                      <i className="size-2.5 rounded-full" style={{ background: item.color }} />{item.name}
                    </span>
                    <span className="font-semibold">{formatMoney(item.amount, locale)}</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-[#eff0eb]">
                    <div className="h-full rounded-full" style={{ width: `${item.amount / maximumImpact * 100}%`, background: item.color }} />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState icon={ReceiptText} title={t('dashboard.noSpending')} text={t('dashboard.noSpendingText')} action={t('dashboard.addTransaction')} onAction={() => navigate('transactions')} />
          )}
        </section>
      </div>

      <section className="card flex flex-col items-start justify-between gap-5 overflow-hidden p-6 sm:flex-row sm:items-center">
        <div>
          <p className="text-xs font-bold uppercase tracking-[.14em] text-[#216c4d]">{t('dashboard.monthlyPulse')}</p>
          <h2 className="display mt-2 text-xl font-extrabold">{t('dashboard.retained', { value: retainedPercentage })}</h2>
          <p className="mt-1 text-sm text-[#69736e]">{t('dashboard.xrayHint')}</p>
        </div>
        <button type="button" onClick={() => navigate('xray')} className="btn-primary shrink-0">
          {t('dashboard.exploreMonth')} <ArrowRight size={16} />
        </button>
      </section>
    </div>
  )
}
