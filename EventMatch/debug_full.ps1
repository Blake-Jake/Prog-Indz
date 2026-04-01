# Complete debugging script - patikrinti LOCAL ir BACKEND duomenis
# Svarbu: Paleisti šį script'ą Android emuliatoriuje ir peržiūrėti debug output

param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com"
)

Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  EventMatch Debug & Data Verification  ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# STEP 1: Patikrinti backend
Write-Host "[STEP 1] Checking Backend Status..." -ForegroundColor Yellow
Write-Host "API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host ""

$backendOnline = $false
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/health" -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "[✓] Backend is ONLINE" -ForegroundColor Green
        $backendOnline = $true
    }
}
catch {
    Write-Host "[✗] Backend is OFFLINE or NOT RESPONDING" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "ŠIANDIEN: Backend neveikia! Reikia:" -ForegroundColor Yellow
    Write-Host "1. Eiti į https://dashboard.render.com" -ForegroundColor Yellow
    Write-Host "2. Pasirinkti 'eventmatch-api' service" -ForegroundColor Yellow
    Write-Host "3. Spausi 'Restart instance' arba 'Manual Deploy'" -ForegroundColor Yellow
    Write-Host "4. Patikrinti Logs tab'ą" -ForegroundColor Yellow
    Write-Host ""
}

# STEP 2: Jei backend veikia, patikrinti duomenis
if ($backendOnline) {
    Write-Host "[STEP 2] Checking Backend Data..." -ForegroundColor Yellow
    Write-Host ""
    
    # Try to get groups
    try {
        $response = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        $groups = $response.Content | ConvertFrom-Json
        $groupCount = if ($groups -is [array]) { $groups.Count } else { if ($groups) { 1 } else { 0 } }
        
        Write-Host "[✓] Groups found: $groupCount" -ForegroundColor Green
        if ($groupCount -gt 0) {
            Write-Host "    Groups in database:" -ForegroundColor Cyan
            $groups | ForEach-Object {
                Write-Host "    - $($_.name) (ID: $($_.id), Members: $($_.memberCount))" -ForegroundColor Cyan
            }
        }
        else {
            Write-Host "    [✓] Database is EMPTY - Good!" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "[✗] Could not fetch groups" -ForegroundColor Yellow
        Write-Host "    (Endpoint may not be implemented yet)" -ForegroundColor Gray
    }
    
    Write-Host ""
}

# STEP 3: LOCAL Database Check Instructions
Write-Host "[STEP 3] Checking Local Database (Android)..." -ForegroundColor Yellow
Write-Host ""
Write-Host "To check LOCAL data on your device:" -ForegroundColor Cyan
Write-Host "1. Run the Android app" -ForegroundColor Cyan
Write-Host "2. Open menu → Debug (Debug page)" -ForegroundColor Cyan
Write-Host "3. Click 'Refresh Data'" -ForegroundColor Cyan
Write-Host "4. You'll see local users, groups, messages" -ForegroundColor Cyan
Write-Host "5. Or click 'Print to Debug' to see in VS Output window" -ForegroundColor Cyan
Write-Host ""

# STEP 4: Summary
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            SUMMARY                     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if ($backendOnline) {
    Write-Host "[STATUS] Backend: ONLINE ✓" -ForegroundColor Green
    Write-Host "[STATUS] Local: Check via Debug page in app" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next: Run app and check Debug page for local data" -ForegroundColor Cyan
}
else {
    Write-Host "[STATUS] Backend: OFFLINE ✗" -ForegroundColor Red
    Write-Host ""
    Write-Host "ACTION NEEDED:" -ForegroundColor Yellow
    Write-Host "1. Go to: https://dashboard.render.com/services" -ForegroundColor Yellow
    Write-Host "2. Find: eventmatch-api" -ForegroundColor Yellow
    Write-Host "3. Click: Manual Deploy or Restart" -ForegroundColor Yellow
    Write-Host "4. Wait 2-3 minutes" -ForegroundColor Yellow
    Write-Host "5. Try again" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or check Render Logs for errors:" -ForegroundColor Yellow
    Write-Host "Dashboard → eventmatch-api → Logs tab" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═════════════════════════════════════════" -ForegroundColor Cyan
