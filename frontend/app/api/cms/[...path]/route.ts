import { NextRequest, NextResponse } from "next/server";

const API = process.env.API_BASE_URL ?? "http://localhost:5000";
const ALLOWED = new Set(["GET", "POST", "PUT", "DELETE"]);

// Authenticated by middleware.ts. Forwards to the admin API and adds the secret key,
// so the key never reaches the browser.
async function proxy(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  if (!ALLOWED.has(req.method)) return new NextResponse(null, { status: 405 });
  const key = process.env.ADMIN_API_KEY;
  if (!key) return NextResponse.json({ title: "ADMIN_API_KEY is not configured." }, { status: 503 });

  const { path } = await params;
  const url = `${API}/api/admin/${path.map(encodeURIComponent).join("/")}${req.nextUrl.search}`;
  const hasBody = req.method === "POST" || req.method === "PUT";

  try {
    const upstream = await fetch(url, {
      method: req.method,
      headers: { "X-Api-Key": key, "Content-Type": "application/json" },
      body: hasBody ? await req.text() || undefined : undefined,
      cache: "no-store",
    });
    return new NextResponse(upstream.status === 204 ? null : upstream.body, {
      status: upstream.status,
      headers: { "Content-Type": upstream.headers.get("content-type") ?? "application/json" },
    });
  } catch {
    return NextResponse.json({ title: "The CMS API is unreachable." }, { status: 502 });
  }
}

export { proxy as GET, proxy as POST, proxy as PUT, proxy as DELETE };
