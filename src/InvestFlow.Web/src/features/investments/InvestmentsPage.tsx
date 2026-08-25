import { useState, type FormEvent } from 'react'
import { ChartNoAxesCombined, CircleDollarSign, Plus, Trash2, TrendingUp } from 'lucide-react'
import type { Investment } from '../../shared/api/schemas'
import { EmptyState } from '../../shared/components/EmptyState'
import { Field } from '../../shared/components/Field'
import { LoadingState } from '../../shared/components/LoadingState'
import { MetricCard } from '../../shared/components/MetricCard'
import { Modal } from '../../shared/components/Modal'
import { ModalActions } from '../../shared/components/ModalActions'
import { Notice } from '../../shared/components/Notice'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { formatShortDate, toLocalDateInput } from '../../shared/lib/date'
import { useI18n } from '../../shared/i18n/i18n'
import { investmentsApi, type RecordContributionInput, type SaveInvestmentInput } from './investmentsApi'

type Dialog = 'investment' | 'contribution' | null

export function InvestmentsPage() {
  const { locale, t } = useI18n()
  const investments = useQuery('investments', investmentsApi.getAll)
  const [dialog, setDialog] = useState<Dialog>(null)
  const [selectedInvestment, setSelectedInvestment] = useState<Investment | null>(null)

  function closeAndRefresh() {
    setDialog(null)
    setSelectedInvestment(null)
    investments.refetch()
  }

  const createInvestment = useMutation(investmentsApi.create, () => closeAndRefresh())
  const recordContribution = useMutation(investmentsApi.recordContribution, () => closeAndRefresh())
  const deleteInvestment = useMutation(investmentsApi.delete, investments.refetch)

  if (investments.isLoading && !investments.data) return <LoadingState />

  async function handleInvestmentSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SaveInvestmentInput = {
      name: String(form.get('name')),
      assetClass: String(form.get('assetClass')),
      investedAmount: Number(form.get('investedAmount')),
      currentValue: Number(form.get('currentValue')),
      updatedAt: String(form.get('updatedAt')),
    }
    await createInvestment.execute(input)
  }

  async function handleContributionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedInvestment) return
    const form = new FormData(event.currentTarget)
    const input: RecordContributionInput = {
      investmentId: selectedInvestment.id,
      amount: Number(form.get('amount')),
      date: String(form.get('date')),
    }
    await recordContribution.execute(input)
  }

  async function handleDelete(id: string) {
    if (window.confirm(t('investments.deleteConfirm'))) await deleteInvestment.execute(id)
  }

  const items = investments.data ?? []
  const currentValue = items.reduce((sum, item) => sum + item.currentValue, 0)
  const investedAmount = items.reduce((sum, item) => sum + item.investedAmount, 0)
  const error = investments.error ?? deleteInvestment.error

  return (
    <>
      <div className="mb-5 flex justify-end"><button type="button" className="btn-primary" onClick={() => setDialog('investment')}><Plus size={16} /> {t('investments.add')}</button></div>
      {error && <div className="mb-4"><Notice message={error} /></div>}
      <div className="mb-5 grid gap-4 sm:grid-cols-3">
        <MetricCard label={t('investments.currentValue')} value={formatMoney(currentValue, locale)} icon={ChartNoAxesCombined} tone="green" />
        <MetricCard label={t('investments.totalContributed')} value={formatMoney(investedAmount, locale)} icon={CircleDollarSign} tone="blue" />
        <MetricCard label={t('investments.growth')} value={formatMoney(currentValue - investedAmount, locale)} icon={TrendingUp} tone={currentValue - investedAmount >= 0 ? 'purple' : 'coral'} note={investedAmount ? t('investments.overall', { value: ((currentValue - investedAmount) / investedAmount * 100).toFixed(1) }) : t('investments.addHint')} />
      </div>

      {items.length ? (
        <div className="grid gap-4 lg:grid-cols-2">
          {items.map(investment => (
            <InvestmentCard
              key={investment.id}
              investment={investment}
              isDeleting={deleteInvestment.isPending}
              onDelete={() => handleDelete(investment.id)}
              onContribute={() => { setSelectedInvestment(investment); setDialog('contribution') }}
            />
          ))}
        </div>
      ) : (
        <div className="card"><EmptyState icon={TrendingUp} title={t('investments.emptyTitle')} text={t('investments.emptyText')} action={t('investments.add')} onAction={() => setDialog('investment')} /></div>
      )}

      <Modal open={dialog === 'investment'} onClose={() => setDialog(null)} title={t('investments.add')}>
        <form onSubmit={handleInvestmentSubmit} className="space-y-4">
          <Field label={t('investments.name')}><input name="name" className="field" placeholder={t('investments.namePlaceholder')} required /></Field>
          <Field label={t('investments.assetClass')}><select name="assetClass" className="field"><option value="Shares">{t('investments.shares')}</option><option value="ETF">{t('investments.etf')}</option><option value="Cash">{t('investments.cash')}</option><option value="Bonds">{t('investments.bonds')}</option><option value="Property">{t('investments.property')}</option><option value="Other">{t('common.other')}</option></select></Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label={t('investments.amountInvested')}><input name="investedAmount" className="field" type="number" min="0" step="0.01" required /></Field>
            <Field label={t('investments.currentValue')}><input name="currentValue" className="field" type="number" min="0" step="0.01" required /></Field>
          </div>
          <Field label={t('investments.valueUpdated')}><input name="updatedAt" className="field" type="date" defaultValue={toLocalDateInput()} required /></Field>
          {createInvestment.error && <Notice message={createInvestment.error} />}
          <ModalActions onCancel={() => setDialog(null)} label={t('investments.add')} isPending={createInvestment.isPending} />
        </form>
      </Modal>

      <Modal open={dialog === 'contribution'} onClose={() => setDialog(null)} title={t('investments.contributeTo', { name: selectedInvestment?.name ?? '' })}>
        <form onSubmit={handleContributionSubmit} className="space-y-4">
          <Field label={t('investments.contributionAmount')}><input name="amount" className="field" type="number" min="0.01" step="0.01" required autoFocus /></Field>
          <Field label={t('common.date')}><input name="date" className="field" type="date" defaultValue={toLocalDateInput()} required /></Field>
          {recordContribution.error && <Notice message={recordContribution.error} />}
          <ModalActions onCancel={() => setDialog(null)} label={t('investments.recordContribution')} isPending={recordContribution.isPending} />
        </form>
      </Modal>
    </>
  )
}

