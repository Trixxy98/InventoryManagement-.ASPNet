import {NavLink, Outlet} from 'react-router-dom';

const links = [
    {to: '/', label: 'Dashboard', end: true},
    {to: '/products', label: 'Products'},
    {to: '/categories', label: 'Categories'},
    {to: '/stock', label: 'Stock'},
]

function linkClass({isActive}) {
    return `block rounded-lg px-3 py-2 text-sm font-medium ${isActive ? 'bg-indigo-600 text-white' : 'text-slate-300 hover:bg-slate-800 hover:text-white'}`
}

export default function Layout() {
    return (
        <div className="min-h-screen bg-slate-50 text-slate-900">
            <div className="flex min-h-screen">
                <aside className="flex w-60 flex-col bg-slate-900 px-4 py-6 text-white">
                    <div className="mb-8 px-3">
                        <p className="text-lg font-semibold">Inventory</p>
                        <p className="text-xs text-slate-400">React + ASP.NET</p>
                    </div>
                    <nav className="space-y-1">
                        {links.map((link) => (
                            <NavLink key={link.to} to={link.to} end={link.end} className={linkClass}>
                                {link.label}
                            </NavLink>
                        ))}
                    </nav>
                </aside>

                <main className="flex-1 p-8">
                    <Outlet />
                </main>
            </div>

        </div>
    )
}