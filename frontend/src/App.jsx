import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import Products from './pages/Products'
import Categories from './pages/Categories'
import Stock from './pages/Stock'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/products" element={<Products />} />
          <Route path="/categories" element={<Categories />} />
          <Route path="/stock" element={<Stock />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
