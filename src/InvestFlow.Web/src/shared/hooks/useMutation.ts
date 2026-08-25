import { useState } from 'react'
import { getErrorMessage } from './useQuery'

export function useMutation<TInput, TOutput>(
  mutation: (input: TInput) => Promise<TOutput>,
  onSuccess?: (output: TOutput) => void | Promise<void>,
) {
  const [error, setError] = useState<string | null>(null)
  const [isPending, setIsPending] = useState(false)

  async function execute(input: TInput): Promise<boolean> {
    setError(null)
    setIsPending(true)

    try {
      const output = await mutation(input)
      await onSuccess?.(output)
      return true
    } catch (caughtError) {
      setError(getErrorMessage(caughtError))
      return false
    } finally {
      setIsPending(false)
    }
  }

  return { error, execute, isPending, resetError: () => setError(null) }
}
