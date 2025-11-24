import React from "react";

export const metadata = {
  title: "AI CMS Template",
  description: "Starter frontend for an AI-powered CMS"
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body style={{ margin: 0, fontFamily: "system-ui, sans-serif", background: "#050816", color: "#f9fafb" }}>
        {children}
      </body>
    </html>
  );
}
