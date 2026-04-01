# 🚀 EVENTMATCH BACKEND - RENDER.COM DEPLOYMENT (FIXED)

## ⚡ GREITA INSTALIACIJA (4 ŽINGSNIAI)

---

## ŽINGSNIS 1️⃣: Instaliuoti Node.js

1. Eik į: https://nodejs.org
2. Download "LTS" versiją
3. Instaliuok su default nustatymai
4. Restart'inti komputerį

**Patikrinti** (PowerShell):
```powershell
node --version
npm --version
```

---

## ŽINGSNIS 2️⃣: Setup Backend Localiai

**Atidaryti PowerShell EventMatch folder'yje**:
```powershell
cd C:\Users\PC\Documents\GitHub\Prog.-Indz\EventMatch
npm install
```

Tai instaliuos visus reikalingus paketus (express, cors, bcryptjs, sqlite3).

Lauk ~3-5 minutes... ☕

---

## ŽINGSNIS 3️⃣: Commit Į GitHub

```powershell
git add .
git commit -m "Add backend with Node.js packages"
git push origin master
```

---

## ŽINGSNIS 4️⃣: Deploy Į Render.com - TEISINGI NUSTATYMAI ⭐

### A) Registracija
1. Eik į https://render.com
2. Spausk "Sign up"
3. Pasirink "GitHub" (su savo GitHub account)
4. Autorizuok Render'ą

### B) Sukurti Web Service
1. Dashboard → "New +" → "Web Service"
2. Pasirink repository: `Blake-Jake/Prog-Indz`
3. Pasirink branch: `master`

### C) KRITINĖ KONFIGŪRACIJA ⚠️

**ŠIE NUSTATYMAI YRA SVARBŪS - SEKA TAIKYMO EILE:**

| Laukas | Vertė | Paaiškinimas |
|--------|-------|------------|
| **Name** | `eventmatch-api` | Service pavadinimas |
| **Region** | `Frankfurt (EU)` | Jums artimiausia |
| **Branch** | `master` | GitHub branch |
| **Root Directory** | `EventMatch` | ⭐ **NE tuščia!** |
| **Build Command** | `npm install` | Package'ų instaliacija |
| **Start Command** | `node BACKEND_EXAMPLE.js` | Paleisti app |
| **Plan** | `Free` | Nemokama |

### D) Environment Variable

1. Spausk "Advanced" (PRIEŠ spaudžiant "Create Web Service")
2. "Add Environment Variable"
   - **Key**: `ADMIN_TOKEN`
   - **Value**: `jusu_secret`
3. Spausk "Add"

### E) Deploy

1. Spausk "Create Web Service"
2. **LAUK** ~5-10 minučių
3. Kai matai žaliąją čekę ✓, sėkmingai!

---

## ✅ PATIKRINTI JEI VEIKIA

Po deployment'o, jūsų API turėtų būti pasiekiamas:

```
https://eventmatch-api.onrender.com
```

### Test su PowerShell:
```powershell
curl https://eventmatch-api.onrender.com/api/health
```

Turi grąžinti:
```json
{
  "status": "OK",
  "message": "Server is running",
  "endpoints": { ... }
}
```

### Test su mūsų skriptu:
```powershell
.\check_data.ps1 -ApiUrl "https://eventmatch-api.onrender.com"
```

---

## 🚨 JEI KLAIDA: "npm error enoent"

Jei matai klaidą `npm error path /opt/render/project/src/package.json`:

**Sprendimas**:
1. Dashboard → Service Settings
2. Ieš "Root Directory" ir įvesk: `EventMatch`
3. Spausk "Save"
4. "Manual Deploy" → "Deploy latest commit"

---

## 🔄 ATNAUJINTI API (Changes)

Kai rašai kodo pakeitimus:

```powershell
git add .
git commit -m "Update API endpoint"
git push origin master
```

**Render automatiškai**:
1. ✓ Mato push'ą
2. ✓ Atsigauna kodą
3. ✓ Rebeildina (`npm install`)
4. ✓ Relaunčina
5. ✓ API atnaujintas! 

---

## 🚨 PROBLEMOS IR SPRENDIMAS

### Problem 1: "npm: command not found"
**Sprendimas**: Instaliuok Node.js iš https://nodejs.org

### Problem 2: "npm error enoent"
**Sprendimas**: 
- Dashboard → Settings
- Root Directory: `EventMatch`
- Save ir Deploy

### Problem 3: Build failas Render'e
**Sprendimas**: Patikrinti Render logs:
- Dashboard → Select Service → Logs
- Ieš klaidos žinutę

### Problem 4: "ADMIN_TOKEN not set"
**Sprendimas**: Patikrinti environment variable'i:
- Dashboard → Service → Settings → Environment
- Patikrinti ar `ADMIN_TOKEN=jusu_secret` yra

### Problem 5: Deploy'inimas neveikia
**Sprendimas**:
- Restart'inti: Dashboard → "Manual Deploy" → "Deploy latest commit"
- Arba push'inti naujas changes: `git push origin master`

---

## 📌 MANO KONFIGŪRACIJA (EXAMPLE)

```
Repository: https://github.com/Blake-Jake/Prog-Indz
Branch: master
Root Directory: EventMatch ⭐ SVARBU!
Build: npm install
Start: node BACKEND_EXAMPLE.js
Env: ADMIN_TOKEN=jusu_secret
URL: https://eventmatch-api.onrender.com
```

---

## 🔗 SVARBIOS NUORODOS

- **Render Docs**: https://docs.render.com
- **Node.js**: https://nodejs.org
- **Your Backend**: https://eventmatch-api.onrender.com
- **Render Dashboard**: https://dashboard.render.com

---

## 📊 PROCESS SUMMARY

```
┌─────────────────────┐
│  Local Development  │ (npm install, test locally)
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Push to GitHub     │ (git push origin master)
└──────────┬──────────┘
           │
           ↓
┌─────────────────────────────────┐
│  Render Deploy                  │
│  - Build (npm install)          │
│  - Start (node BACKEND_*.js)    │
│  - Root Dir: EventMatch ⭐      │
│  - Health Check                 │
└──────────┬──────────────────────┘
           │
           ↓
┌─────────────────────┐
│  Live API! 🚀       │
│ eventmatch-api      │
│ .onrender.com       │
└─────────────────────┘
```

---

## ✨ COMPLETED!

🎉 Jūs turėtumėt:
- ✅ Live backend serveris
- ✅ Pasiekiamas iš visur internete
- ✅ Automatic updates
- ✅ Free SSL certificate
- ✅ Unlimited requests (Free plan)

**Sveikinu! Jūsų backend yra gyvas!** 🚀
