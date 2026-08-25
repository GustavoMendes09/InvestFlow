interface ModalActionsProps {
  onCancel: () => void
  label: string
  isPending?: boolean
}

export function ModalActions({ onCancel, label, isPending = false }: ModalActionsProps) {
  const { t } = useI18n()
  return (
    <div className="flex justify-end gap-2 pt-2">
      <button type="button" onClick={onCancel} className="btn-secondary">
        {t('common.cancel')}
      </button>
      <button type="submit" disabled={isPending} className="btn-primary">
        {isPending ? t('common.saving') : label}
      </button>
    </div>
  )
}
import { useI18n } from '../i18n/i18n'
