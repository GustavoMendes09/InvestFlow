import { useEffect, type ReactNode } from 'react'
import { X } from 'lucide-react'
import { useI18n } from '../i18n/i18n'

interface ModalProps {
  open: boolean
  onClose: () => void
  title: string
  children: ReactNode
}

export function Modal({ open, onClose, title, children }: ModalProps) {
  const { t } = useI18n()
  useEffect(() => {
    if (!open) return

    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose()
    }

    document.addEventListener('keydown', closeOnEscape)
    return () => document.removeEventListener('keydown', closeOnEscape)
  }, [onClose, open])

  if (!open) return null

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-[#14231c]/35 p-0 backdrop-blur-[2px] sm:items-center sm:p-4"
      onMouseDown={event => event.target === event.currentTarget && onClose()}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label={title}
        className="max-h-[92vh] w-full overflow-auto rounded-t-3xl bg-white p-6 shadow-2xl sm:max-w-lg sm:rounded-3xl"
      >
        <div className="mb-6 flex items-center justify-between">
          <h2 className="display text-xl font-extrabold">{title}</h2>
          <button
            type="button"
            aria-label={t('common.close')}
            onClick={onClose}
            className="rounded-xl p-2 text-[#727c77] hover:bg-[#f1f2ee]"
          >
            <X size={19} />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}
