import { Languages } from 'lucide-react'
import { useI18n, type Language } from '../i18n/i18n'

export function LanguageSelector({ showLabel = false }: { showLabel?: boolean }) {
  const { language, setLanguage, t } = useI18n()

  return (
    <label className="flex items-center gap-2">
      {showLabel && <span className="label !mb-0">{t('language.label')}</span>}
      <span className="relative flex items-center">
        <Languages size={15} className="pointer-events-none absolute left-2.5 text-[#527064]" />
        <select
          aria-label={t('language.label')}
          className="field min-w-[104px] py-2 pl-8 pr-7 text-xs font-semibold"
          value={language}
          onChange={event => setLanguage(event.target.value as Language)}
        >
          <option value="pt-BR">PT-BR</option>
          <option value="en">EN</option>
        </select>
      </span>
    </label>
  )
}
