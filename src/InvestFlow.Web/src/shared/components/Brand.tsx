import { TrendingUp } from 'lucide-react'

interface BrandProps {
  tone?: 'default' | 'inverse'
}

export function Brand({ tone = 'default' }: BrandProps) {
  const isInverse = tone === 'inverse'

  return (
    <div className="flex items-center gap-3">
      <div className={`grid size-9 place-items-center rounded-xl text-white ${isInverse ? 'bg-white/10' : 'bg-[#216c4d]'}`}>
        <TrendingUp size={19} strokeWidth={2.5} />
      </div>
      <span className={`display text-xl font-extrabold ${isInverse ? 'text-white' : 'text-[#18201d]'}`}>
        InvestFlow
      </span>
    </div>
  )
}
