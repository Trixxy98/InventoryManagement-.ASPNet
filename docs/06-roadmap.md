# 06 — Implementation Roadmap

Ikut fasa. Jangan buat UI cantik sebelum API stock-out betul.

Anda **cipta fail sendiri**, kemudian paste dari `07` dan `08`.

---

## Fasa 0 — Tooling (30 min)

Checklist:

- [x] Install [.NET 8 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` mula dengan `8.` atau `10.`)
- [x] Install Node.js 20+ (`node -v`)
- [x] VS / VS Code / Cursor — extension C# dan ES7 optional

---

## Fasa 1 — Backend skeleton (45 min)

Di root repo:

Anda **sudah** buat `dotnet new` + EF packages. Skip command di atas.

Checklist:

- [x] Buang `WeatherForecast.cs` + `WeatherForecastController.cs`
- [x] Cipta folder `Models/`, `Data/`, `DTOs/`
- [x] Paste **Fail 1–7 sahaja** dari [07-paste-backend.md](07-paste-backend.md)
- [x] `dotnet run` → listening `http://localhost:5150`
- [x] `inventory.db` muncul

**Fasa 1 siap.** Teruskan Fasa 2 (Fail 8–17). Jangan mula React.

Nota: .NET 10 tiada Swagger UI by default. OpenAPI JSON: `http://localhost:5150/openapi/v1.json`. Test endpoint nanti guna `InventoryApi.http`.

---



## Fasa 2 — API lengkap (1–2 jam)

Paste controllers + DTOs. Test di Swagger ikut order:

1. GET `/api/dashboard` — nombor sepadan seed
2. POST product baru
3. PUT product (quantity **tidak** berubah)
4. POST `/api/stock/in` — quantity naik
5. POST `/api/stock/out` quantity besar — **400**
6. POST `/api/stock/out` quantity sah — quantity turun
7. GET `/api/products?search=mouse`
8. DELETE category yang ada produk — **409**
9. DELETE product — 204, hilang dari list

Checklist:

- [ ] Semua endpoint dalam [04-api.md](04-api.md) hidup
- [ ] Insufficient stock berjaya ditolak

---



## Fasa 3 — Frontend skeleton (45 min)

Dari root:

```bash
npm create vite@latest frontend -- --template react
cd frontend
npm install
npm install axios react-router-dom
npm install -D tailwindcss@3 postcss autoprefixer
npx tailwindcss init -p
```

Paste `index.css`, `tailwind.config.js`, `App.jsx`, `Layout.jsx`.

Checklist:

- [ ] `npm run dev` → sidebar nampak
- [ ] Navigate 4 page (boleh masih kosong)

---



## Fasa 4 — Wire pages (2–3 jam)

Order page (mudah → susah):

1. Categories CRUD
2. Products list + search + add/edit/delete
3. Stock in/out + history
4. Dashboard kad + attention table

Checklist:

- [ ] CRUD produk end-to-end
- [ ] Badge low / out
- [ ] Stock out error papar di UI
- [ ] Dashboard match API

---



## Fasa 5 — Polish portfolio (1 jam)

- [ ] Seed / data cukup untuk screenshot cantik
- [ ] README root: screenshot, cara run, tech stack, apa yang anda belajar
- [ ] `.gitignore`: `bin/`, `obj/`, `node_modules/`, `inventory.db`
- [ ] Demo script 60 saat (bawah)

---



## Urutan belajar (kenapa begini)

```
Model + DbContext     → nampak table
CRUD Category         → CRUD paling mudah
CRUD Product          → FK + unique name
Stock in/out          → transaction + rule
Dashboard             → aggregate
React list            → consume GET
React form            → consume POST/PUT
Search                → query string
Badge + warna         → derived status
```

---



## Demo script (60 saat)

1. Buka Dashboard — tunjuk 4 kad
2. Products — filter Low stock
3. Stock Out produk low — cuba quantity gila — error
4. Stock In — quantity naik, badge jadi In stock
5. Balik Dashboard — Low Stock berkurang
6. (Optional) Add product + search nama

Rakam GIF atau 3 screenshot: Dashboard, Products table, Stock form.

---



## Definition of done (v1)

- [ ] Semua feature dalam [01-overview.md](01-overview.md) section 3
- [ ] Backend boleh run tanpa frontend
- [ ] Frontend boleh run jika API hidup
- [ ] Tiada WeatherForecast tinggal
- [ ] Satu README yang orang lain boleh follow

---



## Halangan biasa


| Masalah               | Check                                                              |
| --------------------- | ------------------------------------------------------------------ |
| CORS error di browser | `Program.cs` origin `http://localhost:5173`                        |
| Port API              | `launchSettings.json` = **5150**. Samakan `api.js` nanti           |
| DB tak seed           | Delete `inventory.db`, run semula (EnsureCreated + seed)           |
| Tailwind tak apply    | `content` dalam `tailwind.config.js` include `./src/**/*.{js,jsx}` |
| PUT quantity berubah  | Pastikan DTO update tiada Quantity                                 |


---



## v2 (nanti, jangan sekarang)

- Login (ASP.NET Identity + JWT)
- Pagination
- SKU
- Export CSV
- Soft delete
- Chart stok 7 hari

