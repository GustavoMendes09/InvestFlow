import { describe, expect, it } from 'vitest'
import { formatMonth, getCurrentMonth, shiftMonth, toLocalDateInput } from './date'

describe('date utilities', () => {
  it('formats dates using local calendar fields instead of UTC', () => {
    const localDate = new Date(2026, 7, 25, 0, 30)

    expect(toLocalDateInput(localDate)).toBe('2026-08-25')
    expect(getCurrentMonth(localDate)).toBe('2026-08')
  })

  it('moves safely between calendar years', () => {
    expect(shiftMonth('2026-01', -1)).toBe('2025-12')
    expect(shiftMonth('2026-12', 1)).toBe('2027-01')
  })

  it('formats a selected month for display', () => {
    expect(formatMonth('2026-08')).toContain('2026')
  })
})
