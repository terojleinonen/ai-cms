# AI CMS

A small, production-minded content management system with AI writing tools built in. Editors draft, rewrite, summarize and SEO-optimize content with Claude, then publish it to a public site.

**Stack:** ASP.NET Core (.NET 10) · EF Core (SQLite / PostgreSQL) · Next.js 15 (App Router) · TypeScript · Anthropic Messages API · Docker · GitHub Actions

## Features

- **Content management** – posts and pages with Markdown bodies, tags, SEO fields, unique slugs, draft/publish workflow, search and pagination.
- **AI assistant in the editor** – generate a draft from a brief, rewrite with a free-form instruction, write a summary, and suggest SEO title/description/slug/tags. AI output only fills form fields; nothing is saved until the editor clicks *Save*.
- **Graceful AI fallback** – with no API key the app uses a deterministic offline provider, so demos, tests and CI work without network access or cost.
- **Public site** – server-rendered list and article pages with per-page SEO metadata. Markdown is rendered without raw HTML, so editor content cannot inject scripts.
- **Secure by default** – admin and AI routes require an API key (constant-time comparison); the admin UI sits behind a password; the API key stays on the Next.js server and never reaches the browser; AI endpoints are rate-limited; provider errors are logged server-side, not leaked to clients.

## Architecture

```
Browser ──► Next.js (UI, auth middleware, /api/cms proxy) ──► ASP.NET Core API ──► SQLite / PostgreSQL
                                                                   │
                                                                   └──► Anthropic API (or offline provider)
```

```
backend/src/
  Cms.Domain/          entities (ContentItem)
  Cms.Application/     DTOs, validation, service + AI interfaces, slug logic
  Cms.Infrastructure/  EF Core DbContext, ContentService, provider selection
  Cms.Ai/              AnthropicAiTextService, OfflineAiTextService
  Cms.Api/             minimal-API endpoints, auth filter, rate limiting, ProblemDetails
backend/tests/         unit + integration tests (xUnit, WebApplicationFactory)
frontend/              Next.js app: public site, admin dashboard, editor
```

## Quick start

### With Docker (PostgreSQL + API + web)

```bash
cp .env.example .env        # set the secrets; add ANTHROPIC_API_KEY for real AI
docker compose up --build
```

Site: <http://localhost:3000> · Admin: <http://localhost:3000/admin> (any username, `ADMIN_PASSWORD`) · API docs are available at `/swagger` when `ASPNETCORE_ENVIRONMENT=Development`.

### Local development

Requires the .NET 10 SDK and Node 22+.

```bash
# API  (http://localhost:5000, SQLite file ./cms.db, admin key "dev-admin-key" in Development)
cd backend/src/Cms.Api
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5000 dotnet run
# optional real AI: dotnet user-secrets set Anthropic:ApiKey sk-ant-...   (or env var Anthropic__ApiKey)

# Web  (http://localhost:3000)
cd frontend
npm install
API_BASE_URL=http://localhost:5000 ADMIN_API_KEY=dev-admin-key ADMIN_PASSWORD=change-me npm run dev
```

### Tests

```bash
cd backend && dotnet test          # 23 unit + integration tests
cd frontend && npm run typecheck && npm run build

# end-to-end smoke test against a running stack (what CI runs after `docker compose up`)
ADMIN_PASSWORD=<your password> scripts/smoke-test.sh
```

## Configuration

| Setting | Where | Purpose |
|---|---|---|
| `Auth__AdminApiKey` | API | Required for `/api/admin/**`. Unset ⇒ admin routes return 503. |
| `Anthropic__ApiKey` / `Anthropic__Model` | API | Enables Claude; empty ⇒ offline provider. |
| `Database__Provider` | API | `Sqlite` (default) or `Postgres`. |
| `ConnectionStrings__Cms` | API | Connection string for the chosen provider. |
| `Cors__AllowedOrigins__0` | API | Only needed if browsers call the API directly. |
| `API_BASE_URL`, `ADMIN_API_KEY`, `ADMIN_PASSWORD` | Web | See `.env.example`. |

## API overview

| Route | Auth | Description |
|---|---|---|
| `GET /api/public/content`, `GET /api/public/content/{slug}` | none | Published content only |
| `GET/POST /api/admin/content`, `GET/PUT/DELETE /api/admin/content/{id}` | `X-Api-Key` | CRUD |
| `POST /api/admin/content/{id}/publish` · `/unpublish` | `X-Api-Key` | Workflow |
| `POST /api/admin/ai/generate` · `rewrite` · `summarize` · `seo`, `GET …/ai/status` | `X-Api-Key` | AI tools (30 req/min/IP) |
| `GET /health` | none | Liveness + DB check |

Errors use RFC 7807 problem details (`400` validation, `401`, `404`, `409` duplicate slug, `429`, `502` AI provider failure).

## Known limitations / next steps

- **Schema management:** the schema is created with `EnsureCreated` on startup. Before evolving the schema in production, switch to EF Core migrations.
- **Single shared admin identity:** one API key and one password, with no per-user accounts or roles. Swap in ASP.NET Identity or an OIDC provider for multi-user use.
- **Rate limiting is in-process** and per client IP; behind a proxy, configure forwarded headers or use a distributed limiter.
- **Unit/integration tests run on SQLite;** PostgreSQL and the containers are covered by the CI compose smoke test (`scripts/smoke-test.sh`).
- Media uploads, content versioning and AI image generation are not implemented.

## License

MIT
