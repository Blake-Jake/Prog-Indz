Reset scripts

- scripts/reset_all.ps1: PowerShell script that calls backend admin delete-all endpoint and deletes local MAUI DB files (`users.db3`).

Usage:
  pwsh -ExecutionPolicy Bypass -File .\scripts\reset_all.ps1

Options:
  -BackendUrl    Backend base url (default: https://eventmatch-api.onrender.com)
  -AdminToken    ADMIN_TOKEN to use (defaults to environment variable ADMIN_TOKEN)
  -AndroidPackage Android package name to clear via adb (optional)

Security:
  The script will prompt for confirmation before destructive actions. ADMIN_TOKEN is required.