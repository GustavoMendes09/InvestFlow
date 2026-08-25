import { useState, type FormEvent } from 'react'
import { Goal as GoalIcon, Plus, Target, Trash2 } from 'lucide-react'
import { EmptyState } from '../../shared/components/EmptyState'
import { Field } from '../../shared/components/Field'
import { LoadingState } from '../../shared/components/LoadingState'
import { Modal } from '../../shared/components/Modal'
import { ModalActions } from '../../shared/components/ModalActions'
import { Notice } from '../../shared/components/Notice'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { formatMoney } from '../../shared/lib/currency'
import { formatMonthYear } from '../../shared/lib/date'
import { useI18n } from '../../shared/i18n/i18n'
import { goalsApi, parseGoalType, type SaveGoalInput } from './goalsApi'

export function GoalsPage() {
  const { locale, t } = useI18n()
  const goals = useQuery('goals', goalsApi.getAll)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const createGoal = useMutation(goalsApi.create, () => { setIsModalOpen(false); goals.refetch() })
  const deleteGoal = useMutation(goalsApi.delete, goals.refetch)

  if (goals.isLoading && !goals.data) return <LoadingState />

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SaveGoalInput = {
      name: String(form.get('name')),
      type: parseGoalType(form.get('type')),
      targetAmount: Number(form.get('targetAmount')),
      currentAmount: Number(form.get('currentAmount')),
      deadline: String(form.get('deadline') || '') || null,
    }
    await createGoal.execute(input)
  }

  async function handleDelete(id: string) {
    if (window.confirm(t('goals.deleteConfirm'))) await deleteGoal.execute(id)
  }

  const items = goals.data ?? []
  const error = goals.error ?? deleteGoal.error

  return (
    <>
      <div className="mb-5 flex justify-end"><button type="button" className="btn-primary" onClick={() => setIsModalOpen(true)}><Plus size={16} /> {t('goals.new')}</button></div>
      {error && <div className="mb-4"><Notice message={error} /></div>}
      {items.length ? (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {items.map(goal => (
            <article key={goal.id} className="card p-5">
              <div className="flex items-start justify-between"><span className="rounded-xl bg-[#eee9f7] p-3 text-[#7352a1]"><Target size={20} /></span><button type="button" aria-label={t('common.delete', { name: goal.name })} disabled={deleteGoal.isPending} onClick={() => handleDelete(goal.id)} className="p-2 text-[#9aa19d] hover:text-[#c8584a]"><Trash2 size={16} /></button></div>
              <h3 className="mt-5 font-bold">{goal.name}</h3>
              <div className="mt-4 flex items-baseline justify-between"><span className="display text-xl font-extrabold">{formatMoney(goal.currentAmount, locale)}</span><span className="text-xs text-[#7a847f]">{t('goals.ofTarget', { value: formatMoney(goal.targetAmount, locale) })}</span></div>
              <div className="mt-3 h-2.5 overflow-hidden rounded-full bg-[#ecebe9]"><div className="h-full rounded-full bg-[#7352a1]" style={{ width: `${Math.min(100, goal.progress)}%` }} /></div>
              <div className="mt-3 flex justify-between text-xs"><span className="font-bold text-[#7352a1]">{t('goals.complete', { value: goal.progress })}</span><span className="text-[#7a847f]">{goal.deadline ? formatMonthYear(goal.deadline, locale) : t('common.noDeadline')}</span></div>
            </article>
          ))}
        </div>
      ) : (
        <div className="card"><EmptyState icon={GoalIcon} title={t('goals.emptyTitle')} text={t('goals.emptyText')} action={t('goals.createOne')} onAction={() => setIsModalOpen(true)} /></div>
      )}
      <Modal open={isModalOpen} onClose={() => setIsModalOpen(false)} title={t('goals.createTitle')}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Field label={t('goals.name')}><input name="name" className="field" placeholder={t('goals.namePlaceholder')} required /></Field>
          <Field label={t('goals.type')}><select name="type" className="field"><option value="0">{t('goals.emergencyFund')}</option><option value="1">{t('goals.travel')}</option><option value="2">{t('goals.payDebt')}</option><option value="3">{t('goals.property')}</option><option value="4">{t('goals.retirement')}</option><option value="5">{t('common.other')}</option></select></Field>
          <div className="grid grid-cols-2 gap-3"><Field label={t('goals.targetAmount')}><input name="targetAmount" className="field" type="number" min="1" step="0.01" required /></Field><Field label={t('goals.alreadySaved')}><input name="currentAmount" className="field" type="number" min="0" step="0.01" defaultValue="0" required /></Field></div>
          <Field label={t('goals.deadline')}><input name="deadline" className="field" type="date" /></Field>
          {createGoal.error && <Notice message={createGoal.error} />}
          <ModalActions onCancel={() => setIsModalOpen(false)} label={t('goals.create')} isPending={createGoal.isPending} />
        </form>
      </Modal>
    </>
  )
}
