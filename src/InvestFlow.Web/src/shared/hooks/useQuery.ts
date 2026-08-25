import { useCallback, useEffect, useEffectEvent, useState } from 'react'

interface QueryState<T> {
  data: T | null
  error: string | null
  isLoading: boolean
}

export function useQuery<T>(
  queryKey: string,
  fetcher: (signal: AbortSignal) => Promise<T>,
) {
  const [revision, setRevision] = useState(0)
  const [state, setState] = useState<QueryState<T>>({
    data: null,
    error: null,
    isLoading: true,
  })

  const load = useEffectEvent(fetcher)

  useEffect(() => {
    const controller = new AbortController()

    load(controller.signal)
      .then(data => setState({ data, error: null, isLoading: false }))
      .catch(error => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setState(previous => ({
          ...previous,
          error: getErrorMessage(error),
          isLoading: false,
        }))
      })

    return () => controller.abort()
  }, [queryKey, revision])

  const refetch = useCallback(() => setRevision(current => current + 1), [])
  return { ...state, refetch }
}

export function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong.'
}
