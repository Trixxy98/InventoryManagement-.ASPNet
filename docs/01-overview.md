# 01 — Project Overview

## 1. Apa yang kita bina

**Inventory Management System (IMS)** — aplikasi web untuk staf stor/kedai:

1. Daftar produk (nama, kategori, harga, kuantiti, minimum stock)
2. Kemaskini / padam produk
3. Cari produk
4. **Stock in** (barang masuk) dan **stock out** (barang keluar)
5. Amaran stok rendah
6. Dashboard ringkas (nombor penting dalam 4 kad)

Ini sesuai untuk portfolio sebab ia bukan CRUD kosong: ada **business rules** (tak boleh stock out lebih dari baki, low stock vs out of stock).

## 2. Persona

| Peranan | Keperluan |
|---------|-----------|
| Store admin (satu user, v1 tiada login) | Urus katalog, masuk/keluar stok, nampak produk yang hampir habis |

v1 **tiada authentication**. Tambah login kemudian (lihat Out of scope).

## 3. Features — acceptance criteria

### 3.1 Products — View

- Senarai semua produk dalam table
- Column: Name, Category, Price, Quantity, MinimumStock, status stok, CreatedAt
- Status stok:
  - **In stock** — `Quantity > MinimumStock`
  - **Low stock** — `Quantity > 0` DAN `Quantity <= MinimumStock`
  - **Out of stock** — `Quantity == 0`

### 3.2 Products — Add

- Form: Name, Category (dropdown), Price, Quantity (stok awal), MinimumStock
- Name wajib, unique (case-insensitive)
- Price >= 0, Quantity >= 0, MinimumStock >= 0
- CreatedAt diisi server (UTC)

### 3.3 Products — Edit

- Semua field kecuali Quantity **boleh** edit dari form produk
- Quantity **tidak** diubah dari Edit Product — hanya melalui Stock In / Stock Out (supaya ada jejak pergerakan)

### 3.4 Products — Delete

- Confirm sebelum padam
- Padam produk → padam juga history stock untuk produk itu (cascade)

### 3.5 Search

- Cari by **nama** (contains, case-insensitive)
- Filter by **category** (optional)
- Boleh gabung search + category

### 3.6 Category

- CRUD kategori: Name, Description (optional)
- Tidak boleh padam kategori yang masih ada produk
- Product mesti pilih Category yang wujud

### 3.7 Stock in

- Pilih produk, masukkan quantity (> 0), note optional
- `Product.Quantity += quantity`
- Rekod dalam `StockTransactions` type = `In`

### 3.8 Stock out

- Pilih produk, quantity (> 0), note optional
- Jika `quantity > Product.Quantity` → **tolak**, mesej "Insufficient stock"
- Jika cukup: `Product.Quantity -= quantity`, rekod type = `Out`

### 3.9 Low-stock warning

- Badge pada row produk
- Dashboard kad **Low Stock**
- Optional: highlight row (kuning / merah)

### 3.10 Dashboard

| Kad | Formula |
|-----|---------|
| Total Products | `COUNT(products)` |
| Total Stock | `SUM(products.Quantity)` |
| Low Stock | `COUNT` di mana `Quantity > 0 AND Quantity <= MinimumStock` |
| Out of Stock | `COUNT` di mana `Quantity == 0` |

Contoh target UI:

```
Total Products: 120
Total Stock:    3,450
Low Stock:      8
Out of Stock:   2
```

## 4. In scope (v1)

- SPA React + REST API
- SQLite (mudah, satu fail `.db`)
- 3 table: Categories, Products, StockTransactions
- Seed data supaya dashboard tidak kosong pada first run

## 5. Out of scope (v1) — elak feature creep

- Login / JWT / roles
- Multi-warehouse / location
- Purchase order, supplier, barcode
- Export Excel/PDF
- Pagination besar-besaran (list boleh load semua; 100–200 row OK)
- Image upload
- Soft delete
- Unit tests (boleh tambah kemudian)

Simpan idea ini untuk **v2** dalam README portfolio.

## 6. Entity utama (seperti contoh anda)

```
Products
---------
Id
Name
Category          → dalam DB ini jadi CategoryId (FK)
Price
Quantity
MinimumStock
CreatedAt
```

Tambahan untuk business logic:

```
Categories          — elak typo "Elecronic" vs "Electronic"
StockTransactions   — jejak setiap in/out (kenapa quantity berubah)
```

## 7. Kenapa sesuai portfolio

| Skill | Di mana nampak |
|-------|----------------|
| REST API | Controllers, status code, validation |
| ORM | EF Core, relations, cascade |
| Business logic | Stock out guard, derived stock status |
| React | Forms, table, search, dashboard state |
| UX | Badge, confirm delete, error dari API |

Bila demo: tunjuk **stock out melebihi baki gagal**, kemudian stock in, kemudian dashboard Low Stock bertukar.
