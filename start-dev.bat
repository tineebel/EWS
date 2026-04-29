@echo off
set "ROOT=%~dp0"
echo Starting EWS Development Servers...
echo.

:: ---- Check Backend (port 5271) ----
netstat -ano | findstr ":5271 " | findstr "LISTENING" >nul 2>&1
if %errorlevel%==0 (
    echo [!] Backend port 5271 is already in use.
    choice /C YN /N /M "    Kill existing process? [Y/N]: "
    if errorlevel 2 (
        echo     Skipping Backend startup.
        goto :start_frontend
    ) else (
        for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":5271 " ^| findstr "LISTENING"') do (
            echo     Killing PID %%p...
            taskkill /PID %%p /F >nul 2>&1
        )
        timeout /t 1 /nobreak >nul
    )
)

echo [1/2] Starting Backend API (http://localhost:5271)...
start "EWS API" cmd /k "pushd ""%ROOT%"" && dotnet run --project src\EWS.API\EWS.API.csproj"
timeout /t 3 /nobreak >nul

:: ---- Check Frontend (port 3000) ----
:start_frontend
netstat -ano | findstr ":3000 " | findstr "LISTENING" >nul 2>&1
if %errorlevel%==0 (
    echo [!] Frontend port 3000 is already in use.
    choice /C YN /N /M "    Kill existing process? [Y/N]: "
    if errorlevel 2 (
        echo     Skipping Frontend startup.
        goto :summary
    ) else (
        for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":3000 " ^| findstr "LISTENING"') do (
            echo     Killing PID %%p...
            taskkill /PID %%p /F >nul 2>&1
        )
        timeout /t 1 /nobreak >nul
    )
)

echo [2/2] Starting Frontend (http://localhost:3000)...
start "EWS Web" cmd /k "pushd ""%ROOT%src\EWS.Web"" && npm run dev"

echo.
echo Waiting for services to start...
timeout /t 5 /nobreak >nul

:: ---- Summary Table (PowerShell) ----
:summary
echo.
powershell -NoProfile -Command ^
  "$api = netstat -ano | Select-String ':5271 .*LISTENING'; " ^
  "$apiPid = if ($api) { ($api -split '\s+')[-1] } else { $null }; " ^
  "$apiUrl = 'http://localhost:5271'; " ^
  "$apiStatus = if ($apiPid) { '[OK] Running (PID ' + $apiPid + ')' } else { '[--] Not Running' }; " ^
  "$web = $null; $webUrl = ''; " ^
  "foreach ($port in @(3000,3001,3002)) { " ^
  "  $r = netstat -ano | Select-String (':' + $port + ' .*LISTENING'); " ^
  "  if ($r) { $web = ($r -split '\s+')[-1]; $webUrl = 'http://localhost:' + $port; break } " ^
  "}; " ^
  "$webStatus = if ($web) { '[OK] Running (PID ' + $web + ')' } else { '[--] Not Running' }; " ^
  "$rows = @( " ^
  "  [pscustomobject]@{ Service='Backend API'; URL=$apiUrl; Status=$apiStatus }, " ^
  "  [pscustomobject]@{ Service='Frontend';    URL=$webUrl;  Status=$webStatus } " ^
  "); " ^
  "$c1=13; $c2=23; $c3=27; " ^
  "$h = '  +' + '-'*($c1+2) + '+' + '-'*($c2+2) + '+' + '-'*($c3+2) + '+'; " ^
  "$hdr = '  | ' + 'Service'.PadRight($c1) + ' | ' + 'URL'.PadRight($c2) + ' | ' + 'Status'.PadRight($c3) + ' |'; " ^
  "Write-Host $h; Write-Host $hdr; Write-Host $h; " ^
  "foreach ($r in $rows) { " ^
  "  Write-Host ('  | ' + $r.Service.PadRight($c1) + ' | ' + $r.URL.PadRight($c2) + ' | ' + $r.Status.PadRight($c3) + ' |') " ^
  "}; " ^
  "Write-Host $h"

echo.
pause
