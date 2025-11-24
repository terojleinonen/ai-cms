"use client";

import { useEffect, useState } from "react";

type ContentItem = {
  id: string;
  slug: string;
  createdAt: string;
  updatedAt: string;
  isDraft: boolean;
};

export default function AdminDashboard() {
  const [items, setItems] = useState<ContentItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const res = await fetch("http://localhost:5000/api/content");
        if (!res.ok) throw new Error("Failed to fetch content");
        const data = await res.json();
        setItems(data);
      } catch (err: any) {
        setError(err.message ?? "Unknown error");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return (
    <main style={{ padding: "2rem" }}>
      <h1>Admin Dashboard</h1>
      <p>This page lists content items from the C# API.</p>

      {loading && <p>Loading…</p>}
      {error && <p style={{ color: "tomato" }}>Error: {error}</p>}

      {!loading && !error && (
        <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
          <thead>
            <tr>
              <th style={{ textAlign: "left", borderBottom: "1px solid #4b5563" }}>ID</th>
              <th style={{ textAlign: "left", borderBottom: "1px solid #4b5563" }}>Slug</th>
              <th style={{ textAlign: "left", borderBottom: "1px solid #4b5563" }}>Created</th>
              <th style={{ textAlign: "left", borderBottom: "1px solid #4b5563" }}>Updated</th>
              <th style={{ textAlign: "left", borderBottom: "1px solid #4b5563" }}>Draft</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td style={{ padding: "0.25rem 0.5rem" }}>{item.id}</td>
                <td style={{ padding: "0.25rem 0.5rem" }}>{item.slug}</td>
                <td style={{ padding: "0.25rem 0.5rem" }}>{new Date(item.createdAt).toLocaleString()}</td>
                <td style={{ padding: "0.25rem 0.5rem" }}>{new Date(item.updatedAt).toLocaleString()}</td>
                <td style={{ padding: "0.25rem 0.5rem" }}>{item.isDraft ? "Yes" : "No"}</td>
              </tr>
            ))}
            {items.length === 0 && (
              <tr>
                <td colSpan={5} style={{ padding: "0.5rem" }}>
                  No content yet. POST to /api/content to create some items.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </main>
  );
}
