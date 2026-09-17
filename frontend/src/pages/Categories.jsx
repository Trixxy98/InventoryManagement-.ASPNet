import {useEffect, useState} from 'react';
import api from '../api';

function getErrorMessage(err) {
    const data = err.response?.data
    if (data?.message) return data.message
    if (data?.errors) return Object.values(data.errors).flat().join(' ')
    return 'Cannot reach API. Is backend running on :5150'
}

const emptyForm = {name: '', description: '', productCount: ''}

export default function Categories() {
    const [items, setItems] = useState([])
    const [error, setError] = useState('')
    const [open, setOpen] = useState(false)
    const [editing, setEditing] = useState(null)
    const [form, setForm] = useState(emptyForm)
    const [saving, setSaving] = useState(false)

    async function load() {
        try {
            const {data} = await api.get('/categories')
            setItems(data)
            setError('')
        } catch (err) {
            setError(getErrorMessage(err))
        }
    }

    useEffect(() => {
        load()
    }, [])

    function openCreate() {
        setEditing(null)
        setForm(emptyForm)
        setOpen(true)
    }

    function openEdit(item) {
        setEditing(item)
        setForm({name: item.name, description: item.description ?? ''})
        setOpen(true)
    }

    async function save(e) {
        e.preventDefault()
        setSaving(true)
        try {
            const body = {
                name: form.name.trim(),
                description: form.description.trim() || null,
            }
            if (editing) {
                await api.put(`/categories/${editing.id}`, body)
            } else {
                await api.post('/categories', body)
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
        if (!window.confirm(`Delete "${item.name}"?`)) return
        try {
            await api.delete(`/categories/${item.id}`)
            await load()
        } catch (err) {
            alert(getErrorMessage(err))
        }
    }

    return (
        <div>
            <div className="mb-6 flex items-center justify-between">
                <h1 className="text-2xl font-semibold">Categories</h1>
                <button
                 type="button"
                 onClick={openCreate}
                 className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500">
                    + Add Category
                </button>
            </div>

            {error && (
                <p className="mb-4 rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">
                    {error}
                </p>
            )}
            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                <table className="w-full text-left text-sm">
                    <thead className="bg-slate-50 text-slate-500">
                        <tr>
                            <th className="px-4 py-3 font-medium">Name</th>
                            <th className="px-4 py-3 font-medium">Description</th>
                            <th className="px-4 py-3 font-medium">Products</th>
                            <th className="px-4 py-3 font-medium">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items.length === 0 && !error && (
                            <tr>
                                <td colSpan={4} className="px-4 py-8 text-center text-slate-500">
                                    No categories found.
                                </td>
                            </tr>
                        )}
                        {items.map((item) => (
                            <tr key={item.id} className="border-t border-slate-100">
                                <td className="px-4 py-3 font-medium">{item.name}</td>
                                <td className="px-4 py-3 font-slate-600">{item.description}</td>
                                <td className="px-4 py-3">{item.productCount}</td>
                                <td className="px-4 py-3 space-x-3">
                                    <button
                                     type="button"
                                     onClick={() => openEdit(item)}
                                     className="text-indigo-600 hover:underline">
                                        Edit
                                    </button>
                                    <button
                                     type="button"
                                     onClick={() => remove(item)}
                                     className="text-rose-600 hover:underline">
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
                    <form
                     onSubmit={save}
                     className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
                        <h2 className="mb-4 text-lg font-semibold">
                            {editing ? 'Edit Category' : 'Add Category'}
                        </h2>
                        <label className="mb-3 block text-sm">
                            Name
                            <input
                             required
                             maxLength={80}
                             value={form.name}
                             onChange={(e) => setForm({...form, name: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
                            />
                        </label>
                        <label className="mb-4 block text-sm">
                            Description
                            <input
                             maxLength={255}
                             value={form.description}
                             onChange={(e) => setForm({...form, description: e.target.value})}
                             className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
                        </label>
                        <div className="flex justify-end gap-2">
                        <button
                            type="button"
                            onClick={() => setOpen(false)}
                            className="rounded-lg px-4 py-2 text-sm text-slate-600 hover:bg-slate-100"
                        >
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