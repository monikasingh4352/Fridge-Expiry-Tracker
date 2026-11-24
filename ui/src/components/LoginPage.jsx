import { useState } from 'react'
import { login, register } from '../api.js'

export default function LoginPage({ onLoggedIn }) {
  const [mode, setMode] = useState('login')   // 'login' or 'register'
  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const isLogin = mode === 'login'

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSaving(true)
    try {
      const user = isLogin
        ? await login(email, password)
        : await register({ name, phone, email, password })
      onLoggedIn(user)
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  function switchMode() {
    setMode(isLogin ? 'register' : 'login')
    setError('')
  }

  return (
    <main className="page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1>Fridge Expiry Tracker</h1>
        <p className="tagline">{isLogin ? 'Log in to see your fridge.' : 'Create an account to get started.'}</p>

        {!isLogin && (
          <>
            <label>
              Name
              <input value={name} onChange={e => setName(e.target.value)}
                     autoComplete="name" maxLength={100} required />
            </label>
            <label>
              Phone number
              <input type="tel" value={phone} onChange={e => setPhone(e.target.value)}
                     autoComplete="tel" placeholder="e.g. +91 98765 43210" maxLength={20} required />
            </label>
          </>
        )}
        <label>
          Email
          <input type="email" value={email} onChange={e => setEmail(e.target.value)}
                 autoComplete="email" maxLength={256} required />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={e => setPassword(e.target.value)}
                 autoComplete={isLogin ? 'current-password' : 'new-password'}
                 minLength={isLogin ? undefined : 8} maxLength={128} required />
        </label>
        {!isLogin && <p className="hint">At least 8 characters.</p>}

        {error && <p className="error" role="alert">{error}</p>}

        <button type="submit" disabled={saving}>
          {saving ? 'Please wait…' : isLogin ? 'Log in' : 'Create account'}
        </button>

        <p className="switch">
          {isLogin ? 'No account yet?' : 'Already have an account?'}{' '}
          <button type="button" className="link" onClick={switchMode}>
            {isLogin ? 'Create one' : 'Log in'}
          </button>
        </p>
      </form>
    </main>
  )
}
