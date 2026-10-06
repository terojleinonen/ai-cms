import Link from "next/link";
import { listPublished } from "@/lib/server-api";

export const dynamic = "force-dynamic";

export default async function HomePage() {
  let items: Awaited<ReturnType<typeof listPublished>> = [];
  let failed = false;
  try {
    items = await listPublished();
  } catch {
    failed = true;
  }

  return (
    <main>
      <h1>Latest</h1>
      {failed && <p className="error">The content API is currently unreachable.</p>}
      {!failed && items.length === 0 && (
        <p className="muted">Nothing published yet. Create and publish content in the <Link href="/admin">admin</Link>.</p>
      )}
      <ul className="cards">
        {items.map((i) => (
          <li key={i.id} className="card">
            <Link href={`/${i.slug}`}><h2>{i.title}</h2></Link>
            {i.summary && <p>{i.summary}</p>}
            <div className="meta">
              {i.publishedAt && new Date(i.publishedAt).toLocaleDateString("en")}
              {i.tags.map((t) => <span key={t} className="tag">{t}</span>)}
            </div>
          </li>
        ))}
      </ul>
    </main>
  );
}
