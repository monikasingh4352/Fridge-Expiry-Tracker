import { useEffect, useState } from 'react'
import { getItems, addItem, deleteItem } from './api.js'
import AddItemForm from './components/AddItemForm.jsx'
import ItemList from './components/ItemList.jsx'

export default function App() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  async function load() {
    try {
      setError('')
      setItems(await getItems())
    } catch (e) {
      setError(`Could not load items. Is the API running? (${e.message})`)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleAdd(item) {
    await addItem(item)   // errors are shown by the form
    await load()
  }

  async function handleDelete(id) {
    try {
      await deleteItem(id)
      await load()
    } catch (e) {
      setError(e.message)
    }
  }

  const counts = {
    expired: items.filter(i => i.status === 'expired').length,
    soon: items.filter(i => i.status === 'soon').length,
    fresh: items.filter(i => i.status === 'fresh').length
  }

  return (
    <main className="page">
      <header>
        <h1>Fridge Expiry Tracker</h1>
        <p className="tagline">Add food with its expiry date and see what to use first.</p>
      </header>

      <section className="summary">
        <span className="chip chip-expired">{counts.expired} expired</span>
        <span className="chip chip-soon">{counts.soon} going off soon</span>
        <span className="chip chip-fresh">{counts.fresh} fresh</span>
      </section>

      <AddItemForm onAdd={handleAdd} />

      {error && <p className="error" role="alert">{error}</p>}
      {loading ? <p>Loading…</p> : <ItemList items={items} onDelete={handleDelete} />}
    </main>
  )
}