function InvestmentCard({ investment, isDeleting, onDelete, onContribute }: { investment: Investment; isDeleting: boolean; onDelete: () => void; onContribute: () => void }) {
  const { locale, t } = useI18n()
  const gain = investment.currentValue - investment.investedAmount
  const assetClass = ({
    Shares: t('investments.shares'), ETF: t('investments.etf'), Cash: t('investments.cash'),
    Bonds: t('investments.bonds'), Property: t('investments.property'), Other: t('common.other'),
  } as Record<string, string>)[investment.assetClass] ?? investment.assetClass
  return (
    <article className="card p-5">
      <div className="flex items-start justify-between">
        <div className="flex gap-3"><span className="rounded-xl bg-[#e7f1eb] p-3 text-[#216c4d]"><TrendingUp size={20} /></span><div><h3 className="font-bold">{investment.name}</h3><p className="text-xs text-[#7a847f]">{assetClass} · {t('investments.updated', { date: formatShortDate(investment.updatedAt, locale) })}</p></div></div>
        <button type="button" aria-label={t('common.delete', { name: investment.name })} disabled={isDeleting} onClick={onDelete} className="p-2 text-[#9aa19d] hover:text-[#c8584a]"><Trash2 size={16} /></button>
      </div>
      <div className="mt-6 grid grid-cols-2 gap-4"><div><p className="text-xs text-[#7a847f]">{t('investments.currentValue')}</p><p className="display mt-1 text-xl font-extrabold">{formatMoney(investment.currentValue, locale)}</p></div><div><p className="text-xs text-[#7a847f]">{t('investments.return')}</p><p className={`mt-1 font-bold ${gain >= 0 ? 'text-[#247454]' : 'text-[#c8584a]'}`}>{gain >= 0 ? '+' : ''}{formatMoney(gain, locale)}</p></div></div>
      <button type="button" onClick={onContribute} className="btn-secondary mt-5 w-full"><Plus size={15} /> {t('investments.recordContribution')}</button>
    </article>
  )
}
