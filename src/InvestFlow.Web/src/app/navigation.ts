export type Page =
  | 'dashboard'
  | 'transactions'
  | 'categories'
  | 'budget'
  | 'investments'
  | 'net-worth'
  | 'goals'
  | 'xray'
  | 'settings'

export const pageTitles: Record<Page, { title: string; subtitle: string }> = {
  dashboard: {
    title: 'Your money, in focus',
    subtitle: 'A clear view of what came in, went out, and moved you forward.',
  },
  transactions: { title: 'Transactions', subtitle: 'Keep every dollar accounted for.' },
  categories: { title: 'Categories', subtitle: 'Organise spending in a way that makes sense to you.' },
  budget: { title: 'Monthly budget', subtitle: 'Give each category a clear, flexible limit.' },
  investments: { title: 'Investments', subtitle: 'Track contributions and current values in one place.' },
  'net-worth': { title: 'Net worth', subtitle: 'Everything you own, minus everything you owe.' },
  goals: { title: 'Financial goals', subtitle: 'Turn the next milestone into visible progress.' },
  xray: { title: 'Monthly X-Ray', subtitle: 'The story behind this month’s numbers.' },
  settings: { title: 'Settings', subtitle: 'Your account and preferences.' },
}
