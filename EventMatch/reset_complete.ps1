# Complete reset - clears both backend AND tries to verify deletion
param(
    [string]$ApiUrl = "https://eventmatch-api.onrender.com",
    [string]$AdminToken = "jusu_secret",
    [switch]$Force = $false,
    [switch]$LocalOnly = $false,
    [switch]$CloudOnly = $false,
    [switch]$AndroidEmulator = $false
)

# Colors
$InfoColor = "Cyan"
$SuccessColor = "Green"
$ErrorColor = "Red"
$WarningColor = "Yellow"

# Get local database path (same as MAUI app uses)
$AppDataPath = [Environment]::GetFolderPath("ApplicationData")
$LocalDbPath = Join-Path $AppDataPath "EventMatch" "users.db3"

# Android emulator package name
$AndroidPackageName = "com.companyname.eventmatch"

Write-Host "======================================"
Write-Host "EventMatch COMPLETE RESET TOOL" -ForegroundColor $InfoColor
Write-Host "======================================"
Write-Host ""

if ($LocalOnly) {
    Write-Host "[OPTION] LOCAL-ONLY mode: will only clear local database"
} elseif ($CloudOnly) {
    Write-Host "[OPTION] CLOUD-ONLY mode: will only clear Render backend"
} elseif ($AndroidEmulator) {
    Write-Host "[OPTION] ANDROID-EMULATOR mode: will only clear Android emulator data"
} else {
    Write-Host "[MODE] HYBRID: will clear BOTH cloud and local data"
}

Write-Host "API URL: $ApiUrl" -ForegroundColor $InfoColor
if (-not $CloudOnly -and -not $AndroidEmulator) {
    Write-Host "Local DB: $LocalDbPath" -ForegroundColor $InfoColor
}
Write-Host ""

# ============= ANDROID EMULATOR CLEAR =============
if ($AndroidEmulator) {
    Write-Host "[STEP 1] Checking ADB availability..."
    try {
        $adbVersion = adb version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] ADB is available" -ForegroundColor $SuccessColor
        } else {
            Write-Host "[ERROR] ADB not found" -ForegroundColor $ErrorColor
            exit 1
        }
    } catch {
        Write-Host "[ERROR] ADB not available: $_" -ForegroundColor $ErrorColor
        exit 1
    }

    Write-Host ""
    Write-Host "[STEP 2] Clearing Android emulator data..."
    
    if (-not $Force) {
        $confirmation = Read-Host "Clear emulator app data? (yes/no)"
        if ($confirmation -ne "yes") {
            Write-Host "[CANCELLED] Operation cancelled" -ForegroundColor $WarningColor
            exit 0
        }
    }

    try {
        $result = adb shell pm clear $AndroidPackageName 2>&1
        if ($result -match "Success") {
            Write-Host "[SUCCESS] Emulator data cleared" -ForegroundColor $SuccessColor
        } else {
            Write-Host "[ERROR] Failed to clear emulator data: $result" -ForegroundColor $ErrorColor
            exit 1
        }
    } catch {
        Write-Host "[ERROR] ADB command failed: $_" -ForegroundColor $ErrorColor
        exit 1
    }

    Write-Host ""
    Write-Host "======================================"
    Write-Host "[COMPLETE] Emulator reset finished" -ForegroundColor $SuccessColor
    Write-Host "======================================"
    exit 0
}

