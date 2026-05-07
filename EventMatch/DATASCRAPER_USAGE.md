# 📖 DataScraper - Greitasis Pradžios Vadovas

## 🚀 Kaip Naudoti DataScraper Aplikacijoje

### **ŽINGSNIS 1: Atidarykite DataScraper Puslapį**

1. Paleiskite **EventMatch** aplikaciją
2. Pagrindiniame meniu spauskite **"Data Scraper"** iš kairės navigacijos
3. Atsidarysite DataScraper puslapis su 3 laukais

---

## 📝 **ŽINGSNIS 2: Užpildykite Formos Laukus**

### **a) Facebook Page ID**
- Kokia yra jūsų Facebook puslapių, kurio renginius norite importuoti, ID?
- **Pvz.:** `meta` (Meta puslapio ID būtų `20531967145`)

### **b) Access Token**
- Tai yra autentifikacijos raktažodis
- Jį gausite iš Facebook Developers

---

## 🔑 **ŽINGSNIS 3: Gaukite Access Token (Facebook)**

### **Greita Instrukcija:**

1. **Eikite į** https://developers.facebook.com/
2. **Prisijunkite** savo Facebook paskyra
3. **Sukurkite** naują App:
   - "Create App" → "Business" → Užpildykite pavadinimą
4. **Gaukite Token:**
   - Tools → **Graph API Explorer**
   - Pasirinkite savo App
   - Spauskite **"Generate Access Token"**
   - **Kopijuokite** token (pradžia: `EAA...`)

---

## 🎯 **ŽINGSNIS 4: Importuokite Renginius**

1. **Įveskite** Facebook Page ID
2. **Įveskite** Access Token
3. **Spauskite** "Scrape Facebook Events" 🔄
4. **Laukite** importavimo (2-5 sekundės)
5. **Spauskite** "✅ X renginių importuota!"

---

## 📊 **ŽINGSNIS 5: Peržiūrėkite Importuotus Renginius**

Importuoti renginiai automatiškai:
- ✅ Priimti į **EventStore** (saugykla)
- ✅ Pasirodyti **EventPreview** lange
- ✅ Turėti **"Facebook" žymą**
- ✅ Parodyti nuotraukas, vietas, datas

---

## 📱 **Kompletas Darbo Ciklas**

```
1. Aplikacija atsidaro
   ↓
2. Spauskite "Data Scraper" meniu
   ↓
3. Įveskite Facebook credentials
   ↓
4. Spauskite "Scrape Facebook Events"
   ↓
5. Renginiai importuojami automatiškai
   ↓
6. Grįžtate į "Events Preview"
   ↓
7. Peržiūrite Facebook renginius 🎉
```

---

## 🔍 **PROBLEMOS IR SPRENDIMAI**

### ❌ "Nerasta renginių arba neteisinga Access Token"

**Sprendimas:**
- ✅ Patikrinkite, ar token nėra pasibaigęs (galioja ~60 dienų)
- ✅ Patikrinkite, ar Page ID teisingas
- ✅ Patikrinkite, ar Facebook puslapyje tikrai yra renginių

### ❌ "Invalid access token"

**Sprendimas:**
- ✅ Regeneruokite naują token Graph API Explorer'e
- ✅ Patikrinkite, ar aplicijoje yra `events` ir `pages_read_engagement` permissions

### ❌ Programa trunka žemyn importavimo metu

**Sprendimas:**
- ✅ Tai normalūs - jei daug renginių ir vaizdų
- ✅ Laukite 5-10 sekundžių
- ✅ Jei vis nutrūksta, renovinkite įjungdami Debug Mode

---

## 💡 **PATARIMAI**

- 📌 Pagal nutylėjimą, importuoti renginiai **nuo šiandien ir ateityje**
- 📌 Praeiti renginiai **automatiškai ištrinami** iš EventStore
- 📌 Kiekvienas renginys gauna **"Facebook" žymą** automatiškai
- 📌 Galite iš jų kurti favoritus ir bendrintis draugams

---

## 📚 **Daugiau Informacijos**

- Facebook API docs: https://developers.facebook.com/docs/events/
- Mūsų dokumentacija: `FACEBOOK_SETUP.md`

**Smagaus naudojimo!** 🎉
