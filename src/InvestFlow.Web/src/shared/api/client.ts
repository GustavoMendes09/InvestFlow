import type { ZodType } from 'zod'

type RequestOptions = Omit<RequestInit, 'body'> & { body?: unknown }

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

export class ApiContractError extends Error {
  constructor(path: string) {
    super(`The API returned an unexpected response for ${path}.`)
    this.name = 'ApiContractError'
  }
}

async function request<T>(
  path: string,
  schema: ZodType<T>,
  options: RequestOptions = {},
): Promise<T> {
  const response = await fetch(`/api${path}`, {
    credentials: 'include',
    ...options,
    headers: {
      ...(options.body === undefined ? {} : { 'Content-Type': 'application/json' }),
      ...options.headers,
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  const result = schema.safeParse(await response.json())
  if (!result.success) {
    console.error('Invalid API response', { path, issues: result.error.issues })
    throw new ApiContractError(path)
  }

  return result.data
}

async function requestWithoutResponse(path: string, options: RequestOptions): Promise<void> {
  const response = await fetch(`/api${path}`, {
    credentials: 'include',
    ...options,
    headers: {
      ...(options.body === undefined ? {} : { 'Content-Type': 'application/json' }),
      ...options.headers,
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }
}

async function readErrorMessage(response: Response): Promise<string> {
  const body = await response.json().catch(() => null) as {
    detail?: string
    title?: string
    errors?: Record<string, string[]>
  } | null

  const validationMessages = body?.errors
    ? Object.values(body.errors).flat().join(' ')
    : undefined

  return validationMessages ?? body?.detail ?? body?.title ?? 'Something went wrong.'
}

export const apiClient = {
  get: <T>(path: string, schema: ZodType<T>, signal?: AbortSignal) =>
    request(path, schema, { signal }),
  post: <T>(path: string, body: unknown, schema: ZodType<T>) =>
    request(path, schema, { method: 'POST', body }),
  put: <T>(path: string, body: unknown, schema: ZodType<T>) =>
    request(path, schema, { method: 'PUT', body }),
  putWithoutResponse: (path: string, body: unknown) =>
    requestWithoutResponse(path, { method: 'PUT', body }),
  postWithoutResponse: (path: string, body?: unknown) =>
    requestWithoutResponse(path, { method: 'POST', body }),
  delete: (path: string) => requestWithoutResponse(path, { method: 'DELETE' }),
}
