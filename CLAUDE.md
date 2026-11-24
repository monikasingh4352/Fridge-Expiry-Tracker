# CLAUDE.md

This file guides Claude Code when working in this repository.

## Project

**Fridge Expiry Tracker**: a small full-stack web app. The user adds food items with an expiry date and sees a list sorted so the item to use first is at the top. Items going off soon are shown in red.


## Tech stack

- **Front end:** React 18 + Vite 5, plain JavaScript (JSX), plain CSS. Folder: `ui/`
- **Back end:** .NET 8 minimal API (C#) with Entity Framework Core. Folder: `api/`
- **Database:** SQLite, file `api/fridge.db`, created automatically on first run (`EnsureCreated`)

## How to run

**Easiest way:**
1. Double-click `start-local.bat` in the project folder.
2. Open http://localhost:5173 in the browser.
3. To stop, close the two windows that opened ("Fridge API" and "Fridge UI").

**Step by step (two terminals):**

| | Terminal 1: back end | Terminal 2: front end |
|---|---|---|
| Go to folder | `cd api` | `cd ui` |
| First time only | (nothing) | `npm install` |
| Start | `dotnet run` | `npm run dev` |
| Runs on | http://localhost:5080 | http://localhost:5173 (open this one) |

Start the back end first, then the front end.

**Check that it builds** (run after every change):
- `cd api` then `dotnet build`
- `cd ui` then `npm run build`

The front end forwards every `/api` call to the back end on port 5080 (set in `ui/vite.config.js`), so no CORS setup is needed locally.

## Structure

```
api/
  Program.cs            cookie login setup; endpoints: /api/auth/*, GET/POST /api/items, DELETE /api/items/{id}, GET /api/categories
  Models/FoodItem.cs    entity, request/response records, validation, Categories, ExpiryRules
  Models/User.cs        User entity, AuthRequest validation, PasswordHashing (PBKDF2)
  Data/FridgeDbContext.cs
  FridgeApi.http        sample requests
ui/
  src/api.js            all fetch calls to the API
  src/App.jsx           checks who is logged in; shows LoginPage or FridgePage
  src/components/       LoginPage.jsx, FridgePage.jsx (items + summary counts), AddItemForm.jsx, ItemList.jsx
  src/App.css           styles (status colours)
start-local.bat         starts API + UI
```

## Business rules

- Status comes from days left until expiry, calculated in the **back end** (`ExpiryRules` in `api/Models/FoodItem.cs`):
  - `expired`: expiry date is in the past (dark red, name crossed out)
  - `soon`: 0 to 3 days left (`SoonThresholdDays = 3`) (red)
  - `fresh`: more than 3 days left (green)
- The API returns items sorted by expiry date (soonest first). The UI shows them in that order.
- Validation (back end): name required (max 100 chars), expiry date required, quantity 1–999, category required and must be in the list.
- Categories are a fixed list owned by the **back end** (`Categories` in `api/Models/FoodItem.cs`): Dairy, Vegetables, Fruit, Meat & Fish, Bakery, Drinks, Leftovers, Other. The UI loads them from `GET /api/categories`; do not hard-code the list in the UI. To add a category, change only `Categories.All`.

## Login

- Users sign up and log in with email + password. Emails are stored trimmed and lower case; passwords are never stored, only a salted PBKDF2 hash (`PasswordHashing` in `api/Models/User.cs`). Password: 8–128 characters.
- After login the API sets an HttpOnly, SameSite=Strict cookie (`fridge.auth`, 8 hours, sliding). Built-in ASP.NET Core cookie authentication, no extra package.
- Endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`.
- `/api/items` and `/api/categories` require login (`RequireAuthorization()`); they return 401 when logged out. Any new endpoint with fridge data must also require login.
- Wrong email and wrong password give the same message ("Email or password is wrong.") so nobody can find out which emails have accounts. Keep it that way.
- Items are shared between all logged-in users (not per user).

## Database changes

- The table is created with `EnsureCreated()` in `api/Program.cs`. This only creates the database if `api/fridge.db` does not exist yet. It **never updates** an existing table.
- So if you change `FoodItem` or `User`, or add a table (add, rename or remove a field), the existing `fridge.db` will not match the code and the API will fail when reading or saving items.
- To apply a model change locally: stop the API, delete `api/fridge.db`, then run `dotnet run` again. **This deletes all saved items.**
- Before changing the model, tell the user that their local data will be lost. If the data must be kept, ask before switching to EF Core migrations (that adds a tool and a `Migrations/` folder).

## Rules for Claude

- Keep the three layers separate: business rules and validation in the **API**, display only in the **UI**. Do not duplicate the status calculation in the front end.
- All HTTP calls from the UI go through `ui/src/api.js`.
- Never hard-code secrets, API keys or connection strings with passwords. Use configuration (`appsettings.*.json` that is not committed, or environment variables).
- Never commit `fridge.db`, `node_modules/`, `bin/`, `obj/` or `.env` files (see `.gitignore`).
- Don't add new libraries or frameworks (UI kits, state libraries, ORMs) without asking first. Keep the project small.
- Don't put ticket numbers or homework notes in code comments.
- Explain any plan in simple English before large changes, then make small, focused edits.
- After changes, run `dotnet build` and `npm run build` and confirm both succeed.
