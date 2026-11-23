// All calls from the UI to the back end live here.
const BASE = '/api/items'

async function handle(response) {
  if (response.ok) {
    return response.status === 204 ? null : response.json()
  }
  // The API returns validation errors as { errors: { field: [messages] } }
  let message = `Request failed (${response.status})`
  try {
    const body = await response.json()
    if (body.errors) message = Object.values(body.errors).flat().join(' ')
  } catch {
    // response had no JSON body
  }
  throw new Error(message)
}

export async function getItems() {
  return handle(await fetch(BASE))
}

export async function addItem(item) {
  return handle(await fetch(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(item)
  }))
}

export async function deleteItem(id) {
  return handle(await fetch(`${BASE}/${id}`, { method: 'DELETE' }))
}
