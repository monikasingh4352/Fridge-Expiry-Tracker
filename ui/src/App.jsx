import { useEffect, useState } from 'react'
import { getMe, logout } from './api.js'
import FridgePage from './components/FridgePage.jsx'
import LoginPage from './components/LoginPage.jsx'

// Shows the login page until the user is logged in, then the fridge.
export default function App() {
  const [user, setUser] = useState(undefined)   // undefined = still checking, null = logged out
  const [error, setError] = useState('')

  useEffect(() => {
    getMe()
      .then(setUser)
      .catch(e => {
        setError(`Could not reach the API. Is it running? (${e.message})`)
        setUser(null)
      })
  }, [])

  async function handleLogout() {
    try {
      await logout()
    } finally {
      setUser(null)
    }
  }

  if (user === undefined) return <main className="page"><p>Loading…</p></main>

  if (user === null) {
    return (
      <>
        {error && <p className="error page" role="alert">{error}</p>}
        <LoginPage onLoggedIn={u => { setError(''); setUser(u) }} />
      </>
    )
  }

  return <FridgePage user={user} onLogout={handleLogout} />
}
