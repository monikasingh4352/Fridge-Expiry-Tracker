@echo off
REM Starts the Fridge Expiry Tracker locally: API (port 5080) and UI (port 5173).
cd /d "%~dp0"

if not exist "ui\node_modules" (
  echo Installing UI packages...
  pushd ui
  call npm install
  popd
)

start "Fridge API" cmd /k "cd /d "%~dp0api" && dotnet run"
start "Fridge UI"  cmd /k "cd /d "%~dp0ui" && npm run dev"

echo.
echo API: http://localhost:5080/api/items
echo UI : http://localhost:5173
echo Close the two new windows to stop the app.
