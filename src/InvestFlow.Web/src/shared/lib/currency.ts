export const formatMoney = (value: number, locale = 'en-AU') => new Intl.NumberFormat(locale, {
  style: 'currency', currency: 'AUD', maximumFractionDigits: 0,
}).format(value)

export const formatMoneyExact = (value: number, locale = 'en-AU') => new Intl.NumberFormat(locale, {
  style: 'currency', currency: 'AUD',
}).format(value)
