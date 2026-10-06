"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { api } from "@/lib/client-api";
import type { ContentListItem } from "@/lib/types";

export default function AdminDashboard() {
  const [items, setItems] = useState<ContentListItem[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async (term: string) => {
    setLoading(true);
    try {
      setItems((await api.list(term)).items);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const t = setTimeout(() => load(search), 250);
    return () => clearTimeout(t);
  }, [search, load]);

  return (
    <main>
      <div className="row between">
        <h1>Content</h1>
        <Link className="btn primary" href="/admin/new">New content</Link>
      </div>
      <input
        className="input"
        placeholder="Search by title or slug…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        aria-label="Search content"
      />
      {error && <p className="error">{error}</p>}
      {loading && <p className="muted">Loading…</p>}
      {!loading && !error && items.length === 0 && <p className="muted">No content found.</p>}
      <table className="table">
        <thead>
          <tr><th>Title</th><th>Type</th><th>Status</th><th>Updated</th></tr>
        </thead>
        <tbody>
          {items.map((i) => (
            <tr key={i.id}>
              <td><Link href={`/admin/${i.id}`}>{i.title}</Link><div className="muted small">/{i.slug}</div></td>
              <td>{i.kind}</td>
              <td><span className={`badge ${i.status.toLowerCase()}`}>{i.status}</span></td>
              <td>{new Date(i.updatedAt).toLocaleString("en")}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  );
}
