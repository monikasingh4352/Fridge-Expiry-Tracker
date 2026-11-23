import { useState } from 'react'

export default function AddItemForm({ onAdd }) {
  const [name, setName] = useState('')
  const [quantity, setQuantity] = useState(1)
  const [expiryDate, setExpiryDate] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSaving(true)
    try {
      await onAdd({ name, quantity: Number(quantity), expiryDate })
      setName('')
      setQuantity(1)
      setExpiryDate('')
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="add-form" onSubmit={handleSubmit}>
      <label>
        Food
        <input value={name} onChange={e => setName(e.target.value)}
               placeholder="e.g. Milk" maxLength={100} required />
      </label>
      <label>
        Qty
        <input type="number" min="1" max="999" value={quantity}
               onChange={e => setQuantity(e.target.value)} required />
      </label>
      <label>
        Expiry date
        <input type="date" value={expiryDate}
               onChange={e => setExpiryDate(e.target.value)} required />
      </label>
      <button type="submit" disabled={saving}>{saving ? 'Adding…' : 'Add item'}</button>
      {error && <p className="error form-error" role="alert">{error}</p>}
    </form>
  )
}
