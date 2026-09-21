# Inventory Management System

Web app for a small store: catalogue, stock in/out, and a dashboard that flags low and empty stock.

**Stack:** React 19 (Vite) · ASP.NET Core 10 Web API · EF Core · SQLite · JWT · Tailwind CSS

## Features

**v1**

- Product and category CRUD
- Search / filter products
- Stock in and stock out, with history
- Stock out rejected when quantity exceeds on-hand
- Low-stock and out-of-stock badges
- Dashboard totals

**v2**

- Login with JWT (`admin` / `Admin123!`)
- Product SKU (unique)
- Product list pagination
- Export products to CSV
- Soft delete (hidden from lists, SKU not reused)
- 7-day stock in/out chart on the dashboard

## Architecture

```
React (localhost:5173)  --JSON + Bearer token-->  ASP.NET API (localhost:5150)  --EF Core-->  inventory.db
```

| Layer | Role |
|-------|------|
| `frontend/` | SPA: pages call REST via Axios |
| `Controllers/` | HTTP endpoints and status codes |
| `DTOs/` | JSON in/out (not the database shape) |
| `Models/` + `AppDbContext` | Tables and rules |

Quantity only changes through `POST /api/stock/in` and `/api/stock/out`.

## Run locally

Needs **.NET 10 SDK** and **Node 20+**.

```bash
# Terminal 1 — API
cd backend
dotnet run

# Terminal 2 — UI
cd frontend
npm install
npm run dev
```

- UI: [http://localhost:5173](http://localhost:5173)
- API: [http://localhost:5150](http://localhost:5150)

**Demo login:** `admin` / `Admin123!`

SQLite file `backend/inventory.db` is created on first API start. It is gitignored. After a schema change (v2 added `Sku`, `IsDeleted`, `Users`), stop the API, delete `inventory.db`, and run again.

## Demo

1. Sign in as `admin`.
2. Dashboard — four cards, 7-day movement chart, attention table.
3. Products — search SKU, paginate, export CSV.
4. Stock — Out with a huge qty → error; In with a valid qty → chart and badges update.
5. Delete a product — it disappears from the list (soft delete).

## API (main)

| Method | Path | Auth | Notes |
|--------|------|------|--------|
| POST | `/api/auth/login` | No | `{ username, password }` → JWT |
| GET | `/api/dashboard` | Yes | Aggregates |
| GET | `/api/dashboard/low-stock` | Yes | Attention list |
| GET | `/api/dashboard/movements` | Yes | Last 7 days in/out |
| GET/POST/PUT/DELETE | `/api/categories` | Yes | `409` if category has products |
| GET | `/api/products` | Yes | `search`, `categoryId`, `stockStatus`, `page`, `pageSize` |
| GET | `/api/products/export` | Yes | CSV download |
| POST/PUT/DELETE | `/api/products` | Yes | Delete is soft |
| POST | `/api/stock/in` · `/out` | Yes | Body: `productId`, `quantity`, `note` |
| GET | `/api/stock` | Yes | History |

## What this project shows

- REST + validation + business rules (not CRUD-only)
- JWT auth on a SPA
- EF Core relations, unique indexes, query filters
- Pagination and file export
- React forms, tables, and derived UI (stock status is not a DB column)

Planning notes live in [`docs/`](docs/).

## License

Personal portfolio project.
