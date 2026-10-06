import ReactMarkdown from "react-markdown";

// react-markdown does not render raw HTML by default, so editor content cannot inject scripts.
export function Markdown({ children }: { children: string }) {
  return (
    <div className="prose">
      <ReactMarkdown>{children}</ReactMarkdown>
    </div>
  );
}
