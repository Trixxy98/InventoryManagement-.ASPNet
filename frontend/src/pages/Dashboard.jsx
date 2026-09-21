import {useEffect, useState} from 'react'
import api from '../api'

function getErrorMessage(err) {
    const data = err.response?.data
    if (data?.message) return data.message
    if (data?.errors) return Object.values(data.errors).flat().join(' ')
    return 'Cannot reach API. Is backend running on :5150?'
}

function StatCard({label, value, hint, accent}) {
    const tones = {
        slate: 'border-slate-200 bg-white',
        indigo: 'border-indigo-100 bg-indigo-50/60',
        amber: 'border-amber-100 bg-amber-50/70',
        rose: 'border-rose-100 bg-rose-50/70',
    }
    return (
        <div className={`rounded-xl border p-5 shadow-sm ${tones[accent] || tones.slate}`}>
            <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">{Number(value).toLocaleString()}</p>
            <p className="mt-1 text-xs text-slate-500">{hint}</p>
        </div>
    )
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
        OutOfStock: 'Out of Stock',
    }
    return (
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${map[status] || 'bg-slate-100'}`}> 
            {label[status] || status}
        </span>
    )
}

export default function Dashboard() {
    const [stats, setStats] = useState(null)
    const [attention, setAttention] = useState([])
    const [movements, setMovements] = useState([])
    const [error, setError] = useState('')

    useEffect(() => {
        async function load() {
            try {
                const [dash, low, move] = await Promise.all([
                    api.get('/dashboard'),
                    api.get('/dashboard/low-stock'),
                    api.get('/dashboard/movements'),
                ])
                setStats(dash.data)
                setAttention(low.data)
                setMovements(move.data)
                setError('')
            } catch (err) {
                setError(getErrorMessage(err))
            }
        }
        load()
    }, [])

    return (
        <div>
            <h1 className="mb-6 text-2xl font-semibold">Dashboard</h1>

            {error && (
                <p className="mb-4 rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</p>
            )}

            {stats && (
                <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                    <StatCard
                     label="Total Products"
                     value={stats.totalProducts}
                     hint="Total SKUs"
                     accent="slate"/>
                     <StatCard
                     label="Total Stock"
                     value={stats.totalStock}
                     hint="Units"
                     accent="indigo"/>
                     <StatCard
                     label="Low Stock"
                     value={stats.lowStock}
                     hint="Need restock"
                     accent="amber"/>
                     <StatCard
                     label="Out of Stock"
                     value={stats.outOfStock}
                     hint="Empty"
                     accent="rose"/>
                </div>
            )}

            {movements.length > 0 && (
                <div className="mb-8 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
                    <h2 className="mb-4 text-lg font-semibold">Stock movement (7 days)</h2>
                    <div className="flex h-40 items-end gap-2">
                        {movements.map((day) => {
                            const max = Math.max(1, ...movements.map((d) => d.stockIn + d.stockOut))
                            const inH = (day.stockIn / max) * 100
                            const outH = (day.stockOut / max) * 100
                            return (
                                <div key={day.date} className="flex flex-1 flex-col items-center gap-1">
                                    <div className="flex h-28 w-full items-end justify-center gap-0.5">
                                        <div
                                            className="w-3 rounded-t bg-emerald-500"
                                            style={{height: `${inH}%`}}
                                            title={`In ${day.stockIn}`}
                                        />
                                        <div
                                            className="w-3 rounded-t bg-rose-400"
                                            style={{height: `${outH}%`}}
                                            title={`Out ${day.stockOut}`}
                                        />
                                    </div>
                                    <span className="text-[10px] text-slate-500">{day.date.slice(5)}</span>
                                </div>
                            )
                        })}
                    </div>
                    <p className="mt-2 text-xs text-slate-500">
                        <span className="mr-3 text-emerald-600">■ In</span>
                        <span className="text-rose-500">■ Out</span>
                    </p>
                </div>
            )}

            <h2 className="mb-3 text-lg font-semibold">Attention needed</h2>
            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                <table className="w-full text-left text-sm">
                    <thead className="bg-slate-50 text-slate-500">
                        <tr>
                            <th className="px-4 py-3 font-medium">Product</th>
                            <th className="px-4 py-3 font-medium">Category</th>
                            <th className="px-4 py-3 font-medium">Qty</th>
                            <th className="px-4 py-3 font-medium">Min</th>
                            <th className="px-4 py-3 font-medium">Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        {attention.length === 0 && !error && (
                            <tr>
                                <td colSpan={5} className="px-4 py-8 text-center text-slate-500">
                                    All products are above minimum stock.
                                </td>
                            </tr>
                        )}
                        {attention.map((item) => (
                            <tr
                                key={item.id}
                                className={
                                    item.stockStatus === 'OutOfStock'
                                        ? 'border-t border-slate-100 bg-rose-50/60'
                                        : 'border-t border-slate-100 bg-amber-50/60'
                                }
                            >
                                <td className="px-4 py-3 font-medium">{item.name}</td>
                                <td className="px-4 py-3">{item.categoryName}</td>
                                <td className="px-4 py-3">{item.quantity}</td>
                                <td className="px-4 py-3">{item.minimumStock}</td>
                                <td className="px-4 py-3">
                                    <StockBadge status={item.stockStatus} />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    )
}