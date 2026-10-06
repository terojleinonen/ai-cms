export type ContentKind = "Post" | "Page";
export type ContentStatus = "Draft" | "Published";

export type ContentListItem = {
  id: string;
  kind: ContentKind;
  title: string;
  slug: string;
  summary: string | null;
  tags: string[];
  status: ContentStatus;
  updatedAt: string;
  publishedAt: string | null;
};

export type ContentItem = ContentListItem & {
  body: string;
  seoTitle: string | null;
  seoDescription: string | null;
  createdAt: string;
};

export type Paged<T> = { items: T[]; page: number; pageSize: number; totalCount: number };

export type SaveContent = {
  kind: ContentKind;
  title: string;
  slug: string | null;
  body: string;
  summary: string | null;
  seoTitle: string | null;
  seoDescription: string | null;
  tags: string[];
};

export type SeoSuggestion = { title: string; description: string; slug: string; tags: string[] };
