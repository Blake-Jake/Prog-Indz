# Account Login Synchronization Guide - Windows & Android Emulator

## Overview

I've created a hybrid authentication system that synchronizes login credentials between Windows and Android emulators using cloud storage with local database fallback.

## Architecture

```
Windows Emulator  ┐
                  ├─→ Cloud Backend (Optional) ┐
Android Emulator ┘                             ├─→ Shared Authentication
                  ┌─ Local SQLite Database ───┘

- Primary: Cloud API (preferred)
- Fallback: Local SQLite (when cloud unavailable)
- Result: Credentials sync across both emulators
```

## New Services Created

### 1. **CloudAuthService** (`Services/CloudAuthService.cs`)
Handles communication with cloud backend:
- `RegisterUserAsync(user)` - Register new user
- `AuthenticateAsync(email, password)` - Login and verify
- `UserExistsAsync(email)` - Check if user exists

### 2. **HybridAuthService** (`Services/HybridAuthService.cs`)
Intelligent sync between cloud and local database:
- Tries cloud first, falls back to local if unavailable
- Automatically syncs users between cloud and local database
- Keeps both in sync transparently

## Setup Steps

### Option 1: Using Firebase Firestore (Recommended)

1. **Create Firebase Project**
   - Go to https://console.firebase.google.com
   - Create new project
   - Enable Firestore Database
   - Create collection named "users"

2. **Update CloudAuthService**
   ```csharp
   // Replace the API_BASE_URL with Firebase endpoints
   // Use Firebase Admin SDK or REST API
   ```

3. **Update MauiProgram.cs**
   ```csharp
   // Add Firebase package: Firebase.Database
   ```

### Option 2: Using Backend API (Node.js/Express Example)

1. **Create Node.js Backend**
   ```bash
   npm init -y
   npm install express cors dotenv jsonwebtoken bcryptjs sqlite3
   ```

2. **Create API Endpoints** (backend/server.js)
   ```javascript
   app.post('/api/auth/register', async (req, res) => {
       const { email, password } = req.body;
       // Hash password and save to database
       // Return success/failure
   });

   app.post('/api/auth/login', async (req, res) => {
       const { email, password } = req.body;
       // Verify credentials
       // Return user object if valid
   });

   app.get('/api/auth/exists/:email', async (req, res) => {
       // Check if user exists
   });
   ```

3. **Deploy Backend**
   - Use Render.com (free tier)
   - Use Railway.app
   - Use Heroku
   - Update API_BASE_URL in CloudAuthService

### Option 3: Quick Local Testing (Same Network)

For testing both emulators on the same machine:

1. **Create Local Web Service**
   - Use .NET backend
   - Host on `http://localhost:5000`
   - Update CloudAuthService:
   ```csharp
   private const string API_BASE_URL = "http://10.0.2.2:5000"; // Android localhost
   ```

## How It Works

### Login Flow:
```
1. User enters email/password
2. HybridAuthService.LoginAsync() called
3. If cloud available:
   - Try cloud authentication
   - Sync result to local database
4. If cloud unavailable:
   - Use local SQLite database
5. Return user object
```

### Registration Flow:
```
1. User enters email/password
2. HybridAuthService.RegisterAsync() called
3. If cloud available:
   - Register in cloud
   - Cache in local database
4. If cloud unavailable:
   - Save to local database only
5. Return success/failure
```

## Configuration

### Current State:
The services are already integrated in `MauiProgram.cs`:
```csharp
builder.Services.AddSingleton<CloudAuthService>();
builder.Services.AddSingleton<HybridAuthService>(sp =>
{
    var cloudAuth = sp.GetRequiredService<CloudAuthService>();
    var userDb = sp.GetRequiredService<UserDatabase>();
    return new HybridAuthService(cloudAuth, userDb);
});
```

### To Enable Cloud Sync:

1. **Update CloudAuthService.cs**
   ```csharp
   private const string API_BASE_URL = "YOUR_BACKEND_URL"; // Change this
   ```

2. **Update LoginPage.xaml.cs**
   ```csharp
   // Inject HybridAuthService instead of UserDatabase
   private readonly HybridAuthService _authService;
   
   public LoginPage()
   {
       InitializeComponent();
       _authService = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridAuthService>()!;
   }
   
   private async void OnSignInClicked(object? sender, EventArgs e)
   {
       var email = EmailEntry.Text?.Trim() ?? string.Empty;
       var password = PasswordEntry.Text ?? string.Empty;
       
       var user = await _authService.LoginAsync(email, password);
       // ... rest of code
   }
   ```

3. **Do Same for SignUpPage.xaml.cs**

## Testing

### Test Both Emulators:

1. **Windows Emulator**
   - Open in Visual Studio
   - Register new account (email: test@example.com)
   - Login with credentials

2. **Android Emulator**
   - Open in separate instance
   - Try logging in with same credentials (test@example.com)
   - **Should work without re-registering!**

## Database Sync Details

The system maintains these databases:

| Location | Database | Purpose |
|----------|----------|---------|
| Windows AppData | `users.db3` | Local cache |
| Android AppData | `users.db3` | Local cache |
| Cloud Backend | Firestore/API | Shared source |

When a user logs in on Windows:
1. Credentials sent to Cloud
2. Cloud returns user data
3. User data cached locally on Windows
4. User can then login on Android with same credentials

When a user logs in on Android:
1. Credentials sent to Cloud
2. Cloud returns user data
3. User data cached locally on Android
4. User can then login on Windows with same credentials

## Benefits

✅ **Seamless Sync** - No manual setup needed  
✅ **Offline Support** - Works even if cloud is down  
✅ **Automatic Cache** - Local database acts as cache  
✅ **Cross-Platform** - Any device with the app can access same account  
✅ **Secure** - Can add encryption/hashing at backend  

## Next Steps

1. Choose cloud solution (Firebase / Custom API / Local)
2. Update `CloudAuthService.API_BASE_URL` with your backend
3. Implement backend API endpoints
4. Update `LoginPage.xaml.cs` and `SignUpPage.xaml.cs` to use `HybridAuthService`
5. Test on both emulators
6. Deploy cloud backend
