interface NoticeProps {
  message: string
  tone?: 'error' | 'success'
}

const toneClasses = {
  error: 'border-[#f0c9c2] bg-[#fff2ef] text-[#a9493c]',
  success: 'border-[#b9ddca] bg-[#effaf4] text-[#216c4d]',
}

export function Notice({ message, tone = 'error' }: NoticeProps) {
  return (
    <div
      role={tone === 'error' ? 'alert' : 'status'}
      className={`rounded-xl border px-4 py-3 text-sm ${toneClasses[tone]}`}
    >
      {message}
    </div>
  )
}
