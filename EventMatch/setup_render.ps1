# EventMatch Backend - Render.com Setup Script
# This script prepares your backend for deployment to Render.com

param(
    [switch]$SkipNpmInstall
)

$InfoColor = 'Cyan'
$SuccessColor = 'Green'
$WarningColor = 'Yellow'
$ErrorColor = 'Red'

Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host "EventMatch Backend Setup for Render" -ForegroundColor $InfoColor
Write-Host "======================================" -ForegroundColor $InfoColor
Write-Host ""

# Check if backend file exists
if (-not (Test-Path "BACKEND_EXAMPLE.js")) {
    Write-Host "[ERROR] BACKEND_EXAMPLE.js not found!" -ForegroundColor $ErrorColor
    Write-Host "Please run this script from the EventMatch directory" -ForegroundColor $ErrorColor
    exit 1
}
Write-Host "[1] Backend file found" -ForegroundColor $SuccessColor

# Create package.json
Write-Host "[2] Setting up package.json..." -ForegroundColor $InfoColor
$packageJson = @{
    name = "eventmatch-api"
    version = "1.0.0"
    description = "EventMatch API Server"
    main = "BACKEND_EXAMPLE.js"
    scripts = @{
        start = "node BACKEND_EXAMPLE.js"
        dev = "nodemon BACKEND_EXAMPLE.js"
    }
    dependencies = @{
        express = "^4.18.2"
        cors = "^2.8.5"
        bcryptjs = "^2.4.3"
        sqlite3 = "^5.1.6"
    }
    devDependencies = @{
        nodemon = "^3.0.1"
    }
} | ConvertTo-Json

if (-not (Test-Path "package.json")) {
    Set-Content -Path "package.json" -Value $packageJson
    Write-Host "[OK] package.json created" -ForegroundColor $SuccessColor
}
else {
    Write-Host "[OK] package.json already exists" -ForegroundColor $SuccessColor
}

# Create .gitignore
Write-Host "[3] Creating .gitignore..." -ForegroundColor $InfoColor
$gitignore = @"
node_modules/
*.db
.env
.env.local
.DS_Store
npm-debug.log
"@

if (-not (Test-Path ".gitignore")) {
    Set-Content -Path ".gitignore" -Value $gitignore
    Write-Host "[OK] .gitignore created" -ForegroundColor $SuccessColor
}
else {
    Write-Host "[OK] .gitignore already exists" -ForegroundColor $SuccessColor
}

# Check npm
Write-Host "[4] Checking npm..." -ForegroundColor $InfoColor
try {
    $npmVersion = npm --version 2>$null
    Write-Host "[OK] npm $npmVersion installed" -ForegroundColor $SuccessColor
}
catch {
    Write-Host "[ERROR] npm not found - please install Node.js" -ForegroundColor $ErrorColor
    exit 1
}

# Install dependencies
if (-not $SkipNpmInstall) {
    Write-Host "[5] Installing dependencies..." -ForegroundColor $InfoColor
    npm install
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Dependencies installed" -ForegroundColor $SuccessColor
    }
    else {
        Write-Host "[ERROR] npm install failed" -ForegroundColor $ErrorColor
        exit 1
    }
}
else {
    Write-Host "[5] Skipping npm install (use -SkipNpmInstall)" -ForegroundColor $WarningColor
}

# Create deployment guide
Write-Host "[6] Creating deployment guide..." -ForegroundColor $InfoColor
$deployGuide = @"
# Render.com Deployment Guide

## Automatic Setup Complete!

Your backend is ready for Render.com deployment.

## Next Steps:

1. **Commit to GitHub**:
   ``````bash
   cd EventMatch
   git add .
   git commit -m "Setup backend for Render deployment"
   git push origin master
   ``````

2. **Go to Render.com**:
   - Visit https://render.com
   - Sign up with GitHub (if not already)
   - Click "New +" → "Web Service"

3. **Configure Service**:
   - Select your GitHub repository (Prog-Indz)
   - Select branch: master
   - Name: eventmatch-api
   - Runtime: Node
   - Build Command: npm install
   - Start Command: node EventMatch/BACKEND_EXAMPLE.js

4. **Add Environment Variables**:
   - Click "Advanced"
   - Add Environment Variable:
     - Key: ADMIN_TOKEN
     - Value: jusu_secret

5. **Deploy**:
   - Click "Create Web Service"
   - Wait 5-10 minutes for deployment
   - Your API will be ready!

## Your API URL:
https://eventmatch-api.onrender.com

## Test Your API:
``````powershell
curl https://eventmatch-api.onrender.com/api/health
``````

## Automatic Updates:
After deployment, just push changes to GitHub and Render will automatically:
1. Detect the push
2. Re-install dependencies
3. Restart the service
4. Deploy new version

Enjoy your live API! 🚀
"@

Set-Content -Path "RENDER_DEPLOYMENT.md" -Value $deployGuide
Write-Host "[OK] Deployment guide created" -ForegroundColor $SuccessColor

# Summary
Write-Host ""
Write-Host "======================================" -ForegroundColor $SuccessColor
Write-Host "[COMPLETE] Setup finished!" -ForegroundColor $SuccessColor
Write-Host "======================================" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Next steps:" -ForegroundColor $InfoColor
Write-Host "1. Commit to GitHub:" -ForegroundColor $WarningColor
Write-Host "   git add ." -ForegroundColor $WarningColor
Write-Host "   git commit -m 'Setup for Render deployment'" -ForegroundColor $WarningColor
Write-Host "   git push origin master" -ForegroundColor $WarningColor
Write-Host ""
Write-Host "2. Go to https://render.com and create Web Service" -ForegroundColor $WarningColor
Write-Host ""
Write-Host "3. Configure:" -ForegroundColor $WarningColor
Write-Host "   - Build: npm install" -ForegroundColor $WarningColor
Write-Host "   - Start: node EventMatch/BACKEND_EXAMPLE.js" -ForegroundColor $WarningColor
Write-Host "   - Env: ADMIN_TOKEN=jusu_secret" -ForegroundColor $WarningColor
Write-Host ""
Write-Host "Your backend will be live at: https://eventmatch-api.onrender.com" -ForegroundColor $SuccessColor
