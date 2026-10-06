import "server-only";
import type { ContentItem, ContentListItem, Paged } from "./types";

const API = process.env.API_BASE_URL ?? "http://localhost:5000";

// Public pages are rendered on demand so newly published content shows up immediately.
async function get<T>(path: string): Promise<T | null> {
  const res = await fetch(`${API}${path}`, { cache: "no-store" });
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`API ${path} failed with ${res.status}`);
  return (await res.json()) as T;
}

export async function listPublished(): Promise<ContentListItem[]> {
  const page = await get<Paged<ContentListItem>>("/api/public/content?pageSize=50");
  return page?.items ?? [];
}

export const getPublished = (slug: string) =>
  get<ContentItem>(`/api/public/content/${encodeURIComponent(slug)}`);
