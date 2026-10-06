import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: { default: "AI CMS", template: "%s · AI CMS" },
  description: "An AI-powered content management system.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <header className="site-header">
          <Link href="/" className="brand">AI CMS</Link>
          <nav>
            <Link href="/">Site</Link>
            <Link href="/admin">Admin</Link>
          </nav>
        </header>
        <div className="container">{children}</div>
      </body>
    </html>
  );
}
