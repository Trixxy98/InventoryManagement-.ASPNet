import {useEffect, useState} from 'react';
import api from '../api';

function getErrorMessage(err) {
    const data = err.response?.data
    if (data?.message) return data.message
    if (data?.errors) return Object.values(data.errors).flat().join(' ')
    return 'Cannot reach API. Is backend running on :5150?'
}

function StockBadge({status}) {
    const map = {
        InStock: 'bg-emerald-50 text-emerald-700',
        LowStock: 'bg-amber-50 text-amber-800',
        OutOfStock: 'bg-rose-50 text-rose-700',
    }
    const label = {
        InStock: 'In Stock',
        LowStock: 'Low Stock',
        OutOfStock: 'Out of stock',
    }
    return (
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${map[status] || 'bg-slate-100'}`}>
            {label[status] || status}
        </span>
    )
}

const emptyForm = {name: '', categoryId: '', price: '', quantity: '', minimumStock: '',}

export default function Products() {
    const [items, setItems] = useState([])
    const [categories, setCategories] = useState([])
    const [error, setError] = useState('')
    const [search, setSearch] = useState('')
    const [categoryId, setCategoryId] = useState('')
    const [stockStatus, setStockStatus] = useState('')
    const [open, setOpen] = useState(false)
    const [editing, setEditing] = useState(null)
    const [form, setForm] = useState(emptyForm)
    const [saving, setSaving] = useState(false)

    async function load() {
        try {
            const params = {}
            if (search.trim()) params.search = search.trim()
            if (categoryId) params.categoryId = categoryId.trim()
            if (stockStatus) params.stockStatus = stockStatus
            const {data} = await api.get('/products', {params})
            setItems(data)
            setError('')
        } catch (err) {
            setError(getErrorMessage(err))
        }
    }

    useEffect(() => {
        api.get('/categories').then(({data}) => setCategories(data)).catch(() => {})
    }, [])

    useEffect(() => {
        load()
    }, [search, categoryId, stockStatus])

    function openCreate() {
        setEditing(null)
        setForm(emptyForm)
        setOpen(true)
    }

    function openEdit(item) {
        setEditing(item)
        setForm({
            name: item.name,
            categoryId: String(item.categoryId),
            price: String(item.price),
            quantity: String(item.quantity),
            minimumStock: String(item.minimumStock),
        })
        setOpen(true)
    }

    async function save(e) {
        e.preventDefault()
        setSaving(true)
        try {
            const body = {
                name: form.name.trim(),
                categoryId: Number(form.categoryId),
                price: Number(form.price),
                minimumStock: Number(form.minimumStock),
            }
            if (editing) {
                await api.put(`/products/${editing.id}`, body)
            } else {
                await api.post('/products', {...body, quantity: Number(form.quantity)})
            }
            setOpen(false)
            await load()
        } catch (err) {
            alert(getErrorMessage(err))
        } finally {
            setSaving(false)
        }
    }

    async function remove(item) {
        if (!window.confirm(`Delete "${item.name}"? This cannot be undone.`)) return
        try {
            await api.delete(`/products/${item.id}`)
            await load()
        } catch (err) {
            alert(getErrorMessage(err))
        }
    }

    function rowClass(status) {
        if (status === 'OutOfStock') return 'border-t border-slate-100 bg-rose-50/60'
        if (status === 'LowStock') return 'border-t border-slate-100 bg-amber-50/60'
        return 'border-t border-slate-100'
    }

    return (
        <div>
            <div className="mb-6 flex items-center justify-between">
                <h1 className="text-2xl font-semibold">Products</h1>
                <button
                 type="button"
                 onClick={openCreate}
                 className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500">
                    + Add Product
                </button>
            </div>

            <div>
                <input
                 value={search}
                 onChange={(e) => setSearch(e.target.value)}
                 placeholder="Search name..."
                 className="rounded-lg border border-slate-300 px-3 py-2 text-sm"/>
                 <select
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm">
                    <option value="">
                        All categories
                    </option>
                    {categories.map((c) => (
                        <option key={c.id} value={c.id}>
                            {c.name}
                        </option>
                    ))}
                 </select>
                 <select
                  value={stockStatus}
                  onChange={(e) => setStockStatus(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm">
                    <option value="">All status</option>
                    <option value="in">In stock</option>
                    <option value="low">Low stock</option>
                    <option value="out">Out of stock</option>
                 </select>
            </div>

            {error && (
                <p className="mb-4 rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</p>
            )}

            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                <table className="w-full text-left text-sm">
                    <thead className="bg-slate-50 text-slate-500">
                        <tr>
                            <th className="px-4 py-3 font-medium">Name</th>
                            <th className="px-4 py-3 font-medium">Category</th>
                            <th className="px-4 py-3 font-medium">Price</th>
                            <th className="px-4 py-3 font-medium">Quantity</th>
                            <th className="px-4 py-3 font-medium">Min</th>
                            <th className="px-4 py-3 font-medium">Status</th>
                            <th className="px-4 py-3 font-medium">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.length === 0 && !error && (
                            <tr>
                                <td colSpan={7} className="px-4 py-8 text-center text-slate-500">
                                    No products found.
                                </td>
                            </tr>
                        )}
                        {items.map((item) => (
                            <tr key={item.id} className={rowClass(item.stockStatus)}>
                                <td className="px-4 py-3 font-medium">{item.name}</td>
                                <td className="px-4 py-3">{item.categoryName}</td>
                                <td className="px-4 py-3">{Number(item.price).toFixed(2)}</td>
                                <td className="px-4 py-3">{item.quantity}</td>
                                <td className="px-4 py-3">{item.minimumStock}</td>
                                <td className="px-4 py-3">StockBadge status = {item.stockStatus}</td>
                                <td className="px-4 py-3 space-x-3">
                                    <button type="button" onClick={() => openEdit(item)} className="text-indigo-600 hover:underline">
                                        Edit
                                    </button>
                                    <button type="button" onClick={() => remove(item)} className="text-rose-600 hover:underline">
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {open && (
                <div className="fixed inset-0 z-10 flex items-center justify-center bg-black/40 p-4">
                    <form onSubmit={save} className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
                        <h2 className="mb-4 text-lg font-semibold">{editing ? 'Edit Product' : 'Add Product'}</h2>

                        <label className="mb-3 block text-sm">
                            Name
                            <input
                             required
                             maxLength={255}
                             value={form.name}
                             onChange={(e) => setForm({...form, name: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
                        </label>

                        <label className="mb-3 block text-sm">
                            Category
                            <select
                             required
                             value={form.categoryId}
                             onChange={(e) => setForm({...form, categoryId: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2">
                                <option>Select...</option>
                                {categories.map((c) => (
                                    <option key={c.id} value={c.id}>
                                        {c.name}
                                    </option>
                                ))}
                            </select>
                        </label>

                        <label className="mb-3 block text-sm">
                            Price
                            <input
                             required
                             type="number"
                             min="0"
                             step="0.01"
                             value={form.price}
                             onChange={(e) => setForm({...form, price: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
                        </label>

                        {!editing && (
                            <label>
                                Quantity
                                <input
                                 required
                                 type="number"
                                 min="0"
                                 value={form.quantity}
                                 onChange={(e) => setForm({...form, quantity: e.target.value})}
                                 className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
                                 />
                            </label>
                        )}

                        {editing && (
                            <p className="mb-3 text-xs text-slate-500">
                                Change quantity via Stock In / Stock Out.
                            </p>
                        )}

                        <label>
                            Minimum Stock
                            <input
                             required
                             type="number"
                             min="0"
                             value={form.minimumStock}
                             onChange={(e) => setForm({...form, minimumStock: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
                        </label>

                        <div className="flex justify-end gap-2">
                            <button
                             type="button"
                             onClick={() => setOpen(false)}
                             className="rounded-lg px-4 py-2 text-sm text-slate-600 hover:bg-slate-100">
                                Cancel
                            </button>
                            <button
                             type="submit"
                             disabled={saving}
                             className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
                                {saving ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </div>
            )}
        </div>
    )
}

