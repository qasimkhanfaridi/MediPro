/** Resolved URL for API calls (Vite proxy in dev, or `VITE_API_ORIGIN` when set). */
export function apiUrl(path: string): string {
  const origin = (import.meta.env.VITE_API_ORIGIN as string | undefined)?.trim()
  if (!origin) return path.startsWith('/') ? path : `/${path}`
  const base = origin.replace(/\/$/, '')
  return `${base}${path.startsWith('/') ? path : `/${path}`}`
}

/** Attach JWT and perform fetch; paths are same-origin e.g. `/api/...` (Vite proxies to the API in dev). */
export function apiFetch(
  path: string,
  init: RequestInit & { accessToken?: string | null } = {},
): Promise<Response> {
  const { accessToken, headers: inHeaders, ...rest } = init
  const headers = new Headers(inHeaders)
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)
  return fetch(apiUrl(path), { ...rest, headers })
}
export function parseErrorDetail(text: string): string {
  if (!text.startsWith('{')) return text
  try {
    const j = JSON.parse(text) as { message?: string; title?: string; detail?: string }
    if (j.message) return j.message
    if (j.detail) return j.detail
    if (j.title) return j.title
  } catch {
    /* ignore */
  }
  return text
}
