"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ApiError, api } from "@/lib/client-api";
import type { ContentItem, ContentKind, SaveContent } from "@/lib/types";
import { Markdown } from "./Markdown";

type AiAction = "generate" | "rewrite" | "summarize" | "seo";

export function Editor({ initial }: { initial?: ContentItem }) {
  const router = useRouter();
  const [item, setItem] = useState<ContentItem | undefined>(initial);
  const [kind, setKind] = useState<ContentKind>(initial?.kind ?? "Post");
  const [title, setTitle] = useState(initial?.title ?? "");
  const [slug, setSlug] = useState(initial?.slug ?? "");
  const [body, setBody] = useState(initial?.body ?? "");
  const [summary, setSummary] = useState(initial?.summary ?? "");
  const [seoTitle, setSeoTitle] = useState(initial?.seoTitle ?? "");
  const [seoDescription, setSeoDescription] = useState(initial?.seoDescription ?? "");
  const [tags, setTags] = useState((initial?.tags ?? []).join(", "));

  const [preview, setPreview] = useState(false);
  const [busy, setBusy] = useState<AiAction | "save" | "publish" | "delete" | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [notice, setNotice] = useState<string | null>(null);
  const [provider, setProvider] = useState<string | null>(null);
  const [prompt, setPrompt] = useState("");
  const [instruction, setInstruction] = useState("Make it more concise and clear");

  useEffect(() => {
    api.ai.status().then((s) => setProvider(s.provider)).catch(() => setProvider(null));
  }, []);

  const payload = (): SaveContent => ({
    kind,
    title,
    slug: slug.trim() || null,
    body,
    summary: summary.trim() || null,
    seoTitle: seoTitle.trim() || null,
    seoDescription: seoDescription.trim() || null,
    tags: tags.split(",").map((t) => t.trim()).filter(Boolean),
  });

  const fail = (e: unknown) => {
    setNotice(null);
    if (e instanceof ApiError) {
      setError(e.message);
      setFieldErrors(e.fieldErrors);
    } else {
      setError(e instanceof Error ? e.message : "Something went wrong");
    }
  };

  async function run<T>(kind: NonNullable<typeof busy>, fn: () => Promise<T>): Promise<T | undefined> {
    setBusy(kind);
    setError(null);
    setFieldErrors({});
    try {
      return await fn();
    } catch (e) {
      fail(e);
    } finally {
      setBusy(null);
    }
  }

  const adopt = (saved: ContentItem) => {
    setItem(saved);
    setSlug(saved.slug);
  };

  const save = async () => {
    const saved = await run("save", () => (item ? api.update(item.id, payload()) : api.create(payload())));
    if (!saved) return;
    if (!item) {
      router.replace(`/admin/${saved.id}`);
      return;
    }
    adopt(saved);
    setNotice("Saved.");
  };

  const togglePublish = async () => {
    if (!item) return;
    const saved = await run("publish", () => (item.status === "Published" ? api.unpublish(item.id) : api.publish(item.id)));
    if (saved) {
      setItem(saved);
      setNotice(saved.status === "Published" ? "Published." : "Moved back to draft.");
    }
  };

  const remove = async () => {
    if (!item || !window.confirm("Delete this content permanently?")) return;
    const ok = await run("delete", async () => { await api.remove(item.id); return true; });
    if (ok) router.replace("/admin");
  };

  // ---- AI actions: each fills a field; nothing is saved until the editor clicks Save ----
  const aiGenerate = async () => {
    const r = await run("generate", () => api.ai.generate(prompt));
    if (r) {
      setBody(r.text);
      if (!title.trim()) setTitle(r.text.match(/^#\s+(.+)$/m)?.[1] ?? prompt.slice(0, 80));
      setNotice("Draft generated — review before saving.");
    }
  };
  const aiRewrite = async () => {
    const r = await run("rewrite", () => api.ai.rewrite(body, instruction));
    if (r) {
      setBody(r.text);
      setNotice("Body rewritten — review before saving.");
    }
  };
  const aiSummarize = async () => {
    const r = await run("summarize", () => api.ai.summarize(body));
    if (r) setSummary(r.text);
  };
  const aiSeo = async () => {
    const r = await run("seo", () => api.ai.seo(title, body));
    if (!r) return;
    setSeoTitle(r.title);
    setSeoDescription(r.description);
    setTags(r.tags.join(", "));
    if (!item && !slug.trim()) setSlug(r.slug);
    setNotice("SEO metadata suggested — review before saving.");
  };

  const err = (f: string) => fieldErrors[f]?.[0];
  const locked = busy !== null;

  return (
    <div className="editor">
      <section className="editor-main">
        <label>Title
          <input className="input" value={title} onChange={(e) => setTitle(e.target.value)} maxLength={200} />
          {err("title") && <span className="error small">{err("title")}</span>}
        </label>
        <div className="row">
          <label className="grow">Slug
            <input className="input" value={slug} placeholder="auto-generated from title"
              onChange={(e) => setSlug(e.target.value)} />
            {err("slug") && <span className="error small">{err("slug")}</span>}
          </label>
          <label>Type
            <select className="input" value={kind} onChange={(e) => setKind(e.target.value as ContentKind)}>
              <option>Post</option><option>Page</option>
            </select>
          </label>
        </div>

        <div className="row between">
          <span className="label">Body (Markdown)</span>
          <button type="button" className="btn small" onClick={() => setPreview((p) => !p)}>
            {preview ? "Edit" : "Preview"}
          </button>
        </div>
        {preview ? (
          <div className="preview"><Markdown>{body || "*Nothing to preview.*"}</Markdown></div>
        ) : (
          <textarea className="input body" value={body} onChange={(e) => setBody(e.target.value)} rows={18} />
        )}
        {err("body") && <span className="error small">{err("body")}</span>}

        <label>Summary
          <textarea className="input" rows={2} value={summary} onChange={(e) => setSummary(e.target.value)} />
        </label>

        <details>
          <summary>SEO &amp; tags</summary>
          <label>SEO title
            <input className="input" value={seoTitle} onChange={(e) => setSeoTitle(e.target.value)} />
          </label>
          <label>SEO description
            <textarea className="input" rows={2} value={seoDescription} onChange={(e) => setSeoDescription(e.target.value)} />
          </label>
          <label>Tags (comma-separated)
            <input className="input" value={tags} onChange={(e) => setTags(e.target.value)} />
          </label>
        </details>

        {error && <p className="error" role="alert">{error}</p>}
        {notice && <p className="success" role="status">{notice}</p>}

        <div className="row">
          <button className="btn primary" disabled={locked || !title.trim()} onClick={save}>
            {busy === "save" ? "Saving…" : "Save"}
          </button>
          {item && (
            <button className="btn" disabled={locked} onClick={togglePublish}>
              {item.status === "Published" ? "Unpublish" : "Publish"}
            </button>
          )}
          {item?.status === "Published" && <a className="btn" href={`/${item.slug}`} target="_blank" rel="noreferrer">View</a>}
          {item && <button className="btn danger" disabled={locked} onClick={remove}>Delete</button>}
        </div>
      </section>

      <aside className="editor-ai">
        <h2>AI assistant</h2>
        <p className="muted small">
          Provider: {provider ?? "unavailable"}
          {provider === "offline" && " — set ANTHROPIC_API_KEY for real AI"}
        </p>

        <label>Generate a draft
          <textarea className="input" rows={3} value={prompt} placeholder="e.g. A beginner's guide to composting"
            onChange={(e) => setPrompt(e.target.value)} />
        </label>
        <button className="btn" disabled={locked || prompt.trim().length < 2} onClick={aiGenerate}>
          {busy === "generate" ? "Generating…" : "Generate draft"}
        </button>

        <label>Rewrite body
          <input className="input" value={instruction} onChange={(e) => setInstruction(e.target.value)} />
        </label>
        <button className="btn" disabled={locked || !body.trim() || !instruction.trim()} onClick={aiRewrite}>
          {busy === "rewrite" ? "Rewriting…" : "Rewrite"}
        </button>

        <hr />
        <button className="btn" disabled={locked || !body.trim()} onClick={aiSummarize}>
          {busy === "summarize" ? "Summarizing…" : "Write summary"}
        </button>
        <button className="btn" disabled={locked || !title.trim() || !body.trim()} onClick={aiSeo}>
          {busy === "seo" ? "Thinking…" : "Suggest SEO & tags"}
        </button>
      </aside>
    </div>
  );
}
