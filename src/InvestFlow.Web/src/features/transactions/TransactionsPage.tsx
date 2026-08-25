import { useState, type FormEvent } from 'react'
import { ArrowDownLeft, ArrowUpRight, Plus, ReceiptText, Trash2 } from 'lucide-react'
import { accountsApi } from '../net-worth/accountsApi'
import { categoriesApi } from '../categories/categoriesApi'
import { EmptyState } from '../../shared/components/EmptyState'
import { Field } from '../../shared/components/Field'
import { LoadingState } from '../../shared/components/LoadingState'
import { Modal } from '../../shared/components/Modal'
import { ModalActions } from '../../shared/components/ModalActions'
import { Notice } from '../../shared/components/Notice'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney, formatMoneyExact } from '../../shared/lib/currency'
import { formatShortDate, getCurrentMonth, toLocalDateInput } from '../../shared/lib/date'
import { useI18n } from '../../shared/i18n/i18n'
import { isIncome, TransactionTypes } from '../../shared/lib/transactions'
import { parseTransactionType, transactionsApi, type SaveTransactionInput } from './transactionsApi'

export function TransactionsPage({ month }: { month: string }) {
  const { locale, t } = useI18n()
  const transactions = useQuery(`transactions:${month}`, signal => transactionsApi.getAll(month, signal))
  const categories = useQuery('categories:options', categoriesApi.getAll)
  const accounts = useQuery('accounts:options', accountsApi.getAll)
  const [isModalOpen, setIsModalOpen] = useState(false)

  const createTransaction = useMutation(transactionsApi.create, () => {
    setIsModalOpen(false)
    transactions.refetch()
  })
  const deleteTransaction = useMutation(transactionsApi.delete, transactions.refetch)

  if (transactions.isLoading && !transactions.data) return <LoadingState />

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SaveTransactionInput = {
      type: parseTransactionType(form.get('type')),
      amount: Number(form.get('amount')),
      date: String(form.get('date')),
      categoryId: String(form.get('categoryId') || '') || null,
      description: String(form.get('description') || '') || null,
      accountId: String(form.get('accountId') || '') || null,
    }
    await createTransaction.execute(input)
  }

  async function handleDelete(id: string) {
    if (window.confirm(t('transactions.deleteConfirm'))) await deleteTransaction.execute(id)
  }

  const items = transactions.data ?? []
  const income = items.filter(item => isIncome(item.type)).reduce((total, item) => total + item.amount, 0)
  const expenses = items.filter(item => !isIncome(item.type)).reduce((total, item) => total + item.amount, 0)
  const error = transactions.error ?? categories.error ?? accounts.error ?? deleteTransaction.error

  return (
    <>
      <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
        <div className="flex gap-2">
          <SummaryPill label={t('transactions.income')} value={formatMoney(income, locale)} tone="green" />
          <SummaryPill label={t('dashboard.expenses')} value={formatMoney(expenses, locale)} tone="red" />
        </div>
        <button type="button" className="btn-primary" onClick={() => setIsModalOpen(true)}>
          <Plus size={16} /> {t('transactions.add')}
        </button>
      </div>

      {error && <div className="mb-4"><Notice message={error} /></div>}

      <section className="card overflow-hidden">
        {items.length ? (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[680px] text-left text-sm">
              <thead className="border-b border-[#e8e9e4] bg-[#fafaf7] text-xs uppercase tracking-wider text-[#7a847f]">
                <tr>
                  <th className="px-5 py-3.5">{t('transactions.transaction')}</th>
                  <th className="px-5 py-3.5">{t('transactions.category')}</th>
                  <th className="px-5 py-3.5">{t('common.date')}</th>
                  <th className="px-5 py-3.5 text-right">{t('transactions.amount')}</th>
                  <th className="w-12"><span className="sr-only">{t('common.actions')}</span></th>
                </tr>
              </thead>
              <tbody>
                {items.map(transaction => (
                  <tr key={transaction.id} className="border-b border-[#ecece7] last:border-0 hover:bg-[#fafbf8]">
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-3">
                        <span className={`rounded-xl p-2 ${isIncome(transaction.type) ? 'bg-[#e7f2eb] text-[#24724f]' : 'bg-[#f7ece8] text-[#c05c4c]'}`}>
                          {isIncome(transaction.type) ? <ArrowDownLeft size={17} /> : <ArrowUpRight size={17} />}
                        </span>
                        <div>
                          <div className="font-semibold">{transaction.description ?? transaction.category?.name ?? (isIncome(transaction.type) ? t('transactions.income') : t('transactions.expense'))}</div>
                          <div className="text-xs text-[#87908b]">{transaction.account?.name ?? t('transactions.noAccount')}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-4">
                      <span className="inline-flex items-center gap-2">
                        <i className="size-2 rounded-full" style={{ background: transaction.category?.color ?? '#9ca3af' }} />
                        {transaction.category?.name ?? t('transactions.uncategorised')}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-[#64706a]">{formatShortDate(transaction.date, locale)}</td>
                    <td className={`px-5 py-4 text-right font-bold ${isIncome(transaction.type) ? 'text-[#237454]' : 'text-[#2f3934]'}`}>
                      {isIncome(transaction.type) ? '+' : '−'}{formatMoneyExact(transaction.amount, locale)}
                    </td>
                    <td>
                      <button type="button" aria-label={t('transactions.deleteLabel')} disabled={deleteTransaction.isPending} onClick={() => handleDelete(transaction.id)} className="p-2 text-[#9aa19d] hover:text-[#c8584a]">
                        <Trash2 size={16} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState icon={ReceiptText} title={t('transactions.emptyTitle')} text={t('transactions.emptyText')} action={t('transactions.add')} onAction={() => setIsModalOpen(true)} />
        )}
      </section>

      <Modal open={isModalOpen} onClose={() => setIsModalOpen(false)} title={t('transactions.add')}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Field label={t('common.type')}>
              <select name="type" className="field" defaultValue={TransactionTypes.Expense}>
                <option value={TransactionTypes.Expense}>{t('transactions.expense')}</option>
                <option value={TransactionTypes.Income}>{t('transactions.income')}</option>
              </select>
            </Field>
            <Field label={t('transactions.amount')}><input name="amount" className="field" type="number" min="0.01" step="0.01" required /></Field>
          </div>
          <Field label={t('transactions.description')}><input name="description" className="field" maxLength={160} placeholder={t('transactions.descriptionPlaceholder')} /></Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label={t('transactions.category')}>
              <select name="categoryId" className="field"><option value="">{t('transactions.uncategorised')}</option>{categories.data?.map(category => <option value={category.id} key={category.id}>{category.name}</option>)}</select>
            </Field>
            <Field label={t('common.date')}><input name="date" className="field" type="date" defaultValue={month === getCurrentMonth() ? toLocalDateInput() : `${month}-01`} required /></Field>
          </div>
          <Field label={t('transactions.accountOptional')}>
            <select name="accountId" className="field"><option value="">{t('transactions.noAccount')}</option>{accounts.data?.map(account => <option value={account.id} key={account.id}>{account.name}</option>)}</select>
          </Field>
          {createTransaction.error && <Notice message={createTransaction.error} />}
          <ModalActions onCancel={() => setIsModalOpen(false)} label={t('transactions.save')} isPending={createTransaction.isPending} />
        </form>
      </Modal>
    </>
  )
}

function SummaryPill({ label, value, tone }: { label: string; value: string; tone: 'green' | 'red' }) {
  return (
    <div className={`rounded-xl px-3 py-2 text-xs ${tone === 'green' ? 'bg-[#e6f1ea] text-[#216c4d]' : 'bg-[#f8eae7] text-[#b55447]'}`}>
      <span className="opacity-75">{label}</span> <strong className="ml-1">{value}</strong>
    </div>
  )
}
