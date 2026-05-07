# Facebook Events Scraper - Setup Instrukcijos

## Kaip gauti Facebook Access Token ir Page ID

### 1️⃣ Sukurkite Facebook Developers Aplikaciją

1. Eikite į https://developers.facebook.com/
2. Prisijunkite arba sukurkite Facebook paskyrą
3. Spauskite "Create App"
4. Pasirinkite "Business" tipą
5. Užpildykite aplikacijos pavadinimą (pvz., "EventMatch Scraper")

### 2️⃣ Gaukite Access Token

1. Aplikacijos dashboard'e spauskite "Tools" → "Graph API Explorer"
2. Pasirinkite savo aplikaciją dropdown'e
3. Spauskite "Generate Access Token"
4. Kopijuokite token (pradžia: "EAA...")
5. ⚠️ **Svarbu**: Tai yra Development token - apribota veikimo trukmė

### 3️⃣ Gaukite Facebook Page ID

**Variantas A: Jei turite savo Facebook puslapį**
1. Eikite į savo Facebook puslapį
2. Dešinysis klik → "Inspect" → "Inspect Element"
3. Ieškokite `data-pageid` arba puslapio ID URL'e

**Variantas B: Per Facebook Graph API**
1. Graph API Explorer'yje įveskite: `/me/accounts`
2. Spauskite "Submit"
3. Kopijuokite norint puslapio ID iš atsakymo

### 4️⃣ EventMatch aplikacijoje

1. Atidarykite "Data Scraper" puslapį
2. Įveskite:
   - **Facebook Page ID**: `123456789`
   - **Access Token**: `EAAxxxxxxxxxxxx...`
3. Spauskite "Scrape Facebook Events"
4. Renginiai automatiškai bus pridėti į jūsų programą! 🎉

---

## Klaidos Diagnostika

| Klaida | Sprendimas |
|--------|-----------|
| `Invalid access token` | Patikrinkite token termino baigtį ir permissions |
| `No events found` | Puslapio ID neteisingas arba puslapyje nėra renginių |
| `Insufficient permissions` | Aplikacijoje reikalingos permissions: `events`, `pages_read_engagement` |

---

## Tūrinys Kuris Bus Importuojamas

- ✅ Renginio pavadinimas ir aprašymas
- ✅ Laikas ir data
- ✅ Vieta
- ✅ Renginio vaizdas (pavadinimas)
- ✅ Automatinė "Facebook" žyma

## 🔐 Saugumas

- **Niekada nešalinskite** Access Token su kitais žmonėmis
- Token galioja ~60 dienų, turite ją regeneruoti
- Gamybai naudokite Server-Side Token, ne Client-Side

---

**Daugiau informacijos**: https://developers.facebook.com/docs/events/
