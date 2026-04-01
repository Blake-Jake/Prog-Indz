# Render.com Deployment Guide - EventMatch Backend

## ŽINGSNIAI:

### 1️⃣ Pasiruošimas (LOCAL)

#### A) Sukurti package.json failą:
```bash
cd EventMatch
npm init -y
```

#### B) Įdiegti reikalingus paketus:
```bash
npm install express cors bcryptjs sqlite3
```

#### C) Redaguoti package.json:
```json
{
  "name": "eventmatch-api",
  "version": "1.0.0",
  "description": "EventMatch API Server",
  "main": "BACKEND_EXAMPLE.js",
  "scripts": {
    "start": "node BACKEND_EXAMPLE.js",
    "dev": "nodemon BACKEND_EXAMPLE.js"
  },
  "keywords": [],
  "author": "",
  "license": "ISC",
  "dependencies": {
    "express": "^4.18.2",
    "cors": "^2.8.5",
    "bcryptjs": "^2.4.3",
    "sqlite3": "^5.1.6"
  },
  "devDependencies": {
    "nodemon": "^3.0.1"
  }
}
```

#### D) Pridėti .gitignore:
```
node_modules/
*.db
.env
.env.local
```

#### E) Commit'inti į GitHub:
```bash
git add .
git commit -m "Add backend code and package.json"
git push origin master
```

---

### 2️⃣ Render.com Setup

#### Žingsniai:

1. **Atidaryti Render.com**: https://render.com

2. **Sign Up** (su GitHub account):
   - Spausk "Sign up with GitHub"
   - Autorizuok Render'ą

3. **Sukurti naują Web Service**:
   - Dashboard → "New +" → "Web Service"
   - Pasirink savo GitHub repository: `Prog-Indz`
   - Pasirink gal jei prašo branch: `master`

4. **Sukonfigūruoti:**
   - **Name**: `eventmatch-api` (arba kas nori)
   - **Runtime**: `Node`
   - **Build Command**: `npm install`
   - **Start Command**: `node EventMatch/BACKEND_EXAMPLE.js`
   - **Plan**: Pasirink "Free" (arba "Pro" jei nori)

5. **Environment Variables** - SVARBU!:
   - Spausk "Advanced"
   - "Add Environment Variable":
     - Key: `ADMIN_TOKEN`
     - Value: `jusu_secret`
   - Spausk "Add"

6. **Deploy**:
   - Spausk "Create Web Service"
   - Lauk ~5-10 minučių
   - Kai matai žaliąją čekę, backend yra gyvas! ✅

---

### 3️⃣ Patikrinti jei veikia

**Jei deploy'inimas baigtas**, jūsų API bus pasiekiamas:
```
https://eventmatch-api.onrender.com
```

Patikrinti:
```powershell
curl https://eventmatch-api.onrender.com/api/health
```

Arba paleisti mūsų skriptą:
```powershell
.\check_data.ps1 -ApiUrl "https://eventmatch-api.onrender.com"
```

---

### 4️⃣ Jei bus problemos

**A) Build nepavyksta:**
- Patikrinti Render logs: Dashboard → Service → Logs
- Patikrinti ar `BACKEND_EXAMPLE.js` yra teisingame kelyje
- Patikrinti ar `package.json` yra EventMatch folder'yje arba šakniniu

**B) API nepasiekiamas:**
- Patikrinti ar konfigūracija yra teisingi
- Restart'inti service'ą: Dashboard → "Manual Deploy" → "Deploy latest commit"

**C) ADMIN_TOKEN klaidai:**
- Patikrinti ar environment variable'i yra pridėtas
- Restartuoti deployment

---

### 5️⃣ Atnaujinti API (changes)

Kai rasiš kodo pakeitimus:

```bash
git add .
git commit -m "Update API endpoint"
git push origin master
```

Render automatiškai:
1. Mato push'ą
2. Atsigauna kodą
3. Rebeildina (npm install)
4. Relaunčina
5. API atnaujintas! ✅

---

## 📌 MANO KONFIGŪRACIJA (EXAMPLE):

```
Repository: https://github.com/Blake-Jake/Prog-Indz
Branch: master
Service Name: eventmatch-api
Build Command: npm install
Start Command: node EventMatch/BACKEND_EXAMPLE.js
Environment: ADMIN_TOKEN=jusu_secret
API URL: https://eventmatch-api.onrender.com
```

---

## 🔗 SVARBIOS NUORODOS:

- Render Documentation: https://docs.render.com
- Node.js Guide: https://docs.render.com/deploy-node
- Environment Variables: https://docs.render.com/environment-variables

---

## ⚡ SUMARYMAS:

| Žingsnis | Darbas |
|---------|--------|
| 1 | npm init, npm install |
| 2 | Commit į GitHub |
| 3 | Render.com New Web Service |
| 4 | Configure build/start commands |
| 5 | Add ADMIN_TOKEN env var |
| 6 | Deploy ir lauk |
| 7 | Test /api/health endpoint |
| 8 | Gatavas! 🎉 |
