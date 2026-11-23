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

Open http://localhost:5173.

## How it works

| Part | Tech | Folder |
|------|------|--------|
| Front end | React + Vite | `ui/` |
| Back end | .NET 8 minimal API + Entity Framework Core | `api/` |
| Database | SQLite (file `api/fridge.db`, created on first run) | — |

The UI calls `/api/items`. Vite forwards those calls to the API on port 5080.

| Method | Endpoint | What it does |
|--------|----------|--------------|
| GET | `/api/items` | List items, soonest expiry first, with `category`, `daysLeft` and `status` |
| POST | `/api/items` | Add an item `{ name, quantity, category, expiryDate }` |
| DELETE | `/api/items/{id}` | Remove an item |
| GET | `/api/categories` | List the categories to choose from (Dairy, Vegetables, Fruit, …) |

Status rule (in `api/Models/FoodItem.cs`): `expired` if past its date, `soon` if 0–3 days left (shown in red), otherwise `fresh`.
