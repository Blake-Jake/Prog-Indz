# 🧹 EventMatch Complete Reset Tool

## Apžvalga

`reset_complete.ps1` yra **kompletiškas reset tool**, kuris išvalo:
- ☁️ **Cloud data** (Render backend)
- 💾 **Local data** (Android/Windows app's SQLite database)

---

## 🚀 Naudojimas

### 1. **Full Reset** (Rekomenduojama)
```powershell
.\reset_complete.ps1 -Force
```
- Ištrini VISUS duomenis iš Render backend
- Ištrini lokalią SQLite duomenų bazę
- Nereikalingos patvirtinimo užklausos (`-Force` flag)

### 2. **Tik Cloud Reset**
```powershell
.\reset_complete.ps1 -CloudOnly -Force
```
- Išvalo tik Render backend
- Nepaliečia lokalios duomenų bazės

### 3. **Tik Local Reset**
```powershell
.\reset_complete.ps1 -LocalOnly -Force
```
- Ištrini tik lokalią SQLite duomenų bazę
- Nepaliečia Render backend

### 4. **Su Patvirtinimo Dialogo**
```powershell
.\reset_complete.ps1
```
- Be `-Force` flag, script paprašys patvirtinimo prieš kiekvieną ištrinimą

---

## 📊 Kas Atsitinka?

### STEP 1: Health Check
- ✅ Patikrina ar Render API veikia

### STEP 2: Pre-deletion Verification
- ✅ Suskaičiuoja grupes prieš ištrinimą

### STEP 3: Delete All Data
- ⚠️ **DESTRUCTIVE**: Ištrini VISUS users, groups, members, messages iš Render

### STEP 4: Post-deletion Verification
- ✅ Patikrina ar duomenys tikrai ištrinti (turi būti 0 grupos)

### STEP 5: Local Database Cleanup
- ✅ Ieško lokalios `users.db3` duomenų bazės keliais path'ais
- ⚠️ Jei randa - ištrini
- ℹ️ Jei neranda - praneša kuriose vietose buvo jos ieškota

---

## 🔍 Lokalinės DB Ieška

Script bando rasti lokalią duomenų bazę šiose vietose:
```
%APPDATA%\EventMatch\users.db3
%APPDATA%\EventMatch\users.db
%LOCALAPPDATA%\EventMatch\users.db3
%LOCALAPPDATA%\EventMatch\users.db
%USERPROFILE%\.eventmatch\users.db3
+ dar 5 papildomos vietos
```

---

## ✅ Sėkminga Reset Sesija

```
======================================
EventMatch COMPLETE RESET TOOL
======================================

[MODE] HYBRID: will clear BOTH cloud and local data
API URL: https://eventmatch-api.onrender.com
Local DB: Not found (will try to locate)

[STEP 1] Checking API health...
[OK] API is healthy ✓

[STEP 2] Checking existing data BEFORE delete...
[INFO] Found 0 groups before delete

[STEP 3] Executing DELETE ALL command...
[SUCCESS] Delete command executed ✓

[STEP 4] Verifying deletion...
[OK] No groups remaining ✓

[STEP 5] Handling local database...
[INFO] Local database not found (app may not have been run yet)

======================================
[COMPLETE] Reset finished ✓
======================================

Next steps:
1. Start fresh: Register with NEW email (different from before)
2. Or run again: .\reset_complete.ps1 -Force
```

---

## 🛠️ Konfigūracija

### Custom API URL
```powershell
.\reset_complete.ps1 -ApiUrl "https://your-backend.onrender.com" -Force
```

### Custom Admin Token
```powershell
.\reset_complete.ps1 -AdminToken "your_secret_token" -Force
```

---

## ❌ Troubleshooting

### "API health check failed"
- ❌ Render backend offline arba URL neteisingas
- ✅ Patikrink: https://eventmatch-api.onrender.com/api/health

### "Failed to delete data"
- ❌ ADMIN_TOKEN nesuderina arba nesustatyta Render
- ✅ Patikrink Render Dashboard → Settings → Environment Variables

### "Local database not found"
- ℹ️ Normalus scenarijus - app dar nebuvo paleista
- ✅ Arba database yra nežinomoje vietoje
- 🔧 Paleid app vieną kartą, tada vėl bandyk reset

---

## 🔐 Saugumo Pastabos

⚠️ **ŠITAS TOOL YRAIŠTRINI-VISKAS!**
- Nėra "undo" funkcijos
- Nėra recycle bin recovery
- Naudok tik test/development aplinkoje!

---

## 📚 Susijusios Komandos

```powershell
# Patikrinti duomenų statusą (be ištrinimo)
.\check_data.ps1

# Ištrinti tik grupę
.\reset_all.ps1

# Debug current data state
.\debug_full.ps1
```

---

## 📝 Pavyzdžiai

**Scenario 1: Pasiruošti naujam testui**
```powershell
.\reset_complete.ps1 -Force
# Dabar galima pradėti nuo scratch'o
```

**Scenario 2: Ištrinti tik remote data, local keep**
```powershell
.\reset_complete.ps1 -CloudOnly -Force
```

**Scenario 3: Interaktyvus mode (su patvirtinimais)**
```powershell
.\reset_complete.ps1
# Script paprašys "yes/no" patvirtinimo kiekvienam žingsniui
```

---

## 📞 Support

Jei reset neveikia:
1. Patikrink Render Logs: https://dashboard.render.com → Logs
2. Patikrink ADMIN_TOKEN yra nustatytas
3. Bandyk Cloud-only reset: `.\reset_complete.ps1 -CloudOnly -Force`
4. Jei vis neveikia - manual cleanup per Render dashboard
