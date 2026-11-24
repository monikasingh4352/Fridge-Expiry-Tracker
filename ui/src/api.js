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

function postJson(url, data) {
  return fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  })
}

// Returns { email } of the logged-in user, or null if nobody is logged in.
export async function getMe() {
  const response = await fetch('/api/auth/me')
  if (response.status === 401) return null
  return handle(response)
}

export async function login(email, password) {
  return handle(await postJson('/api/auth/login', { email, password }))
}

export async function register(email, password) {
  return handle(await postJson('/api/auth/register', { email, password }))
}

export async function logout() {
  return handle(await fetch('/api/auth/logout', { method: 'POST' }))
}

export async function getCategories() {
  return handle(await fetch('/api/categories'))
}

export async function getItems() {
  return handle(await fetch(BASE))
}

export async function addItem(item) {
  return handle(await postJson(BASE, item))
}

export async function deleteItem(id) {
  return handle(await fetch(`${BASE}/${id}`, { method: 'DELETE' }))
}
