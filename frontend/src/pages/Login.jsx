import {useState} from 'react';
import {Navigate, useNavigate} from 'react-router-dom';
import {useAuth} from '../auth';
import api from '../api';

export default function Login() {
  const {login, isAuthed} = useAuth()
  const navigate = useNavigate()
  const [mode, setMode] = useState('login')
  const [username, setUsername] = useState('admin')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const isRegister = mode === 'register'

  if (isAuthed) {
    return <Navigate to="/" replace />
  }

  async function submit(e) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      if (isRegister) {
        await api.post('/auth/register', {username, password})
        await login(username, password)
      } else {
        await login(username, password)
      }
      navigate('/', {replace: true})
    } catch (err) {
      setError(err.response?.data?.message || (isRegister ? 'Register failed' : 'Login failed'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 p-4">
      <form onSubmit={submit} className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <p className="text-lg font-semibold">Inventory</p>
        <p className="mb-4 text-xs text-slate-500">{isRegister ? 'Create an account' : 'Sign in to your account'}</p>
        {error && <p className="mb-3 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p>}
        <label>
          Username
          <input
           required
           maxLength={40}
           value={username}
           onChange={(e) => setUsername(e.target.value)}
           className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
        </label>
        <label>
          Password
          <input
           required
           type="password"
           minLength={isRegister ? 8 : undefined}
           value={password}
           onChange={(e) => setPassword(e.target.value)}
           className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"/>
        </label>
        <button
         type="submit"
         disabled={saving}
         className="w-full rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
          {saving ? 'Please wait...' : isRegister ? 'Create account' : 'Sign in'}
        </button>
        <button
         type="button"
         onClick={() => {
          setMode(isRegister ? 'login' : 'register')
          setError('')
          setUsername('')
          setPassword('')
         }}
         className="mt-3 w-full text-sm text-indigo-600 hover:underline">
          {isRegister ? 'Already have an account? Sign in' : 'Need an account? Create one'}
        </button>
        {!isRegister && (
          <p className="mt-3 text-xs text-slate-400">Demo: admin </p>
        )}
      </form>
    </div>
  )
}