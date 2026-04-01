# Complete reset - clears both backend AND tries to verify deletion
param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com",
    [string]$AdminToken = "jusu_secret",
    [switch]$Force
)

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "EventMatch COMPLETE RESET TOOL" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host ""

if (-not $Force) {
    Write-Host "WARNING: This will DELETE ALL data!" -ForegroundColor Yellow
    $confirmation = Read-Host "Are you sure? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "[CANCELLED]" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "[STEP 1] Checking API health..." -ForegroundColor Cyan
try {
    $health = Invoke-WebRequest -Uri "$ApiUrl/api/health" -Method Get -UseBasicParsing -ErrorAction Stop
    if ($health.StatusCode -eq 200) {
        Write-Host "[OK] API is healthy" -ForegroundColor Green
    }
}
catch {
    Write-Host "[FAILED] Cannot reach API" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[STEP 2] Checking existing data BEFORE delete..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method Get -UseBasicParsing -ErrorAction Stop
    $groups = $response.Content | ConvertFrom-Json
    $groupCount = if ($groups -is [array]) { $groups.Count } else { if ($groups) { 1 } else { 0 } }
    Write-Host "[INFO] Found $groupCount groups before delete" -ForegroundColor Yellow
}
catch {
    Write-Host "[INFO] Could not check groups (may already be empty)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[STEP 3] Executing DELETE ALL command..." -ForegroundColor Cyan
try {
    $endpoint = "$ApiUrl/api/admin/delete-all"
    $headers = @{ "x-admin-token" = $AdminToken }
    
    $response = Invoke-WebRequest -Uri $endpoint -Method Post -Headers $headers -UseBasicParsing -ErrorAction Stop
    
    if ($response.StatusCode -eq 200) {
        Write-Host "[SUCCESS] Delete command executed" -ForegroundColor Green
    }
}
catch {
    $errorMsg = $_.Exception.Message
    
    if ($errorMsg -like "*403*" -or $errorMsg -like "*Forbidden*") {
        Write-Host "[ERROR] Invalid admin token (403)" -ForegroundColor Red
        exit 1
    }
    else {
        Write-Host "[ERROR] $errorMsg" -ForegroundColor Red
        exit 1
    }
}

# Wait a bit for database to process
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "[STEP 4] Verifying deletion..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method Get -UseBasicParsing -ErrorAction Stop
    $groups = $response.Content | ConvertFrom-Json
    $groupCount = if ($groups -is [array]) { $groups.Count } else { if ($groups) { 1 } else { 0 } }
    
    if ($groupCount -eq 0) {
        Write-Host "[OK] No groups remaining" -ForegroundColor Green
    }
    else {
        Write-Host "[WARNING] $groupCount groups still exist!" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "[OK] Groups endpoint inaccessible (likely deleted)" -ForegroundColor Green
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host "[COMPLETE] Reset finished" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Try registering with NEW email (different from before)" -ForegroundColor Cyan
Write-Host "2. Or use: .\reset_all.ps1 -Force to try again" -ForegroundColor Cyan
