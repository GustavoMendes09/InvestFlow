import type { Dashboard } from '../../shared/api/schemas'

export function NetWorthChart({ history }: { history: Dashboard['history'] }) {
  if (history.length < 2) {
    return (
      <div className="mt-8 flex h-36 items-end rounded-xl bg-[#f6f7f3] p-5 text-sm text-[#7a847f]">
        Your trend will appear as monthly snapshots build.
      </div>
    )
  }

  const values = history.map(item => item.netWorth)
  const minimum = Math.min(...values)
  const maximum = Math.max(...values)
  const spread = maximum - minimum || 1
  const points = values
    .map((value, index) => `${index / (values.length - 1) * 100},${90 - (value - minimum) / spread * 75}`)
    .join(' ')

  return (
    <div className="mt-6 h-40 w-full">
      <svg viewBox="0 0 100 100" preserveAspectRatio="none" className="h-full w-full overflow-visible">
        <defs>
          <linearGradient id="net-worth-fill" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#4b9b78" stopOpacity=".25" />
            <stop offset="100%" stopColor="#4b9b78" stopOpacity="0" />
          </linearGradient>
        </defs>
        <polygon points={`0,100 ${points} 100,100`} fill="url(#net-worth-fill)" />
        <polyline
          points={points}
          fill="none"
          stroke="#287253"
          strokeWidth="2"
          vectorEffect="non-scaling-stroke"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
    </div>
  )
}
