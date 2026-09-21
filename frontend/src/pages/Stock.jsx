import { useEffect, useState } from 'react'
import api from '../api'

function getErrorMessage(err) {
    const data = err.response?.data
    if (data?.message) return data.message
    if (data?.errors) return Object.values(data.errors).flat().join(' ')
    return 'Cannot reach API. Is backend running on :5150'
}

const emptyForm = {type: 'in', productId: '', quantity: '', note: ''}

export default function Stock() {
    const [products, setProducts] = useState([])
    const [history, setHistory] = useState([])
    const [form, setForm] = useState(emptyForm)
    const [filterProductId, setFilterProductId] = useState('')
    const [error, setError] = useState('')
    const [success, setSuccess] = useState('')
    const [saving, setSaving] = useState(false)

    async function loadProducts() {
        const {data} = await api.get('/products', {params: {page: 1, pageSize: 100}})
        setProducts(data.items ?? data)
    }

    async function loadHistory() {
        const params = {}
        if (filterProductId) params.productId = filterProductId
        const {data} = await api.get('/stock', {params})
        setHistory(data)
    }

    async function load() {
        try {
            await Promise.all([loadProducts(), loadHistory()])
            setError('')
        } catch (err) {
            setError(getErrorMessage(err))
        }
    }

    useEffect(() => {
        load()
    }, [filterProductId])

    async function submit(e) {
        e.preventDefault()
        setSaving(true)
        setSuccess('')
        try {
            const body = {
                productId: Number(form.productId),
                quantity: Number(form.quantity),
                note: form.note.trim() || null,
            }
            const url = form.type === 'out' ? '/stock/out' : '/stock/in'
            const {data} = await api.post(url, body)
            setSuccess(`Stock updated. New balance: ${data.balanceAfter}`)
            setForm({...emptyForm, type: form.type})
            await load()
        } catch (err) {
            alert(getErrorMessage(err))
        }finally {
            setSaving(false)
        }
    }

    return (
        <div>
            <h1 className="mb-6 text-2xl font-semibold">Stock</h1>

            {error && (
                <p className="mb-4 rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</p>
            )}
            {success && (
                <p className="mb-4 rounded-lg bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</p>
            )}

            <form
             onSubmit={submit}
             className="mb-8 max-w-lg rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
                <h2 className="mb-4 text-lg font-semibold">Stock movement</h2>

                <div className="mb-4 flex gap-6 text-sm">
                    <label className="flex items-center gap-2">
                        <input
                         type="radio"
                         name="type"
                         checked={form.type === 'in'}
                         onChange={() => setForm({...form, type: 'in'})}/>
                        In
                    </label>
                    <label>
                        <input
                         type="radio"
                         name="type"
                         checked={form.type === 'out'}
                         onChange={() => setForm({...form, type: 'out'})}/>
                        Out
                    </label>
                </div>

                <label className="mb-3 block text-sm">
                    Product
                    <select
                     required
                     value={form.productId}
                     onChange={(e) => setForm({...form, productId: e.target.value})}
                     className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2">
                        <option value="">Select...</option>
                        {products.map((p) => (
                            <option key={p.id} value={p.id}>
                                {p.name} (qty {p.quantity})
                            </option>
                        ))}
                    </select>
                </label>

                <label className="mb-3 block text-sm">
                    Quantity
                    <input
                     required
                     type="number"
                     min="1"
                     value={form.quantity}
                     onChange={(e) => setForm({...form, quantity: e.target.value})}
                     className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
                </label>

                <label className="mb-4 block text-sm">
                    Note
                    <input
                     maxLength={255}
                     value={form.note}
                     onChange={(e) => setForm({...form, note: e.target.value})}
                     placeholder="optional"
                     className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
                    />
                </label>

                <button
                 type="submit"
                 disabled={saving}
                 className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
                    {saving ? 'Saving...' : 'Submit'}
                </button>
            </form>

            <div className="mb-3 flex items-center justify-between">
                <h2 className="text-lg font-semibold">Recent transactions</h2>
                <select
                 value={filterProductId}
                 onChange={(e) => setFilterProductId(e.target.value)}
                 className="rounded-lg border border-slate-300 px-3 py-2 text-sm">
                    <option value="">All products</option>
                    {products.map((p) => (
                        <option key={p.id} value={p.id}>
                            {p.name}
                        </option>
                    ))}
                </select>
            </div>

            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                <table className="w-full text-left text-sm">
                    <thead className="bg-slate-50 text-slate-500">
                        <tr>
                            <th className="px-4 py-3 font-medium">Date</th>
                            <th className="px-4 py-3 font-medium">Product</th>
                            <th className="px-4 py-3 font-medium">Type</th>
                            <th className="px-4 py-3 font-medium">Qty</th>
                            <th className="px-4 py-3 font-medium">Note</th>
                        </tr>
                    </thead>
                    <tbody>
                        {history.length === 0 && !error && (
                            <tr>
                                <td colSpan={5} className="px-4 py-8 text-center text-slate-500">
                                    No stock movements yet.
                                </td>
                            </tr>
                        )}
                        {history.map((row) => (
                            <tr key={row.id} className="border-t border-slate-100">
                                <td className="px-4 py-3 text-slate-600">
                                    {new Date(row.createdAt).toLocaleString()}
                                </td>
                                <td className="px-4 py-3 font-medium">{row.productName}</td>
                                <td className="px-4 py-3">
                                    <span
                                     className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${row.type === 'In' ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}`}>
                                        {row.type}
                                    </span>
                                </td>
                                <td className="px-4 py-3">{row.quantity}</td>
                                <td className="px-4 py-3 text-slate-600">{row.note || '-'}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    )
}