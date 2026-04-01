# 🚀 Manual Redeploy to Render

## The Problem
The local `BACKEND_EXAMPLE.js` has been fixed (syntax error removed), but Render is still running the old version with the syntax error. That's why the `/api/admin/delete-all` endpoint returns 404.

## Solution: Manual Redeploy

### Option 1: Via Render Dashboard (RECOMMENDED - Fastest)

1. Go to: https://dashboard.render.com
2. Select your service: `eventmatch-api`
3. Click **"Manual Deploy"** button (top right)
4. Select **"Deploy latest commit"**
5. Wait for deployment to complete (~2-3 minutes)
6. Check the **Logs** tab to confirm successful startup

### Option 2: Via Git Push (Auto-Redeploy)

1. Commit the changes:
```powershell
cd C:\Users\PC\Documents\GitHub\Prog.-Indz
git add EventMatch/BACKEND_EXAMPLE.js
git commit -m "Fix: Remove syntax error in admin delete endpoint"
git push origin master
```

2. Render will automatically detect the push and redeploy
3. Check dashboard logs for deployment progress

## Verification

After deployment completes, test the endpoint:

```powershell
powershell -ExecutionPolicy Bypass -File "EventMatch\reset_complete.ps1" `
    -ApiUrl "https://eventmatch-api.onrender.com" `
    -AdminToken "jusu_secret" `
    -Force
```

**Expected Output:**
```
[OK] API is healthy ✓
[INFO] Found 0 groups before delete
[OK] DELETE ALL succeeded! ✓
[OK] Verified: 0 groups after delete
```

## If Still Failing

1. Check Render **Logs** tab for startup errors
2. Verify `ADMIN_TOKEN` environment variable is set: `jusu_secret`
3. Restart the service (blue "Restart" button in dashboard)
4. Check `/api/health` endpoint is responding

## Related Files
- `EventMatch/BACKEND_EXAMPLE.js` - Fixed backend code
- `EventMatch/reset_complete.ps1` - Reset verification script
