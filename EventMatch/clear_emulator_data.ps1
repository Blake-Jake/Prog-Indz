param(
    [switch]$Force = $false,
    [switch]$WipeAll = $false
)

# Colors
$InfoColor = "Cyan"
$SuccessColor = "Green"
$ErrorColor = "Red"
$WarningColor = "Yellow"

$PackageName = "com.companyname.eventmatch"

Write-Host "======================================"
Write-Host "Android Emulator Data Cleaner" -ForegroundColor $InfoColor
Write-Host "======================================"
Write-Host ""

# Check if ADB is available
Write-Host "[STEP 1] Checking ADB availability..."
try {
    $adbVersion = adb version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] ADB is available" -ForegroundColor $SuccessColor
    } else {
        Write-Host "[ERROR] ADB not found or not in PATH" -ForegroundColor $ErrorColor
        Write-Host "Please install Android SDK tools and add ADB to PATH" -ForegroundColor $WarningColor
        exit 1
    }
} catch {
    Write-Host "[ERROR] Failed to run ADB: $_" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host ""
Write-Host "[STEP 2] Checking connected devices..."
$devices = adb devices 2>&1 | Select-Object -Skip 1 | Where-Object { $_ -and $_ -notmatch "^List" }
if ($devices.Count -eq 0) {
    Write-Host "[ERROR] No Android devices/emulators found!" -ForegroundColor $ErrorColor
    Write-Host "Please start the Android emulator first" -ForegroundColor $WarningColor
    exit 1
}

Write-Host "[OK] Found device(s):" -ForegroundColor $SuccessColor
$devices | ForEach-Object { 
    $device = $_.Trim().Split()[0]
    Write-Host "  - $device" -ForegroundColor $InfoColor
}

Write-Host ""
Write-Host "[STEP 3] Clearing $PackageName data..." -ForegroundColor $WarningColor

if (-not $Force) {
    $confirmation = Read-Host "Clear app data? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "[CANCELLED] Operation cancelled" -ForegroundColor $WarningColor
        exit 0
    }
}

try {
    # Clear app data (includes database)
    Write-Host "  - Running: adb shell pm clear $PackageName"
    $result = adb shell pm clear $PackageName 2>&1
    
    if ($result -match "Success") {
        Write-Host "[SUCCESS] App data cleared" -ForegroundColor $SuccessColor
    } else {
        Write-Host "[ERROR] Failed to clear data: $result" -ForegroundColor $ErrorColor
        exit 1
    }
} catch {
    Write-Host "[ERROR] ADB command failed: $_" -ForegroundColor $ErrorColor
    exit 1
}

# Optional: Wipe entire emulator data
if ($WipeAll) {
    Write-Host ""
    Write-Host "[STEP 4] Wiping ALL emulator data..." -ForegroundColor $WarningColor
    
    if (-not $Force) {
        $confirmation = Read-Host "RESET ENTIRE EMULATOR? This cannot be undone (yes/no)"
        if ($confirmation -ne "yes") {
            Write-Host "[SKIPPED] Full wipe cancelled" -ForegroundColor $WarningColor
        } else {
            try {
                Write-Host "  - Running: adb emu kill"
                adb emu kill
                Write-Host "[OK] Emulator stopped - restart it from AVD Manager to factory reset" -ForegroundColor $SuccessColor
            } catch {
                Write-Host "[ERROR] Failed to stop emulator: $_" -ForegroundColor $ErrorColor
            }
        }
    }
}

Write-Host ""
Write-Host "======================================"
Write-Host "[COMPLETE] Emulator cleanup finished" -ForegroundColor $SuccessColor
Write-Host "======================================"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor $InfoColor
Write-Host "1. App data has been cleared"
Write-Host "2. Run the app again - it will start fresh"
Write-Host "3. Register with new user to test"
Write-Host ""
Write-Host "To fully reset emulator:" -ForegroundColor $InfoColor
Write-Host "  .\clear_emulator_data.ps1 -WipeAll -Force"
Write-Host ""
