import { createContext, useContext, useMemo, useState } from 'react'
import api from './api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('token'))
  const [username, setUsername] = useState(() => localStorage.getItem('username') || '')
  const [role, setRole] = useState(() => localStorage.getItem('role') || '')

  const value = useMemo(
    () => ({
      token,
      username,
      isAuthed: Boolean(token),
      async login(user, password) {
        const { data } = await api.post('/auth/login', { username: user, password })
        localStorage.setItem('token', data.token)
        localStorage.setItem('username', data.username)
        localStorage.setItem('role', data.role)
        setToken(data.token)
        setUsername(data.username)
        setRole(data.role)
      },
      logout() {
        localStorage.removeItem('token')
        localStorage.removeItem('username')
        localStorage.removeItem('role')
        setToken(null)
        setUsername('')
        setRole('')
      },
    }),
    [token, username]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
