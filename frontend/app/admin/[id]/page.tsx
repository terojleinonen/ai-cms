"use client";

import { use, useEffect, useState } from "react";
import { Editor } from "@/components/Editor";
import { api } from "@/lib/client-api";
import type { ContentItem } from "@/lib/types";

export default function EditContent({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const [item, setItem] = useState<ContentItem | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get(id).then(setItem).catch((e) => setError(e instanceof Error ? e.message : "Failed to load"));
  }, [id]);

  return (
    <main>
      <h1>Edit content</h1>
      {error && <p className="error">{error}</p>}
      {!item && !error && <p className="muted">Loading…</p>}
      {item && <Editor initial={item} />}
    </main>
  );
}
