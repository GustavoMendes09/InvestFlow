import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react'
import { formatMonth, shiftMonth } from '../lib/date'
import { useI18n } from '../i18n/i18n'

interface MonthPickerProps {
  value: string
  onChange: (month: string) => void
}

export function MonthPicker({ value, onChange }: MonthPickerProps) {
  const { locale, t } = useI18n()
  return (
    <div className="flex items-center rounded-xl border border-[#dfe2da] bg-white p-1">
      <button
        type="button"
        aria-label={t('month.previous')}
        className="rounded-lg p-1.5 hover:bg-[#f3f4ef]"
        onClick={() => onChange(shiftMonth(value, -1))}
      >
        <ChevronLeft size={16} />
      </button>
      <div className="flex min-w-[130px] items-center justify-center gap-2 px-2 text-xs font-semibold sm:text-sm">
        <CalendarDays size={15} className="text-[#216c4d]" />
        <span>{formatMonth(value, locale)}</span>
      </div>
      <button
        type="button"
        aria-label={t('month.next')}
        className="rounded-lg p-1.5 hover:bg-[#f3f4ef]"
        onClick={() => onChange(shiftMonth(value, 1))}
      >
        <ChevronRight size={16} />
      </button>
    </div>
  )
}
