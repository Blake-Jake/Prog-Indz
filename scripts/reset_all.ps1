<#
Reset local MAUI DB files and call backend admin delete-all endpoint.

Usage examples:
  # Use ADMIN_TOKEN from env and default backend
  pwsh -ExecutionPolicy Bypass -File .\scripts\reset_all.ps1

  # Provide token and backend URL explicitly
  pwsh -ExecutionPolicy Bypass -File .\scripts\reset_all.ps1 -AdminToken "your_token" -BackendUrl "https://eventmatch-api.onrender.com"

  # Clear Android app data (provide package name)
  pwsh -ExecutionPolicy Bypass -File .\scripts\reset_all.ps1 -AndroidPackage "com.company.app"

This script is destructive. It WILL delete local users.db3 files it finds and will call the backend admin endpoint (/api/admin/delete-all).
You must provide a valid ADMIN_TOKEN via environment variable or the -AdminToken parameter.
#>
param(
    [string]$BackendUrl = "https://eventmatch-api.onrender.com",
    [string]$AdminToken = $env:ADMIN_TOKEN,
    [string]$AndroidPackage = '',
    [switch]$Force
)

function Prompt-Confirm {
    param([string]$Message)
    $resp = Read-Host "$Message Type 'YES' to continue"
    return $resp -eq 'YES'
}

Write-Host "Reset script starting..."
Write-Host "Backend URL: $BackendUrl"

if (-not $AdminToken -or $AdminToken.Trim() -eq '') {
    $AdminToken = Read-Host -Prompt 'Enter ADMIN_TOKEN (will not be stored)'
}

if (-not $AdminToken -or $AdminToken.Trim() -eq '') {
    Write-Error "ADMIN_TOKEN is required. Aborting."
    exit 1
}

if (-not $Force) {
    if (-not (Prompt-Confirm "This will DELETE backend data AND local DB files.")) {
        Write-Host "Aborted by user. No actions performed."
        exit 0
    }
}
else {
    Write-Host "Force flag provided - skipping confirmation prompt."
}

# Call backend admin endpoint
try {
    Write-Host "Calling backend: $BackendUrl/api/admin/delete-all"
    $headers = @{ 'x-admin-token' = $AdminToken }
    $resp = Invoke-RestMethod -Method Post -Uri "$BackendUrl/api/admin/delete-all" -Headers $headers -ErrorAction Stop
    Write-Host "Backend response:"; Write-Output $resp
}
catch {
    Write-Warning "Backend call failed: $($_.Exception.Message)"
}

# Delete local MAUI DB files (users.db3)
$searchRoots = @()
if ($env:LOCALAPPDATA) { $searchRoots += $env:LOCALAPPDATA }
if ($env:USERPROFILE) { $searchRoots += $env:USERPROFILE }
$deleted = @()
foreach ($root in $searchRoots) {
    Write-Host "Searching for users.db3 under: $root"
    try {
        $files = Get-ChildItem -Path $root -Filter users.db3 -Recurse -ErrorAction SilentlyContinue
        foreach ($f in $files) {
            try {
                Remove-Item -Path $f.FullName -Force -ErrorAction Stop
                Write-Host "Deleted local DB: $($f.FullName)"
                $deleted += $f.FullName
            }
            catch {
                Write-Warning "Failed to delete $($f.FullName): $($_.Exception.Message)"
            }
        }
    }
    catch {
        # $root: would be parsed as a variable with a scope modifier inside double quotes.
        # Use format operator to avoid parsing ambiguities.
        Write-Warning ("Error searching {0}: {1}" -f $root, $_.Exception.Message)
    }
}

if ($deleted.Count -eq 0) { Write-Host "No local users.db3 files found." }

# Optionally clear Android app data via adb
if ($AndroidPackage -ne '') {
    $adb = Get-Command adb -ErrorAction SilentlyContinue
    if ($null -eq $adb) {
        Write-Warning "adb not found in PATH. Skipping Android clearing."
    }
    else {
        try {
            Write-Host "Clearing Android app data for package: $AndroidPackage"
            & adb shell pm clear $AndroidPackage | Write-Host
            Write-Host "If the device/emulator is connected and package exists, data should be cleared." 
        }
        catch {
            Write-Warning "adb command failed: $($_.Exception.Message)"
        }
    }
}

# If no AndroidPackage provided, try to infer package from local appdata folders and attempt to clear via adb if available
elseif ($AndroidPackage -eq '') {
    try {
        $possible = @()
        if ($env:LOCALAPPDATA) {
            $dirs = Get-ChildItem -Path $env:LOCALAPPDATA -Directory -ErrorAction SilentlyContinue
            foreach ($d in $dirs) {
                if ($d.Name -like 'com.*') {
                    # check if this directory contains a MAUI Data folder with users.db3
                    $candidate = Join-Path $d.FullName 'Data\users.db3'
                    if (Test-Path $candidate) {
                        $possible += $d.Name
                    }
                }
            }
        }

        if ($possible.Count -gt 0) {
            $inferredPackage = $possible[0]
            Write-Host "Inferred Android package: $inferredPackage"
            # locate adb if not in PATH
            $adbCmd = Get-Command adb -ErrorAction SilentlyContinue
            if ($null -eq $adbCmd) {
                $sdkPaths = @(
                    "$env:ANDROID_SDK_ROOT\platform-tools\adb.exe",
                    "$env:ANDROID_HOME\platform-tools\adb.exe",
                    "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe",
                    "$env:ProgramFiles(x86)\Android\android-sdk\platform-tools\adb.exe",
                    "$env:ProgramFiles\Android\android-sdk\platform-tools\adb.exe"
                )
                foreach ($p in $sdkPaths) {
                    if ($p -and (Test-Path $p)) { $adbCmd = $p; break }
                }
            }

            if ($null -ne $adbCmd) {
                Write-Host "Using adb at: $adbCmd"
                try {
                    & $adbCmd shell pm clear $inferredPackage | Write-Host
                    Write-Host "Cleared Android app data for $inferredPackage"
                }
                catch {
                    Write-Warning "Failed to clear Android data via adb: $($_.Exception.Message)"
                }
            }
            else {
                Write-Warning "adb not found. Install Android platform-tools or add adb to PATH to clear emulator app data. Inferred package: $inferredPackage"
            }
        }
        else {
            Write-Host "No Android package inferred from local AppData; skipping Android clearing. Provide -AndroidPackage to force."
        }
    }
    catch {
        Write-Warning "Error while attempting to infer Android package: $($_.Exception.Message)"
    }
}

Write-Host "Reset complete. Backend called and local DB files removed where found."
Write-Host "If you still see users in the app, uninstall the app or clear its data in emulator/device."