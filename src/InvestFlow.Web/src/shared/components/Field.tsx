import type { ReactNode } from 'react'

interface FieldProps {
  label: string
  children: ReactNode
}

export function Field({ label, children }: FieldProps) {
  return (
    <label>
      <span className="label">{label}</span>
      {children}
    </label>
  )
}
