function pad(value: number): string {
  return value.toString().padStart(2, '0')
}

export function toLocalDateInput(date = new Date()): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function getCurrentMonth(date = new Date()): string {
  return toLocalDateInput(date).slice(0, 7)
}

export function formatMonth(month: string, locale = 'en-AU'): string {
  const [year, monthNumber] = month.split('-').map(Number)
  return new Date(year, monthNumber - 1, 1).toLocaleDateString(locale, {
    month: 'long',
    year: 'numeric',
  })
}

export function shiftMonth(month: string, offset: number): string {
  const [year, monthNumber] = month.split('-').map(Number)
  return getCurrentMonth(new Date(year, monthNumber - 1 + offset, 1))
}

export function formatShortDate(value: string, locale = 'en-AU'): string {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, month - 1, day).toLocaleDateString(locale, {
    day: 'numeric',
    month: 'short',
  })
}

export function formatMonthYear(value: string, locale = 'en-AU'): string {
  const [year, month] = value.split('-').map(Number)
  return new Date(year, month - 1, 1).toLocaleDateString(locale, {
    month: 'short',
    year: 'numeric',
  })
}
