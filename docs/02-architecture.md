# 02 — Architecture

## 1. Big picture

```
┌─────────────────────┐         HTTP JSON          ┌──────────────────────┐
│  React (Vite)       │  ───────────────────────►  │  ASP.NET Core Web API│
│  localhost:5173     │  ◄───────────────────────  │  localhost:5150      │
│                     │         CORS enabled       │                      │
│  Pages + Axios      │                            │  Controllers         │
└─────────────────────┘                            │  EF Core             │
                                                   │  SQLite file         │
                                                   └──────────────────────┘
```

- Frontend **tidak** tulis terus ke database
- Semua change stok melalui API (satu sumber kebenaran)
- Quantity produk hanya berubah dalam endpoint Stock In/Out

## 2. Tech stack (kunci versi)

| Layer | Pilih | Kenapa |
|-------|--------|--------|
| UI | React 18 + Vite | Cepat, standard portfolio |
| Routing | react-router-dom v6 | Multi-page SPA |
| HTTP | axios | Error handling senang |
| CSS | Tailwind CSS v3 | Dashboard nampak kemas tanpa CSS file besar |
| API | ASP.NET Core 10 (`net10.0`) | Sudah terpilih dalam `InventoryApi.csproj` |
| ORM | EF Core + SQLite | Zero SQL Server setup |
| Validation | Data Annotations + manual stock check | Cukup untuk v1 |

**Tidak guna** (v1): Redux, Identity, JWT, SQL Server, Swagger UI wajib (tapi bagus kalau hidup by default).

## 3. Folder yang ANDA cipta (jangan generate auto)

Selepas CLI `dotnet new` dan `npm create vite`, struktur sasaran:

```
InventoryManagement/
├── README.md
├── docs/                          ← sudah ada (planning)
├── backend/
│   ├── InventoryApi.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── inventory.db               ← auto-create bila app jalan
│   ├── Models/
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   └── StockTransaction.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── DTOs/
│   │   ├── ProductDtos.cs
│   │   ├── CategoryDtos.cs
│   │   ├── StockDtos.cs
│   │   └── DashboardDto.cs
│   └── Controllers/
│       ├── ProductsController.cs
│       ├── CategoriesController.cs
│       ├── StockController.cs
│       └── DashboardController.cs
└── frontend/
    ├── package.json
    ├── vite.config.js
    ├── index.html
    ├── src/
    │   ├── main.jsx
    │   ├── index.css
    │   ├── App.jsx
    │   ├── api.js
    │   ├── components/
    │   │   ├── Layout.jsx
    │   │   ├── StatCard.jsx
    │   │   └── StockBadge.jsx
    │   └── pages/
    │       ├── Dashboard.jsx
    │       ├── Products.jsx
    │       ├── Categories.jsx
    │       └── Stock.jsx
    └── tailwind.config.js
```

## 4. Data flow

### Add product

```
Form React → POST /api/products → validate → INSERT Product → 201 + JSON
```

### Stock out

```
Form React → POST /api/stock/out
  → load Product
  → if qty > Quantity → 400
  → else Quantity -= qty, INSERT StockTransaction, SaveChanges
  → 200 + product terbaru
```

### Dashboard

```
GET /api/dashboard → aggregate SQL/LINQ → 4 nombor → StatCard
```

Frontend **jangan** kira Total Stock sendiri dari senarai produk jika dashboard ada endpoint khas — guna `/api/dashboard` supaya formula sama di satu tempat.

## 5. CORS

Vite dev server (`5173`) dan API (`5088`) berbeza origin.

Dalam `Program.cs`:

- Allow origin `http://localhost:5173`
- Allow any header + GET/POST/PUT/DELETE

Production (nanti): same-origin atau senarai domain tetap. v1 = localhost sahaja.

## 6. API conventions

| Item | Rule |
|------|------|
| Base URL | `http://localhost:5150/api` |
| Format | JSON, camelCase (default ASP.NET) |
| Id | integer, route `/api/products/5` |
| Error | `{ "message": "..." }` atau validation `{ "errors": { ... } }` |
| Dates | ISO-8601 UTC |

## 7. Keamanan v1 (jujur untuk portfolio)

- Tiada auth → sesiapa di localhost boleh CRUD
- Input divalidasi (required, range)
- Stock out tidak boleh negatif
- SQL injection: EF Core parameterized (jangan concat SQL)

Sebut dalam README: "Demo app, no auth in v1."

## 8. Kenapa Quantity tidak diedit terus

Jika user boleh taip Quantity = 999 pada Edit Product:

- Tiada audit
- Mudah silap
- Dashboard dan history tidak sepadan

**Rule:** Edit Product = master data. Stock In/Out = perubahan kuantiti.

Stok **awal** semasa Add Product dibenarkan (barang yang sudah ada di rak). Optional: cipta 1 transaction `In` dengan note `"Initial stock"` — v1 **tidak wajib**; Quantity set terus semasa create.
