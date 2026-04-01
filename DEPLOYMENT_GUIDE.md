# Quick Deployment Guide - EventMatch Cloud Sync

## Overview
This guide helps you deploy the EventMatch backend to enable login synchronization between Windows and Android emulators.

---

## Option A: Deploy to Render.com (FREE, Recommended)

### Step 1: Create GitHub Repository for Backend

```bash
# Create new folder for backend
mkdir eventmatch-backend
cd eventmatch-backend

# Copy these files:
# - BACKEND_EXAMPLE.js (rename to server.js)
# - BACKEND_PACKAGE.json (rename to package.json)

# Initialize git
git init
git add .
git commit -m "Initial backend commit"

# Create GitHub repo and push
git remote add origin https://github.com/YOUR_USERNAME/eventmatch-backend.git
git push -u origin main
```

### Step 2: Deploy to Render

1. **Sign Up**
   - Go to https://render.com
   - Sign up with GitHub account

2. **Create New Web Service**
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Choose "eventmatch-backend"

3. **Configure**
   - **Name**: eventmatch-api
   - **Environment**: Node
   - **Build Command**: `npm install`
   - **Start Command**: `npm start`
   - **Plan**: Free (or upgrade later)

4. **Deploy**
   - Click "Create Web Service"
   - Wait for deployment (2-3 minutes)
   - Copy your URL: `https://eventmatch-api-xxxxx.onrender.com`

5. **Verify Deployment**
   - Visit: `https://eventmatch-api-xxxxx.onrender.com/api/health`
   - Should see: `{"status":"OK","message":"Server is running"}`

---

## Option B: Deploy to Railway.app

### Step 1: Create Backend Repository (same as Option A)

### Step 2: Deploy to Railway

1. **Sign Up**
   - Go to https://railway.app
   - Sign up with GitHub

2. **Deploy**
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Choose your backend repository

3. **Configure**
   - Railway auto-detects Node.js
   - No additional config needed

4. **Get URL**
   - Go to "Settings"
   - Copy your deployment URL

---

## Option C: Local Testing (Same Network)

### For Development/Testing Only

```bash
# 1. Install Node.js from https://nodejs.org/

# 2. Create backend folder
mkdir eventmatch-backend
cd eventmatch-backend

# 3. Copy BACKEND_EXAMPLE.js and rename to server.js
# 4. Copy BACKEND_PACKAGE.json and rename to package.json

# 5. Install dependencies
npm install

# 6. Run server
npm start
# Output: "EventMatch API Server running on port 5000"

# 7. Find your computer IP
# Windows: ipconfig (look for IPv4 Address)
# Mac/Linux: ifconfig
# Example: 192.168.1.100
```

### Update CloudAuthService for Local Testing

In `EventMatch/Services/CloudAuthService.cs`:

```csharp
#if ANDROID
private const string API_BASE_URL = "http://10.0.2.2:5000"; // Android emulator localhost
#else
private const string API_BASE_URL = "http://YOUR_COMPUTER_IP:5000"; // e.g., http://192.168.1.100:5000
#endif
```

---

## Update CloudAuthService with Your URL

### In `EventMatch/Services/CloudAuthService.cs`

```csharp
namespace EventMatch.Services;

public class CloudAuthService
{
    private readonly HttpClient _httpClient;
    
#if ANDROID
    // Android emulator needs special localhost address
    private const string API_BASE_URL = "http://10.0.2.2:5000"; // For local development
#else
    // Windows/other platforms
    private const string API_BASE_URL = "https://eventmatch-api-xxxxx.onrender.com"; // Your deployed URL
#endif

    public CloudAuthService()
    {
        _httpClient = new HttpClient();
    }
    // ... rest of code
}
```

---

## Update LoginPage & SignUpPage to Use Cloud Auth

### Modify `LoginPage.xaml.cs`

```csharp
using EventMatch.Services;
using EventMatch.Models;

namespace EventMatch;

public partial class LoginPage : ContentPage
{
    private readonly HybridAuthService _authService; // Change from UserDatabase

    public LoginPage()
    {
        InitializeComponent();
        // Inject HybridAuthService
        _authService = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridAuthService>()!;
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        // Use cloud sync authentication
        var user = await _authService.LoginAsync(email, password);
        
        if (user != null)
        {
            await DisplayAlertAsync("Success", "Login successful!", "OK");
            Session.CurrentUserEmail = email;
            Preferences.Set("UserAlreadyLoggedIn", true);
            await Shell.Current.GoToAsync("//EventPreview");
        }
        else
        {
            await DisplayAlertAsync("Error", "Invalid email or password.", "OK");
        }
    }

    // ... rest of code
}
```

### Modify `SignUpPage.xaml.cs`

```csharp
using EventMatch.Services;
using EventMatch.Models;

namespace EventMatch;

public partial class SignUpPage : ContentPage
{
    private readonly HybridAuthService _authService; // Change from UserDatabase

    public SignUpPage()
    {
        InitializeComponent();
        _authService = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridAuthService>()!;
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        if (password != confirmPassword)
        {
            await DisplayAlertAsync("Error", "Passwords don't match.", "OK");
            return;
        }

        var newUser = new User { Email = email, Password = password };
        
        // Use cloud sync registration
        var success = await _authService.RegisterAsync(newUser);

        if (success)
        {
            await DisplayAlertAsync("Success", "Account created successfully!", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            await DisplayAlertAsync("Error", "Registration failed. Email may already exist.", "OK");
        }
    }

    // ... rest of code
}
```

---

## Testing Cloud Sync

### Test 1: Windows Emulator
1. Open app in Windows emulator
2. Go to Sign Up
3. Register: `testuser@example.com` / `password123`
4. Logout
5. Try login - should work ✅

### Test 2: Android Emulator
1. Open app in Android emulator
2. Go to Login (don't sign up!)
3. Login with: `testuser@example.com` / `password123`
4. Should work without creating new account ✅

**If Test 2 works, your sync is successful!**

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Android can't reach API | Check your computer IP is correct in CloudAuthService |
| "Invalid email or password" on Android | Backend might not have user from Windows registration |
| Local testing not working | Ensure `npm start` is running and backend shows "Server running" |
| Render deployment fails | Check `package.json` has all dependencies listed |

---

## URLs Reference

### Render.com Deployment
- Live API: `https://eventmatch-api-xxxxx.onrender.com`
- Health: `https://eventmatch-api-xxxxx.onrender.com/api/health`

### Local Testing
- Windows: `http://localhost:5000`
- Android from Windows host: `http://YOUR_COMPUTER_IP:5000`
- Android from Android emulator: `http://10.0.2.2:5000`

---

## Next Steps

1. ✅ Choose deployment option (Render/Railway/Local)
2. ✅ Deploy backend
3. ✅ Get your API URL
4. ✅ Update `CloudAuthService.API_BASE_URL`
5. ✅ Update `LoginPage.xaml.cs` and `SignUpPage.xaml.cs`
6. ✅ Rebuild both emulators
7. ✅ Test registration and login sync
8. ✅ Enjoy seamless cross-platform authentication!

---

## Production Checklist

- [ ] Add HTTPS enforcement
- [ ] Add rate limiting
- [ ] Add input validation
- [ ] Use environment variables for secrets
- [ ] Add logging/monitoring
- [ ] Add email verification
- [ ] Add password hashing (bcrypt is already used)
- [ ] Add JWT tokens for sessions
- [ ] Set up automatic backups
- [ ] Monitor free tier limits on Render/Railway
