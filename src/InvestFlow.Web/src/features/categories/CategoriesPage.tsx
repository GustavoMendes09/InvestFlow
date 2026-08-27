import { useState, type FormEvent } from 'react'
import { Banknote, FolderTree, Plus, Trash2 } from 'lucide-react'
import { Field } from '../../shared/components/Field'
import { LoadingState } from '../../shared/components/LoadingState'
import { Modal } from '../../shared/components/Modal'
import { ModalActions } from '../../shared/components/ModalActions'
import { Notice } from '../../shared/components/Notice'
import { useMutation } from '../../shared/hooks/useMutation'
import { useQuery } from '../../shared/hooks/useQuery'
import { useI18n } from '../../shared/i18n/i18n'
import { categoriesApi, type SaveCategoryInput } from './categoriesApi'

export function CategoriesPage() {
  const { t } = useI18n()
  const categories = useQuery('categories', categoriesApi.getAll)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const createCategory = useMutation(categoriesApi.create, () => {
    setIsModalOpen(false)
    categories.refetch()
  })
  const deleteCategory = useMutation(categoriesApi.delete, categories.refetch)

  if (categories.isLoading && !categories.data) return <LoadingState />

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SaveCategoryInput = {
      name: String(form.get('name')),
      color: String(form.get('color')),
      isIncome: form.get('kind') === 'income',
    }
    await createCategory.execute(input)
  }

  async function handleDelete(id: string) {
    if (window.confirm(t('categories.deleteConfirm'))) {
      await deleteCategory.execute(id)
    }
  }

  return (
    <>
      <div className="mb-5 flex justify-end">
        <button type="button" className="btn-primary" onClick={() => setIsModalOpen(true)}><Plus size={16} /> {t('categories.new')}</button>
      </div>
      {(categories.error ?? deleteCategory.error) && <div className="mb-4"><Notice message={(categories.error ?? deleteCategory.error)!} /></div>}
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {categories.data?.map(category => (
          <div key={category.id} className="card flex items-center gap-4 p-5">
            <span className={`grid size-11 place-items-center rounded-xl ${category.isIncome ? 'bg-[#e4f1e9] text-[#18734d]' : 'bg-[#fbeae6] text-[#c4483a]'}`}>
              {category.isIncome ? <Banknote size={20} /> : <FolderTree size={20} />}
            </span>
            <div className="flex-1"><h3 className="flex items-center gap-2 font-bold">{category.name}<i className="size-2 rounded-full" style={{ background: category.color }} /></h3><p className={`text-xs font-semibold ${category.isIncome ? 'text-[#18734d]' : 'text-[#c4483a]'}`}>{category.isIncome ? t('categories.incomeCategory') : t('categories.expenseCategory')}</p></div>
            <button type="button" aria-label={t('common.delete', { name: category.name })} disabled={deleteCategory.isPending} onClick={() => handleDelete(category.id)} className="p-2 text-[#9aa19d] hover:text-[#c8584a]"><Trash2 size={16} /></button>
          </div>
        ))}
      </div>
      <Modal open={isModalOpen} onClose={() => setIsModalOpen(false)} title={t('categories.new')}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Field label={t('categories.categoryName')}><input name="name" className="field" maxLength={60} placeholder={t('categories.namePlaceholder')} required /></Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label={t('common.type')}><select name="kind" className="field"><option value="expense">{t('transactions.expense')}</option><option value="income">{t('transactions.income')}</option></select></Field>
            <Field label={t('categories.colour')}><input name="color" className="field h-[42px] p-1" type="color" defaultValue="#216c4d" /></Field>
          </div>
          {createCategory.error && <Notice message={createCategory.error} />}
          <ModalActions onCancel={() => setIsModalOpen(false)} label={t('categories.create')} isPending={createCategory.isPending} />
        </form>
      </Modal>
    </>
  )
}
