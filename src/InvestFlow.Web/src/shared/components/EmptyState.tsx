import { Plus, type LucideIcon } from 'lucide-react'

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  text: string
  action?: string
  onAction?: () => void
}

export function EmptyState({ icon: Icon, title, text, action, onAction }: EmptyStateProps) {
  return (
    <div className="flex min-h-56 flex-col items-center justify-center p-8 text-center">
      <span className="rounded-2xl bg-[#edf2ed] p-4 text-[#45735f]">
        <Icon size={24} />
      </span>
      <h3 className="mt-4 font-bold">{title}</h3>
      <p className="mt-1 max-w-sm text-sm text-[#7a847f]">{text}</p>
      {action && (
        <button type="button" className="btn-primary mt-5" onClick={onAction}>
          <Plus size={16} />
          {action}
        </button>
      )}
    </div>
  )
}