# ============= STEP 1: CLOUD RESET =============
if (-not $LocalOnly) {
    Write-Host "[STEP 1] Checking API health..."
    try {
        $healthResponse = Invoke-WebRequest -Uri "$ApiUrl/api/health" -Method GET -ErrorAction Stop
        if ($healthResponse.StatusCode -eq 200) {
            Write-Host "[OK] API is healthy" -ForegroundColor $SuccessColor
        }
    } catch {
        Write-Host "[ERROR] API health check failed: $_" -ForegroundColor $ErrorColor
        exit 1
    }

    Write-Host ""
    Write-Host "[STEP 2] Checking existing data BEFORE delete..."
    try {
        $groupsCheck = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method GET -ErrorAction Stop
        $groups = $groupsCheck.Content | ConvertFrom-Json
        $groupCount = if ($groups -is [array]) { $groups.Count } else { if ($groups) { 1 } else { 0 } }
        Write-Host "[INFO] Found $groupCount groups before delete" -ForegroundColor $InfoColor
    } catch {
        Write-Host "[WARNING] Could not fetch groups count" -ForegroundColor $WarningColor
    }

    Write-Host ""
    Write-Host "[STEP 3] Executing DELETE ALL command..." -ForegroundColor $WarningColor
    
    if (-not $Force) {
        $confirmation = Read-Host "Are you sure you want to DELETE ALL DATA from Render backend? (yes/no)"
        if ($confirmation -ne "yes") {
            Write-Host "[CANCELLED] Reset operation cancelled by user" -ForegroundColor $WarningColor
            exit 0
        }
    }

    try {
        $deleteResponse = Invoke-WebRequest `
            -Uri "$ApiUrl/api/admin/delete-all" `
            -Method POST `
            -Headers @{ 'x-admin-token' = $AdminToken } `
            -ContentType "application/json" `
            -Body "{}" `
            -ErrorAction Stop

        if ($deleteResponse.StatusCode -eq 200) {
            Write-Host "[SUCCESS] Delete command executed" -ForegroundColor $SuccessColor
        }
    } catch {
        Write-Host "[ERROR] The remote server returned an error: $_" -ForegroundColor $ErrorColor
        exit 1
    }

    Write-Host ""
    Write-Host "[STEP 4] Verifying deletion..."
    try {
        $groupsCheckAfter = Invoke-WebRequest -Uri "$ApiUrl/api/groups" -Method GET -ErrorAction Stop
        $groupsAfter = $groupsCheckAfter.Content | ConvertFrom-Json
        $groupCountAfter = if ($groupsAfter -is [array]) { $groupsAfter.Count } else { if ($groupsAfter) { 1 } else { 0 } }
        
        if ($groupCountAfter -eq 0) {
            Write-Host "[OK] No groups remaining" -ForegroundColor $SuccessColor
        } else {
            Write-Host "[WARNING] Found $groupCountAfter groups after delete (cleanup may have partial failure)" -ForegroundColor $WarningColor
        }
    } catch {
        Write-Host "[WARNING] Could not verify deletion" -ForegroundColor $WarningColor
    }

    Write-Host ""
}

# ============= STEP 5: LOCAL RESET =============
if (-not $CloudOnly) {
    Write-Host "[STEP 5] Handling local database..." -ForegroundColor $WarningColor
    
    # Try multiple local database path possibilities
    $possiblePaths = @(
        (Join-Path $env:APPDATA "EventMatch" "users.db3"),
        (Join-Path $env:APPDATA "EventMatch" "users.db"),
        (Join-Path $env:LocalAppData "EventMatch" "users.db3"),
        (Join-Path $env:LocalAppData "EventMatch" "users.db"),
        (Join-Path $env:UserProfile ".eventmatch" "users.db3"),
        (Join-Path $env:UserProfile "AppData" "Local" "EventMatch" "users.db3"),
        (Join-Path $env:UserProfile "AppData" "Roaming" "EventMatch" "users.db3"),
        (Join-Path $env:TEMP "EventMatch" "users.db3"),
        "C:\Users\$env:USERNAME\AppData\Local\EventMatch\users.db3",
        "C:\Users\$env:USERNAME\AppData\Roaming\EventMatch\users.db3"
    )

    $LocalDbPath = $null
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $LocalDbPath = $path
            break
        }
    }

    if ($LocalDbPath -and (Test-Path $LocalDbPath)) {
        Write-Host "[INFO] Found local database at: $LocalDbPath" -ForegroundColor $InfoColor
        
        if (-not $Force) {
            $confirmation = Read-Host "Delete local SQLite database? (yes/no)"
            if ($confirmation -ne "yes") {
                Write-Host "[SKIPPED] Local database kept intact" -ForegroundColor $WarningColor
            } else {
                try {
                    Remove-Item -Path $LocalDbPath -Force -ErrorAction Stop
                    Write-Host "[SUCCESS] Local database deleted" -ForegroundColor $SuccessColor
                } catch {
                    Write-Host "[ERROR] Failed to delete local database: $_" -ForegroundColor $ErrorColor
                    exit 1
                }
            }
        } else {
            # Force mode - delete without asking
            try {
                Remove-Item -Path $LocalDbPath -Force -ErrorAction Stop
                Write-Host "[SUCCESS] Local database deleted (forced)" -ForegroundColor $SuccessColor
            } catch {
                Write-Host "[ERROR] Failed to delete local database: $_" -ForegroundColor $ErrorColor
                exit 1
            }
        }
    } else {
        Write-Host "[INFO] Local database not found on Windows (expected if using Android emulator)" -ForegroundColor $InfoColor
    }

    Write-Host ""
}

# ============= COMPLETION =============
Write-Host "======================================"
Write-Host "[COMPLETE] Reset finished" -ForegroundColor $SuccessColor
Write-Host "======================================"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor $InfoColor
Write-Host "1. Start fresh: Register with NEW email (different from before)"
Write-Host "2. Android emulator: .\reset_complete.ps1 -AndroidEmulator -Force"
Write-Host "3. Or run again: .\reset_complete.ps1 -Force"
Write-Host ""
