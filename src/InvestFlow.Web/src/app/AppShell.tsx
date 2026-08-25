import { useState } from 'react'
import { Activity, ChartNoAxesCombined, FolderTree, Goal, Landmark, LayoutDashboard, LogOut, Menu, PiggyBank, ReceiptText, Settings } from 'lucide-react'
import { authApi } from '../features/auth/authApi'
import { BudgetPage } from '../features/budget/BudgetPage'
import { CategoriesPage } from '../features/categories/CategoriesPage'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { GoalsPage } from '../features/goals/GoalsPage'
import { InvestmentsPage } from '../features/investments/InvestmentsPage'
import { MonthlyXrayPage } from '../features/monthly-xray/MonthlyXrayPage'
import { NetWorthPage } from '../features/net-worth/NetWorthPage'
import { SettingsPage } from '../features/settings/SettingsPage'
import { TransactionsPage } from '../features/transactions/TransactionsPage'
import { Brand } from '../shared/components/Brand'
import { LanguageSelector } from '../shared/components/LanguageSelector'
import { MonthPicker } from '../shared/components/MonthPicker'
import { Notice } from '../shared/components/Notice'
import { PageHeader } from '../shared/components/PageHeader'
import { getErrorMessage } from '../shared/hooks/useQuery'
import { getCurrentMonth } from '../shared/lib/date'
import { useI18n } from '../shared/i18n/i18n'
import type { Page } from './navigation'

const navigation = [
  { page: 'dashboard', labelKey: 'nav.dashboard', icon: LayoutDashboard },
  { page: 'transactions', labelKey: 'nav.transactions', icon: ReceiptText },
  { page: 'categories', labelKey: 'nav.categories', icon: FolderTree },
  { page: 'budget', labelKey: 'nav.budget', icon: PiggyBank },
  { page: 'investments', labelKey: 'nav.investments', icon: ChartNoAxesCombined },
  { page: 'net-worth', labelKey: 'nav.netWorth', icon: Landmark },
  { page: 'goals', labelKey: 'nav.goals', icon: Goal },
  { page: 'xray', labelKey: 'nav.xray', icon: Activity },
  { page: 'settings', labelKey: 'nav.settings', icon: Settings },
] as const

export function AppShell({ onSignedOut }: { onSignedOut: () => void }) {
  const { t } = useI18n()
  const [page, setPage] = useState<Page>('dashboard')
  const [month, setMonth] = useState(getCurrentMonth())
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [signOutError, setSignOutError] = useState<string | null>(null)

  function navigate(nextPage: Page) {
    setPage(nextPage)
    setIsMenuOpen(false)
  }

  async function handleSignOut() {
    setSignOutError(null)
    try {
      await authApi.logout()
      onSignedOut()
    } catch (error) {
      setSignOutError(getErrorMessage(error))
    }
  }

  return (
    <div className="min-h-screen bg-[#f7f7f2] lg:grid lg:grid-cols-[248px_1fr]">
      <aside className={`fixed inset-y-0 left-0 z-40 flex w-[248px] flex-col border-r border-[#e3e5de] bg-[#fbfbf8] p-4 transition-transform lg:translate-x-0 ${isMenuOpen ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="px-2 py-3"><Brand /></div>
        <nav className="mt-5 flex-1 space-y-1">
          {navigation.map(item => (
            <button key={item.page} type="button" onClick={() => navigate(item.page)} className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${page === item.page ? 'bg-[#e7f1eb] text-[#1c6247]' : 'text-[#58635e] hover:bg-[#f0f1ec] hover:text-[#25312b]'}`}>
              <item.icon size={18} />{t(item.labelKey)}
              {item.page === 'xray' && <span className="ml-auto rounded-full bg-[#d8ebdf] px-2 py-0.5 text-[10px] font-bold">{t('common.new')}</span>}
            </button>
          ))}
        </nav>
        <button type="button" onClick={handleSignOut} className="flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-[#69736e] hover:bg-[#f0f1ec]"><LogOut size={18} /> {t('nav.signOut')}</button>
      </aside>

      {isMenuOpen && <button type="button" aria-label={t('nav.closeMenu')} onClick={() => setIsMenuOpen(false)} className="fixed inset-0 z-30 bg-black/25 lg:hidden" />}

      <main className="min-w-0 lg:col-start-2">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-[#e3e5de]/80 bg-[#f7f7f2]/90 px-4 backdrop-blur sm:px-8">
          <div className="flex items-center gap-3"><button type="button" aria-label={t('nav.openMenu')} onClick={() => setIsMenuOpen(true)} className="btn-secondary !p-2 lg:hidden"><Menu size={18} /></button><span className="hidden text-sm font-semibold text-[#35413b] sm:inline">{t(navigation.find(item => item.page === page)!.labelKey)}</span></div>
          <div className="flex items-center gap-2"><LanguageSelector /><MonthPicker value={month} onChange={setMonth} /></div>
        </header>
        <div className="mx-auto max-w-[1380px] p-4 sm:p-7 lg:p-9">
          <PageHeader page={page} />
          {signOutError && <div className="mb-4"><Notice message={signOutError} /></div>}
          <div key={`${page}-${month}`} className="page-enter"><PageContent page={page} month={month} navigate={navigate} onSignOut={handleSignOut} /></div>
        </div>
      </main>
    </div>
  )
}

function PageContent({ page, month, navigate, onSignOut }: { page: Page; month: string; navigate: (page: Page) => void; onSignOut: () => void }) {
  switch (page) {
    case 'dashboard': return <DashboardPage month={month} navigate={navigate} />
    case 'transactions': return <TransactionsPage month={month} />
    case 'categories': return <CategoriesPage />
    case 'budget': return <BudgetPage month={month} />
    case 'investments': return <InvestmentsPage />
    case 'net-worth': return <NetWorthPage />
    case 'goals': return <GoalsPage />
    case 'xray': return <MonthlyXrayPage month={month} />
    case 'settings': return <SettingsPage onSignOut={onSignOut} />
  }
}
