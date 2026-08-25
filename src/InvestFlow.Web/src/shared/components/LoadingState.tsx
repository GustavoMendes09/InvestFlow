import { useI18n } from '../i18n/i18n'

export function LoadingState() {
  const { t } = useI18n()
  return (
    <div className="card flex h-64 items-center justify-center text-sm text-[#7a847f]">
      {t('loading.financialPicture')}
    </div>
  )
}
