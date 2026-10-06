import type { ContentItem, ContentListItem, Paged, SaveContent, SeoSuggestion } from "./types";

export class ApiError extends Error {
  constructor(message: string, public status: number, public fieldErrors: Record<string, string[]> = {}) {
    super(message);
  }
}

// All browser calls go through the Next.js proxy, which attaches the API key server-side.
async function call<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`/api/cms/${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });
  if (res.status === 204) return undefined as T;
  const data = await res.json().catch(() => null);
  if (!res.ok) {
    throw new ApiError(data?.title ?? `Request failed (${res.status})`, res.status, data?.errors ?? {});
  }
  return data as T;
}

const post = <T>(path: string, body?: unknown) =>
  call<T>(path, { method: "POST", body: body === undefined ? undefined : JSON.stringify(body) });

export const api = {
  list: (search = "") => call<Paged<ContentListItem>>(`content?pageSize=100&search=${encodeURIComponent(search)}`),
  get: (id: string) => call<ContentItem>(`content/${id}`),
  create: (b: SaveContent) => post<ContentItem>("content", b),
  update: (id: string, b: SaveContent) =>
    call<ContentItem>(`content/${id}`, { method: "PUT", body: JSON.stringify(b) }),
  publish: (id: string) => post<ContentItem>(`content/${id}/publish`),
  unpublish: (id: string) => post<ContentItem>(`content/${id}/unpublish`),
  remove: (id: string) => call<void>(`content/${id}`, { method: "DELETE" }),
  ai: {
    status: () => call<{ provider: string }>("ai/status"),
    generate: (prompt: string) => post<{ text: string }>("ai/generate", { prompt }),
    rewrite: (text: string, instruction: string) => post<{ text: string }>("ai/rewrite", { text, instruction }),
    summarize: (text: string) => post<{ text: string }>("ai/summarize", { text }),
    seo: (title: string, body: string) => post<SeoSuggestion>("ai/seo", { title, body }),
  },
};
