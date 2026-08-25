import { useState, type FormEvent } from 'react'
import { CreditCard, Landmark, Plus, Trash2 } from 'lucide-react'
import { Field } from '../../shared/components/Field'
import { LoadingState } from '../../shared/components/LoadingState'
import { Modal } from '../../shared/components/Modal'
import { ModalActions } from '../../shared/components/ModalActions'
import { Notice } from '../../shared/components/Notice'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { useI18n } from '../../shared/i18n/i18n'
import { investmentsApi } from '../investments/investmentsApi'
import { accountsApi, type SaveAccountInput } from './accountsApi'

export function NetWorthPage() {
  const { locale, t } = useI18n()
  const accounts = useQuery('accounts', accountsApi.getAll)
  const investments = useQuery('investments:net-worth', investmentsApi.getAll)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const createAccount = useMutation(accountsApi.create, () => { setIsModalOpen(false); accounts.refetch() })
  const deleteAccount = useMutation(accountsApi.delete, accounts.refetch)

  if ((accounts.isLoading && !accounts.data) || (investments.isLoading && !investments.data)) return <LoadingState />

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SaveAccountInput = {
      name: String(form.get('name')),
      balance: Number(form.get('balance')),
      isDebt: form.get('kind') === 'debt',
    }
    await createAccount.execute(input)
  }

  async function handleDelete(id: string) {
    if (window.confirm(t('netWorth.deleteConfirm'))) await deleteAccount.execute(id)
  }

  const assets = (accounts.data?.filter(account => !account.isDebt).reduce((sum, account) => sum + account.balance, 0) ?? 0)
    + (investments.data?.reduce((sum, investment) => sum + investment.currentValue, 0) ?? 0)
  const debts = accounts.data?.filter(account => account.isDebt).reduce((sum, account) => sum + account.balance, 0) ?? 0
  const error = accounts.error ?? investments.error ?? deleteAccount.error

  return (
    <>
      {error && <div className="mb-4"><Notice message={error} /></div>}
      <section className="mb-5 overflow-hidden rounded-3xl bg-[#173f30] p-7 text-white sm:p-9">
        <p className="text-sm font-medium text-[#a9c9bb]">{t('netWorth.total')}</p><div className="display mt-2 text-4xl font-extrabold sm:text-5xl">{formatMoney(assets - debts, locale)}</div>
        <div className="mt-7 grid max-w-xl grid-cols-2 gap-4"><div className="rounded-2xl bg-white/10 p-4"><p className="text-xs text-[#a9c9bb]">{t('netWorth.own')}</p><p className="mt-1 font-bold">{formatMoney(assets, locale)}</p></div><div className="rounded-2xl bg-white/10 p-4"><p className="text-xs text-[#a9c9bb]">{t('netWorth.owe')}</p><p className="mt-1 font-bold">{formatMoney(debts, locale)}</p></div></div>
      </section>
      <div className="mb-4 flex items-center justify-between"><h2 className="font-bold">{t('netWorth.accountsDebts')}</h2><button type="button" className="btn-primary" onClick={() => setIsModalOpen(true)}><Plus size={16} /> {t('netWorth.addItem')}</button></div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {accounts.data?.map(account => (
          <div className="card flex items-center gap-4 p-5" key={account.id}>
            <span className={`rounded-xl p-3 ${account.isDebt ? 'bg-[#f9e9e6] text-[#bd594a]' : 'bg-[#e6f1ea] text-[#216c4d]'}`}>{account.isDebt ? <CreditCard size={20} /> : <Landmark size={20} />}</span>
            <div className="flex-1"><p className="text-sm font-semibold">{account.name}</p><p className="display mt-1 text-xl font-extrabold">{account.isDebt ? '−' : ''}{formatMoney(account.balance, locale)}</p></div>
            <button type="button" aria-label={t('common.delete', { name: account.name })} disabled={deleteAccount.isPending} onClick={() => handleDelete(account.id)} className="p-2 text-[#9aa19d] hover:text-[#c8584a]"><Trash2 size={16} /></button>
          </div>
        ))}
      </div>
      <Modal open={isModalOpen} onClose={() => setIsModalOpen(false)} title={t('netWorth.addTitle')}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Field label={t('common.name')}><input name="name" className="field" placeholder={t('netWorth.namePlaceholder')} required /></Field>
          <div className="grid grid-cols-2 gap-3"><Field label={t('common.type')}><select name="kind" className="field"><option value="asset">{t('netWorth.accountAsset')}</option><option value="debt">{t('netWorth.debt')}</option></select></Field><Field label={t('netWorth.currentBalance')}><input name="balance" className="field" type="number" min="0" step="0.01" required /></Field></div>
          {createAccount.error && <Notice message={createAccount.error} />}
          <ModalActions onCancel={() => setIsModalOpen(false)} label={t('netWorth.addItem')} isPending={createAccount.isPending} />
        </form>
      </Modal>
    </>
  )
}
