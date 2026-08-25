import { LogOut } from 'lucide-react'
import { Field } from '../../shared/components/Field'
import { LanguageSelector } from '../../shared/components/LanguageSelector'
import { useI18n } from '../../shared/i18n/i18n'

export function SettingsPage({ onSignOut }: { onSignOut: () => void }) {
  const { t } = useI18n()
  return (
    <div className="max-w-2xl space-y-5">
      <section className="card p-6">
        <h2 className="font-bold">{t('settings.preferences')}</h2>
        <div className="mt-5 grid gap-4 sm:grid-cols-2">
          <Field label={t('settings.currency')}><select className="field" disabled><option>{t('settings.aud')}</option></select></Field>
          <Field label={t('settings.weekStart')}><select className="field" defaultValue="monday"><option value="monday">{t('settings.monday')}</option><option value="sunday">{t('settings.sunday')}</option></select></Field>
          <LanguageSelector showLabel />
        </div>
        <p className="mt-4 text-xs text-[#7a847f]">{t('settings.currencyHint')}</p>
      </section>
      <section className="card p-6">
        <h2 className="font-bold">{t('settings.account')}</h2>
        <p className="mt-1 text-sm text-[#69736e]">{t('settings.signOutHint')}</p>
        <button type="button" onClick={onSignOut} className="btn-secondary mt-5"><LogOut size={16} /> {t('nav.signOut')}</button>
      </section>
    </div>
  )
}
