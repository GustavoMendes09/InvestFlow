import type { ReactNode } from 'react'
import { Brand } from '../../shared/components/Brand'
import { LanguageSelector } from '../../shared/components/LanguageSelector'
import { useI18n } from '../../shared/i18n/i18n'

interface AuthLayoutProps {
  eyebrow: string
  title: string
  description: string
  children: ReactNode
}

export function AuthLayout({ eyebrow, title, description, children }: AuthLayoutProps) {
  return (
    <main className="min-h-screen bg-[#f7f7f2] p-4 sm:p-8">
      <div className="mx-auto grid min-h-[calc(100vh-4rem)] max-w-6xl overflow-hidden rounded-[2rem] border border-[#e1e3da] bg-white shadow-[0_30px_80px_rgba(35,52,43,.09)] lg:grid-cols-[1.08fr_.92fr]">
        <AuthIntroduction />
        <section className="relative flex items-center justify-center p-6 sm:p-12">
          <div className="absolute right-5 top-5"><LanguageSelector /></div>
          <div className="w-full max-w-md">
            <div className="mb-10 lg:hidden"><Brand /></div>
            <p className="text-sm font-semibold text-[#216c4d]">{eyebrow}</p>
            <h1 className="display mt-2 text-3xl font-extrabold">{title}</h1>
            <p className="mt-2 text-sm text-[#69736e]">{description}</p>
            {children}
          </div>
        </section>
      </div>
    </main>
  )
}

function AuthIntroduction() {
  const { t } = useI18n()
  return (
    <section className="relative hidden overflow-hidden bg-[#173f30] p-12 text-white lg:flex lg:flex-col lg:justify-between">
      <div className="absolute -right-24 -top-24 size-80 rounded-full border border-white/10" />
      <div className="absolute -right-10 -top-10 size-52 rounded-full border border-white/10" />
      <Brand tone="inverse" />
      <div className="relative max-w-lg">
        <span className="mb-6 inline-flex rounded-full border border-white/15 bg-white/10 px-3 py-1 text-xs font-semibold uppercase tracking-[.16em] text-[#bde4d1]">
          {t('auth.clarity')}
        </span>
        <h2 className="display text-5xl font-extrabold leading-[1.06]">
          {t('auth.heroTitle')}
        </h2>
        <p className="mt-6 max-w-md text-lg leading-relaxed text-[#c8dbd3]">
          {t('auth.heroDescription')}
        </p>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <MiniStat label={t('auth.income')} value={t('auth.incomeValue')} />
        <MiniStat label={t('auth.spending')} value={t('auth.spendingValue')} />
        <MiniStat label={t('auth.goals')} value={t('auth.goalsValue')} />
      </div>
    </section>
  )
}

function MiniStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/[.07] p-4">
      <div className="text-xs text-[#9db8ad]">{label}</div>
      <div className="mt-1 font-semibold">{value}</div>
    </div>
  )
}
