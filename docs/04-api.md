# 04 — API Specification

Base URL (dev): `http://localhost:5150/api`

(Port dari `backend/Properties/launchSettings.json` — profile `http`.)

Semua JSON camelCase.

---

## Error shape

**Business / not found**

```json
{ "message": "Insufficient stock. Available: 8" }
```

**Validation (400)** — default ASP.NET:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

Frontend: paparkan `message` jika ada, else gabung `errors`.

---

## Categories

### GET `/api/categories`

Response `200`:

```json
[
  {
    "id": 1,
    "name": "Electronics",
    "description": "Gadgets and accessories",
    "createdAt": "2026-09-12T10:00:00Z",
    "productCount": 2
  }
]
```

### GET `/api/categories/{id}`

- `200` object
- `404` `{ "message": "Category not found" }`

### POST `/api/categories`

Body:

```json
{ "name": "Electronics", "description": "Gadgets and accessories" }
```

- `201` + Location header + object
- `400` name kosong / duplicate name

### PUT `/api/categories/{id}`

Body sama macam POST. `200` object. `404` jika tiada.

### DELETE `/api/categories/{id}`

- `204` no content
- `409` `{ "message": "Cannot delete category that has products" }`
- `404` tidak wujud

---

## Products

### GET `/api/products`

Query (semua optional):

| Param | Contoh | Maksud |
|-------|--------|--------|
| search | `?search=mouse` | Name contains, ignore case |
| categoryId | `?categoryId=1` | Filter kategori |
| stockStatus | `?stockStatus=low` | `in` \| `low` \| `out` |

Response `200`:

```json
[
  {
    "id": 2,
    "name": "Wireless Mouse",
    "categoryId": 1,
    "categoryName": "Electronics",
    "price": 29.00,
    "quantity": 8,
    "minimumStock": 10,
    "stockStatus": "LowStock",
    "createdAt": "2026-09-12T10:00:00Z"
  }
]
```

`stockStatus` values: `"InStock"` | `"LowStock"` | `"OutOfStock"`

### GET `/api/products/{id}`

Sama shape, single object. `404` jika tiada.

### POST `/api/products`

```json
{
  "name": "Wireless Mouse",
  "categoryId": 1,
  "price": 29.00,
  "quantity": 8,
  "minimumStock": 10
}
```

Rules:

- name required, unique
- categoryId mesti wujud
- price >= 0, quantity >= 0, minimumStock >= 0

`201` created object.

### PUT `/api/products/{id}`

```json
{
  "name": "Wireless Mouse",
  "categoryId": 1,
  "price": 32.00,
  "minimumStock": 10
}
```

**Tiada `quantity`.** Quantity hanya berubah via stock endpoints.

`200` / `404` / `400`

### DELETE `/api/products/{id}`

`204` / `404`

---

## Stock

### POST `/api/stock/in`

```json
{
  "productId": 2,
  "quantity": 20,
  "note": "Supplier delivery"
}
```

- `quantity` integer > 0
- `200`:

```json
{
  "transactionId": 15,
  "productId": 2,
  "productName": "Wireless Mouse",
  "type": "In",
  "quantity": 20,
  "balanceAfter": 28,
  "note": "Supplier delivery",
  "createdAt": "2026-09-12T14:00:00Z"
}
```

### POST `/api/stock/out`

Body sama.

- Jika 20 diminta tapi baki 8 → `400`:

```json
{ "message": "Insufficient stock. Available: 8" }
```

- Success: `type: "Out"`, `balanceAfter` = baki baru

### GET `/api/stock?productId=2`

History, terbaru dulu. `productId` optional (kalau tiada = semua).

```json
[
  {
    "id": 15,
    "productId": 2,
    "productName": "Wireless Mouse",
    "type": "In",
    "quantity": 20,
    "note": "Supplier delivery",
    "createdAt": "2026-09-12T14:00:00Z"
  }
]
```

History **tidak** simpan balanceAfter dalam DB (boleh kira, skip v1). Response list tidak wajib ada balanceAfter.

---

## Dashboard

### GET `/api/dashboard`

```json
{
  "totalProducts": 6,
  "totalStock": 271,
  "lowStock": 3,
  "outOfStock": 1
}
```

### GET `/api/dashboard/low-stock`

Senarai produk LowStock + OutOfStock (untuk widget / table kecil di dashboard).

```json
[
  {
    "id": 2,
    "name": "Wireless Mouse",
    "categoryName": "Electronics",
    "quantity": 8,
    "minimumStock": 10,
    "stockStatus": "LowStock"
  }
]
```

Sort: OutOfStock dulu, kemudian LowStock, kemudian name.

---

## Status code ringkas

| Code | Bila |
|------|------|
| 200 | GET/PUT/stock success |
| 201 | POST create |
| 204 | DELETE success |
| 400 | Validation / insufficient stock |
| 404 | Id tidak wujud |
| 409 | Delete category yang ada produk |

## Swagger

.NET 10 template hidupkan OpenAPI JSON di `http://localhost:5150/openapi/v1.json` (bukan `/swagger`). Test request guna `backend/InventoryApi.http` atau browser GET.
