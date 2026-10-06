import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Markdown } from "@/components/Markdown";
import { getPublished } from "@/lib/server-api";

export const dynamic = "force-dynamic";
type Props = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const item = await getPublished((await params).slug).catch(() => null);
  if (!item) return {};
  return {
    title: item.seoTitle ?? item.title,
    description: item.seoDescription ?? item.summary ?? undefined,
  };
}

export default async function ContentPage({ params }: Props) {
  const item = await getPublished((await params).slug);
  if (!item) notFound();

  return (
    <article>
      <h1>{item.title}</h1>
      <div className="meta">
        {item.publishedAt && new Date(item.publishedAt).toLocaleDateString("en")}
        {item.tags.map((t) => <span key={t} className="tag">{t}</span>)}
      </div>
      <Markdown>{item.body}</Markdown>
    </article>
  );
}
