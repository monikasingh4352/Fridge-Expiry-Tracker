# Fridge Expiry Tracker

Add food with its expiry date and see which items to use first. Items going off soon turn red.

## Run it

Needs the .NET 8 SDK and Node.js.

Double-click `start-local.bat`, or start each part in its own terminal:

```
cd api
dotnet run          # API on http://localhost:5080

cd ui
npm install         # first time only
npm run dev         # UI on http://localhost:5173
```

Open http://localhost:5173, create an account, and log in.

## How it works

| Part | Tech | Folder |
|------|------|--------|
| Front end | React + Vite | `ui/` |
| Back end | .NET 8 minimal API + Entity Framework Core | `api/` |
| Database | SQLite (file `api/fridge.db`, created on first run) | — |

The UI calls `/api/items`. Vite forwards those calls to the API on port 5080.

| Method | Endpoint | What it does |
|--------|----------|--------------|
| POST | `/api/auth/register` | Create an account `{ email, password }` and log in |
| POST | `/api/auth/login` | Log in `{ email, password }` (sets a login cookie) |
| POST | `/api/auth/logout` | Log out |
| GET | `/api/auth/me` | The logged-in user's `{ email }`, or 401 |
| GET | `/api/items` | List items, soonest expiry first, with `category`, `daysLeft` and `status` |
| POST | `/api/items` | Add an item `{ name, quantity, category, expiryDate }` |
| DELETE | `/api/items/{id}` | Remove an item |
| GET | `/api/categories` | List the categories to choose from (Dairy, Vegetables, Fruit, …) |

The items and categories endpoints need a logged-in user (they return 401 otherwise).

Status rule (in `api/Models/FoodItem.cs`): `expired` if past its date, `soon` if 0–3 days left (shown in red), otherwise `fresh`.
