# ⚠️ RENDER.COM FIX - npm error ENOENT

## PROBLEMA:
```
npm error enoent no such file or directory, open '/opt/render/project/src/package.json'
```

Render ieškotas `package.json` neteisingoje vietoje!

---

## SPRENDIMAS:

Jūs turite dvi opcijas:

### OPTION 1: Pataisyti Start Command (REKOMENDUOJAMA)

1. Eik į Render Dashboard: https://dashboard.render.com
2. Pasirink savo service'ą: `eventmatch-api`
3. Spausk "Settings"
4. Rask "Build & Deploy" skiltį
5. Patikrinti / Pakeisti:

| Parametras | Turėtų Būti |
|-----------|------------|
| **Build Command** | `cd EventMatch && npm install` |
| **Start Command** | `cd EventMatch && node BACKEND_EXAMPLE.js` |
| **Root Directory** | (palikti tuščią arba `.`) |

6. Spausk "Save"
7. Render automatiškai relaunčins service'ą

---

### OPTION 2: Perkelti package.json į šakninį katalogą

Jei option 1 neveikia:

1. Perkelti `EventMatch/package.json` → `package.json` (šaknis)
2. Perkelti `EventMatch/BACKEND_EXAMPLE.js` → `BACKEND_EXAMPLE.js` (šaknis)
3. Atnaujinti Render:
   - Build Command: `npm install`
   - Start Command: `node BACKEND_EXAMPLE.js`

---

## ✅ PALAIDUOTI DEPLOYMENT

Po pakeitimų:

1. Spausk "Manual Deploy" → "Deploy latest commit"
2. Arba push'inti naujas changes:
```powershell
git add .
git commit -m "Fix Render deployment configuration"
git push origin master
```

Render automatiškai perdeploins su naujomis settings'ais!

---

## 🔍 PATIKRINTI STATUS

Eik į Render Dashboard → Logs ir patikrink:
- ✅ npm install baigiasi sėkmingai
- ✅ Server startina nurodytame porte
- ✅ Health check endpoint pasiekiamas

---

## JEI NEPAVYKO:

1. **Patikrinti Render Logs** - Dashboard → Logs tab
2. **Patikrinti konfigūraciją** - Dashboard → Settings → Build & Deploy
3. **Pabandyti manual redeploy** - "Manual Deploy" → "Deploy latest commit"
4. **Restart'inti service'ą** - Dashboard → "Restart" button

