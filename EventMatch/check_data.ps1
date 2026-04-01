# Check what data exists in EventMatch backend
param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com"
)

$InfoColor = 'Cyan'
$SuccessColor = 'Green'
$ErrorColor = 'Red'
$WarningColor = 'Yellow'

Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host "EventMatch Backend Data Status" -ForegroundColor $InfoColor
Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "API URL: $ApiUrl" -ForegroundColor $InfoColor
Write-Host ""

# Check health first
Write-Host "[1] Checking API health..." -ForegroundColor $InfoColor
try {
    $health = Invoke-WebRequest -Uri "$ApiUrl/api/health" -Method Get -UseBasicParsing -ErrorAction Stop
    if ($health.StatusCode -eq 200) {
        Write-Host "[OK] API is healthy" -ForegroundColor $SuccessColor
    }
}
catch {
    Write-Host "[FAILED] Cannot reach API" -ForegroundColor $ErrorColor
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host ""
Write-Host "[2] Fetching all users..." -ForegroundColor $InfoColor
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/users" -Method Get -UseBasicParsing -ErrorAction Stop
    $users = $response.Content | ConvertFrom-Json
    
    if ($users -and $users.Count -gt 0) {
        Write-Host "[FOUND] $($users.Count) users:" -ForegroundColor $SuccessColor
        $users | ForEach-Object {
            Write-Host "  - Email: $($_.email)" -ForegroundColor $SuccessColor
            Write-Host "    ID: $($_.id)" -ForegroundColor $SuccessColor
            Write-Host "    Created: $($_.created_at)" -ForegroundColor $SuccessColor
        }
    }
    else {
        Write-Host "[EMPTY] No users found" -ForegroundColor $WarningColor
    }
}
catch {
    Write-Host "[FAILED] Could not fetch users" -ForegroundColor $ErrorColor
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor $ErrorColor
}

Write-Host ""
Write-Host "[3] Fetching all groups..." -ForegroundColor $InfoColor
try {
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method Get -UseBasicParsing -ErrorAction Stop
    $groups = $response.Content | ConvertFrom-Json
    
    if ($groups -and $groups.Count -gt 0) {
        Write-Host "[FOUND] $($groups.Count) groups:" -ForegroundColor $SuccessColor
        $groups | ForEach-Object {
            Write-Host "  - Group: $($_.name)" -ForegroundColor $SuccessColor
            Write-Host "    ID: $($_.id), Owner: $($_.ownerEmail)" -ForegroundColor $SuccessColor
            Write-Host "    Members: $($_.memberCount), Created: $($_.createdAt)" -ForegroundColor $SuccessColor
        }
    }
    else {
        Write-Host "[EMPTY] No groups found" -ForegroundColor $WarningColor
    }
}
catch {
    Write-Host "[FAILED] Could not fetch groups" -ForegroundColor $ErrorColor
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor $ErrorColor
}

Write-Host ""
Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host "[DONE] Status check completed" -ForegroundColor $SuccessColor
Write-Host "======================================" -ForegroundColor $InfoColor
