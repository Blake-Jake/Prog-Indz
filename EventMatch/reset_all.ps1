# Reset all data from EventMatch backend
param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com",
    [string]$AdminToken = "jusu_secret",
    [switch]$Force
)

$ErrorColor = 'Red'
$SuccessColor = 'Green'
$WarningColor = 'Yellow'
$InfoColor = 'Cyan'

Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host "EventMatch Backend Reset Tool" -ForegroundColor $InfoColor
Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host ""

Write-Host "API URL: $ApiUrl" -ForegroundColor $InfoColor
Write-Host "Admin Token: $($AdminToken.Substring(0, [Math]::Min(4, $AdminToken.Length)))..." -ForegroundColor $InfoColor
Write-Host ""

Write-Host "WARNING: This will DELETE ALL data from the backend!" -ForegroundColor $WarningColor
Write-Host "   - All users will be deleted" -ForegroundColor $WarningColor
Write-Host "   - All groups will be deleted" -ForegroundColor $WarningColor
Write-Host "   - All messages will be deleted" -ForegroundColor $WarningColor
Write-Host ""

if (-not $Force) {
    $confirmation = Read-Host "Are you sure? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "[CANCELLED]" -ForegroundColor $WarningColor
        exit 0
    }
}

Write-Host "[INFO] Sending reset request..." -ForegroundColor $InfoColor
Write-Host ""

try {
    $endpoint = "$ApiUrl/api/admin/delete-all"
    $headers = @{ "x-admin-token" = $AdminToken }
    
    $response = Invoke-WebRequest -Uri $endpoint -Method Post -Headers $headers -ErrorAction Stop
    
    if ($response.StatusCode -eq 200) {
        Write-Host "[SUCCESS] All data deleted from backend!" -ForegroundColor $SuccessColor
        Write-Host "[DELETED] Users, groups, and messages have been removed" -ForegroundColor $SuccessColor
    }
}
catch {
    $errorMsg = $_.Exception.Message

    if ($errorMsg -like "*403*" -or $errorMsg -like "*Forbidden*") {
        Write-Host "[ERROR] Invalid admin token (403 Forbidden)" -ForegroundColor $ErrorColor
    }
    elseif ($errorMsg -like "*500*" -or $errorMsg -like "*Server*") {
        Write-Host "[ERROR] Server error (500)" -ForegroundColor $ErrorColor
    }
    elseif ($errorMsg -like "*Connection*" -or $errorMsg -like "*unreachable*") {
        Write-Host "[ERROR] Cannot reach API at $ApiUrl" -ForegroundColor $ErrorColor
    }
    else {
        Write-Host "[ERROR] $errorMsg" -ForegroundColor $ErrorColor
    }
    exit 1
}

Write-Host ""
Write-Host "[DONE]" -ForegroundColor $SuccessColor
exit 0
