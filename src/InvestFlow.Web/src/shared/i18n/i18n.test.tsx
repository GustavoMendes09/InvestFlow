import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, expect, it } from 'vitest'
import { LanguageSelector } from '../components/LanguageSelector'
import { I18nProvider, useI18n } from './i18n'

function Example() {
  const { t } = useI18n()
  return <><LanguageSelector /><p>{t('nav.transactions')}</p></>
}

beforeEach(() => window.localStorage.clear())

it('switches to Brazilian Portuguese and persists the preference', async () => {
  const user = userEvent.setup()
  render(<I18nProvider><Example /></I18nProvider>)

  await user.selectOptions(screen.getByRole('combobox'), 'pt-BR')

  expect(screen.getByText('Transações')).toBeInTheDocument()
  expect(window.localStorage.getItem('investflow-language')).toBe('pt-BR')
  expect(document.documentElement.lang).toBe('pt-BR')
})
