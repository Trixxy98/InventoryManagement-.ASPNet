# Inventory Management System

Portfolio project: **CRUD + business logic** (stock in/out, low-stock warning, dashboard).

**Tech stack:** React (Vite) + ASP.NET Core Web API + SQLite

> Kod aplikasi **tidak dijana automatik**. Semua planning dan kod rujukan ada dalam folder `docs/`. Anda cipta project sendiri, kemudian **copy-paste** ke editor.

## Cara guna dokumen ini

Baca mengikut susunan:

| # | Fail | Isi |
|---|------|-----|
| 1 | [docs/01-overview.md](docs/01-overview.md) | Matlamat, scope, features, acceptance criteria |
| 2 | [docs/02-architecture.md](docs/02-architecture.md) | Struktur folder, tech stack, data flow |
| 3 | [docs/03-database.md](docs/03-database.md) | ERD, tables, business rules |
| 4 | [docs/04-api.md](docs/04-api.md) | Endpoints, JSON examples, validation |
| 5 | [docs/05-ui.md](docs/05-ui.md) | Pages, wireframe, components, routes |
| 6 | [docs/06-roadmap.md](docs/06-roadmap.md) | Fasa kerja + checklist (mula di sini bila nak code) |
| 7 | [docs/07-paste-backend.md](docs/07-paste-backend.md) | Kod backend — paste manual |
| 8 | [docs/08-paste-frontend.md](docs/08-paste-frontend.md) | Kod frontend — paste manual |

## Features (v1)

- Add / Edit / Delete / View products
- Search product
- Category
- Stock in / Stock out
- Low-stock warning
- Dashboard (Total Products, Total Stock, Low Stock, Out of Stock)

## Quick start (selepas anda paste kod)

```bash
# Terminal 1 — API
cd backend
dotnet run

# Terminal 2 — UI
cd frontend
npm install
npm run dev
```

- API: `http://localhost:5150`
- UI: `http://localhost:5173` (Fasa 3, belum)
