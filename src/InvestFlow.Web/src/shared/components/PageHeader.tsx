import type { Page } from '../../app/navigation'
import { useI18n } from '../i18n/i18n'

export function PageHeader({ page }: { page: Page }) {
  const { t } = useI18n()
  const keys = page === 'net-worth' ? 'netWorth' : page

  return (
    <div className="mb-7">
      <h1 className="display text-2xl font-extrabold sm:text-3xl">{t(`page.${keys}.title` as Parameters<typeof t>[0])}</h1>
      <p className="mt-1.5 text-sm text-[#69736e]">{t(`page.${keys}.subtitle` as Parameters<typeof t>[0])}</p>
    </div>
  )
}
