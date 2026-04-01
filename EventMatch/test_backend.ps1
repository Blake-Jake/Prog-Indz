# Simple test - patikrinti ar backend veikia ir kokia duomenys

param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "EventMatch - Data Verification" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health check
Write-Host "[1] Testing /api/health..." -ForegroundColor Cyan
try {
    $health = curl -s "$ApiUrl/api/health" -UseBasicParsing
    if ($health) {
        Write-Host "[OK] API responds to health check" -ForegroundColor Green
    }
}
catch {
    Write-Host "[ERROR] API not responding" -ForegroundColor Red
    exit 1
}

# Test 2: Test user creation
Write-Host ""
Write-Host "[2] Testing user creation..." -ForegroundColor Cyan
$testUser = @{
    email = "test_$(Get-Random)@test.com"
    password = "TestPassword123"
} | ConvertTo-Json

try {
    $response = curl -s -X POST "$ApiUrl/api/auth/register" `
        -ContentType "application/json" `
        -Body $testUser `
        -UseBasicParsing
    
    if ($response -like "*registered successfully*") {
        Write-Host "[OK] User registration works" -ForegroundColor Green
        Write-Host "Created: $($testUser | ConvertFrom-Json | Select -ExpandProperty email)" -ForegroundColor Green
    }
    else {
        Write-Host "[INFO] Response: $response" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "[ERROR] Failed to create user" -ForegroundColor Red
}

# Test 3: Test group creation
Write-Host ""
Write-Host "[3] Testing group creation..." -ForegroundColor Cyan
$testGroup = @{
    name = "Test Group $(Get-Random)"
    description = "Testing group creation"
    ownerEmail = "test@example.com"
} | ConvertTo-Json

try {
    $response = curl -s -X POST "$ApiUrl/api/groups/create" `
        -ContentType "application/json" `
        -Body $testGroup `
        -UseBasicParsing
    
    if ($response -like "*Created*" -or $response -like "*id*") {
        Write-Host "[OK] Group creation works" -ForegroundColor Green
        Write-Host "Response: $response" -ForegroundColor Green
    }
    else {
        Write-Host "[INFO] Response: $response" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "[ERROR] Failed to create group" -ForegroundColor Red
}

# Test 4: Check database status
Write-Host ""
Write-Host "[4] Database Status Check..." -ForegroundColor Cyan
Write-Host "Note: /api/users and /api/groups need to be added to backend" -ForegroundColor Yellow
Write-Host "These would show current data count from database" -ForegroundColor Yellow

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "[DONE] Testing completed" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
