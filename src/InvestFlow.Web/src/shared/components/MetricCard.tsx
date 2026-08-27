import type { LucideIcon } from 'lucide-react'

type Tone = 'green' | 'coral' | 'blue' | 'purple'

interface MetricCardProps {
  label: string
  value: string
  icon: LucideIcon
  tone: Tone
  note?: string
  emphasizeValue?: boolean
}

const tones: Record<Tone, string> = {
  green: 'bg-[#e4f1e9] text-[#216c4d]',
  coral: 'bg-[#fbeae6] text-[#c45a48]',
  blue: 'bg-[#e7eef9] text-[#3767a4]',
  purple: 'bg-[#eee9f7] text-[#7352a1]',
}

const valueTones: Record<Tone, string> = {
  green: 'text-[#18734d]',
  coral: 'text-[#c4483a]',
  blue: 'text-[#315f98]',
  purple: 'text-[#684792]',
}

export function MetricCard({ label, value, icon: Icon, tone, note, emphasizeValue = false }: MetricCardProps) {
  return (
    <div className="card p-5">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-[#6c7671]">{label}</span>
        <span className={`rounded-xl p-2.5 ${tones[tone]}`}>
          <Icon size={18} />
        </span>
      </div>
      <div className={`display mt-4 text-2xl font-extrabold ${emphasizeValue ? valueTones[tone] : ''}`}>{value}</div>
      {note && <p className="mt-1 text-xs text-[#7a847f]">{note}</p>}
    </div>
  )
}
